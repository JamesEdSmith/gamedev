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

namespace PikeAndShot
{
    class TitleScreen : BattleScreen
    {
        Sprite title;

        private float waitTime = 2000f;
        private float fadeTime = 1000f;

        private System.Drawing.Rectangle rect;

        private FilterInfoCollection captureDevices;
        Texture2D texture;
        private VideoCaptureDevice videoSource;

        public TitleScreen(PikeAndShotGame game)
            : base(game)
        {
            title = new Sprite(PikeAndShotGame.TITLE_ANIMATION, new Microsoft.Xna.Framework.Rectangle(253, 122, 253, 122), 506, 244);
            captureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (captureDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(captureDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(newFrameEvent);
                videoSource.Start();
            }

        }

        private void newFrameEvent(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();

            rect.Width = bmp.Width;
            rect.Height = bmp.Height;

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(rect,
            System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] bytes = new byte[data.Height * data.Width * 4];

            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            // Unlock the bits.
            bmp.UnlockBits(data);
            if (texture == null)
            {
                texture = new Texture2D(_game.GraphicsDevice, bmp.Width, bmp.Height);
            }
            texture.SetData<byte>(bytes, 0, data.Height * data.Width * 4);

        }

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

        }

        public override void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, rect.Width, rect.Height), Microsoft.Xna.Framework.Color.White);
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
            if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
            }
            if ((keyboardState.IsKeyDown(Keys.X) && !keyboardState.IsKeyDown(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && !gamePadState.IsButtonDown(Buttons.A)))
            {
                _game.setScreen(PikeAndShotGame.SCREEN_LEVELPLAY);
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
