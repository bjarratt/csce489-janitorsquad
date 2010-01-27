﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace _2DGame489
{
    class Jeep : Sprite
    {
        const string JEEP_ASSET_NAME = "jeep-hurricane-9";  
        const int START_POS_X = 400;    //Player starting position
        const int START_POS_Y = 450;
        const int JEEP_SPEED = 600;     //speed constant
        const int UP = -1;
        const int DOWN = 1;
        const int RIGHT = 1;
        const int LEFT = -1;
        float firing_rate = 0;  //used for continuous but regulated fire from turret machine gun
        
        List<Bullet> bullets = new List<Bullet>();  //bullets for the turret
        ContentManager mContentManager;     //will be used to create new bullets when we need them

        Turret Jeep_Turret = new Turret();  //gun turret for jeep

        enum State      //what state is the jeep in?  Driving?  In the air?  etc...
        {
            Driving
        }
        State current_state = State.Driving;    //init current state to Driving

        Vector2 speed = Vector2.Zero;   //init jeep speed to zero
        Vector2 direction = Vector2.Zero;   //init jeep direction to zero

        KeyboardState previous_keyboard_state;  //save the previous keyboard state
        GamePadState previous_gamepad_state;    //save the previous gamepad state

        public void LoadContent(ContentManager theContentManager)
        {
            mContentManager = theContentManager;
            foreach (Bullet aBullet in bullets)     //load the bullets... so to speak :)
            {
                aBullet.LoadContent(theContentManager);
            }

            Position = new Vector2(START_POS_X, START_POS_Y);   //init player position
            base.LoadContent(theContentManager, JEEP_ASSET_NAME);
            Source = new Rectangle(0, 0, Source.Width, Source.Height);
            rotation = 0.0f;    //no need to rotate the jeep... init to 0.0f
            is_projectile = false;  //the jeep is not a projectile

            Jeep_Turret.Position = new Vector2(Position.X + 40, Position.Y + 75);
            Jeep_Turret.LoadContent(theContentManager);
        }

        public void Update(GameTime theGameTime)
        {
            KeyboardState current_key_state = Keyboard.GetState();
            GamePadState current_gamepad_state = GamePad.GetState(PlayerIndex.One);
            MouseState current_mouse_state = Mouse.GetState();

            UpdateMovement(current_key_state, current_gamepad_state);   //update the player movement
            UpdateBullet(theGameTime, current_key_state, current_gamepad_state, current_mouse_state);    //update bullet movements
            UpdateTurret(Position, speed, direction, theGameTime, current_key_state, current_gamepad_state, current_mouse_state);   //update turret

            previous_keyboard_state = current_key_state;    //store previous states
            previous_gamepad_state = current_gamepad_state;

            base.Update(theGameTime, speed, direction);     //call Sprite.Update
            Jeep_Turret.Position = new Vector2(Position.X + 40, Position.Y + 75);
        }

        private void UpdateMovement(KeyboardState currentKeyState, GamePadState currentGamepadState)
        {
            if (current_state == State.Driving)     //only accept movement commands if Driving
            {
                speed = Vector2.Zero;   //zero out speed and direction
                direction = Vector2.Zero;
                if (currentGamepadState.ThumbSticks.Left != Vector2.Zero)   //look for gamepad input first
                {
                    direction = currentGamepadState.ThumbSticks.Left;
                    speed = new Vector2(JEEP_SPEED * (float)direction.Length(), -JEEP_SPEED * (float)direction.Length());
                }
                else
                {
                    if (currentKeyState.IsKeyDown(Keys.Left) == true)
                    {
                        speed.X = JEEP_SPEED;
                        direction.X = LEFT;
                    }
                    if (currentKeyState.IsKeyDown(Keys.Right) == true)
                    {
                        speed.X = JEEP_SPEED;
                        direction.X = RIGHT;
                    }
                    if (currentKeyState.IsKeyDown(Keys.Up) == true)
                    {
                        speed.Y = JEEP_SPEED;
                        direction.Y = UP;
                    }
                    if (currentKeyState.IsKeyDown(Keys.Down) == true)
                    {
                        speed.Y = JEEP_SPEED;
                        direction.Y = DOWN;
                    }
                }
            }
        }

        private void UpdateBullet(GameTime theGameTime, KeyboardState theKeyState, GamePadState theGPState, MouseState theMouseState)
        {
            foreach (Bullet aBullet in bullets)
            {
                aBullet.Update(theGameTime);    //call Bullet.Update for each Bullet in list
            }
            float time = (float)theGameTime.ElapsedGameTime.TotalSeconds;
            firing_rate += time;
            if (theGPState.ThumbSticks.Right != Vector2.Zero && firing_rate > 0.2)  //0.2 controls firing rate... only shoot a bullet if
            {                                                                       //time elapsed greater than 0.2 seconds
                ShootBullet(theGPState, theMouseState);
                firing_rate = 0.0f;
            }
            else if (theMouseState.LeftButton == ButtonState.Pressed && firing_rate > 0.2)
            {
                ShootBullet(theGPState, theMouseState);
                firing_rate = 0.0f;
            }
        }

        private void ShootBullet(GamePadState theGPState, MouseState theMouseState)
        {
            if (current_state == State.Driving) 
            {
                //we'll reuse bullets that have gone off screen rather than creating new ones... if at all possible
                bool aCreateNew = true;    //flag 
                foreach (Bullet aBullet in bullets)
                {
                    if (aBullet.visible == false)   //if there's a bullet that isn't visible, reuse it... no need to create another one
                    {
                        aCreateNew = false;     //set creation flag to false
                        if (theGPState.ThumbSticks.Right != Vector2.Zero)
                        {
                            Vector2 dir = new Vector2(theGPState.ThumbSticks.Right.X, -theGPState.ThumbSticks.Right.Y);
                            dir.Normalize();    //normalize so all bullets fly at same speed
                            aBullet.Fire(theGPState, theMouseState, Position + new Vector2(Size.Width / 2, Size.Height / 2) + dir * 30,
                                new Vector2(1000, 1000), dir);    //(1000,1000) is bullet speed...completely arbitrary
                        }
                        else                  //using mouse rather than gamepad
                        {
                            Vector2 dir = new Vector2(theMouseState.X - (Position.X + Size.Width / 2), (theMouseState.Y - (Position.Y + Size.Height / 2)));
                            dir.Normalize();
                            aBullet.Fire(theGPState, theMouseState, Position + new Vector2(Size.Width / 2, Size.Height / 2) + dir * 30,
                                new Vector2(1000, 1000), dir); 
                        }
                        break;
                    }
                }

                if (aCreateNew == true)     //all bullets in the list are currently visible, we need to create a new one
                {
                    Bullet aBullet = new Bullet();
                    aBullet.LoadContent(mContentManager);
                    if (theGPState.ThumbSticks.Right != Vector2.Zero)
                    {
                        Vector2 dir = new Vector2(theGPState.ThumbSticks.Right.X, -theGPState.ThumbSticks.Right.Y);
                        dir.Normalize();
                        aBullet.Fire(theGPState, theMouseState, Position + new Vector2(Size.Width / 2, Size.Height / 2) + dir * 30,
                            new Vector2(1000, 1000), dir);
                    }
                    else               //using mouse
                    {
                        Vector2 dir = new Vector2(theMouseState.X - (Position.X + Size.Width / 2), (theMouseState.Y - (Position.Y + Size.Height / 2)));
                        dir.Normalize();
                        aBullet.Fire(theGPState, theMouseState, Position + new Vector2(Size.Width / 2, Size.Height / 2) + dir * 30,
                                new Vector2(1000, 1000), dir);
                    }
                    bullets.Add(aBullet);   //add the new bullet to the list
                }
            }
        }

        private void UpdateTurret(Vector2 pos, Vector2 speed, Vector2 direction, GameTime theGameTime, KeyboardState key_state, GamePadState gamepad_state, MouseState mouse_state)
        {
            if (gamepad_state.ThumbSticks.Left != Vector2.Zero)
            {
                Jeep_Turret.MoveTurret(pos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            }
            else if (gamepad_state.ThumbSticks.Right != Vector2.Zero)
            {
                Jeep_Turret.RotateTurret(pos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            }
            else if (gamepad_state.ThumbSticks.Left != Vector2.Zero
                && gamepad_state.ThumbSticks.Right != Vector2.Zero)
            {
                Jeep_Turret.MoveTurret(pos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            }
            else if (key_state.IsKeyDown(Keys.Up) || key_state.IsKeyDown(Keys.Down) || key_state.IsKeyDown(Keys.Left) || key_state.IsKeyDown(Keys.Right))
            {
                Jeep_Turret.MoveTurret(pos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            }
            else
            {
                Jeep_Turret.RotateTurret(pos, speed, direction, theGameTime, gamepad_state, key_state, mouse_state);
            }
        }

        public override void Draw(SpriteBatch theSpriteBatch)
        {
            base.Draw(theSpriteBatch);  //draw jeep before bullets
            Jeep_Turret.Draw(theSpriteBatch);
            foreach (Bullet aBullet in bullets)
            {
                aBullet.Draw(theSpriteBatch);   //call Bullet.Draw for each bullet
            }
        }
    }
}