using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace MoleHillMountain
{
    public class Mole
    {
        public static Vector2 STAR_OFFSET_UP = new Vector2(0, -6);
        public static Vector2 STAR_OFFSET_RIGHT = new Vector2(6, 0);

        const float WALK_SPEED = 38f;
        const float DIG_SPEED = 30f;
        const float ZOOM_SPEED = 76f;
        public const float FIGHT_TIME = 900f;
        public const float DIZZY_TIME = 3000f;
        public const float DIZZY_MARK_TIME = 325f;

        public const int MOVING_NONE = 0;
        public const int MOVING_LEFT = 1;
        public const int MOVING_RIGHT = 2;
        public const int MOVING_UP = 3;
        public const int MOVING_DOWN = 4;

        public const int STATE_DIGGING = 1;
        public const int STATE_NUDGING = 2;
        public const int STATE_SQUASHED = 4;
        public const int STATE_SNIFFING = 8;
        public const int STATE_SCARED = 16;
        public const int STATE_MAD = 32;
        public const int STATE_USE = 64;
        public const int STATE_HIT = 128;
        public const int STATE_FIGHTING = 256;
        public const int STATE_DIZZY = 512;
        public const int STATE_CHARGE = 1024;
        public const int STATE_ZOOM = 2048;
        public const int STATE_CRASH = 4096;

        private const float MOLE_NUDGE_SPACING = 7;

        protected float animationTimer;
        protected float dizzyMarkTimer;
        public float walkSpeed = WALK_SPEED;
        protected float walkTime = 325f;
        protected float digTime = 650;
        protected float hitTime = 500;
        protected float animationTime;

        protected Sprite walking;
        protected Sprite digging;
        protected Sprite dizzy;
        protected Sprite nudging;
        protected Sprite squashed;
        protected Sprite slingshot;
        protected Sprite mad;
        protected Sprite unseen;
        protected Sprite hookshot;

        protected Sprite dizzy_stars;

        protected Sprite walkingSprite;
        public Vector2 position;
        protected Vector2 drawPosition;
        //flags
        public int state = 0;
        private float fightTimer;

        //notflags
        public int moving = 0;
        public int horzFacing = Sprite.DIRECTION_LEFT;
        public int vertFacing = Sprite.DIRECTION_NONE;

        protected DungeonScreen dungeonScene;
        protected Vegetable vegetable;
        public Tunnel diggingTunnel;
        public float nudgeMovement;

        public int str;
        public int per;
        public int con;
        public int health;
        internal int prevMoleRight;
        internal int prevMoleLeft;
        internal int prevMoleDown;
        internal int prevMoleUp;
        private float useTime = 250;
        private Vector2 hitPosition;
        private Vector2 startPosition;
        protected Color dimColor = new Color(255, 255, 255, 255);

        protected bool centeringOnTile;

        const int ITEM_SLINGSHOT = 0;
        const int ITEM_HOOKSHOT = 1;

        public int item;

        public Mole(DungeonScreen dungeonScene)
        {
            this.dungeonScene = dungeonScene;
            walking = new Sprite(PikeAndShotGame.MOLE_MINER_WALKING, new Rectangle(0, 0, 18, 18), 18, 18);
            digging = new Sprite(PikeAndShotGame.MOLE_MINER_DIGGING, new Rectangle(0, 0, 18, 18), 18, 18);
            dizzy = new Sprite(PikeAndShotGame.MOLE_DIZZY, new Rectangle(0, 0, 20, 20), 20, 20);
            nudging = new Sprite(PikeAndShotGame.MOLE_MINER_NUDGE, new Rectangle(0, 0, 18, 18), 18, 18);
            squashed = new Sprite(PikeAndShotGame.MOLE_SQUASHED, new Rectangle(0, 0, 18, 18), 18, 18);
            slingshot = new Sprite(PikeAndShotGame.MINER_SLING, new Rectangle(0, 0, 40, 18), 40, 18);
            hookshot = new Sprite(PikeAndShotGame.MINER_HOOK, new Rectangle(0, 0, 24, 20), 24, 20);
            mad = new Sprite(PikeAndShotGame.RAT_MAD, new Rectangle(0, 0, 20, 18), 20, 18);
            unseen = new Sprite(PikeAndShotGame.UNSEEN_WALK2, new Rectangle(0, 0, 18, 18), 18, 18);
            dizzy_stars = new Sprite(PikeAndShotGame.DIZZY_MARK, new Rectangle(0, 0, 20, 12), 20, 12);
            squashed.setFrame(squashed.getMaxFrames() - 2);
            walkingSprite = walking;
            animationTime = walkTime;
            position = new Vector2(10, 10);
            drawPosition = new Vector2(position.X, position.Y);
            str = 3;
            per = 3;
            con = 3;
            health = con;
            item = ITEM_HOOKSHOT;
        }

        public Mole(float x, float y, DungeonScreen dungeonScene) : this(dungeonScene)
        {
            position = new Vector2(x, y);
            drawPosition = new Vector2(position.X, position.Y);
        }

        public Mole(int x, int y, DungeonScreen dungeonScreen) : this(0f, 0f, dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = new Vector2(position.X, position.Y);
        }

        int prevFrame;

        public virtual void update(GameTime gameTime)
        {
            TimeSpan timeSpan = gameTime.ElapsedGameTime;
            animationTimer -= (float)timeSpan.TotalMilliseconds;

            if ((state & STATE_HIT) != 0)
            {
                if (animationTimer >= 0)
                {
                    int maxFrames = mad.getMaxFrames();
                    float frameTime = hitTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    mad.setFrame(frameNumber);
                    Vector2 diff = startPosition - hitPosition;
                    float p = EasingFunction.EaseOutQuint(0, 1, 1 - animationTimer / hitTime);
                    Tunnel currTunnel = dungeonScene.getCurrTunnel(position);
                    int staggerAmount = 5;
                    if (currTunnel != null)
                    {
                        if (Math.Abs(diff.X) > Math.Abs(diff.Y) && diff.X >= 0 && currTunnel.right != Tunnel.NOT_DUG)
                            position.X = MathHelper.Lerp(startPosition.X, startPosition.X + staggerAmount, p);
                        else if (Math.Abs(diff.X) > Math.Abs(diff.Y) && diff.X <= 0 && currTunnel.left != Tunnel.NOT_DUG)
                            position.X = MathHelper.Lerp(startPosition.X, startPosition.X - staggerAmount, p);
                        else if (Math.Abs(diff.X) < Math.Abs(diff.Y) && diff.Y >= 0)
                        {
                            if (currTunnel.bottom != Tunnel.NOT_DUG)
                                position.Y = MathHelper.Lerp(startPosition.Y, startPosition.Y + staggerAmount, p);
                            else
                            {
                                if (diff.X >= 0)
                                    position.X = MathHelper.Lerp(startPosition.X, startPosition.X + staggerAmount, p);
                                else
                                    position.X = MathHelper.Lerp(startPosition.X, startPosition.X - staggerAmount, p);
                            }
                        }
                        else
                        {
                            if (currTunnel.top != Tunnel.NOT_DUG)
                                position.Y = MathHelper.Lerp(startPosition.Y, startPosition.Y - staggerAmount, p);
                            else
                            {
                                if (diff.X >= 0)
                                    position.X = MathHelper.Lerp(startPosition.X, startPosition.X + staggerAmount, p);
                                else
                                    position.X = MathHelper.Lerp(startPosition.X, startPosition.X - staggerAmount, p);
                            }
                        }
                    }

                    drawPosition.X = (int)position.X;
                    drawPosition.Y = (int)position.Y;
                }
                else
                {
                    state &= ~STATE_HIT;
                    health--;
                    if (health <= 0)
                    {
                        state = STATE_SQUASHED;
                    }
                }
            }
            else if ((state & STATE_SQUASHED) != 0)
            {
                if (vegetable != null && position.Y - vegetable.position.Y <= vegetable.position.Y + DungeonScreen.GRID_SIZE / 4)
                {
                    position.Y = vegetable.position.Y + DungeonScreen.GRID_SIZE / 4;
                    drawPosition.X = (int)position.X;
                    drawPosition.Y = (int)position.Y;
                }
            }
            else if ((state & STATE_USE) != 0)
            {
                if (animationTimer >= 0)
                {
                    Sprite sprite;
                    int useFrame;

                    switch (item)
                    {
                        case ITEM_SLINGSHOT:
                            sprite = slingshot;
                            useFrame = 3;
                            break;
                        case ITEM_HOOKSHOT:
                            sprite = hookshot;
                            useFrame = 2;
                            break;
                        default:
                            sprite = slingshot;
                            useFrame = 3;
                            break;
                    }

                    int maxFrames = sprite.getMaxFrames();
                    float frameTime = animationTime / (float)maxFrames;
                    int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                    sprite.setFrame(frameNumber);

                    if(frameNumber != prevFrame && frameNumber == useFrame)
                    {
                        switch (item)
                        {
                            case ITEM_SLINGSHOT:
                                dungeonScene.spawnStone(position, horzFacing, vertFacing);
                                break;
                            case ITEM_HOOKSHOT:
                                dungeonScene.spawnHook(position, horzFacing, vertFacing);
                                break;
                            default:
                                dungeonScene.spawnStone(position, horzFacing, vertFacing);
                                break;
                        }
                        
                    }
                    prevFrame = frameNumber;
                }
                else
                {
                    state &= ~STATE_USE;
                }
            }
            else if ((state & STATE_FIGHTING) != 0)
            {
                fightTimer -= (float)timeSpan.TotalMilliseconds;
                if (fightTimer <= 0)
                {
                    state &= ~STATE_FIGHTING;
                    health--;
                    if (health <= 0)
                    {
                        state = STATE_SQUASHED;
                    }
                    else
                    {
                        state |= STATE_DIZZY;
                        dizzyMarkTimer = DIZZY_MARK_TIME;
                        fightTimer = DIZZY_TIME;
                    }
                }
            }
            else if (moving == MOVING_NONE || (state & STATE_CRASH) != 0)
            {
                walkingSprite.setFrame(0);
                position.X += nudgeMovement;
                nudgeMovement = 0;
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;
            }
            else
            {
                if (animationTimer < 0)
                {
                    animationTimer += animationTime;
                }

                if ((state & STATE_DIGGING) != 0)
                {
                    animationTime = digTime;
                    walkSpeed = DIG_SPEED;
                }
                else if ((state & STATE_ZOOM) != 0)
                {
                    animationTime = 375;
                    walkSpeed = ZOOM_SPEED;
                }
                else
                {
                    animationTime = walkTime;
                    walkSpeed = WALK_SPEED;
                }

                switch (moving)
                {
                    case MOVING_LEFT:
                        if (position.X > 10)
                        {
                            if (!dungeonScene.vegetableLeft(position, new ArrayList { this }, MOLE_NUDGE_SPACING))
                            {
                                state &= ~STATE_NUDGING;
                                position.X -= (float)timeSpan.TotalSeconds * walkSpeed;
                            }
                            else
                            {
                                state |= STATE_NUDGING;
                                position.X += nudgeMovement;
                                animationTime = walkTime;
                                walkingSprite = nudging;
                                nudgeMovement = 0;
                                //Console.WriteLine("nudgeMovement: " + nudgeMovement);
                            }
                        }
                        break;
                    case MOVING_RIGHT:
                        if (position.X < DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_WIDTH - 0.5))
                        {
                            if (!dungeonScene.vegetableRight(position, new ArrayList { this }, MOLE_NUDGE_SPACING))
                            {
                                state &= ~STATE_NUDGING;
                                position.X += (float)timeSpan.TotalSeconds * walkSpeed;
                            }
                            else
                            {
                                state |= STATE_NUDGING;
                                position.X += nudgeMovement;
                                animationTime = walkTime;
                                walkingSprite = nudging;
                                nudgeMovement = 0;
                                //Console.WriteLine("nudgeMovement: " + nudgeMovement);
                            }
                        }
                        break;
                    case MOVING_UP:
                        if (position.Y > 10 && !dungeonScene.vegetableAbove(this))
                        {
                            position.Y -= (float)timeSpan.TotalSeconds * walkSpeed;
                            state &= ~STATE_NUDGING;
                        }
                        break;
                    case MOVING_DOWN:
                        if (position.Y < DungeonScreen.GRID_SIZE * (DungeonScreen.GRID_HEIGHT - 0.5f) && !dungeonScene.vegetableBelow(this))
                        {
                            state &= ~STATE_NUDGING;
                            position.Y += (float)timeSpan.TotalSeconds * walkSpeed;
                        }
                        break;
                }

                if (centeringOnTile)
                {
                    centeringOnTile = false;
                    if( Math.Abs((int)position.X % DungeonScreen.GRID_SIZE - DungeonScreen.GRID_SIZE/2) < 2)
                    {
                        Tunnel tunnel = dungeonScene.getCurrTunnel(position);
                        position.X = tunnel.position.X + DungeonScreen.GRID_SIZE / 2;
                    }
                    if (Math.Abs((int)position.Y % DungeonScreen.GRID_SIZE - DungeonScreen.GRID_SIZE/2) < 2)
                    {
                        Tunnel tunnel = dungeonScene.getCurrTunnel(position);
                        position.Y = tunnel.position.Y + DungeonScreen.GRID_SIZE / 2;
                    }
                }
                drawPosition.X = (int)position.X;
                drawPosition.Y = (int)position.Y;

                int maxFrames = walkingSprite.getMaxFrames();
                float frameTime = animationTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;

                walkingSprite.setFrame(frameNumber);

                nudgeMovement = 0;
            }
            if ((state & STATE_DIZZY) != 0)
            {
                dimColor.A = (byte)(255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 10f));
                fightTimer -= (float)timeSpan.TotalMilliseconds;

                if (fightTimer <= 0)
                {
                    dimColor.A = (byte)(255f);
                    state &= ~STATE_DIZZY;
                }

                dizzyMarkTimer -= (float)timeSpan.TotalMilliseconds;
                if (dizzyMarkTimer <= 0)
                {
                    dizzyMarkTimer += DIZZY_MARK_TIME;
                }
                int maxFrames = dizzy_stars.getMaxFrames();
                float frameTime = DIZZY_MARK_TIME / (float)maxFrames;
                int frameNumber = maxFrames - (int)(dizzyMarkTimer / frameTime) - 1;

                dizzy_stars.setFrame(frameNumber);
            }
        }

        public void fight()
        {
            state = STATE_FIGHTING;
            fightTimer = FIGHT_TIME;
        }

        public virtual void draw(SpriteBatch spritebatch)
        {
            if ((state & STATE_FIGHTING) != 0)
            {
                Console.WriteLine("Cool mang");
            }
            else if ((state & STATE_HIT) != 0)
            {
                mad.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing);
            }
            else if ((state & STATE_USE) != 0)
            {
                Sprite sprite;
                switch (item)
                {
                    case ITEM_SLINGSHOT:
                        sprite = slingshot;
                        break;
                    case ITEM_HOOKSHOT:
                        sprite = hookshot;
                        break;
                    default:
                        sprite = slingshot;
                        break;
                }
                sprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing);
            }
            else if ((state & STATE_SQUASHED) == 0)
            {
                if ((state & STATE_DIZZY) != 0)
                {
                    walkingSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        dizzy_stars.draw(spritebatch, drawPosition + DungeonScreen.OFFSET + STAR_OFFSET_UP, horzFacing, vertFacing);
                    }
                    else if (horzFacing == Sprite.DIRECTION_LEFT)
                    {
                        dizzy_stars.draw(spritebatch, drawPosition + DungeonScreen.OFFSET + STAR_OFFSET_RIGHT, horzFacing, vertFacing);
                    }
                    else
                    {
                        dizzy_stars.draw(spritebatch, drawPosition + DungeonScreen.OFFSET - STAR_OFFSET_RIGHT, horzFacing, vertFacing);
                    }
                }
                else
                {
                    walkingSprite.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, vertFacing, dimColor);
                }
            }
            else
            {
                if (vegetable != null && (vegetable.state == Vegetable.SPLITTING || vegetable.state == Vegetable.DEAD))
                {
                    squashed.setFrame(squashed.getMaxFrames() - 1);
                }
                else
                {
                    squashed.setFrame(squashed.getMaxFrames() - 2);
                }

                if (horzFacing == Sprite.DIRECTION_LEFT)
                {
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_LEFT, Sprite.DIRECTION_NONE);
                    }
                }
                else
                {
                    if (vertFacing == Sprite.DIRECTION_NONE)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, horzFacing, Sprite.DIRECTION_NONE);
                    }
                    else if (vertFacing == Sprite.DIRECTION_DOWN)
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
                    }
                    else
                    {
                        squashed.draw(spritebatch, drawPosition + DungeonScreen.OFFSET, Sprite.DIRECTION_RIGHT, Sprite.DIRECTION_NONE);
                    }
                }
            }
        }

        void walk()
        {
            if (moving == MOVING_NONE)
            {
                animationTimer = animationTime;
            }
        }

        public void setDig(bool yes)
        {
            if ((state & STATE_NUDGING) == 0 && (state & STATE_SNIFFING) == 0 && (state & STATE_CHARGE) == 0
                && (state & STATE_ZOOM) == 0 && (state & STATE_CRASH) == 0)
            {
                if (yes)
                {
                    state |= STATE_DIGGING;
                    if (!(this is Rat) || ((Rat)this).seen == SeenStatus.SEEN)
                        walkingSprite = digging;
                    animationTime = digTime;
                }
                else
                {
                    state &= ~STATE_DIGGING;
                    if ((state & STATE_DIZZY) != 0)
                    {
                        walkingSprite = dizzy;
                    }
                    else
                    {
                        if (!(this is Rat) || (((Rat)this).seen == SeenStatus.SEEN))
                            walkingSprite = walking;
                    }
                    animationTime = walkTime;
                    if (animationTimer > animationTime && (state & STATE_HIT) == 0)
                    {
                        animationTimer -= animationTime;
                    }
                }
            }
        }

        public virtual void moveLeft()
        {
            int vert = getVert();
            if (vert == 0)
            {
                left();
            }
            else if (vert > 0)
            {
                up();
                centeringOnTile = true;
            }
            else
            {
                down();
                centeringOnTile = true;
            }
        }

        private int getVert()
        {
            int vertPosition = (int)position.Y;
            int remainder = vertPosition % DungeonScreen.GRID_SIZE;
            return remainder - (int)(DungeonScreen.GRID_SIZE * 0.5f);
        }
        private int getHorz()
        {
            int horzPosition = (int)position.X;
            int remainder = horzPosition % DungeonScreen.GRID_SIZE;
            return remainder - (int)(DungeonScreen.GRID_SIZE * 0.5f);
        }

        public virtual void moveRight()
        {
            int vert = getVert();
            if (vert == 0)
            {
                right();
            }
            else if (vert > 0)
            {
                up();
                centeringOnTile = true;
            }
            else
            {
                down();
                centeringOnTile = true;
            }
        }

        public virtual void squash(Vegetable vegetable)
        {
            state = STATE_SQUASHED;
            this.vegetable = vegetable;
        }

        public void hit(Vector2 position)
        {
            if ((state & STATE_SQUASHED) == 0)
            {
                hitPosition = position;
                startPosition = this.position;
                state |= STATE_HIT;
                animationTimer = hitTime;
            }
        }

        public virtual void moveUp()
        {
            int horz = getHorz();
            if (horz == 0)
            {
                up();
            }
            else if (horz > 0)
            {
                left();
                centeringOnTile = true;
            }
            else
            {
                right();
                centeringOnTile = true;
            }
        }

        public virtual void moveDown()
        {
            int horz = getHorz();
            if (horz == 0)
            {
                down();
            }
            else if (horz > 0)
            {
                left();
                centeringOnTile = true;
            }
            else
            {
                right();
                centeringOnTile = true;
            }
        }

        private void down()
        {
            walk();
            moving = MOVING_DOWN;
            if (vertFacing == Sprite.DIRECTION_NONE)
            {
                if (horzFacing == Sprite.DIRECTION_LEFT)
                {
                    horzFacing = Sprite.DIRECTION_RIGHT;
                }
                else
                {
                    horzFacing = Sprite.DIRECTION_LEFT;
                }
            }
            vertFacing = Sprite.DIRECTION_DOWN;
        }

        private void up()
        {
            walk();
            moving = MOVING_UP;
            vertFacing = Sprite.DIRECTION_UP;
        }

        private void right()
        {
            walk();
            moving = MOVING_RIGHT;
            vertFacing = Sprite.DIRECTION_NONE;
            horzFacing = Sprite.DIRECTION_RIGHT;
        }

        private void left()
        {
            walk();
            moving = MOVING_LEFT;
            vertFacing = Sprite.DIRECTION_NONE;
            horzFacing = Sprite.DIRECTION_LEFT;
        }

        internal void stopMoving()
        {
            moving = MOVING_NONE;
            state &= ~STATE_NUDGING;
        }

        internal bool alive()
        {
            return (state & STATE_SQUASHED) == 0;
        }

        internal float getDigSpeed()
        {
            return DIG_SPEED;
        }

        internal void useItem(int itemID)
        {
            item = itemID;
            if ((state & STATE_SQUASHED) == 0 && (state & STATE_USE) == 0)
            {
                state |= STATE_USE;
                animationTime = animationTimer = useTime;
            }
        }
    }
}