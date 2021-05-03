using Hivemind.GUI;
using Hivemind.Input;
using Hivemind.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hivemind.World.Entity
{
    [Serializable]
    public abstract class BaseEntity : ISerializable, ISelectable
    {
        //Entity type specific variables
        public const string UType = "BaseEntity";
        public const string UDescription = "Lazy Coder";
        public const float USightDistance = 0;
     
        public virtual string Type => UType;
        public virtual string Description => UDescription;
        public virtual float SightDistance => USightDistance;

        public Vector2 Pos;

        public TileMap TileMap;
        public SpriteController Controller;

        public bool Focused = false;



        public BaseEntity()
        {
            Controller = new SpriteController();
        }

        public BaseEntity(SerializationInfo info, StreamingContext context)
        {
            //Pos = ((V2S)info.GetValue("Pos", typeof(V2S))).ToVector2();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //info.AddValue("Pos", new V2S(Pos));
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateVision();
        }

        public virtual void Init()
        {

        }
        public virtual void Destroy()
        {
            if (Selection.Selected.Contains(this))
                Selection.Selected.Remove(this);
            GC.SuppressFinalize(this);
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        /// <summary>
        /// Draws the rectangle around this object if it is selected
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="gameTime"></param>
        public abstract void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime);

        public void Command(Vector2 position)
        {
            throw new NotImplementedException();
        }

        public virtual bool SetFocused(bool focused)
        {
            Focused = focused;
            return true;
        }

        public virtual void AddInfo(Panel panel)
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
                Text = Description,
                Font = GuiController.AutobusSmaller,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            stack.AddChild<Label>(info);
        }

        public abstract void UpdateVision();
    }

    [Serializable]
    public class MovingEntity : BaseEntity
    {
        public const string UType = "MovingEntity";
        public override string Type => UType;

        public readonly Point USize = new Point(TileManager.TileSize, TileManager.TileSize);
        public virtual Point Size => USize;
        public virtual Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)(Pos.X - Size.X / 2), (int)(Pos.Y - Size.Y / 2), Size.X, Size.Y);
            }
        }

        public HashCell<MovingEntity> Cell;
        public int ID;

        public MovingEntity(Vector2 pos)
        {
            Pos = pos;
            ID = EntityManager.GetID();
        }

        public MovingEntity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ID = info.GetInt32("ID");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", ID);
        }

        public override void Update(GameTime gameTime)
        {
            if (Cell != null)
                if (!(Pos.X >= Cell.Position.X && Pos.X <= Cell.Position.X + Cell.Size.X &&
                        Pos.Y >= Cell.Position.Y && Pos.Y <= Cell.Position.Y + Cell.Size.Y))
                {
                    Cell.RemoveMember(this);
                    Cell = null;
                }
            if (Cell == null)
                if (Pos.X >= 0 && Pos.X < TileMap.Size * TileManager.TileSize && Pos.Y >= 0 && Pos.Y < TileMap.Size * TileManager.TileSize)
                    TileMap.AddEntity(this);

            if (Focused)
            {
                Vector2 FPos = Pos;
                FPos.Floor();
                TileMap.Cam.MoveTo(FPos);
                Focused = false;
            }

            base.Update(gameTime);
        }

        public virtual void Destroy()
        {
            TileMap.RemoveEntity(this);

            base.Destroy();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var frame = Controller.GetFrame(gameTime);
            var sprite = EntityManager.GetSprite(Type, frame);
            spriteBatch.Draw(sprite, new Rectangle((int)(Pos.X - sprite.Width / 2), (int)(Pos.Y - sprite.Height / 2), sprite.Width, sprite.Height),
                new Rectangle(0, 0, sprite.Width, sprite.Height),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: TileMap.GetLayerDepth((int)Pos.Y / TileManager.TileSize) + 0.0005f);
        }

        public override void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            spriteBatch.Draw(EntityManager.Selected, Bounds, Color.White);
        }

        public override void UpdateVision()
        {
            if (SightDistance > 0)
            {
                Fog.RevealCircle(Pos / TileManager.TileSize, SightDistance, TileMap);
            }
        }

    }

    [Serializable]
    public class TileEntity : BaseEntity
    {
        public readonly Point USize = new Point(1, 1);
        public virtual Point Size => USize;

        public bool Updated = false, Rendered = false;

        public virtual Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)Pos.X, (int)Pos.Y, Size.X, Size.Y);
            }
        }

        public TileEntity(Point p)
        {
            Pos = p.ToVector2();
        }

        public TileEntity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public new virtual void Update(GameTime gameTime)
        {
            if (Focused)
            {
                Vector2 FPos = (Pos + new Vector2(0.5f)) * TileManager.TileSize;
                FPos.Floor();
                TileMap.Cam.MoveTo(FPos);
                Focused = false;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var frame = Controller.GetFrame(gameTime);
            var sprite = EntityManager.GetSprite(Type, frame);
            spriteBatch.Draw(sprite, new Rectangle((int)(Pos.X * TileManager.TileSize), (int)(Pos.Y * TileManager.TileSize), TileManager.TileSize * Size.X, TileManager.TileSize * Size.Y),
                new Rectangle(0, 0, TileManager.TileSize * Size.Y, TileManager.TileSize * Size.Y),
                Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                layerDepth: TileMap.GetLayerDepth((int)Pos.Y) + 0.0005f);
        }

        public override void Destroy()
        {
            Rectangle r = Bounds;
            for (int x = r.X; x < r.Right; x++)
                for (int y = r.Y; y < r.Bottom; y++)
                    TileMap.RemoveTileEntity(new Point(x, y));

            TileMap.TileEntities.Remove(Pos.ToPoint());

            base.Destroy();
        }

        public override void DrawSelected(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            Rectangle r = new Rectangle((int)(Pos.X * TileManager.TileSize), (int)(Pos.Y * TileManager.TileSize), Size.X * TileManager.TileSize, Size.Y * TileManager.TileSize);
            spriteBatch.Draw(EntityManager.Selected, r, Color.White);
        }

        public override void UpdateVision()
        {
            if (SightDistance > 0)
            {
                Fog.RevealCircle(Pos, SightDistance, TileMap);
            }
        }
    }
}
