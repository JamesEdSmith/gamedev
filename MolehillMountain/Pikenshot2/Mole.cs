using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    internal class Mole
    {
        public const int MOVING_NONE = 0;
        public const int MOVING_LEFT = 1;
        public const int MOVING_RIGHT = 2;
        public const int MOVING_UP = 3;
        public const int MOVING_DOWN = 4;

        const int STATE_DIGGING = 1;

        float animationTimer;
        float walkSpeed = 30f;
        float walkTime = 325f;
        float digTime = 375f;

        Sprite walking;
        Sprite digging;
        Sprite walkingSprite;
        public Vector2 position;
        Vector2 drawPosition;
        //flags
        int state = 0;
        //notflags
        public int moving = 0;
        public int horzFacing = Sprite.DIRECTION_LEFT;
        public int vertFacing = Sprite.DIRECTION_NONE;

        public Mole()
        {
            walking = new Sprite(PikeAndShotGame.MOLE_MINER_WALKING, new Rectangle(0, 0, 18, 18), 18, 18);
            digging = new Sprite(PikeAndShotGame.MOLE_MINER_DIGGING, new Rectangle(0, 0, 18, 18), 18, 18);
            walkingSprite = walking;
            position = new Vector2(10, 10);
            drawPosition = new Vector2(position.X, position.Y);
        }

        public void update(TimeSpan timeSpan)
        {
            animationTimer -= (float)timeSpan.TotalMilliseconds;

            if (moving == MOVING_NONE)
            {
                if ((state & STATE_DIGGING) != 0)
                {
                    digging.setFrame(0);
                }
                else
                {
                    walking.setFrame(0);
                }
            }
            else
            {
                if (animationTimer < 0)
                {
                    animationTimer += walkTime;
                }

                if ((state & STATE_DIGGING) != 0)
                {
                    walkSpeed = 22;
                }
                else
                {
                    walkSpeed = 30;
                }

                switch (moving)
                {
                    case MOVING_LEFT:
                        if (position.X > 10)
                            position.X -= (float)timeSpan.TotalSeconds * walkSpeed;
                        break;
                    case MOVING_RIGHT:
                        if (position.X < DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_WIDTH - 0.5))
                            position.X += (float)timeSpan.TotalSeconds * walkSpeed;
                        break;
                    case MOVING_UP:
                        if (position.Y > 10)
                            position.Y -= (float)timeSpan.TotalSeconds * walkSpeed;
                        break;
                    case MOVING_DOWN:
                        if (position.Y < DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_HEIGHT - 0.5f))
                            position.Y += (float)timeSpan.TotalSeconds * walkSpeed;
                        break;
                }
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;

                int maxFrames = walkingSprite.getMaxFrames();
                float frameTime = walkTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;

                walkingSprite.setFrame(frameNumber);
            }
        }
        public void draw(SpriteBatch spritebatch)
        {
            walkingSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing);
        }

        void walk()
        {
            if (moving == MOVING_NONE)
            {
                animationTimer = walkTime;
            }
        }

        public void setDig(bool yes)
        {
            if (yes)
            {
                state |= STATE_DIGGING;
                walkingSprite = digging;
            }
            else
            {
                state &= ~STATE_DIGGING;
                walkingSprite = walking;
            }
        }

        internal void moveLeft()
        {
            int vert = getVert();
            if (vert == 0)
            {
                left();
            }
            else if (vert > 0)
            {
                up();
            }
            else
            {
                down();
            }
        }

        private int getVert()
        {
            int vertPosition = (int)position.Y;
            int remainder = vertPosition % DungeonScreen.GRID_SIZE;
            return remainder - (int)(DungeonScreen.GRID_SIZE * 0.5f);
        }
        private int getHorz()
        {
            int horzPosition = (int)position.X;
            int remainder = horzPosition % DungeonScreen.GRID_SIZE;
            return remainder - (int)(DungeonScreen.GRID_SIZE * 0.5f);
        }

        internal void moveRight()
        {
            int vert = getVert();
            if (vert == 0)
            {
                right();
            }
            else if (vert > 0)
            {
                up();
            }
            else
            {
                down();
            }
        }

        internal void moveUp()
        {
            int horz = getHorz();
            if (horz == 0)
            {
                up();
            }
            else if (horz > 0)
            {
                left();
            }
            else
            {
                right();
            }
        }

        internal void moveDown()
        {
            int horz = getHorz();
            if (horz == 0)
            {
                down();
            }
            else if (horz > 0)
            {
                left();
            }
            else
            {
                right();
            }
        }

        private void down()
        {
            walk();
            moving = MOVING_DOWN;
            if (vertFacing == Sprite.DIRECTION_NONE)
            {
                if (horzFacing == Sprite.DIRECTION_LEFT)
                {
                    horzFacing = Sprite.DIRECTION_RIGHT;
                }
                else
                {
                    horzFacing = Sprite.DIRECTION_LEFT;
                }
            }
            vertFacing = Sprite.DIRECTION_DOWN;
        }

        private void up()
        {
            walk();
            moving = MOVING_UP;
            vertFacing = Sprite.DIRECTION_UP;
        }

        private void right()
        {
            walk();
            moving = MOVING_RIGHT;
            vertFacing = Sprite.DIRECTION_NONE;
            horzFacing = Sprite.DIRECTION_RIGHT;
        }

        private void left()
        {
            walk();
            moving = MOVING_LEFT;
            vertFacing = Sprite.DIRECTION_NONE;
            horzFacing = Sprite.DIRECTION_LEFT;
        }

        internal void stopMoving()
        {
            moving = MOVING_NONE;
        }
    }
}