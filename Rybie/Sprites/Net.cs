using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Rybie.Sprites
{
    class Net : SpriteBase
    {
        private bool _dying;
        private float _dyingTimer;
        private float _currentTimer;
        private float _animationTimer;
        private float _currentAnimationTimer;
        private bool _flipped;

        public bool Dead { get; set; }

        public Net(Texture2D texture, float scale) : base(texture, scale)
        {
            _dyingTimer = 0.5f;
            _currentTimer = 0f;
            _animationTimer = 0.2f;
        }

        public new void Update(float elapsedTime)
        {
            if (Colided)
            {
                //SpriteEffects = SpriteEffects.FlipVertically;
                dX = 0;
                dY = 0;
                Dead = true;
            }

            if (!_dying)
            {
                if (_currentAnimationTimer > _animationTimer)
                {
                    _flipped = !_flipped;
                    SpriteEffects = _flipped ? SpriteEffects.None : SpriteEffects.FlipVertically;
                    _currentAnimationTimer = 0f;
                }

                _currentAnimationTimer += elapsedTime;
            }

            base.Update(elapsedTime);
        }
    }
}
