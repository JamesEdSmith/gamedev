﻿using System;
using System.Xml;
using System.IO;
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
using System.Collections;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace PikeAndShot
{
    public class PikeAndShotGame : Microsoft.Xna.Framework.Game
    {
        public const bool DEBUG = true;
        public const bool TEST_BOSS = false;

        public static TimeSpan DUMMY_TIMESPAN;

        public const int SCREENWIDTH = 1024;
        public const int SCREENHEIGHT = 768;

        public const int SCREEN_LEVELPLAY = 0;
        public const int SCREEN_FORMATIONMAKER = 1;
        public const int SCREEN_LEVELEDITOR = 2;

        GraphicsDeviceManager graphics;
        public static Viewport viewport;
        SpriteBatch spriteBatch;

        Rectangle drawRectangle = new Rectangle(0, 0, SCREENWIDTH, SCREENHEIGHT);
        Rectangle drawSourceRectangle = new Rectangle(2, 2, SCREENWIDTH, SCREENHEIGHT);

        RenderTarget2D ShaderRenderTarget;
        RenderTarget2D ShaderRenderTarget2;
        RenderTarget2D ShaderRenderTarget3;
        RenderTarget2D _bloomTarget;

        int _bloomTargetWidth, _bloomTargetHeight;

        Effect _bloomFx, _bloomExtractFx;
        float _blurPower = 0.01f, _baseIntensity = 1f, _bloomIntensity = 0.2f,
            _baseSaturation = 1f, _bloomSaturation = 1.0f, _bloomThreshold = 0.1f;

        static SpriteFont soldierFont;
        public static Random random = new Random();
        public static bool useShaders = true;

        public LevelConstructorForm _form;

        public static Effect effect;
        public static Effect effect2;

        public static Texture2D TERRAIN_DRY_GRASS;

        public static Texture2D PUCELLE_IDLE;
        public static Texture2D PUCELLE_MOTION;

        public static Texture2D PIKEMAN_FEET;
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

        public static Texture2D ARQUEBUSIER_FEET;
        public static Texture2D ARQUEBUSIER_IDLE;
        public static Texture2D ARQUEBUSIER_DEATH;
        public static Texture2D ARQUEBUSIER_MELEE;
        public static Texture2D ARQUEBUSIER_RELOAD;
        public static Texture2D ARQUEBUSIER_SHOOT;
        public static Texture2D ARQUEBUSIER_SMOKE;
        public static Texture2D ARQUEBUSIER_ROUTE;
        public static Texture2D ARQUEBUSIER_ROUTED;
        public static Texture2D ARQUEBUSIER_SHOT1;
        public static Texture2D ARQUEBUSIER_SHOT2;
        public static Texture2D ARQUEBUSIER_SHOT3;
        public static Texture2D ARQUEBUSIER_GROUND;

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

        public static Texture2D BRIGAND2_IDLE;
        public static Texture2D BRIGAND2_DEATH;
        public static Texture2D BRIGAND2_MELEE1;
        public static Texture2D BRIGAND2_DEFEND1;
        public static Texture2D BRIGAND2_CHARGE;

        public static Texture2D HAULER_HAUL;
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


        public static Texture2D COIN;
        public static Texture2D COIN_METER;
        public static Texture2D COIN_METER_BACK;
        public static Texture2D DOPPEL_METER;
        public static Texture2D LOOT;
        public static Texture2D COIN_SPINNA;

        //Utility Graphics
        public static Texture2D DOT;
        public static Texture2D SWORD_POINTER;
        public static Texture2D TEST;

        //Audio
        public static Song THEME_1;
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
        private BattleScreen _currScreen;

        public static float ZOOM = 1.0f;
        public Color screenColor;
        public Color screenColorShader;

        public PikeAndShotGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREENWIDTH;
            graphics.PreferredBackBufferHeight = SCREENHEIGHT;
            graphics.PreferMultiSampling = false;
            if (!DEBUG)
            {
                graphics.IsFullScreen = true;
                useShaders = true;
            }
            else
                useShaders = false;

            Content.RootDirectory = "Content";
            screenColor = new Color(16, 16, 16, 255);
            screenColorShader = new Color(8, 8, 8, 255);
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

            ShaderRenderTarget = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
            ShaderRenderTarget2 = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
            ShaderRenderTarget3 = new RenderTarget2D(GraphicsDevice, SCREENWIDTH, SCREENHEIGHT, false, SurfaceFormat.Color, DepthFormat.None);
            _bloomTargetWidth = SCREENWIDTH / 2;
            _bloomTargetHeight = SCREENHEIGHT / 2;

            _bloomTarget = new RenderTarget2D(GraphicsDevice, _bloomTargetWidth, _bloomTargetHeight, false, SurfaceFormat.Color, DepthFormat.None);

            effect = Content.Load<Effect>(@"cgwg-xna_new");
            effect.Parameters["TexelSize"].SetValue(new Vector2(1f / (float)SCREENWIDTH/1f, 1f / (float)SCREENHEIGHT/1f));
            effect.Parameters["Viewport"].SetValue(new Vector2((float)SCREENWIDTH, (float)SCREENHEIGHT));

            _bloomFx = Content.Load<Effect>("Bloom");
            _bloomExtractFx = Content.Load<Effect>("BloomExtract");

            effect2 = Content.Load<Effect>(@"cgwg-xna");

            //TERRAIN_DRY_GRASS = Content.Load<Texture2D>(@"dry_grass");
            ROAD_TERRAIN = new List<Texture2D>(11);

            for(int i = 0; i < 11; i++)
                ROAD_TERRAIN.Add(getDimmerClone(Content.Load<Texture2D>(@"roadTerrain" + i)));

            TREE0 = getDimmerClone(Content.Load<Texture2D>(@"roadTerrain11"));
            TREE1 = getDimmerClone(Content.Load<Texture2D>(@"tree01"));
            TREE2 = getDimmerClone(Content.Load<Texture2D>(@"tree02"));

            BUSH0 = getDimmerClone(Content.Load<Texture2D>(@"bush00"));
            BUSH1 = getDimmerClone(Content.Load<Texture2D>(@"bush01"));
            BUSH2 = getDimmerClone(Content.Load<Texture2D>(@"bush03"));

            ROAD_HORIZONTAL = getDimmerClone(Content.Load<Texture2D>(@"roadHorizontal"), 0.5f);
            ROAD_TURN = getDimmerClone(Content.Load<Texture2D>(@"roadTurn"), 0.5f);
            ROAD_MILE_MARKER = getDimmerClone(Content.Load<Texture2D>(@"roadMileMarker"), 0.5f);
            ROAD_TURN_MARKER = getDimmerClone(Content.Load<Texture2D>(@"roadTurnMarker"), 0.5f);
            ROAD_HORIZONTAL_2 = getDimmerClone(Content.Load<Texture2D>(@"road_h2"), 0.5f);

            PUCELLE_IDLE = Content.Load<Texture2D>(@"pucelle_ready2");
            PUCELLE_MOTION = Content.Load<Texture2D>(@"pucelle_motion");

            SOLDIER_FEET = Content.Load<Texture2D>(@"soldier_feet");
            SOLDIER_IDLE = Content.Load<Texture2D>(@"soldier_idle");
            SOLDIER2_IDLE = Content.Load<Texture2D>(@"soldier2_idle");
            SOLDIER_DEATH = Content.Load<Texture2D>(@"soldier_death");
            SOLDIER2_DEATH = Content.Load<Texture2D>(@"soldier2_death");
            SOLDIER_MELEE1 = Content.Load<Texture2D>(@"soldier_melee1");
            SOLDIER_DEFEND1 = Content.Load<Texture2D>(@"soldier_defend1");
            SOLDIER_MELEE2 = Content.Load<Texture2D>(@"soldier_melee2");
            SOLDIER_DEFEND2 = Content.Load<Texture2D>(@"soldier_defend2");
            SOLDIER2_MELEE2 = Content.Load<Texture2D>(@"soldier2_melee");
            SOLDIER2_DEFEND2 = Content.Load<Texture2D>(@"soldier2_defend");
            SOLDIER_SHIELDBREAK = Content.Load<Texture2D>(@"soldier_shieldbreak");
            SOLDIER_FALL = Content.Load<Texture2D>(@"soldier_fall");
            SOLDIER_BROKENSHIELD1 = Content.Load<Texture2D>(@"brokenshield1");
            SOLDIER_BROKENSHIELD2 = Content.Load<Texture2D>(@"brokenshield2");
            SOLDIER_BROKENARMOUR = Content.Load<Texture2D>(@"brokenarmour");
            SOLDIER_IDLENOSHIELD = Content.Load<Texture2D>(@"soldier_idlenoshield");
            SOLDIER_ROUTE = Content.Load<Texture2D>(@"soldier_route");
            SOLDIER_ROUTED = Content.Load<Texture2D>(@"soldier_routed");
            SOLDIER_CHARGENOSHIELD = Content.Load<Texture2D>(@"soldier_chargenoshield");
            SOLDIER_CHARGE = Content.Load<Texture2D>(@"soldier_charge");

            CAVALRY_HORSE_IDLE = Content.Load<Texture2D>(@"horse_idle");
            CAVALRY_HORSE_RUN = Content.Load<Texture2D>(@"horse_run");
            CAVALRY_HORSE_HALT = Content.Load<Texture2D>(@"horse_halt");
            CAVALRY_HORSE_TURN = Content.Load<Texture2D>(@"horse_turn");
            CAVALRY_HORSE_DEATH = Content.Load<Texture2D>(@"horse_death");
            CAVALRY_LEFTFOOT = Content.Load<Texture2D>(@"cavalry_leftfoot");
            CAVALRY_RIGHTFOOT = Content.Load<Texture2D>(@"cavalry_rightfoot");
            CAVALRY_LEFTIDLE = Content.Load<Texture2D>(@"cavalry_leftidle");
            CAVALRY_RIGHTIDLE = Content.Load<Texture2D>(@"cavalry_rightidle");
            CAVALRY_LEFTLOWER = Content.Load<Texture2D>(@"cavalry_leftlower");
            CAVALRY_RIGHTLOWER = Content.Load<Texture2D>(@"cavalry_rightlower");
            CAVALRY_LEFTRECOIL = Content.Load<Texture2D>(@"cavalry_leftrecoil");
            CAVALRY_RIGHTRECOIL = Content.Load<Texture2D>(@"cavalry_rightrecoil");

            CAVALRY_FALL = Content.Load<Texture2D>(@"cavalry_fall");
            DISMOUNTED_CAVALRY_IDLE = Content.Load<Texture2D>(@"dismountedcavalry_idle");
            DISMOUNTED_CAVALRY_DEATH = Content.Load<Texture2D>(@"dismountedcavalry_death");
            DISMOUNTED_CAVALRY_IDLENOSHIELD = Content.Load<Texture2D>(@"dismountedcavalry_idlenoshield");
            DISMOUNTED_CAVALRY_SHIELDBREAK = Content.Load<Texture2D>(@"dismountedcavalry_shieldbreak");
            DISMOUNTED_CAVALRY_FALL = Content.Load<Texture2D>(@"dismountedcavalry_fall");
            DISMOUNTED_CAVALRY_MELEE1 = Content.Load<Texture2D>(@"dismountedcavalry_melee1");
            DISMOUNTED_CAVALRY_DEFEND1 = Content.Load<Texture2D>(@"dismountedcavalry_defend1");

            PIKEMAN_FEET = Content.Load<Texture2D>(@"pikeman_feet");
            PIKEMAN_IDLE = Content.Load<Texture2D>(@"pikeman_idle");
            PIKEMAN_LOWER_LOW = Content.Load<Texture2D>(@"pikeman_lower_low");
            PIKEMAN_LOWER_HIGH = Content.Load<Texture2D>(@"pikeman_lower_high");
            PIKEMAN_RECOIL = Content.Load<Texture2D>(@"pikeman_recoil");
            PIKEMAN_DEATH = Content.Load<Texture2D>(@"pikeman_death");
            PIKEMAN1_IDLE = Content.Load<Texture2D>(@"pikeman1_idle");
            PIKEMAN1_LOWER_LOW = Content.Load<Texture2D>(@"pikeman1_lower_low");
            PIKEMAN1_LOWER_HIGH = Content.Load<Texture2D>(@"pikeman1_lower_high");
            PIKEMAN1_RECOIL = Content.Load<Texture2D>(@"pikeman1_recoil");
            PIKEMAN1_DEATH = Content.Load<Texture2D>(@"pikeman1_death");
            PIKEMAN1_MELEE = Content.Load<Texture2D>(@"pikeman1_melee1");
            PIKEMAN1_DEFEND = Content.Load<Texture2D>(@"pikeman1_defend1");
            PIKEMAN2_IDLE = Content.Load<Texture2D>(@"pikeman2_idle");
            PIKEMAN2_LOWER_LOW = Content.Load<Texture2D>(@"pikeman2_lower_low");
            PIKEMAN2_LOWER_HIGH = Content.Load<Texture2D>(@"pikeman2_lower_high");
            PIKEMAN2_RECOIL = Content.Load<Texture2D>(@"pikeman2_recoil");
            PIKEMAN2_DEATH = Content.Load<Texture2D>(@"pikeman2_death");
            PIKEMAN2_MELEE = Content.Load<Texture2D>(@"pikeman2_melee1");
            PIKEMAN2_DEFEND = Content.Load<Texture2D>(@"pikeman2_defend1");
            PIKEMAN_MELEE = Content.Load<Texture2D>(@"pikeman_melee");
            PIKEMAN_ROUTE = Content.Load<Texture2D>(@"pikeman_route");
            PIKEMAN_ROUTED = Content.Load<Texture2D>(@"pikeman_routed");

            ARQUEBUSIER_FEET = Content.Load<Texture2D>(@"arquebusier_feet");
            ARQUEBUSIER_IDLE = Content.Load<Texture2D>(@"arquebusier_idle");
            ARQUEBUSIER_RELOAD = Content.Load<Texture2D>(@"arquebusier_reload");
            ARQUEBUSIER_SHOOT = Content.Load<Texture2D>(@"arquebusier_shoot");
            ARQUEBUSIER_SMOKE = Content.Load<Texture2D>(@"arquebusier_smoke");
            ARQUEBUSIER_DEATH = Content.Load<Texture2D>(@"arquebusier_death");
            ARQUEBUSIER_MELEE = Content.Load<Texture2D>(@"arquebusier_melee");
            ARQUEBUSIER_ROUTE = Content.Load<Texture2D>(@"arquebusier_route");
            ARQUEBUSIER_ROUTED = Content.Load<Texture2D>(@"arquebusier_routed");
            ARQUEBUSIER_SHOT1 = Content.Load<Texture2D>(@"shot1");
            ARQUEBUSIER_SHOT2 = Content.Load<Texture2D>(@"shot2");
            ARQUEBUSIER_SHOT3 = Content.Load<Texture2D>(@"shot3");
            ARQUEBUSIER_GROUND = Content.Load<Texture2D>(@"arquebusier_ground");

            CROSSBOWMAN_IDLE = Content.Load<Texture2D>(@"crossbowman_idle");
            CROSSBOWMAN_RELOAD = Content.Load<Texture2D>(@"crossbowman_reload");
            CROSSBOWMAN_RELOAD2 = Content.Load<Texture2D>(@"crossbowman_reload2");
            CROSSBOWMAN_SHOOT = Content.Load<Texture2D>(@"crossbowman_shoot");
            CROSSBOWMAN_DEATH = Content.Load<Texture2D>(@"crossbowman_death");
            CROSSBOWMAN_MELEE = Content.Load<Texture2D>(@"crossbowman_melee");
            CROSSBOWMAN_MELEE2 = Content.Load<Texture2D>(@"crossbowman_melee2");
            CROSSBOWMAN_DEFEND2 = Content.Load<Texture2D>(@"crossbowman_defend2");
            CROSSBOWMAN_BOLT = Content.Load<Texture2D>(@"crossbowman_bolt");
            CROSSBOWMAN_BOLT2 = Content.Load<Texture2D>(@"crossbowman_bolt2");
            CROSSBOWMAN_GROUND = Content.Load<Texture2D>(@"crossbowman_ground");
            CROSSBOWMAN_ROUTE = Content.Load<Texture2D>(@"crossbowman_route");
            CROSSBOWMAN_ROUTED = Content.Load<Texture2D>(@"crossbowman_routed");
            CROSSBOWMAN_PAVISE = Content.Load<Texture2D>(@"crossbowman_pavise_idle");
            CROSSBOWMAN_PAVISE_PLACE = Content.Load<Texture2D>(@"crossbowman_pavise_place");
            CROSSBOWMAN_SHIELDBREAK = Content.Load<Texture2D>(@"crossbowman_shieldbreak");
            
            PLACED_PAVISE = Content.Load<Texture2D>(@"placed_pavise");
            PAVISE_FALL = Content.Load<Texture2D>(@"pavise_fall");

            DOPPLE_DEATH = Content.Load<Texture2D>(@"dopple_death");
            DOPPLE_IDLE = Content.Load<Texture2D>(@"dopple_idle");
            DOPPLE_SWING1 = Content.Load<Texture2D>(@"dopple_swing1");
            DOPPLE_RELOAD1 = Content.Load<Texture2D>(@"dopple_reload1");
            DOPPLE_ROUTE = Content.Load<Texture2D>(@"dopple_route");
            DOPPLE_ROUTED = Content.Load<Texture2D>(@"dopple_routed");

            SLINGER_IDLE = Content.Load<Texture2D>(@"slinger_idle");
            SLINGER_DEATH = Content.Load<Texture2D>(@"slinger_death");
            SLINGER_MELEE = Content.Load<Texture2D>(@"slinger_melee2");
            SLINGER_DEFEND = Content.Load<Texture2D>(@"slinger_defend2");
            SLINGER_RELOAD = Content.Load<Texture2D>(@"slinger_reload");
            SLINGER_ROCK = Content.Load<Texture2D>(@"slinger_rock");
            SLINGER_SHOOT = Content.Load<Texture2D>(@"slinger_shoot");
            SLINGER_ROUTE = Content.Load<Texture2D>(@"slinger_route");
            SLINGER_ROUTED = Content.Load<Texture2D>(@"slinger_routed");
            SLINGER_RETREAT = Content.Load<Texture2D>(@"slinger_retreat");

            SKIRMISHER_IDLE = Content.Load<Texture2D>(@"skirmisher_idle");
            SKIRMISHER_RELOAD = Content.Load<Texture2D>(@"skirmisher_reload");
            SKIRMISHER_SHOOT = Content.Load<Texture2D>(@"skirmisher_shoot");
            SKIRMISHER_JAVELIN = Content.Load<Texture2D>(@"skirmisher_javelin");
            SKIRMISHER_GROUND = Content.Load<Texture2D>(@"skirmisher_ground");
            SKIRMISHER_DEATH = Content.Load<Texture2D>(@"skirmisher_death");
            SKIRMISHER_MELEE = Content.Load<Texture2D>(@"skirmisher_melee2");
            SKIRMISHER_DEFEND = Content.Load<Texture2D>(@"skirmisher_defend2");
            SKIRMISHER_RETREAT = Content.Load<Texture2D>(@"skirmisher_retreat");

            BERZERKER_IDLE = Content.Load<Texture2D>(@"gobraider1_idle");
            BERZERKER_DEATH = Content.Load<Texture2D>(@"gobraider1_death");
            BERZERKER_MELEE1 = Content.Load<Texture2D>(@"gobraider1_melee1");
            BERZERKER_DEFEND1 = Content.Load<Texture2D>(@"gobraider1_defend1");
            BERZERKER_MELEE2 = Content.Load<Texture2D>(@"gobraider1_melee2");
            BERZERKER_DEFEND2 = Content.Load<Texture2D>(@"gobraider1_defend2");
            BERZERKER_ROUTE = Content.Load<Texture2D>(@"beserker_route");
            BERZERKER_ROUTED = Content.Load<Texture2D>(@"beserker_routed");
            BERZERKER_IDLENOSHIELD = Content.Load<Texture2D>(@"gobraider1_idlenoshield");
            BERZERKER_SHIELDBREAK = Content.Load<Texture2D>(@"gobraider1_shieldbreak");
            BERZERKER_FALL = Content.Load<Texture2D>(@"gobraider1_fall");
            BERZERKER_CHARGENOSHIELD = Content.Load<Texture2D>(@"gobraider1_chargenoshield");
            BERZERKER_CHARGE = Content.Load<Texture2D>(@"gobraider1_charge");

            BERZERKER2_IDLE = Content.Load<Texture2D>(@"gobraider2_idle");
            BERZERKER2_DEATH = Content.Load<Texture2D>(@"gobraider2_death");
            BERZERKER2_MELEE1 = Content.Load<Texture2D>(@"gobraider2_melee1");
            BERZERKER2_DEFEND1 = Content.Load<Texture2D>(@"gobraider2_defend1");
            BERZERKER2_MELEE2 = Content.Load<Texture2D>(@"gobraider2_melee2");
            BERZERKER2_DEFEND2 = Content.Load<Texture2D>(@"gobraider2_defend2");
            BERZERKER2_ROUTE = Content.Load<Texture2D>(@"beserker_route");
            BERZERKER2_ROUTED = Content.Load<Texture2D>(@"beserker_routed");
            BERZERKER2_IDLENOSHIELD = Content.Load<Texture2D>(@"gobraider2_idlenoshield");
            BERZERKER2_SHIELDBREAK = Content.Load<Texture2D>(@"gobraider2_shieldbreak");
            BERZERKER2_FALL = Content.Load<Texture2D>(@"gobraider2_fall");
            BERZERKER2_CHARGENOSHIELD = Content.Load<Texture2D>(@"gobraider2_chargenoshield");
            BERZERKER2_CHARGE = Content.Load<Texture2D>(@"gobraider2_charge");

            BRIGAND1_IDLE = Content.Load<Texture2D>(@"berzerker_idle");
            BRIGAND1_DEATH = Content.Load<Texture2D>(@"bezerker_death");
            BRIGAND1_MELEE1 = Content.Load<Texture2D>(@"berserker_melee2");
            BRIGAND1_DEFEND1 = Content.Load<Texture2D>(@"berserker_defend2");
            BRIGAND1_CHARGE = Content.Load<Texture2D>(@"berserker_charge");

            BRIGAND2_IDLE = Content.Load<Texture2D>(@"brigand_idle");
            BRIGAND2_DEATH = Content.Load<Texture2D>(@"brigand_death");
            BRIGAND2_MELEE1 = Content.Load<Texture2D>(@"brigand_melee2");
            BRIGAND2_DEFEND1 = Content.Load<Texture2D>(@"brigand_defend2");
            BRIGAND2_CHARGE = Content.Load<Texture2D>(@"brigand_charge");

            HAULER_HAUL = Content.Load<Texture2D>(@"hauler_haul");

            WOLF_IDLE = Content.Load<Texture2D>(@"wolf_idle");
            WOLF_RUN = Content.Load<Texture2D>(@"wolf_run");
            WOLF_SPOOKED = Content.Load<Texture2D>(@"wolf_spooked");
            WOLF_TURN = Content.Load<Texture2D>(@"wolf_turn");
            WOLF_BITE = Content.Load<Texture2D>(@"wolf_bite");
            WOLF_MELEE = Content.Load<Texture2D>(@"wolf_melee");
            WOLF_DEFEND = Content.Load<Texture2D>(@"wolf_defend");
            WOLF_KILL = Content.Load<Texture2D>(@"wolf_kill");
            WOLF_HOWL = Content.Load<Texture2D>(@"wolf_howl");

            WOLF_IDLE_COL = Content.Load<Texture2D>(@"wolf_idle_col");
            WOLF_TURN_COL = Content.Load<Texture2D>(@"wolf_turn_col");
            WOLF_ATTACK_COL = Content.Load<Texture2D>(@"wolf_bite_col");
            WOLF_HOWL_COL = Content.Load<Texture2D>(@"wolf_howl_col");
            WOLF_RUN_COL = Content.Load<Texture2D>(@"wolf_run_col");
            WOLF_GETUP_COL = Content.Load<Texture2D>(@"wolf_getup_col");

            WOLF_IDLEg = getGreyscaleClone(WOLF_IDLE);
            WOLF_RUNg = getGreyscaleClone(WOLF_RUN);
            WOLF_SPOOKEDg = getGreyscaleClone(WOLF_SPOOKED);
            WOLF_TURNg = getGreyscaleClone(WOLF_TURN);
            WOLF_BITEg = getGreyscaleClone(WOLF_BITE);
            WOLF_MELEEg = getGreyscaleClone(WOLF_MELEE);
            WOLF_DEFENDg = getGreyscaleClone(WOLF_DEFEND);
            WOLF_KILLg = getGreyscaleClone(WOLF_KILL);
            WOLF_HOWLg = getGreyscaleClone(WOLF_HOWL);

            COLMILLOS_IDLE = Content.Load<Texture2D>(@"los_colmillos_idle0");
            COLMILLOS_IDLENOSHIELD = Content.Load<Texture2D>(@"los_colmillos_idle1");
            COLMILLOS_IDLENOARMOUR = Content.Load<Texture2D>(@"los_colmillos_idle2");
            COLMILLOS_SHIELDBREAK = Content.Load<Texture2D>(@"los_colmillos_shieldbreak");

            COLMILLOS_FALL = Content.Load<Texture2D>(@"los_colmillos_fall1");
            COLMILLOS_FALLNOSHIELD = Content.Load<Texture2D>(@"los_colmillos_fall2");
            COLMILLOS_ATTACK = Content.Load<Texture2D>(@"los_colmillos_attack");
            COLMILLOS_ATTACK2 = Content.Load<Texture2D>(@"los_colmillos_attack2");
            COLMILLOS_ATTACK3 = Content.Load<Texture2D>(@"los_colmillos_attack3");
            COLMILLOS_DEATH = Content.Load<Texture2D>(@"los_colmillos_death");
            COLMILLOS_HELMET = Content.Load<Texture2D>(@"helmet");
            COLMILLOS_RISE = Content.Load<Texture2D>(@"los_colmillos_rise");
            COLMILLOS_HOWL = Content.Load<Texture2D>(@"los_colmillos_howl");
            COLMILLOS_HOWL_NOSHIELD = Content.Load<Texture2D>(@"los_colmillos_howl_noshield");
            COLMILLOS_HOWL_NOARMOUR = Content.Load<Texture2D>(@"los_colmillos_howl_noarmour");
            COLMILLOS_STAGGER = Content.Load<Texture2D>(@"los_colmillos_stagger");
            FALCHION_THROWN = Content.Load<Texture2D>(@"falchion_thrown");
            FALCHION_DIRT = Content.Load<Texture2D>(@"falchion_dirt");

            PEASANT1_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_01_idle"));
            PEASANT2_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_02_idle"));
            PEASANT3_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_03_idle"));
            PEASANT4_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_04_idle"));
            PEASANT5_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_05_idle"));
            PEASANT6_IDLE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_06_idle"));

            PEASANT1_FLEE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_01_fleeing"));
            PEASANT5_FLEE = getDimmerClone(Content.Load<Texture2D>(@"npc_1_05_fleeing"));

            GOBLIN_FEET = Content.Load<Texture2D>(@"goblin_feet");
            BROWN_FEET = Content.Load<Texture2D>(@"brown_feet");
            BLUE_FEET = Content.Load<Texture2D>(@"blue_feet");
            SLINGER_GROUND = Content.Load<Texture2D>(@"slinger_ground");

            COIN = Content.Load<Texture2D>(@"coin");
            COIN_METER = Content.Load<Texture2D>(@"coin_meter");
            COIN_METER_BACK = Content.Load<Texture2D>(@"coin_meter_back");
            LOOT = Content.Load<Texture2D>(@"loot");
            DOPPEL_METER = Content.Load<Texture2D>(@"doppel_meter");

            DOT = Content.Load<Texture2D>(@"dot");
            SWORD_POINTER = Content.Load<Texture2D>(@"sword_pointer");
            //TEST = Content.Load<Texture2D>(@"sfea");

            COIN_SPINNA = Content.Load<Texture2D>(@"coin_spinna");

            THEME_1 = Content.Load<Song>(@"boss01");
            SHOT_0 = Content.Load<SoundEffect>(@"shot00");
            SHOT_1 = Content.Load<SoundEffect>(@"shot01");
            SHOT_2 = Content.Load<SoundEffect>(@"shot02");
            SHOT_3 = Content.Load<SoundEffect>(@"shot04");
            SHOT_4 = Content.Load<SoundEffect>(@"shot05");
            PIKES_LOWER = Content.Load<SoundEffect>(@"down").CreateInstance();
            PIKES_LOWER.Volume = 0.5f;
            PIKES_RAISE = Content.Load<SoundEffect>(@"drop").CreateInstance();
            PIKES_RAISE.Volume = 0.5f;
            SHOT_HIT = Content.Load<SoundEffect>(@"hit");
            ROCK_HIT = Content.Load<SoundEffect>(@"smack");
            SLING_ROCK = Content.Load<SoundEffect>(@"huck");
            OWW_ALLY = Content.Load<SoundEffect>(@"oww");
            OWW_ENEMY = Content.Load<SoundEffect>(@"bark");
            BODY_FALL = Content.Load<SoundEffect>(@"thud");
            CHARGE_ROAR = Content.Load<SoundEffect>(@"roar2");
            LOOT_SOUND = Content.Load<SoundEffect>(@"coin3");
            COIN_SOUND = Content.Load<SoundEffect>(@"coin2");
            SHIELD_BREAK = Content.Load<SoundEffect>(@"shield");
            PIKE_0 = Content.Load<SoundEffect>(@"pike1");
            PIKE_1 = Content.Load<SoundEffect>(@"pike2");
            PIKE_2 = Content.Load<SoundEffect>(@"pike3");
            PIKE_3 = Content.Load<SoundEffect>(@"pike4");
            PIKE_4 = Content.Load<SoundEffect>(@"pike5");
            PIKE_5 = Content.Load<SoundEffect>(@"pike6");
            MELEE_CLANG_0 = Content.Load<SoundEffect>(@"clang2");
            MELEE_CLANG_1 = Content.Load<SoundEffect>(@"clang3");
            MELEE_CLANG_2 = Content.Load<SoundEffect>(@"clang4");
            SLASH = Content.Load<SoundEffect>(@"slash");
            GAME_OVER = Content.Load<SoundEffect>(@"go");
            POWER_UP = Content.Load<SoundEffect>(@"loaded");
            DOPPEL_DOWN = Content.Load<SoundEffect>(@"downbeat");
            DOPPEL_UP = Content.Load<SoundEffect>(@"loot_sound");
            COLMILLOS_HURT = Content.Load<SoundEffect>(@"arg");
            COLMILLOS_YELL = Content.Load<SoundEffect>(@"boss");

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
            _currScreen = new LevelScreen(this, levels.ElementAt(3));
            _gameScreens.Add(_currScreen);

            // MAKE FORMATION
            _gameScreens.Add(new FormationEditorScreen(this));
            _form = new LevelConstructorForm(levels);
            _gameScreens.Add( new LevelEditorScreen(this, _form));
            _form.addFormListener((FormListener)_gameScreens[SCREEN_LEVELEDITOR]);
            _form.addFormListener((LevelScreen)_currScreen);
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
            UpdateValues();
            base.Update(gameTime);
        }

        public void setScreen(int screenIndex)
        {
            if (screenIndex >= 0 && screenIndex <= SCREEN_LEVELEDITOR)
                _currScreen = (BattleScreen)_gameScreens[screenIndex];

            if (screenIndex == SCREEN_LEVELEDITOR)
                _form.Show();
            else
                _form.Hide();

            if (screenIndex == SCREEN_LEVELPLAY)
                ((LevelScreen)_currScreen).restart();
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
                    GraphicsDevice.Clear(screenColor);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null);
                    spriteBatch.Draw(ShaderRenderTarget, drawRectangle, drawSourceRectangle, Color.White);
                    spriteBatch.End();
                }
                else
                {
                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget);
                    GraphicsDevice.Viewport = viewport;
                    GraphicsDevice.Clear(screenColorShader);
                    //get rid of blurry sprites
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, mapTransform);

                    if (_currScreen != null)
                    {
                        _currScreen.draw(gameTime, spriteBatch);
                    }
                    base.Draw(gameTime);

                    spriteBatch.End();

                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget2);

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, effect);
                    spriteBatch.Draw(ShaderRenderTarget, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    
                    GraphicsDevice.SetRenderTarget(ShaderRenderTarget3);

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, effect2);
                    spriteBatch.Draw(ShaderRenderTarget2, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    
                    GraphicsDevice.SetRenderTarget(_bloomTarget);
                    GraphicsDevice.Clear(Color.Black);
                    
                    //Extract highlights on the original image using BloomExtract
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _bloomExtractFx);
                    spriteBatch.Draw(ShaderRenderTarget3, new Rectangle(0, 0, _bloomTargetWidth, _bloomTargetHeight), null, Color.White);
                    spriteBatch.End();

                    //Set original backbuffer as target
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Black);

                    //Compose bloomed image using Bloom effect
                    GraphicsDevice.Textures[1] = _bloomTarget;

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, _bloomFx);

                    spriteBatch.Draw(ShaderRenderTarget3, drawRectangle, drawSourceRectangle, Color.White);

                    spriteBatch.End();
                }
            }
        }

        private void UpdateValues()
        {
            _bloomFx.Parameters["BlurPower"].SetValue(_blurPower);
            _bloomFx.Parameters["BaseIntensity"].SetValue(_baseIntensity);
            _bloomFx.Parameters["BloomIntensity"].SetValue(_bloomIntensity);
            _bloomFx.Parameters["BaseSaturation"].SetValue(_baseSaturation);
            _bloomFx.Parameters["BloomSaturation"].SetValue(_bloomSaturation);

            _bloomExtractFx.Parameters["BloomThreshold"].SetValue(_bloomThreshold);
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
            return getDimmerClone(texture, 0.75f);
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
            graphics.ToggleFullScreen();
            setDrawRect();
        }

        void setDrawRect()
        {
            if (graphics.IsFullScreen && (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height / (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width < 0.74f)
            {
                float oldWidth = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                float newWidth = (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height * 4f / 3f;
                int drawWidth = (int)(viewport.Width * newWidth / oldWidth);

                int drawX = (viewport.Width - drawWidth) / 2;
                int drawY = (viewport.Height - SCREENHEIGHT) / 2;
                drawRectangle = new Rectangle(drawX, drawY, drawWidth, SCREENHEIGHT);
            }
            else if (graphics.IsFullScreen && (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height / (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width > 0.76f)
            {
                float oldHeight = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                float newHeight = (float)graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width * 3f / 4f;
                int drawHeight = (int)(viewport.Height * newHeight / oldHeight);

                int drawX = (viewport.Width - SCREENWIDTH) / 2;
                int drawY = (viewport.Height - drawHeight) / 2;
                drawRectangle = new Rectangle(drawX, drawY, SCREENWIDTH, drawHeight);
            }
            else
            {
                drawRectangle = new Rectangle(0, 0, SCREENWIDTH, SCREENHEIGHT);
            }
        }
    }
}
