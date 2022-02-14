using Hivemind.World.Entity.Tile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Entity
{
    class EntityManager
    {
        public static Texture2D Selected;
        private static int CurrentID = 0;

        public delegate BaseEntity EntityConstructorMethod(Vector2 position);
        public static Dictionary<string, EntityConstructorMethod> EntityConstructors = new Dictionary<string, EntityConstructorMethod>();

        public static void LoadAssets(ContentManager content)
        {
            Selected = content.Load<Texture2D>("selected");

            Bush1.LoadAssets(content);
            Rock1.LoadAssets(content);

            SpaseShip.LoadAssets(content);
            BasicGenerator.LoadAssets(content);
            GunTurret.LoadAssets(content);

            Worker.LoadAssets(content);
            Nommer.LoadAssets(content);
            //Entity_BasicComputerClump.LoadAssets(content);
            //Entity_BasicComputer.LoadAssets(content);
            //Entity_PowerBreaker.LoadAssets(content);
            //Entity_MineralDeposit.LoadAssets(content);

            //Entity_Rat.LoadAssets(g);

            //BloodSplat.LoadAssets(g);
            //SparkSource.LoadAssets(g);
        }

        public static int GetID()
        {
            CurrentID++;
            return CurrentID;
        }

        public static void AddConstructor(string t, EntityConstructorMethod c)
        {
            if (!EntityConstructors.ContainsKey(t))
                EntityConstructors.Add(t, c);
        }

        //Creates a new Entity instance of the type t
        public static BaseEntity CreateEntity(string t, Vector2 pos)
        {
            if (EntityConstructors.ContainsKey(t))
            {
                return EntityConstructors[t](pos);
            }

            return null;
        }

        /*public static TileEntity.TileEntity CreateTileEntity(string t, Vector2 p)
        {
            switch (t)
            {
                case Entity_BasicComputer.UType:
                    return new Entity_BasicComputer(p);
                case Entity_BasicComputerClump.UType:
                    return new Entity_BasicComputerClump(p);
                case Entity_PowerBreaker.UType:
                    return new Entity_PowerBreaker(p);
                case Entity_MineralDeposit.UType:
                    return new Entity_MineralDeposit(p);
            }

            return null;
        }*/
    }
}
