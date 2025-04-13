using Microsoft.Xna.Framework;
using MonoStereoMod;
using Terraria;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems
{
    public class MonoStereoSystem : ModSystem
    {
        public static bool CheckTrack = false;

        public override void UpdateUI(GameTime gameTime)
        {
            if (CheckTrack)
            {
                MonoStereoAudioTrack track = MonoStereoMod.MonoStereoMod.GetSong(Main.curMusic);

                if (track != null)
                {
                    Main.NewText("Track Detected!");
                    //if (track.IsPaused)
                    //    track.Resume();
                    //else
                    //    track.Pause();
                }
                else
                {
                    Main.NewText("No Track Detected!");
                }



                CheckTrack = false;
            }
        }
    }
}
