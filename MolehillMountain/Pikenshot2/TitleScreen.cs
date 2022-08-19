using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace MoleHillMountain
{
    class TitleScreen : BattleScreen
    {

        Sprite title;
        float waitTime = 2000f;
        float fadeTime = 1000f;
        float animationTime = 1000f;
        float textTime = 2000f;
        float textTimer = 2000f;
        Color textColor;

        int frameNumber = 0;

        public TitleScreen(PikeAndShotGame game)
            : base(game)
        {
            //   title = new Sprite(PikeAndShotGame.TITLE_ANIMATION, new Rectangle(0, 0, 253, 122), 506, 244);
            title = new Sprite(PikeAndShotGame.TITLE_ANIMATION, new Rectangle(0, 0, 256, 192), 256, 192);
            textColor = new Color(0.8f, 0.8f, 0f);
        }

        public override void update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            //if (waitTime > 0)
            //{
            //    waitTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //}
            //else if (fadeTime > 0)
            //{
            //    fadeTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //    int maxFrames = title.getMaxFrames();
            //    float deathFrameTime = animationTime / (float)maxFrames;
            //    int frameNumber = maxFrames - (int)(fadeTime / deathFrameTime) - 1;

                title.setFrame(frameNumber);
            //}
            //else
            //{
            //    textTimer -= (float)gameTime.ElapsedGameTime.Milliseconds;
            //    if(textTimer <= 0)
            //    {
            //        textTimer = textTime + textTimer;
            //    }
            //    textColor = new Color(0.8f, 0.8f, 0f, Math.Abs(textTime - textTimer) / (textTime/2f) );
            //}
        }

        public override void draw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            title.draw(spriteBatch, new Vector2(10,0), SIDE_PLAYER, PikeAndShotGame.DUMMY_TIMESPAN);
            //spriteBatch.Draw(PikeAndShotGame.DOT, new Rectangle(0, 0, PikeAndShotGame.SCREENWIDTH, PikeAndShotGame.SCREENHEIGHT), Color.White);
            //spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle(0, 0, 200, 125), new Rectangle(0, 0, 200, 125), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
            //spriteBatch.Draw(PikeAndShotGame.TEXT, new Vector2(0,80), Color.White);
            

            spriteBatch.DrawString(PikeAndShotGame.GOBLIN_FONT, "Smith's Amusements", new Vector2(68f, 30f), Color.Black);
            spriteBatch.DrawString(PikeAndShotGame.GOBLIN_FONT, "Presents", new Vector2(100f, 45f), Color.Black);


        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                _game.Exit();
            if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }
            if ((keyboardState.IsKeyDown(Keys.X) && !previousKeyboardState.IsKeyDown(Keys.X)) || (gamePadState.IsButtonDown(Buttons.X) && !previousGamePadState.IsButtonDown(Buttons.X)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }
            if ((keyboardState.IsKeyDown(Keys.Q) && !previousKeyboardState.IsKeyDown(Keys.Q)))
            {
                if(title.getCurrFrame() == 1)
                {
                    frameNumber = 0;
                } else
                {
                    frameNumber = 1;
                }
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
