using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cheyenne_Show_offs
{
    class TextPopup
    {
        public string text;
        public Vector2 position;
        private float lifeTime;
        public float timer;
        private float fadeTime;
        private float fadeStart;
        public float popEnd;
        private float popTime;
        public byte alpha;
        public bool done;
        public bool good;
        float scale;
        Color frontColor;

        public TextPopup(string text, Vector2 position, float lifeTime, bool good)
        {
            this.text = text;
            this.position = position;
            this.lifeTime = lifeTime;
            fadeStart = lifeTime * 3 / 4;
            fadeTime = lifeTime - fadeStart;
            popEnd = lifeTime / 16;
            popTime = popEnd;
            timer = 0f;
            alpha = 255;
            this.good = good;
            done = false;
            scale = 1.5f;
            frontColor = Color.White;
        }

        public void update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timer > fadeStart)
            {
                alpha = (byte)(255f - 255f * ((timer - fadeStart) / fadeTime));
            }
            else if (timer < popEnd)
            {
                scale = 1.5f + 2 * ( popTime/2f - Math.Abs(popTime/2f - timer) / popTime/2f);
                if(good)
                    frontColor = new Color((byte)(255f * timer / popTime), 255, (byte)(255f * timer / popTime));
                else
                    frontColor = new Color(255, (byte)(255f * timer / popTime), (byte)(255f * timer / popTime));
            }
            
            if (timer > lifeTime)
            {
                done = true;
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            drawText(spritebatch, Game1.FONT, text, new Color(35, 55, 58, alpha), new Color(frontColor, alpha), scale, 0, position);
        }

        private void drawText(SpriteBatch sb, SpriteFont font, string text, Color backColor, Color frontColor, float scale, float rotation, Vector2 position)
        {

            //If we want to draw the text from the origin we need to find that point, otherwise you can set all origins to Vector2.Zero.

            Vector2 origin = new Vector2(font.MeasureString(text).X / 2, font.MeasureString(text).Y / 2);

            //These 4 draws are the background of the text and each of them have a certain displacement each way.

            sb.DrawString(font, text, position + new Vector2(1 * scale, 1 * scale),//Here’s the displacement

            backColor,

            rotation,

            origin,

            scale,

            SpriteEffects.None, 1f);

            sb.DrawString(font, text, position + new Vector2(-1 * scale, -1 * scale),

            backColor,

            rotation,

            origin,

            scale,

            SpriteEffects.None, 1f);

            sb.DrawString(font, text, position + new Vector2(-1 * scale, 1 * scale),

            backColor,

            rotation,

            origin,

            scale,

            SpriteEffects.None, 1f);

            sb.DrawString(font, text, position + new Vector2(1 * scale, -1 * scale),

            backColor,

            rotation,

            origin,

            scale,

            SpriteEffects.None, 1f);

            //This is the top layer which we draw in the middle so it covers all the other texts except that displacement.

            sb.DrawString(font, text, position,

            frontColor,

            rotation,

            origin,

            scale,

            SpriteEffects.None, 1f);

        }
    }
}
