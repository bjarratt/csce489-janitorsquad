using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace DinoEscape
{
    enum ObType
    {
        Rock,
        Log,
        Crystal
    }
    class Obstacle : Sprite
    {
        //public int type;
        public ObType type;

        public Obstacle( string assetName )
        {
            this.AssetName = assetName;
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, this.AssetName);
            this.Scale = 1.0f;
            this.is_projectile = false;
        }

        public void Update(GameTime theGameTime)
        {

        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            base.Draw(theSpriteBatch);
        }
    }
}
