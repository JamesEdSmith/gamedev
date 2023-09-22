using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    public class Tunnel
    {
        public static Rectangle sourceRect = new Rectangle(0, 0, DungeonScreen.GRID_SIZE, DungeonScreen.GRID_SIZE);
        public static Rectangle halfSourceRect = new Rectangle(0, 0, 12, DungeonScreen.GRID_SIZE);

        private static Vector2 halfRight = new Vector2(DungeonScreen.GRID_SIZE - 2, DungeonScreen.GRID_SIZE / 2);
        private static Vector2 halfTop = new Vector2(DungeonScreen.GRID_SIZE / 2, 2);
        private static Vector2 halfBottom = new Vector2(DungeonScreen.GRID_SIZE / 2, DungeonScreen.GRID_SIZE - 2);
        private static Vector2 halfLeft = new Vector2(0, DungeonScreen.GRID_SIZE / 2);

        public static Vector2 center = new Vector2(DungeonScreen.GRID_SIZE / 2, DungeonScreen.GRID_SIZE / 2);

        public Vector2 position;
        public const int DUG = 2;
        public const int HALF_DUG = 1;
        public const int NOT_DUG = 0;
        // 1 = half dug
        // 2 = all the way dug
        public int top;
        public int right;
        public int left;
        public int bottom;

        public const int SEEN = 2;
        public const int HALF_SEEN = 1;
        public const int NOT_SEEN = 0;

        public int seen;
        internal bool revealed;
        internal bool starting;

        bool animateFire;
        public bool animateWind;
        float animationTimer;
        public Mothy mothy;
        float fireTime = 333.333f;
        float windTime = 900f;
        private int length;
        public int direction;
        bool fired;
        bool windy;

        Sprite fireSprite;
        Sprite fireEndSprite;
        Sprite windSprite;
        Sprite waterSprite;
        Sprite waterFullSprite;

        Color dimColor;

        static Vector2 oneX = new Vector2(1, 0);
        static Vector2 oneY = new Vector2(0, 1);

        public bool water;

        public Tunnel(int x, int y)
        {
            position = new Vector2(x, y);
            seen = SeenStatus.NOT_SEEN;
            fireSprite = new Sprite(PikeAndShotGame.TUNNEL_FIRE_BACK, new Rectangle(0, 0, 20, 20), 20, 20);
            fireEndSprite = new Sprite(PikeAndShotGame.FIRE, new Rectangle(0, 0, 20, 20), 20, 20);
            windSprite = new Sprite(PikeAndShotGame.MOTHY_WIND, new Rectangle(0, 0, 20, 20), 20, 20);
            waterSprite = new Sprite(PikeAndShotGame.WATER, new Rectangle(0, 0, 20, 20), 20, 20);

        }

        public void waterDraw(SpriteBatch spritebatch)
        {
            //draw(spritebatch);
            waterSprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, 0f);

        }

        public void update(DungeonScreen dungeonScreen, GameTime gameTime)
        {
            water = false;
            seen = dungeonScreen.checkMoleSight(this);
            dimColor = SeenStatus.getVisibilityColor(seen);

            if (animateFire || animateWind)
            {
                animationTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (animateFire)
            {
                fired = false;

                if (length != 0)
                {
                    int maxFrames = fireSprite.getMaxFrames();
                    float frameTime = fireTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    fireSprite.setFrame(frameNumber);
                }
                else
                {
                    int maxFrames = fireEndSprite.getMaxFrames();
                    float frameTime = fireTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    fireEndSprite.setFrame(frameNumber);
                }
            }

            if (animateWind)
            {
                windy = false;

                int maxFrames = windSprite.getMaxFrames();
                float frameTime = windTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                windSprite.setFrame(frameNumber);
            }

            if (animationTimer <= 0)
            {
                if (animateFire)
                    animateFire = false;
                if (animateWind)
                {
                    animateWind = false;
                }
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            if (right == DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.None, 0);
            else if (right == HALF_DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + halfRight + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.None, 0);

            if (left == DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.FlipHorizontally, 0);
            else if (left == HALF_DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.FlipHorizontally, 0);

            if (bottom == DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * 0.5f, center, 1, SpriteEffects.None, 0);
            else if (bottom == HALF_DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + halfBottom + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * 0.5f, center, 1, SpriteEffects.None, 0);

            if (top == DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * -0.5f, center, 1, SpriteEffects.None, 0);
            else if (top == HALF_DUG)
                spritebatch.Draw(PikeAndShotGame.TUNNEL, position + halfTop + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * -0.5f, center, 1, SpriteEffects.None, 0);
        }

        public void drawEffect(SpriteBatch spritebatch)
        {
            Sprite sprite;
            if (animateFire)
            {
                if (length == 0)
                {
                    sprite = fireEndSprite;
                    if (direction == Mole.MOVING_LEFT)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, Mole.MOVING_NONE, Mole.MOVING_NONE, SeenStatus.getVisibilityColor(seen));
                    else if (direction == Mole.MOVING_RIGHT)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, Mole.MOVING_RIGHT, Mole.MOVING_NONE, SeenStatus.getVisibilityColor(seen));
                    else if (direction == Mole.MOVING_UP)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, MathHelper.Pi * 0.5f);
                    else
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, -MathHelper.Pi * 0.5f);

                }
                else
                {
                    sprite = fireSprite;

                    if (right == DUG)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, Mole.MOVING_NONE, Mole.MOVING_NONE, SeenStatus.getVisibilityColor(seen));
                    else if (right == HALF_DUG)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, Mole.MOVING_NONE, Mole.MOVING_NONE, SeenStatus.getVisibilityColor(seen));

                    if (left == DUG)
                        sprite.draw(spritebatch, position - oneX + center + DungeonScreen.OFFSET, Mole.MOVING_RIGHT, Mole.MOVING_NONE, SeenStatus.getVisibilityColor(seen));
                    else if (left == HALF_DUG)
                        sprite.draw(spritebatch, position - oneX + center + DungeonScreen.OFFSET, Mole.MOVING_RIGHT, Mole.MOVING_NONE, SeenStatus.getVisibilityColor(seen));

                    if (bottom == DUG)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, MathHelper.Pi * 0.5f);
                    else if (bottom == HALF_DUG)
                        sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, MathHelper.Pi * 0.5f);

                    if (top == DUG)
                        sprite.draw(spritebatch, position - oneY + center + DungeonScreen.OFFSET, -MathHelper.Pi * 0.5f);
                    else if (top == HALF_DUG)
                        sprite.draw(spritebatch, position - oneY + center + DungeonScreen.OFFSET, -MathHelper.Pi * 0.5f);
                }


            }

            if (animateWind)
            {
                sprite = windSprite;
                if (direction == Mole.MOVING_LEFT)
                    sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, Mole.MOVING_NONE, Mole.MOVING_NONE);
                else if (direction == Mole.MOVING_RIGHT)
                    sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, Mole.MOVING_RIGHT, Mole.MOVING_NONE);
                else if (direction == Mole.MOVING_UP)
                    sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, MathHelper.Pi * 0.5f);
                else
                    sprite.draw(spritebatch, position + center + DungeonScreen.OFFSET, -MathHelper.Pi * 0.5f);
            }
        }

        internal void fire(int length, int direction)
        {
            this.length = length;
            this.direction = direction;
            fired = true;
            animateFire = true;
            animationTimer = fireTime;
        }

        internal void wind(int length, int direction, Mothy moth)
        {

            this.length = length;
            this.direction = direction;
            windy = true;
            animateWind = true;
            animationTimer = windTime;
            this.mothy = moth;
        }

        internal bool isFire()
        {
            return fired;
        }

        internal bool isWind()
        {
            return windy;
        }
    }
}
