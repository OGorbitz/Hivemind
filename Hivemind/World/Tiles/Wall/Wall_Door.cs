using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hivemind.World.Tile.Wall
{
    [Serializable]
    public class Wall_Door : BaseTile
    {
        //Static variables
        public const string UName = "Wall_Dirt";
        public const Layer ULayer = Layer.WALL;

        public override string Name => UName;
        public override float Resistance => GetResistance();
        public override Layer Layer => ULayer;
        public virtual Texture2D Icon => UIcon;

        //Assets
        public static Texture2D UIcon;
        private static int[] Tex;

        //Custom variables
        private static int[,] tilecheck;
        private int renderindex;
        private const int TimeToOpen = 500;
        private float PercentOpen;
        private readonly bool Rotation;
        private bool IsOpen;
        


        public Wall_Door(Point p) : base(p)
        {
            Rotation = false;
        }

        public Wall_Door(bool r, Point p) : base(p)
        {
            Rotation = r;
        }

        public Wall_Door(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Rotation = (bool) info.GetValue("Rotation", typeof(bool));
            IsOpen = (bool) info.GetValue("Open", typeof(bool));
        }

        public static void LoadAssets(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            var textures = new Texture2D[7];
            Tex = new int[textures.Length];
            for (var i = 0; i < 7; i++) textures[i] = contentManager.Load<Texture2D>("Tiles/Wall/Door/Door" + (i + 1));

            UIcon = textures[0];
            
            for (int i = 0; i < textures.Length; i++)
            {
                Tex[i] = TextureAtlas.AddTexture(textures[i], graphicsDevice);
            }
        }

        public float GetResistance()
        {
            if (IsOpen)
                return 2;
            else
                return -1;
        }
        
        public void SetState(bool s)
        {
            IsOpen = s;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsOpen)
                PercentOpen += (float) gameTime.ElapsedGameTime.TotalMilliseconds / TimeToOpen;
            else
                PercentOpen -= (float) gameTime.ElapsedGameTime.TotalMilliseconds / TimeToOpen;

            if (PercentOpen < 0)
                PercentOpen = 0f;
            if (PercentOpen > 1)
                PercentOpen = 1f;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Rotation", Rotation);
            info.AddValue("Open", IsOpen);
        }

        public void OnClick(GameTime gameTime)
        {
            SetState(!IsOpen);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color)
        {
            /*if (Rotation)
            {
                spriteBatch.Draw(TextureAtlas.Atlas,
                    new Vector2(Pos.X * TileManager.TileSize,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                    sourceRectangle: TextureAtlas.GetSourceRect(Tex[0]),
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y));
                
                var DoorMovement = TileManager.TileSize / 2 - 8;
                DoorMovement = (int) (DoorMovement * PercentOpen);
                
                var r = TextureAtlas.GetSourceRect(Tex[3]);
                spriteBatch.Draw(TextureAtlas.Atlas,
                    position: new Vector2(Pos.X * TileManager.TileSize,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                    sourceRectangle: new Rectangle(r.X, r.Y + DoorMovement, TileManager.TileSize,
                        TileManager.TileSize + TileManager.WallHeight - DoorMovement), 
                    rotation: 0f,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y) + 0.0001f);
                
                r = TextureAtlas.GetSourceRect(Tex[4]);
                spriteBatch.Draw(TextureAtlas.Atlas,
                    new Vector2(Pos.X * TileManager.TileSize,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight + DoorMovement),
                    sourceRectangle: new Rectangle(r.X, r.Y, TileManager.TileSize,
                        TileManager.TileSize + TileManager.WallHeight - DoorMovement),
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y) + 0.0002f);
                
                spriteBatch.Draw(TextureAtlas.Atlas,
                    new Vector2(Pos.X * TileManager.TileSize,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                    sourceRectangle: TextureAtlas.GetSourceRect(Tex[1]),
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y) + 0.0005f);
            }
            else
            {
                var DoorMovement = TileManager.TileSize / 2 - 9;
                DoorMovement = (int) (DoorMovement * PercentOpen);
                
                var r = TextureAtlas.GetSourceRect(Tex[5]);
                spriteBatch.Draw(TextureAtlas.Atlas,
                    new Vector2(Pos.X * TileManager.TileSize,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                    sourceRectangle: new Rectangle(r.X + DoorMovement, r.Y, TileManager.TileSize - DoorMovement,
                        TileManager.TileSize + TileManager.WallHeight),
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y) + 0.0001f);
                
                r = TextureAtlas.GetSourceRect(Tex[6]);
                spriteBatch.Draw(TextureAtlas.Atlas,
                    new Vector2(Pos.X * TileManager.TileSize + DoorMovement,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                    sourceRectangle: new Rectangle(r.X, r.Y, TileManager.TileSize - DoorMovement,
                        TileManager.TileSize + TileManager.WallHeight),
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y) + 0.0002f);
                
                spriteBatch.Draw(TextureAtlas.Atlas,
                    new Vector2(Pos.X * TileManager.TileSize,
                        Pos.Y * TileManager.TileSize - TileManager.WallHeight),
                    sourceRectangle: TextureAtlas.GetSourceRect(Tex[2]),
                    color: Color.White, layerDepth: Parent.GetLayerDepth((int) Pos.Y) + 0.0005f);
            }*/
        }
    }
}