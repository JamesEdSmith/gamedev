using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MoleHillMountain
{
    class Pickup
    {
        public const int STATE_IDLE = 0;
        public const int STATE_LOOKING = 1;
        public const int STATE_COLLECTED = 2;

        static Random random = new Random();
        static Color SEEN_COLOR = new Color(255, 255, 255, 255);
        static Color HALF_SEEN_COLOR = new Color(255, 255, 255, 127);
        Color currHalfSeenColor = new Color(255, 255, 255, 127);
        static Color NOT_SEEN_COLOR = new Color(255, 255, 255, 0);

        DungeonScreen dungeonScreen;
        public Vector2 position;
        Sprite idle;
        Sprite looking;
        Sprite currSprite;
        public int state = 0;

        float timeToAntic;
        float anticTime = 1000f;
        float blinkTime = 400f;

        float animationTimer;

        bool isGrub = false;
        bool flip = false;

        public const int SEEN = 2;
        public const int HALF_SEEN = 1;
        public const int NOT_SEEN = 0;

        public int seen;
        float totalTime;


        public Pickup(float x, float y, DungeonScreen dungeonScreen)
        {
            position = new Vector2(x, y);
            this.dungeonScreen = dungeonScreen;
            if (random.Next(100) > 95)
            {
                isGrub = true;
                looking = new Sprite(PikeAndShotGame.GRUB_LOOK, new Rectangle(0, 0, 20, 20), 20, 20);
                idle = new Sprite(PikeAndShotGame.GRUB_GRUB, new Rectangle(0, 0, 20, 20), 20, 20);
            }
            else
            {
                idle = new Sprite(PikeAndShotGame.GRUB_EGG, new Rectangle(0, 0, 20, 20), 20, 20);
            }
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
            if (state == STATE_IDLE)
            {
                idle.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, getVisibilityColor());
            }
            else
            {
                if (flip)
                {
                    currSprite.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, getVisibilityColor(), SpriteEffects.FlipHorizontally);
                }
                else
                {
                    currSprite.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, getVisibilityColor());
                }
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
            totalTime += elapsedMilliseconds / 50f; // flickering

            if (state == STATE_IDLE)
            {
                if (timeToAntic > 0)
                {
                    timeToAntic -= elapsedMilliseconds;
                }
                else
                {
                    if (animationTimer > 0)
                    {
                        animationTimer -= elapsedMilliseconds;
                    }
                    else
                    {
                        resetAntic();
                    }
                }


                int maxFrames = idle.getMaxFrames();
                float frameTime = anticTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                idle.setFrame(frameNumber);
            }
            else if (state == STATE_LOOKING)
            {
                if (animationTimer > 0)
                {
                    animationTimer -= elapsedMilliseconds;
                    if (animationTimer > blinkTime * 0.75f)
                    {
                        idle.setFrame(0);
                        currSprite = idle;
                    }
                    else if (animationTimer > blinkTime * 0.5f)
                    {
                        looking.setFrame(0);
                        currSprite = looking;
                    }
                    else if (animationTimer > blinkTime * 0.25f)
                    {
                        idle.setFrame(0);
                        currSprite = idle;
                    }
                    else
                    {
                        looking.setFrame(0);
                        currSprite = looking;
                    }
                }
                else
                {
                    Vector2 diff = dungeonScreen.getMolePosition() - position;
                    bool left = false;
                    bool right = false;
                    bool down = false;
                    bool up = false;

                    if (diff.Y >= 0 && Math.Abs(diff.Y) >= Math.Abs(diff.X) * 0.75f)
                    {
                        down = true;
                    }
                    else if (diff.Y < 0 && Math.Abs(diff.Y) >= Math.Abs(diff.X) * 0.75f)
                    {
                        up = true;
                    }
                    if (diff.X >= 0 && Math.Abs(diff.X) >= Math.Abs(diff.Y) * 0.75f)
                    {
                        right = true;
                    }
                    else if (diff.X < 0 && Math.Abs(diff.X) >= Math.Abs(diff.Y)* 0.75f)
                    {
                        left = true;
                    }

                    if (left)
                    {
                        flip = true;
                    }
                    else
                    {
                        flip = false;
                    }

                    if (down)
                    {
                        if (!right && !left)
                        {
                            looking.setFrame(5);
                        }
                        else
                        {
                            looking.setFrame(4);
                        }
                        currSprite = looking;
                    }
                    else if (up)
                    {
                        if (!right && !left)
                        {
                            looking.setFrame(1);
                        }
                        else
                        {
                            looking.setFrame(2);
                        }
                        currSprite = looking;
                    }
                    else
                    {
                        looking.setFrame(3);
                        currSprite = looking;
                    }
                }
            }

            int moleSeen = dungeonScreen.checkMoleSight(position);


            if (moleSeen > seen)
            {
                seen = moleSeen;
            }

            if (isGrub)
            {
                int moleClose = dungeonScreen.checkMoleClose(position);
                if (moleClose > 0)
                {
                    if (state != STATE_LOOKING && state != STATE_COLLECTED)
                    {
                        state = STATE_LOOKING;
                        animationTimer = blinkTime;
                        idle.setFrame(0);
                        currSprite = idle;
                    }
                }
                else
                {
                    if (state != STATE_IDLE && state != STATE_COLLECTED)
                    {
                        state = STATE_IDLE;
                        animationTimer = anticTime;
                    }
                }
            }

        }

        internal void collected(int count)
        {
            SoundEffectInstance collectedNoise = PikeAndShotGame.PICKUP_GRUB.CreateInstance();
            collectedNoise.Volume = 0.5f;
            collectedNoise.Pitch = -0.25f + 0.125f * (float)count;
            collectedNoise.Play();
            state = STATE_COLLECTED;
        }

        private void resetAntic()
        {
            timeToAntic = random.Next(1, 10) * anticTime;
            animationTimer = anticTime;
        }
    }
}
