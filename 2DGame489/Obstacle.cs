using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace _2DGame489
{
    class Obstacle : Sprite
    {
        private int scrollSpeed;
        public Obstacle( string assetName, int scrollSpeed )
        {
            this.AssetName = assetName;
            this.scrollSpeed = scrollSpeed;
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, this.AssetName);
            this.Scale = 1.0f;
            this.is_projectile = false;
        }

        public void Update(GameTime theGameTime)
        {
            // 25 is half of the width or height of the reticle
            // Otherwise, the bullets go through the upper-left corner of the reticle, not the center
            //this.Position.X = Mouse.GetState().X - 25;
            //this.Position.Y = Mouse.GetState().Y - 25;
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            base.Draw(theSpriteBatch);
        }
    }
}
