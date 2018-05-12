using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PikeAndShot
{
    class TitleScreen : BattleScreen
    {

        Sprite title;
        float waitTime = 3000f;
        float fadeTime = 1000f;
        float animationTime = 1000f;

        public TitleScreen(PikeAndShotGame game)
            : base(game)
        {
            title = new Sprite(PikeAndShotGame.TITLE_ANIMATION, new Rectangle(253, 122, 253, 122), 506, 244);
        }

        public override void update(Microsoft.Xna.Framework.GameTime gameTime)
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
        }

        public override void draw(Microsoft.Xna.Framework.GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            title.draw(spriteBatch, new Vector2(PikeAndShotGame.SCREENWIDTH / 2f, PikeAndShotGame.SCREENHEIGHT / 3f), SIDE_PLAYER);
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
            if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && !gamePadState.IsButtonDown(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }

            base.getInput(timeSpan);
            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }
    }
}
