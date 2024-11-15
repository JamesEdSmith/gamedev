﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    public class Rat : Mole
    {
        protected const int DOWN_CLEAR = 1;
        protected const int LEFT_CLEAR = 2;
        protected const int UP_CLEAR = 3;
        protected const int RIGHT_CLEAR = 4;

        protected const float SQUASHED_TIME = 3500;

        protected float sniffTime = 1000;
        protected float sniffInterval = 5000;
        protected float madTimer = 0;
        float getMadAnimationTime = 750f;
        protected float getMadTimer = 6000;
        public float squashedTimer = SQUASHED_TIME;
        protected const float MAD_TIME = 2000;

        public Tunnel tunnel;
        protected ArrayList clearDirections;
        protected static Random random = new Random();
        protected int intendingToMove;

        Sprite sniffing;
        Sprite gettingMad;
        protected Vector2 molePosition;
        protected List<Tunnel> molePath;

        protected bool sawMole;
        private float sniffTimer;

        public Rat(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.RAT_WALKING, new Rectangle(0, 0, 22, 18), 22, 18);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.RAT_CRUSHED, new Rectangle(0, 0, 22, 18), 22, 18);
            digging = new Sprite(PikeAndShotGame.RAT_DIGGING, new Rectangle(0, 0, 22, 18), 22, 18);
            sniffing = new Sprite(PikeAndShotGame.RAT_SNIFF, new Rectangle(0, 0, 20, 18), 20, 18);
            mad = new Sprite(PikeAndShotGame.RAT_MAD, new Rectangle(0, 0, 20, 18), 20, 18);
            gettingMad = new Sprite(PikeAndShotGame.RAT_MAD, new Rectangle(0, 0, 20, 18), 20, 18);
            dizzy = new Sprite(PikeAndShotGame.RAT_DIZZY, new Rectangle(0, 0, 20, 20), 20, 20);
            clearDirections = new ArrayList(4);
            str = 3;
            health = 2;
            digTime = 325;
            sniffTimer = 0;
            molePath = new List<Tunnel>();
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
            else if ((state & STATE_GETMAD) != 0)
            {
                if((state & STATE_DIGGING) == 0)
                    gettingMad.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
                else
                    walkingSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_SCARED) != 0 || ((state & STATE_MAD) != 0 && (state & STATE_DIGGING) == 0))
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
        protected int targetDirection;

        public override void update(GameTime gameTime)
        {
            TimeSpan timeSpan = gameTime.ElapsedGameTime;
            if ((state & STATE_WHACKED) == 0)
            {
                seen = dungeonScene.checkMoleSight(tunnel != null ? tunnel : dungeonScene.getCurrTunnel(position));
                if (seen != SeenStatus.SEEN)
                {
                    walkingSprite = unseen;
                }
            }
            if ((state & STATE_SQUASHED) != 0 && squashedTimer >= 0)
            {
                squashedTimer -= (float)timeSpan.TotalMilliseconds;
                if (squashedTimer <= SQUASHED_TIME / 3f)
                {
                    dimColor.A = (byte)(255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 10f));
                }
            }
            else
            {
                dimColor = SeenStatus.getVisibilityColor(seen);
            }

            if ((state & STATE_SQUASHED) == 0 && (state & STATE_SCARED) == 0 && (state & STATE_FIGHTING) == 0 && (state & STATE_HIT) == 0 && (state & STATE_WHACKED) == 0)
            {
                targetDirection = dungeonScene.checkForTarget(dungeonScene.mole, this, (state & STATE_MAD) != 0);

                if (targetDirection == Mole.MOVING_NONE)
                    sawMole = false;
                else
                    sawMole = true;

                myLogic(timeSpan);
            }
            else
            {
                sawMole = false;
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
            }else if ((state & STATE_MAD) != 0)
            {
                int maxFrames = squashed.getMaxFrames() - 2;
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
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

        protected void walkTheTunnels()
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

        protected virtual void myLogic(TimeSpan timeSpan)
        {

            if ((state & STATE_SNIFFING) == 0 && (state & STATE_SCARED) == 0
                && (state & STATE_NUDGING) == 0 && (state & STATE_MAD) == 0
                && (state & STATE_GETMAD) == 0 && (state & STATE_HIT) == 0
                && (state & STATE_WASHED) == 0 && (state & STATE_WHACKED) == 0
                && sawMole == false
                && sniffTimer <= 0)
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
                    sniffTimer = sniffInterval;
                    state &= ~STATE_SNIFFING;
                    molePosition = dungeonScene.mole.position;
                    molePath = dungeonScene.hasPath(dungeonScene.getCurrTunnel(position), dungeonScene.getCurrTunnel(molePosition));
                }
            }
            else if ((state & STATE_GETMAD) != 0)
            {
                if (animationTimer >= 0)
                {
                    int maxFrames = gettingMad.getMaxFrames();
                    float frameTime = animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    gettingMad.setFrame(frameNumber);
                }
                else
                {
                    state &= ~STATE_GETMAD;
                    state |= STATE_MAD;
                }
            }
            else
            {

                if (molePath != null && molePath.Count > 0 && dungeonScene.getCurrTunnel(position) == molePath[0])
                {
                    molePath.RemoveAt(0);
                }
                if (molePath != null && molePath.Count > 0)
                {
                    targetDirection = dungeonScene.checkForTarget(molePath[0].position + Tunnel.center, position, false);
                    intendingToMove = targetDirection;
                }
                walkTheTunnels();
                sniffTimer -= (float)timeSpan.TotalMilliseconds;
                if (madTimer > 0)
                {
                    madTimer -= (float)timeSpan.TotalMilliseconds;
                    if (madTimer <= 0 && (state & STATE_MAD) != 0)
                    {
                        state &= ~STATE_MAD;
                        getMadTimer = 6000f + PikeAndShotGame.random.Next(0, 6000);
                    }
                }
                else if (getMadTimer > 0)
                {
                    getMadTimer -= (float)timeSpan.TotalMilliseconds;
                    if (getMadTimer <= 0)
                    {
                        state |= STATE_GETMAD;
                        animationTimer = animationTime = getMadAnimationTime;
                        stopMoving();
                        molePath = null;
                        madTimer = MAD_TIME + PikeAndShotGame.random.Next(0, (int)MAD_TIME);
                    }
                }

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

        internal void whack(Water water)
        {
            state = STATE_WHACKED;

            animationTime = animationTimer = MAD_TIME;

            if(water.state == Water.MOVING_LEFT)
            {
                whackedMovement = new Vector2 (-24, -100);
            }else
            {
                whackedMovement = new Vector2(24, -100);
            }
        }
    }
}
