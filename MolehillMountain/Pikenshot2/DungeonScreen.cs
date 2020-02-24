using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace PikeAndShot
{
    class DungeonScreen : GameScreen
    {

        ArrayList sprites;
        Mole mole;

        public DungeonScreen()
        {
            sprites = new ArrayList(10);
            mole = new Mole();
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            mole.draw(spriteBatch);
        }

        public void update(GameTime gameTime)
        {
            
        }
    }
}
