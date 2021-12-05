using Hivemind.GUI;
using Hivemind.Utility;
using Hivemind.World.Colony;
using Hivemind.World.Entity.Moving;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    public enum WorkerBehavior {IDLE, FINDING, GETTING, DELIVERING, BUILDING}


    [Serializable]
    public class Worker : MovingEntity, IControllable
    {
        public const string UType = "SmallDrone";
        public readonly Point USize = new Point(32);
        public readonly Point USpriteSize = new Point(64);
        public const int USpeed = 200;
        public const float USightDistance = 8;
        public static Texture2D USpriteSheet;
        public override Texture2D SpriteSheet => USpriteSheet;
        public static Texture2D UIcon;

        public override string Type => UType;
        public override Point Size => USize;
        public override Point SpriteSize => USpriteSize;
        public override float SightDistance => USightDistance;

        public Vector2 Vel = Vector2.Zero;


        public TimeSpan NextAction;

        public WorkerBehavior Thought = WorkerBehavior.IDLE;
        public BaseTask CurrentTask;
        private Pathfinder Pathfinder;
        private int CurrentPathNode;

        public DroppedMaterial TargetMaterial;
        public float HaulingAmount;
        public Material CarryType;
        public float CarryAmount;

        public Worker(Vector2 pos) : base(pos)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 1}, 3, true);
            Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6}, 5, true);
            Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11}, 5, true);
            Controller.AddAnimation("DOWN", new[] { 12, 13, 14 }, 4, true);
            Controller.AddAnimation("UP", new[] { 15, 16, 17 }, 4, true);
            Controller.SetAnimation("IDLE");
        }

        public Worker(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 1}, 3, true);
            Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6}, 5, true);
            Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11}, 5, true);
            Controller.AddAnimation("UP", new[] { 12, 13, 14 }, 4, true);
            Controller.AddAnimation("DOWN", new[] { 15, 16, 17 }, 4, true);
            Controller.SetAnimation("IDLE");
        }

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/Robot/SmallDrone");

            UIcon = content.Load<Texture2D>("Entity/Robot/SmallDroneIcon");
        }

        public override void AddInfo(Panel panel)
        {
            var stack = new VerticalStackPanel
            {
                Spacing = 10
            };
            panel.AddChild<VerticalStackPanel>(stack);

            var info = new Label()
            {
                Text = Type,
                Font = GuiController.AutobusMedium,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.AddChild<Label>(info);

            string action = null;

            switch (Thought)
            {
                case WorkerBehavior.IDLE:
                    action = "Idle";
                    break;
                case WorkerBehavior.FINDING:
                    action = "Finding";
                    break;
                case WorkerBehavior.GETTING:
                    action = "Getting";
                    break;
                case WorkerBehavior.DELIVERING:
                    action = "Delivering";
                    break;
                case WorkerBehavior.BUILDING:
                    action = "Building " + (int) (100 * CurrentTask.WorkDone / CurrentTask.WorkRequired) + "%";
                    break;
            }

            info = new Label()
            {
                Text = @"\c[White]Current Action: \c[#444444]" + action + "\n" +
                @"\c[White]Carrying: \c[#444444](" + CarryAmount + ")\n",
                Font = GuiController.AutobusSmaller,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            stack.AddChild<Label>(info);
        }

        public virtual void ChooseTask()
        {
            foreach (BaseTask t in TileMap.TaskManager.Tasks)
            {
                if (t.GetType() == typeof(BuildTask))
                {
                    CurrentTask = t;
                    Thought = WorkerBehavior.BUILDING;
                    break;
                }
                if (t.GetType() == typeof(HaulTask))
                {
                    CurrentTask = t;
                    Thought = WorkerBehavior.FINDING;
                    break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (Thought)
            {
                case WorkerBehavior.IDLE:
                    ChooseTask();
                    break;
                case WorkerBehavior.FINDING:
                    if (Pathfinder == null)
                    {
                        var task = ((HaulTask)CurrentTask);
                        
                        foreach(KeyValuePair<Material, float> p in task.TotalMaterials)
                        {
                            if (task.MaterialsQueued.ContainsKey(p.Key))
                            {
                                if(task.MaterialsQueued[p.Key] < task.TotalMaterials[p.Key])
                                {
                                    HaulingAmount = task.TotalMaterials[p.Key] - task.MaterialsQueued[p.Key];
                                    CarryType = p.Key;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                HaulingAmount = task.TotalMaterials[p.Key];
                                CarryType = p.Key;
                                break;
                            }
                        }

                        float distance = -1;
                        DroppedMaterial goal = null;
                        foreach (DroppedMaterial m in TileMap.GetTile(TileMap.GetTileCoords(Pos)).Room.Materials)
                        {
                            if (m.Type == CarryType.Name)
                            {
                                float d = (m.Pos - Pos).Length();
                                if (d < distance || distance == -1)
                                {
                                    distance = d;
                                    goal = m;
                                }
                            }
                        }
                        if (goal != null)
                        {
                            Pathfinder = new Pathfinder(TileMap.GetTileCoords(Pos), TileMap.GetTileCoords(goal.Pos), 5000);
                            CurrentPathNode = -1;
                            TargetMaterial = goal;
                            Thought = WorkerBehavior.GETTING;
                        }
                    }
                    break;
                case WorkerBehavior.GETTING:
                    if (CurrentPathNode == 0)
                    {
                        CarryType = TargetMaterial.MaterialType;
                        CarryAmount = TargetMaterial.TryTake(HaulingAmount);

                        Pathfinder = new Pathfinder(TileMap.GetTileCoords(Pos), ((HaulTask)CurrentTask).Target.GetPosition(), 5000);
                        CurrentPathNode = -1;
                        Thought = WorkerBehavior.DELIVERING;
                    }
                    break;
                case WorkerBehavior.DELIVERING:
                    if (CurrentPathNode == 0)
                    {
                        ((HaulTask)CurrentTask).Deposit(CarryType, CarryAmount);

                        //IF MATERIALS MET
                        Pathfinder = null;
                        Thought = WorkerBehavior.IDLE;
                    }
                    break;
                case WorkerBehavior.BUILDING:
                    CurrentTask.DoWork(1000f/1000 * gameTime.ElapsedGameTime.Milliseconds);
                    if (CurrentTask.Complete)
                        Thought = WorkerBehavior.IDLE;
                    break;
            }

            if(Pathfinder != null)
            {
                if (Pathfinder.Finished)
                {
                    if (Pathfinder.Solution && CurrentPathNode >= 0)
                    {
                        //Check distance to target node
                        Vector2 dist = ((Pathfinder.Path[CurrentPathNode].Pos.ToVector2() + new Vector2(0.5f)) * TileManager.TileSize) - Pos;

                        //Calculate desired velocity
                        Vector2 targetVel = dist;
                        targetVel.Normalize();
                        targetVel *= USpeed;

                        //Check if within a certain distance of next node
                        if (dist.Length() < USpeed / 4)
                        {
                            if (CurrentPathNode == 0)
                            {
                                targetVel = Vector2.Zero;
                                Vel = Vector2.Zero;
                            }
                            if (CurrentPathNode > 0)
                            {
                                CurrentPathNode--;
                                dist = (Pathfinder.Path[CurrentPathNode].Pos.ToVector2() * TileManager.TileSize) - Pos;
                            }
                        }

                        //Shift velocity towards desired velocity
                        float time = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
                        time *= 4;
                        Vel = (Vel + targetVel * time) / (1 + time);
                        Pos += Vel * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
                    }
                }
                else
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Pathfinder.Cycle();
                        if (Pathfinder.Finished)
                        {
                            if (Pathfinder.Solution)
                            {
                                CurrentPathNode = Pathfinder.Path.Count - 1;
                            }
                            else
                            {
                                Pathfinder = null;
                            }
                            break;
                        }
                    }
                }
            }


            Vector2 v = new Vector2(Vel.X, Vel.Y);
            v.Normalize();
            double angle = Math.Atan2(v.X, - v.Y);
            if (angle <= -(3f / 4f) * Math.PI || angle >= (3f / 4f) * Math.PI)
                Controller.SetAnimation("DOWN");
            if (angle >= (-3f / 4f) * Math.PI && angle <= (-1f / 4f) * Math.PI)
                Controller.SetAnimation("LEFT");
            if (angle >= (-1f / 4f) * Math.PI && angle <= (1f / 4f) * Math.PI)
                Controller.SetAnimation("UP");
            if (angle >= (1f / 4f) * Math.PI && angle <= (3f / 4f) * Math.PI)
                Controller.SetAnimation("RIGHT");
            if (Vel == Vector2.Zero)
                Controller.SetAnimation("IDLE");

            base.Update(gameTime);
        }

        public void ControllerMove(Vector2 vel)
        {
            
        }

        public override void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            if (Pathfinder != null && Pathfinder.Finished && Pathfinder.Solution && CurrentPathNode >= 0)
            {
                for (int i = CurrentPathNode - 1; i >= 0; i--)
                {
                    Helper.DrawLine(spriteBatch, Helper.pixel, Pathfinder.Path[i].Pos.ToVector2() * new Vector2(TileManager.TileSize) + new Vector2(TileManager.TileSize / 2), Pathfinder.Path[i + 1].Pos.ToVector2() * new Vector2(TileManager.TileSize) + new Vector2(TileManager.TileSize / 2), Color.White);
                }
                Helper.DrawLine(spriteBatch, Helper.pixel, Pathfinder.Path[CurrentPathNode].Pos.ToVector2() * new Vector2(TileManager.TileSize) + new Vector2(TileManager.TileSize / 2), Pos, Color.White);
            }

            base.DrawSelected(spriteBatch, graphicsDevice, gameTime);
        }
    }
}
