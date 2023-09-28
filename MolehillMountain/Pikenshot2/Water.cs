﻿using System;
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
        public Tunnel tunnel;
        Tunnel leftTunnel;
        Tunnel rightTunnel;

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

            tunnel = dungeonScreen.getCurrTunnel(position);
            if (tunnel != null)
            {
                seen = dungeonScreen.checkMoleSight(tunnel);
                if (seen == SeenStatus.SEEN)
                {
                    tunnel.water = true;
                }

            }

            if (state == MOVING_LEFT || state == MOVING_RIGHT)
            {
                leftTunnel = dungeonScreen.getCurrTunnel(position + leftSide);
                rightTunnel = dungeonScreen.getCurrTunnel(position + leftRight);

                if (leftTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(leftTunnel) == SeenStatus.SEEN)
                    {
                        leftTunnel.water = true;
                    }
                }
                if (rightTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(rightTunnel) == SeenStatus.SEEN)
                    {
                        rightTunnel.water = true;
                    }
                }
            }
            else if (state == FALLING)
            {
                Tunnel topTunnel = dungeonScreen.getTunnelAbove(position);
                Tunnel bottomTunnel = dungeonScreen.getTunnelBelow(position);

                if (topTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(topTunnel) == SeenStatus.SEEN)
                    {
                        topTunnel.water = true;
                    }
                }
                if (bottomTunnel != null)
                {
                    if (dungeonScreen.checkMoleSight(bottomTunnel) == SeenStatus.SEEN)
                    {
                        bottomTunnel.water = true;
                    }
                }
            }

            Tunnel tunnelBelow = dungeonScreen.getTunnelBelow(position);
            Tunnel tunnelLeft = dungeonScreen.getTunnelLeft(position);
            Tunnel tunnelRight = dungeonScreen.getTunnelRight(position);
            Tunnel tunnelAbove = dungeonScreen.getTunnelAbove(position);
            Water waterBelow = null;
            Water waterLeft = null;
            Water waterRight = null;
            Water waterAbove = null;

            foreach (Water water in dungeonScreen.waters)
            {
                if (water.tunnel == tunnelBelow)
                    waterBelow = water;
                if (water.tunnel == tunnelLeft)
                    waterLeft = water;
                if (water.tunnel == tunnelRight)
                    waterRight = water;
                if (water.tunnel == tunnelAbove)
                    waterAbove = water;
            }

            bool leftOfCenter = position.X < tunnel.position.X + Tunnel.center.X;
            bool rightOfCenter = position.X > tunnel.position.X + Tunnel.center.X;


            if (tunnelBelow != null && tunnelBelow.top != Tunnel.NOT_DUG && waterBelow == null && Math.Abs(position.X - (tunnel.position.X + Tunnel.center.X)) < 5)
            {
                fall();
            }
            else if (state == FALLING)
            {
                if (tunnelBelow == null || tunnelBelow.top == Tunnel.NOT_DUG || waterBelow != null)
                {
                    land();
                }
            }
            else if (state == NONE || state == MOVING_LEFT)
            {
                if ((tunnelLeft != null && tunnelLeft.right != Tunnel.NOT_DUG) || rightOfCenter)
                {
                    moveLeft();
                }
                else if ((tunnelRight != null && tunnelRight.left != Tunnel.NOT_DUG) || leftOfCenter)
                {
                    moveRight();
                }
                else
                {
                    land();
                }
            }
            else
            {
                if ((tunnelRight != null && tunnelRight.left != Tunnel.NOT_DUG) || leftOfCenter)
                {
                    moveRight();
                }
                else if ((tunnelLeft != null && tunnelLeft.right != Tunnel.NOT_DUG) || rightOfCenter)
                {
                    moveLeft();
                }
                else
                {
                    land();
                }

            }

            move(gameTime);
            animate(gameTime, waterAbove);

        }

        private void move(GameTime gameTime)
        {
            if (state == FALLING)
            {
                position.Y += fallSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (state == MOVING_LEFT)
            {
                position.X -= fallSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (state == MOVING_RIGHT)
            {
                position.X += fallSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private void moveLeft()
        {
            if (state != MOVING_LEFT)
            {
                state = MOVING_LEFT;
                animationTime = waveMidTime;
                animationTimer = animationTime;
                Water waterLeft = dungeonScreen.waterLeft(position, 5);
                if (waterLeft == null || waterLeft.state == FALLING)
                {
                    currSprite = moving;
                }
                else
                {
                    currSprite = waterIdle;
                }
            }
        }

        private void moveRight()
        {
            if (state != MOVING_RIGHT)
            {
                state = MOVING_RIGHT;
                animationTime = waveMidTime;
                animationTimer = animationTime;
                Water waterRight = dungeonScreen.waterRight(position, 5);
                if (waterRight == null || waterRight.state == FALLING)
                {
                    currSprite = moving;

                }
                else
                {
                    currSprite = waterIdle;
                }
            }
        }

        private void fall()
        {
            if (state != FALLING)
            {
                state = FALLING;
                currSprite = falling;
                animationTime = fallTime;
                animationTimer = animationTime;
                fallingFrom = (int)position.Y;
                position.X = tunnel.position.X + Tunnel.center.X;
            }
        }

        private void animate(GameTime gameTime, Water waterAbove)
        {
            switch (state)
            {
                case NONE:
                    if (waterAbove != null)
                        currSprite = waterFull;
                    else
                        currSprite = waterIdle;
                    break;
                case FALLING:
                    currSprite = falling;
                    break;
                case MOVING_LEFT:
                    if (waterAbove != null)
                        currSprite = waterFull;
                    else
                    {
                        Water waterLeft = dungeonScreen.waterLeft(position, 5);
                        if (waterLeft == null || waterLeft.state == FALLING)
                        {
                            currSprite = moving;
                        }
                        else
                        {
                            currSprite = waterIdle;
                        }
                    }
                    break;
                case MOVING_RIGHT:
                    if (waterAbove != null)
                        currSprite = waterFull;
                    else
                    {
                        Water waterRight = dungeonScreen.waterRight(position, 5);
                        if (waterRight == null || waterRight.state == FALLING)
                        {
                            currSprite = moving;
                        }
                        else
                        {
                            currSprite = waterIdle;
                        }
                    }
                    break;

            }

            animationTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
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
            if (state != NONE && (state != FALLING || position.Y >= tunnel.position.Y + Tunnel.center.Y))
            {
                position.Y = tunnel.position.Y + Tunnel.center.Y;
                state = NONE;
                currSprite.setFrame(0);
                currSprite = waterIdle;
                animationTime = waveMidTime;
                animationTimer = animationTime;
            }
        }
    }
}
