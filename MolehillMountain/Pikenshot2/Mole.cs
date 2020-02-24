using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace PikeAndShot
{
    internal class Mole
    {
        const int MOVING_NONE = 0;
        const int MOVING_LEFT = 1;
        const int MOVING_RIGHT = 2;
        const int STATE_WALKING = 1;
        const int STATE_DIGGING = 2;

        float animationTimer;
        float walkSpeed = 25f;
        float walkTime = 400f;

        int horzFacing = Sprite.DIRECTION_LEFT;
        int vertFacing = Sprite.DIRECTION_NONE;

        Sprite walking;
        Sprite digging;
        Sprite activeSprite;
        Vector2 position;
        //flags
        int state = 0;
        //notflags
        int moving = 0;

        public Mole()
        {
            walking = new Sprite(PikeAndShotGame.MOLE_MINER_WALKING, new Rectangle(0, 0, 18, 18), 18, 18);
            digging = new Sprite(PikeAndShotGame.MOLE_MINER_DIGGING, new Rectangle(0, 0, 18, 18), 18, 18);
            activeSprite = walking;
            position = new Vector2(10, 10);
        }

        public void update(TimeSpan timeSpan)
        {
            animationTimer -= (float)timeSpan.TotalMilliseconds;

            if (moving == MOVING_NONE)
            {
                state &= ~STATE_WALKING;
                walking.setFrame(0);
            }
            else
            {
                if (animationTimer < 0)
                {
                    animationTimer += walkTime;
                }
                state |= STATE_WALKING;
                activeSprite = walking;
                if (moving == MOVING_LEFT)
                {
                    position.X -= (float)timeSpan.TotalSeconds * walkSpeed;
                }
                else if (moving == MOVING_RIGHT)
                {
                    position.X += (float)timeSpan.TotalSeconds * walkSpeed;
                }
                int maxFrames = walking.getMaxFrames();
                float frameTime = walkTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;

                walking.setFrame(frameNumber);
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            activeSprite.draw(spritebatch, position, horzFacing, vertFacing);
        }

        void walk()
        {
            if (moving == MOVING_NONE)
            {
                animationTimer = walkTime;
            }
            state |= STATE_WALKING;
        }

        internal void moveLeft()
        {
            walk();
            moving = MOVING_LEFT;
            horzFacing = Sprite.DIRECTION_LEFT;
        }

        internal void moveRight()
        {
            walk();
            moving = MOVING_RIGHT;
            horzFacing = Sprite.DIRECTION_RIGHT;
        }

        internal void stopMoving()
        {
            moving = MOVING_NONE;
        }
    }
}