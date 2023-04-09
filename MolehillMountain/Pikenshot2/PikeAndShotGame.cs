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
        Rectangle fullDrawRectangle = new Rectangle(0, 0, SCREENWIDTH + 50, SCREENHEIGHT + 50);
        Rectangle screenDrawRectangle = new Rectangle(0, 25, 25, SCREENHEIGHT);
        Rectangle screenDrawRectangle2 = new Rectangle(SCREENWIDTH + 25, 25, 25, SCREENHEIGHT);
        Rectangle screenDrawRectangle3 = new Rectangle(0, 22, SCREENWIDTH + 50, 3);
        Rectangle screenDrawRectangle4 = new Rectangle(0, SCREENHEIGHT + 25, SCREENWIDTH + 50, 3);
        Rectangle drawSourceRectangle = new Rectangle(2, 2, SCREENWIDTH, SCREENHEIGHT);

        RenderTarget2D ShaderRenderTarget;
        RenderTarget2D ShaderRenderTarget2;
        RenderTarget2D ShaderRenderTarget3;
        RenderTarget2D ShaderRenderTarget4;

        static SpriteFont soldierFont;
        public static SpriteFont MOLE_FONT;
        public static SpriteFont GOBLIN_FONT;
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
        public static Texture2D SLIDE;
        public static Texture2D TEXT;

        //Utility Graphics
        public static Texture2D DOT;
        public static Texture2D SWORD_POINTER;
        public static Texture2D TEST;

        public static Texture2D MOLE_MINER_WALKING;
        public static Texture2D MOLE_MINER_DIGGING;
        public static Texture2D MOLE_MINER_NUDGE;
        public static Texture2D MINER_SLING;
        public static Texture2D MOLE_SQUASHED;
        public static Texture2D MOLE_DIZZY;

        public static Texture2D PALETTE;
        public static Texture2D BACKGROUND;
        public static Texture2D BACKGROUND2;

        public static Texture2D SCREEN_TEXT;
        public static Texture2D TUNNEL;
        public static Texture2D TUNNEL_REVEAL;
        public static Texture2D TUNNEL_FIRE;
        public static Texture2D FIRE;
        public static Texture2D TUNNEL_FIRE_BACK;
        public static Texture2D HEART;

        public static Texture2D TURNIP_SHAKE;
        public static Texture2D TURNIP_SPLIT;
        public static Texture2D TURNIP_TWIRL;

        public static Texture2D ONION_SHAKE;
        public static Texture2D ONION_SPLIT;
        public static Texture2D ONION_TWIRL;

        public static Texture2D RAT_WALKING;
        public static Texture2D RAT_NUDGE;
        public static Texture2D RAT_CRUSHED;
        public static Texture2D RAT_DIGGING;
        public static Texture2D RAT_MAD;
        public static Texture2D RAT_SNIFF;

        public static Texture2D GRUB_EGG;
        public static Texture2D GRUB_GRUB;
        public static Texture2D GRUB_LOOK;

        public static Texture2D BEEBLE_WALKING;
        public static Texture2D BEEBLE_CHARGE;
        public static Texture2D BEEBLE_ZOOM;
        public static Texture2D BEEBLE_CRASH;
        public static Texture2D BEEBLE_SPAWN;
        public static Texture2D BEEBLE_CRUSHED;

        public static Texture2D SALAMANDO_WALKING;
        public static Texture2D SALAMANDO_SPITTING;

        public static Texture2D SLINGSHOT;
        public static Texture2D STONE;
        public static Texture2D STONE_IMPACT;
        public static Texture2D DOOR;
        public static Texture2D FIGHT_CLOUD;
        public static Texture2D DIZZY_MARK;

        public static Texture2D UNSEEN_WALK;
        public static Texture2D UNSEEN_WALK2;

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
        private Dictionary<Texture2D, Texture2D> flashTextures;

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
                this.Window.Position = new Point((int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - (float)SCREENWIDTH) / 2 - 50, (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - (float)SCREENHEIGHT) / 2 - 50);
                this.Window.IsBorderless = true;
                graphics.PreferredBackBufferWidth = SCREENWIDTH + 50;
                graphics.PreferredBackBufferHeight = SCREENHEIGHT + 50;
                graphics.IsFullScreen = false;
                graphics.PreferMultiSampling = false;
                this.Window.AllowUserResizing = false;
                graphics.ApplyChanges();
                useShaders = true;
            }
            else
                useShaders = false;

            this.Window.ClientSizeChanged += Window_ClientSizeChanged;

            Content.RootDirectory = "Content";
            screenColor = new Color(166, 172, 132, 150);
            screenColorShader = new Color(166, 172, 132, 0);

            flashTextures = new Dictionary<Texture2D, Texture2D>();
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

            DOT = CreateTexture(GraphicsDevice, 1, 1, pixel => Color.Black);

            setDrawRect();

            soldierFont = Content.Load<SpriteFont>("SpriteFont1");
            MOLE_FONT = Content.Load<SpriteFont>("Alagard");
            GOBLIN_FONT = Content.Load<SpriteFont>("GoblinFont");

            PALETTE = Content.Load<Texture2D>(@"palette");
            BACKGROUND = Content.Load<Texture2D>(@"safeplace_fullsize");
            //BACKGROUND2 = Content.Load<Texture2D>(@"bg_red");
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


            MOLE_MINER_WALKING = Content.Load<Texture2D>(@"mole_miner_sprite");
            MOLE_MINER_DIGGING = Content.Load<Texture2D>(@"mole_miner_sprite_dig");
            MOLE_MINER_NUDGE = Content.Load<Texture2D>(@"mole_miner_nudge");
            MOLE_SQUASHED = Content.Load<Texture2D>(@"mole_squashed");
            MINER_SLING = Content.Load<Texture2D>(@"miner_slingshot");
            MOLE_DIZZY = Content.Load<Texture2D>(@"mole_miner_dizzy");
            DIZZY_MARK = Content.Load<Texture2D>(@"seeing_stars");

            UNSEEN_WALK = Content.Load<Texture2D>(@"questionmark_sprite");
            UNSEEN_WALK2 = Content.Load<Texture2D>(@"unseen_sprite");

            TUNNEL = Content.Load<Texture2D>(@"tunnel");
            TUNNEL_REVEAL = Content.Load<Texture2D>(@"dirt");
            TUNNEL_FIRE = Content.Load<Texture2D>(@"tunnel_fire");
            FIRE = Content.Load<Texture2D>(@"fire");
            TUNNEL_FIRE_BACK = Content.Load<Texture2D>(@"tunnel_fire_back");
            HEART = Content.Load<Texture2D>(@"heart");

            TURNIP_SHAKE = Content.Load<Texture2D>(@"turnip_fall");
            TURNIP_TWIRL = Content.Load<Texture2D>(@"turnip_twirl");
            TURNIP_SPLIT = Content.Load<Texture2D>(@"turnip_split");

            ONION_SHAKE = Content.Load<Texture2D>(@"onion_shake");
            ONION_TWIRL = Content.Load<Texture2D>(@"onion_twirl");
            ONION_SPLIT = Content.Load<Texture2D>(@"onion_split");

            RAT_WALKING = Content.Load<Texture2D>(@"rat_walk_one");
            RAT_NUDGE = Content.Load<Texture2D>(@"rat_push");
            RAT_CRUSHED = Content.Load<Texture2D>(@"rat_crushed");
            RAT_DIGGING = Content.Load<Texture2D>(@"rat_digging");
            RAT_MAD = Content.Load<Texture2D>(@"rat_mad");
            RAT_SNIFF = Content.Load<Texture2D>(@"rat_sniff");

            BEEBLE_WALKING = Content.Load<Texture2D>(@"beeble_walk");           
            BEEBLE_CHARGE = Content.Load<Texture2D>(@"beeble_charge");
            BEEBLE_ZOOM = Content.Load<Texture2D>(@"beeble_zoom");
            BEEBLE_CRASH = Content.Load<Texture2D>(@"beeble_crash");
            BEEBLE_SPAWN = Content.Load<Texture2D>(@"beeble_spawn");
            BEEBLE_CRUSHED = Content.Load<Texture2D>(@"beeble_crushed");

            SALAMANDO_WALKING = Content.Load<Texture2D>(@"salamando_walk");
            SALAMANDO_SPITTING = Content.Load<Texture2D>(@"salamando_spit");

            GRUB_EGG = Content.Load<Texture2D>(@"egg");
            GRUB_GRUB = Content.Load<Texture2D>(@"grub_craw_sml");
            GRUB_LOOK = Content.Load<Texture2D>(@"grub_look");

            SLINGSHOT = Content.Load<Texture2D>(@"slingshot");
            STONE = Content.Load<Texture2D>(@"stone");
            STONE_IMPACT = Content.Load<Texture2D>(@"stone_impact");

            DOOR = Content.Load<Texture2D>(@"door");
            FIGHT_CLOUD = Content.Load<Texture2D>(@"fight_cloud");


            SANDBOX = Content.Load<Texture2D>(@"sandbox");

            TEST = new Texture2D(GraphicsDevice, 1, 1);
            Color[] colors = { Color.Black };
            TEST.SetData(colors);


            COIN_SPINNA = Content.Load<Texture2D>(@"coin_spinna");
            SPARKLE = Content.Load<Texture2D>(@"sparkle");
            //TITLE_ANIMATION = Content.Load<Texture2D>(@"title_animation");
            //TITLE_ANIMATION = Content.Load<Texture2D>(@"title_screen");
            TITLE_ANIMATION = Content.Load<Texture2D>(@"title_screen");
            //SLIDE = Content.Load<Texture2D>(@"slide");
            //TEXT = Content.Load<Texture2D>(@"text");


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
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, mapTransform);

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

                    if (prevFrames.Count < 1)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            prevFrames.Enqueue(ShaderRenderTarget2);
                        }
                    }
                    else
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

                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget);
                    GraphicsDevice.Clear(screenColor);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect3);
                    spriteBatch.Draw(ShaderRenderTarget2, testDrawRectangle, testDrawRectangle, Color.White);
                    spriteBatch.End();

                    effect3.CurrentTechnique = effect3.Techniques["gameboy4"];
                    effect3.Parameters["text"].SetValue(ShaderRenderTarget2);
                    effect3.Parameters["$PASS2"].SetValue(ShaderRenderTarget);
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(screenColorShader);
                    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null);
                    //spriteBatch.Draw(BACKGROUND2, fullDrawRectangle, Color.White);
                    //spriteBatch.End();
                    
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect3);
                    spriteBatch.Draw(BACKGROUND, fullDrawRectangle, Color.White);
                    spriteBatch.Draw(ShaderRenderTarget, drawRectangle, testDrawRectangle, Color.White);
                    //spriteBatch.Draw(ShaderRenderTarget, new Rectangle(drawRectangle.X, drawRectangle.Y, drawRectangle.Width*2, drawRectangle.Height * 2), testDrawRectangle, Color.White);
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

                graphics.PreferredBackBufferWidth = SCREENWIDTH + 50;
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
                    float newWidth = (float)(Window.ClientBounds.Height - 50) * 4f / 3f;

                    int drawX = (Window.ClientBounds.Width - (int)newWidth) / 2;
                    int drawY = 0;
                    drawRectangle = new Rectangle(drawX, drawY + 25, (int)newWidth, Window.ClientBounds.Height - 50);
                }
                else
                if ((float)this.Window.ClientBounds.Height / (float)this.Window.ClientBounds.Width > 0.76f)
                {
                    float newWidth = (float)(Window.ClientBounds.Width - 50) * 3f / 4f;

                    int drawX = 0;
                    int drawY = (Window.ClientBounds.Height - (int)newWidth) / 2; ;
                    drawRectangle = new Rectangle(drawX + 25, drawY, Window.ClientBounds.Width - 50, (int)newWidth);
                }
                else
                {
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

        public Texture2D getFlashTexture(Texture2D bitmap)
        {
            if (flashTextures.ContainsKey(bitmap))
            {
                return flashTextures[bitmap];
            }
            else
            {
                //create flash texture
                Color[] pixelData = new Color[bitmap.Width * bitmap.Height];
                bitmap.GetData<Color>(pixelData);

                for (int i = 0; i < pixelData.Length; i++)
                {
                    if (pixelData[i].A != 0)
                        pixelData[i] = Color.White;
                }
                Texture2D flashTexture = new Texture2D(bitmap.GraphicsDevice, bitmap.Width, bitmap.Height);
                flashTexture.SetData<Color>(pixelData);
                flashTextures.Add(bitmap, flashTexture);

                return flashTexture;
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
