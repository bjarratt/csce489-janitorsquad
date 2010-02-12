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
    class Turret : Sprite
    {
        const string TURRET1_ASSET_NAME = "turret1";
        const string TURRET2_ASSET_NAME = "turret2";
        const int TURRET_SPEED = 600;   //same as jeep speed

        Vector2 speed = Vector2.Zero;
        Vector2 direction = Vector2.Zero;
          
        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, TURRET1_ASSET_NAME);
            //base.LoadContent(theContentManager, TURRET2_ASSET_NAME);
            Scale = 1.0f;
            is_projectile = false;   //turret is not a projectile
        }

        public void Update(GameTime theGameTime)
        {
            //base.Update(theGameTime, speed, direction);
            //Position += direction * speed * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }

        public void MoveTurret(Vector2 jeepPos, Vector2 speed, Vector2 direction, 
            GameTime theGameTime, GamePadState gamepad_state, KeyboardState key_state, MouseState mouse_state)
        {
            if (gamepad_state.ThumbSticks.Right != Vector2.Zero)
                RotateTurret(jeepPos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            else
            {
                RotateTurret(jeepPos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            }
            base.Update(theGameTime, speed, direction);
        }

        public void RotateTurret(Vector2 pos, Vector2 speed, Vector2 direction, 
            GameTime theGameTime, GamePadState gamepad_state, KeyboardState key_state, MouseState mouse_state)
        {
            if (gamepad_state.ThumbSticks.Right != Vector2.Zero)
            {
                Vector2 dir = gamepad_state.ThumbSticks.Right;
                dir.Normalize();

                if (dir.X >= 0) rotation = (float)Math.Atan(-dir.Y / dir.X) - MathHelper.PiOver2;
                else rotation = (float)Math.Atan(-dir.Y / dir.X) + MathHelper.PiOver2;
                origin = new Vector2(13, 11);     //18, 27origin of rotation for turret 
            }
            else
            {
                //Vector2 dir = new Vector2(mouse_state.X - (Position.X + Size.Width / 2), (mouse_state.Y - (Position.Y + Size.Height / 2)));
                Vector2 dir = new Vector2(mouse_state.X - (Position.X + 13), mouse_state.Y - (Position.Y + 11));
                dir.Normalize();
                if (dir.X >= 0) rotation = (float)Math.Atan(dir.Y / dir.X) - MathHelper.PiOver2;
                else rotation = (float)Math.Atan(dir.Y / dir.X) + MathHelper.PiOver2;
                origin = new Vector2(13, 11);     //18, 27origin of rotation for turret 
            }
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            base.Draw(theSpriteBatch);  //call Sprite.Draw only if the bullet is visible
        }
    }
}
