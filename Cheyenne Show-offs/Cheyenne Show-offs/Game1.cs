using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Cheyenne_Show_offs
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const int SCREENWIDTH = 1024;
        public const int SCREENHEIGHT = 768;
        public const bool VERTICAL_SCREEN = false;

        public enum modes {splash, title, duel};

        public static Texture2D IMAGE_ANVIL;
        public static Texture2D IMAGE_DOT;
        public static Texture2D IMAGE_GUN;
        public static Texture2D IMAGE_GUN_IN_HAND;
        public static Texture2D IMAGE_HAMMER;
        public static Texture2D IMAGE_TRIGGER;
        public static Texture2D IMAGE_ARM;
        public static Texture2D IMAGE_UPPER_ARM;
        public static Texture2D IMAGE_FORE_ARM;
        public static Texture2D IMAGE_SPIN_HAND;
        public static Texture2D IMAGE_FIST_HAND;
        public static Texture2D IMAGE_OPEN_HAND;
        public static Texture2D IMAGE_BODY;
        public static Texture2D IMAGE_HEAD;
        public static Texture2D IMAGE_HOLSTER;
        public static Texture2D IMAGE_LEFTARM;
        public static Texture2D IMAGE_LEFTLEG;
        public static Texture2D IMAGE_RIGHTLEG;

        public static Texture2D IMAGE_GUN2;
        public static Texture2D IMAGE_GUN_IN_HAND2;
        public static Texture2D IMAGE_HAMMER2;
        public static Texture2D IMAGE_TRIGGER2;
        public static Texture2D IMAGE_ARM2;
        public static Texture2D IMAGE_UPPER_ARM2;
        public static Texture2D IMAGE_FORE_ARM2;
        public static Texture2D IMAGE_SPIN_HAND2;
        public static Texture2D IMAGE_FIST_HAND2;
        public static Texture2D IMAGE_OPEN_HAND2;
        public static Texture2D IMAGE_BODY2;
        public static Texture2D IMAGE_HEAD2;
        public static Texture2D IMAGE_HOLSTER2;
        public static Texture2D IMAGE_LEFTARM2;
        public static Texture2D IMAGE_LEFTLEG2;
        public static Texture2D IMAGE_RIGHTLEG2;

        public static Texture2D IMAGE_BULLET;
        public static Texture2D IMAGE_ARROW;

        public static Texture2D IMAGE_BG;
        public static Texture2D IMAGE_TITLE;
        public static SpriteFont FONT;
        public static Song MUSIC_BG;
        public static Song MUSIC_BG2;
        public static Song MUSIC_BG3;

        public static SoundEffect SOUND_COCK;
        public static SoundEffect SOUND_TRIGGER;
        public static SoundEffect SOUND_WHIRL;
        public static SoundEffect SOUND_TWIRL;
        public static SoundEffect SOUND_SHOOT;
        public static SoundEffect SOUND_RICOCHET;
        public static SoundEffect SOUND_HOLSTER1;
        public static SoundEffect SOUND_HOLSTER2;
        public static SoundEffect SOUND_GOOD;
        public static SoundEffect SOUND_BAD;
        public static SoundEffect SOUND_THROW;
        public static SoundEffect SOUND_DROP;
        public static SoundEffect SOUND_DEAD;
        public static SoundEffect SOUND_ANVIL;
        public static SoundEffect SOUND_RING;

        public modes mode;
        public DuelScreen p1Screen;
        public DuelScreen p2Screen;
        public MenuScreen mainScreen;
        public SplashScreen splashScreen;

        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Color renderColor;
        public Camera main1Camera;
        public Camera main2Camera;
        public Camera p1Camera;
        public Camera p2Camera;
        public Viewport mainView;
        public Viewport p1View;
        public Viewport p2View;
        public RenderTarget2D tempTarget;

        public bool debugText;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREENWIDTH;
            graphics.PreferredBackBufferHeight = SCREENHEIGHT;
            graphics.IsFullScreen = false;

            graphics.PreferMultiSampling = false;
            Content.RootDirectory = "Content";

            mode = modes.splash;
            main1Camera = new Camera(PlayerIndex.One, this);
            main2Camera = new Camera(PlayerIndex.Two, this);
            p1Camera = new Camera(PlayerIndex.One, this);
            p2Camera = new Camera(PlayerIndex.Two, this);

            main1Camera.matrix = Matrix.Identity;
            main2Camera.matrix = Matrix.Identity;
            p1Camera.matrix = Matrix.Identity;
            p2Camera.matrix = Matrix.Identity;
            setCameras(null);

            debugText = false;
        }

        public void setCameras(Camera camera)
        {
            float twoThirds = 2f / 3f;

            if (VERTICAL_SCREEN)
            {
                if (camera == null)
                {
                    p1Camera.matrix *= Matrix.CreateScale(0.5f);
                    main1Camera.matrix *= Matrix.CreateScale(0.5f);

                    p2Camera.matrix *= Matrix.CreateScale(0.5f);
                    main2Camera.matrix *= Matrix.CreateScale(0.5f);
                }
                else
                {
                    camera.matrix *= Matrix.CreateScale(0.5f);
                }
            }
            else
            {
                if (camera == null)
                {
                    p1Camera.matrix *= Matrix.CreateScale(twoThirds);
                    p1Camera.matrix *= Matrix.CreateTranslation((float)SCREENWIDTH * twoThirds / -2, (float)SCREENHEIGHT * twoThirds / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi / 2f) * Matrix.CreateTranslation((float)SCREENHEIGHT * twoThirds / 2f, (float)SCREENWIDTH * twoThirds / 2f, 0);
                    main1Camera.matrix = Matrix.CreateScale(twoThirds);
                    main1Camera.matrix *= Matrix.CreateTranslation((float)SCREENWIDTH * twoThirds / -2, (float)SCREENHEIGHT * twoThirds / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi / 2f) * Matrix.CreateTranslation((float)SCREENHEIGHT * twoThirds / 2f, (float)SCREENWIDTH * twoThirds / 2f, 0);

                    p2Camera.matrix *= Matrix.CreateScale(twoThirds);
                    p2Camera.matrix *= Matrix.CreateRotationZ(MathHelper.Pi / -2f) * Matrix.CreateTranslation(0, (float)SCREENWIDTH * twoThirds, 0);
                    main2Camera.matrix *= Matrix.CreateScale(twoThirds);
                    main2Camera.matrix *= Matrix.CreateRotationZ(MathHelper.Pi / -2f) * Matrix.CreateTranslation(0, (float)SCREENWIDTH * twoThirds, 0);
                }
                else if (camera == p1Camera || camera == main1Camera)
                {
                    camera.matrix *= Matrix.CreateScale(twoThirds);
                    camera.matrix *= Matrix.CreateTranslation((float)SCREENWIDTH * twoThirds / -2, (float)SCREENHEIGHT * twoThirds / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi / 2f) * Matrix.CreateTranslation((float)SCREENHEIGHT * twoThirds / 2f, (float)SCREENWIDTH * twoThirds / 2f, 0);
                }
                else if (camera == p2Camera || camera == main2Camera)
                {
                    camera.matrix *= Matrix.CreateScale(twoThirds);
                    camera.matrix *= Matrix.CreateRotationZ(MathHelper.Pi / -2f) * Matrix.CreateTranslation(0, (float)SCREENWIDTH * twoThirds, 0);
                }
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //make viewports
            if (VERTICAL_SCREEN)
            {
                mainView = p1View = p2View = GraphicsDevice.Viewport;
                p1View.Height = p2View.Height = p1View.Height / 2;
                p2View.Y = p1View.Height;
            }
            else
            {
                mainView = p1View = p2View = GraphicsDevice.Viewport;
                p1View.Width = p2View.Width = p1View.Width / 2;
                p2View.X = p1View.Width;
            }

            //render target for flipping the player two screen for UPRIGHT VIEW
            tempTarget = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, 1,
            GraphicsDevice.DisplayMode.Format, GraphicsDevice.PresentationParameters.MultiSampleType,
            GraphicsDevice.PresentationParameters.MultiSampleQuality, RenderTargetUsage.PreserveContents);

            IMAGE_BG = Content.Load<Texture2D>("BG");
            IMAGE_TITLE = Content.Load<Texture2D>("Title");
            IMAGE_ANVIL = Content.Load<Texture2D>("anvi");

            IMAGE_GUN = Content.Load<Texture2D>("Colt");
            IMAGE_GUN_IN_HAND = Content.Load<Texture2D>("gun1");
            IMAGE_HAMMER = Content.Load<Texture2D>("ColtHammer");
            IMAGE_TRIGGER = Content.Load<Texture2D>("ColtTrigger");
            IMAGE_ARM = Content.Load<Texture2D>("dumb_arm");
            IMAGE_UPPER_ARM = Content.Load<Texture2D>("CB_UpperArm");
            IMAGE_FORE_ARM = Content.Load<Texture2D>("CB_GunForearm");
            IMAGE_BODY = Content.Load<Texture2D>("CB_Body");
            IMAGE_HEAD = Content.Load<Texture2D>("CB_Head");
            IMAGE_HOLSTER = Content.Load<Texture2D>("CB_Holster");
            IMAGE_LEFTARM = Content.Load<Texture2D>("CB_LeftArm");
            IMAGE_LEFTLEG = Content.Load<Texture2D>("CB_LeftLeg");
            IMAGE_RIGHTLEG = Content.Load<Texture2D>("CB_RightLeg");
            IMAGE_FIST_HAND = Content.Load<Texture2D>("CB1_Fisthand");
            IMAGE_OPEN_HAND = Content.Load<Texture2D>("CB1_Openhand");
            IMAGE_SPIN_HAND = Content.Load<Texture2D>("CB1_Spinhand");

            IMAGE_GUN2 = Content.Load<Texture2D>("SW_Body");
            IMAGE_GUN_IN_HAND2 = Content.Load<Texture2D>("gun2");
            IMAGE_HAMMER2 = Content.Load<Texture2D>("SW_Hammer");
            IMAGE_TRIGGER2 = Content.Load<Texture2D>("SW_Trigger");
            IMAGE_ARM2 = Content.Load<Texture2D>("dumb_arm");
            IMAGE_UPPER_ARM2 = Content.Load<Texture2D>("CB2_UpperArm");
            IMAGE_FORE_ARM2 = Content.Load<Texture2D>("CB2_GunForearm");
            IMAGE_BODY2 = Content.Load<Texture2D>("CB2_Body");
            IMAGE_HEAD2 = Content.Load<Texture2D>("CB2_Head");
            IMAGE_HOLSTER2 = Content.Load<Texture2D>("CB2_Holster");
            IMAGE_LEFTARM2 = Content.Load<Texture2D>("CB2_RightArm");
            IMAGE_LEFTLEG2 = Content.Load<Texture2D>("CB2_LeftLeg");
            IMAGE_RIGHTLEG2 = Content.Load<Texture2D>("CB2_RightLeg");
            IMAGE_FIST_HAND2 = Content.Load<Texture2D>("CB2_Fisthand");
            IMAGE_OPEN_HAND2 = Content.Load<Texture2D>("CB2_Openhand");
            IMAGE_SPIN_HAND2 = Content.Load<Texture2D>("CB2_Spinhand");

            FONT = Content.Load<SpriteFont>("SpriteFont1");
            IMAGE_DOT = Content.Load<Texture2D>("dot");
            IMAGE_BULLET = Content.Load<Texture2D>("bullet");
            IMAGE_ARROW = Content.Load<Texture2D>("red_arrow");

            MUSIC_BG = Content.Load<Song>("Ennio Morricone Two Mules for Sister Sara");
            MUSIC_BG2 = Content.Load<Song>("For A Few Dollars More Theme (Ennio Morricone)");
            MUSIC_BG3 = Content.Load<Song>("Mexican Music");
            SOUND_COCK = Content.Load<SoundEffect>("cock");
            SOUND_TRIGGER = Content.Load<SoundEffect>("trigger");
            SOUND_TWIRL = Content.Load<SoundEffect>("twirl");
            SOUND_WHIRL = Content.Load<SoundEffect>("whirl");
            SOUND_SHOOT = Content.Load<SoundEffect>("shoot");
            SOUND_RICOCHET = Content.Load<SoundEffect>("ricochet");
            SOUND_HOLSTER1 = Content.Load<SoundEffect>("holster1");
            SOUND_HOLSTER2 = Content.Load<SoundEffect>("holster2");
            SOUND_GOOD = Content.Load<SoundEffect>("good_sound");
            SOUND_BAD = Content.Load<SoundEffect>("bad_sound");
            SOUND_THROW = Content.Load<SoundEffect>("throw_sound");
            SOUND_DROP = Content.Load<SoundEffect>("drop_sound");
            SOUND_DEAD = Content.Load<SoundEffect>("dead");
            SOUND_ANVIL = Content.Load<SoundEffect>("anvil");
            SOUND_RING = Content.Load<SoundEffect>("RingingForJames");

            //p1Screen = new DuelScreen(this, PlayerIndex.One);
            //p2Screen = new DuelScreen(this, PlayerIndex.Two);
            splashScreen = new SplashScreen(this);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            input(gameTime);

            if (mode == modes.duel)
            {
                p1Screen.update(gameTime);
                p2Screen.update(gameTime);
            }
            else if( mode == modes.title)
                mainScreen.update(gameTime);
            else
                splashScreen.update(gameTime);
            base.Update(gameTime);
        }

        private void input(GameTime time)
        {
            if (mode == modes.duel)
            {
                p1Screen.input(time);
                p2Screen.input(time);
            }
            else if (mode == modes.title)
                mainScreen.input(time);
            else
                splashScreen.input(time);
        }

        protected override void Draw(GameTime gameTime)
        {
            //early drawing to texture for player 2's screen if in UPRIGHT display mode
            if (VERTICAL_SCREEN && mode == modes.duel)
            {
                GraphicsDevice.Viewport = p2View;
                GraphicsDevice.SetRenderTarget(0,tempTarget);
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                p2Screen.draw(gameTime, spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                //if (p2Screen.gunWindow != null)
                //    p2Screen.gunWindow.draw(spriteBatch);
                spriteBatch.End();               
                GraphicsDevice.SetRenderTarget(0,null);
            }
            
            GraphicsDevice.Viewport = mainView;
            GraphicsDevice.Clear(Color.Black);

            //Random rand = new Random(gameTime.TotalGameTime.Seconds);
            //int value = rand.Next();
            //renderColor = new Color((value + gameTime.TotalGameTime.Seconds)% 255, (value + gameTime.TotalGameTime.Milliseconds) % 255, (value + gameTime.TotalGameTime.Minutes) % 255, 255);
            if (mode == modes.splash)
            {
                GraphicsDevice.Viewport = mainView;
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                splashScreen.draw(gameTime, spriteBatch);
                spriteBatch.End();
            }
            if (mode == modes.title)
            {
                GraphicsDevice.Viewport = mainView;
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                mainScreen.draw(gameTime, spriteBatch);
                spriteBatch.End();
            }
            //screen 1
            if (mode == modes.duel)
            {
                GraphicsDevice.Viewport = p1View;
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, p1Camera.getMatrix());
                //graphics.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.None;
                p1Screen.draw(gameTime, spriteBatch);
                spriteBatch.End();
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, main1Camera.getMatrix());
                //graphics.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.None;
                if (p1Screen.gunWindow != null)
                    p1Screen.gunWindow.draw(spriteBatch);

                // score display
                Vector2 strLength = Game1.FONT.MeasureString("Trick Bank: $" + (int)p1Screen.P1Score.score);
                foreach (TextPopup tp in p1Screen.textPopups)
                    tp.draw(spriteBatch);
                //spriteBatch.DrawString(Game1.FONT, "Trick Bank: $" + (int)p1Screen.P1Score.score, new Vector2(SCREENWIDTH/2 - strLength.X/2, 50), Color.Black);
                drawText(spriteBatch, Game1.FONT, "Trick Bank: $" + (int)p1Screen.P1Score.score, new Color(35,55,58), Color.White, 1.5f, 0, new Vector2(SCREENWIDTH / 2, 100));
                //debuginfo
                if (debugText)
                {
                    spriteBatch.DrawString(Game1.FONT, "aVel: " + p1Screen.gun.aVelocity, new Vector2(50, 50), Color.Black);
                    spriteBatch.DrawString(Game1.FONT, "turn: " + p1Screen.P1Score.turnAmount, new Vector2(50, 100), Color.Black);
                }
                spriteBatch.End();
            }            

            //screen 2
            if (mode == modes.duel)
            {
                GraphicsDevice.Viewport = p2View;

                if (!VERTICAL_SCREEN)
                {
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, p2Camera.getMatrix());
                    //graphics.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.None;
                    p2Screen.draw(gameTime, spriteBatch);
                    spriteBatch.End();
                }
                else
                {
                    spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, p2Camera.getMatrix());
                    //graphics.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.None;
                    spriteBatch.Draw(tempTarget.GetTexture(), new Rectangle(0, 0, SCREENWIDTH, SCREENHEIGHT), new Rectangle(0, 0, SCREENWIDTH, SCREENHEIGHT), Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                    spriteBatch.End();
                }
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, main2Camera.getMatrix());
                //graphics.GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.None;
                if (p2Screen.gunWindow != null)
                    p2Screen.gunWindow.draw(spriteBatch);

                // score display
                Vector2 strLength = Game1.FONT.MeasureString("Trick Bank: $" + (int)p2Screen.P1Score.score);
                //spriteBatch.DrawString(Game1.FONT, "Trick Bank: $" + (int)p2Screen.P1Score.score, new Vector2(SCREENWIDTH / 2 - strLength.X / 2, 50), Color.Black);
                drawText(spriteBatch, Game1.FONT, "Trick Bank: $" + (int)p2Screen.P1Score.score, new Color(35, 55, 58), Color.White, 1.5f, 0, new Vector2(SCREENWIDTH / 2, 100));
                foreach (TextPopup tp in p2Screen.textPopups)
                    tp.draw(spriteBatch);
                //debuginfo
                if (debugText)
                {
                    spriteBatch.DrawString(Game1.FONT, "aVel: " + p1Screen.gun.aVelocity, new Vector2(50, 50), Color.Black);
                    spriteBatch.DrawString(Game1.FONT, "turn: " + p1Screen.P1Score.turnAmount, new Vector2(50, 100), Color.Black);
                }
                spriteBatch.End();
            }
            

            base.Draw(gameTime);
        }

        public void drawText(SpriteBatch sb, SpriteFont font, string text, Color backColor, Color frontColor, float scale, float rotation, Vector2 position)
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

        internal void swapBullet(Bullet b, DuelScreen screen)
        {
            b.rotation = mirrorAngle(b.rotation);
            if (screen == p1Screen)
                ((DuelScreen)p2Screen).firedBullets.Add(b);
            else if (screen == p2Screen)
                ((DuelScreen)p1Screen).firedBullets.Add(b);
        }

        private float mirrorAngle(float p)
        {
            float angle = MathHelper.WrapAngle(p);
            float pi2 = (float)Math.PI / 2f;
            if (angle > 0f)
            {
                if (angle < pi2)
                    angle += (pi2 - angle) * 2f;
                else
                    angle -= (angle - pi2) * 2f;
            }
            else
            {
                if (angle > -pi2)
                    angle += (pi2 - angle) * 2f;
                else
                    angle -= (angle - pi2) * 2f;
            }

            return angle;
        }

        internal bool getEnemyGunState(DuelScreen screen)
        {
            if (screen == p1Screen)
                return !p2Screen.gun.holstered && !p2Screen.droppedGun;
            else
                return !p1Screen.gun.holstered && !p1Screen.droppedGun;
        }

        internal void setMode(modes mode)
        {
            MediaPlayer.Stop();
            this.mode = mode;
            splashScreen = null;
            if (mode == modes.duel)
            {
                p1Screen = new DuelScreen(this, PlayerIndex.One);
                p2Screen = new DuelScreen(this, PlayerIndex.Two);
                MediaPlayer.Play(MUSIC_BG2);
            }
            else if (mode == modes.title)
            {
                mainScreen = new MenuScreen(this);
                MediaPlayer.Play(MUSIC_BG);
            }
        }
    }
}