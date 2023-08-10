using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MoleHillMountain
{
    public class Item
    {
        public const int DROP_NONE = 0;
        public const int DROP_SLINGSHOT = 1;

        protected Sprite sprite;
        protected Sprite flash;
        protected DungeonScreen dungeonScreen;
        public Vector2 position;
        protected Vector2 drawPosition;
        protected Color flashColor = new Color(255, 255, 255, 255);
        protected Color drawColor = new Color(255, 255, 255, 255);
        protected bool flashing = false;
        protected float flashReduction;
        public bool pulse;
        private int flashCycle;

        public Item(float x, float y, DungeonScreen dungeonScreen)
        {
            position = new Vector2(x, y);
            this.dungeonScreen = dungeonScreen;
            drawPosition = new Vector2(x, y);
            flashing = true;
            flashReduction = 64f;
            pulse = false;
            initSprites();
        }

        protected virtual void initSprites()
        {
            sprite = new Sprite(PikeAndShotGame.SLINGSHOT, new Rectangle(0, 0, 20, 20), 20, 20);
            flash = new Sprite(dungeonScreen._game.getFlashTexture(PikeAndShotGame.SLINGSHOT), new Rectangle(0, 0, 20, 20), 20, 20);
        }

        public Item(int x, int y, DungeonScreen dungeonScreen) : this(0f, 0f, dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
            drawPosition = position;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (pulse)
            {
                sprite.draw(spriteBatch, drawPosition + DungeonScreen.OFFSET, 0f, SeenStatus.getVisibilityColor(dungeonScreen.checkMoleSight(dungeonScreen.getCurrTunnel(position))));
                if (flashing)
                {
                    flash.draw(spriteBatch, drawPosition + DungeonScreen.OFFSET, 0f, flashColor);
                }
            }
            else
            {
                if (flashing)
                {
                    if (flashCycle > 0)
                    {
                        sprite.draw(spriteBatch, drawPosition + DungeonScreen.OFFSET, 0f, Color.White);
                        //flash.draw(spriteBatch, drawPosition + DungeonScreen.OFFSET, 0f, flashColor);
                    }
                    else
                    {
                        sprite.draw(spriteBatch, drawPosition + DungeonScreen.OFFSET, 0f, drawColor);
                    }
                }
                else
                {
                    sprite.draw(spriteBatch, drawPosition + DungeonScreen.OFFSET, 0f, Color.White);
                }
            }
        }

        public virtual void update(GameTime gameTime)
        {
            if (flashing)
            {
                if (pulse)
                {
                    flashColor.A = (byte)(255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / flashReduction));
                }
                else
                {
                    flashCycle = (int)(255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / flashReduction));
                    if (flashCycle > 0)
                    {
                        flashColor.A = (byte)(255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / flashReduction));
                    }
                    else
                    {
                        drawColor.A = (byte)(255f + (255f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / flashReduction)));
                    }
                    //flashColor.A = (byte)(255f * Math.Abs((float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / flashReduction)));
                }
            }


        }
    }

    public class Door : Item
    {
        public Door(float x, float y, DungeonScreen dungeonScreen) : base(x, y, dungeonScreen) { }

        public Door(int x, int y, DungeonScreen dungeonScreen) : base(x, y, dungeonScreen) { }

        protected override void initSprites()
        {
            sprite = new Sprite(PikeAndShotGame.DOOR, new Rectangle(0, 0, 20, 20), 20, 20);
            flash = new Sprite(dungeonScreen._game.getFlashTexture(PikeAndShotGame.DOOR), new Rectangle(0, 0, 20, 20), 20, 20);
            sprite.setFrame(8);
            flashing = false;
            flashReduction = 32f;
            pulse = true;
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);

            if (dungeonScreen.enemyCount <= 0 && dungeonScreen.enemies.Count == 0)
            {
                flashing = true;
            }

            if (dungeonScreen.enemyCount > 0 && dungeonScreen.enemyTimer < DungeonScreen.ENEMY_TIME / 4)
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
    }

    public static class SeenStatus
    {
        public const int SEEN = 2;
        public const int HALF_SEEN = 1;
        public const int NOT_SEEN = 0;

        public static Color SEEN_COLOR = new Color(255, 255, 255, 255);
        private static Color currHalfSeenColor = new Color(255, 255, 255, 127);
        private static Color NOT_SEEN_COLOR = new Color(255, 255, 255, 0);

        private static float totalTime;

        public static void update(GameTime gameTime)
        {
            totalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 50f; // flickering
        }

        public static Color getVisibilityColor(int seen)
        {
            switch (seen)
            {
                case NOT_SEEN:
                    return NOT_SEEN_COLOR;
                case HALF_SEEN:
                    currHalfSeenColor.A = (byte)(64 + 32f * Math.Sin(totalTime));
                    return currHalfSeenColor;
                default:
                    return SEEN_COLOR;
            }
        }
    }

    class Grub
    {
        public const int STATE_IDLE = 0;
        public const int STATE_LOOKING = 1;
        public const int STATE_COLLECTED = 2;
        public const int STATE_SPAWNING = 3;

        static Random random = new Random();

        DungeonScreen dungeonScreen;
        public Vector2 position;
        Sprite idle;
        Sprite looking;
        Sprite currSprite;
        Sprite spawning;
        float spawnTime;
        public int state = 0;

        float timeToAntic;
        float anticTime = 1000f;
        float blinkTime = 400f;

        float animationTimer;

        bool isGrub = false;
        bool flip = false;
        public int seen;

        Tunnel spawnTarget;

        public Grub(float x, float y, DungeonScreen dungeonScreen)
        {
            position = new Vector2(x, y);
            this.dungeonScreen = dungeonScreen;
            if (random.Next(100) > 95)
            {
                isGrub = true;
                looking = new Sprite(PikeAndShotGame.GRUB_LOOK, new Rectangle(0, 0, 20, 20), 20, 20);
                idle = new Sprite(PikeAndShotGame.GRUB_GRUB, new Rectangle(0, 0, 20, 20), 20, 20);
                spawning = new Sprite(PikeAndShotGame.BEEBLE_SPAWN, new Rectangle(0, 0, 20, 20), 20, 20);
                spawnTime = 1250f;
            }
            else
            {
                idle = new Sprite(PikeAndShotGame.GRUB_EGG, new Rectangle(0, 0, 20, 20), 20, 20);
            }
            resetAntic();
            seen = SeenStatus.NOT_SEEN;
        }

        public Grub(int x, int y, DungeonScreen dungeonScreen) : this(0f, 0f, dungeonScreen)
        {
            position.X = DungeonScreen.GRID_SIZE * x + DungeonScreen.GRID_SIZE * 0.5f;
            position.Y = DungeonScreen.GRID_SIZE * y + DungeonScreen.GRID_SIZE * 0.5f;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (state == STATE_IDLE)
            {
                idle.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, SeenStatus.getVisibilityColor(seen));
            }
            else if (state == STATE_SPAWNING)
            {
                spawning.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, SeenStatus.getVisibilityColor(seen));
            }
            else
            {
                if (flip)
                {
                    currSprite.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, SeenStatus.getVisibilityColor(seen), SpriteEffects.FlipHorizontally);
                }
                else
                {
                    currSprite.draw(spriteBatch, position + DungeonScreen.OFFSET, 0f, SeenStatus.getVisibilityColor(seen));
                }
            }
        }

        public static void shuffle(List<Tunnel> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Tunnel value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void update(GameTime gameTime)
        {
            float elapsedMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (state == STATE_IDLE)
            {
                if (timeToAntic > 0)
                {
                    timeToAntic -= elapsedMilliseconds;
                }
                else
                {
                    if (animationTimer > 0)
                    {
                        animationTimer -= elapsedMilliseconds;
                    }
                    else
                    {
                        resetAntic();
                    }
                }


                int maxFrames = idle.getMaxFrames();
                float frameTime = anticTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                idle.setFrame(frameNumber);
            }
            else if (state == STATE_LOOKING)
            {
                if (animationTimer > 0)
                {
                    animationTimer -= elapsedMilliseconds;
                    if (animationTimer > blinkTime * 0.75f)
                    {
                        idle.setFrame(0);
                        currSprite = idle;
                    }
                    else if (animationTimer > blinkTime * 0.5f)
                    {
                        looking.setFrame(0);
                        currSprite = looking;
                    }
                    else if (animationTimer > blinkTime * 0.25f)
                    {
                        idle.setFrame(0);
                        currSprite = idle;
                    }
                    else
                    {
                        looking.setFrame(0);
                        currSprite = looking;
                    }
                }
                else
                {
                    Vector2 diff = dungeonScreen.getMolePosition() - position;
                    bool left = false;
                    bool right = false;
                    bool down = false;
                    bool up = false;

                    if (diff.Y >= 0 && Math.Abs(diff.Y) >= Math.Abs(diff.X) * 0.75f)
                    {
                        down = true;
                    }
                    else if (diff.Y < 0 && Math.Abs(diff.Y) >= Math.Abs(diff.X) * 0.75f)
                    {
                        up = true;
                    }
                    if (diff.X >= 0 && Math.Abs(diff.X) >= Math.Abs(diff.Y) * 0.75f)
                    {
                        right = true;
                    }
                    else if (diff.X < 0 && Math.Abs(diff.X) >= Math.Abs(diff.Y) * 0.75f)
                    {
                        left = true;
                    }

                    if (left)
                    {
                        flip = true;
                    }
                    else
                    {
                        flip = false;
                    }

                    if (down)
                    {
                        if (!right && !left)
                        {
                            looking.setFrame(5);
                        }
                        else
                        {
                            looking.setFrame(4);
                        }
                        currSprite = looking;
                    }
                    else if (up)
                    {
                        if (!right && !left)
                        {
                            looking.setFrame(1);
                        }
                        else
                        {
                            looking.setFrame(2);
                        }
                        currSprite = looking;
                    }
                    else
                    {
                        looking.setFrame(3);
                        currSprite = looking;
                    }
                }
            }else if (state == STATE_SPAWNING)
            {
                animationTimer -= elapsedMilliseconds;
                if(animationTimer <= 0)
                {
                    state = STATE_COLLECTED;
                    dungeonScreen.enemies.Add(new Beeble(dungeonScreen, position.X, position.Y));
                }

                int maxFrames = spawning.getMaxFrames();
                float frameTime = spawnTime / (float)maxFrames;
                int frameNumber = maxFrames - (int)(animationTimer / frameTime) - 1;
                spawning.setFrame(frameNumber);
            }

            seen = dungeonScreen.checkMoleSight(position);


            if (isGrub && state != STATE_SPAWNING)
            {
                int moleClose = dungeonScreen.checkMoleSight(position);
                if (moleClose == SeenStatus.SEEN)
                {
                    if (state == STATE_LOOKING)
                    {
                        Tunnel currTunnel = dungeonScreen.getCurrTunnel(position);
                        List<Tunnel> surroundingTunnels = new List<Tunnel>();
                        Tunnel moleTunnel = dungeonScreen.getCurrTunnel(dungeonScreen.getMolePosition());
                        Tunnel left = dungeonScreen.getTunnelLeft(position);
                        Tunnel right = dungeonScreen.getTunnelRight(position);
                        Tunnel up = dungeonScreen.getTunnelAbove(position);
                        Tunnel down = dungeonScreen.getTunnelBelow(position);

                        if(moleTunnel != left && left != null &&
                            (left.left == Tunnel.DUG || left.right == Tunnel.DUG|| left.top == Tunnel.DUG || left.bottom == Tunnel.DUG))
                            surroundingTunnels.Add(left);
                        if (moleTunnel != right && right != null &&
                            (right.left == Tunnel.DUG || right.right == Tunnel.DUG || right.top == Tunnel.DUG || right.bottom == Tunnel.DUG))
                            surroundingTunnels.Add(right);
                        if (moleTunnel != up && up != null &&
                            (up.left == Tunnel.DUG || up.right == Tunnel.DUG || up.top == Tunnel.DUG || up.bottom == Tunnel.DUG))
                            surroundingTunnels.Add(up);
                        if (moleTunnel != down && down != null &&
                            (down.left == Tunnel.DUG || down.right == Tunnel.DUG || down.top == Tunnel.DUG || down.bottom == Tunnel.DUG))
                            surroundingTunnels.Add(down);

                        if (surroundingTunnels.Count > 0)
                        {
                            shuffle(surroundingTunnels);
                            spawnTarget = surroundingTunnels[0];

                            if (spawnTarget == left)
                            {
                                currTunnel.left = Tunnel.DUG;
                                spawnTarget.right = Tunnel.DUG;
                            }
                            else if (spawnTarget == right)
                            {
                                currTunnel.right = Tunnel.DUG;
                                spawnTarget.left = Tunnel.DUG;
                            }
                            else if (spawnTarget == up)
                            {
                                currTunnel.top = Tunnel.DUG;
                                spawnTarget.bottom = Tunnel.DUG;
                            }
                            else
                            {
                                currTunnel.bottom = Tunnel.DUG;
                                spawnTarget.top = Tunnel.DUG;
                            }

                            state = STATE_SPAWNING;
                            animationTimer = spawnTime;
                        }
                    }
                    else if (state != STATE_LOOKING && state != STATE_COLLECTED)
                    {
                        state = STATE_LOOKING;
                        animationTimer = blinkTime;
                        idle.setFrame(0);
                        currSprite = idle;
                    }
                }
                else
                {
                    if (state != STATE_IDLE && state != STATE_COLLECTED)
                    {
                        state = STATE_IDLE;
                        animationTimer = anticTime;
                    }
                }
            }

        }

        internal void collected(int count)
        {
            SoundEffectInstance collectedNoise = PikeAndShotGame.PICKUP_GRUB.CreateInstance();
            collectedNoise.Volume = 0.10f;
            collectedNoise.Pitch = -0.25f + 0.125f * (float)count;
            collectedNoise.Play();
            state = STATE_COLLECTED;
        }

        private void resetAntic()
        {
            timeToAntic = random.Next(1, 10) * anticTime;
            animationTimer = anticTime;
        }
    }
}
