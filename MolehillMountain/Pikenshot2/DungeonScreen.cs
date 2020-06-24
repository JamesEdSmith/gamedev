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

        internal Tunnel getCurrTunnel(Vector2 position)
        {
            return tunnels[(int)position.X / GRID_SIZE, (int)position.Y / GRID_SIZE];
        }

        public PikeAndShotGame _game;
        Mole mole;
        public Tunnel[,] tunnels;
        ArrayList enemies;
        ArrayList vegetables;
        ArrayList pickups;
        ArrayList deadStuff;

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
            init();
        }

        internal Tunnel getTunnelBelow(Vector2 position)
        {
            int x = (int)(position.X / GRID_SIZE);
            int y = (int)(position.Y / GRID_SIZE) + 1;
            if (x < GRID_WIDTH && y < GRID_HEIGHT)
                return tunnels[x, y];
            else
                return null;
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Tunnel tunnel in tunnels)
            {
                tunnel.draw(spriteBatch);
            }

            foreach (Vegetable vegetable in vegetables)
            {
                vegetable.draw(spriteBatch);
            }

            foreach(Pickup pickup in pickups)
            {
                pickup.draw(spriteBatch);
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.draw(spriteBatch);
            }

            mole.draw(spriteBatch);

//            spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle((int)OFFSET.X, 110 + (int)OFFSET.Y, 70, 70), new Rectangle(128, 0, 70, 70), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);

        }

        public void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            mole.update(gameTime.ElapsedGameTime);

            foreach (Vegetable vege in vegetables)
            {
                vege.update(gameTime);
                if (vege.state == Vegetable.FALLING)
                {
                    checkCollisions(vege);
                }
                if (vege.state == Vegetable.DEAD)
                {
                    deadStuff.Add(vege);
                }
            }

            foreach (Vegetable vege in deadStuff)
            {
                vegetables.Remove(vege);
            }

            foreach(Pickup pickup in pickups)
            {
                pickup.update(gameTime);
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.update(gameTime.ElapsedGameTime);
            }

            if (mole.alive())
            {
                updateTunnels();
            }

        }

        void updateTunnels()
        {
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

        internal bool moleBelow(Vector2 position)
        {
            Vector2 absPos = (mole.position - position);

            if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(mole.position.Y - position.Y) > GRID_SIZE / 2 && (int)(mole.position.Y - position.Y) < GRID_SIZE + 5))
            {
                return true;
            }

            return false;
        }

        internal bool moleJustBelow(Vector2 position)
        {
            Vector2 absPos = (mole.position - position);

            if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(mole.position.Y - position.Y) > 0 && (int)(mole.position.Y - position.Y) <= GRID_SIZE / 4))
            {
                return true;
            }

            return false;
        }

        internal void squashMole(Vegetable vegetable)
        {
            mole.squash(vegetable);
        }

        internal bool vegetableRight(Mole mole, float movement)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state == Vegetable.NONE || vege.state == Vegetable.MOVING)
                {
                    if (vege.position.X - mole.position.X < GRID_SIZE - 7 && Math.Abs(vege.position.Y - mole.position.Y) < GRID_SIZE - 2 && vege.position.X - mole.position.X > 0)
                    {
                        vege.state = Vegetable.MOVING;
                        vege.position.X += movement;
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool vegetableLeft(Mole mole, float movement)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state == Vegetable.NONE || vege.state == Vegetable.MOVING)
                {
                    if (mole.position.X - vege.position.X < GRID_SIZE - 7 && Math.Abs(vege.position.Y - mole.position.Y) < GRID_SIZE - 2 && mole.position.X - vege.position.X > 0)
                    {
                        vege.state = Vegetable.MOVING;
                        vege.position.X -= movement;
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool vegetableBelow(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state != Vegetable.SPLITTING)
                {
                    if (Math.Abs(vege.position.X - mole.position.X) < GRID_SIZE - 8 && vege.position.Y - mole.position.Y <= GRID_SIZE && vege.position.Y - mole.position.Y > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool vegetableAbove(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state != Vegetable.SPLITTING)
                {
                    if (Math.Abs(vege.position.X - mole.position.X) < GRID_SIZE - 8 && mole.position.Y - vege.position.Y <= GRID_SIZE && mole.position.Y - vege.position.Y > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void checkCollisions(Vegetable vege)
        {
            int middleX = ((int)vege.position.X) / GRID_SIZE;
            int bottomY = ((int)vege.position.Y + GRID_SIZE / 2) / GRID_SIZE;

            if (vege.squashingMole)
            {
                bottomY = ((int)vege.position.Y + GRID_SIZE * 3 / 4) / GRID_SIZE;
            }

            if (bottomY >= GRID_HEIGHT)
            {
                vege.split();
            }
            else if (tunnels[middleX, bottomY].left != Tunnel.DUG && tunnels[middleX, bottomY].right != Tunnel.DUG
                && tunnels[middleX, bottomY].top != Tunnel.DUG && tunnels[middleX, bottomY].bottom != Tunnel.DUG)
            {
                if (tunnels[middleX, bottomY].top != Tunnel.HALF_DUG)
                {
                    if ((int)vege.position.Y - vege.fallingFrom > GRID_SIZE)
                    {
                        vege.split();
                    }
                    else
                    {
                        vege.land();
                    }
                }
            }
            else
            {
                tunnels[middleX, bottomY].top = Tunnel.DUG;
                tunnels[middleX, bottomY - 1].bottom = Tunnel.DUG;
            }
        }
        private void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                _game.Exit();

            if (mole.alive())
            {
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
            }

            if (keyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q)) 
            {
                enemies.Add(new Enemy(this));
            }
            else if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
            {
                init();
            }


            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        private void init()
        {
            mole = new Mole(this);
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

            vegetables = new ArrayList(5);
            //vegetables.Add(new Vegetable(4 * GRID_SIZE - GRID_SIZE * 0.5f, 3 * GRID_SIZE - GRID_SIZE * 0.5f, this));
            vegetables.Add(new Vegetable(2 * GRID_SIZE - GRID_SIZE * 0.5f, 3 * GRID_SIZE - GRID_SIZE * 0.5f, this));
            //vegetables.Add(new Vegetable(5 * GRID_SIZE - GRID_SIZE * 0.5f, 3 * GRID_SIZE - GRID_SIZE * 0.5f, this));

            pickups = new ArrayList(80);
            pickups.Add(new Pickup(5 * GRID_SIZE - GRID_SIZE * 0.5f, 2 * GRID_SIZE - GRID_SIZE * 0.5f, this));


            deadStuff = new ArrayList(5);
            enemies = new ArrayList(10);
        }
    }
}
