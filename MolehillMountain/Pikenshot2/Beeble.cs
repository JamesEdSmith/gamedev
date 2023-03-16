using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    class Beeble : Rat
    {

        Sprite crashing;
        Sprite charging;
        Sprite zooming;

        public Beeble(DungeonScreen dungeonScene) : base(dungeonScene)
        {
            walking = new Sprite(PikeAndShotGame.BEEBLE_WALKING, new Rectangle(0, 0, 20, 20), 20, 20);

            walkingSprite = walking;
            nudging = new Sprite(PikeAndShotGame.RAT_NUDGE, new Rectangle(0, 0, 22, 18), 22, 18);
            squashed = new Sprite(PikeAndShotGame.RAT_CRUSHED, new Rectangle(0, 0, 22, 18), 22, 18);
            digging = new Sprite(PikeAndShotGame.BEEBLE_ZOOM, new Rectangle(0, 0, 20, 20), 20, 20);
            crashing = new Sprite(PikeAndShotGame.BEEBLE_CRASH, new Rectangle(0, 0, 20, 20), 20, 20);
            charging = new Sprite(PikeAndShotGame.BEEBLE_CHARGE, new Rectangle(0, 0, 20, 20), 20, 20);
            zooming = new Sprite(PikeAndShotGame.BEEBLE_ZOOM, new Rectangle(0, 0, 20, 20), 20, 20);


            clearDirections = new ArrayList(4);
            str = 3;
            health = 1;
            digTime = 325;
        }

        public Beeble(DungeonScreen dungeonScreen, int x, int y) : this(dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Beeble(DungeonScreen dungeonScreen, float x, float y) : this(dungeonScreen)
        {
            position.X = x;
            position.Y = y;
            drawPosition = new Vector2(position.X, position.Y);
            if (dungeonScene.checkMoleSight(dungeonScene.getCurrTunnel(position)) != SeenStatus.SEEN)
                walkingSprite = unseen;
        }
    }
}
