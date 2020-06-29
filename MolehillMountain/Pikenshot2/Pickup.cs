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
        static Color SEEN_COLOR = new Color(255, 255, 255, 255);
        static Color HALF_SEEN_COLOR = new Color(255, 255, 255, 127);
        Color currHalfSeenColor = new Color(255, 255, 255, 127);
        static Color NOT_SEEN_COLOR = new Color(255, 255, 255, 0);

        DungeonScreen dungeonScreen;
        public Vector2 position;
        Sprite idle;
        public int state = 0;

        float timeToAntic;
        float anticTime = 1000f;
        float anticTimer;

        public const int SEEN = 2;
        public const int HALF_SEEN = 1;
        public const int NOT_SEEN = 0;

        public int seen;
        float totalTime;


        public Pickup(float x, float y, DungeonScreen dungeonScreen)
        {
            position = new Vector2(x, y);
            this.dungeonScreen = dungeonScreen;
            idle = new Sprite(PikeAndShotGame.GRUB_EGG, new Rectangle(0, 0, 20, 20), 20, 20);
            resetAntic();
            seen = NOT_SEEN;
        }

        public Pickup(int x, int y, DungeonScreen dungeonScreen) : this(0f, 0f, dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (state == IDLE)
            {
                idle.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, getVisibilityColor());
            }
        }

        private Color getVisibilityColor()
        {
            switch (seen)
            {
                case NOT_SEEN:
                    return NOT_SEEN_COLOR;
                case HALF_SEEN:
                    currHalfSeenColor.A = (byte)(64 + 32f * Math.Sin(totalTime));
                    return currHalfSeenColor;
                default:
                    return SEEN_COLOR;
            }
        }

        public void update(GameTime gameTime)
        {
            float elapsedMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            totalTime += elapsedMilliseconds/50f;

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

            int moleSeen = dungeonScreen.checkMoleSight(position);

            if (moleSeen > seen)
            {
                seen = moleSeen;
            }

        }

        private void resetAntic()
        {
            timeToAntic = random.Next(1, 10) * anticTime;
            anticTimer = anticTime;
        }
    }
}
