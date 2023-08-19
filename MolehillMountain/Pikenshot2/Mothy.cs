using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    public class Mothy : Rat
    {
        static Vector2 rightVector = new Vector2(DungeonScreen.GRID_SIZE, 0);
        static Vector2 leftVector = new Vector2(-DungeonScreen.GRID_SIZE, 0);
        static Vector2 upVector = new Vector2(0, -DungeonScreen.GRID_SIZE);
        static Vector2 downVector = new Vector2(0, DungeonScreen.GRID_SIZE);

        Sprite flapping;
        Sprite spooked;
        Sprite running;
        Sprite blinking;
        Sprite stepping;

        float spookTime;
        float runTime;
        float flapTime;
        float idleTime;

        float idleInterval = 3000f;
        float idleTimer;

        bool waitToWind;
        private int windDirection;

        public Mothy(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.MOTHY_WALK, new Rectangle(0, 0, 30, 20), 30, 20);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.BEEBLE_CRUSHED, new Rectangle(0, 0, 20, 20), 20, 20);
            digging = new Sprite(PikeAndShotGame.MOTHY_FLAP, new Rectangle(0, 0, 30, 20), 30, 20);
            flapping = new Sprite(PikeAndShotGame.MOTHY_FLAP, new Rectangle(0, 0, 30, 20), 30, 20);
            spooked = new Sprite(PikeAndShotGame.MOTHY_SPOOKED, new Rectangle(0, 0, 30, 20), 30, 20);
            running = new Sprite(PikeAndShotGame.MOTHY_RUN, new Rectangle(0, 0, 30, 20), 30, 20);
            blinking = new Sprite(PikeAndShotGame.MOTHY_BLINK, new Rectangle(0, 0, 30, 20), 30, 20);
            stepping = new Sprite(PikeAndShotGame.MOTHY_STEP, new Rectangle(0, 0, 30, 20), 30, 20);

            walkTime = 1000f;
            currWalkSpeed = walkSpeed = 7f;
            clearDirections = new ArrayList(4);
            str = 3;
            health = 1;
            digTime = 325;
            spookTime = 400;
            runTime = 275;
            flapTime = 700f;
            idleTime = 3000f;

            idleTimer = idleInterval + PikeAndShotGame.random.Next(0, (int)idleInterval);
        }

        public Mothy(DungeonScreen dungeonScreen, int x, int y) : this(dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Mothy(DungeonScreen dungeonScreen, float x, float y) : this(dungeonScreen)
        {
            position.X = x;
            position.Y = y;
            drawPosition = new Vector2(position.X, position.Y);
            if (dungeonScene.checkMoleSight(dungeonScene.getCurrTunnel(position)) != SeenStatus.SEEN)
                walkingSprite = unseen;
        }

        protected override void myLogic(TimeSpan timeSpan)
        {

            if (targetDirection != MOVING_NONE && (state & STATE_CHARGE) == 0 && (state & STATE_ZOOM) == 0 && (state & STATE_CRASH) == 0
                && sawMole)
            {
                molePosition = dungeonScene.mole.position;
                molePath = dungeonScene.hasPath(dungeonScene.getCurrTunnel(position), dungeonScene.getCurrTunnel(molePosition));
                if (molePath != null)
                {
                    state |= STATE_CHARGE;
                    idleTimer = idleInterval + PikeAndShotGame.random.Next(0, (int)idleInterval);
                    animationTime = animationTimer = spookTime;
                    intendingToMove = targetDirection;

                    if (targetDirection == MOVING_RIGHT)
                    {
                        vertFacing = Sprite.DIRECTION_NONE;
                        horzFacing = Sprite.DIRECTION_RIGHT;
                    }
                    else if (targetDirection == MOVING_LEFT)
                    {
                        moving = MOVING_LEFT;
                        vertFacing = Sprite.DIRECTION_NONE;
                        horzFacing = Sprite.DIRECTION_LEFT;
                    }
                    else if (targetDirection == MOVING_UP)
                    {
                        moving = MOVING_UP;
                        vertFacing = Sprite.DIRECTION_UP;
                    }
                    else
                    {
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

                    stopMoving();
                }
            }
            else if ((state & STATE_CHARGE) != 0)
            {
                if (animationTimer >= 0)
                {
                    int maxFrames = spooked.getMaxFrames();
                    float frameTime = animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    spooked.setFrame(frameNumber);
                }
                else
                {
                    state &= ~STATE_CHARGE;
                    state |= STATE_ZOOM;
                    animationTime = animationTimer = runTime;
                }
            }
            else if ((state & STATE_ZOOM) != 0)
            {
                int maxFrames = running.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                running.setFrame(frameNumber);
                if (animationTimer <= 0)
                {
                    animationTimer += runTime;
                }
                if (molePath.Count > 0 && dungeonScene.getCurrTunnel(position) == molePath[0])
                {
                    molePath.RemoveAt(0);

                }
                if (molePath.Count > 0)
                {
                    targetDirection = dungeonScene.checkForTarget(molePath[0].position + Tunnel.center, position, false);
                    intendingToMove = targetDirection;
                    zoomTheTunnels();
                }
                else
                {
                    state &= ~STATE_ZOOM;
                    state &= ~STATE_SNIFFING;
                    state |= STATE_CRASH;
                    waitToWind = true;
                    windDirection = intendingToMove;
                    animationTime = animationTimer = flapTime;
                }

            }
            else if ((state & STATE_CRASH) != 0)
            {
                int maxFrames = flapping.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                flapping.setFrame(frameNumber);
                if (frameNumber >= 3 && waitToWind)
                {
                    waitToWind = false;
                    dungeonScene.wind(windDirection != Mole.MOVING_NONE ? windDirection : intendingToMove, position, 4, this);
                }
                if (animationTimer <= 0)
                {
                    state &= ~STATE_CRASH;
                    intendingToMove = MOVING_NONE;
                    tunnel = null;
                }
            }
            else if ((state & STATE_SNIFFING) != 0)
            {
                if (animationTimer >= idleTime / 2f)
                {
                    int maxFrames = blinking.getMaxFrames();
                    float frameTime = animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)((animationTimer - animationTime) / frameTime) - 1;
                    blinking.setFrame(frameNumber);

                }
                else
                {
                    int maxFrames = stepping.getMaxFrames();
                    float frameTime = animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    stepping.setFrame(frameNumber);
                    if (animationTimer <= 0)
                    {
                        state &= ~STATE_SNIFFING;
                        idleTimer = idleInterval + PikeAndShotGame.random.Next(0, (int)idleInterval);
                    }
                }
            }
            else
            {
                if (idleTimer >= 0)
                {
                    walkTheTunnels();
                    idleTimer -= (float)timeSpan.TotalMilliseconds;
                }
                else
                {
                    state |= STATE_SNIFFING;
                    animationTimer = idleTime;
                    animationTime = idleTime / 2f;
                    stopMoving();
                }
            }
        }

        private void zoomTheTunnels()
        {
            Tunnel newTunnel = dungeonScene.getCurrTunnel(position);

            if (Vector2.Distance(position, molePosition) <= DungeonScreen.GRID_SIZE)
            {
                state &= ~STATE_ZOOM;
                state |= STATE_CRASH;
                waitToWind = true;
                animationTime = animationTimer = flapTime;
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
                    state &= ~STATE_ZOOM;
                    state |= STATE_CRASH;
                    animationTime = animationTimer = flapTime;
                }

            }
        }

        public override void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_CHARGE) != 0)
            {
                spooked.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_ZOOM) != 0)
            {
                running.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_CRASH) != 0)
            {
                flapping.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_SNIFFING) != 0)
            {
                if (animationTimer >= idleTime / 2f)
                    blinking.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
                else
                    stepping.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }

            else
            {
                base.draw(spritebatch);
            }
        }
    }
}

