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
        const float WALK_SPEED = 38f;
        const float DIG_SPEED = 30f;

        public const int MOVING_NONE = 0;
        public const int MOVING_LEFT = 1;
        public const int MOVING_RIGHT = 2;
        public const int MOVING_UP = 3;
        public const int MOVING_DOWN = 4;

        public const int STATE_DIGGING = 1;
        public const int STATE_NUDGING = 2;
        public const int STATE_SQUASHED = 4;
        public const int STATE_SNIFFING = 8;
        public const int STATE_SCARED = 16;
        public const int STATE_MAD = 32;

        private const float MOLE_NUDGE_SPACING = 7;

        protected float animationTimer;
        public float walkSpeed = WALK_SPEED;
        protected float walkTime = 325f;
        protected float digTime = 650;
        protected float animationTime;

        protected Sprite walking;
        protected Sprite digging;
        protected Sprite nudging;
        protected Sprite squashed;

        protected Sprite walkingSprite;
        public Vector2 position;
        protected Vector2 drawPosition;
        //flags
        public int state = 0;
        //notflags
        public int moving = 0;
        public int horzFacing = Sprite.DIRECTION_LEFT;
        public int vertFacing = Sprite.DIRECTION_NONE;

        protected DungeonScreen dungeonScene;
        protected Vegetable vegetable;
        public Tunnel diggingTunnel;
        public float nudgeMovement;

        public int str;
        public int per;
        internal int prevMoleRight;
        internal int prevMoleLeft;
        internal int prevMoleDown;
        internal int prevMoleUp;

        public Mole(DungeonScreen dungeonScene)
        {
            this.dungeonScene = dungeonScene;
            walking = new Sprite(PikeAndShotGame.MOLE_MINER_WALKING, new Rectangle(0, 0, 18, 18), 18, 18);
            digging = new Sprite(PikeAndShotGame.MOLE_MINER_DIGGING, new Rectangle(0, 0, 18, 18), 18, 18);
            nudging = new Sprite(PikeAndShotGame.MOLE_MINER_NUDGE, new Rectangle(0, 0, 18, 18), 18, 18);
            squashed = new Sprite(PikeAndShotGame.MOLE_SQUASHED, new Rectangle(0, 0, 18, 18), 18, 18);
            squashed.setFrame(squashed.getMaxFrames() - 2);
            walkingSprite = walking;
            animationTime = walkTime;
            position = new Vector2(10, 10);
            drawPosition = new Vector2(position.X, position.Y);
            str = 3;
            per = 3;
        }

        public Mole(float x, float y, DungeonScreen dungeonScene) : this(dungeonScene)
        {
            position = new Vector2(x, y);
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Mole(int x, int y, DungeonScreen dungeonScreen) : this(0f, 0f, dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public virtual void update(TimeSpan timeSpan)
        {

            animationTimer -= (float)timeSpan.TotalMilliseconds;

            if ((state & STATE_SQUASHED) != 0)
            {
                if (position.Y - vegetable.position.Y <= vegetable.position.Y + DungeonScreen.GRID_SIZE / 4)
                {
                    position.Y = vegetable.position.Y + DungeonScreen.GRID_SIZE / 4;
                    drawPosition.X = (int)position.X;
                    drawPosition.Y = (int)position.Y;
                }
            }
            else if (moving == MOVING_NONE)
            {
                if ((state & STATE_DIGGING) != 0)
                {
                    digging.setFrame(0);
                }
                else
                {
                    walking.setFrame(0);
                }
                position.X += nudgeMovement;
                nudgeMovement = 0;
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;

            }
            else
            {
                if (animationTimer < 0)
                {
                    animationTimer += animationTime;
                }

                if ((state & STATE_DIGGING) != 0)
                {
                    animationTime = digTime;
                    walkSpeed = DIG_SPEED;
                }
                else
                {
                    animationTime = walkTime;
                    walkSpeed = WALK_SPEED;
                }

                switch (moving)
                {
                    case MOVING_LEFT:
                        if (position.X > 10)
                        {
                            if (!dungeonScene.vegetableLeft(position, new ArrayList { this }, MOLE_NUDGE_SPACING))
                            {
                                state &= ~STATE_NUDGING;
                                position.X -= (float)timeSpan.TotalSeconds * walkSpeed;
                            }
                            else
                            {
                                state |= STATE_NUDGING;
                                position.X += nudgeMovement;
                                animationTime = walkTime;
                                walkingSprite = nudging;
                                nudgeMovement = 0;
                                //Console.WriteLine("nudgeMovement: " + nudgeMovement);
                            }
                        }
                        break;
                    case MOVING_RIGHT:
                        if (position.X < DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_WIDTH - 0.5))
                        {
                            if (!dungeonScene.vegetableRight(position, new ArrayList { this }, MOLE_NUDGE_SPACING))
                            {
                                state &= ~STATE_NUDGING;
                                position.X += (float)timeSpan.TotalSeconds * walkSpeed;
                            }
                            else
                            {
                                state |= STATE_NUDGING;
                                position.X += nudgeMovement;
                                animationTime = walkTime;
                                walkingSprite = nudging;
                                nudgeMovement = 0;
                                //Console.WriteLine("nudgeMovement: " + nudgeMovement);
                            }
                        }
                        break;
                    case MOVING_UP:
                        if (position.Y > 10 && !dungeonScene.vegetableAbove(this))
                        {
                            position.Y -= (float)timeSpan.TotalSeconds * walkSpeed;
                            state &= ~STATE_NUDGING;
                        }
                        break;
                    case MOVING_DOWN:
                        if (position.Y < DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_HEIGHT - 0.5f) && !dungeonScene.vegetableBelow(this))
                        {
                            state &= ~STATE_NUDGING;
                            position.Y += (float)timeSpan.TotalSeconds * walkSpeed;
                        }
                        break;
                }
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;

                int maxFrames = walkingSprite.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;

                walkingSprite.setFrame(frameNumber);

                nudgeMovement = 0;
            }
        }
        public virtual void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_SQUASHED) == 0)
            {
                walkingSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing);
            }
            else
            {
                if (vegetable != null && (vegetable.state == Vegetable.SPLITTING || vegetable.state == Vegetable.DEAD))
                {
                    squashed.setFrame(squashed.getMaxFrames() - 1);
                }
                else
                {
                    squashed.setFrame(squashed.getMaxFrames() - 2);
                }

                if (horzFacing == Sprite.DIRECTION_LEFT)
                {
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE);
                    }
                }
                else
                {
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
                    }
                }
            }
        }

        void walk()
        {
            if (moving == MOVING_NONE)
            {
                animationTimer = animationTime;
            }
        }

        public void setDig(bool yes)
        {
            if ((state & STATE_NUDGING) == 0 && (state & STATE_SNIFFING) == 0)
            {
                if (yes)
                {
                    state |= STATE_DIGGING;
                    walkingSprite = digging;
                    animationTime = digTime;
                }
                else
                {
                    state &= ~STATE_DIGGING;
                    walkingSprite = walking;
                    animationTime = walkTime;
                    if (animationTimer > animationTime)
                    {
                        animationTimer -= animationTime;
                    }
                }
            }
        }

        public virtual void moveLeft()
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

        public virtual void moveRight()
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

        public virtual void squash(Vegetable vegetable)
        {
            state = STATE_SQUASHED;
            this.vegetable = vegetable;
        }

        public virtual void moveUp()
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

        public virtual void moveDown()
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
            state &= ~STATE_NUDGING;
        }

        internal bool alive()
        {
            return (state & STATE_SQUASHED) == 0;
        }

        internal float getDigSpeed()
        {
            return DIG_SPEED;
        }
    }
}