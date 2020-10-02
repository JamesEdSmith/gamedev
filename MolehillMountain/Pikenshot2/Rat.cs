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

        protected float sniffTime = 900;

        Tunnel tunnel;
        ArrayList clearDirections;
        static Random random = new Random();
        int intendingToMove;

        Sprite sniffing;
        Sprite mad;

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
            digTime = 325;
        }

        public Rat(DungeonScreen dungeonScreen, int x, int y) : this(dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_SCARED) != 0)
            {
                squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing);
            }
            else if ((state & STATE_SNIFFING) != 0)
            {
                sniffing.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing);
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
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
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
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE);
                    }
                }
            }
            else
            {
                base.draw(spritebatch);
            }
        }

        public override void update(TimeSpan timeSpan)
        {
            if ((state & STATE_SQUASHED) == 0)
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
                        int maxFrames = 16;
                        float frameTime = animationTime / (float)maxFrames;
                        int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                        sniffing.setFrame(frameNumber+4);
                    }
                    else
                    {
                        state &= ~STATE_SNIFFING;
                        if (dungeonScene.mole.moving != MOVING_NONE)
                        {
                            state |= STATE_MAD;
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

                        if ((state & STATE_MAD) == 0)
                        {
                            setDig(false);
                        }

                        if (targetDirection != MOVING_NONE)
                        {
                            intendingToMove = targetDirection;
                        }

                        clearDirections.Clear();

                        if ((state & STATE_MAD) != 0 || (tunnel.left == Tunnel.DUG || tunnel.left == Tunnel.HALF_DUG))
                            clearDirections.Add(LEFT_CLEAR);
                        if ((state & STATE_MAD) != 0 || (tunnel.right == Tunnel.DUG || tunnel.right == Tunnel.HALF_DUG))
                            clearDirections.Add(RIGHT_CLEAR);
                        if ((state & STATE_MAD) != 0 || (tunnel.top == Tunnel.DUG || tunnel.top == Tunnel.HALF_DUG))
                            clearDirections.Add(UP_CLEAR);
                        if ((state & STATE_MAD) != 0 || (tunnel.bottom == Tunnel.DUG || tunnel.bottom == Tunnel.HALF_DUG))
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
                        switch (intendingToMove)
                        {
                            case MOVING_DOWN: moveDown(); break;
                            case MOVING_LEFT: moveLeft(); break;
                            case MOVING_RIGHT: moveRight(); break;
                            case MOVING_UP: moveUp(); break;
                        }
                    }
                }
            }

            base.update(timeSpan);

            if (dungeonScene.vegetableFallingAbove(this) && (state & STATE_SQUASHED) == 0)
            {
                state = STATE_SCARED;
                int maxFrames = squashed.getMaxFrames() - 2;
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;

                squashed.setFrame(frameNumber);
            }
            else
            {
                state &= ~STATE_SCARED;
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
