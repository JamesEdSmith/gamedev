using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace PikeAndShot
{
    class TitleScreen : BattleScreen
    {

        Sprite title;
        float waitTime = 2000f;
        float fadeTime = 1000f;
        float animationTime = 1000f;
        float textTime = 1000f;
        float shakeTime = 275f;
        float diskTime = 300f;
        float textTimer;
        float textTimer2;

        Color textColor;

        Vector2 girlPosition = new Vector2(117, 6);
        Vector2 currentGirlPosition = new Vector2(117, 6);
        Vector2 bgPostion = new Vector2(0, 0);

        int stage = 0;
        Texture2D flashSprite;
        Texture2D blackTexture;
        Texture2D redTexture;

        public TitleScreen(PikeAndShotGame game)
            : base(game)
        {
            title = new Sprite(PikeAndShotGame.TITLE_ANIMATION, new Rectangle(253, 122, 253, 122), 506, 244);
            textColor = new Color(0.8f, 0.8f, 0f);
            flashSprite = getFlashTexture(PikeAndShotGame.GIRL);
            flashColor = Color.Transparent;
            blackTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            redTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            Color[] pixels = { Color.Black };
            blackTexture.SetData<Color>(pixels);
            Color[] pixels2 = { Color.Red };
            redTexture.SetData<Color>(pixels2);
        }

        Color flashColor;

        public override void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            //if (waitTime > 0)
            //{
            //    waitTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            //}
            //else if (fadeTime > 0)
            //{
            //    fadeTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            //    int maxFrames = title.getMaxFrames();
            //    float deathFrameTime = animationTime / (float)maxFrames;
            //    int frameNumber = maxFrames - (int)(fadeTime / deathFrameTime) - 1;

            //    title.setFrame(frameNumber);
            //}
            //else
            //{
            //    textTimer -= (float)gameTime.ElapsedGameTime.Milliseconds;
            //    if (textTimer <= 0)
            //    {
            //        textTimer = textTime + textTimer;
            //    }
            //    textColor = new Color(0.8f, 0.8f, 0f, Math.Abs(textTime - textTimer) / (textTime / 2f));
            //}
            if (stage == 1)
            {
                textTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                float time = shakeTime / 2f;
                flashColor = new Color(1f, 1f, 1f, (time - Math.Abs(time - textTimer)) / time);
                currentGirlPosition.X = (int)(girlPosition.X + 5f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds));
                currentGirlPosition.Y = (int)(girlPosition.Y + 5f * (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds));
                if (textTimer <= 0)
                {
                    sourceStr = strs[currStr++];
                    textTimer = textTime;
                    textTimer2 = textTime;
                    currentGirlPosition = girlPosition;
                    stage++;
                }
            }
            else if (stage == 2)
            {
                if (textTimer > 0)
                {
                    textTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (textTimer <= 0)
                    {
                        textTimer2 = textTime + textTimer;
                        textTimer = 0;
                        string1 = sourceStr;
                        sourceStr = strs[currStr++];
                    }
                    else
                    {
                        int length = sourceStr.Length - (int)(sourceStr.Length * textTimer / textTime);
                        string1 = sourceStr.Substring(0, length);
                    }
                }
                else if (textTimer2 > 0)
                {
                    textTimer2 -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (textTimer2 <= 0)
                    {
                        textTimer2 = 0;
                        string2 = sourceStr;
                        sourceStr = strs[currStr++];

                    }
                    else
                    {
                        int length = sourceStr.Length - (int)(sourceStr.Length * textTimer2 / textTime);
                        string2 = sourceStr.Substring(0, length);
                    }
                }

            }
            else if (stage == 3)
            {
                textTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                float time = diskTime / 2f;
                flashColor = new Color(1f, 1f, 1f, (time - Math.Abs(time - textTimer)) / time);
                if (textTimer <= 0)
                {
                    textTimer = diskTime;
                    disk++;
                    if (disk == 1)
                    {
                        textTimer = diskTime * 2;
                        flashSprite = getFlashTexture(PikeAndShotGame.DISK2);
                    }
                    else if (disk == 2)
                    {
                        //textTimer = diskTime = diskTime / 2;
                        flashSprite = getFlashTexture(PikeAndShotGame.DISK3);
                    }
                    else
                    {
                        stage++;
                        textTimer = textTime;
                    }
                }
            }
            else if (stage == 4)
            {
                if (textTimer > 0)
                {
                    textTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (textTimer <= 0)
                    {
                        textTimer = 0;
                        string1 = sourceStr;
                        bgPostion = Vector2.Zero;
                    }
                    else
                    {
                        int length = sourceStr.Length - (int)(sourceStr.Length * textTimer / textTime);
                        string1 = sourceStr.Substring(0, length);
                        bgPostion.X = (int)(5f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds));
                        bgPostion.Y = (int)(5f * (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds));
                    }
                }
            }
        }
        int disk = 0;

        string[] strs = { "Dirty Rectangles Attendee used", "Diskette Storm!", "It's super effective!" };

        string sourceStr;
        string string1 = "";
        string string2 = "";
        int currStr = 0;
        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(PikeAndShotGame.GIRL, currentGirlPosition, new Rectangle(0, 0, 213, 266), Color.White);
            if (stage == 1)
            {
                spriteBatch.Draw(flashSprite, currentGirlPosition, new Rectangle(0, 0, 213, 266), flashColor);
            }
            else if (stage >= 3)
            {

                if (disk == 0)
                {
                    spriteBatch.Draw(PikeAndShotGame.DISK1, new Vector2(283, 150), Color.White);
                    spriteBatch.Draw(flashSprite, new Vector2(283, 150), flashColor);
                }
                else if (disk == 1)
                {
                    spriteBatch.Draw(PikeAndShotGame.DISK1, new Vector2(283, 150), Color.White);
                    spriteBatch.Draw(PikeAndShotGame.DISK2, new Vector2(275, 36), Color.White);
                    spriteBatch.Draw(flashSprite, new Vector2(275, 36), flashColor);
                }
                else if (disk == 2)
                {
                    spriteBatch.Draw(PikeAndShotGame.DISK1, new Vector2(283, 150), Color.White);
                    spriteBatch.Draw(PikeAndShotGame.DISK2, new Vector2(275, 36), Color.White);
                    spriteBatch.Draw(PikeAndShotGame.DISK3, new Vector2(72, 18), Color.White);
                    spriteBatch.Draw(flashSprite, new Vector2(72, 18), flashColor);
                }
                else
                {
                    spriteBatch.Draw(PikeAndShotGame.DISK1, new Vector2(283, 150), Color.White);
                    spriteBatch.Draw(PikeAndShotGame.DISK2, new Vector2(275, 36), Color.White);
                    spriteBatch.Draw(PikeAndShotGame.DISK3, new Vector2(72, 18), Color.White);
                }
            }


            //title.draw(spriteBatch, new Vector2(PikeAndShotGame.SCREENWIDTH / 2f, PikeAndShotGame.SCREENHEIGHT / 3f), SIDE_PLAYER);
            //if (fadeTime <= 0)
            //{
            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), string1, new Vector2(20, 300), Color.White);
            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), string2, new Vector2(20, 350), Color.White);

            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Magic", new Vector2(490, 102), Color.White);
            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Chat", new Vector2(490, 142), Color.White);
            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Item", new Vector2(490, 182), Color.White);

            //if (gameTime.TotalGameTime.Milliseconds % 250 < 112)
            //{
            //    spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Fight", new Vector2(490, 62), textColor);
            //    spriteBatch.Draw(PikeAndShotGame.STAR, new Rectangle(575, 57, 29, 32), new Rectangle(0, 0, 29, 32), Color.White);
            //}
            //else
            //{
            spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Fight", new Vector2(490, 62), Color.White);
            //}

            if (stage == 2 && textTimer2 <= 0)
            {
                if (gameTime.TotalGameTime.Milliseconds % 250 < 112)
                {
                    spriteBatch.Draw(PikeAndShotGame.ARROW_DOWN, new Vector2(390, 370), Color.White);
                }
            }

            //}
            spriteBatch.Draw(PikeAndShotGame.TEST, bgPostion, Color.White);
            if (stage == 4)
            {
                int amount = (int)(50 * (textTimer / textTime));
                Color white = new Color(1.0f, 1.0f, 1.0f, 1f - 1f * (textTimer / textTime));

                spriteBatch.Draw(redTexture, new Rectangle(488 + (int)bgPostion.X, 340 + (int)bgPostion.Y, 93, 10), white);
                spriteBatch.Draw(blackTexture, new Rectangle(531 + amount + (int)bgPostion.X, 340 + (int)bgPostion.Y, 50 - amount, 10), Color.White);

            }
        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                _game.Exit();
            if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }
            if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && !gamePadState.IsButtonDown(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }

            if ((keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter)))
            {
                switch (stage)
                {
                    case 0:
                        textTimer = shakeTime;
                        stage++;
                        break;
                    case 2:
                        textTimer = diskTime;
                        string1 = string2 = "";
                        stage++;
                        flashSprite = getFlashTexture(PikeAndShotGame.DISK1);
                        break;
                    default:
                        break;
                }

            }

            base.getInput(timeSpan);
            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        public void restart()
        {
            waitTime = 2000f;
            fadeTime = 1000f;
            title.setFrame(0);
        }
    }
}
