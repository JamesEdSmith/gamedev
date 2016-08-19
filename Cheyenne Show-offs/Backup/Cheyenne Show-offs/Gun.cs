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
using System.Collections;

namespace Cheyenne_Show_offs
{
    public class Gun
    {
        public float X = 292;
        public float Y = 390;
        public const float g = 0.4f;
        public const int CATCH_DISTANCE = 40;
        public const float HOLSTER_X = 155;
        public const float HOLSTER_Y = 435;
        public const float HOLSTER_ANGLE = 1.80f;
        public float barrelLength = 100f;
        public float handleLength = 30f;

        public Vector2 position;
        public Vector2 thrownPosition;
        public Vector2 throwVelocity;
        public float angle;
        public float aVelocity;
        private Vector2 center;
        public Vector2 size;
        private Vector2 proportion;
        private Arm myArm;
        public bool grab;
        public bool justGrabbed;
        public bool thrown;
        public bool holstered;
        public bool flatAngle;
        public PlayerIndex player;
        public Texture2D gunImage;
        private SpriteEffects effect;

        public Gun(PlayerIndex player)
        {
            gunImage = Game1.IMAGE_GUN_IN_HAND;
            effect = SpriteEffects.None;
            center = new Vector2(95, 78);

            if (player == PlayerIndex.Two)
            {
                gunImage = Game1.IMAGE_GUN_IN_HAND2;
                effect = SpriteEffects.FlipHorizontally;
                X -= 16;
                Y -= 12;
                center = new Vector2(114, 78);
            }

            position = new Vector2(X, Y);
            thrownPosition = new Vector2(X, Y);
            size = new Vector2((float)Game1.IMAGE_GUN.Width / 2.2f, (float)Game1.IMAGE_GUN.Height / 2.2f);
            proportion = new Vector2(size.X / (float)Game1.IMAGE_GUN.Width, size.Y / (float)Game1.IMAGE_GUN.Height);
            throwVelocity = Vector2.Zero;
            angle = 0;
            aVelocity = 0;
            grab = false;
            justGrabbed = false;
            thrown = false;
            flatAngle = true;
            holstered = false;
            this.player = player;
        }

        public void setArm(Arm a)
        {
            myArm = a;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            Color c;
            if (Math.Abs(MathHelper.WrapAngle(angle) - MathHelper.WrapAngle(myArm.getArmAngle())) < Math.Abs(MathHelper.WrapAngle(angle) - MathHelper.WrapAngle(myArm.getArmAngle() + (MathHelper.Pi / 2f))))
            {
                c = Color.Red;
            }
            else
            {
                c = Color.Green;
            }
            c = Color.White;

            Vector2 pos;
            if (thrown)
                pos = thrownPosition;
            else if (holstered)
                pos = new Vector2(HOLSTER_X, HOLSTER_Y);
            else
                pos = position;

            Rectangle positionRect = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
            Rectangle sourceRect = new Rectangle(0, 0, gunImage.Width, gunImage.Height);
            spriteBatch.Draw(gunImage, positionRect, sourceRect, c, angle, center, effect, 0);
        }

        public void update(GameTime time, DuelScreen game)
        {
            float force = 1.2f;
            float rForce;
            float fAngle;
            float pfAngle;
            float aAccel;
            float targetAngle = angle;

            Vector2 newPosition = new Vector2 (position.X, position.Y);
            newPosition += game.movementVector;

            if(thrown)
            {
                throwVelocity.Y += g;
                thrownPosition += throwVelocity;

                if (DuelScreen.distance(position, thrownPosition) < CATCH_DISTANCE && throwVelocity.Y > 0 && grab && game.bullets > 0)
                {
                    thrown = false;
                    if (player == PlayerIndex.One)
                    {
                        Game1.SOUND_WHIRL.Play(0.5f, 0, -1);
                    }
                    else
                    {
                        Game1.SOUND_WHIRL.Play(0.5f, 0, 1);
                    }

                    if(!game.won)
                        game.P1Score.scoreThrow(time);
                }
            }
            
            if (grab && !thrown)
            {
                //adjust angle of the gun if I just grabbed it
                if (justGrabbed)
                {
                    justGrabbed = false;
                    if (Math.Abs(MathHelper.WrapAngle(angle) - MathHelper.WrapAngle(myArm.getArmAngle())) < Math.Abs(MathHelper.WrapAngle(angle) - MathHelper.WrapAngle(myArm.getArmAngle() + (MathHelper.Pi / 2f))))
                    {
                        targetAngle = MathHelper.WrapAngle(myArm.getArmAngle());
                        aVelocity = 0f;
                        flatAngle = true;
                    }
                    else
                    {
                        targetAngle = MathHelper.WrapAngle(myArm.getArmAngle() + (MathHelper.Pi / 2f));
                        aVelocity = 0f;
                        flatAngle = false;
                    }
                }
                else
                {
                    // keep gun lined up with hand
                    if (flatAngle)
                    {
                        targetAngle = angle = MathHelper.WrapAngle(myArm.getArmAngle());
                        aVelocity = 0f;
                        
                    }
                    else
                    {
                        targetAngle = angle = MathHelper.WrapAngle(myArm.getArmAngle() + (MathHelper.Pi / 2f));
                        aVelocity = 0f;
                    }
                }
                
                if (DuelScreen.distance(newPosition, myArm.joints[Arm.shoulder]) < Arm.ARM_LENGTH * 2f)
                {
                    position += game.movementVector;
                }
                else
                {
                    game.movementVector = Vector2.Zero;
                }

                if (Math.Abs(angle - targetAngle) > 0.01)
                {
                    angle -= (angle - targetAngle) / 10f;
                }
                else
                {
                    angle = targetAngle;
                }
                
            }
            else
            {
                if (DuelScreen.distance(newPosition, myArm.joints[Arm.shoulder]) < Arm.ARM_LENGTH * 2f)
                {
                    position += game.movementVector;
                }
                else
                {
                    game.movementVector = Vector2.Zero;
                }

                if ((game.movementVector.X != 0 || game.movementVector.Y != 0) && !thrown)
                {
                    //force due to movement
                    fAngle = (float)Math.Atan2(game.movementVector.Y, game.movementVector.X);
                    pfAngle = angle - fAngle;
                    rForce = (float)Math.Sin(pfAngle) * force;
                    aAccel = rForce * (float)time.ElapsedGameTime.TotalSeconds;
                    aVelocity += aAccel;
                }
                else
                {
                    fAngle = (float)Math.Atan2(1, 0.0);
                    pfAngle = MathHelper.WrapAngle(angle) /*+ MathHelper.Pi*/ - fAngle;
                    rForce = (float)Math.Sin(pfAngle) * g;
                    aAccel = rForce * (float)time.ElapsedGameTime.TotalSeconds;
                    aVelocity += aAccel;

                    float pos = (aVelocity > 0 ? 1.0f : -1.0f);
                    aAccel = 0.001f * pos;

                    if (Math.Abs(aVelocity) > 0.001f)
                        aVelocity -= 1 * aAccel;
                    else
                        aVelocity = 0;
                }

                angle += aVelocity;
                angle = MathHelper.WrapAngle(angle);
            }

            if (!thrown && DuelScreen.distance(position, new Vector2(HOLSTER_X, HOLSTER_Y)) < CATCH_DISTANCE)
            {
                if (grab)
                {
                    targetAngle = MathHelper.WrapAngle(myArm.getArmAngle());
                    flatAngle = true;
                    justGrabbed = false;
                    if (holstered)
                    {
                        holstered = false;
                        if (player == PlayerIndex.One)
                        {
                            Game1.SOUND_HOLSTER1.Play(1, 0, -1);
                            Game1.SOUND_WHIRL.Play(0.5f, 0, -1);
                        }
                        else
                        {
                            Game1.SOUND_HOLSTER1.Play(1, 0, 1);
                            Game1.SOUND_WHIRL.Play(0.5f, 0, 1);
                        }
                    }
                }
                else if (!holstered)
                {
                    holstered = true;
                    if (player == PlayerIndex.One)
                        Game1.SOUND_HOLSTER2.Play(1, 0, -1);
                    else
                        Game1.SOUND_HOLSTER2.Play(1, 0, 1);
                }
            }

            if (holstered || DuelScreen.distance(position, new Vector2(HOLSTER_X, HOLSTER_Y)) < CATCH_DISTANCE)
            {
                angle = HOLSTER_ANGLE;
            }
        }
    }

    public class Arm
    {
        public const float ARM_LENGTH = 100f;

        public Vector2[] joints;
        Gun gun;
        public const int shoulder = 0;
        public const int elbow = 1;
        public const int hand = 2;
        public float angle;
        public Texture2D handTexture;
        private Texture2D upperArmTexture;
        private Texture2D foreArmTexture;
        private SpriteEffects effect; 

        public Arm(Gun gun)
        {
            joints = new Vector2[3];

            joints[0] = new Vector2(gun.X - ARM_LENGTH, gun.Y - ARM_LENGTH);  //shoulder
            joints[1] = new Vector2(gun.X - ARM_LENGTH, gun.Y);        //elbow
            joints[2] = new Vector2(gun.X, gun.Y);              //hand

            this.gun = gun;
            gun.setArm(this);
            handTexture = Game1.IMAGE_SPIN_HAND;

            upperArmTexture = Game1.IMAGE_UPPER_ARM;
            foreArmTexture = Game1.IMAGE_FORE_ARM;
            effect = SpriteEffects.None;

            if (gun.player == PlayerIndex.Two)
            {
                upperArmTexture = Game1.IMAGE_UPPER_ARM2;
                foreArmTexture = Game1.IMAGE_FORE_ARM2;
                effect = SpriteEffects.FlipHorizontally;
            }
        }

        public float getArmAngle()
        {
            Vector2 diff = joints[hand];
            diff -= joints[elbow];
            return (float)Math.Atan2(diff.Y, diff.X);
        }

        public void update(GameTime time, DuelScreen game)
        {
            joints[hand] = gun.position;

            Vector2 diff = joints[elbow];
            diff -= joints[hand];
            double angle = Math.Atan2(diff.Y, diff.X);
            Vector2 tempElbow = new Vector2(ARM_LENGTH * (float)Math.Cos(angle), ARM_LENGTH * (float)Math.Sin(angle));
            tempElbow += joints[hand];
            diff = joints[shoulder] - tempElbow;
            if (Math.Abs(Math.Atan2(diff.Y, diff.X)) == ARM_LENGTH)
            {
                joints[elbow] = tempElbow;
            }
            else
            {
                diff = tempElbow - joints[shoulder];
                angle = Math.Atan2(diff.Y,diff.X);
                //check joint constraint
                double wristAngle = Math.Atan2(joints[hand].Y - joints[elbow].Y, joints[hand].X - joints[elbow].X);

                double aDiff = MathHelper.WrapAngle((float)wristAngle - (float)angle);

                if (aDiff > 0)
                    angle = wristAngle;
                joints[elbow].X = ARM_LENGTH * (float)Math.Cos(angle);
                joints[elbow].Y = ARM_LENGTH * (float)Math.Sin(angle);

                joints[elbow] += joints[shoulder];

                diff = joints[elbow] - tempElbow;
                joints[hand] -= diff;
            }

        }

        public void draw(SpriteBatch spriteBatch)
        {
            Rectangle positionRect;
            if(gun.player == PlayerIndex.One)
                positionRect = new Rectangle((int)(joints[0].X), (int)(joints[0].Y), (int)(ARM_LENGTH*1.6f), (int)(ARM_LENGTH*1.1f));
            else
                positionRect = new Rectangle((int)(joints[0].X), (int)(joints[0].Y), (int)(ARM_LENGTH * 1.5), (int)(ARM_LENGTH));

            Rectangle sourceRect = new Rectangle(0, 0, upperArmTexture.Width, upperArmTexture.Height);
            Vector2 diff = joints[1];
            diff -= joints[0];
            angle = (float)Math.Atan2(diff.Y, diff.X);

            spriteBatch.Draw(upperArmTexture, positionRect, sourceRect, Color.White, angle, new Vector2(upperArmTexture.Width / 4f, upperArmTexture.Height / 2f), effect, 0);

            if(gun.player == PlayerIndex.One)
                positionRect = new Rectangle((int)(joints[1].X), (int)(joints[1].Y), (int)(ARM_LENGTH*1.10f), (int)ARM_LENGTH/2);
            else
                positionRect = new Rectangle((int)(joints[1].X), (int)(joints[1].Y), (int)(ARM_LENGTH*1.10f), (int)ARM_LENGTH * 3/ 5);

            sourceRect = new Rectangle(0, 0, foreArmTexture.Width, foreArmTexture.Height);
            diff = joints[2];
            diff -= joints[1];
            angle = (float)Math.Atan2(diff.Y, diff.X);

            spriteBatch.Draw(foreArmTexture, positionRect, sourceRect, Color.White, angle+0.1f, new Vector2(foreArmTexture.Width / 4f, foreArmTexture.Height/2f), effect, 0);
            
            if (gun.grab)
                handTexture = Game1.IMAGE_FIST_HAND;
            else if (gun.thrown || gun.holstered)
                handTexture = Game1.IMAGE_OPEN_HAND;
            else
                handTexture = Game1.IMAGE_SPIN_HAND;

            positionRect = new Rectangle((int)(joints[2].X), (int)(joints[2].Y), (int)(ARM_LENGTH/1.2), (int)(ARM_LENGTH/1.2f));
            sourceRect = new Rectangle(0, 0, handTexture.Width, handTexture.Height);

            spriteBatch.Draw(handTexture, positionRect, sourceRect, Color.White, angle, new Vector2(handTexture.Width / 1.6f, handTexture.Height / 2.25f), SpriteEffects.None, 0);

            //spriteBatch.Draw(Game1.IMAGE_DOT, joints[0], Color.White);
            //spriteBatch.Draw(Game1.IMAGE_DOT, joints[1], Color.White);
            //spriteBatch.Draw(Game1.IMAGE_DOT, joints[2], Color.White);
        }
    }

    public class Bullet
    {
        const float speed = 20f;

        public Vector2 position;
        public float rotation;
        Texture2D image;
        SpriteEffects spriteEffect;
        public PlayerIndex player;

        public Bullet(Vector2 position, float rotation, PlayerIndex player, SpriteEffects spriteEffect)
        {
            this.position = position;
            this.rotation = rotation;
            this.spriteEffect = spriteEffect;
            this.player = player;
            image = Game1.IMAGE_BULLET;
        }

        public void update(GameTime gameTime)
        {
            this.position.X += (float)Math.Cos(rotation) * speed;
            this.position.Y += (float)Math.Sin(rotation) * speed;
        }

        public void draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(image, position, new Rectangle(0, 0, image.Width, image.Height), Color.White, rotation, new Vector2(image.Width / 2f, image.Height / 2f), 1f, spriteEffect, 0);
        }
    }
}
