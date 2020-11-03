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
            if (((int)position.X / GRID_SIZE >= 0 && (int)position.X / GRID_SIZE < GRID_WIDTH) && ((int)position.Y / GRID_SIZE >= 0 && (int)position.Y / GRID_SIZE < GRID_HEIGHT))
            {
                return tunnels[(int)position.X / GRID_SIZE, (int)position.Y / GRID_SIZE];
            }
            else
            {
                return null;
            }
        }

        public PikeAndShotGame _game;
        public Mole mole;
        public Tunnel[,] tunnels;
        ArrayList enemies;
        ArrayList vegetables;
        ArrayList pickups;
        ArrayList stones;
        ArrayList deadStuff;

        protected KeyboardState keyboardState;
        protected KeyboardState previousKeyboardState;
        protected GamePadState gamePadState;
        protected GamePadState previousGamePadState;

        private float pickupTime;
        private float pickupTimer;
        private int pickupSequenceCount;
        bool firstCheck = true;

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

            foreach (Rat enemy in enemies)
            {
                enemy.draw(spriteBatch);
            }

            foreach (Stone stone in stones)
            {
                stone.draw(spriteBatch);
            }

            mole.draw(spriteBatch);

            //spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle((int)OFFSET.X, 80 + (int)OFFSET.Y, 70, 100), new Rectangle(128, 0, 70, 100), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
            //spriteBatch.Draw(PikeAndShotGame.SANDBOX, new Rectangle((int)OFFSET.X, 80 + (int)OFFSET.Y, 72, 20), new Rectangle(0, 1, 72, 20), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);

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

            foreach (Vegetable vege in vegetables)
            {
                if (vege.leftPushers.Count == 0 && vege.rightPushers.Count == 0 && vege.state == Vegetable.MOVING)
                {
                    vege.state = Vegetable.NONE;
                }
                vege.push((float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            foreach (Vegetable vege in deadStuff)
            {
                vegetables.Remove(vege);
            }
            deadStuff.Clear();

            foreach (Pickup pickup in pickups)
            {
                Vector2 distance = pickup.position - mole.position;
                if (distance.Length() <= 5)
                {
                    if (pickupTimer < 0)
                    {
                        pickupSequenceCount = 0;
                    }
                    else
                    {
                        pickupSequenceCount++;
                        if (pickupSequenceCount > 7)
                        {
                            pickupSequenceCount = 0;
                        }
                    }
                    pickupTimer = pickupTime;

                    pickup.collected(pickupSequenceCount);
                    deadStuff.Add(pickup);
                }
                pickup.update(gameTime);
            }

            foreach (Pickup pickup in deadStuff)
            {
                pickups.Remove(pickup);
            }
            deadStuff.Clear();

            foreach (Stone stone in stones)
            {
                stone.update(gameTime.ElapsedGameTime);
            }

            foreach (Rat enemy in enemies)
            {
                enemy.update(gameTime.ElapsedGameTime);
                if ((enemy.state & Mole.STATE_SQUASHED) == 0 && (enemy.state & Mole.STATE_NUDGING) == 0)
                {
                    updateTunnels(enemy);
                }

            }

            if (mole.alive())
            {
                updateTunnels(mole);
            }

            pickupTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        }

        public void spawnStone(Vector2 position, int horzFacing, int vertFacing)
        {
            stones.Add(new Stone(position, vertFacing, horzFacing, this));
        }

        internal int checkForTarget(Mole mole, Rat enemy, bool mad)
        {
            float yDiff = mole.position.Y - enemy.position.Y;
            float xDiff = mole.position.X - enemy.position.X;

            float slope = yDiff / xDiff;
            float yIntercept = enemy.position.Y - (slope * enemy.position.X);

            Tunnel startingTunnel = getCurrTunnel(enemy.position);
            Tunnel tunnel;
            Vector2 vect = Vector2.Zero;
            if (!mad)
            {
                if (yDiff >= 0)
                {
                    for (int y = (int)startingTunnel.position.Y + GRID_SIZE; y < mole.position.Y && y < GRID_SIZE * (GRID_HEIGHT - 1); y += GRID_SIZE)
                    {
                        float x = (y - yIntercept) / slope;

                        if (float.IsNaN(x))
                            x = enemy.position.X;

                        if (x < 0 || x > GRID_SIZE * GRID_WIDTH)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x;
                            vect.Y = y - 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.bottom == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.Y = y + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.top == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                else
                {
                    for (int y = (int)startingTunnel.position.Y; y > mole.position.Y && y > 0; y -= GRID_SIZE)
                    {
                        float x = (y - yIntercept) / slope;

                        if (float.IsNaN(x))
                            x = enemy.position.X;

                        if (x < 0 || x > GRID_SIZE * GRID_WIDTH)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x;
                            vect.Y = y - 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.bottom == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.Y = y + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.top == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                if (xDiff >= 0)
                {
                    for (int x = (int)startingTunnel.position.X + GRID_SIZE; x < mole.position.X && x < GRID_SIZE * (GRID_WIDTH - 1); x += GRID_SIZE)
                    {
                        float y = x * slope + yIntercept;
                        if (y < 0 || y > GRID_SIZE * GRID_HEIGHT)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x - 1;
                            vect.Y = y;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.right == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.X = x + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.left == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
                else
                {
                    for (int x = (int)startingTunnel.position.X; x > mole.position.X && x > 0; x -= GRID_SIZE)
                    {
                        float y = x * slope + yIntercept;
                        if (y < 0 || y > GRID_SIZE * GRID_HEIGHT)
                        {
                            break;
                        }
                        else
                        {
                            vect.X = x - 1;
                            vect.Y = y;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.right == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                            vect.X = x + 1;
                            tunnel = getCurrTunnel(vect);
                            if (tunnel.left == Tunnel.NOT_DUG)
                            {
                                return Mole.MOVING_NONE;
                            }
                        }
                    }
                }
            }

            if (Math.Abs(xDiff) > Math.Abs(yDiff))
            {
                if (xDiff > 0)
                    return Mole.MOVING_RIGHT;
                else
                    return Mole.MOVING_LEFT;
            }
            else
            {
                if (yDiff > 0)
                    return Mole.MOVING_DOWN;
                else
                    return Mole.MOVING_UP;
            }
        }

        internal bool vegetableRightClear(Rat mole)
        {
            Vector2 position = mole.position;
            if (!vegetableRight(mole.position, new ArrayList { }, Vegetable.NUDGE_SPACING))
            {
                return true;
            }
            else if ((int)(position.X - 5 + GRID_SIZE * 2) < GRID_WIDTH * GRID_SIZE)
            {
                Tunnel tunnelTwoOver = tunnels[(int)(position.X - 5 + GRID_SIZE * 2) / GRID_SIZE, (int)mole.position.Y / GRID_SIZE];
                if (tunnelTwoOver.left == Tunnel.DUG || tunnelTwoOver.left == Tunnel.HALF_DUG)
                {
                    return true;
                }
            }
            return false;
        }

        Vector2 moleWidth = new Vector2(GRID_SIZE / 2, 0);

        internal bool vegetableLeftClear(Rat mole)
        {
            Vector2 position = mole.position;

            if (!vegetableLeft(mole.position, new ArrayList { }, Vegetable.NUDGE_SPACING))
            {
                return true;
            }
            else if ((int)(position.X + 5 - GRID_SIZE * 2) > 0)
            {
                Tunnel tunnelTwoOver = tunnels[(int)(position.X + 5 - GRID_SIZE * 2) / GRID_SIZE, (int)mole.position.Y / GRID_SIZE];
                if (tunnelTwoOver.right == Tunnel.DUG || tunnelTwoOver.right == Tunnel.HALF_DUG)
                {
                    return true;
                }
            }
            return false;

        }

        internal int checkMoleSight(Vector2 position)
        {
            float distance = (mole.position - position).Length();

            if (distance <= GRID_SIZE * (mole.per * 0.5f + 0.1f))
            {
                return Pickup.SEEN;
            }
            else if (distance <= GRID_SIZE * (mole.per + 0.1f))
            {
                return Pickup.HALF_SEEN;
            }
            else
            {
                return Pickup.NOT_SEEN;
            }
        }


        void updateTunnels(Mole mole)
        {
            int moleMiddleX = ((int)mole.position.X) / GRID_SIZE;
            int moleMiddleY = ((int)mole.position.Y) / GRID_SIZE;

            int moleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            int moleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            int moleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            int moleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;

            if (mole.horzFacing == Sprite.DIRECTION_RIGHT)
            {
                if ((tunnels[moleRight, moleMiddleY].left != Tunnel.DUG && tunnels[moleRight, moleMiddleY].right != Tunnel.DUG) || mole.diggingTunnel == tunnels[moleRight, moleMiddleY])
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleRight, moleMiddleY];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }
            else if (mole.horzFacing == Sprite.DIRECTION_LEFT)
            {
                if ((tunnels[moleLeft, moleMiddleY].right != Tunnel.DUG && tunnels[moleLeft, moleMiddleY].left != Tunnel.DUG) || mole.diggingTunnel == tunnels[moleLeft, moleMiddleY])
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleLeft, moleMiddleY];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }

            if (mole.vertFacing == Sprite.DIRECTION_DOWN)
            {
                if ((tunnels[moleMiddleX, moleDown].top != Tunnel.DUG && tunnels[moleMiddleX, moleDown].bottom != Tunnel.DUG) || (mole.diggingTunnel == tunnels[moleMiddleX, moleDown] && tunnels[moleMiddleX, moleDown].bottom != Tunnel.DUG))
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleMiddleX, moleDown];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }
            else if (mole.vertFacing == Sprite.DIRECTION_UP)
            {
                if ((tunnels[moleMiddleX, moleUp].bottom != Tunnel.DUG && tunnels[moleMiddleX, moleUp].top != Tunnel.DUG) || (mole.diggingTunnel == tunnels[moleMiddleX, moleUp] && tunnels[moleMiddleX, moleDown].top != Tunnel.DUG))
                {
                    mole.setDig(true);
                    mole.diggingTunnel = tunnels[moleMiddleX, moleUp];
                }
                else
                {
                    mole.setDig(false);
                    mole.diggingTunnel = null;
                }
            }

            if (tunnels[moleRight, moleMiddleY].left == Tunnel.NOT_DUG && mole.prevMoleRight == moleRight - 1)
            {
                tunnels[moleRight, moleMiddleY].left = Tunnel.HALF_DUG;
                tunnels[moleRight - 1, moleMiddleY].right = Tunnel.DUG;
            }
            if (tunnels[moleLeft, moleMiddleY].left == Tunnel.HALF_DUG && mole.prevMoleLeft == moleLeft - 1)
            {
                tunnels[moleLeft, moleMiddleY].left = Tunnel.DUG;
            }

            if (tunnels[moleLeft, moleMiddleY].right == Tunnel.NOT_DUG && mole.prevMoleLeft == moleLeft + 1)
            {
                tunnels[moleLeft, moleMiddleY].right = Tunnel.HALF_DUG;
                tunnels[moleLeft + 1, moleMiddleY].left = Tunnel.DUG;
            }
            if (tunnels[moleRight, moleMiddleY].right == Tunnel.HALF_DUG && mole.prevMoleRight == moleRight + 1)
            {
                tunnels[moleRight, moleMiddleY].right = Tunnel.DUG;
            }

            mole.prevMoleLeft = moleLeft;
            mole.prevMoleRight = moleRight;

            if (tunnels[moleMiddleX, moleDown].top == Tunnel.NOT_DUG && mole.prevMoleDown == moleDown - 1)
            {
                tunnels[moleMiddleX, moleDown].top = Tunnel.HALF_DUG;
                tunnels[moleMiddleX, moleDown - 1].bottom = Tunnel.DUG;
            }
            if (tunnels[moleMiddleX, moleUp].top == Tunnel.HALF_DUG && mole.prevMoleUp == moleUp - 1)
            {
                tunnels[moleMiddleX, moleUp].top = Tunnel.DUG;
            }

            if (tunnels[moleMiddleX, moleUp].bottom == Tunnel.NOT_DUG && mole.prevMoleUp == moleUp + 1)
            {
                tunnels[moleMiddleX, moleUp].bottom = Tunnel.HALF_DUG;
                tunnels[moleMiddleX, moleUp + 1].top = Tunnel.DUG;
            }
            if (tunnels[moleMiddleX, moleDown].bottom == Tunnel.HALF_DUG && mole.prevMoleDown == moleDown + 1)
            {
                tunnels[moleMiddleX, moleDown].bottom = Tunnel.DUG;
            }

            mole.prevMoleUp = moleUp;
            mole.prevMoleDown = moleDown;

        }

        internal Vector2 getMolePosition()
        {
            return mole.position;
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

        internal bool moleJustBelow(Vegetable vegetable)
        {
            Vector2 position = vegetable.position;
            Vector2 absPos = (mole.position - position);
            bool squashedSomeone = false;

            if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(mole.position.Y - position.Y) > 0 && (int)(mole.position.Y - position.Y) <= GRID_SIZE / 4))
            {
                squashMole(vegetable);
                squashedSomeone = true;
            }

            foreach (Rat enemy in enemies)
            {
                absPos = (enemy.position - position);
                if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(enemy.position.Y - position.Y) >= -GRID_SIZE / 8 && (int)(enemy.position.Y - position.Y) <= GRID_SIZE / 4))
                {
                    enemy.squash(vegetable);
                    squashedSomeone = true;
                }
            }

            foreach (Vegetable vege in vegetables)
            {
                absPos = (vege.position - position);
                if (Math.Abs(absPos.X) < GRID_SIZE / 2 && ((int)(vege.position.Y - position.Y) > 0
                    && (int)(vege.position.Y - position.Y) <= GRID_SIZE * 3 / 4)
                    && vege.state != Vegetable.FALLING)
                {
                    vege.split();
                    vegetable.split();
                    squashedSomeone = true;
                }
            }

            return squashedSomeone;
        }

        internal void squashMole(Vegetable vegetable)
        {
            mole.squash(vegetable);
        }

        internal bool vegetableRight(Vector2 position, ArrayList pushers, float spacing)
        {
            foreach (Vegetable vege in vegetables)
            {
                if ((vege.state == Vegetable.NONE || vege.state == Vegetable.MOVING) && position != vege.position)
                {
                    if (vege.position.X - position.X < GRID_SIZE - spacing && Math.Abs(vege.position.Y - position.Y) < GRID_SIZE - 2 && vege.position.X - position.X > 0)
                    {
                        vege.state = Vegetable.MOVING;
                        vege.leftPushers.AddRange(pushers);
                        vegetableRight(vege.position, pushers, Vegetable.NUDGE_SPACING);
                        moleRight(vege, spacing);
                        return true;
                    }
                }
            }
            return false;
        }

        private void moleRight(Vegetable vegetable, float spacing)
        {
            if (mole.position.X - vegetable.position.X < GRID_SIZE - spacing
                && Math.Abs(mole.position.Y - vegetable.position.Y) < GRID_SIZE - 2
                && mole.position.X - vegetable.position.X > 0
                && !vegetable.rightPushers.Contains(mole))
            {
                vegetable.rightPushers.Add(mole);
            }

            foreach (Rat enemy in enemies)
            {
                if (enemy.position.X - vegetable.position.X < GRID_SIZE - spacing
                && Math.Abs(enemy.position.Y - vegetable.position.Y) < GRID_SIZE - 2
                && enemy.position.X - vegetable.position.X > 0
                && !vegetable.rightPushers.Contains(enemy))
                {
                    vegetable.rightPushers.Add(enemy);
                }
            }
        }

        internal bool vegetableLeft(Vector2 position, ArrayList pushers, float spacing)
        {
            foreach (Vegetable vege in vegetables)
            {
                if ((vege.state == Vegetable.NONE || vege.state == Vegetable.MOVING) && position != vege.position)
                {
                    if (position.X - vege.position.X < GRID_SIZE - spacing && Math.Abs(vege.position.Y - position.Y) < GRID_SIZE - 2 && position.X - vege.position.X > 0)
                    {
                        vege.state = Vegetable.MOVING;
                        vege.rightPushers.AddRange(pushers);
                        vegetableLeft(vege.position, pushers, Vegetable.NUDGE_SPACING);
                        moleLeft(vege, spacing);
                        return true;
                    }
                }
            }
            return false;
        }

        private void moleLeft(Vegetable vegetable, float spacing)
        {
            if (vegetable.position.X - mole.position.X < GRID_SIZE - spacing
                && Math.Abs(vegetable.position.Y - mole.position.Y) < GRID_SIZE - 2
                && vegetable.position.X - mole.position.X > 0
                && !vegetable.leftPushers.Contains(mole))
            {
                vegetable.leftPushers.Add(mole);
            }

            foreach (Rat enemy in enemies)
            {
                if (vegetable.position.X - enemy.position.X < GRID_SIZE - spacing
                && Math.Abs(vegetable.position.Y - enemy.position.Y) < GRID_SIZE - 2
                && vegetable.position.X - enemy.position.X > 0
                && !vegetable.leftPushers.Contains(enemy))
                {
                    vegetable.leftPushers.Add(enemy);
                }
            }
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

        internal bool vegetableDirectlyBelow(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state != Vegetable.SPLITTING)
                {
                    if (Math.Abs(vege.position.X - mole.position.X) < GRID_SIZE / 2 - 2 && vege.position.Y - mole.position.Y <= GRID_SIZE && vege.position.Y - mole.position.Y > 0)
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

        internal bool vegetableFallingAbove(Mole mole)
        {
            foreach (Vegetable vege in vegetables)
            {
                if (vege.state == Vegetable.FALLING)
                {
                    Vector2 absPos = (mole.position - vege.position);
                    if (Math.Abs(absPos.X) < GRID_SIZE / 2 && mole.position.Y - vege.position.Y <= GRID_SIZE && mole.position.Y - vege.position.Y > 0)
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

            if (vege.squashing)
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
                    if ((int)vege.position.Y - vege.fallingFrom > GRID_SIZE || vege.gonnaBreak)
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

            if (!firstCheck)
            {
                // Allows the game to exit
                if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    _game.Exit();

                if (mole.alive() && (mole.state & Mole.STATE_USE) == 0)
                {
                    if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
                    {
                        mole.stopMoving();
                        mole.useItem();
                    }
                    else
                    {

                        if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < 0)
                        {
                            mole.moveLeft();
                        }
                        else if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0)
                        {
                            mole.moveRight();
                        }
                        else if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.IsButtonDown(Buttons.DPadDown) || gamePadState.ThumbSticks.Left.Y < 0)
                        {
                            mole.moveDown();
                        }
                        else if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.IsButtonDown(Buttons.DPadUp) || gamePadState.ThumbSticks.Left.Y > 0)
                        {
                            mole.moveUp();
                        }
                        else
                        {
                            mole.stopMoving();
                        }
                    }
                }

                if (keyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
                {
                    enemies.Add(new Rat(this));
                }
                else if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
                {
                    init();
                }
            }
            else
            {
                firstCheck = false;
            }

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        private void init()
        {
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
            stones = new ArrayList(10);

            generateLevel();

            mole.prevMoleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
            mole.prevMoleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
            mole.prevMoleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
            mole.prevMoleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;

            foreach (Mole mole in enemies)
            {
                mole.prevMoleLeft = ((int)mole.position.X - GRID_SIZE / 4) / GRID_SIZE;
                mole.prevMoleRight = ((int)mole.position.X + GRID_SIZE / 4) / GRID_SIZE;
                mole.prevMoleUp = ((int)mole.position.Y - GRID_SIZE / 4) / GRID_SIZE;
                mole.prevMoleDown = ((int)mole.position.Y + GRID_SIZE / 4) / GRID_SIZE;
            }

            pickupTime = 22f / mole.getDigSpeed();
            pickupTimer = -1;
        }

        int[,] combinedTunnels;
        private void generateLevel()
        {
            const int generations = 2;
            int tunnelId = 1;
            int[][,] generatedTunnels = new int[generations][,];
            combinedTunnels = new int[GRID_WIDTH, GRID_HEIGHT];
            ArrayList vegetablePlacements = new ArrayList(10);
            ArrayList pickupClusters = new ArrayList(20);

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
                            foreach (Point point in vegetablePlacements)
                            {
                                if (point.X == i && Math.Abs(point.Y - j) < 2)
                                {
                                    vegetableVertical = true;
                                }
                            }
                            if (!vegetableVertical)
                            {
                                Point addedPoint = new Point(i, j);
                                vegetablePlacements.Add(addedPoint);
                                if (combinedTunnels[i + 1, j] == 0 && combinedTunnels[i - 1, j] == 0 && combinedTunnels[i, j + 2] == 0)
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

            ArrayList possibleSpawns = new ArrayList(GRID_WIDTH * GRID_HEIGHT);

            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] != 0)
                    {
                        if (i > 0 && combinedTunnels[i, j] == combinedTunnels[i - 1, j])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                        else if (i < GRID_WIDTH - 1 && combinedTunnels[i, j] == combinedTunnels[i + 1, j])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                        else if (j > 0 && combinedTunnels[i, j] == combinedTunnels[i, j - 1])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                        else if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j] == combinedTunnels[i, j + 1])
                        {
                            possibleSpawns.Add(new Point(i, j));
                        }
                    }
                }
            }

            ArrayList narrowDownSpawns = new ArrayList(GRID_WIDTH * GRID_HEIGHT);
            int distance = 100;
            int xDist;
            int yDist;

            foreach (Point point in possibleSpawns)
            {
                xDist = point.X;
                yDist = point.Y;
                if (point.X > GRID_WIDTH * 0.5f)
                {
                    xDist = GRID_WIDTH - point.X;
                }
                if (point.Y > GRID_HEIGHT * 0.5f)
                {
                    xDist = GRID_HEIGHT - point.Y;
                }

                if (xDist < distance)
                {
                    distance = xDist;
                }
                if (yDist < distance)
                {
                    distance = yDist;
                }
            }

            foreach (Point point in possibleSpawns)
            {
                xDist = point.X;
                yDist = point.Y;
                if (point.X > GRID_WIDTH * 0.5f)
                {
                    xDist = GRID_WIDTH - point.X;
                }
                if (point.Y > GRID_HEIGHT * 0.5f)
                {
                    xDist = GRID_HEIGHT - point.Y;
                }

                if (xDist == distance || yDist == distance)
                {
                    narrowDownSpawns.Add(point);
                }
            }

            Point point2 = (Point)narrowDownSpawns[random.Next(narrowDownSpawns.Count)];

            mole = new Mole(point2.X, point2.Y, this);

            //remove all tunnel identities so the can all just be '1' for "tunnel" for the rest of the proc gen
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] != 0)
                    {
                        combinedTunnels[i, j] = 1;
                    }
                }
            }

            //place vegetables
            int totalVegetables = random.Next(4, 6);
            if (vegetablePlacements.Count < totalVegetables)
            {
                totalVegetables = vegetablePlacements.Count;
            }

            for (int i = 0; i < totalVegetables; i++)
            {
                int vegetableSpotIndex = random.Next(vegetablePlacements.Count);
                Point vegetableSpot = (Point)vegetablePlacements[vegetableSpotIndex];
                vegetables.Add(new Vegetable(vegetableSpot.X * GRID_SIZE + GRID_SIZE * 0.5f, vegetableSpot.Y * GRID_SIZE + GRID_SIZE * 0.5f, this));
                vegetablePlacements.RemoveAt(vegetableSpotIndex);
                combinedTunnels[vegetableSpot.X, vegetableSpot.Y] = 2; // 2 for vegetable
            }

            //populate grub
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    if (combinedTunnels[i, j] == 0)
                    {
                        ArrayList paths = new ArrayList();
                        generatePickupClusters(paths, new ArrayList(), i, j);
                        if (paths.Count > 0)
                        {
                            pickupClusters.Add(paths[random.Next(paths.Count)]);
                        }
                    }
                }
            }

            if (pickupClusters.Count < 3)
            {
                //level couldn't populate any pickup clusters(very rare), in future might make this cause something interesting, but for now just redo generation
                init();
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    int pickupClusterIndex = random.Next(pickupClusters.Count);
                    ArrayList pickupCluster = (ArrayList)pickupClusters[pickupClusterIndex];
                    pickupClusters.RemoveAt(pickupClusterIndex);
                    foreach (Point point in pickupCluster)
                    {
                        pickups.Add(new Pickup(point.X, point.Y, this));
                    }
                }

                placeEnemies();
            }
        }


        private void placeEnemies()
        {
            ArrayList[] tunnelBuckets = new ArrayList[10];
            for (int j = 0; j < 10; j++)
            {
                tunnelBuckets[j] = new ArrayList();
            }

            //fill tunnel buckets
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    int adjacent = 0;
                    float debug = (mole.position - new Vector2(i * GRID_SIZE, j * GRID_SIZE)).Length();

                    if ((mole.position - new Vector2(i * GRID_SIZE, j * GRID_SIZE)).Length() > 80)
                    {

                        if (combinedTunnels[i, j] == 1)
                        {
                            adjacent++;
                            if (i > 0 && combinedTunnels[i - 1, j] == 1)
                            {
                                adjacent++;
                                if (j > 0 && combinedTunnels[i - 1, j - 1] == 1)
                                {
                                    adjacent++;
                                }
                                if (j < GRID_HEIGHT - 1 && combinedTunnels[i - 1, j + 1] == 1)
                                {
                                    adjacent++;
                                }
                            }
                            if (i < GRID_WIDTH - 1 && combinedTunnels[i + 1, j] == 1)
                            {
                                adjacent++;
                                if (j > 0 && combinedTunnels[i + 1, j - 1] == 1)
                                {
                                    adjacent++;
                                }
                                if (j < GRID_HEIGHT - 1 && combinedTunnels[i + 1, j + 1] == 1)
                                {
                                    adjacent++;
                                }
                            }
                            if (j > 0 && combinedTunnels[i, j - 1] == 1)
                            {
                                adjacent++;
                            }
                            if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j + 1] == 1)
                            {
                                adjacent++;
                            }
                        }
                    }
                    tunnelBuckets[adjacent].Add(new Point(i, j));
                }
            }

            //place rats
            int enemyCount = random.Next(1, 4);
            for (int i = 0; i < enemyCount; i++)
            {
                int index = 7;
                ArrayList possibleSpots = new ArrayList();
                while (possibleSpots.Count < 5)
                {
                    if (tunnelBuckets[index].Count > 0)
                    {
                        int chosen = random.Next(tunnelBuckets[index].Count);
                        possibleSpots.Add(tunnelBuckets[index][chosen]);
                        tunnelBuckets[index].RemoveAt(chosen);
                    }
                    else
                    {
                        index--;
                    }
                }
                Point chosenPoint = (Point)possibleSpots[random.Next(possibleSpots.Count)];
                enemies.Add(new Rat(this, chosenPoint.X, chosenPoint.Y));
            }

        }

        private void generatePickupClusters(ArrayList paths, ArrayList path, int i, int j)
        {
            if (combinedTunnels[i, j] == 0)
            {
                path.Add(new Point(i, j));
                combinedTunnels[i, j] = 3;
                if (path.Count == 8)
                {
                    if (checkForTwoRoutes(path))
                    {
                        paths.Add(path);
                    }
                }
                else
                {
                    if (i > 0 && combinedTunnels[i - 1, j] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i - 1, j);
                    }
                    if (i < GRID_WIDTH - 1 && combinedTunnels[i + 1, j] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i + 1, j);
                    }
                    if (j > 0 && combinedTunnels[i, j - 1] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i, j - 1);
                    }
                    if (j < GRID_HEIGHT - 1 && combinedTunnels[i, j + 1] == 0)
                    {
                        generatePickupClusters(paths, new ArrayList(path), i, j + 1);
                    }
                }
            }
        }

        private bool checkForTwoRoutes(ArrayList path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Point checkThisPoint = (Point)path[i];
                int tally = 0;

                foreach (Point point in path)
                {
                    if (path.IndexOf(point) != i && Math.Abs(checkThisPoint.X - point.X) <= 1 && Math.Abs(checkThisPoint.Y - point.Y) <= 1)
                    {
                        tally++;
                    }
                }
                if (tally < 3)
                {
                    return false;
                }
            }
            return true;
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
