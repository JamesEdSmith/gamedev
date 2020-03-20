using System;
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
        public const int NONE = 0;
        public const int SHAKING = 1;
        public const int FALLING = 2;
        public const int SPLITTING = 3;
        public const int DEAD = 4;

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
        public Vegetable(float x, float y, DungeonScreen dungeonScreen)
        {
            this.dungeonScreen = dungeonScreen;
            shaking = new Sprite(PikeAndShotGame.TURNIP_SHAKE, new Rectangle(0, 0, 20, 20), 20, 20);
            falling = new Sprite(PikeAndShotGame.TURNIP_TWIRL, new Rectangle(0, 0, 20, 20), 20, 20);
            splitting = new Sprite(PikeAndShotGame.TURNIP_SPLIT, new Rectangle(0, 0, 40, 20), 40, 20);
            currSprite = shaking;
            animationTime = shakeTime;
            position = new Vector2(x, y);
            drawPosition = new Vector2(position.X, position.Y);
            state = NONE;
        }

        public void draw(SpriteBatch spritebatch)
        {
            currSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
        }

        internal void update(TimeSpan elapsedGameTime)
        {
            

            if (state == NONE)
            {
                Tunnel tunnelBelow = dungeonScreen.getTunnelBelow(position);
                if (tunnelBelow?.left == Tunnel.DUG || tunnelBelow?.right == Tunnel.DUG || tunnelBelow?.bottom == Tunnel.DUG || tunnelBelow?.top == Tunnel.DUG)
                {
                    if (!dungeonScreen.molebelow(position))
                    {
                        state = SHAKING;
                        animationTime = shakeTime;
                        animationTimer = animationTime;
                        currSprite = shaking;
                    }
                }
            }
            else
            {
                animationTimer -= (float)elapsedGameTime.TotalMilliseconds;
                if (state == SHAKING)
                {
                    if (animationTimer < 0)
                    {
                        state = FALLING;
                        currSprite = falling;
                        animationTime = fallTime;
                        animationTimer = animationTime;
                    }
                }
                else if (state == FALLING)
                {
                    position.Y += (float)elapsedGameTime.TotalSeconds * fallSpeed;
                    if (animationTimer < 0)
                    {
                        animationTime = fallTime;
                        animationTimer = animationTime;
                    }
                } else if (state == SPLITTING)
                {
                    if (animationTimer < 0)
                    {
                        state = DEAD;
                    }
                }

                animate(elapsedGameTime);
            }
            drawPosition.X = (int)position.X;
            drawPosition.Y = (int)position.Y;
        }

        private void animate(TimeSpan elapsedGameTime)
        {
            int maxFrames = currSprite.getMaxFrames();
            float frameTime = animationTime / (float)maxFrames;
            int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;

            currSprite.setFrame(frameNumber);
        }

        internal void split()
        {
            state = SPLITTING;
            currSprite = splitting;
            animationTime = splitTime;
            animationTimer = animationTime;
        }
    }
}
