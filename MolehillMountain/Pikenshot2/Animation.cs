﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace MoleHillMountain
{
    class Animation
    {
        Vector2 position;
        public bool active;
        Sprite sprite;
        float timer;
        float time;
        int horz;
        int vert;

        public Animation()
        {
            position = new Vector2(0, 0);
            active = false;
            sprite = new Sprite(PikeAndShotGame.STONE_IMPACT, new Rectangle(0, 0, 20, 20), 20, 20);
            time = 500;
        }
        public void activate()
        {
            timer = time;
            active = true;
        }

        public void activate(int x, int y, int horz, int vert)
        {
            activate();
            position.X = x;
            position.Y = y;
            this.horz = horz;
            this.vert = vert;
        }

        public void update(TimeSpan timeSpan)
        {
            timer -= (float)timeSpan.TotalMilliseconds;

            if (timer < 0)
            {
                timer = 0;
                active = false;
            }

            int maxFrames = sprite.getMaxFrames();
            float frameTime = time / (float)maxFrames;
            int frameNumber = maxFrames - (int)(timer / frameTime) - 1;
            sprite.setFrame(frameNumber);

        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (active)
            {
                sprite.draw(spriteBatch, position, horz, vert);
            }
        }
    }
}
