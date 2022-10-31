using Hivemind.GUI;
using Hivemind.Utility;
using Hivemind.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity.Moving
{
    public class Material
    {
        public Texture2D Texture;
        public string Name { get; private set; }

        public Material(string name, string texture)
        {
            Name = name;
            Texture = Hivemind.CManager.Load<Texture2D>("Items/" + texture);
        }

        public static readonly Material CrushedRock = new Material("Crushed Rock", "CrushedRock");
        public static readonly Material IronOre = new Material("Iron Ore", "IronOre");
    }

    public class DroppedMaterial : MovingEntity
    {
        new public readonly Point USize = new Point(32, 32);
        public override Point Size => USize;
        public override string Type => MaterialType.Name;

        private Room _room;
        public Room Room
        {
            get
            {
                return _room;
            }
            set
            {
                if (_room != null && _room.Materials != null)
                    _room.Materials.Remove(this);

                _room = value;

                if(_room != null && _room.Materials != null && !_room.Materials.Contains(this))
                    _room.Materials.Add(this);
            }
        }


        public Material MaterialType;
        public float Amount;
        
        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)(Pos.X - Size.X / 2), (int)(Pos.Y - Size.Y / 2), Size.X, Size.Y);
            }
        }

        public DroppedMaterial(Point pos, Material material, float amount) : base((pos.ToVector2() + new Vector2(0.25f + 0.5f * Helper.Random(), 0.25f + 0.5f * Helper.Random())) * TileManager.TileSize)
        {
            MaterialType = material;
            Amount = amount;
        }

        public override void Init()
        {
            base.Init();

            Room r = TileMap.GetTile(TileMap.GetTileCoords(Pos)).Room;
            if(r != null)
            {
                if(!r.Materials.Contains(this))
                    r.Materials.Add(this);
                this.Room = r;
            }
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

            info = new Label()
            {
                Text = "/c[White]Amount: /c[#444444](" + Amount + ")\n" +
                "/c[White]Room: /c[#444444](" + Room.Size + ")\n",
                Font = GuiController.AutobusSmaller,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            stack.AddChild<Label>(info);
        }

        public DroppedMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(MaterialType.Texture, new Rectangle((int)(Pos.X - MaterialType.Texture.Width / 2), (int)(Pos.Y - MaterialType.Texture.Height / 2), MaterialType.Texture.Width, MaterialType.Texture.Height),
                new Rectangle(0, 0, MaterialType.Texture.Width, MaterialType.Texture.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: TileMap.GetLayerDepth((int)Pos.Y / TileManager.TileSize) + 0.0005f);
        }

        public override void Destroy()
        {
            this.Room.Materials.Remove(this);
            base.Destroy();
        }

        public float TryTake(float amount)
        {
            if (Amount > amount)
            {
                Amount -= amount;
                return amount;
            }
            Destroy();
            return Amount;
        }
    }
}
