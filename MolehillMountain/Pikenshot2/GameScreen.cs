﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    public interface GameScreen
    {
        void update(GameTime gameTime);

        void preDraw(GameTime gameTime, SpriteBatch spriteBatch);
        void preDraw2(GameTime gameTime, SpriteBatch spriteBatch);

        void draw(GameTime gameTime, SpriteBatch spriteBatch);
    }
}
