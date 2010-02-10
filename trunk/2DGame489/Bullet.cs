using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace _2DGame489
{
    class Bullet : Sprite
    {
        const int MAX_DIST = 850;   //max distance in pixels that the bullet can travel before losing visibility
        const string BULLET_ASSET_NAME = "bullet";
        

        public bool visible = false;    //is the bullet visible
        Vector2 start_position;
        Vector2 speed; 
        Vector2 direction;

        //sound stuff
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, BULLET_ASSET_NAME);
            Scale = 1.0f;
            is_projectile = true;   //bullet is a projectile

            //load audio
            audioEngine = new AudioEngine("Content/2DGame489Audio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Sound Bank.xsb");
        }

        public void Update(GameTime theGameTime)
        {
            if (Vector2.Distance(start_position, Position) > MAX_DIST)
            {
                visible = false;    //check to see if bullet has exceeded maximum distance
            }
            if (visible == true)
            {
                base.Update(theGameTime, speed, direction);
            }
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            if (visible == true)
            {
                soundBank.PlayCue("gunfire");
                base.Draw(theSpriteBatch);  //call Sprite.Draw only if the bullet is visible
            }
        }

        public void Fire(GamePadState currentGPState, MouseState currentMouseState, Vector2 theStartPos, Vector2 theSpeed, Vector2 theDirection)
        {
            Position = theStartPos;     //init bullet position to start position
            start_position = theStartPos;   //init start position
            speed = theSpeed;   //init bullet speed
            direction = theDirection;   //init direction
            visible = true;     //just fired... it must be visible
            Vector2 dir;
            if (currentGPState.ThumbSticks.Right != Vector2.Zero)
            {
                dir = currentGPState.ThumbSticks.Right;
            }
            else
            {
                dir.X = theDirection.X;
                dir.Y = -theDirection.Y;
            }
            dir.Normalize();
            if (dir.X >= 0) rotation = (float)Math.Atan(-dir.Y / dir.X);    //bullet fired to right... rot=atan(-Y/X)
            else rotation = (float)Math.Atan(-dir.Y / dir.X) + MathHelper.Pi;    //bullet fired to left... rot=atan(-Y/X)+pi
            origin = new Vector2(0, 9);     //origin of rotation for bullet                                                               //since orig picture faces right... need to flip horizontally    
        }
    }
}
