using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Mothy : Rat
    {
        static Vector2 rightVector = new Vector2(DungeonScreen.GRID_SIZE, 0);
        static Vector2 leftVector = new Vector2(-DungeonScreen.GRID_SIZE, 0);
        static Vector2 upVector = new Vector2(0, -DungeonScreen.GRID_SIZE);
        static Vector2 downVector = new Vector2(0, DungeonScreen.GRID_SIZE);

        Sprite flapping;
        Sprite spooked;
        Sprite running;

        float spookTime;
        float runTime;
        float flapTime;

        private Vector2 molePosition;
        private Vector2 mothPosition;

        private List<Tunnel> zoomPath;


        public Mothy(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.MOTHY_RUN, new Rectangle(0, 0, 30, 20), 30, 20);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.BEEBLE_CRUSHED, new Rectangle(0, 0, 20, 20), 20, 20);
            digging = new Sprite(PikeAndShotGame.MOTHY_FLAP, new Rectangle(0, 0, 30, 20), 30, 20);
            flapping = new Sprite(PikeAndShotGame.MOTHY_FLAP, new Rectangle(0, 0, 30, 20), 30, 20);
            spooked = new Sprite(PikeAndShotGame.MOTHY_SPOOKED, new Rectangle(0, 0, 30, 20), 30, 20);
            running = new Sprite(PikeAndShotGame.MOTHY_RUN, new Rectangle(0, 0, 30, 20), 30, 20);

            walkTime = 570f;
            clearDirections = new ArrayList(4);
            str = 3;
            health = 1;
            digTime = 325;
            spookTime = 325;
            runTime = 275;
            flapTime = 700f;
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

        protected override void myLogic()
        {

            if (targetDirection != MOVING_NONE && (state & STATE_CHARGE) == 0 && (state & STATE_ZOOM) == 0 && (state & STATE_CRASH) == 0
                && sawMole)
            {
                molePosition = dungeonScene.mole.position;//dungeonScene.getCurrTunnel(dungeonScene.mole.position).position + Tunnel.center;
                mothPosition = position;
                zoomPath = dungeonScene.hasPath(dungeonScene.getCurrTunnel(mothPosition), dungeonScene.getCurrTunnel(molePosition));
                if (zoomPath != null)
                {
                    state |= STATE_CHARGE;
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
                if (zoomPath.Count > 0 && dungeonScene.getCurrTunnel(position) == zoomPath[0])
                {
                    zoomPath.RemoveAt(0);
                }
                if (zoomPath.Count > 0)
                {
                    targetDirection = dungeonScene.checkForTarget(zoomPath[0].position + Tunnel.center, position, false);
                    intendingToMove = targetDirection;
                    zoomTheTunnels();
                }
                else
                {
                    state &= ~STATE_ZOOM;
                    state |= STATE_CRASH;
                    animationTime = animationTimer = flapTime;
                }
                
            }
            else if ((state & STATE_CRASH) != 0)
            {
                int maxFrames = flapping.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                flapping.setFrame(frameNumber);
                if (animationTimer <= 0)
                {
                    state &= ~STATE_CRASH;
                    intendingToMove = MOVING_NONE;
                    tunnel = null;
                }
            }
            else
            {
                walkTheTunnels();
            }
        }

        private void zoomTheTunnels()
        {
            Tunnel newTunnel = dungeonScene.getCurrTunnel(position);

            if (Vector2.Distance(position, molePosition) <= DungeonScreen.GRID_SIZE)
            {
                state &= ~STATE_ZOOM;
                state |= STATE_CRASH;
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
            else
            {
                base.draw(spritebatch);
            }
        }
    }
}

