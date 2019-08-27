using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rybie.Sprites
{
    public class Fish : SpriteBase
    {
        private float _modifier;
        private double _currentTimer;
        private float _duration;
        private bool _isGoingUp;
        private bool _dying;
        private float _dyingTimer;
        private SpriteFont _fishFont;
        private readonly string _equation;

        public bool IsCorrect { get; set; }
        public bool Dead { get; set; }

        public Fish(Texture2D texture, float scale, Random random, SpriteFont fishFont, string equation, bool isCorrect)
            : base(texture, scale)
        {
            _modifier = random.Next(8,13) / 10f;
            _currentTimer = 0d;
            _duration = 0.5f;
            _dyingTimer = 0.5f;
            Colided = false;
            _fishFont = fishFont;
            _equation = equation;
            IsCorrect = isCorrect;
        }

        public new void Update(float elapsedTime)
        {
            if (!Colided)
            {
                if (_currentTimer > _duration * _modifier)
                {
                    _currentTimer = 0;
                    _isGoingUp = !_isGoingUp;
                }

                _currentTimer += elapsedTime;

                dY = _isGoingUp ? -100f * _modifier : 100f * _modifier;
            }
            else if(!_dying)
            {
                //SpriteEffects = SpriteEffects.FlipVertically;
                dX = 0;
                dY = 0;
                _currentTimer = 0f;
                _dying = true;
            }

            if (_dying)
            {
                if (_currentTimer > _dyingTimer)
                {
                    Dead = true;
                }

                _currentTimer += elapsedTime;
            }

            base.Update(elapsedTime);
        }

        public new void Draw(SpriteBatch spriteBatch)
        {
            var fishPosition = new Vector2(X, Y);
            var equationSize = _fishFont.MeasureString(_equation);
            var equationPosition = new Vector2(X, Y + (Texture.Height * Scale) + 5);

            if (_dying)
            {
                var score = IsCorrect ? $"Dobrze +{100 + (int)(X/2)}" : "Zle -100";

                spriteBatch.DrawString(_fishFont, score, fishPosition, Color.White );
            }
            else
            {
                spriteBatch.Draw(Texture, fishPosition, null, Color.White, Angle, new Vector2(0, Texture.Height / 2), new Vector2(Scale), SpriteEffects, 0f );
                spriteBatch.DrawString(_fishFont, _equation, equationPosition, Color.White);

            }
        }

    }
}
