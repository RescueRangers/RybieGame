using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// ReSharper disable InconsistentNaming

namespace Rybie.Sprites
{
    public class SpriteBase
    {
        public Texture2D Texture
        {
            get;
        }

        public float X
        {
            get;
            set;
        }

        public float Y
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            set;
        }

        public float dX
        {
            get;
            set;
        }

        public float dY
        {
            get;
            set;
        }

        public float dA
        {
            get;
            set;
        }

        public float Scale
        {
            get;
            set;
        }

        public bool Colided { get; set; }

        private const float _hitboxScale = 1f;

        public SpriteEffects SpriteEffects = SpriteEffects.None;

        public SpriteBase(GraphicsDevice graphicsDevice, string textureName, float scale)
        {
            Scale = scale;
            if (Texture == null)
            {
                using (var stram = TitleContainer.OpenStream(textureName))
                {
                    Texture = Texture2D.FromStream(graphicsDevice, stram);
                }
            }
        }

        public SpriteBase(Texture2D texture, float scale)
        {
            Scale = scale;
            Texture = texture;
        }

        public bool RectangleCollision(SpriteBase otherSprite)
        {
            if (!Colided)
            {
                if (X + Texture.Width * Scale * _hitboxScale / 2 < otherSprite.X - otherSprite.Texture.Width * otherSprite.Scale / 2) return false;
                if (Y + Texture.Height * Scale * _hitboxScale / 2 < otherSprite.Y - otherSprite.Texture.Height * otherSprite.Scale / 2) return false;
                if (X - Texture.Width * Scale * _hitboxScale / 2 > otherSprite.X + otherSprite.Texture.Width * otherSprite.Scale / 2) return false;
                if (Y - Texture.Height * Scale * _hitboxScale / 2 > otherSprite.Y + otherSprite.Texture.Height * otherSprite.Scale / 2) return false;
                Colided = true;
                otherSprite.Colided = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Update(float elapsedTime)
        {
            X += dX * elapsedTime;
            Y += dY * elapsedTime;
            Angle += dA * elapsedTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var spritePosition = new Vector2(X, Y);
            spriteBatch.Draw(Texture, spritePosition, null, Color.White, Angle, new Vector2(0, Texture.Height / 2), new Vector2(Scale), SpriteEffects, 0f );
        }
    }
}
