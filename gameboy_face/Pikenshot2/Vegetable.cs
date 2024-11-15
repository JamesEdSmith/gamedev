﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Vegetable
    {
        static Random random = new Random();

        public const int NONE = 0;
        public const int SHAKING = 1;
        public const int FALLING = 2;
        public const int SPLITTING = 3;
        public const int DEAD = 4;
        public const int MOVING = 5;

        bool listingLeft;
        bool listingRight;

        float animationTimer;
        float fallSpeed = 68f;
        float fallTime = 225f;
        float shakeTime = 595f;
        float splitTime = 400f;
        float animationTime;

        DungeonScreen dungeonScreen;

        Sprite shaking;
        Sprite falling;
        Sprite splitting;
        Sprite currSprite;
        public Vector2 position;
        Vector2 drawPosition;
        //not flags
        public int state = 0;

        public int fallingFrom;

        public bool squashingMole;
        public bool gonnaBreak;
        public Vegetable(float x, float y, DungeonScreen dungeonScreen)
        {
            this.dungeonScreen = dungeonScreen;
            
            if (random.Next(2) == 0)
            {
                shaking = new Sprite(PikeAndShotGame.TURNIP_SHAKE, new Rectangle(0, 0, 20, 20), 20, 20);
                falling = new Sprite(PikeAndShotGame.TURNIP_TWIRL, new Rectangle(0, 0, 20, 20), 20, 20);
                splitting = new Sprite(PikeAndShotGame.TURNIP_SPLIT, new Rectangle(0, 0, 40, 20), 40, 20);
                fallTime = 112.5f;
            }
            else
            {
                shaking = new Sprite(PikeAndShotGame.ONION_SHAKE, new Rectangle(0, 0, 20, 20), 20, 20);
                falling = new Sprite(PikeAndShotGame.ONION_TWIRL, new Rectangle(0, 0, 20, 20), 20, 20);
                splitting = new Sprite(PikeAndShotGame.ONION_SPLIT, new Rectangle(0, 0, 40, 20), 40, 20);
                fallTime = 225f;
            }
            currSprite = shaking;
            animationTime = shakeTime;
            position = new Vector2(x, y);
            drawPosition = new Vector2(position.X, position.Y);
            state = NONE;
        }

        public void draw(SpriteBatch spritebatch)
        {
            if (listingLeft)
            {
                shaking.setFrame(1);
                shaking.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
            }
            else if (listingRight)
            {
                shaking.setFrame(3);
                shaking.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
            }
            else
            {
                currSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
            }
        }

        internal void update(GameTime gameTime)
        {
            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
            if (state == NONE || state == MOVING)
            {
                Tunnel tunnelBelow = dungeonScreen.getTunnelBelow(position);
                if (tunnelBelow?.left == Tunnel.DUG || tunnelBelow?.right == Tunnel.DUG || tunnelBelow?.bottom == Tunnel.DUG || tunnelBelow?.top == Tunnel.DUG)
                {
                    if (!dungeonScreen.moleBelow(position))
                    {
                        if (state == NONE)
                        {
                            state = SHAKING;
                            animationTime = shakeTime;
                            animationTimer = animationTime;
                            currSprite = shaking;
                        }
                        else
                        {
                            fall();
                        }
                    }
                }
                if (state == MOVING)
                {
                    state = NONE;
                }
            }
            else
            {
                animationTimer -= (float)elapsedGameTime.TotalMilliseconds;
                if (state == SHAKING)
                {
                    if (animationTimer < 0)
                    {
                        fall();
                    }
                }
                else if (state == FALLING)
                {
                    position.Y += (float)elapsedGameTime.TotalSeconds * fallSpeed;
                    Tunnel tunnel = dungeonScreen.getTunnelBelow(position);

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
                    if (dungeonScreen.moleJustBelow(position))
                    {
                        dungeonScreen.squashMole(this);
                        squashingMole = true;
                        gonnaBreak = true;
                    }

                    if (animationTimer < 0)
                    {
                        animationTime = fallTime;
                        animationTimer = animationTime;
                    }
                }
                else if (state == SPLITTING)
                {
                    if (animationTimer < 0)
                    {
                        state = DEAD;
                    }
                }

                animate(elapsedGameTime);
            }

            if (state == SHAKING)
            {
                drawPosition.X = (int)position.X + (int)(1.1f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds/25f));
                drawPosition.Y = (int)position.Y + (int)(1.1f * (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds/25f));
            }
            else
            {
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;
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

        private void animate(TimeSpan elapsedGameTime)
        {
            if (state != SHAKING)
            {
                int maxFrames = currSprite.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                currSprite.setFrame(frameNumber);
            }
        }

        internal void split()
        {
            state = SPLITTING;
            currSprite = splitting;
            animationTime = splitTime;
            animationTimer = animationTime;
        }

        internal void land()
        {
            state = Vegetable.NONE;
            currSprite.setFrame(0);
        }
    }
}
