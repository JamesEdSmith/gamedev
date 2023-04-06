using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Salamando : Rat
    {
        private Sprite charging;
        float chargeTime = 2000f;
        private bool waitToFire;
        int fireDirection;

        public Salamando(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.SALAMANDO_WALKING, new Rectangle(0, 0, 20, 20), 20, 20);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.BEEBLE_CRUSHED, new Rectangle(0, 0, 20, 20), 20, 20);
            digging = new Sprite(PikeAndShotGame.BEEBLE_ZOOM, new Rectangle(0, 0, 20, 20), 20, 20);
            charging = new Sprite(PikeAndShotGame.SALAMANDO_SPITTING, new Rectangle(0, 0, 20, 20), 20, 20);

            clearDirections = new ArrayList(4);
            str = 3;
            health = 1;
            digTime = 325;
        }

        public Salamando(DungeonScreen dungeonScreen, int x, int y) : this(dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Salamando(DungeonScreen dungeonScreen, float x, float y) : this(dungeonScreen)
        {
            position.X = x;
            position.Y = y;
            drawPosition = new Vector2(position.X, position.Y);
            if (dungeonScene.checkMoleSight(dungeonScene.getCurrTunnel(position)) != SeenStatus.SEEN)
                walkingSprite = unseen;
        }

        protected override void myLogic()
        {
            float yDiff = Math.Abs(dungeonScene.mole.position.Y - position.Y);
            float xDiff = Math.Abs(dungeonScene.mole.position.X - position.X);

            if (targetDirection != MOVING_NONE && (state & STATE_CHARGE) == 0 && (state & STATE_ZOOM) == 0 && (state & STATE_CRASH) == 0
                && (((targetDirection == MOVING_LEFT || targetDirection == MOVING_RIGHT) 
                && yDiff < 1 && xDiff < DungeonScreen.GRID_SIZE * 4) 
                || ((targetDirection == MOVING_DOWN || targetDirection == MOVING_UP) 
                && xDiff < 1 && yDiff < DungeonScreen.GRID_SIZE * 4)))
            {
                state |= STATE_CHARGE;
                animationTime = animationTimer = chargeTime;

                intendingToMove = targetDirection;
                waitToFire = true;
                fireDirection = targetDirection;

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
            else if ((state & STATE_CHARGE) != 0)
            {
                if (animationTimer >= 0)
                {
                    int maxFrames = charging.getMaxFrames();
                    float frameTime = animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    charging.setFrame(frameNumber);
                    if(waitToFire && frameNumber >= 18)
                    {
                        waitToFire = false;
                        dungeonScene.fire(fireDirection, position, 4);
                    }
                }
                else
                {
                    state &= ~STATE_CHARGE;
                    animationTime = animationTimer = walkTime;
                }
            }
            else
            {
                walkTheTunnels();
            }
        }
        public override void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_CHARGE) != 0)
            {
                charging.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else
            {
                base.draw(spritebatch);
            }
        }
    }
}
