using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoleHillMountain
{
    class Pickup
    {
        public const int IDLE = 0;
        public const int LOOKING = 1;

        static Random random = new Random();
        DungeonScreen dungeonScreen;
        public Vector2 position;
        Sprite idle;
        public int state = 0;

        float timeToAntic;
        float anticTime = 1000f;
        float anticTimer;

        public Pickup(float x, float y, DungeonScreen dungeonScreen)
        {
            position = new Vector2(x, y);
            this.dungeonScreen = dungeonScreen;
            idle = new Sprite(PikeAndShotGame.GRUB_EGG, new Rectangle(0, 0, 20, 20), 20, 20);
            resetAntic();
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (state == IDLE)
            {
                idle.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f);
            }
        }

        public void update(GameTime gameTime)
        {
            float elapsedMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeToAntic > 0)
            {
                timeToAntic -= elapsedMilliseconds;
            }
            else
            {
                if (anticTimer > 0)
                {
                    anticTimer -= elapsedMilliseconds;
                }
                else
                {
                    resetAntic();
                }
            }

            if (state == IDLE)
            {
                int maxFrames = idle.getMaxFrames();
                float frameTime = anticTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(anticTimer / frameTime) - 1;
                idle.setFrame(frameNumber);
            }

        }

        private void resetAntic()
        {
            timeToAntic = random.Next(1, 10) * anticTime;
            anticTimer = anticTime;
        }
    }
}
