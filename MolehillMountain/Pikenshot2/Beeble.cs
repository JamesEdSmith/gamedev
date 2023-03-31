using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Beeble : Rat
    {
        static Vector2 rightVector = new Vector2(DungeonScreen.GRID_SIZE, 0);
        static Vector2 leftVector = new Vector2(-DungeonScreen.GRID_SIZE, 0);
        static Vector2 upVector = new Vector2(0, -DungeonScreen.GRID_SIZE);
        static Vector2 downVector = new Vector2(0, DungeonScreen.GRID_SIZE);

        Sprite crashing;
        Sprite charging;
        Sprite zooming;
        
        float chargeTime;
        float zoomTime;
        float crashTime;
        

        public Beeble(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.BEEBLE_WALKING, new Rectangle(0, 0, 20, 20), 20, 20);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.RAT_CRUSHED, new Rectangle(0, 0, 22, 18), 22, 18);
            digging = new Sprite(PikeAndShotGame.BEEBLE_ZOOM, new Rectangle(0, 0, 20, 20), 20, 20);
            crashing = new Sprite(PikeAndShotGame.BEEBLE_CRASH, new Rectangle(0, 0, 20, 20), 20, 20);
            charging = new Sprite(PikeAndShotGame.BEEBLE_CHARGE, new Rectangle(0, 0, 20, 20), 20, 20);
            zooming = new Sprite(PikeAndShotGame.BEEBLE_ZOOM, new Rectangle(0, 0, 20, 20), 20, 20);
            
            walkTime = 570f;
            clearDirections = new ArrayList(4);
            str = 3;
            health = 1;
            digTime = 325;
            chargeTime = 750;
            zoomTime = 325;
            crashTime = 700f;
        }

        public Beeble(DungeonScreen dungeonScreen, int x, int y) : this(dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Beeble(DungeonScreen dungeonScreen, float x, float y) : this(dungeonScreen)
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
                && (((targetDirection == MOVING_LEFT || targetDirection == MOVING_RIGHT) && yDiff < 1) || ((targetDirection == MOVING_DOWN || targetDirection == MOVING_UP) && xDiff < 5)))
            {
                state |= STATE_CHARGE;
                animationTime = animationTimer = chargeTime;

                intendingToMove = targetDirection;

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
                }
                else
                {
                    state &= ~STATE_CHARGE;
                    state |= STATE_ZOOM;
                    animationTime = animationTimer = zoomTime;
                }
            }
            else if ((state & STATE_ZOOM) != 0)
            {
                int maxFrames = zooming.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                zooming.setFrame(frameNumber);
                if (animationTimer <= 0)
                {
                    animationTimer += zoomTime;
                }
                zoomTheTunnels();
            }
            else if ((state & STATE_CRASH) != 0)
            {
                int maxFrames = crashing.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                crashing.setFrame(frameNumber);
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

            if ((tunnel == null || newTunnel != tunnel)
                && Math.Abs((int)position.X % DungeonScreen.GRID_SIZE - DungeonScreen.GRID_SIZE / 2) < 2
                && Math.Abs((int)position.Y % DungeonScreen.GRID_SIZE - DungeonScreen.GRID_SIZE / 2) < 2)
            {
                tunnel = newTunnel;

                Tunnel leftTunnel = dungeonScene.getCurrTunnel(position + leftVector);
                Tunnel rightTunnel = dungeonScene.getCurrTunnel(position + rightVector);
                Tunnel upTunnel = dungeonScene.getCurrTunnel(position + upVector);
                Tunnel downTunnel = dungeonScene.getCurrTunnel(position + downVector);

                clearDirections.Clear();

                if (leftTunnel != null && dungeonScene.vegetableLeftClear(this) && ((state & STATE_MAD) != 0 || (tunnel.left == Tunnel.DUG || tunnel.left == Tunnel.HALF_DUG || leftTunnel.left == Tunnel.DUG || leftTunnel.top == Tunnel.DUG || leftTunnel.bottom == Tunnel.DUG)))
                    clearDirections.Add(LEFT_CLEAR);
                if (rightTunnel != null && dungeonScene.vegetableRightClear(this) && ((state & STATE_MAD) != 0 || (tunnel.right == Tunnel.DUG || tunnel.right == Tunnel.HALF_DUG || rightTunnel.right == Tunnel.DUG || rightTunnel.top == Tunnel.DUG || rightTunnel.bottom == Tunnel.DUG)))
                    clearDirections.Add(RIGHT_CLEAR);
                if (upTunnel != null && ((state & STATE_MAD) != 0 || (tunnel.top == Tunnel.DUG || tunnel.top == Tunnel.HALF_DUG || upTunnel.right == Tunnel.DUG || upTunnel.top == Tunnel.DUG || upTunnel.left == Tunnel.DUG)))
                    clearDirections.Add(UP_CLEAR);
                if (downTunnel != null && !dungeonScene.vegetableDirectlyBelow(this) && ((state & STATE_MAD) != 0 || (tunnel.bottom == Tunnel.DUG || tunnel.bottom == Tunnel.HALF_DUG || downTunnel.right == Tunnel.DUG || downTunnel.left == Tunnel.DUG || downTunnel.bottom == Tunnel.DUG)))
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
                    state &= ~STATE_ZOOM;
                    state |= STATE_CRASH;
                    position = tunnel.position + new Vector2(DungeonScreen.GRID_SIZE / 2, DungeonScreen.GRID_SIZE / 2);
                    animationTime = animationTimer = crashTime;
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

        public override void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_CHARGE) != 0)
            {
                charging.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_ZOOM) != 0)
            {
                zooming.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else if ((state & STATE_CRASH) != 0)
            {
                crashing.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
            }
            else
            {
                base.draw(spritebatch);
            }
        }
    }
}
