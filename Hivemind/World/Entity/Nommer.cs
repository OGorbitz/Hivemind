using Hivemind.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    public enum NommerState { IDLE, MOVING, ATTACKING }

    [Serializable]
    public class Nommer : BaseEntity
    {
        public const string UType = "Nommer";
        public const int USpeed = 50;
        public static Texture2D UIcon;

        public override string Type => UType;

        public Vector2 Vel = Vector2.Zero, DesiredVel = Vector2.Zero;
        public NommerState State = NommerState.IDLE, NextState = NommerState.IDLE;
        public int CurrentPathNode;
        public Vector2 Target;

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
            var sprites = new Texture2D[12];
            for (var i = 0; i < sprites.Length; i++) sprites[i] = content.Load<Texture2D>("Entity/Alien/Nommer/" + (i + 1));
            EntityManager.sprites.Add(UType, sprites);

            UIcon = sprites[0];
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case NommerState.IDLE:
                    Controller.SetAnimation("IDLE");
                    if (NextAction == null || NextAction == TimeSpan.Zero)
                        NextAction = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, milliseconds: (int)(Helper.Random() * 2000 + 1000));
                    if (gameTime.TotalGameTime > NextAction)
                    {
                        Vector2 goal = Vector2.Zero;

                        Vector2 tpos = new Vector2((int)Math.Floor(Pos.X / TileManager.TileSize), (int)Math.Floor(Pos.Y / TileManager.TileSize));

                        List<TileEntity> returned = Parent.GetTileEntities(new Rectangle((int)tpos.X - 4, (int)tpos.Y - 4, 8, 8));

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
                                float smallestdistance = Math.Abs((returned[smallestindex].Pos - tpos).Length());

                                for (int x = returned.Count - 1; x >= 0; x--)
                                {
                                    Vector2 v = returned[x].Pos;
                                    float dist = Math.Abs((v - tpos).Length());
                                    if (dist < smallestdistance)
                                    {
                                        smallestdistance = dist;
                                        smallestindex = x;
                                    }
                                }

                                goal = returned[smallestindex].Pos;

                            }
                        }

                        Pathfind = new Pathfinder(new Vector2((int)Pos.X / TileManager.TileSize, (int)Pos.Y / TileManager.TileSize), goal, 1000);

                        State = NommerState.MOVING;
                        NextState = NommerState.ATTACKING;
                        NextAction = TimeSpan.Zero;
                        Target = goal;
                    }

                    break;
                case NommerState.ATTACKING:
                    Parent.RemoveTileEntity(Target);
                    State = NommerState.IDLE;
                    break;
                case NommerState.MOVING:
                    if (Pathfind.Finished)
                    {
                        //Check distance to target node
                        Vector2 dist = ((Pathfind.Path[CurrentPathNode].Pos + new Vector2(0.5f)) * TileManager.TileSize) - Pos;

                        //Calculate desired velocity
                        DesiredVel = dist;
                        DesiredVel.Normalize();
                        DesiredVel *= USpeed;

                        //Check if within a certain distance of next node
                        if (dist.Length() < USpeed / 2)
                        {
                            if (CurrentPathNode == 0)
                            {
                                DesiredVel = Vector2.Zero;
                                State = NextState;
                            }
                            if (CurrentPathNode > 0)
                            {
                                CurrentPathNode--;
                                dist = (Pathfind.Path[CurrentPathNode].Pos * TileManager.TileSize) - Pos;
                            }
                        }

                        //Shift velocity towards desired velocity
                        Vel = (Vel * 4 + DesiredVel) / 5;
                        Pos += Vel * (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
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

            if (Vel.Y > 0)
                Controller.SetAnimation("DOWN");
            if (Vel.Y < 0)
                Controller.SetAnimation("UP");
            if (Vel.Y == 0)
                Controller.SetAnimation("IDLE");

            base.Update(gameTime);
        }
    }
}
