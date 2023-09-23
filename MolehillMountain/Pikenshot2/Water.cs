using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace MoleHillMountain
{
    public class Water
    {
        public const int NONE = 0;
        public const int SHAKING = 1;
        public const int FALLING = 2;
        public const int DEAD = 4;
        public const int MOVING_LEFT = 5;
        public const int MOVING_RIGHT = 6;

        private DungeonScreen dungeonScreen;
        private Sprite waterIdle;
        private Sprite waterFull;
        private Sprite currSprite;
        private float animationTime;
        private float animationTimer;
        private int fallingFrom;
        public Vector2 position;
        public int state;
        float waveMidTime = 650f;
        public int seen;
        public static Vector2 center = new Vector2(DungeonScreen.GRID_SIZE / 2, DungeonScreen.GRID_SIZE / 2);
        private float fallTime = 650f;
        private float shakeTime = 350f;
        private Sprite moving;
        private Sprite falling;
        private float fallSpeed = 68f;
        private bool listingLeft;
        private bool listingRight;
        private static Vector2 leftSide = new Vector2(-DungeonScreen.GRID_SIZE / 2, 0);
        private static Vector2 leftRight = new Vector2(DungeonScreen.GRID_SIZE / 2, 0);

        public Water(float x, float y, DungeonScreen dungeonScreen)
        {
            this.dungeonScreen = dungeonScreen;

            waterIdle = new Sprite(PikeAndShotGame.WATER, new Rectangle(0, 0, 20, 20), 20, 20);
            waterFull = new Sprite(PikeAndShotGame.WATER_FULL, new Rectangle(0, 0, 20, 20), 20, 20);
            falling = new Sprite(PikeAndShotGame.WATER_FULL, new Rectangle(0, 0, 20, 20), 20, 20);
            moving = new Sprite(PikeAndShotGame.WATER_WAVE, new Rectangle(0, 0, 20, 20), 20, 20);

            currSprite = waterIdle;
            animationTime = waveMidTime;
            position = new Vector2(x, y);

            state = NONE;

            seen = SeenStatus.NOT_SEEN;
        }
        public void draw(SpriteBatch spritebatch)
        {
            if (state == MOVING_RIGHT)
            {
                currSprite.draw(spritebatch, position + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
            }
            else
            {
                currSprite.draw(spritebatch, position + DungeonScreen.OFFSET, 0f);
            }
        }
        public void update(GameTime gameTime)
        {
            Tunnel tunnel = dungeonScreen.getCurrTunnel(position);
            Tunnel leftTunnel = dungeonScreen.getCurrTunnel(position + leftSide);
            Tunnel rightTunnel = dungeonScreen.getCurrTunnel(position + leftRight);
            if (tunnel != null)
            {
                seen = dungeonScreen.checkMoleSight(tunnel);
                if (seen == SeenStatus.SEEN)
                {
                    tunnel.water = true;
                }
            }
            if (leftTunnel != null)
            {
                seen = dungeonScreen.checkMoleSight(leftTunnel);
                if (seen == SeenStatus.SEEN)
                {
                    leftTunnel.water = true;
                }
            }
            if (rightTunnel != null)
            {
                seen = dungeonScreen.checkMoleSight(rightTunnel);
                if (seen == SeenStatus.SEEN)
                {
                    rightTunnel.water = true;
                }
            }

            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
            if (state != FALLING)
            {
                Tunnel tunnelBelow = dungeonScreen.getTunnelBelow(position);

                if ((tunnelBelow?.top == Tunnel.DUG || tunnelBelow?.top == Tunnel.HALF_DUG) && !dungeonScreen.waterBelow(position))
                {
                    fall();
                }
                else if (tunnelBelow?.left == Tunnel.DUG || tunnelBelow?.right == Tunnel.DUG || tunnelBelow?.bottom == Tunnel.DUG)
                {
                    if (!dungeonScreen.moleBelow(position) && !dungeonScreen.waterBelow(position))
                    {
                        if (state == NONE)
                        {
                            state = SHAKING;
                            animationTime = shakeTime;
                            animationTimer = animationTime;

                        }
                        else if ((tunnelBelow?.top == Tunnel.DUG || tunnelBelow?.top == Tunnel.HALF_DUG) && !dungeonScreen.waterBelow(position))
                        {
                            fall();
                        }
                    }
                }
            }

            if (state != FALLING)
            {
                Tunnel tunnelLeft = dungeonScreen.getTunnelLeft(position);
                Tunnel tunnelRight = dungeonScreen.getTunnelRight(position);
                Water waterLeft = dungeonScreen.waterLeft(position, 0);
                Water waterRight = dungeonScreen.waterRight(position, 0);

                if (state == NONE)
                {
                    if (tunnelRight?.left == Tunnel.DUG)
                    {
                        if (waterRight == null || (waterRight != null && waterRight.state == MOVING_RIGHT))
                            moveRight();
                        else if (waterRight.state == MOVING_LEFT)
                        {
                            if (tunnelLeft?.right == Tunnel.DUG)
                            {
                                if (waterLeft == null || (waterLeft != null && waterLeft.state == MOVING_LEFT))
                                    moveLeft();
                            }
                        }

                    }
                    else if (tunnelLeft?.right == Tunnel.DUG)
                    {
                        if (waterLeft == null || (waterLeft != null && waterLeft.state == MOVING_LEFT))
                            moveLeft();
                        else if (waterLeft.state == MOVING_RIGHT)
                        {
                            if (tunnelRight?.left == Tunnel.DUG)
                            {
                                if (waterRight == null || (waterRight != null && waterRight.state == MOVING_RIGHT))
                                    moveRight();
                            }
                        }
                    }
                }
                else if (state == MOVING_LEFT)
                {
                    if (dungeonScreen.waterLeft(position, 0) == null)
                    {
                        currSprite = moving;
                    }
                    else
                    {
                        currSprite = waterIdle;
                    }
                }
                else if (state == MOVING_RIGHT)
                {
                    if (dungeonScreen.waterRight(position, 0) == null)
                    {
                        currSprite = moving;
                    }
                    else
                    {
                        currSprite = waterIdle;
                    }
                }
            }

            animationTimer -= (float)elapsedGameTime.TotalMilliseconds;
            if (state == SHAKING)
            {
                if (animationTimer < 0)
                {
                    if (!dungeonScreen.waterBelow(position))
                        fall();
                    else
                        state = NONE;
                }
            }
            else if (state == FALLING)
            {
                position.Y += (float)elapsedGameTime.TotalSeconds * fallSpeed;
                tunnel = dungeonScreen.getTunnelBelow(position);

                if (tunnel != null)
                {
                    if ((int)tunnel.position.X + Tunnel.center.X < (int)position.X)
                    {
                        position.X -= 60f * (float)elapsedGameTime.TotalSeconds;
                        listingLeft = true;
                    }
                    else if ((int)tunnel.position.X + Tunnel.center.X > (int)position.X)
                    {
                        position.X += 60f * (float)elapsedGameTime.TotalSeconds;
                        listingRight = true;
                    }
                    else
                    {
                        listingLeft = false;
                        listingRight = false;
                    }
                }

                if (animationTimer < 0)
                {
                    animationTime = fallTime;
                    animationTimer = animationTime;
                }
            }
            else if (state == MOVING_LEFT)
            {
                position.X -= (float)elapsedGameTime.TotalSeconds * fallSpeed;

                if (animationTimer < 0)
                {
                    animationTime = fallTime;
                    animationTimer = animationTime;
                }
            }
            else if (state == MOVING_RIGHT)
            {
                position.X += (float)elapsedGameTime.TotalSeconds * fallSpeed;

                if (animationTimer < 0)
                {
                    animationTime = fallTime;
                    animationTimer = animationTime;
                }
            }

            animate(gameTime);

        }

        private void moveLeft()
        {
            state = MOVING_LEFT;
            animationTime = waveMidTime;
            animationTimer = animationTime;
            if (dungeonScreen.waterLeft(position, 0) == null)
            {
                currSprite = moving;
            }
            else
            {
                currSprite = waterIdle;
            }
        }

        private void moveRight()
        {
            state = MOVING_RIGHT;
            animationTime = waveMidTime;
            animationTimer = animationTime;
            if (dungeonScreen.waterRight(position, 0) == null)
            {
                currSprite = moving;
            }
            else
            {
                currSprite = waterIdle;
            }
        }

        private void fall()
        {
            state = FALLING;
            currSprite = falling;
            animationTime = fallTime;
            animationTimer = animationTime;
            fallingFrom = (int)position.Y;
        }

        private void animate(GameTime gameTime)
        {

            if (animationTimer < 0)
            {
                animationTimer = animationTime;
            }

            int maxFrames = currSprite.getMaxFrames();
            float frameTime = animationTime / (float)maxFrames;
            int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
            currSprite.setFrame(frameNumber);
        }

        internal void land()
        {
            state = NONE;
            currSprite.setFrame(0);
            currSprite = waterIdle;
        }

        internal void turnRight()
        {
            moveRight();
        }

        internal void turnLeft()
        {
            moveLeft();
        }
    }
}
