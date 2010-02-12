using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class Reticle : Sprite
    {
        const string RETICLE_ASSET_NAME = "reticle";

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, RETICLE_ASSET_NAME);
            Scale = 1.0f;
            is_projectile = false;
        }

        public void Update(GameTime theGameTime)
        {
            // 25 is half of the width or height of the reticle
            // Otherwise, the bullets go through the upper-left corner of the reticle, not the center
            Position.X = Mouse.GetState().X - 25;
            Position.Y = Mouse.GetState().Y - 25;
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            base.Draw(theSpriteBatch);
        }
    }
}
