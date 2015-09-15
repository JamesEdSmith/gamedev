using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Cheyenne_Show_offs
{
    public class Score
    {
        const float INCREASE = 50;

        public float score;
        float increase;
        float multiplier;
        public float throwTime;
        public float turnAmount;
        public int totalTurns;
        public float timer;

        DuelScreen screen;

        public Score(DuelScreen screen)
        {
            score = 0;
            increase = 0;
            multiplier = 0;
            turnAmount = 0;
            this.screen = screen;
            totalTurns = 0;
            timer = 0f;
        }

        public void update(GameTime time)
        {
            timer += (float)time.ElapsedGameTime.TotalSeconds;
            if (screen.gun.thrown)
                multiplier = 5f;
            else if (multiplier > 1f)
                multiplier -= 2.5f * (float)time.ElapsedGameTime.TotalSeconds;

            if (multiplier < 1f)
                multiplier = 1f;
        
            if (timer>3 && score > 0)
            {
                multiplier = 1f;
                score -= (INCREASE * 3);
                if (score < 0)
                    score = 0;
                timer = 0;
                bool found = false;
                foreach (TextPopup tp in screen.textPopups)
                {
                    if (tp.position.Y == Game1.SCREENHEIGHT - 100f)
                    {
                        tp.timer = 0;
                        tp.text = "No Tricks - $" + (INCREASE* 3);
                        tp.alpha = 255;
                        tp.good = false;
                        found = true;
                    }
                }
                if (!found)
                    screen.textPopups.Add(new TextPopup("No Tricks - $" + (INCREASE * 3), new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT - 100f), 2, false));

                if (screen.player == PlayerIndex.One)
                    Game1.SOUND_BAD.Play(1, 0, -1);
                else
                    Game1.SOUND_BAD.Play(1, 0, 1);
            }
            if (timer > 1)
            {
                totalTurns = 0;
            }

            if (!screen.gun.holstered && !screen.droppedGun)
            {
                turnAmount += screen.gun.aVelocity;// *(float)time.ElapsedGameTime.TotalSeconds;
                if (turnAmount >= MathHelper.Pi * 2f || turnAmount <= MathHelper.Pi * -2f)
                {
                    turnAmount = 0f;
                    totalTurns++;
                    scoreTurn();
                    timer = 0;
                    float pitch = Math.Abs(screen.gun.aVelocity) * 0.3f;
                    if (screen.player == PlayerIndex.One)
                    {
                        Game1.SOUND_WHIRL.Play(0.5f, pitch, -1);
                        Game1.SOUND_TWIRL.Play(0.25f, pitch, -1);
                    }
                    else
                    {
                        Game1.SOUND_WHIRL.Play(0.5f, pitch, 1);
                        Game1.SOUND_TWIRL.Play(0.25f, pitch, 1);
                    }
                }
            }
        }

        internal void scoreTurn()
        {
            float reduction = (int)((float)totalTurns/INCREASE*15)* 5;
            if (reduction > 49)
                reduction = 49;
            score += (int)(INCREASE - reduction) * (int)multiplier;
            int amount = (int)(INCREASE - reduction);
            bool found = false;
            foreach (TextPopup tp in screen.textPopups)
            {
                if (tp.position.Y == Game1.SCREENHEIGHT - 100f)
                {
                    tp.timer = 0;
                    tp.text = totalTurns + " Spins + $" + amount;
                    if(multiplier > 1.5)
                        tp.text += " x" + (int)multiplier;
                    tp.alpha = 255;
                    tp.good = true;
                    found = true;
                }
            }
            if (!found)
            {
                if (multiplier > 1.5)
                    screen.textPopups.Add(new TextPopup(totalTurns + " Spins + $" + amount + " x" + (int)multiplier, new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT - 100f), 2, true));
                else
                    screen.textPopups.Add(new TextPopup(totalTurns + " Spins + $" + amount, new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT - 100f), 2, true));
            }
            if (screen.player == PlayerIndex.One)
                Game1.SOUND_GOOD.Play(0.25f, 0, -1);
            else
                Game1.SOUND_GOOD.Play(0.25f, 0, 1);
 

        }

        internal void scoreThrow(GameTime time)
        {
            score += ((float)time.TotalGameTime.TotalSeconds - throwTime) * 500f * multiplier;
            int amount = (int)(((float)time.TotalGameTime.TotalSeconds - throwTime) * 500f);
            bool found = false;
            foreach (TextPopup tp in screen.textPopups)
            {
                if (tp.position.Y == Game1.SCREENHEIGHT - 100f)
                {
                    tp.timer = 0;
                    tp.text = "Throw + $" + amount + " x" + (int)multiplier;
                    tp.alpha = 255;
                    tp.good = true;
                    found = true;
                }
            }
            if (!found)
                screen.textPopups.Add(new TextPopup("Throw + $" + amount + " x" + (int)multiplier, new Vector2(Game1.SCREENWIDTH / 2, Game1.SCREENHEIGHT - 100f), 2, true));

            if (screen.player == PlayerIndex.One)
                Game1.SOUND_THROW.Play(1, 0, -1);
            else
                Game1.SOUND_THROW.Play(1, 0, 1);
        }
    }
}
