using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace MoleHillMountain
{
    class Animation
    {
        Vector2 position;
        public bool active;
        protected Sprite sprite;
        private float timer;
        public float time;
        private int horz;
        private int vert;
        private int loops;
        private float delay;
        public AnimationType type;
        public const int REVEAL_TIME = 600;

        public Animation(AnimationType type)
        {
            position = new Vector2(0, 0);
            active = false;
            this.type = type;
            switch (type)
            {
                case AnimationType.stoneImpact:
                    sprite = new Sprite(PikeAndShotGame.STONE_IMPACT, new Rectangle(0, 0, 20, 20), 20, 20);
                    time = 500;
                    break;
                case AnimationType.fightCloud:
                    sprite = new Sprite(PikeAndShotGame.FIGHT_CLOUD, new Rectangle(0, 0, 22, 22), 22, 22);
                    time = Mole.FIGHT_TIME / 2f;
                    loops = 1;
                    break;
                case AnimationType.tunnelReveal:
                    sprite = new Sprite(PikeAndShotGame.TUNNEL_REVEAL, new Rectangle(0, 0, 20, 20), 20, 20);
                    time = REVEAL_TIME;
                    break;
                case AnimationType.hookImpact:
                    sprite = new Sprite(PikeAndShotGame.STONE_IMPACT2, new Rectangle(0, 0, 20, 20), 20, 20);
                    time = 250;
                    break;
                case AnimationType.explosion:
                    sprite = new Sprite(PikeAndShotGame.EXPLOSION, new Rectangle(0, 0, 20, 20), 20, 20);
                    time = 250f;
                    break;
                case AnimationType.explosionAngle:
                    sprite = new Sprite(PikeAndShotGame.EXPLOSION_ANGLE, new Rectangle(0, 0, 20, 20), 20, 20);
                    time = 250f;
                    break;
                case AnimationType.explosionCenter:
                    sprite = new Sprite(PikeAndShotGame.TUNNEL_FIRE_BACK, new Rectangle(0, 0, 20, 20), 20, 20);
                    time = 300f;
                    break;
            }
        }

        public void activate()
        {
            timer = time;
            active = true;
        }

        public void activate(int x, int y, int horz, int vert, float delay = 0)
        {
            position.X = x;
            position.Y = y;
            this.horz = horz;
            this.vert = vert;
            this.delay = delay;
            activate();
        }

        public void update(TimeSpan timeSpan)
        {
            if (delay > 0)
            {
                delay -= (float)timeSpan.TotalMilliseconds;
            }
            else
            {
                timer -= (float)timeSpan.TotalMilliseconds;
            }

            if (timer <= 0)
            {
                if (loops > 0)
                {
                    timer = time;
                    loops--;
                }
                else
                {
                    timer = 0;
                    active = false;
                }
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
