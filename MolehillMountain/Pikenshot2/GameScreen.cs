using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace PikeAndShot
{
    public interface GameScreen
    {
        void update(GameTime gameTime);


        void draw(GameTime gameTime, SpriteBatch spriteBatch);
    }
}
