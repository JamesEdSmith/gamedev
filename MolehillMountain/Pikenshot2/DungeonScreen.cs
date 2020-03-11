using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class DungeonScreen : GameScreen
    {
        public const int GRID_SIZE = 20;
        public const int GRID_WIDTH = 12;
        public const int GRID_HEIGHT = 9;
        public static Vector2 OFFSET = new Vector2(8, 6);

        public PikeAndShotGame _game;
        ArrayList sprites;
        Mole mole;
        Tunnel[,] tunnels;

        protected KeyboardState keyboardState;
        protected KeyboardState previousKeyboardState;
        protected GamePadState gamePadState;
        protected GamePadState previousGamePadState;

        int prevMoleRight;
        int prevMoleLeft;
        int prevMoleUp;
        int prevMoleDown;

        public DungeonScreen(PikeAndShotGame game)
        {
            _game = game;
            sprites = new ArrayList(10);
            sprites.Add(new Sprite(PikeAndShotGame.TURNIP, new Rectangle(0, 0, 20, 20), 20, 20, true));
            sprites.Add(new Sprite(PikeAndShotGame.TURNIP, new Rectangle(0, 0, 20, 20), 20, 20, true));
            sprites.Add(new Sprite(PikeAndShotGame.TURNIP, new Rectangle(0, 0, 20, 20), 20, 20, true));
            sprites.Add(new Sprite(PikeAndShotGame.TURNIP, new Rectangle(0, 0, 20, 20), 20, 20, true));
            sprites.Add(new Sprite(PikeAndShotGame.TURNIP, new Rectangle(0, 0, 20, 20), 20, 20, true));
            sprites.Add(new Sprite(PikeAndShotGame.TURNIP, new Rectangle(0, 0, 20, 20), 20, 20, true));
            mole = new Mole();
            tunnels = new Tunnel[GRID_WIDTH, GRID_HEIGHT];
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    tunnels[i, j] = new Tunnel(i * GRID_SIZE, j * GRID_SIZE);
                }
            }
            tunnels[0, 0].right = Tunnel.DUG;
            tunnels[1, 0].left = Tunnel.DUG;

            prevMoleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            prevMoleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            prevMoleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            prevMoleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Tunnel tunnel in tunnels)
            {
                tunnel.draw(spriteBatch);
            }
            mole.draw(spriteBatch);

            foreach (Sprite sprite in sprites)
            {
                sprite.setFrame(sprites.IndexOf(sprite));
                sprite.draw(spriteBatch, new Vector2(60 + 20 * sprites.IndexOf(sprite), 40) + OFFSET, BattleScreen.SIDE_PLAYER);
            }
        }

        public void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            mole.update(gameTime.ElapsedGameTime);

            int moleMiddleX = ((int)mole.position.X) / GRID_SIZE;
            int moleMiddleY = ((int)mole.position.Y) / GRID_SIZE;

            int moleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            int leftRemainder = ((int)mole.position.X - GRID_SIZE / 4) % GRID_SIZE;
            int moleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            int rightRemainder = ((int)mole.position.X + GRID_SIZE / 4) % GRID_SIZE;

            if (tunnels[moleRight, moleMiddleY].left == Tunnel.NOT_DUG && prevMoleRight == moleRight - 1)
            {
                tunnels[moleRight, moleMiddleY].left = Tunnel.HALF_DUG;
                tunnels[moleRight - 1, moleMiddleY].right = Tunnel.DUG;
            }
            if (tunnels[moleLeft, moleMiddleY].left == Tunnel.HALF_DUG && prevMoleLeft == moleLeft - 1)
            {
                tunnels[moleLeft, moleMiddleY].left = Tunnel.DUG;
            }

            if (tunnels[moleLeft, moleMiddleY].right == Tunnel.NOT_DUG && prevMoleLeft == moleLeft + 1)
            {
                tunnels[moleLeft, moleMiddleY].right = Tunnel.HALF_DUG;
                tunnels[moleLeft + 1, moleMiddleY].left = Tunnel.DUG;
            }
            if (tunnels[moleRight, moleMiddleY].right == Tunnel.HALF_DUG && prevMoleRight == moleRight + 1)
            {
                tunnels[moleRight, moleMiddleY].right = Tunnel.DUG;
            }

            prevMoleLeft = moleLeft;
            prevMoleRight = moleRight;

            int moleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            int upRemainder = ((int)mole.position.Y - GRID_SIZE / 4) % GRID_SIZE;
            int moleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;
            int downRemainder = ((int)mole.position.Y + GRID_SIZE / 4) % GRID_SIZE;

            if (tunnels[moleMiddleX, moleDown].top == Tunnel.NOT_DUG && prevMoleDown == moleDown - 1)
            {
                tunnels[moleMiddleX, moleDown].top = Tunnel.HALF_DUG;
                tunnels[moleMiddleX, moleDown - 1].bottom = Tunnel.DUG;
            }
            if (tunnels[moleMiddleX, moleUp].top == Tunnel.HALF_DUG && prevMoleUp == moleUp - 1)
            {
                tunnels[moleMiddleX, moleUp].top = Tunnel.DUG;
            }

            if (tunnels[moleMiddleX, moleUp].bottom == Tunnel.NOT_DUG && prevMoleUp == moleUp + 1)
            {
                tunnels[moleMiddleX, moleUp].bottom = Tunnel.HALF_DUG;
                tunnels[moleMiddleX, moleUp + 1].top = Tunnel.DUG;
            }
            if (tunnels[moleMiddleX, moleDown].bottom == Tunnel.HALF_DUG && prevMoleDown == moleDown + 1)
            {
                tunnels[moleMiddleX, moleDown].bottom = Tunnel.DUG;
            }

            if (mole.horzFacing == Sprite.DIRECTION_RIGHT)
            {
                if (tunnels[moleRight, moleMiddleY].right != Tunnel.DUG && (tunnels[moleRight, moleMiddleY].left != Tunnel.DUG || rightRemainder > 8))
                    mole.setDig(true);
                else
                    mole.setDig(false);
            }
            else if (mole.horzFacing == Sprite.DIRECTION_LEFT)
            {
                if (tunnels[moleLeft, moleMiddleY].left != Tunnel.DUG && (tunnels[moleLeft, moleMiddleY].right != Tunnel.DUG || leftRemainder < 10))
                    mole.setDig(true);
                else
                    mole.setDig(false);
            }

            if (mole.vertFacing == Sprite.DIRECTION_DOWN)
            {
                if (tunnels[moleMiddleX, moleDown].bottom != Tunnel.DUG && (tunnels[moleMiddleX, moleDown].top != Tunnel.DUG || downRemainder > 8))
                    mole.setDig(true);
                else
                    mole.setDig(false);
            }
            else if (mole.vertFacing == Sprite.DIRECTION_UP)
            {
                if (tunnels[moleMiddleX, moleUp].top != Tunnel.DUG && (tunnels[moleMiddleX, moleUp].bottom != Tunnel.DUG || upRemainder < 10))
                    mole.setDig(true);
                else
                    mole.setDig(false);
            }

            prevMoleUp = moleUp;
            prevMoleDown = moleDown;
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
            else if (keyboardState.IsKeyDown(Keys.Down))
            {
                mole.moveDown();
            }
            else if (keyboardState.IsKeyDown(Keys.Up))
            {
                mole.moveUp();
            }
            else
            {
                mole.stopMoving();
            }

            //if (keyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
            //{
            //    (sprites[0] as Sprite).nextFrame();
            //}

            previousKeyboardState = keyboardState;
        }
    }
}
