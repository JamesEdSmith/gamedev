using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Tunnel
    {
        public static Rectangle sourceRect = new Rectangle(0, 0, DungeonScreen.GRID_SIZE, DungeonScreen.GRID_SIZE);
        public static Rectangle halfSourceRect = new Rectangle(0, 0, 12, DungeonScreen.GRID_SIZE);

        private static Vector2 halfRight = new Vector2(DungeonScreen.GRID_SIZE-2, DungeonScreen.GRID_SIZE / 2);
        private static Vector2 halfTop = new Vector2(DungeonScreen.GRID_SIZE / 2, 2);
        private static Vector2 halfBottom = new Vector2(DungeonScreen.GRID_SIZE / 2, DungeonScreen.GRID_SIZE-2);

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

        bool fired;

        public Tunnel(int x, int y)
        {
            position = new Vector2(x, y);
            seen = SeenStatus.NOT_SEEN;
        }

        public void update(DungeonScreen dungeonScreen)
        {
            seen = dungeonScreen.checkMoleSight(this);
        }

        public void draw(SpriteBatch spritebatch)
        {
            if (right == DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.None, 0);
            else if (right == HALF_DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + halfRight + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.None, 0);

            if (left == DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.FlipHorizontally, 0);
            else if (left == HALF_DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), 0, center, 1, SpriteEffects.FlipHorizontally, 0);

            if (bottom == DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * 0.5f, center, 1, SpriteEffects.None, 0);
            else if (bottom == HALF_DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + halfBottom + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * 0.5f, center, 1, SpriteEffects.None, 0);

            if (top == DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + center + DungeonScreen.OFFSET, sourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * -0.5f, center, 1, SpriteEffects.None, 0);
            else if (top == HALF_DUG)
                spritebatch.Draw(fired ? PikeAndShotGame.TUNNEL_FIRE_BACK : PikeAndShotGame.TUNNEL, position + halfTop + DungeonScreen.OFFSET, halfSourceRect, SeenStatus.getVisibilityColor(seen), MathHelper.Pi * -0.5f, center, 1, SpriteEffects.None, 0);

        }

        internal void fire()
        {
            fired = true;        
        }

        internal bool isFire()
        {
            return fired;
        }
    }
}
