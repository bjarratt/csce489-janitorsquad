using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagement
{
    class HealthBar : Sprite
    {
        const string HEALTHBAR_ASSET_NAME = "HealthBar";
        Game game;
        Jeep Player1;
        private float health;

        public HealthBar(Game game1, Jeep Player)
        {
            Player1 = Player;
            game = game1;
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, HEALTHBAR_ASSET_NAME);
            this.Scale = 0.5f;
            this.is_projectile = false;
        }

        public void Update(GameTime theGameTime)
        {
            health = Player1.Jeep_health;
            base.Update(theGameTime, Vector2.Zero, Vector2.Zero);
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            theSpriteBatch.Draw(base.mSpriteTexture, new Rectangle(game.Window.ClientBounds.Width / 2 - base.mSpriteTexture.Width / 2,
                 0, base.mSpriteTexture.Width, 20), new Rectangle(0, 45, base.mSpriteTexture.Width, 44), Color.Gray);

            //Draw the current health level based on the current Health
            theSpriteBatch.Draw(base.mSpriteTexture, new Rectangle(game.Window.ClientBounds.Width / 2 - base.mSpriteTexture.Width / 2,
                 0, (int)(base.mSpriteTexture.Width * ((double)health / 100)), 20),
                 new Rectangle(0, 45, base.mSpriteTexture.Width, 20), Color.Red);

            //Draw the box around the health bar
            theSpriteBatch.Draw(base.mSpriteTexture, new Rectangle(game.Window.ClientBounds.Width / 2 - base.mSpriteTexture.Width / 2,
                  0, base.mSpriteTexture.Width, 20), new Rectangle(0, 0, base.mSpriteTexture.Width, 44), Color.White);
        }
    }
}
