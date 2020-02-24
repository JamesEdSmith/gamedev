using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace PikeAndShot
{
    class DungeonScreen : GameScreen
    {
        public PikeAndShotGame _game;
        ArrayList sprites;
        Mole mole;

        protected KeyboardState keyboardState;
        protected KeyboardState previousKeyboardState;
        protected GamePadState gamePadState;
        protected GamePadState previousGamePadState;

        public DungeonScreen(PikeAndShotGame game)
        {
            _game = game;
            sprites = new ArrayList(10);
            mole = new Mole();
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(PikeAndShotGame.TEST, new Rectangle(5, 5, 100, 60), new Rectangle(0, 0, 1, 1), Color.White);
            mole.draw(spriteBatch);
        }

        public void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            mole.update(gameTime.ElapsedGameTime);
        }

        private void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                _game.Exit();

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                mole.moveLeft();
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                mole.moveRight();
            }
            else
            {
                mole.stopMoving();
            }
        }
    }
}
