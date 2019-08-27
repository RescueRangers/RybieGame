using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Rybie.Sprites
{
    public class BackgroundBubble : SpriteBase
    {
        public BackgroundBubble(Texture2D texture, float scale) : base(texture, scale)
        {
        }

        public new void Update(float elapsedTime)
        {

            base.Update(elapsedTime);
        }
    }
}
