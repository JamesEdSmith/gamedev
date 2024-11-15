﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace PikeAndShot
{
    class TitleScreen : BattleScreen
    {
        Sprite title;

        private float waitTime = 2000f;
        private float fadeTime = 1000f;

        public TitleScreen(PikeAndShotGame game)
            : base(game)
        {
            title = new Sprite(PikeAndShotGame.TITLE_ANIMATION, new Microsoft.Xna.Framework.Rectangle(253, 122, 253, 122), 506, 244);

        }

        Color textColor;
        float animationTime = 300f;
        float textTimer;
        float textTime = 300f;


        public override void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            if (waitTime > 0)
            {
                waitTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else if (fadeTime > 0)
            {
                fadeTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                int maxFrames = title.getMaxFrames();
                float deathFrameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(fadeTime / deathFrameTime) - 1;

                title.setFrame(frameNumber);
            }
            else
            {
                textTimer -= (float)gameTime.ElapsedGameTime.Milliseconds;
                if (textTimer <= 0)
                {
                    textTimer = textTime + textTimer;
                }
                textColor = new Color(0.8f, 0.8f, 0f, Math.Abs(textTime - textTimer) / (textTime / 2f));
            }

        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            //spriteBatch.Draw(PikeAndShotGame.TEST, new Rectangle((PikeAndShotGame.SCREENWIDTH - 834) /2, (PikeAndShotGame.SCREENHEIGHT - 738) / 2, 834, 738), Color.White);
            //spriteBatch.Draw(PikeAndShotGame.TEST, new Rectangle(0, 0, 1024, PikeAndShotGame.SCREENHEIGHT), Color.White);
            title.draw(spriteBatch, new Vector2(PikeAndShotGame.SCREENWIDTH / 2f, PikeAndShotGame.SCREENHEIGHT / 3f), SIDE_PLAYER);
            if (fadeTime <= 0)
            {
                spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Press Pike or Shot", new Vector2(450, PikeAndShotGame.SCREENHEIGHT * 2f/ 3f), textColor);
            }

        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                
                _game.Exit();
            }
            if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }
            if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && !gamePadState.IsButtonDown(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }

            base.getInput(timeSpan);
            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        public void restart()
        {
            waitTime = 2000f;
            fadeTime = 1000f;
            title.setFrame(0);
        }
    }
}
