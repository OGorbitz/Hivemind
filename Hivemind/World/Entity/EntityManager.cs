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
        public static Dictionary<string, Texture2D[]> sprites;
        private static int CurrentID = 0;

        public static void LoadAssets(ContentManager content)
        {
            Selected = content.Load<Texture2D>("selected");
            sprites = new Dictionary<string, Texture2D[]>();

            Bush1.LoadAssets(content);
            Rock1.LoadAssets(content);

            SpaseShip.LoadAssets(content);

            SmallDrone.LoadAssets(content);
            Nommer.LoadAssets(content);
            //Entity_BasicComputerClump.LoadAssets(content);
            //Entity_BasicComputer.LoadAssets(content);
            //Entity_PowerBreaker.LoadAssets(content);
            //Entity_MineralDeposit.LoadAssets(content);

            //Entity_Rat.LoadAssets(g);

            //BloodSplat.LoadAssets(g);
            //SparkSource.LoadAssets(g);
        }

        public static Texture2D GetSprite(string id, int index)
        {
            if (sprites.ContainsKey(id))
            {
                if (index < sprites[id].Length) return sprites[id][index];
                throw new Exception("Sprite index out of bounds for " + id + ": " + index);
            }

            throw new Exception("No such sprite sheet for type " + id);
        }

        public static int GetID()
        {
            CurrentID++;
            return CurrentID;
        }

        //Creates a new Entity instance of the type t
        public static BaseEntity CreateEntity(string t, Vector2 pos)
        {
            switch (t)
            {
                //case Entity_Rat.UType:
                    //return new Entity_Rat(p);
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
