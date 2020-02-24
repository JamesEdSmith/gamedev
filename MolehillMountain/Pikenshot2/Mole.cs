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
        const int STATE_DIGGING = 1;

        Sprite walking;
        Sprite digging;
        Vector2 position;
        int state = 0;

        public Mole()
        {
            walking = new Sprite(PikeAndShotGame.MOLE_MINER_WALKING, new Rectangle(0, 0, 18, 18), 18, 18);
            digging = new Sprite(PikeAndShotGame.MOLE_MINER_DIGGING, new Rectangle(0, 0, 18, 18), 18, 18);
            position = new Vector2(10, 10);
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            if((state & STATE_DIGGING) != 0)
            {
                digging.draw(spritebatch, position, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE);
            } else
            {
                walking.draw(spritebatch, position, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE);
            }
        }

    }
}