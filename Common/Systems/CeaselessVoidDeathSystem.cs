using CalamityMod.NPCs.CeaselessVoid;
using Microsoft.Xna.Framework;
using MonoStereo.Filters;
using MonoStereoMod;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems
{
    public class CeaselessVoidDeathSystem : ModSystem
    {
        public static SpeedChangeFilter speedUp { get; private set; } = new();
        private static bool CeaselessVoidAlive = false;
        private static bool SlowDown = false;
        private static int VoidIndex = -1;

        public override void UpdateUI(GameTime gameTime)
        {
            int voidID = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoid>());
            if (voidID == -1)
                return;
            if (Main.npc[voidID].ModNPC is not CeaselessVoid)
                return;

            MonoStereoAudioTrack CurrentMusic = MonoStereoMod.MonoStereoMod.GetSong(Main.curMusic);
            if(CurrentMusic == null)
                return;
            if (!CurrentMusic.Filters.Any())
                CurrentMusic.AddFilter(speedUp);
        }

        public override void PostUpdateNPCs()
        {
            if (ModCompat.InfernumMode != null)
                return;

            MonoStereoAudioTrack currentMusic;
            bool veryDead = false;

            if (VoidIndex == -1)
            {
                int voidID = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoid>());
                if (voidID == -1)
                {
                    if (CeaselessVoidAlive)
                    {
                        currentMusic = MonoStereoMod.MonoStereoMod.GetSong(Main.curMusic);
                        currentMusic?.Stop();

                        speedUp.Speed = 1f;
                        CeaselessVoidAlive = false;
                    }
                    veryDead = true;
                }
                else
                    VoidIndex = voidID;
            }

            if (!veryDead && Main.npc[VoidIndex].life <= 0)
                veryDead = true;

            if (veryDead)
            {
                if (SlowDown && speedUp.Speed > 0)
                {
                    speedUp.Speed *= 0.975f;
                    if (speedUp.Speed < 0.001f)
                        speedUp.Speed = 0;
                }
                return;
            }

            CeaselessVoidAlive = true;

            //Incase Ceaseless decides to be a motherfucker and not finish its death animation.
            if (NPC.AnyNPCs(ModContent.NPCType<DarkEnergy>()))
                ((CeaselessVoid)Main.npc[VoidIndex].ModNPC).playedbuildsound = false;

            if (((CeaselessVoid)Main.npc[VoidIndex].ModNPC).playedbuildsound)
            {
                currentMusic = MonoStereoMod.MonoStereoMod.GetSong(Main.curMusic);
                if (currentMusic != null && !currentMusic.Filters.Any(f => f is SpeedChangeFilter))
                    currentMusic.AddFilter(speedUp);

                speedUp.Speed += 0.004f;
            }
            else
            {
                if(speedUp.Speed != 1f)
                    speedUp.Speed = 1f;
            }

        }

        public static void OnCeaselessDeath()
        {
            CeaselessVoidAlive = false;
            SlowDown = true;
        }
    }
}
