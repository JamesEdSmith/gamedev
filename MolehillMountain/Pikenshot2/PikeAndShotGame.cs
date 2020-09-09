using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace MoleHillMountain
{
    public class PikeAndShotGame : Microsoft.Xna.Framework.Game
    {
        public static TimeSpan DUMMY_TIMESPAN;
        public const bool DEBUG = false;
        public static bool TEST_BOSS = false;

        public const int SCREENWIDTH = 1024;
        public const int SCREENHEIGHT = 768;

        public const int SCREEN_LEVELPLAY = 0;
        public const int SCREEN_FORMATIONMAKER = 1;
        public const int SCREEN_LEVELEDITOR = 2;

        public const float ROAD_FADE = 0.4f;

        GraphicsDeviceManager graphics;
        public static Viewport viewport;
        SpriteBatch spriteBatch;

        Rectangle testDrawRectangle = new Rectangle(0, 0, SCREENWIDTH, SCREENHEIGHT);
        Rectangle drawRectangle = new Rectangle(0, 0, SCREENWIDTH, SCREENHEIGHT);
        Rectangle fullDrawRectangle = new Rectangle(0, 0, SCREENWIDTH+50, SCREENHEIGHT+50);
        Rectangle screenDrawRectangle = new Rectangle(0, 25, 25, SCREENHEIGHT);
        Rectangle screenDrawRectangle2 = new Rectangle(SCREENWIDTH+25, 25, 25, SCREENHEIGHT);
        Rectangle screenDrawRectangle3 = new Rectangle(0, 22, SCREENWIDTH+50, 3);
        Rectangle screenDrawRectangle4 = new Rectangle(0, SCREENHEIGHT+25, SCREENWIDTH+50, 3);
        Rectangle drawSourceRectangle = new Rectangle(2, 2, SCREENWIDTH, SCREENHEIGHT);

        RenderTarget2D ShaderRenderTarget;
        RenderTarget2D ShaderRenderTarget2;
        RenderTarget2D ShaderRenderTarget3;
        RenderTarget2D ShaderRenderTarget4;

        static SpriteFont soldierFont;
        public static Random random = new Random();
        public static bool useShaders = false;

        //public LevelConstructorForm _form;

        public static Effect effect;
        public static Effect effect2;
        public static Effect effect3;

        public static Texture2D TERRAIN_DRY_GRASS;

        public static Texture2D PUCELLE_IDLE;
        public static Texture2D PUCELLE_MOTION;
        public static Texture2D PUCELLE_ROUTED;
        public static Texture2D PUCELLE_DROP;

        public static Texture2D PIKEMAN_FEET;
        public static Texture2D PIKEMAN_DROP;
        public static Texture2D PIKEMAN_IDLE;
        public static Texture2D PIKEMAN_LOWER_LOW;
        public static Texture2D PIKEMAN_LOWER_HIGH;
        public static Texture2D PIKEMAN_RECOIL;
        public static Texture2D PIKEMAN_DEATH;
        public static Texture2D PIKEMAN1_IDLE;
        public static Texture2D PIKEMAN1_LOWER_LOW;
        public static Texture2D PIKEMAN1_LOWER_HIGH;
        public static Texture2D PIKEMAN1_RECOIL;
        public static Texture2D PIKEMAN1_DEATH;
        public static Texture2D PIKEMAN1_MELEE;
        public static Texture2D PIKEMAN1_DEFEND;
        public static Texture2D PIKEMAN2_IDLE;
        public static Texture2D PIKEMAN2_LOWER_LOW;
        public static Texture2D PIKEMAN2_LOWER_HIGH;
        public static Texture2D PIKEMAN2_RECOIL;
        public static Texture2D PIKEMAN2_DEATH;
        public static Texture2D PIKEMAN2_MELEE;
        public static Texture2D PIKEMAN2_DEFEND;
        public static Texture2D PIKEMAN_MELEE;
        public static Texture2D PIKEMAN_ROUTE;
        public static Texture2D PIKEMAN_ROUTED;
        public static Texture2D PIKEMAN2_ROUTED;
        public static Texture2D PIKEMAN_TUG;
        public static Texture2D PIKEMAN2_TUG;
        public static Texture2D WOLF2_TUG;
        public static Texture2D WOLF_TUG;
        public static Texture2D WOLF2_TUGg;
        public static Texture2D WOLF_TUGg;

        public static Texture2D ARQUEBUSIER_FEET;
        public static Texture2D ARQUEBUSIER_IDLE;
        public static Texture2D ARQUEBUSIER_DEATH;
        public static Texture2D ARQUEBUSIER_MELEE;
        public static Texture2D ARQUEBUSIER_DEFEND;
        public static Texture2D ARQUEBUSIER_RELOAD;
        public static Texture2D ARQUEBUSIER_SHOOT;
        public static Texture2D ARQUEBUSIER_SMOKE;
        public static Texture2D ARQUEBUSIER_ROUTE;
        public static Texture2D ARQUEBUSIER_ROUTED;
        public static Texture2D ARQUEBUSIER2_ROUTED;
        public static Texture2D ARQUEBUSIER_SHOT1;
        public static Texture2D ARQUEBUSIER_SHOT2;
        public static Texture2D ARQUEBUSIER_SHOT3;
        public static Texture2D ARQUEBUSIER_GROUND;

        public static Texture2D ARQUEBUSIER_FEET2;
        public static Texture2D ARQUEBUSIER_IDLE2;
        public static Texture2D ARQUEBUSIER_DEATH2;
        public static Texture2D ARQUEBUSIER_MELEE2;
        public static Texture2D ARQUEBUSIER_DEFEND2;
        public static Texture2D ARQUEBUSIER_RELOAD2;
        public static Texture2D ARQUEBUSIER_SHOOT2;
        public static Texture2D ARQUEBUSIER_SMOKE2;
        public static Texture2D ARQUEBUSIER_DROP;
        public static Texture2D ARQUEBUSIER2_DROP;

        public static Texture2D CROSSBOWMAN_IDLE;
        public static Texture2D CROSSBOWMAN_DEATH;
        public static Texture2D CROSSBOWMAN_MELEE;
        public static Texture2D CROSSBOWMAN_MELEE2;
        public static Texture2D CROSSBOWMAN_DEFEND2;
        public static Texture2D CROSSBOWMAN_RELOAD;
        public static Texture2D CROSSBOWMAN_RELOAD2;
        public static Texture2D CROSSBOWMAN_SHOOT;
        public static Texture2D CROSSBOWMAN_BOLT;
        public static Texture2D CROSSBOWMAN_BOLT2;
        public static Texture2D CROSSBOWMAN_GROUND;
        public static Texture2D CROSSBOWMAN_ROUTE;
        public static Texture2D CROSSBOWMAN_ROUTED;
        public static Texture2D CROSSBOWMAN_PAVISE;
        public static Texture2D CROSSBOWMAN_PAVISE_PLACE;
        public static Texture2D CROSSBOWMAN_SHIELDBREAK;

        public static Texture2D PLACED_PAVISE;
        public static Texture2D PAVISE_FALL;

        public static Texture2D DOPPLE_IDLE;
        public static Texture2D DOPPLE_DEATH;
        public static Texture2D DOPPLE_SWING1;
        public static Texture2D DOPPLE_RELOAD1;
        public static Texture2D DOPPLE_ROUTE;
        public static Texture2D DOPPLE_ROUTED;

        public static Texture2D SOLDIER_FEET;
        public static Texture2D SOLDIER_IDLE;
        public static Texture2D SOLDIER2_IDLE;
        public static Texture2D SOLDIER_DEATH;
        public static Texture2D SOLDIER2_DEATH;
        public static Texture2D SOLDIER_MELEE1;
        public static Texture2D SOLDIER_DEFEND1;
        public static Texture2D SOLDIER_MELEE2;
        public static Texture2D SOLDIER2_MELEE2;
        public static Texture2D SOLDIER_DEFEND2;
        public static Texture2D SOLDIER2_DEFEND2;
        public static Texture2D SOLDIER_ROUTE;
        public static Texture2D SOLDIER_ROUTED;
        public static Texture2D SOLDIER2_ROUTED;
        public static Texture2D SOLDIER_SHIELDBREAK;
        public static Texture2D SOLDIER_IDLENOSHIELD;
        public static Texture2D SOLDIER_BROKENSHIELD1;
        public static Texture2D SOLDIER_BROKENSHIELD2;
        public static Texture2D SOLDIER_BROKENARMOUR;
        public static Texture2D SOLDIER_FALL;
        public static Texture2D SOLDIER_CHARGE;
        public static Texture2D SOLDIER_CHARGENOSHIELD;

        public static Texture2D CAVALRY_HORSE_IDLE;
        public static Texture2D CAVALRY_HORSE_RUN;
        public static Texture2D CAVALRY_HORSE_HALT;
        public static Texture2D CAVALRY_HORSE_TURN;
        public static Texture2D CAVALRY_HORSE_DEATH;
        public static Texture2D CAVALRY_FALL;
        public static Texture2D CAVALRY_LEFTFOOT;
        public static Texture2D CAVALRY_RIGHTFOOT;
        public static Texture2D CAVALRY_LEFTIDLE;
        public static Texture2D CAVALRY_RIGHTIDLE;
        public static Texture2D CAVALRY_LEFTLOWER;
        public static Texture2D CAVALRY_RIGHTLOWER;
        public static Texture2D CAVALRY_LEFTRECOIL;
        public static Texture2D CAVALRY_RIGHTRECOIL;

        public static Texture2D DISMOUNTED_CAVALRY_IDLE;
        public static Texture2D DISMOUNTED_CAVALRY_DEATH;
        public static Texture2D DISMOUNTED_CAVALRY_IDLENOSHIELD;
        public static Texture2D DISMOUNTED_CAVALRY_SHIELDBREAK;
        public static Texture2D DISMOUNTED_CAVALRY_FALL;
        public static Texture2D DISMOUNTED_CAVALRY_MELEE1;
        public static Texture2D DISMOUNTED_CAVALRY_DEFEND1;

        public static Texture2D SLINGER_IDLE;
        public static Texture2D SLINGER_DEATH;
        public static Texture2D SLINGER_MELEE;
        public static Texture2D SLINGER_DEFEND;
        public static Texture2D SLINGER_RELOAD;
        public static Texture2D SLINGER_SHOOT;
        public static Texture2D SLINGER_ROCK;
        public static Texture2D GOBLIN_FEET;
        public static Texture2D SLINGER_GROUND;
        public static Texture2D SLINGER_ROUTE;
        public static Texture2D SLINGER_ROUTED;
        public static Texture2D SLINGER_RETREAT;

        public static Texture2D SKIRMISHER_IDLE;
        public static Texture2D SKIRMISHER_RELOAD;
        public static Texture2D SKIRMISHER_SHOOT;
        public static Texture2D SKIRMISHER_JAVELIN;
        public static Texture2D SKIRMISHER_GROUND;
        public static Texture2D SKIRMISHER_DEATH;
        public static Texture2D SKIRMISHER_MELEE;
        public static Texture2D SKIRMISHER_DEFEND;
        public static Texture2D SKIRMISHER_RETREAT;

        public static Texture2D BERZERKER_IDLE;
        public static Texture2D BERZERKER_DEATH;
        public static Texture2D BERZERKER_MELEE1;
        public static Texture2D BERZERKER_DEFEND1;
        public static Texture2D BERZERKER_MELEE2;
        public static Texture2D BERZERKER_DEFEND2;
        public static Texture2D BERZERKER_ROUTE;
        public static Texture2D BERZERKER_ROUTED;
        public static Texture2D BERZERKER_IDLENOSHIELD;
        public static Texture2D BERZERKER_SHIELDBREAK;
        public static Texture2D BERZERKER_FALL;
        public static Texture2D BERZERKER_CHARGE;
        public static Texture2D BERZERKER_CHARGENOSHIELD;

        public static Texture2D BERZERKER2_IDLE;
        public static Texture2D BERZERKER2_DEATH;
        public static Texture2D BERZERKER2_MELEE1;
        public static Texture2D BERZERKER2_DEFEND1;
        public static Texture2D BERZERKER2_MELEE2;
        public static Texture2D BERZERKER2_DEFEND2;
        public static Texture2D BERZERKER2_ROUTE;
        public static Texture2D BERZERKER2_ROUTED;
        public static Texture2D BERZERKER2_IDLENOSHIELD;
        public static Texture2D BERZERKER2_SHIELDBREAK;
        public static Texture2D BERZERKER2_FALL;
        public static Texture2D BERZERKER2_CHARGE;
        public static Texture2D BERZERKER2_CHARGENOSHIELD;

        public static Texture2D BRIGAND1_IDLE;
        public static Texture2D BRIGAND1_DEATH;
        public static Texture2D BRIGAND1_MELEE1;
        public static Texture2D BRIGAND1_DEFEND1;
        public static Texture2D BRIGAND1_CHARGE;
        public static Texture2D BRIGAND1_SPAWN;

        public static Texture2D BRIGAND2_IDLE;
        public static Texture2D BRIGAND2_DEATH;
        public static Texture2D BRIGAND2_MELEE1;
        public static Texture2D BRIGAND2_DEFEND1;
        public static Texture2D BRIGAND2_CHARGE;
        public static Texture2D BRIGAND2_SPAWN;

        public static Texture2D HAULER_FEET;
        public static Texture2D HAULER_HAUL;
        public static Texture2D HAULER_IDLE;
        public static Texture2D HAULER_DEATH;
        public static Texture2D HAULER_THROW;

        public static Texture2D BAGGER_DEATH;
        public static Texture2D BAGGER_HAUL;
        public static Texture2D BAGGER_IDLE;
        public static Texture2D BAGGER_THROW;
        public static Texture2D BAGGER_BAG;

        public static Texture2D WOLF_IDLE;
        public static Texture2D WOLF_RUN;
        public static Texture2D WOLF_SPOOKED;
        public static Texture2D WOLF_TURN;
        public static Texture2D WOLF_BITE;
        public static Texture2D WOLF_MELEE;
        public static Texture2D WOLF_DEFEND;
        public static Texture2D WOLF_KILL;
        public static Texture2D WOLF_HOWL;

        public static Texture2D WOLF_IDLEg;
        public static Texture2D WOLF_RUNg;
        public static Texture2D WOLF_SPOOKEDg;
        public static Texture2D WOLF_TURNg;
        public static Texture2D WOLF_BITEg;
        public static Texture2D WOLF_MELEEg;
        public static Texture2D WOLF_DEFENDg;
        public static Texture2D WOLF_KILLg;
        public static Texture2D WOLF_HOWLg;

        public static Texture2D WOLF_IDLE_COL;
        public static Texture2D WOLF_TURN_COL;
        public static Texture2D WOLF_ATTACK_COL;
        public static Texture2D WOLF_HOWL_COL;
        public static Texture2D WOLF_RUN_COL;
        public static Texture2D WOLF_GETUP_COL;

        public static Texture2D COLMILLOS_IDLE;
        public static Texture2D COLMILLOS_IDLENOSHIELD;
        public static Texture2D COLMILLOS_IDLENOARMOUR;
        public static Texture2D COLMILLOS_SHIELDBREAK;
        public static Texture2D COLMILLOS_FALL;
        public static Texture2D COLMILLOS_FALLNOSHIELD;
        public static Texture2D COLMILLOS_DEATH;
        public static Texture2D COLMILLOS_ATTACK;
        public static Texture2D COLMILLOS_ATTACK2;
        public static Texture2D COLMILLOS_ATTACK3;
        public static Texture2D COLMILLOS_RISE;
        public static Texture2D COLMILLOS_HELMET;
        public static Texture2D COLMILLOS_HOWL;
        public static Texture2D COLMILLOS_HOWL_NOSHIELD;
        public static Texture2D COLMILLOS_HOWL_NOARMOUR;
        public static Texture2D COLMILLOS_STAGGER;
        public static Texture2D FALCHION_THROWN;
        public static Texture2D FALCHION_DIRT;

        public static Texture2D PEASANT1_IDLE;
        public static Texture2D PEASANT2_IDLE;
        public static Texture2D PEASANT3_IDLE;
        public static Texture2D PEASANT4_IDLE;
        public static Texture2D PEASANT5_IDLE;
        public static Texture2D PEASANT6_IDLE;

        public static Texture2D CANNON;
        public static Texture2D CANNON_IDLE;
        public static Texture2D CANNON_BALL;
        public static Texture2D CANNON_WAVE;

        public static Texture2D PEASANT1_FLEE;
        public static Texture2D PEASANT5_FLEE;

        public static Texture2D BLUE_FEET;
        public static Texture2D BROWN_FEET;

        public static List<Texture2D> ROAD_TERRAIN;
        public static Texture2D TREE0;
        public static Texture2D TREE1;
        public static Texture2D TREE2;
        public static Texture2D ROAD_HORIZONTAL;
        public static Texture2D ROAD_HORIZONTAL_2;
        public static Texture2D ROAD_TURN;
        public static Texture2D ROAD_MILE_MARKER;
        public static Texture2D ROAD_TURN_MARKER;
        public static Texture2D BUSH0;
        public static Texture2D BUSH1;
        public static Texture2D BUSH2;
        public static Texture2D WAGON;
        public static Texture2D OX_GREY;
        public static Texture2D OX_BROWN;
        public static Texture2D OX_DEAD;
        public static Texture2D WOUNDED_PEASANT;
        public static Texture2D DEAD_PEASANT;
        public static Texture2D TOTEMPOLE;
        public static Texture2D RIVER;
        public static Texture2D RIVER_BED_0;
        public static Texture2D RIVER_BED_1;
        public static Texture2D RIVER_BED_0L;
        public static Texture2D RIVER_BED_1L;
        public static List<Texture2D> WATER_TERRAIN;
        public static Texture2D BOTTLES;
        public static Texture2D RUIN_BUILDING;
        public static Texture2D RUIN_CROSS;
        public static Texture2D FANG_ROCK;
        public static Texture2D FANG_ROCKS;
        public static Texture2D BARRICADES;
        public static Texture2D TRUNKS;

        public static Texture2D DROP_SPLASH;
        public static Texture2D DROP;
        public static Texture2D SPLASHING;
        public static Texture2D WADING;

        public static Texture2D COIN;
        public static Texture2D COIN_METER;
        public static Texture2D COIN_METER_BACK;
        public static Texture2D DOPPEL_METER;
        public static Texture2D LOOT;
        public static Texture2D COIN_SPINNA;
        public static Texture2D SPARKLE;
        public static Texture2D TITLE_ANIMATION;

        //Utility Graphics
        public static Texture2D DOT;
        public static Texture2D SWORD_POINTER;
        public static Texture2D TEST;

        public static Texture2D MOLE_MINER_WALKING;
        public static Texture2D MOLE_MINER_DIGGING;
        public static Texture2D MOLE_MINER_NUDGE;
        public static Texture2D MOLE_SQUASHED;

        public static Texture2D PALETTE;
        public static Texture2D BACKGROUND;
        public static Texture2D BACKGROUND2;
        public static Texture2D BACKGROUND3;

        public static Texture2D SCREEN_TEXT;

        public static Texture2D TUNNEL;
        public static Texture2D TUNNEL_DIGGING;

        public static Texture2D TURNIP_SHAKE;
        public static Texture2D TURNIP_SPLIT;
        public static Texture2D TURNIP_TWIRL;

        public static Texture2D ONION_SHAKE;
        public static Texture2D ONION_SPLIT;
        public static Texture2D ONION_TWIRL;

        public static Texture2D RAT_WALKING;
        public static Texture2D RAT_NUDGE;
        public static Texture2D RAT_CRUSHED;

        public static Texture2D GRUB_EGG;
        public static Texture2D GRUB_GRUB;
        public static Texture2D GRUB_LOOK;

        public static Texture2D SANDBOX;

        public Queue<Texture2D> prevFrames;

        //Audio
        public static SoundEffect PICKUP_GRUB;

        public static Song THEME_1;
        public static Song THEME_2;
        public static SoundEffect SHOT_0;
        public static SoundEffect SHOT_1;
        public static SoundEffect SHOT_2;
        public static SoundEffect SHOT_3;
        public static SoundEffect SHOT_4;
        public static SoundEffect PIKE_0;
        public static SoundEffect PIKE_1;
        public static SoundEffect PIKE_2;
        public static SoundEffect PIKE_3;
        public static SoundEffect PIKE_4;
        public static SoundEffect PIKE_5;
        public static SoundEffectInstance PIKES_LOWER;
        public static SoundEffectInstance PIKES_RAISE;
        public static SoundEffect SHOT_HIT;
        public static SoundEffect ROCK_HIT;
        public static SoundEffect SLING_ROCK;
        public static SoundEffect OWW_ALLY;
        public static SoundEffect OWW_ENEMY;
        public static SoundEffect BODY_FALL;
        public static SoundEffect CHARGE_ROAR;
        public static SoundEffect LOOT_SOUND;
        public static SoundEffect COIN_SOUND;
        public static SoundEffect SHIELD_BREAK;
        public static SoundEffect MELEE_CLANG_0;
        public static SoundEffect MELEE_CLANG_1;
        public static SoundEffect MELEE_CLANG_2;
        public static SoundEffect SLASH;
        public static SoundEffect GAME_OVER;
        public static SoundEffect POWER_UP;
        public static SoundEffect DOPPEL_UP;
        public static SoundEffect DOPPEL_DOWN;
        public static SoundEffect COLMILLOS_HURT;
        public static SoundEffect COLMILLOS_YELL;

        private ArrayList _gameScreens;
        private GameScreen _currScreen;

        public static float ZOOM = 1.0f;
        public Color screenColor;
        public Color screenColorShader;

        public PikeAndShotGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREENWIDTH;
            graphics.PreferredBackBufferHeight = SCREENHEIGHT;
            graphics.PreferMultiSampling = false;

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
            if (!DEBUG)
            {
                //make it full screen... (borderless if you want to is an option as well)
                this.Window.Position = new Point((int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - (float)SCREENWIDTH)/2 -50, (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - (float)SCREENHEIGHT) / 2 - 50);
                this.Window.IsBorderless = true;
                graphics.PreferredBackBufferWidth = SCREENWIDTH+50;
                graphics.PreferredBackBufferHeight = SCREENHEIGHT+50;
                graphics.IsFullScreen = false;
                this.Window.AllowUserResizing = false;
                graphics.ApplyChanges();
                useShaders = true;
            }
            else
                useShaders = false;

            this.Window.ClientSizeChanged += Window_ClientSizeChanged;

            Content.RootDirectory = "Content";
            screenColor = new Color(166, 172, 132, 150);
            screenColorShader = new Color(166, 172, 132,0);
        }

        public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
        {
            //initialize a texture
            Texture2D texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = paint(pixel);
            }

            //set the color
            texture.SetData(data);

            return texture;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            setDrawRect();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
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
            viewport = GraphicsDevice.Viewport;

            setDrawRect();

            soldierFont = Content.Load<SpriteFont>("SpriteFont1");

            PALETTE = Content.Load<Texture2D>(@"palette");
            BACKGROUND2 = Content.Load<Texture2D>(@"safeplace_fullsize");
            BACKGROUND3 = Content.Load<Texture2D>(@"safeplace_fullsize2");
            SCREEN_TEXT = CreateTexture(GraphicsDevice, 1, 1, pixel => new Color(166, 172, 132, 150));

            ShaderRenderTarget = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
            ShaderRenderTarget2 = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
            ShaderRenderTarget3 = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
            ShaderRenderTarget4 = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);

            effect = Content.Load<Effect>(@"newshader");
            effect.Parameters["Viewport"].SetValue(new Vector2((float)SCREENWIDTH, (float)SCREENHEIGHT));

            effect2 = Content.Load<Effect>(@"newshader2");
            effect2.Parameters["Viewport"].SetValue(new Vector2((float)SCREENWIDTH, (float)SCREENHEIGHT));
            effect2.Parameters["iResolution"].SetValue(new Vector2((float)SCREENWIDTH, (float)SCREENHEIGHT) * 1.5f);

            effect3 = Content.Load<Effect>(@"gb-pass-0"); 
            effect3.Parameters["video_size"].SetValue(new Vector2(256, 192));
            effect3.Parameters["texture_size"].SetValue(new Vector2(256, 192));
            effect3.Parameters["output_size"].SetValue(new Vector2((float)SCREENWIDTH, (float)SCREENHEIGHT));
     //       effect3.Parameters["modelViewProj"].SetValue(Matrix.Identity);
            effect3.Parameters["$COLOR_PALETTE"].SetValue(PALETTE);
            //effect3.Parameters["$BACKGROUND"].SetValue(BACKGROUND);

            prevFrames = new Queue<Texture2D>(7);

            //TERRAIN_DRY_GRASS = Content.Load<Texture2D>(@"dry_grass");
            //ROAD_TERRAIN = new List<Texture2D>(11);

            //for (int i = 0; i < 11; i++)
            //    ROAD_TERRAIN.Add(getDimmerClone(Content.Load<Texture2D>(@"roadTerrain" + i)));

            //TREE0 = getDimmerClone(Content.Load<Texture2D>(@"roadTerrain11"));
            //TREE1 = getDimmerClone(Content.Load<Texture2D>(@"tree01"));
            //TREE2 = getDimmerClone(Content.Load<Texture2D>(@"tree02"));

            //BUSH0 = getDimmerClone(Content.Load<Texture2D>(@"bush00"));
            //BUSH1 = getDimmerClone(Content.Load<Texture2D>(@"bush01"));
            //BUSH2 = getDimmerClone(Content.Load<Texture2D>(@"bush03"));
            //WAGON = getDimmerClone(Content.Load<Texture2D>(@"wagon"));
            //OX_BROWN = getDimmerClone(Content.Load<Texture2D>(@"ox_brown"));
            //OX_GREY = getDimmerClone(Content.Load<Texture2D>(@"ox_grey"));
            //OX_DEAD = getDimmerClone(Content.Load<Texture2D>(@"ox_dead"));
            //WOUNDED_PEASANT = getDimmerClone(Content.Load<Texture2D>(@"wounded_dude"));
            //DEAD_PEASANT = getDimmerClone(Content.Load<Texture2D>(@"bodies"));
            //TOTEMPOLE = getDimmerClone(Content.Load<Texture2D>(@"totempole"));
            //RIVER = getDimmerClone(Content.Load<Texture2D>(@"river"));
            //RIVER_BED_0 = getDimmerClone(Content.Load<Texture2D>(@"river_bed00"));
            //RIVER_BED_1 = getDimmerClone(Content.Load<Texture2D>(@"river_bed01"));
            //RIVER_BED_0L = getDimmerClone(Content.Load<Texture2D>(@"left_river_bed00"));
            //RIVER_BED_1L = getDimmerClone(Content.Load<Texture2D>(@"left_river_bed01"));
            //WATER_TERRAIN = new List<Texture2D>(3);
            //WATER_TERRAIN.Add(getDimmerClone(Content.Load<Texture2D>(@"watery_rock")));
            //WATER_TERRAIN.Add(getDimmerClone(Content.Load<Texture2D>(@"watery_rock2")));
            //WATER_TERRAIN.Add(getDimmerClone(Content.Load<Texture2D>(@"watery_rock3")));
            //BOTTLES = getDimmerClone(Content.Load<Texture2D>(@"bottles"));
            //RUIN_BUILDING = getDimmerClone(Content.Load<Texture2D>(@"ruin_building"));
            //RUIN_CROSS = getDimmerClone(Content.Load<Texture2D>(@"ruin_cross"));
            //FANG_ROCK = getDimmerClone(Content.Load<Texture2D>(@"fang_rock"));
            //FANG_ROCKS = getDimmerClone(Content.Load<Texture2D>(@"fang_rocks"));
            //BARRICADES = getDimmerClone(Content.Load<Texture2D>(@"barricades"));
            //TRUNKS = getDimmerClone(Content.Load<Texture2D>(@"trunks"));

            //DROP_SPLASH = Content.Load<Texture2D>(@"splash");
            //DROP = Content.Load<Texture2D>(@"water_drop");
            //SPLASHING = Content.Load<Texture2D>(@"splashing");
            //WADING = Content.Load<Texture2D>(@"wading");

            //ROAD_HORIZONTAL = getDimmerClone(Content.Load<Texture2D>(@"roadHorizontal"), ROAD_FADE);
            //ROAD_TURN = getDimmerClone(Content.Load<Texture2D>(@"roadTurn"), ROAD_FADE);
            //ROAD_MILE_MARKER = getDimmerClone(Content.Load<Texture2D>(@"roadMileMarker"));
            //ROAD_TURN_MARKER = getDimmerClone(Content.Load<Texture2D>(@"roadTurnMarker"));
            //ROAD_HORIZONTAL_2 = getDimmerClone(Content.Load<Texture2D>(@"road_h2"), ROAD_FADE);

            //PUCELLE_IDLE = Content.Load<Texture2D>(@"pucelle_ready2");
            //PUCELLE_MOTION = Content.Load<Texture2D>(@"pucelle_motion");
            //PUCELLE_ROUTED = Content.Load<Texture2D>(@"pucelle_routed");
            //PUCELLE_DROP = Content.Load<Texture2D>(@"dropped_flag");

            //SOLDIER_FEET = Content.Load<Texture2D>(@"soldier_feet");
            //SOLDIER_IDLE = Content.Load<Texture2D>(@"soldier_idle");
            //SOLDIER2_IDLE = Content.Load<Texture2D>(@"soldier2_idle");
            //SOLDIER_DEATH = Content.Load<Texture2D>(@"soldier_death");
            //SOLDIER2_DEATH = Content.Load<Texture2D>(@"soldier2_death");
            //SOLDIER_MELEE1 = Content.Load<Texture2D>(@"soldier_melee1");
            //SOLDIER_DEFEND1 = Content.Load<Texture2D>(@"soldier_defend1");
            //SOLDIER_MELEE2 = Content.Load<Texture2D>(@"soldier_melee2");
            //SOLDIER_DEFEND2 = Content.Load<Texture2D>(@"soldier_defend2");
            //SOLDIER2_MELEE2 = Content.Load<Texture2D>(@"soldier2_melee");
            //SOLDIER2_DEFEND2 = Content.Load<Texture2D>(@"soldier2_defend");
            //SOLDIER_SHIELDBREAK = Content.Load<Texture2D>(@"soldier_shieldbreak");
            //SOLDIER_FALL = Content.Load<Texture2D>(@"soldier_fall");
            //SOLDIER_BROKENSHIELD1 = Content.Load<Texture2D>(@"brokenshield1");
            //SOLDIER_BROKENSHIELD2 = Content.Load<Texture2D>(@"brokenshield2");
            //SOLDIER_BROKENARMOUR = Content.Load<Texture2D>(@"brokenarmour");
            //SOLDIER_IDLENOSHIELD = Content.Load<Texture2D>(@"soldier_idlenoshield");
            //SOLDIER_ROUTE = Content.Load<Texture2D>(@"soldier_route");
            //SOLDIER_ROUTED = Content.Load<Texture2D>(@"soldier1_routed");
            //SOLDIER2_ROUTED = Content.Load<Texture2D>(@"soldier2_routed");
            //SOLDIER_CHARGENOSHIELD = Content.Load<Texture2D>(@"soldier_chargenoshield");
            //SOLDIER_CHARGE = Content.Load<Texture2D>(@"soldier_charge");

            //CAVALRY_HORSE_IDLE = Content.Load<Texture2D>(@"horse_idle");
            //CAVALRY_HORSE_RUN = Content.Load<Texture2D>(@"horse_run");
            //CAVALRY_HORSE_HALT = Content.Load<Texture2D>(@"horse_halt");
            //CAVALRY_HORSE_TURN = Content.Load<Texture2D>(@"horse_turn");
            //CAVALRY_HORSE_DEATH = Content.Load<Texture2D>(@"horse_death");
            //CAVALRY_LEFTFOOT = Content.Load<Texture2D>(@"cavalry_leftfoot");
            //CAVALRY_RIGHTFOOT = Content.Load<Texture2D>(@"cavalry_rightfoot");
            //CAVALRY_LEFTIDLE = Content.Load<Texture2D>(@"cavalry_leftidle");
            //CAVALRY_RIGHTIDLE = Content.Load<Texture2D>(@"cavalry_rightidle");
            //CAVALRY_LEFTLOWER = Content.Load<Texture2D>(@"cavalry_leftlower");
            //CAVALRY_RIGHTLOWER = Content.Load<Texture2D>(@"cavalry_rightlower");
            //CAVALRY_LEFTRECOIL = Content.Load<Texture2D>(@"cavalry_leftrecoil");
            //CAVALRY_RIGHTRECOIL = Content.Load<Texture2D>(@"cavalry_rightrecoil");

            //CAVALRY_FALL = Content.Load<Texture2D>(@"cavalry_fall");
            //DISMOUNTED_CAVALRY_IDLE = Content.Load<Texture2D>(@"dismountedcavalry_idle");
            //DISMOUNTED_CAVALRY_DEATH = Content.Load<Texture2D>(@"dismountedcavalry_death");
            //DISMOUNTED_CAVALRY_IDLENOSHIELD = Content.Load<Texture2D>(@"dismountedcavalry_idlenoshield");
            //DISMOUNTED_CAVALRY_SHIELDBREAK = Content.Load<Texture2D>(@"dismountedcavalry_shieldbreak");
            //DISMOUNTED_CAVALRY_FALL = Content.Load<Texture2D>(@"dismountedcavalry_fall");
            //DISMOUNTED_CAVALRY_MELEE1 = Content.Load<Texture2D>(@"dismountedcavalry_melee1");
            //DISMOUNTED_CAVALRY_DEFEND1 = Content.Load<Texture2D>(@"dismountedcavalry_defend1");

            //PIKEMAN_FEET = Content.Load<Texture2D>(@"pikeman_feet");
            //PIKEMAN_IDLE = Content.Load<Texture2D>(@"pikeman_idle");
            //PIKEMAN_DROP = Content.Load<Texture2D>(@"dropped_pike");
            //PIKEMAN_LOWER_LOW = Content.Load<Texture2D>(@"pikeman_lower_low");
            //PIKEMAN_LOWER_HIGH = Content.Load<Texture2D>(@"pikeman_lower_high");
            //PIKEMAN_RECOIL = Content.Load<Texture2D>(@"pikeman_recoil");
            //PIKEMAN_DEATH = Content.Load<Texture2D>(@"pikeman_death");
            //PIKEMAN1_IDLE = Content.Load<Texture2D>(@"pikeman1_idle");
            //PIKEMAN1_LOWER_LOW = Content.Load<Texture2D>(@"pikeman1_lower_low");
            //PIKEMAN1_LOWER_HIGH = Content.Load<Texture2D>(@"pikeman1_lower_high");
            //PIKEMAN1_RECOIL = Content.Load<Texture2D>(@"pikeman1_recoil");
            //PIKEMAN1_DEATH = Content.Load<Texture2D>(@"pikeman1_death");
            //PIKEMAN1_MELEE = Content.Load<Texture2D>(@"pikeman1_melee1");
            //PIKEMAN1_DEFEND = Content.Load<Texture2D>(@"pikeman1_defend1");
            //PIKEMAN2_IDLE = Content.Load<Texture2D>(@"pikeman2_idle");
            //PIKEMAN2_LOWER_LOW = Content.Load<Texture2D>(@"pikeman2_lower_low");
            //PIKEMAN2_LOWER_HIGH = Content.Load<Texture2D>(@"pikeman2_lower_high");
            //PIKEMAN2_RECOIL = Content.Load<Texture2D>(@"pikeman2_recoil");
            //PIKEMAN2_DEATH = Content.Load<Texture2D>(@"pikeman2_death");
            //PIKEMAN2_MELEE = Content.Load<Texture2D>(@"pikeman2_melee1");
            //PIKEMAN2_DEFEND = Content.Load<Texture2D>(@"pikeman2_defend1");
            //PIKEMAN_MELEE = Content.Load<Texture2D>(@"pikeman_melee");
            //PIKEMAN_ROUTE = Content.Load<Texture2D>(@"pikeman_route");
            //PIKEMAN_ROUTED = Content.Load<Texture2D>(@"pikeman1_over");
            //PIKEMAN2_ROUTED = Content.Load<Texture2D>(@"pikeman2_over");
            //PIKEMAN_TUG = Content.Load<Texture2D>(@"tug_pikeman");
            //PIKEMAN2_TUG = Content.Load<Texture2D>(@"tug_pikeman2");
            //WOLF_TUG = Content.Load<Texture2D>(@"tug_wolf");
            //WOLF2_TUG = Content.Load<Texture2D>(@"tug_wolf2");
            //WOLF_TUGg = getGreyscaleClone(WOLF_TUG);
            //WOLF2_TUGg = getGreyscaleClone(WOLF2_TUG);

            //ARQUEBUSIER_FEET = Content.Load<Texture2D>(@"arquebusier_feet");
            //ARQUEBUSIER_IDLE = Content.Load<Texture2D>(@"gonner_idle");
            //ARQUEBUSIER_RELOAD = Content.Load<Texture2D>(@"gonner_reload");
            //ARQUEBUSIER_SHOOT = Content.Load<Texture2D>(@"gonner_shoot");
            //ARQUEBUSIER_SMOKE = Content.Load<Texture2D>(@"gonner_smoke");
            //ARQUEBUSIER_DEATH = Content.Load<Texture2D>(@"gonner_death");
            //ARQUEBUSIER_MELEE = Content.Load<Texture2D>(@"gonner_melee");
            //ARQUEBUSIER_DEFEND = Content.Load<Texture2D>(@"gonner_defend");
            //ARQUEBUSIER_ROUTE = Content.Load<Texture2D>(@"arquebusier_route");
            //ARQUEBUSIER_ROUTED = Content.Load<Texture2D>(@"gonner_over");
            //ARQUEBUSIER_SHOT1 = Content.Load<Texture2D>(@"shot1");
            //ARQUEBUSIER_SHOT2 = Content.Load<Texture2D>(@"shot2");
            //ARQUEBUSIER_SHOT3 = Content.Load<Texture2D>(@"shot3");
            //ARQUEBUSIER_GROUND = Content.Load<Texture2D>(@"arquebusier_ground");

            //ARQUEBUSIER_FEET2 = Content.Load<Texture2D>(@"brown_lady_feet");
            //ARQUEBUSIER_IDLE2 = Content.Load<Texture2D>(@"arque_idle");
            //ARQUEBUSIER_DEATH2 = Content.Load<Texture2D>(@"arque_death");
            //ARQUEBUSIER_MELEE2 = Content.Load<Texture2D>(@"arque_melee");
            //ARQUEBUSIER_DEFEND2 = Content.Load<Texture2D>(@"arque_defend");
            //ARQUEBUSIER_RELOAD2 = Content.Load<Texture2D>(@"arque_reload");
            //ARQUEBUSIER_SHOOT2 = Content.Load<Texture2D>(@"arque_shoot");
            //ARQUEBUSIER_SMOKE2 = Content.Load<Texture2D>(@"arque_smoke");
            //ARQUEBUSIER2_ROUTED = Content.Load<Texture2D>(@"arque_over");
            //ARQUEBUSIER_DROP = Content.Load<Texture2D>(@"dropped_gonne");
            //ARQUEBUSIER2_DROP = Content.Load<Texture2D>(@"dropped_arque");

            //CROSSBOWMAN_IDLE = Content.Load<Texture2D>(@"crossbowman_idle");
            //CROSSBOWMAN_RELOAD = Content.Load<Texture2D>(@"crossbowman_reload");
            //CROSSBOWMAN_RELOAD2 = Content.Load<Texture2D>(@"crossbowman_reload2");
            //CROSSBOWMAN_SHOOT = Content.Load<Texture2D>(@"crossbowman_shoot");
            //CROSSBOWMAN_DEATH = Content.Load<Texture2D>(@"crossbowman_death");
            //CROSSBOWMAN_MELEE = Content.Load<Texture2D>(@"crossbowman_melee");
            //CROSSBOWMAN_MELEE2 = Content.Load<Texture2D>(@"crossbowman_melee2");
            //CROSSBOWMAN_DEFEND2 = Content.Load<Texture2D>(@"crossbowman_defend2");
            //CROSSBOWMAN_BOLT = Content.Load<Texture2D>(@"crossbowman_bolt");
            //CROSSBOWMAN_BOLT2 = Content.Load<Texture2D>(@"crossbowman_bolt2");
            //CROSSBOWMAN_GROUND = Content.Load<Texture2D>(@"crossbowman_ground");
            //CROSSBOWMAN_ROUTE = Content.Load<Texture2D>(@"crossbowman_route");
            //CROSSBOWMAN_ROUTED = Content.Load<Texture2D>(@"crossbowman_routed");
            //CROSSBOWMAN_PAVISE = Content.Load<Texture2D>(@"crossbowman_pavise_idle");
            //CROSSBOWMAN_PAVISE_PLACE = Content.Load<Texture2D>(@"crossbowman_pavise_place");
            //CROSSBOWMAN_SHIELDBREAK = Content.Load<Texture2D>(@"crossbowman_shieldbreak");

            //PLACED_PAVISE = Content.Load<Texture2D>(@"placed_pavise");
            //PAVISE_FALL = Content.Load<Texture2D>(@"pavise_fall");

            //DOPPLE_DEATH = Content.Load<Texture2D>(@"dopple_death");
            //DOPPLE_IDLE = Content.Load<Texture2D>(@"dopple_idle");
            //DOPPLE_SWING1 = Content.Load<Texture2D>(@"dopple_swing1");
            //DOPPLE_RELOAD1 = Content.Load<Texture2D>(@"dopple_reload1");
            //DOPPLE_ROUTE = Content.Load<Texture2D>(@"dopple_route");
            //DOPPLE_ROUTED = Content.Load<Texture2D>(@"dopple_routed");

            //SLINGER_IDLE = Content.Load<Texture2D>(@"slinger_idle");
            //SLINGER_DEATH = Content.Load<Texture2D>(@"slinger_death");
            //SLINGER_MELEE = Content.Load<Texture2D>(@"slinger_melee2");
            //SLINGER_DEFEND = Content.Load<Texture2D>(@"slinger_defend2");
            //SLINGER_RELOAD = Content.Load<Texture2D>(@"slinger_reload");
            //SLINGER_ROCK = Content.Load<Texture2D>(@"slinger_rock");
            //SLINGER_SHOOT = Content.Load<Texture2D>(@"slinger_shoot");
            //SLINGER_ROUTE = Content.Load<Texture2D>(@"slinger_route");
            //SLINGER_ROUTED = Content.Load<Texture2D>(@"slinger_routed");
            //SLINGER_RETREAT = Content.Load<Texture2D>(@"slinger_retreat");

            //SKIRMISHER_IDLE = Content.Load<Texture2D>(@"skirmisher_idle");
            //SKIRMISHER_RELOAD = Content.Load<Texture2D>(@"skirmisher_reload");
            //SKIRMISHER_SHOOT = Content.Load<Texture2D>(@"skirmisher_shoot");
            //SKIRMISHER_JAVELIN = Content.Load<Texture2D>(@"skirmisher_javelin");
            //SKIRMISHER_GROUND = Content.Load<Texture2D>(@"skirmisher_ground");
            //SKIRMISHER_DEATH = Content.Load<Texture2D>(@"skirmisher_death");
            //SKIRMISHER_MELEE = Content.Load<Texture2D>(@"skirmisher_melee2");
            //SKIRMISHER_DEFEND = Content.Load<Texture2D>(@"skirmisher_defend2");
            //SKIRMISHER_RETREAT = Content.Load<Texture2D>(@"skirmisher_retreat");

            //BERZERKER_IDLE = Content.Load<Texture2D>(@"gobraider1_idle");
            //BERZERKER_DEATH = Content.Load<Texture2D>(@"gobraider1_death");
            //BERZERKER_MELEE1 = Content.Load<Texture2D>(@"gobraider1_melee1");
            //BERZERKER_DEFEND1 = Content.Load<Texture2D>(@"gobraider1_defend1");
            //BERZERKER_MELEE2 = Content.Load<Texture2D>(@"gobraider1_melee2");
            //BERZERKER_DEFEND2 = Content.Load<Texture2D>(@"gobraider1_defend2");
            //BERZERKER_ROUTE = Content.Load<Texture2D>(@"beserker_route");
            //BERZERKER_ROUTED = Content.Load<Texture2D>(@"beserker_routed");
            //BERZERKER_IDLENOSHIELD = Content.Load<Texture2D>(@"gobraider1_idlenoshield");
            //BERZERKER_SHIELDBREAK = Content.Load<Texture2D>(@"gobraider1_shieldbreak");
            //BERZERKER_FALL = Content.Load<Texture2D>(@"gobraider1_fall");
            //BERZERKER_CHARGENOSHIELD = Content.Load<Texture2D>(@"gobraider1_chargenoshield");
            //BERZERKER_CHARGE = Content.Load<Texture2D>(@"gobraider1_charge");

            //BERZERKER2_IDLE = Content.Load<Texture2D>(@"gobraider2_idle");
            //BERZERKER2_DEATH = Content.Load<Texture2D>(@"gobraider2_death");
            //BERZERKER2_MELEE1 = Content.Load<Texture2D>(@"gobraider2_melee1");
            //BERZERKER2_DEFEND1 = Content.Load<Texture2D>(@"gobraider2_defend1");
            //BERZERKER2_MELEE2 = Content.Load<Texture2D>(@"gobraider2_melee2");
            //BERZERKER2_DEFEND2 = Content.Load<Texture2D>(@"gobraider2_defend2");
            //BERZERKER2_ROUTE = Content.Load<Texture2D>(@"beserker_route");
            //BERZERKER2_ROUTED = Content.Load<Texture2D>(@"beserker_routed");
            //BERZERKER2_IDLENOSHIELD = Content.Load<Texture2D>(@"gobraider2_idlenoshield");
            //BERZERKER2_SHIELDBREAK = Content.Load<Texture2D>(@"gobraider2_shieldbreak");
            //BERZERKER2_FALL = Content.Load<Texture2D>(@"gobraider2_fall");
            //BERZERKER2_CHARGENOSHIELD = Content.Load<Texture2D>(@"gobraider2_chargenoshield");
            //BERZERKER2_CHARGE = Content.Load<Texture2D>(@"gobraider2_charge");

            //BRIGAND1_IDLE = Content.Load<Texture2D>(@"berzerker_idle");
            //BRIGAND1_DEATH = Content.Load<Texture2D>(@"bezerker_death");
            //BRIGAND1_MELEE1 = Content.Load<Texture2D>(@"berserker_melee2");
            //BRIGAND1_DEFEND1 = Content.Load<Texture2D>(@"berserker_defend2");
            //BRIGAND1_CHARGE = Content.Load<Texture2D>(@"berserker_charge");
            //BRIGAND1_SPAWN = Content.Load<Texture2D>(@"brigand_eating");

            //BRIGAND2_IDLE = Content.Load<Texture2D>(@"brigand_idle");
            //BRIGAND2_DEATH = Content.Load<Texture2D>(@"brigand_death");
            //BRIGAND2_MELEE1 = Content.Load<Texture2D>(@"brigand_melee2");
            //BRIGAND2_DEFEND1 = Content.Load<Texture2D>(@"brigand_defend2");
            //BRIGAND2_CHARGE = Content.Load<Texture2D>(@"brigand_charge");
            //BRIGAND2_SPAWN = Content.Load<Texture2D>(@"brigand_stealing");

            //HAULER_HAUL = Content.Load<Texture2D>(@"hauler_haul");
            //HAULER_FEET = Content.Load<Texture2D>(@"grey_feet");
            //HAULER_IDLE = Content.Load<Texture2D>(@"hauler_idle");
            //HAULER_DEATH = Content.Load<Texture2D>(@"hauler_death");
            //HAULER_THROW = Content.Load<Texture2D>(@"chest_throw");
            //BAGGER_DEATH = Content.Load<Texture2D>(@"bagger_death");
            //BAGGER_HAUL = Content.Load<Texture2D>(@"bagger_haul");
            //BAGGER_IDLE = Content.Load<Texture2D>(@"bagger_idle");
            //BAGGER_THROW = Content.Load<Texture2D>(@"bag_throw");
            //BAGGER_BAG = Content.Load<Texture2D>(@"bagger_bag_idle");

            //WOLF_IDLE = Content.Load<Texture2D>(@"wolf_idle");
            //WOLF_RUN = Content.Load<Texture2D>(@"wolf_run");
            //WOLF_SPOOKED = Content.Load<Texture2D>(@"wolf_spooked");
            //WOLF_TURN = Content.Load<Texture2D>(@"wolf_turn");
            //WOLF_BITE = Content.Load<Texture2D>(@"wolf_bite");
            //WOLF_MELEE = Content.Load<Texture2D>(@"wolf_melee");
            //WOLF_DEFEND = Content.Load<Texture2D>(@"wolf_defend");
            //WOLF_KILL = Content.Load<Texture2D>(@"wolf_kill");
            //WOLF_HOWL = Content.Load<Texture2D>(@"wolf_howl");

            //WOLF_IDLE_COL = Content.Load<Texture2D>(@"wolf_idle_col");
            //WOLF_TURN_COL = Content.Load<Texture2D>(@"wolf_turn_col");
            //WOLF_ATTACK_COL = Content.Load<Texture2D>(@"wolf_bite_col");
            //WOLF_HOWL_COL = Content.Load<Texture2D>(@"wolf_howl_col");
            //WOLF_RUN_COL = Content.Load<Texture2D>(@"wolf_run_col");
            //WOLF_GETUP_COL = Content.Load<Texture2D>(@"wolf_getup_col");

            //WOLF_IDLEg = getGreyscaleClone(WOLF_IDLE);
            //WOLF_RUNg = getGreyscaleClone(WOLF_RUN);
            //WOLF_SPOOKEDg = getGreyscaleClone(WOLF_SPOOKED);
            //WOLF_TURNg = getGreyscaleClone(WOLF_TURN);
            //WOLF_BITEg = getGreyscaleClone(WOLF_BITE);
            //WOLF_MELEEg = getGreyscaleClone(WOLF_MELEE);
            //WOLF_DEFENDg = getGreyscaleClone(WOLF_DEFEND);
            //WOLF_KILLg = getGreyscaleClone(WOLF_KILL);
            //WOLF_HOWLg = getGreyscaleClone(WOLF_HOWL);

            //COLMILLOS_IDLE = Content.Load<Texture2D>(@"los_colmillos_idle0");
            //COLMILLOS_IDLENOSHIELD = Content.Load<Texture2D>(@"los_colmillos_idle1");
            //COLMILLOS_IDLENOARMOUR = Content.Load<Texture2D>(@"los_colmillos_idle2");
            //COLMILLOS_SHIELDBREAK = Content.Load<Texture2D>(@"los_colmillos_shieldbreak");

            //COLMILLOS_FALL = Content.Load<Texture2D>(@"los_colmillos_fall1");
            //COLMILLOS_FALLNOSHIELD = Content.Load<Texture2D>(@"los_colmillos_fall2");
            //COLMILLOS_ATTACK = Content.Load<Texture2D>(@"los_colmillos_attack");
            //COLMILLOS_ATTACK2 = Content.Load<Texture2D>(@"los_colmillos_attack2");
            //COLMILLOS_ATTACK3 = Content.Load<Texture2D>(@"los_colmillos_attack3");
            //COLMILLOS_DEATH = Content.Load<Texture2D>(@"los_colmillos_death");
            //COLMILLOS_HELMET = Content.Load<Texture2D>(@"helmet");
            //COLMILLOS_RISE = Content.Load<Texture2D>(@"los_colmillos_rise");
            //COLMILLOS_HOWL = Content.Load<Texture2D>(@"los_colmillos_howl");
            //COLMILLOS_HOWL_NOSHIELD = Content.Load<Texture2D>(@"los_colmillos_howl_noshield");
            //COLMILLOS_HOWL_NOARMOUR = Content.Load<Texture2D>(@"los_colmillos_howl_noarmour");
            //COLMILLOS_STAGGER = Content.Load<Texture2D>(@"los_colmillos_stagger");
            //FALCHION_THROWN = Content.Load<Texture2D>(@"falchion_thrown");
            //FALCHION_DIRT = Content.Load<Texture2D>(@"falchion_dirt");

            //CANNON = Content.Load<Texture2D>(@"cannon");
            //CANNON_IDLE = Content.Load<Texture2D>(@"cannon_idle");
            //CANNON_BALL = Content.Load<Texture2D>(@"cannonball");
            //CANNON_WAVE = Content.Load<Texture2D>(@"cannon_wave");

            //PEASANT1_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_01_idle"));
            //PEASANT2_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_02_idle"));
            //PEASANT3_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_03_idle"));
            //PEASANT4_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_04_idle"));
            //PEASANT5_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_05_idle"));
            //PEASANT6_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_06_idle"));

            //PEASANT1_FLEE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_01_fleeing"));
            //PEASANT5_FLEE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_05_fleeing"));

            //GOBLIN_FEET = Content.Load<Texture2D>(@"goblin_feet");
            //BROWN_FEET = Content.Load<Texture2D>(@"brown_feet");
            //BLUE_FEET = Content.Load<Texture2D>(@"blue_feet");
            //SLINGER_GROUND = Content.Load<Texture2D>(@"slinger_ground");

            //COIN = Content.Load<Texture2D>(@"coin");
            //COIN_METER = Content.Load<Texture2D>(@"coin_meter");
            //COIN_METER_BACK = Content.Load<Texture2D>(@"coin_meter_back");
            //LOOT = Content.Load<Texture2D>(@"loot");
            //DOPPEL_METER = Content.Load<Texture2D>(@"doppel_meter");

            //DOT = Content.Load<Texture2D>(@"dot");
            //SWORD_POINTER = Content.Load<Texture2D>(@"sword_pointer");
            MOLE_MINER_WALKING = Content.Load<Texture2D>(@"mole_miner_sprite");
            MOLE_MINER_DIGGING = Content.Load<Texture2D>(@"mole_miner_sprite_dig");
            MOLE_MINER_NUDGE = Content.Load<Texture2D>(@"mole_miner_nudge");
            MOLE_SQUASHED = Content.Load<Texture2D>(@"mole_squashed");

            TUNNEL = Content.Load<Texture2D>(@"tunnel");
            TUNNEL_DIGGING = Content.Load<Texture2D>(@"tunnel_digging");
            TUNNEL_DIGGING = Content.Load<Texture2D>(@"tunnel_digging");

            TURNIP_SHAKE = Content.Load<Texture2D>(@"turnip_fall");
            TURNIP_TWIRL = Content.Load<Texture2D>(@"turnip_twirl");
            TURNIP_SPLIT = Content.Load<Texture2D>(@"turnip_split");
            
            ONION_SHAKE = Content.Load<Texture2D>(@"onion_shake");
            ONION_TWIRL = Content.Load<Texture2D>(@"onion_twirl");
            ONION_SPLIT = Content.Load<Texture2D>(@"onion_split");

            RAT_WALKING = Content.Load<Texture2D>(@"rat_walk_one");
            RAT_NUDGE = Content.Load<Texture2D>(@"rat_push");
            RAT_CRUSHED = Content.Load<Texture2D>(@"rat_crushed");

            GRUB_EGG = Content.Load<Texture2D>(@"egg");
            GRUB_GRUB = Content.Load<Texture2D>(@"grub_craw_sml");
            GRUB_LOOK = Content.Load<Texture2D>(@"grub_look");

            SANDBOX = Content.Load<Texture2D>(@"sandbox");

            TEST = new Texture2D(GraphicsDevice, 1, 1);
            Color[] colors = { Color.Black };
            TEST.SetData(colors);


            COIN_SPINNA = Content.Load<Texture2D>(@"coin_spinna");
            SPARKLE = Content.Load<Texture2D>(@"sparkle");
            //TITLE_ANIMATION = Content.Load<Texture2D>(@"title_animation");
            TITLE_ANIMATION = Content.Load<Texture2D>(@"title_screen");


            PICKUP_GRUB = Content.Load<SoundEffect>("possibleSound5");

 //           THEME_2 = Content.Load<Song>(@"boss_music");
 //           THEME_1 = Content.Load<Song>(@"level_music");
            //SHOT_0 = Content.Load<SoundEffect>(@"shot00");
            //SHOT_1 = Content.Load<SoundEffect>(@"shot01");
            //SHOT_2 = Content.Load<SoundEffect>(@"shot02");
            //SHOT_3 = Content.Load<SoundEffect>(@"shot04");
            //SHOT_4 = Content.Load<SoundEffect>(@"shot05");



            //PIKES_LOWER = Content.Load<SoundEffect>(@"down").CreateInstance();
            //PIKES_LOWER.Volume = 1f;
            //PIKES_RAISE = Content.Load<SoundEffect>(@"drop").CreateInstance();
            //PIKES_RAISE.Volume = 1f;
            //SHOT_HIT = Content.Load<SoundEffect>(@"hit");
            //ROCK_HIT = Content.Load<SoundEffect>(@"smack");
            //SLING_ROCK = Content.Load<SoundEffect>(@"huck");
            //OWW_ALLY = Content.Load<SoundEffect>(@"oww");
            //OWW_ENEMY = Content.Load<SoundEffect>(@"bark");
            //BODY_FALL = Content.Load<SoundEffect>(@"thud");
            //CHARGE_ROAR = Content.Load<SoundEffect>(@"roar2");
            //LOOT_SOUND = Content.Load<SoundEffect>(@"coin3");
            //COIN_SOUND = Content.Load<SoundEffect>(@"coin2");
            //SHIELD_BREAK = Content.Load<SoundEffect>(@"shield");
            //PIKE_0 = Content.Load<SoundEffect>(@"pike1");
            //PIKE_1 = Content.Load<SoundEffect>(@"pike2");
            //PIKE_2 = Content.Load<SoundEffect>(@"pike3");
            //PIKE_3 = Content.Load<SoundEffect>(@"pike4");
            //PIKE_4 = Content.Load<SoundEffect>(@"pike5");
            //PIKE_5 = Content.Load<SoundEffect>(@"pike6");
            //MELEE_CLANG_0 = Content.Load<SoundEffect>(@"clang2");
            //MELEE_CLANG_1 = Content.Load<SoundEffect>(@"clang3");
            //MELEE_CLANG_2 = Content.Load<SoundEffect>(@"clang4");
            //SLASH = Content.Load<SoundEffect>(@"slash");
            //GAME_OVER = Content.Load<SoundEffect>(@"go");
            //POWER_UP = Content.Load<SoundEffect>(@"loaded");
            //DOPPEL_DOWN = Content.Load<SoundEffect>(@"downbeat");
            //DOPPEL_UP = Content.Load<SoundEffect>(@"loot_sound");
            //COLMILLOS_HURT = Content.Load<SoundEffect>(@"arg");
            //COLMILLOS_YELL = Content.Load<SoundEffect>(@"boss");


            _gameScreens = new ArrayList(3);

            // MAKE LEVELS
            List<Level> levels = new List<Level>(32);
            FileStream stream;
            LevelPipeline.LevelContent levelContent;
            XmlReaderSettings settings = new XmlReaderSettings();
            String[] levelFiles = Directory.GetFiles(@"Content\", @"*_level.xml");

            foreach (String filename in levelFiles)
            {
                stream = new FileStream(filename, FileMode.Open);
                using (XmlReader xmlReader = XmlReader.Create(stream, settings))
                {
                    levelContent = IntermediateSerializer.Deserialize<LevelPipeline.LevelContent>(xmlReader, null);
                }
                stream.Close();
                levels.Add(new Level(levelContent));
            }

            // PLAY LEVEL
            //_currScreen = new LevelScreen(this, levels.ElementAt(0));
            _currScreen = new DungeonScreen(this);
            _gameScreens.Add(_currScreen);
            _gameScreens.Add(new TitleScreen(this));
            _currScreen = (TitleScreen)_gameScreens[1];

            // MAKE FORMATION
            //_gameScreens.Add(new FormationEditorScreen(this));
            //_form = new LevelConstructorForm(levels);
            //_gameScreens.Add( new LevelEditorScreen(this, _form));
            //_form.addFormListener((FormListener)_gameScreens[SCREEN_LEVELEDITOR]);
            //_form.addFormListener((LevelScreen)_currScreen);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (_currScreen != null)
            {
                _currScreen.update(gameTime);
            }
            //UpdateValues();
            base.Update(gameTime);
        }

        public void setScreen(int screenIndex)
        {
            _currScreen = (GameScreen)_gameScreens[screenIndex];

            if (_currScreen is LevelScreen)
                ((LevelScreen)_currScreen).restart();
            else if (screenIndex == 1)
            {
                ((TitleScreen)_currScreen).restart();
            }
        }

        protected override void Draw(GameTime gameTime)
        {

            if (_currScreen != null)
            {
                Matrix mapTransform = Matrix.CreateScale(ZOOM);
                if (!useShaders)
                {
                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget);
                    GraphicsDevice.Viewport = viewport;
                    GraphicsDevice.Clear(screenColor); 
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, mapTransform);

                    if (_currScreen != null)
                    {
                        _currScreen.draw(gameTime, spriteBatch);
                    }

                    base.Draw(gameTime);

                    spriteBatch.End();

                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.White);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null);
                    spriteBatch.Draw(ShaderRenderTarget, drawRectangle, drawSourceRectangle, Color.White);
                    spriteBatch.End();
                }
                else
                {
                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget);
                    GraphicsDevice.Viewport = viewport;
                    GraphicsDevice.Clear(Color.White);
                    //get rid of blurry sprites
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, mapTransform);

                    if (_currScreen != null)
                    {
                        _currScreen.draw(gameTime, spriteBatch);
                    }
                    //spriteBatch.Draw(DOT, new Rectangle(0,0,10, 10), Color.White);

                    base.Draw(gameTime);

                    spriteBatch.End();

                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget2);
                    GraphicsDevice.Clear(Color.White);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null);
                    spriteBatch.Draw(ShaderRenderTarget, testDrawRectangle, new Rectangle(0, 0, SCREENWIDTH / 4, SCREENHEIGHT / 4), Color.White);
                    spriteBatch.End();

                    if (prevFrames.Count<1)
                    {
                        for(int i = 0; i< 7; i++)
                        {
                            prevFrames.Enqueue(ShaderRenderTarget2);
                        }
                    } else
                    {
                        prevFrames.Dequeue();
                        prevFrames.Enqueue(ShaderRenderTarget2);
                    }

                    /*GraphicsDevice.SetRenderTarget(ShaderRenderTarget2);
                    GraphicsDevice.Clear(screenColorShader);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, effect2);
                    spriteBatch.Draw(ShaderRenderTarget, Vector2.Zero, Color.White);
                    spriteBatch.End();*/

                    Texture2D[] arrayOfPrevs = prevFrames.ToArray();
                    effect3.Parameters["text"].SetValue(ShaderRenderTarget2);
                    effect3.Parameters["$PREV"].SetValue(arrayOfPrevs[6]);
                    effect3.Parameters["$PREV1"].SetValue(arrayOfPrevs[5]);
                    effect3.Parameters["$PREV2"].SetValue(arrayOfPrevs[4]);
                    effect3.Parameters["$PREV3"].SetValue(arrayOfPrevs[3]);
                    effect3.Parameters["$PREV4"].SetValue(arrayOfPrevs[2]);
                    effect3.Parameters["$PREV5"].SetValue(arrayOfPrevs[1]);
                    effect3.Parameters["$PREV6"].SetValue(arrayOfPrevs[0]);
                    effect3.CurrentTechnique = effect3.Techniques["gameboy"];
                    effect3.Parameters["texture_size"].SetValue(new Vector2(256, 192));

                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget);
                    GraphicsDevice.Clear(screenColor);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect3);
                    spriteBatch.Draw(ShaderRenderTarget2, testDrawRectangle, testDrawRectangle, Color.White);
                    spriteBatch.End();

                    effect3.Parameters["texture_size"].SetValue(new Vector2(256, 192));
                    effect3.CurrentTechnique = effect3.Techniques["gameboy4"];
                    effect3.Parameters["text"].SetValue(ShaderRenderTarget2);
                    effect3.Parameters["$PASS2"].SetValue(ShaderRenderTarget);
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(screenColorShader);
                    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null);
                    //spriteBatch.Draw(BACKGROUND2, fullDrawRectangle, Color.White);
                    //spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect3);
                    spriteBatch.Draw(BACKGROUND2, fullDrawRectangle, Color.White);
                    spriteBatch.Draw(ShaderRenderTarget, drawRectangle, testDrawRectangle, Color.White);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null);
                    spriteBatch.Draw(SCREEN_TEXT, screenDrawRectangle, Color.White);
                    spriteBatch.Draw(SCREEN_TEXT, screenDrawRectangle2, Color.White);
                    spriteBatch.Draw(SCREEN_TEXT, screenDrawRectangle3, Color.White);
                    spriteBatch.Draw(SCREEN_TEXT, screenDrawRectangle4, Color.White);
                    spriteBatch.End();

                }
            }
        }

        private void UpdateValues()
        {

        }

        internal static SpriteFont getSpriteFont()
        {
            return soldierFont;
        }

        internal static Texture2D getDotTexture()
        {
            return DOT;
        }

        internal Texture2D getGreyscaleClone(Texture2D texture)
        {
            Texture2D greyTexture = new Texture2D(GraphicsDevice, texture.Width, texture.Height);
            Color[] pixels = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(0, texture.Bounds, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                int r = c.R;
                int g = c.G;
                int b = c.B;
                int avg = (r + g + b) / 3;
                pixels[i] = Color.FromNonPremultiplied(avg, avg, avg, c.A);
            }
            greyTexture.SetData<Color>(pixels);
            return greyTexture;
        }

        internal Texture2D getDimmerClone(Texture2D texture)
        {
            return getDimmerClone(texture, 0.60f);
        }

        internal Texture2D getDimmerClone(Texture2D texture, float factor)
        {
            Texture2D greyTexture = new Texture2D(GraphicsDevice, texture.Width, texture.Height);
            Color[] pixels = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(0, texture.Bounds, pixels, 0, pixels.Length);

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                int r = c.R;
                int g = c.G;
                int b = c.B;
                pixels[i] = Color.FromNonPremultiplied((int)(r * factor), (int)(g * factor), (int)(b * factor), c.A);
            }
            greyTexture.SetData<Color>(pixels);
            return greyTexture;
        }

        // helper function to randomly make a number positive or negative
        internal static int getRandPlusMinus(int in_number)
        {
            int number = random.Next(in_number);

            if (random.Next(2) == 0)
                return number;
            else
                return number * -1;
        }

        internal static int getRandPlusMinus()
        {
            int number = 1;

            if (random.Next(2) == 0)
                return number;
            else
                return number * -1;
        }

        internal void fullScreen()
        {
            if (!graphics.IsFullScreen)
            {
                //get user's primary screen size...
                float _ScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                float _ScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                //make it full screen... (borderless if you want to is an option as well)
                graphics.PreferredBackBufferWidth = (int)_ScreenWidth;
                graphics.PreferredBackBufferHeight = (int)_ScreenHeight;
                graphics.IsFullScreen = true;
                graphics.ApplyChanges();
            }
            else
            {
                //get user's primary screen size...
                float _ScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                float _ScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                //make it full screen... (borderless if you want to is an option as well)

                graphics.PreferredBackBufferWidth = SCREENWIDTH+50;
                graphics.PreferredBackBufferHeight = SCREENHEIGHT + 50;
                graphics.IsFullScreen = false;
                graphics.ApplyChanges();
                this.Window.IsBorderless = false;
                this.Window.AllowUserResizing = true;
            }

            setDrawRect();
        }

        void setDrawRect()
        {
            if (!graphics.IsFullScreen)
            {
                if ((float)this.Window.ClientBounds.Height / (float)this.Window.ClientBounds.Width < 0.74f)
                {                  
                    float newWidth = (float)(Window.ClientBounds.Height -50)* 4f / 3f;

                    int drawX = (Window.ClientBounds.Width - (int)newWidth) / 2;
                    int drawY = 0;
                    drawRectangle = new Rectangle(drawX, drawY+25, (int)newWidth, Window.ClientBounds.Height -50);
                } else
                if ((float)this.Window.ClientBounds.Height / (float)this.Window.ClientBounds.Width > 0.76f)
                {
                    float newWidth = (float)(Window.ClientBounds.Width - 50) * 3f / 4f;

                    int drawX = 0;
                    int drawY = (Window.ClientBounds.Height - (int)newWidth) / 2; ;
                    drawRectangle = new Rectangle(drawX+25, drawY, Window.ClientBounds.Width -50, (int)newWidth);
                } else { 
                    drawRectangle = new Rectangle(25, 25, Window.ClientBounds.Width - 50, Window.ClientBounds.Height - 50);
                }
            }
            else if ((float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height / (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width < 0.74f)
            {
                viewport = new Viewport(0, 0, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height);
                float newWidth = (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height * 4f / 3f;

                int drawX = (viewport.Width - (int)newWidth) / 2;
                int drawY = 0;
                drawRectangle = new Rectangle(drawX, drawY, (int)newWidth, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height);
            }
            else if ((float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height / (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width > 0.76f)
            {
                viewport = new Viewport(0, 0, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height);
                float newHeight = (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width * 3f / 4f;

                int drawX = 0;
                int drawY = (viewport.Height - (int)newHeight) / 2;
                drawRectangle = new Rectangle(drawX, drawY, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width, (int)newHeight);
            }
            else
            {
                drawRectangle = new Rectangle(0, 0, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width, graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height);
            }
        }
    }

    public interface LevelEditorGrabbable
    {
        bool selected { get; set; }
        int index { get; set; }
        void setPosition(float x, float y);
        void setPosition(Vector2 pos);
        Vector2 getPosition();
    }
}
