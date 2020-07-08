using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;

namespace MoleHillMountain
{
    class TitleScreen : BattleScreen
    {
        Sprite title;
        Sprite subTitle;
        Vector2 titlePosition;
        Texture2D testTexture;
        Microsoft.Xna.Framework.Rectangle screenPosition = new Microsoft.Xna.Framework.Rectangle(0, 0, PikeAndShotGame.SMALL_WIDTH, PikeAndShotGame.SMALL_HEIGHT);

        private float waitTime = 10000f;
        private float fadeTime = 5000f;
        bool fadeFlag = false;
        bool titleFlag = false;
        private int lightBoost = 0;
        byte[] bytes;
        Bitmap bmp;
        System.Drawing.Imaging.BitmapData data;

        private System.Drawing.Rectangle rect;

        private FilterInfoCollection captureDevices;
        Texture2D texture;
        private VideoCaptureDevice videoSource;
        private Microsoft.Xna.Framework.Rectangle barRect;
        private Vector2 subTitleOffset;
        private const float FADE_TIME = 900f;

        public TitleScreen(PikeAndShotGame game)
            : base(game)
        {
            title = new Sprite(PikeAndShotGame.JAMEBOY, new Microsoft.Xna.Framework.Rectangle(0, 0, 54, 15), 108, 30);
            subTitle = new Sprite(PikeAndShotGame.DIRTYRECTANGLES, new Microsoft.Xna.Framework.Rectangle(0, 0, 51, 5), 102, 10);
            subTitleOffset = new Vector2(3, 50);
            titlePosition = new Vector2();
            titlePosition.X = 130 - 54;
            captureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (captureDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(captureDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(newFrameEvent);
                videoSource.Start();
            }
            testTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            Microsoft.Xna.Framework.Color[] colors = { Microsoft.Xna.Framework.Color.White };
            testTexture.SetData(colors);
            barRect = new Microsoft.Xna.Framework.Rectangle(0, 0, PikeAndShotGame.SMALL_WIDTH, 1);
        }

        private void newFrameEvent(object sender, NewFrameEventArgs eventArgs)
        {
            if (!fadeFlag)
            {
                bmp = (Bitmap)eventArgs.Frame.Clone();

                rect.Width = bmp.Width;
                rect.Height = bmp.Height;

                data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                bytes = new byte[data.Height * data.Width * 4];


                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                // Unlock the bits.
                bmp.UnlockBits(data);
                bool blueMatch;
                bool greenMatch;
                bool redMatch;
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    //blueMatch = bytes[i] < 90;
                    //greenMatch = bytes[i+1] < 95;
                    //redMatch = bytes[i + 2] > 170;
                    //if (blueMatch && greenMatch && redMatch)
                    //{
                    //    bytes[i] = bytes[i + 1] = bytes[i + 2] = 255;
                    //}
                    //else
                    //{
                    //    blueMatch = true;
                    //}
                    byte avg = (byte)((bytes[i] + bytes[i + 1] + bytes[i + 2]) * 0.333);
                    //light boost
                    if (avg < 255 - lightBoost)
                    {
                        avg += (byte)lightBoost;
                    }

                    avg = (byte)(avg * 0.023);
                    int temp = (byte)((avg + 1) * 42);
                    if (temp > 255)
                    {
                        avg = 255;
                    }
                    else
                    {
                        avg = (byte)temp;
                    }


                    bytes[i] = bytes[i + 1] = bytes[i + 2] = avg;
                }

                if (texture == null)
                {
                    texture = new Texture2D(_game.GraphicsDevice, bmp.Width, bmp.Height);
                }
                texture.SetData<byte>(bytes, 0, data.Height * data.Width * 4);
            }
        }

        public override void update(GameTime gameTime)
        {
            getInput(gameTime.ElapsedGameTime);
            if (titleFlag)
            {
                if (waitTime >= 0)
                {
                    waitTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (fadeTime > 0)
                    {
                        fadeTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (fadeTime < 0)
                        {
                            PikeAndShotGame.GAMEBOY_SOUND.CreateInstance().Play();
                        }
                    }
                    else
                    {
                        fadeTime = 0f;
                    }

                }
                else
                {
                    titleFlag = false;
                }

                if (fadeTime > 0)
                {
                    titlePosition.Y = 65 - 65 * (fadeTime / 2000f);
                }
                else
                {
                    titlePosition.Y = 65;
                }

            }
            else if (fadeFlag)
            {
                if (waitTime > 0)
                {
                    waitTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (waitTime < 0)
                    {
                        waitTime = 0;
                    }
                }
            }
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

        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (texture != null && !titleFlag)
            {
                spriteBatch.Draw(texture, screenPosition, Microsoft.Xna.Framework.Color.White);
                if (fadeFlag)
                {
                    for (int i = 0; i < PikeAndShotGame.SMALL_HEIGHT; i += 2)
                    {
                        barRect.Y = i;
                        spriteBatch.Draw(testTexture, barRect, Microsoft.Xna.Framework.Color.White);
                    }
                    for (int i = 1; i < PikeAndShotGame.SMALL_HEIGHT; i += 2)
                    {
                        barRect.Y = i;
                        Microsoft.Xna.Framework.Color fadeDrawColor = Microsoft.Xna.Framework.Color.White;
                        byte colorValue = (byte)(255 * (1 - (waitTime / FADE_TIME)));
                        fadeDrawColor = new Microsoft.Xna.Framework.Color(colorValue, colorValue, colorValue, colorValue);
                        spriteBatch.Draw(testTexture, barRect, fadeDrawColor);
                    }
                }
            }
            else
            {
                title.draw(spriteBatch, titlePosition, SIDE_PLAYER);
                if (fadeTime <= 0)
                {
                    subTitle.draw(spriteBatch, titlePosition + subTitleOffset, SIDE_PLAYER);
                }


            }
            //title.draw(spriteBatch, new Vector2(PikeAndShotGame.SCREENWIDTH / 2f, PikeAndShotGame.SCREENHEIGHT / 3f), SIDE_PLAYER);
            //if (fadeTime <= 0)
            //{
            //    spriteBatch.DrawString(PikeAndShotGame.getSpriteFont(), "Press Pike or Shot", new Vector2(450, PikeAndShotGame.SCREENHEIGHT * 2f/ 3f), textColor);
            //}

        }

        protected override void getInput(TimeSpan timeSpan)
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                videoSource.Stop();
                _game.Exit();
            }

            //if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            //{
            //    _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            //}
            //else if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && previousGamePadState.IsButtonUp(Buttons.X)))
            //{
            //    _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            //}

            if ((keyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up)))
            {
                lightBoost += 10;
            }
            else if ((keyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down)))
            {
                lightBoost -= 10;
            }

            if ((keyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P)))
            {
                _game.changePalette();
            }

            if ((keyboardState.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T)))
            {
                titleFlag = true;
                restart();
            }

            if ((keyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F)))
            {
                fadeOut();
            }

            base.getInput(timeSpan);
            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        private void fadeOut()
        {
            if (!fadeFlag)
            {
                waitTime = FADE_TIME;

            }
            fadeFlag = !fadeFlag;
            _game.changeLightFlag(fadeFlag);

        }

        public void restart()
        {
            waitTime = 4000f;
            fadeTime = 2000f;
        }
    }
}
