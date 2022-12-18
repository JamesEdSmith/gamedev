using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Rat : Mole
    {
        const int DOWN_CLEAR = 1;
        const int LEFT_CLEAR = 2;
        const int UP_CLEAR = 3;
        const int RIGHT_CLEAR = 4;

        const float SQUASHED_TIME = 3500;

        protected float sniffTime = 900;
        protected float madTimer = 0;
        public float squashedTimer = SQUASHED_TIME;
        protected const float MAD_TIME = 1500;
        protected const float MAD_RESET_TIME = 2000;

        public Tunnel tunnel;
        ArrayList clearDirections;
        static Random random = new Random();
        int intendingToMove;

        Sprite sniffing;

        public Rat(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.RAT_WALKING, new Rectangle(0, 0, 22, 18), 22, 18);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.RAT_CRUSHED, new Rectangle(0, 0, 22, 18), 22, 18);
            digging = new Sprite(PikeAndShotGame.RAT_DIGGING, new Rectangle(0, 0, 22, 18), 22, 18);
            sniffing = new Sprite(PikeAndShotGame.RAT_SNIFF, new Rectangle(0, 0, 20, 18), 20, 18);
            mad = new Sprite(PikeAndShotGame.RAT_MAD, new Rectangle(0, 0, 20, 18), 20, 18);
            clearDirections = new ArrayList(4);
            str = 3;
            health = 1;
            digTime = 325;
        }

        public Rat(DungeonScreen dungeonScreen, int x, int y) : this(dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Rat(DungeonScreen dungeonScreen, float x, float y) : this(dungeonScreen)
        {
            position.X = x;
            position.Y = y;
            drawPosition = new Vector2(position.X, position.Y);
            if (dungeonScene.checkMoleSight(dungeonScene.getCurrTunnel(position)) != SeenStatus.SEEN)
                walkingSprite = unseen;
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_HIT) != 0)
            {
                mad.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_SCARED) != 0)
            {
                squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_SNIFFING) != 0)
            {
                sniffing.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_SQUASHED) != 0)
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
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE, dimColor);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE, dimColor);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE, dimColor);
                    }
                }
                else
                {
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE, dimColor);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, dimColor);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, dimColor);
                    }
                }
            }
            else
            {
                base.draw(spritebatch);
            }
        }

        public int seen;

        public override void update(GameTime gameTime)
        {
            TimeSpan timeSpan = gameTime.ElapsedGameTime;

            seen = dungeonScene.checkMoleSight(tunnel != null ? tunnel : dungeonScene.getCurrTunnel(position));
            if (seen != SeenStatus.SEEN)
            {
                walkingSprite = unseen;
            }
            else
            {
                if((state & STATE_DIGGING) == 0)
                    walkingSprite = walking;
                else
                    walkingSprite = digging;
            }

            if (madTimer > 0)
            {
                madTimer -= (float)timeSpan.TotalMilliseconds;
                if (madTimer <= 0 && (state & STATE_MAD) != 0)
                {
                    state &= ~STATE_MAD;
                    madTimer = MAD_RESET_TIME;
                }
            }

            if ((state & STATE_SQUASHED) != 0 && squashedTimer >= 0)
            {
                squashedTimer -= (float)timeSpan.TotalMilliseconds;
                if (squashedTimer <= SQUASHED_TIME / 3f)
                {
                    dimColor.A = (byte)(255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 10f));
                }
            }else
            {
                dimColor = SeenStatus.getVisibilityColor(seen);
            }

            if ((state & STATE_SQUASHED) == 0 && (state & STATE_SCARED) == 0)
            {
                int targetDirection = dungeonScene.checkForTarget(dungeonScene.mole, this, (state & STATE_MAD) != 0);
                if (targetDirection == MOVING_NONE && (state & STATE_SNIFFING) == 0 && (state & STATE_SCARED) == 0 && (state & STATE_NUDGING) == 0 && (state & STATE_MAD) == 0
                    && (dungeonScene.mole.position - position).Length() <= DungeonScreen.GRID_SIZE * 1.5f && dungeonScene.mole.moving != MOVING_NONE)
                {
                    state |= STATE_SNIFFING;
                    animationTime = animationTimer = sniffTime;
                    stopMoving();
                }
                else if ((state & STATE_SNIFFING) != 0)
                {
                    if (animationTimer >= 0)
                    {
                        //magic numbers cause the animation was too long
                        int maxFrames = 16;
                        float frameTime = animationTime / (float)maxFrames;
                        int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                        sniffing.setFrame(frameNumber + 4);
                    }
                    else
                    {
                        state &= ~STATE_SNIFFING;
                        if (dungeonScene.mole.moving != MOVING_NONE)
                        {
                            state |= STATE_MAD;
                            madTimer = MAD_TIME;
                            tunnel = null;
                        }
                    }
                }
                else
                {
                    Tunnel newTunnel = dungeonScene.getCurrTunnel(position);

                    if (tunnel == null || newTunnel != tunnel)
                    {
                        tunnel = newTunnel;

                        if (targetDirection != MOVING_NONE)
                        {
                            intendingToMove = targetDirection;
                        }

                        clearDirections.Clear();

                        if (dungeonScene.vegetableLeftClear(this) && ((state & STATE_MAD) != 0 || (tunnel.left == Tunnel.DUG || tunnel.left == Tunnel.HALF_DUG)))
                            clearDirections.Add(LEFT_CLEAR);
                        if (dungeonScene.vegetableRightClear(this) && ((state & STATE_MAD) != 0 || (tunnel.right == Tunnel.DUG || tunnel.right == Tunnel.HALF_DUG)))
                            clearDirections.Add(RIGHT_CLEAR);
                        if ((state & STATE_MAD) != 0 || (tunnel.top == Tunnel.DUG || tunnel.top == Tunnel.HALF_DUG))
                            clearDirections.Add(UP_CLEAR);
                        if (!dungeonScene.vegetableDirectlyBelow(this) && ((state & STATE_MAD) != 0 || (tunnel.bottom == Tunnel.DUG || tunnel.bottom == Tunnel.HALF_DUG)))
                            clearDirections.Add(DOWN_CLEAR);

                        if (intendingToMove == MOVING_LEFT && clearDirections.Contains(LEFT_CLEAR))
                        {
                            moveLeft();
                        }
                        else if (intendingToMove == MOVING_RIGHT && clearDirections.Contains(RIGHT_CLEAR))
                        {
                            moveRight();
                        }
                        else if (intendingToMove == MOVING_DOWN && clearDirections.Contains(DOWN_CLEAR))
                        {
                            moveDown();
                        }
                        else if (intendingToMove == MOVING_UP && clearDirections.Contains(UP_CLEAR))
                        {
                            moveUp();
                        }
                        else
                        {
                            if (clearDirections.Count > 1)
                            {
                                switch (intendingToMove)
                                {
                                    case MOVING_DOWN: clearDirections.Remove(UP_CLEAR); break;
                                    case MOVING_LEFT: clearDirections.Remove(RIGHT_CLEAR); break;
                                    case MOVING_RIGHT: clearDirections.Remove(LEFT_CLEAR); break;
                                    case MOVING_UP: clearDirections.Remove(DOWN_CLEAR); break;
                                }
                            }
                            int choice = random.Next(clearDirections.Count);
                            if (clearDirections.Count < 1)
                            {
                                // handle state here
                            }
                            else
                            {
                                switch ((int)clearDirections[choice])
                                {
                                    case LEFT_CLEAR: moveLeft(); break;
                                    case RIGHT_CLEAR: moveRight(); break;
                                    case UP_CLEAR: moveUp(); break;
                                    case DOWN_CLEAR: moveDown(); break;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (intendingToMove != MOVING_DOWN || !dungeonScene.vegetableDirectlyBelow(this))
                        {
                            switch (intendingToMove)
                            {
                                case MOVING_DOWN: moveDown(); break;
                                case MOVING_LEFT: moveLeft(); break;
                                case MOVING_RIGHT: moveRight(); break;
                                case MOVING_UP: moveUp(); break;
                            }
                        }
                        else
                        {
                            tunnel = null;
                        }
                    }
                }
            }

            base.update(gameTime);

            if (dungeonScene.vegetableFallingAbove(this) && (state & STATE_SQUASHED) == 0)
            {
                state = STATE_SCARED;
                int maxFrames = squashed.getMaxFrames() - 2;
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                moving = MOVING_NONE;
                squashed.setFrame(frameNumber);
            }
            else
            {
                state &= ~STATE_SCARED;
            }

            if ((state & STATE_NUDGING) != 0)
            {
                tunnel = null;
            }
        }

        public override void squash(Vegetable vegetable)
        {
            base.squash(vegetable);
            state &= ~STATE_SCARED;
        }

        public override void moveLeft()
        {
            intendingToMove = MOVING_LEFT;
            base.moveLeft();
        }

        public override void moveRight()
        {
            intendingToMove = MOVING_RIGHT;
            base.moveRight();
        }
        public override void moveUp()
        {
            intendingToMove = MOVING_UP;
            base.moveUp();
        }
        public override void moveDown()
        {
            intendingToMove = MOVING_DOWN;
            base.moveDown();
        }
    }
}
