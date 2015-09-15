using System;
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


namespace Cheyenne_Show_offs
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GameScreen
    {
        protected KeyboardState previousKeyboardState;
        protected GamePadState previousGamePadState;

        public Game1 game;

        public GameScreen(Game1 game)
        {
            this.game = game;
        }

        public virtual void update(GameTime gameTime)
        {
            
        }

        public virtual void draw(GameTime time, SpriteBatch sprite)
        {

        }

        public virtual void input(GameTime time)
        {
            
        }
    }

    public class SplashScreen : GameScreen
    {
        Vector2 bg1Pos;
        byte titleAlpha;
        int alphaFlip;
        float timer;
        bool anvil;

        public SplashScreen(Game1 game)
            : base(game)
        {
            bg1Pos = new Vector2(Game1.SCREENWIDTH/2 - Game1.IMAGE_ANVIL.Width/2, Game1.SCREENHEIGHT/2 - Game1.IMAGE_ANVIL.Height/2);
            titleAlpha = 0;
            alphaFlip = 0;
            timer = 0;
            anvil = false;
        }

        public override void input(GameTime time)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            if ((gamePadState.Buttons.Back == ButtonState.Pressed && previousGamePadState.Buttons.Back == ButtonState.Released) || (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape)))
                game.Exit();

            if ((gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)) ||
                (gamePadState.IsButtonDown(Buttons.Start) && previousGamePadState.IsButtonUp(Buttons.Start)) ||
                (gamePadState.IsButtonDown(Buttons.B) && previousGamePadState.IsButtonUp(Buttons.B)) ||
                (gamePadState.IsButtonDown(Buttons.X) && previousGamePadState.IsButtonUp(Buttons.X)) ||
                (gamePadState.IsButtonDown(Buttons.Y) && previousGamePadState.IsButtonUp(Buttons.Y)) ||
                (keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) ||
                (keyboardState.IsKeyDown(Keys.X) && previousKeyboardState.IsKeyUp(Keys.X)) ||
                (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C)))
            {
                game.setMode(Game1.modes.title);
            }

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        public override void update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (alphaFlip == 0)
                titleAlpha = (byte)(255 * timer / 1.5);
            else if (alphaFlip == 1)
            {
                if (timer >= 0 && !anvil)
                {
                    anvil = true;
                    Game1.SOUND_ANVIL.Play();
                    Game1.SOUND_RING.Play();
                }
                titleAlpha = 255;
            }
            else if (alphaFlip == 2)
                titleAlpha = (byte)(255 - 255 * timer / 1.5);
            else
                game.setMode(Game1.modes.title);
            if (timer >= 1.5 )
            {
                timer = 0;
                alphaFlip++;
            }
        }

        public override void draw(GameTime time, SpriteBatch sprite)
        {
            sprite.Draw(Game1.IMAGE_ANVIL, new Rectangle((int)bg1Pos.X, (int)bg1Pos.Y, Game1.IMAGE_ANVIL.Width, Game1.IMAGE_ANVIL.Height), new Color(Color.White, titleAlpha));
            game.drawText(sprite, Game1.FONT, "Smith's Fine Electric Amusements", new Color(new Color(35, 55, 58), titleAlpha), new Color(Color.White, titleAlpha), 1.5f, 0, new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT / 2 - 150));
            game.drawText(sprite, Game1.FONT, "EST 1983", new Color(new Color(35, 55, 58), titleAlpha), new Color(Color.White, titleAlpha), 1, 0, new Vector2(Game1.SCREENWIDTH / 2, 525));
        }
    }

    public class MenuScreen : GameScreen
    {
        Vector2 bg1Pos;
        Vector2 bg2Pos;
        byte titleAlpha;
        bool alphaFlip;
        float timer;

        public MenuScreen(Game1 game) 
            : base(game)
        {
            bg1Pos = Vector2.Zero;
            bg2Pos = new Vector2(-Game1.SCREENWIDTH, 0);
            titleAlpha = 255;
            alphaFlip = false;
            timer = 0;
            previousKeyboardState = Keyboard.GetState();
            previousGamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
        }

        public override void update(GameTime gameTime)
        {
            bg1Pos.X += 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            bg2Pos.X += 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (bg1Pos.X >= Game1.SCREENWIDTH)
                bg1Pos.X -= Game1.SCREENWIDTH*2;
            if (bg2Pos.X >= Game1.SCREENWIDTH)
                bg2Pos.X -= Game1.SCREENWIDTH*2;

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (alphaFlip)
                titleAlpha = (byte)(255 - 255 * timer / 2);
            else
                titleAlpha = (byte)( 255 * timer / 2);
            if (timer >= 2)
            {
                timer = 0;
                alphaFlip = !alphaFlip;
            }
        }

        public override void input(GameTime time)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            if ((gamePadState.Buttons.Back == ButtonState.Pressed && previousGamePadState.Buttons.Back == ButtonState.Released) || (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape)))
                game.Exit();

            if ((gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A))||
                (gamePadState.IsButtonDown(Buttons.Start) && previousGamePadState.IsButtonUp(Buttons.Start)) ||
                (gamePadState.IsButtonDown(Buttons.B) && previousGamePadState.IsButtonUp(Buttons.B)) ||
                (gamePadState.IsButtonDown(Buttons.X) && previousGamePadState.IsButtonUp(Buttons.X)) ||
                (gamePadState.IsButtonDown(Buttons.Y) && previousGamePadState.IsButtonUp(Buttons.Y)) ||
                (keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) ||
                (keyboardState.IsKeyDown(Keys.X) && previousKeyboardState.IsKeyUp(Keys.X)) ||
                (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C)))
            {
                game.setMode(Game1.modes.duel);   
            }

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        public override void draw(GameTime time, SpriteBatch sprite)
        {
            sprite.Draw(Game1.IMAGE_BG, new Rectangle((int)bg1Pos.X, (int)bg1Pos.Y, Game1.SCREENWIDTH, Game1.SCREENHEIGHT), Color.White);
            sprite.Draw(Game1.IMAGE_BG, new Rectangle((int)bg2Pos.X, (int)bg2Pos.Y, Game1.SCREENWIDTH, Game1.SCREENHEIGHT), new Rectangle(0,0, Game1.IMAGE_BG.Width, Game1.IMAGE_BG.Height), Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            sprite.Draw(Game1.IMAGE_TITLE, new Rectangle(Game1.SCREENWIDTH / 2 - Game1.IMAGE_TITLE.Width / 2, 50, Game1.IMAGE_TITLE.Width, Game1.IMAGE_TITLE.Height), Color.White);
            game.drawText(sprite, Game1.FONT, "Press button to duel...", new Color(new Color(35,55,58), titleAlpha), new Color(Color.White, titleAlpha),1.5f, 0, new Vector2(Game1.SCREENWIDTH/2, 500));
        }
    }

    public class DuelScreen : GameScreen
    {
        private const float MOVE_MAX = 5f;
        private const float ACCELLERATION_RATE = 5f;
        private const float DECELLERATION_RATE = 5f;
        private const bool REDUCE_X = true;
        private const bool REDUCE_Y = false;
        private const float BODY_RIGHT = 300f;
        private const float BODY_TOP = 200f;
        private const float BODY_BOTTOM = 500f;
        private const float GUN_RADIUS = 50f;
        public const float DEATH_TIME = 1500f;
        public const float DROP_TIME = 3000f;

        public Gun gun;
        private Arm arm;
        public Vector2 movementVector;
        public Vector2 accellerationVector;
        public GunWindow gunWindow;
        public Score P1Score;
        public PlayerIndex player;
        public ArrayList firedBullets;
        public ArrayList textPopups;
        public bool dead;
        public bool droppedGun;
        private float droppedGunTimer;
        public bool dropping;
        public bool fall;
        public bool won;
        public float deathTimer;
        public Vector2[] fallForces;
        private float dropX;

        public int bullets;

        private Camera camera;

        Vector2 leftArmPos;
        Vector2 bodyPos;
        Vector2 headPos;
        Vector2 leftLegPos;
        Vector2 rightLegPos;
        Vector2 holsterPos;

        Texture2D leftArmText;
        Texture2D bodyText;
        Texture2D headText;
        Texture2D leftLegText;
        Texture2D rightLegText;
        Texture2D holsterText;

        public DuelScreen(Game1 game, PlayerIndex player)
            : base(game)
        {
            movementVector = Vector2.Zero;
            accellerationVector = Vector2.Zero; 
            gun = new Gun(player);
            arm = new Arm(gun);
            P1Score = new Score(this);
            bullets = 6;
            firedBullets = new ArrayList(12);
            textPopups = new ArrayList(20);
            this.player = player;
            this.dead = false;
            this.droppedGun = false;
            this.droppedGunTimer = 0f;
            this.won = false;
            this.deathTimer = 0f;
            dropping = false;

            this.fallForces = new Vector2[8];
            Random rand = new Random();
            for (int i = 0; i < 8; i++)
            {
                fallForces[i] = new Vector2(rand.Next(5), rand.Next(10));
                //fallForces[i].X *= rand.Next(2) == 0 ? -1 : 1;
                fallForces[i].X += 5;
            }

            //set up cowboy body parts drawing locations
            if (player == PlayerIndex.One)
            {
                leftArmPos = new Vector2(280, 275);
                bodyPos = new Vector2(148, 240);
                headPos = new Vector2(245, 157);
                leftLegPos = new Vector2(216, 430);
                rightLegPos = new Vector2(36, 430);
                holsterPos = new Vector2(140, 418);

                leftArmText = Game1.IMAGE_LEFTARM;
                bodyText = Game1.IMAGE_BODY;
                headText = Game1.IMAGE_HEAD;
                leftLegText = Game1.IMAGE_LEFTLEG;
                rightLegText = Game1.IMAGE_RIGHTLEG;
                holsterText = Game1.IMAGE_HOLSTER;

                camera = game.p1Camera;
            }
            else
            {
                leftArmPos = new Vector2(280, 238);
                bodyPos = new Vector2(45, 192);
                headPos = new Vector2(245, 157);
                leftLegPos = new Vector2(30, 440);
                rightLegPos = new Vector2(206, 428);
                holsterPos = new Vector2(140, 418);

                leftArmText = Game1.IMAGE_LEFTARM2;
                bodyText = Game1.IMAGE_BODY2;
                headText = Game1.IMAGE_HEAD2;
                leftLegText = Game1.IMAGE_LEFTLEG2;
                rightLegText = Game1.IMAGE_RIGHTLEG2;
                holsterText = Game1.IMAGE_HOLSTER2;

                camera = game.p2Camera;
            }
        }

        public override void update(GameTime gameTime)
        {
            // arm and gun updates
            if (fall)
            {
                headPos -= fallForces[0];
                leftArmPos -= fallForces[1];
                bodyPos -= fallForces[2];
                leftLegPos -= fallForces[3];
                rightLegPos -= fallForces[4];
                holsterPos -= fallForces[5];
                gun.position -= fallForces[6];
                arm.joints[0] -= fallForces[7];

                arm.update(gameTime, this);

                for (int i = 0; i < 8; i++)
                {
                    fallForces[i].Y -= 0.4f;
                }
            }
            else if (dead)
            {

                if (deathTimer < DEATH_TIME)
                    deathTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                else
                {
                    fall = true;
                    deathTimer = 0;

                }

                arm.update(gameTime, this);
                gun.update(gameTime, this);
            }
            else
            {
                if (gunWindow != null)
                {
                    gunWindow.update(gameTime);
                }
                else
                {
                    gun.update(gameTime, this);
                    arm.update(gameTime, this);
                    if (gun.thrownPosition.Y > Game1.SCREENHEIGHT && !droppedGun)
                    {
                        droppedGun = true;
                        bullets = 6;
                        textPopups.Add(new TextPopup("GUN DROPPED!", new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT / 2), 4f, false));
                        if (player == PlayerIndex.One)
                            Game1.SOUND_DROP.Play(1f, 0, -1);
                        else
                            Game1.SOUND_DROP.Play(1f, 0, 1);
                        droppedGunTimer = 0f;
                    }
                }
                if (!dead && !won)
                    P1Score.update(gameTime);

                if (droppedGun)
                {
                    droppedGunTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (droppedGunTimer > DROP_TIME)
                    {
                        droppedGun = false;
                        dropping = false;
                        P1Score.throwTime = (float)gameTime.TotalGameTime.TotalSeconds;
                        gun.thrownPosition = new Vector2(dropX, 20);
                        gun.throwVelocity = Vector2.Zero;
                    }
                    else if (droppedGunTimer > DROP_TIME * 3f / 4f && !dropping)
                    {
                        Random rand = new Random();
                        dropX = rand.Next(100) * (rand.Next(2) == 0 ? -1 : 1);
                        dropX += headPos.X + (headText.Width * 0.375f / 2f) -50;
                        dropping = true;
                    }
                }
            }

            // camera update

            if (!gun.thrown && !gun.holstered && gunWindow == null && !dead)
            {
                camera.desiredScale = 2.0f;
                camera.desiredPosition.X = gun.position.X - (((float)Game1.SCREENWIDTH / 2f) / camera.getScale()); ;
                camera.desiredPosition.Y = gun.position.Y - (((float)Game1.SCREENHEIGHT / 2f) / camera.getScale());
            }
            else
            {
                camera.desiredScale = 1.0f;
                camera.desiredPosition = Vector2.Zero;
            }

            camera.update(gameTime);

            // bullets' updates

            ArrayList removal = new ArrayList(10);
            foreach (Bullet b in firedBullets)
            {
                b.update(gameTime);
                if (b.position.X > Game1.SCREENWIDTH)
                {
                    game.swapBullet(b, this);
                    removal.Add(b);
                }
                else if (collidedWithPlayer(b) && !dead)
                {
                    removal.Add(b);
                    die();
                }
                else if(collidedWithGun(b))
                {
                    if (!gun.holstered)
                    {
                        if (!gun.thrown)
                        {
                            gun.thrownPosition.X = gun.position.X;
                            gun.thrownPosition.Y = gun.position.Y;
                        }
                        gun.grab = false;
                        gun.justGrabbed = false;
                        gunWindow = null;
                        gun.thrown = true;
                        Random rand = new Random();
                        gun.throwVelocity.Y = -3 * rand.Next(5)+1;
                        gun.throwVelocity.X = (rand.Next(2) == 0 ? 1 : -1) * (rand.Next(5) + 1);
                        P1Score.throwTime = (float)gameTime.TotalGameTime.TotalSeconds;
                        removal.Add(b);

                        if (player == PlayerIndex.One)
                            Game1.SOUND_RICOCHET.Play(1, 0, -1);
                        else
                            Game1.SOUND_RICOCHET.Play(1, 0, 1);
                    }
                }
            }

            ArrayList removalText = new ArrayList(10);
            foreach (TextPopup tp in textPopups)
            {
                tp.update(gameTime);
                if (tp.done)
                    removalText.Add(tp);
            }

            foreach (Bullet r in removal)
                firedBullets.Remove(r);

            foreach (TextPopup rp in removalText)
                textPopups.Remove(rp);

            if (!game.getEnemyGunState(this))
                gunWindow = null;
            
            base.update(gameTime);
        }

        public void die()
        {
            dead = true;
            deathTimer = 0f;
            if (player == PlayerIndex.One)
            {
                headText = game.Content.Load<Texture2D>("CB_HitHead");
                game.p2Screen.win();
            }
            else
            {
                headText = game.Content.Load<Texture2D>("CB2_HitHead");
                game.p1Screen.win();
            }
            textPopups.Add(new TextPopup("DEAD!", new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT/2), 4f, false));
            if (player == PlayerIndex.One)
                Game1.SOUND_DEAD.Play(1f, 0, -1);
            else
                Game1.SOUND_DEAD.Play(1f, 0, 1);
        }

        private void win()
        {
            MediaPlayer.Stop();
            MediaPlayer.Play(Game1.MUSIC_BG3);
            won = true;
            if (!dead)
            {
                if(player == PlayerIndex.One)
                    headText = game.Content.Load<Texture2D>("CB_VictoryHead");
                else
                    headText = game.Content.Load<Texture2D>("CB2_VictoryHead");
            }
            textPopups.Add(new TextPopup("VICTORY!", new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT/2), 4f, true));
        }

        private bool collidedWithGun(Bullet b)
        {
            if (b.player == this.player)
                return false;

            if (gun.thrown)
            {
                if (b.position.X > gun.thrownPosition.X + GUN_RADIUS)
                    return false;
                else if (b.position.X < gun.thrownPosition.X - GUN_RADIUS)
                    return false;
                else if (b.position.Y < gun.thrownPosition.Y - GUN_RADIUS)
                    return false;
                else if (b.position.Y > gun.thrownPosition.Y + GUN_RADIUS)
                    return false;
            }
            else{
                if (b.position.X > gun.position.X + GUN_RADIUS)
                    return false;
                else if (b.position.X < gun.position.X - GUN_RADIUS)
                    return false;
                else if (b.position.Y < gun.position.Y - GUN_RADIUS)
                    return false;
                else if (b.position.Y > gun.position.Y + GUN_RADIUS)
                    return false;
            }
            return true;
        }

        private bool collidedWithPlayer(Bullet b)
        {
            if (b.player == this.player)
                return false;
            else if (b.position.X > BODY_RIGHT)
                return false;
            else if (b.position.Y < BODY_TOP)
                return false;
            else if (b.position.Y > BODY_BOTTOM)
                return false;

            return true;
        }

        public override void draw(GameTime time, SpriteBatch sprite)
        {
            sprite.Draw(Game1.IMAGE_BG, new Rectangle(Game1.SCREENWIDTH / -2, Game1.SCREENHEIGHT / -2, Game1.SCREENWIDTH*2, Game1.SCREENHEIGHT*2), new Rectangle(0, 0, Game1.IMAGE_BG.Width, Game1.IMAGE_BG.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);
            sprite.Draw(leftArmText, new Rectangle((int)leftArmPos.X, (int)leftArmPos.Y, (int)((float)leftArmText.Width * 0.375f), (int)((float)leftArmText.Height * 0.375f)), Color.White);
            sprite.Draw(bodyText, new Rectangle((int)bodyPos.X, (int)bodyPos.Y, (int)((float)bodyText.Width * 0.375f), (int)((float)bodyText.Height * 0.375f)), Color.White);
            sprite.Draw(headText, new Rectangle((int)headPos.X, (int)headPos.Y, (int)((float)headText.Width * 0.375f), (int)((float)headText.Height * 0.375f)), Color.White);
            sprite.Draw(leftLegText, new Rectangle((int)leftLegPos.X, (int)leftLegPos.Y, (int)((float)leftLegText.Width * 0.375f), (int)((float)leftLegText.Height * 0.375f)), Color.White);
            sprite.Draw(rightLegText, new Rectangle((int)rightLegPos.X, (int)rightLegPos.Y, (int)((float)rightLegText.Width * 0.375f), (int)((float)rightLegText.Height * 0.375f)), Color.White);

            if (gun.holstered || distance(gun.position, new Vector2(Gun.HOLSTER_X, Gun.HOLSTER_Y)) < Gun.CATCH_DISTANCE)
            {
                gun.draw(sprite);
                sprite.Draw(holsterText, new Rectangle((int)holsterPos.X, (int)holsterPos.Y, (int)((float)holsterText.Width * 0.375f), (int)((float)holsterText.Height * 0.375f)), Color.White);
                //sprite.Draw(Game1.IMAGE_HOLSTER, new Rectangle((int)Gun.HOLSTER_X, (int)Gun.HOLSTER_Y, (int)((float)Game1.IMAGE_HOLSTER.Width * 0.375f), (int)((float)Game1.IMAGE_HOLSTER.Height * 0.375f)), Color.Red);
            }
            else
            {
                sprite.Draw(holsterText, new Rectangle((int)holsterPos.X, (int)holsterPos.Y, (int)((float)holsterText.Width * 0.375f), (int)((float)holsterText.Height * 0.375f)), Color.White);
                //sprite.Draw(Game1.IMAGE_HOLSTER, new Rectangle((int)Gun.HOLSTER_X, (int)Gun.HOLSTER_Y, (int)((float)Game1.IMAGE_HOLSTER.Width * 0.375f), (int)((float)Game1.IMAGE_HOLSTER.Height * 0.375f)), Color.Red);
                gun.draw(sprite);
            }

            arm.draw(sprite);

            //sprite.DrawString(Game1.FONT, "gun angle: " + MathHelper.ToDegrees(gun.angle), new Vector2(100, 150), Color.White);
            //sprite.DrawString(Game1.FONT, "gun pos: " + gun.position.X + " " + gun.position.Y, new Vector2(100, 150), Color.Red);

            foreach (Bullet b in firedBullets)
                b.draw(sprite);

            if (dropping)
            {
                int result = (int)(droppedGunTimer / 100f);
                if (result % 2 == 0)
                {
                    sprite.Draw(Game1.IMAGE_ARROW, new Rectangle((int)dropX, 0, 75, 100), new Rectangle(0, 0, Game1.IMAGE_ARROW.Width, Game1.IMAGE_ARROW.Height), Color.White, 0, new Vector2(Game1.IMAGE_ARROW.Width / 2f, Game1.IMAGE_ARROW.Height / 2f), SpriteEffects.None, 0f);
                    sprite.Draw(gun.gunImage, new Rectangle((int)dropX, 100, (int)gun.size.X, (int)gun.size.Y), new Rectangle(0, 0, gun.gunImage.Width, gun.gunImage.Height), Color.White, -MathHelper.PiOver4, new Vector2(gun.gunImage.Width / 2f, gun.gunImage.Height / 2f), SpriteEffects.None, 0f);
                }
            }

            base.draw(time, sprite);
        }

        public static float distance(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v2.X - v1.X, 2.0) + Math.Pow(v2.Y - v1.Y, 2.0));
        }

        public override void input(GameTime time)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(player, GamePadDeadZone.Circular);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                game.setMode(Game1.modes.title);

            if (keyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                game.graphics.ToggleFullScreen();

            if (keyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2) && player == PlayerIndex.One)
                game.debugText = !game.debugText;

            accellerationVector = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.ThumbSticks.Left.X < 0)
                accellerationVector.X = -1 * ACCELLERATION_RATE;

            if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.ThumbSticks.Left.X > 0)
                accellerationVector.X = 1 * ACCELLERATION_RATE;

            if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.ThumbSticks.Left.Y > 0)
                accellerationVector.Y = -1 * ACCELLERATION_RATE;

            if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.ThumbSticks.Left.Y < 0)
                accellerationVector.Y = 1 * ACCELLERATION_RATE;

            movementVector += accellerationVector;

            if (movementVector.X > MOVE_MAX)
                movementVector.X = MOVE_MAX;
            else if (movementVector.X < -1 * MOVE_MAX)
                movementVector.X = -1 * MOVE_MAX;

            if (movementVector.Y > MOVE_MAX)
                movementVector.Y = MOVE_MAX;
            else if (movementVector.Y < -1 * MOVE_MAX)
                movementVector.Y = -1 * MOVE_MAX;

            if (accellerationVector.X == 0f && movementVector.X != 0f)
                reduce(REDUCE_X, DECELLERATION_RATE);
            if (accellerationVector.Y == 0f && movementVector.Y != 0f)
                reduce(REDUCE_Y, DECELLERATION_RATE);

            if ((keyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z)) || (gamePadState.IsButtonDown(Buttons.X) && previousGamePadState.IsButtonUp(Buttons.X)))
            {
                gun.grab = true;
                gun.justGrabbed = true;
                P1Score.totalTurns = 0;
                if (!droppedGun && !gun.thrown && !gun.holstered)
                {
                    if (player == PlayerIndex.One)
                        Game1.SOUND_WHIRL.Play(0.5f, 0, -1);
                    else
                        Game1.SOUND_WHIRL.Play(0.5f, 0, 1);
                }
            }
            else if(keyboardState.IsKeyUp(Keys.Z) && gamePadState.IsButtonUp(Buttons.X))
                gun.grab = false;

            if (keyboardState.IsKeyDown(Keys.X) && previousKeyboardState.IsKeyUp(Keys.X) || gamePadState.IsButtonDown(Buttons.Y) && previousGamePadState.IsButtonUp(Buttons.Y))
            {
                if (!gun.thrown && !gun.holstered)
                {
                    gun.thrown = true;
                    gun.thrownPosition.X = gun.position.X;
                    gun.thrownPosition.Y = gun.position.Y;
                    gun.throwVelocity.Y = 3 * movementVector.Y;
                    gun.throwVelocity.X = 0.25f *  movementVector.X;
                    P1Score.throwTime = (float)time.TotalGameTime.TotalSeconds;
                }
            }

            if ((keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C)) || (gamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A)))
            {
                if (!gun.holstered && !gun.thrown && gun.grab && gun.flatAngle)
                {
                    if (game.getEnemyGunState(this) || won)
                        gunWindow = new GunWindow(this);
                    else
                    {
                        textPopups.Add(new TextPopup("CAN'T SHOOT AN UNARMED MAN!", new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT / 2), 4f, false));
                        if (player == PlayerIndex.One)
                            Game1.SOUND_DROP.Play(1f, 0, -1);
                        else
                            Game1.SOUND_DROP.Play(1f, 0, 1);
                    }
                }
            }
            else if ((keyboardState.IsKeyUp(Keys.C) && previousKeyboardState.IsKeyDown(Keys.C)) || (gamePadState.IsButtonUp(Buttons.A) && previousGamePadState.IsButtonDown(Buttons.A)))
            {
                gunWindow = null;
            }

            if(won)
            {
                if ((gamePadState.IsButtonDown(Buttons.Start) && previousGamePadState.IsButtonUp(Buttons.Start)) ||
                 (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter)))
                {
                    game.setMode(Game1.modes.title);
                }
            }

            // gun window controlls 
            if (gunWindow != null)
            {
                if (gunWindow.state == GunWindow.State.STATE_NONE)
                {
                    gunWindow.state = GunWindow.State.STATE_HAMMER;
                }
                else if (gunWindow.state == GunWindow.State.STATE_COCKED && (keyboardState.IsKeyDown(Keys.Left) && previousKeyboardState.IsKeyUp(Keys.Left)  || gamePadState.ThumbSticks.Left.X < 0 && previousGamePadState.ThumbSticks.Left.X >= 0))
                {
                    gunWindow.state = GunWindow.State.STATE_TRIGGER;
                }
            }

            previousKeyboardState = keyboardState;
            previousGamePadState = gamePadState;
        }

        void reduce(bool coord, float amount)
        {
            if(coord == REDUCE_X){
                if (movementVector.X >= 0f)
                    movementVector.X -= amount;
                else
                    movementVector.X += amount;
            }
            else
            {
                if (movementVector.Y >= 0f)
                    movementVector.Y -= amount;
                else
                    movementVector.Y += amount;
            }
        }
    }

    public class GunWindow
    {
        public float WINDOW_SPACING = 40;
        public float HAMMER_PIVOT_X = 19;
        public float HAMMER_PIVOT_Y = 29;
        public float HAMMER_FINAL_PIVOT = MathHelper.Pi / -3f;
        public const float HAMMER_TIME = 250;
        
        public float TRIGGER_PIVOT_X = 6;
        public float TRIGGER_PIVOT_Y = 0;
        public float TRIGGER_FINAL_PIVOT = MathHelper.Pi / 5f;
        public const float TRIGGER_TIME = 150;

        public enum State
        {
            STATE_NONE,
            STATE_HAMMER,
            STATE_COCKED,
            STATE_TRIGGER
        }
        public State state;
        float hammerAngle;
        float triggerAngle;
        public float timmer;
        DuelScreen screen;
        float alpha = 0f;
        private Texture2D gunImage;
        private Texture2D hammerImage;
        private Texture2D triggerImage;
        private SpriteEffects spriteEffect;

        public GunWindow(DuelScreen screen)
        {
            this.screen = screen;
            state = State.STATE_NONE;
            timmer = 0;
            hammerAngle = 0;
            triggerAngle = 0;
            gunImage = Game1.IMAGE_GUN;
            hammerImage = Game1.IMAGE_HAMMER;
            triggerImage = Game1.IMAGE_TRIGGER;
            spriteEffect = SpriteEffects.None;

            if (screen.player == PlayerIndex.Two)
            {
                gunImage = Game1.IMAGE_GUN2;
                hammerImage = Game1.IMAGE_HAMMER2;
                triggerImage = Game1.IMAGE_TRIGGER2;
                spriteEffect = SpriteEffects.FlipHorizontally;
                HAMMER_PIVOT_X = 8;
                HAMMER_PIVOT_Y = 32;
                TRIGGER_PIVOT_X = 11;
                TRIGGER_PIVOT_Y = 6;
            }
        }

        public void update(GameTime time)
        {
            // make it slowly appear
            if (alpha < 255f)
            {
                alpha += (float)time.ElapsedGameTime.TotalMilliseconds;
                if (alpha > 255f)
                    alpha = 255f;
            }
            float force = 6f;
            float rForce;
            float fAngle;
            float pfAngle;
            float aAccel;

            //force due to movement
            if (screen.movementVector.X != 0 || screen.movementVector.Y != 0)
            {
                if (state == State.STATE_HAMMER)
                {
                    fAngle = (float)Math.Atan2(screen.movementVector.Y, screen.movementVector.X);
                    pfAngle = hammerAngle /*- MathHelper.Pi/2f*/ - fAngle;
                    rForce = (float)Math.Sin(pfAngle) * force;
                    aAccel = rForce * (float)time.ElapsedGameTime.TotalSeconds;
                    hammerAngle -= aAccel;

                    if (hammerAngle <= MathHelper.Pi / -2f)
                    {
                        hammerAngle = MathHelper.Pi / -2f;
                        state = State.STATE_COCKED;
                        if(screen.player == PlayerIndex.One)
                            Game1.SOUND_COCK.Play(1,0,-1);
                        else
                            Game1.SOUND_COCK.Play(1, 0, 1);
                    }
                    else if (hammerAngle > 0f)
                        hammerAngle = 0f;
                }

                else if (state == State.STATE_TRIGGER && screen.movementVector.X < 0)
                {
                    if (timmer < TRIGGER_TIME)
                    {
                        timmer += (float)time.ElapsedGameTime.TotalMilliseconds;
                        triggerAngle = TRIGGER_FINAL_PIVOT * (timmer / TRIGGER_TIME);
                    }
                    else
                    {
                        timmer = 0;
                        state = State.STATE_HAMMER;
                        hammerAngle = 0f;
                        triggerAngle = 0f;

                        if (screen.player == PlayerIndex.One)
                            Game1.SOUND_TRIGGER.Play(1, 0, -1);
                        else
                            Game1.SOUND_TRIGGER.Play(1, 0, 1);

                        if (screen.bullets > 0)
                            shootGun();
                    }
                }
            }
            else
            {
                if (state == State.STATE_HAMMER)
                {
                    hammerAngle += (MathHelper.Pi / 2f) * ((float)time.ElapsedGameTime.TotalMilliseconds / HAMMER_TIME);
                    if (hammerAngle < MathHelper.Pi / -2f)
                        hammerAngle = MathHelper.Pi / -2f;
                    else if (hammerAngle > 0f)
                        hammerAngle = 0f;
                }
                if (state == State.STATE_TRIGGER)
                {
                    timmer -= (float)time.ElapsedGameTime.TotalMilliseconds;
                    if (timmer < 0f)
                        timmer = 0;

                    triggerAngle = TRIGGER_FINAL_PIVOT * (timmer / TRIGGER_TIME);
                }
            }
        }

        private void shootGun()
        {
            if (screen.player == PlayerIndex.One)
                Game1.SOUND_SHOOT.Play(1, 0, -1);
            else
                Game1.SOUND_SHOOT.Play(1, 0, 1);

            screen.bullets -= 1;
            Vector2 barrel = new Vector2((float)Math.Cos((double)screen.gun.angle) * screen.gun.barrelLength, (float)Math.Sin((double)screen.gun.angle) * screen.gun.barrelLength);
            Vector2 handle = new Vector2((float)Math.Cos((double)(screen.gun.angle - Math.PI / 2d)) * screen.gun.handleLength, (float)Math.Sin((double)(screen.gun.angle - Math.PI / 2d)) * screen.gun.handleLength);
            Vector2 gunTip = screen.gun.position + barrel + handle;
            screen.firedBullets.Add(new Bullet(gunTip, screen.gun.angle, screen.player, spriteEffect));

            if (screen.bullets == 0)
            {
                screen.gun.thrown = true;
                screen.gun.thrownPosition = screen.gun.position;
                screen.gun.grab = false;
                screen.gun.throwVelocity.Y = 0f;
                screen.gunWindow = null;
            }
        }

        public void draw(SpriteBatch spritebatch)
        {
            Vector2 windowPos = new Vector2(Game1.SCREENWIDTH/2 - gunImage.Width/2, Game1.SCREENHEIGHT/2 - gunImage.Height/2);

            Color drawColor = Color.White;
            drawColor.A = (byte)alpha;
            Rectangle positionRect;
            Rectangle sourceRect;
            if (screen.player == PlayerIndex.One)
            {
                positionRect = new Rectangle((int)windowPos.X + (int)HAMMER_PIVOT_X + 55, (int)windowPos.Y + (int)HAMMER_PIVOT_Y + 6, hammerImage.Width, hammerImage.Height);
                sourceRect = new Rectangle(0, 0, hammerImage.Width, hammerImage.Height);
            }
            else
            {
                positionRect = new Rectangle((int)windowPos.X + 85, (int)windowPos.Y + 38, hammerImage.Width, hammerImage.Height);
                sourceRect = new Rectangle(0, 0, hammerImage.Width, hammerImage.Height);
            }
            spritebatch.Draw(hammerImage, positionRect, sourceRect, drawColor, hammerAngle, new Vector2(HAMMER_PIVOT_X, HAMMER_PIVOT_Y), spriteEffect, 0);
            if (screen.player == PlayerIndex.One)
            {
                positionRect = new Rectangle((int)windowPos.X + (int)TRIGGER_PIVOT_X + 88, (int)windowPos.Y + (int)TRIGGER_PIVOT_Y + 68, triggerImage.Width, triggerImage.Height);
                sourceRect = new Rectangle(0, 0, triggerImage.Width, triggerImage.Height);
            }
            else
            {
                positionRect = new Rectangle((int)windowPos.X + 110, (int)windowPos.Y + (int)TRIGGER_PIVOT_Y + 64, triggerImage.Width, triggerImage.Height);
                sourceRect = new Rectangle(0, 0, triggerImage.Width, triggerImage.Height);
            }
            spritebatch.Draw(triggerImage, positionRect, sourceRect, drawColor, triggerAngle, new Vector2(TRIGGER_PIVOT_X, TRIGGER_PIVOT_Y), spriteEffect, 0);
            positionRect = new Rectangle((int)windowPos.X, (int)windowPos.Y, gunImage.Width, gunImage.Height);
            sourceRect = new Rectangle(0, 0, gunImage.Width, gunImage.Height);
            spritebatch.Draw(gunImage, positionRect, sourceRect, drawColor, 0, Vector2.Zero, spriteEffect, 0);

            //spritebatch.DrawString(Game1.FONT, "arm angle: " + MathHelper.ToDegrees(hammerAngle), new Vector2(100, 100), Color.Black);
            //spritebatch.DrawString(Game1.FONT, "arm angle: " + timmer, new Vector2(100, 50), Color.Black);
        }
    }
}