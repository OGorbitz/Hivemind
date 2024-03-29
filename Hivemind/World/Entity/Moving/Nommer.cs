﻿using Hivemind.GUI;
using Hivemind.Utility;
using Hivemind.World.Particle;
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
    public enum NommerState { IDLE, MOVING, ATTACKING }

    [Serializable]
    public class Nommer : MovingEntity
    {
        new public const string UType = "Nommer";
        new public readonly Point USize = new Point(48, 48);
        new public readonly Point USpriteSize = new Point(48, 48);
        public override Point Size => USize;
        public override Point SpriteSize => USpriteSize;
        public override string Type => UType;
        new public static Texture2D USpriteSheet;
        public override Texture2D SpriteSheet => USpriteSheet;

        public const int USpeed = 50;
        public static Texture2D UIcon;
        public static Texture2D pixel;

        public Vector2 Vel = Vector2.Zero, DesiredVel = Vector2.Zero;
        public NommerState State = NommerState.IDLE, NextState = NommerState.IDLE;
        public int CurrentPathNode;
        public Point Target;

        public TimeSpan NextAction;

        public Pathfinder Pathfind;

        public Nommer(Vector2 pos) : base(pos)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 0, 1 }, 4, true);
            //Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6 }, 5, true);
            //Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11 }, 5, true);
            Controller.AddAnimation("DOWN", new[] { 4, 0, 5, 0 }, 6, true);
            Controller.AddAnimation("ATTACK_DOWN", new[] { 2, 2, 3, 1 }, 5, false);
            Controller.AddAnimation("UP", new[] { 10, 6, 11, 6 }, 6, true);
            Controller.AddAnimation("ATTACK_UP", new[] { 9, 9, 10, 7 }, 5, false);
            Controller.SetAnimation("IDLE");
        }

        public Nommer(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Controller.AddAnimation("IDLE", new[] { 0, 0, 1 }, 3, true);
            //Controller.AddAnimation("LEFT", new[] { 2, 3, 4, 5, 6 }, 5, true);
            //Controller.AddAnimation("RIGHT", new[] { 7, 8, 9, 10, 11 }, 5, true);
            Controller.AddAnimation("UP", new[] { 4, 0, 5, 0 }, 4, true);
            Controller.AddAnimation("DOWN", new[] { 10, 6, 11, 6 }, 4, true);
            Controller.SetAnimation("IDLE");
        }

        public static void LoadAssets(ContentManager content)
        {
            USpriteSheet = content.Load<Texture2D>("Entity/Alien/Nommer");
            UIcon = content.Load<Texture2D>("Entity/Alien/NommerIcon");
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

            switch (State)
            {
                case NommerState.IDLE:
                    action = "Idle";
                    break;
                case NommerState.MOVING:
                    action = "Moving";
                    break;
                case NommerState.ATTACKING:
                    action = "Attacking";
                    break;
            }

            info = new Label()
            {
                Text = "/c[White]Current Action: /c[#663300]" + action,
                Font = GuiController.AutobusSmaller,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            stack.AddChild<Label>(info);
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case NommerState.IDLE:
                    Controller.SetAnimation("IDLE");
                    if (NextAction == TimeSpan.MinValue)
                        NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 2000 + 1000));
                    if (gameTime.TotalGameTime > NextAction)
                    {
                        Point tpos = TileMap.GetTileCoords(Pos);

                        Point goal = tpos + new Point((int)(Helper.Random() * 10 - 5), (int)(Helper.Random() * 10 - 5));

                        List<TileEntity> returned = TileMap.GetTileEntities(new Rectangle((int)tpos.X - 4, (int)tpos.Y - 4, 8, 8));

                        if (returned.Count > 0)
                        {
                            for (int x = returned.Count - 1; x >= 0; x--)
                            {
                                if (returned[x].Type != Bush1.UType)
                                    returned.RemoveAt(x);
                            }


                            if (returned.Count > 0)
                            {
                                int smallestindex = returned.Count - 1;
                                float smallestdistance = Math.Abs((returned[smallestindex].Pos.ToPoint() - tpos).ToVector2().Length());

                                for (int x = returned.Count - 1; x >= 0; x--)
                                {
                                    Point v = returned[x].Pos.ToPoint();
                                    float dist = Math.Abs((v - tpos).ToVector2().Length());
                                    if (dist < smallestdistance)
                                    {
                                        smallestdistance = dist;
                                        smallestindex = x;
                                    }
                                }

                                goal = returned[smallestindex].Pos.ToPoint();
                            }
                        }

                        Pathfind = new Pathfinder(TileMap.GetTileCoords(Pos), goal, 1000);

                        State = NommerState.MOVING;
                        NextState = NommerState.ATTACKING;
                        NextAction = TimeSpan.Zero;
                        Target = goal;
                    }

                    break;
                case NommerState.ATTACKING:
                    if (!Controller.AnimationFinished)
                        break;

                    var e = TileMap.GetTileEntity(Target);
                    if(e != null)
                        e.Destroy();

                    TileMap.AddParticleSource(new SparkSource(Pos, 50));

                    State = NommerState.IDLE;
                    break;
                case NommerState.MOVING:
                    if (Pathfind.Finished)
                    {
                        //Check distance to target node
                        Vector2 dist = ((Pathfind.Path[CurrentPathNode].Pos.ToVector2() + new Vector2(0.5f)) * TileManager.TileSize) - Pos;

                        //Calculate desired velocity
                        DesiredVel = dist;
                        DesiredVel.Normalize();
                        DesiredVel *= USpeed;

                        //Check if within a certain distance of next node
                        if (dist.Length() < USpeed / 4)
                        {
                            if (CurrentPathNode == 0)
                            {
                                DesiredVel = Vector2.Zero;
                                State = NextState;
                                Controller.SetAnimation("ATTACK_DOWN");
                                break;
                            }
                            if (CurrentPathNode > 0)
                            {
                                CurrentPathNode--;
                                dist = (Pathfind.Path[CurrentPathNode].Pos.ToVector2() * TileManager.TileSize) - Pos;
                            }
                        }

                        //Shift velocity towards desired velocity
                        float v = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
                        v *= 4;
                        Vel = (Vel + DesiredVel * v) / (1 + v);
                        Pos += Vel * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);

                        if (Vel.Y > 0)
                            Controller.SetAnimation("DOWN");
                        if (Vel.Y < 0)
                            Controller.SetAnimation("UP");
                        if (Vel.Y == 0)
                            Controller.SetAnimation("IDLE");
                    }
                    else
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            Pathfind.Cycle();
                            if (Pathfind.Finished)
                            {
                                if (Pathfind.Solution)
                                {
                                    CurrentPathNode = Pathfind.Path.Count - 1;
                                    break;
                                }
                                else
                                {
                                    DesiredVel = Vector2.Zero;
                                    State = NextState;
                                }
                            }
                        }
                    }
                    break;
            }

            base.Update(gameTime);
        }

        public override void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            if(pixel == null)
            {
                pixel = new Texture2D(graphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
            }

            if(State == NommerState.MOVING && Pathfind.Finished && Pathfind.Solution)
            {
                for (int i = CurrentPathNode - 1; i >= 0; i--)
                {
                    Helper.DrawLine(spriteBatch, Pathfind.Path[i].Pos.ToVector2() * new Vector2(TileManager.TileSize) + new Vector2(TileManager.TileSize / 2), Pathfind.Path[i + 1].Pos.ToVector2() * new Vector2(TileManager.TileSize) + new Vector2(TileManager.TileSize / 2), Color.White);
                }
                Helper.DrawLine(spriteBatch, Pathfind.Path[CurrentPathNode].Pos.ToVector2() * new Vector2(TileManager.TileSize) + new Vector2(TileManager.TileSize / 2), Pos, Color.White);
            }

            base.DrawSelected(spriteBatch, graphicsDevice, gameTime);
        }
    }
}
