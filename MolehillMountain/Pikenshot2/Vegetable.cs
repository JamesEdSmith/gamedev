﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;

namespace MoleHillMountain
{
    public class Vegetable
    {
        static Random random = new Random();

        public const int NONE = 0;
        public const int SHAKING = 1;
        public const int FALLING = 2;
        public const int SPLITTING = 3;
        public const int DEAD = 4;
        public const int MOVING = 5;
        public const int BURNT = 6;

        public const float NUDGE_SPACING = 2;

        bool listingLeft;
        bool listingRight;

        protected float animationTimer;
        float fallSpeed = 68f;
        float fallTime = 225f;
        float shakeTime = 350f;
        float burnTime = 1000f;

        protected float splitTime = 400f;
        protected float animationTime;

        protected DungeonScreen dungeonScreen;

        protected Sprite shaking;
        protected Sprite falling;
        protected Sprite splitting;
        protected Sprite burnt;

        protected Sprite currSprite;
        public Vector2 position;
        protected Vector2 drawPosition;
        //not flags
        public int state = 0;

        public int fallingFrom;

        public bool squashing;
        public bool gonnaBreak;

        public ArrayList leftPushers;
        public ArrayList rightPushers;

        public List<int> dropList;

        public Vegetable(float x, float y, DungeonScreen dungeonScreen)
        {
            this.dungeonScreen = dungeonScreen;

            if (random.Next(2) == 0)
            {
                shaking = new Sprite(PikeAndShotGame.TURNIP_SHAKE, new Rectangle(0, 0, 20, 20), 20, 20);
                falling = new Sprite(PikeAndShotGame.TURNIP_TWIRL, new Rectangle(0, 0, 20, 20), 20, 20);
                splitting = new Sprite(PikeAndShotGame.TURNIP_SPLIT, new Rectangle(0, 0, 40, 20), 40, 20);
                burnt = new Sprite(PikeAndShotGame.TURNIP_BURNT, new Rectangle(0, 0, 20, 20), 20, 20);
                fallTime = 112.5f;
            }
            else
            {
                shaking = new Sprite(PikeAndShotGame.ONION_SHAKE, new Rectangle(0, 0, 20, 20), 20, 20);
                falling = new Sprite(PikeAndShotGame.ONION_TWIRL, new Rectangle(0, 0, 20, 20), 20, 20);
                splitting = new Sprite(PikeAndShotGame.ONION_SPLIT, new Rectangle(0, 0, 40, 20), 40, 20);
                burnt = new Sprite(PikeAndShotGame.TURNIP_BURNT, new Rectangle(0, 0, 20, 20), 20, 20);
                fallTime = 225f;
            }
            currSprite = shaking;
            animationTime = shakeTime;
            position = new Vector2(x, y);
            drawPosition = new Vector2(position.X, position.Y);
            state = NONE;
            leftPushers = new ArrayList(2);
            rightPushers = new ArrayList(2);
            dropList = new List<int> {
                Item.DROP_NONE,
                Item.DROP_NONE,
                Item.DROP_SLINGSHOT
            };
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            if (listingLeft)
            {
                shaking.setFrame(1);
                shaking.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
            }
            else if (listingRight)
            {
                shaking.setFrame(3);
                shaking.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
            }
            else
            {
                currSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
            }
        }

        public virtual void update(GameTime gameTime)
        {
            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
            if (state == NONE || state == MOVING)
            {
                Tunnel tunnelBelow = dungeonScreen.getTunnelBelow(position);
                if (tunnelBelow?.left == Tunnel.DUG || tunnelBelow?.right == Tunnel.DUG || tunnelBelow?.bottom == Tunnel.DUG || tunnelBelow?.top == Tunnel.DUG)
                {
                    if (!dungeonScreen.moleBelow(position))
                    {
                        if (state == NONE)
                        {
                            state = SHAKING;
                            animationTime = shakeTime;
                            animationTimer = animationTime;
                            currSprite = shaking;
                        }
                        else
                        {
                            fall();
                        }
                    }
                }
            }
            else
            {
                animationTimer -= (float)elapsedGameTime.TotalMilliseconds;
                if (state == SHAKING)
                {
                    if (animationTimer < 0)
                    {
                        fall();
                    }
                }
                else if (state == BURNT)
                {
                    if(animationTimer < 0)
                    {
                        state = DEAD;
                    }
                }
                else if (state == FALLING)
                {
                    position.Y += (float)elapsedGameTime.TotalSeconds * fallSpeed;
                    Tunnel tunnel = dungeonScreen.getTunnelBelow(position);

                    if (tunnel != null)
                    {
                        if ((int)tunnel.position.X + Tunnel.center.X < (int)position.X)
                        {
                            position.X -= 60f * (float)elapsedGameTime.TotalSeconds;
                            listingLeft = true;
                        }
                        else if ((int)tunnel.position.X + Tunnel.center.X > (int)position.X)
                        {
                            position.X += 60f * (float)elapsedGameTime.TotalSeconds;
                            listingRight = true;
                        }
                        else
                        {
                            listingLeft = false;
                            listingRight = false;
                        }
                    }

                    if (dungeonScreen.moleJustBelow(this))
                    {
                        squashing = true;
                        gonnaBreak = true;
                    }

                    if (animationTimer < 0)
                    {
                        animationTime = fallTime;
                        animationTimer = animationTime;
                    }
                }
                else if (state == SPLITTING)
                {
                    if (animationTimer < 0)
                    {
                        state = DEAD;
                        if (this is Bomb)
                        {
                            Vector2 tunnelPosition = dungeonScreen.getCurrTunnel(position).position + Tunnel.center;

                            dungeonScreen.createAnimation(position, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.explosionCenter);
                            dungeonScreen.createAnimation(position, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE, AnimationType.explosionCenter);
                            dungeonScreen.createAnimation(position, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_UP, AnimationType.explosionCenter);
                            dungeonScreen.createAnimation(position, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_DOWN, AnimationType.explosionCenter);

                            dungeonScreen.createAnimation(position, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);                            

                            dungeonScreen.createAnimation(position - new Vector2(DungeonScreen.GRID_SIZE, 0), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.explosion);
                            dungeonScreen.createAnimation(position + new Vector2(DungeonScreen.GRID_SIZE, 0), Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE, AnimationType.explosion);
                            dungeonScreen.createAnimation(position - new Vector2(0, DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_UP, AnimationType.explosion);
                            dungeonScreen.createAnimation(position + new Vector2(0, DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_DOWN, AnimationType.explosion);

                            dungeonScreen.createAnimation(tunnelPosition - new Vector2(DungeonScreen.GRID_SIZE, 0), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);
                            dungeonScreen.createAnimation(tunnelPosition + new Vector2(DungeonScreen.GRID_SIZE, 0), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);
                            dungeonScreen.createAnimation(tunnelPosition - new Vector2(0, DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);
                            dungeonScreen.createAnimation(tunnelPosition + new Vector2(0, DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);

                            dungeonScreen.createAnimation(position - new Vector2(DungeonScreen.GRID_SIZE * 0.5f, DungeonScreen.GRID_SIZE * 0.5f), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.explosionAngle);
                            dungeonScreen.createAnimation(position + new Vector2(DungeonScreen.GRID_SIZE * 0.5f, -DungeonScreen.GRID_SIZE * 0.5f), Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE, AnimationType.explosionAngle);
                            dungeonScreen.createAnimation(position - new Vector2(DungeonScreen.GRID_SIZE * 0.5f, -DungeonScreen.GRID_SIZE * 0.5f), Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_DOWN, AnimationType.explosionAngle);
                            dungeonScreen.createAnimation(position + new Vector2(DungeonScreen.GRID_SIZE * 0.5f, DungeonScreen.GRID_SIZE * 0.5f), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_DOWN, AnimationType.explosionAngle);

                            dungeonScreen.createAnimation(tunnelPosition - new Vector2(DungeonScreen.GRID_SIZE, DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);
                            dungeonScreen.createAnimation(tunnelPosition + new Vector2(DungeonScreen.GRID_SIZE, -DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);
                            dungeonScreen.createAnimation(tunnelPosition - new Vector2(DungeonScreen.GRID_SIZE, -DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);
                            dungeonScreen.createAnimation(tunnelPosition + new Vector2(DungeonScreen.GRID_SIZE, DungeonScreen.GRID_SIZE), Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.tunnelReveal);

                            dungeonScreen.explodeTunnels(dungeonScreen.getCurrTunnel(position).position, 1);
                        }
                    }
                }

                animate(elapsedGameTime);
            }

            if (state == SHAKING)
            {
                drawPosition.X = (int)position.X + (int)(1.1f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 25f));
                drawPosition.Y = (int)position.Y + (int)(1.1f * (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds / 25f));
            }
            else
            {
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;
            }
        }

        public virtual void burn()
        {
            if(state != DEAD && state != SPLITTING)
            {
                state = BURNT;
                animationTime = animationTimer = burnTime;
                currSprite = burnt;
            }
        }

        public void push(float totalSeconds)
        {
            int leftPushersStr = 0;
            int rightPushersStr = 0;
            float movement = 0;
            bool moleLeft = false;
            bool moleRight = false;

            foreach (Mole pusher in leftPushers)
            {
                if ((pusher.state & Mole.STATE_NUDGING) != 0 || (pusher != dungeonScreen.mole && pusher.moving == Mole.MOVING_RIGHT))
                {
                    leftPushersStr += pusher.str;
                }
                if (pusher == dungeonScreen.mole)
                {
                    moleLeft = true;
                }
            }
            foreach (Mole pusher in rightPushers)
            {
                if ((pusher.state & Mole.STATE_NUDGING) != 0 || (pusher != dungeonScreen.mole && pusher.moving == Mole.MOVING_LEFT))
                {
                    rightPushersStr += pusher.str;
                }
                if (pusher == dungeonScreen.mole)
                {
                    moleRight = true;
                }
            }
            Tunnel currTunnel = dungeonScreen.getCurrTunnel(position);

            if (leftPushersStr > rightPushersStr && (currTunnel != null && (currTunnel.right > 0 || position.X % DungeonScreen.GRID_SIZE < 11 || moleLeft)))
            {
                movement = totalSeconds * ((Mole)leftPushers[0]).currWalkSpeed * 0.5f;
                position.X += movement;
            }
            else if (leftPushersStr < rightPushersStr && (currTunnel != null && (currTunnel.left > 0 || position.X % DungeonScreen.GRID_SIZE > 9 || moleRight)))
            {
                movement = totalSeconds * ((Mole)rightPushers[0]).currWalkSpeed * -0.5f;
                position.X += movement;
            }

            foreach (Mole pusher in leftPushers)
            {
                pusher.nudgeMovement = movement;
            }
            foreach (Mole pusher in rightPushers)
            {
                pusher.nudgeMovement = movement;
            }

            leftPushers.Clear();
            rightPushers.Clear();
        }

        private void fall()
        {
            state = FALLING;
            currSprite = falling;
            animationTime = fallTime;
            animationTimer = animationTime;
            fallingFrom = (int)position.Y;
        }

        protected virtual void animate(TimeSpan elapsedGameTime)
        {
            if (state != SHAKING)
            {
                int maxFrames = currSprite.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                currSprite.setFrame(frameNumber);
            }
        }

        public virtual void split()
        {
            state = SPLITTING;
            currSprite = splitting;
            animationTime = splitTime;
            animationTimer = animationTime;
            dungeonScreen.spawnItem(this);
        }

        internal void land()
        {
            state = NONE;
            if(!(this is Bomb))
                currSprite.setFrame(0);
        }
    }

    class Bomb : Vegetable
    {
        float bombTime = 2500;
        float fuseTime = 250;
        float fuseTimer;
        Vector2 fusePosition;
        bool isItem;

        public Bomb(float x, float y, DungeonScreen dungeonScreen, bool isItem = false) : base(x, y, dungeonScreen)
        {
            shaking = new Sprite(PikeAndShotGame.BOMB, new Rectangle(0, 0, 20, 20), 20, 20);
            falling = new Sprite(PikeAndShotGame.BOMB, new Rectangle(0, 0, 20, 20), 20, 20);
            splitting = new Sprite(PikeAndShotGame.BOMB_BLAST, new Rectangle(0, 0, 20, 20), 20, 20);

            currSprite = shaking;
            splitTime = 200f;

            dropList = new List<int> {
                Item.DROP_NONE,
                Item.DROP_NONE,
                Item.DROP_NONE
            };

            if (isItem)
            {
                shaking.setFrame(2);
                falling.setFrame(2);
            }

            this.isItem = isItem;

            fusePosition = new Vector2();
        }

        public override void draw(SpriteBatch spritebatch)
        {
            currSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, 0, 0);
        }

        public override void split()
        {
            state = SPLITTING;
            currSprite = splitting;
            animationTime = splitTime;
            animationTimer = animationTime;
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);

            if (isItem)
            {
                fuseTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (fuseTimer <= 0)
                {
                    fuseTimer = fuseTime;

                    fusePosition.X = PikeAndShotGame.random.Next(0, 4);
                    fusePosition.Y = PikeAndShotGame.random.Next(-6, -3);
                    dungeonScreen.createAnimation(position + fusePosition, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE, AnimationType.hookImpact);
                }

                bombTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if(bombTime <= 0 && state != SPLITTING && state != DEAD)
                {
                    split();
                }
            }

        }

        protected override void animate(TimeSpan elapsedGameTime)
        {
            if (state != SHAKING && state != FALLING)
            {
                int maxFrames = currSprite.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                currSprite.setFrame(frameNumber);
            }
        }

        public override void burn()
        {
            split();
        }
    }
}
