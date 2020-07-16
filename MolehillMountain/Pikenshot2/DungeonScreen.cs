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
        static Random random = new Random();
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

            foreach (Pickup pickup in pickups)
            {
                pickup.draw(spriteBatch);
            }

            foreach (Enemy enemy in enemies)
            {
                enemy.draw(spriteBatch);
            }

            mole.draw(spriteBatch);

            //spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle((int)OFFSET.X, 80 + (int)OFFSET.Y, 70, 100), new Rectangle(128, 0, 70, 100), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);

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

            foreach (Pickup pickup in pickups)
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

        internal int checkMoleSight(Vector2 position)
        {
            Vector2 distance = mole.position - position;
            float avg = (Math.Abs(distance.X) + Math.Abs(distance.Y)) / 2f;
            //if ((int)Math.Abs(distance.X) <= GRID_SIZE && (int)Math.Abs(distance.Y) <= GRID_SIZE)
            //{
            //    return Pickup.SEEN;
            //}
            //else if ((int)Math.Abs(distance.X) <= GRID_SIZE * 2 && (int)Math.Abs(distance.Y) <= GRID_SIZE * 2)
            //{
            //    return Pickup.HALF_SEEN;
            //}
            //else
            //{
            //    return Pickup.NOT_SEEN;
            //}
            if (avg <= GRID_SIZE * 0.9f)
            {
                return Pickup.SEEN;
            }
            else if (avg <= GRID_SIZE * 1.5f)
            {
                return Pickup.HALF_SEEN;
            }
            else
            {
                return Pickup.NOT_SEEN;
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

            //tunnels[0, 0].right = Tunnel.DUG;
            //tunnels[1, 0].left = Tunnel.DUG;
            vegetables = new ArrayList(5);
            //vegetables.Add(new Vegetable(4 * GRID_SIZE - GRID_SIZE * 0.5f, 3 * GRID_SIZE - GRID_SIZE * 0.5f, this));
            //vegetables.Add(new Vegetable(2 * GRID_SIZE - GRID_SIZE * 0.5f, 3 * GRID_SIZE - GRID_SIZE * 0.5f, this));
            //vegetables.Add(new Vegetable(5 * GRID_SIZE - GRID_SIZE * 0.5f, 3 * GRID_SIZE - GRID_SIZE * 0.5f, this));

            pickups = new ArrayList(40);
            //pickups.Add(new Pickup(5, 2, this));
            //pickups.Add(new Pickup(6, 2, this));

            deadStuff = new ArrayList(5);
            enemies = new ArrayList(10);

            generateLevel();

            prevMoleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            prevMoleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            prevMoleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            prevMoleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;
        }

        private void generateLevel()
        {
            const int generations = 2;
            int tunnelId = 1;
            int[][,] generatedTunnels = new int[generations][,];
            int[,] combinedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            ArrayList vegetablePlacements = new ArrayList(10);

            //generate
            for (int i = 0; i < generations; i++)
            {
                switch (random.Next(6))
                {
                    case 0:
                    case 1:
                        generatedTunnels[i] = generateVerticalLine(tunnelId);
                        break;
                    case 2:
                    case 3:
                        generatedTunnels[i] = generateHorizontalLine(tunnelId);
                        break;
                    default:
                        generatedTunnels[i] = generateLoop(tunnelId);
                        break;
                }
                if (random.Next(3) == 0)
                {
                    tunnelId++;
                }
            }

            for (int k = 0; k < generations; k++)
            {
                for (int j = 0; j < GRID_HEIGHT; j++)
                {
                    for (int i = 0; i < GRID_WIDTH; i++)
                    {
                        if (generatedTunnels[k][i, j] != 0)
                        {
                            combinedTunnels[i, j] = generatedTunnels[k][i, j];
                        }
                    }
                }
            }

            //translate to the actual Tunnel array
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] != 0)
                    {
                        //dig tunnels
                        if (i > 0 && combinedTunnels[i - 1, j] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].left = Tunnel.DUG;
                        }
                        if (i < GRID_WIDTH - 1 && combinedTunnels[i + 1, j] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].right = Tunnel.DUG;
                        }
                        if (j > 0 && combinedTunnels[i, j - 1] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].top = Tunnel.DUG;
                        }
                        if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j + 1] == combinedTunnels[i, j])
                        {
                            tunnels[i, j].bottom = Tunnel.DUG;
                        }
                    }
                    else
                    {
                        //populate possible position vegetable array
                        if (j < GRID_HEIGHT - 2 && j > 0 && combinedTunnels[i, j + 1] == 0 && i > 0 && i < GRID_WIDTH - 1)
                        {
                            bool vegetableVertical = false;
                            foreach(Point point in vegetablePlacements)
                            {
                                if(point.X == i && Math.Abs(point.Y - j) < 2)
                                {
                                    vegetableVertical = true;
                                }
                            }
                            if (!vegetableVertical)
                            {
                                Point addedPoint = new Point(i, j);
                                vegetablePlacements.Add(addedPoint);
                                if(combinedTunnels[i + 1, j] == 0 && combinedTunnels[i - 1, j] == 0 && combinedTunnels[i, j+2] == 0)
                                {
                                    if (random.Next(5) != 0)
                                    {
                                        vegetablePlacements.Remove(addedPoint);
                                    }
                                }
                                else if (j > GRID_HEIGHT * 0.5f)
                                {
                                    if (random.Next(2) != 0)
                                    {
                                        vegetablePlacements.Remove(addedPoint);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            int totalVegetables = random.Next(4, 6);
            for (int i = 0; i < totalVegetables; i++)
            {
                int vegetableSpotIndex = random.Next(vegetablePlacements.Count);
                Point vegetableSpot = (Point)vegetablePlacements[vegetableSpotIndex];
                vegetables.Add(new Vegetable(vegetableSpot.X * GRID_SIZE + GRID_SIZE * 0.5f, vegetableSpot.Y * GRID_SIZE + GRID_SIZE * 0.5f, this));
                vegetablePlacements.RemoveAt(vegetableSpotIndex);
            }
        }

        private int[,] generateHorizontalLine(int id)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            int currX = 0;
            int currY = random.Next(GRID_HEIGHT);
            int result = 0;
            int lastResult;

            while (currX < GRID_WIDTH && generatedTunnels[currX, currY] != id)
            {
                generatedTunnels[currX, currY] = id;
                lastResult = result;
                result = random.Next(6);
                switch (result)
                {
                    case 0:
                        if (currY > 0 && generatedTunnels[currX, currY - 1] != id && lastResult != id)
                        {
                            currY--;
                        }
                        else
                        {
                            currX++;
                        }
                        break;
                    case 1:
                        if (currY < GRID_HEIGHT - 1 && generatedTunnels[currX, currY + 1] != id && lastResult != 0)
                        {
                            currY++;
                        }
                        else
                        {
                            currX++;
                        }
                        break;
                    default:
                        currX++;
                        break;
                }
                if (random.Next(random.Next(10, 30)) == 0)
                {
                    id++;
                }
            }
            return generatedTunnels;
        }

        private int[,] generateLoop(int id)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];

            int radiusX = random.Next(3, (GRID_WIDTH + GRID_HEIGHT) / 4);
            int radiusY = random.Next(3, (GRID_WIDTH + GRID_HEIGHT) / 4);

            int currX = random.Next(radiusX, GRID_WIDTH - radiusX);
            int initialX = currX;
            int currY = random.Next(0, GRID_HEIGHT - radiusY * 2); ;
            int initial = currY;

            while (currX - initialX < radiusX)
            {
                generatedTunnels[currX, currY] = id;

                int downPaths = (int)(((float)currX - (float)initialX) * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY++;
                    if (currY >= GRID_HEIGHT)
                    {
                        currY = GRID_HEIGHT - 1;
                    }
                    generatedTunnels[currX, currY] = 1;
                }
                currX++;
                if (currX >= GRID_WIDTH)
                {
                    currX = GRID_WIDTH - 1;
                }
            }

            while (currX - initialX > 0)
            {
                generatedTunnels[currX, currY] = id;

                int downPaths = (int)(((float)currX - (float)initialX) * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY++;
                    if (currY >= GRID_HEIGHT)
                    {
                        currY = GRID_HEIGHT - 1;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX--;
                if (currX < 0)
                {
                    currX = 0;
                }
            }

            while (currX - initialX > -radiusX)
            {
                generatedTunnels[currX, currY] = 1;

                int downPaths = (int)(((float)currX - (float)initialX) * -1 * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY--;
                    if (currY < 0)
                    {
                        currY = 0;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX--;
                if (currX < 0)
                {
                    currX = 0;
                }
            }

            while (currX - initialX < 0)
            {
                generatedTunnels[currX, currY] = 1;

                int downPaths = (int)(((float)currX - (float)initialX) * -1 * ((float)radiusY / (float)(radiusX + 1)));
                for (int j = 0; j < downPaths; j++)
                {
                    currY--;
                    if (currY < 0)
                    {
                        currY = 0;
                    }
                    generatedTunnels[currX, currY] = id;
                }
                currX++;
                if (currX >= GRID_WIDTH)
                {
                    currX = GRID_WIDTH - 1;
                }
            }

            return generatedTunnels;
        }

        private int[,] generateVerticalLine(int id)
        {
            int[,] generatedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            int currX = random.Next(GRID_WIDTH);
            int currY = 0;
            int result = 0;
            int lastResult;

            while (currY < GRID_HEIGHT && generatedTunnels[currX, currY] != id)
            {
                generatedTunnels[currX, currY] = id;
                lastResult = result;
                result = random.Next(6);
                switch (result)
                {
                    case 0:
                        if (currX > 0 && generatedTunnels[currX - 1, currY] != id && lastResult != id)
                        {
                            currX--;
                        }
                        else
                        {
                            currY++;
                        }
                        break;
                    case 1:
                        if (currX < GRID_WIDTH - 1 && generatedTunnels[currX + 1, currY] != id && lastResult != 0)
                        {
                            currX++;
                        }
                        else
                        {
                            currY++;
                        }
                        break;
                    default:
                        currY++;
                        break;
                }
                if (random.Next(random.Next(10, 30)) == 0)
                {
                    id++;
                }
            }
            return generatedTunnels;
        }
    }
}
