using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems
{
    public class ModCompat : ModSystem
    {
        private static readonly string displayPath = "ModCompat.MusicDisplay.";

        public static Mod InfernumMode { get; private set; } = null;
        public static Mod InfernumModeMusic { get; private set; } = null;

        public override void PostAddRecipes()
        {
            if (!ModLoader.TryGetMod("MusicDisplay", out Mod display))
                return;

            LocalizedText modName = Language.GetOrRegister("Mods.Sundowner." + displayPath + "ModName");

            void AddMusic(string path, string name)
            {
                LocalizedText author = Language.GetOrRegister("Mods.Sundowner." + displayPath + name + ".Author");
                LocalizedText displayName = Language.GetOrRegister("Mods.Sundowner." + displayPath + name + ".DisplayName");
                display.Call("AddMusic", (short)MusicLoader.GetMusicSlot(SundownerMod.Instance, path), displayName, author, modName);
            }

            AddMusic("Assets/Music/BrimstoneElemental", "BrimstoneElemental");
            AddMusic("Assets/Music/CeaselessVoid", "CeaselessVoid");
            AddMusic("Assets/Music/Signus", "Signus");
            AddMusic("Assets/Music/StormWeaver", "StormWeaver");
            AddMusic("Assets/Music/ExoMechs", "ExoMechs");
        }
        public override void OnModLoad()
        {
            ModLoader.TryGetMod("InfernumMode", out Mod infernum);
            InfernumMode = infernum;

            ModLoader.TryGetMod("InfernumModeMusic", out Mod infernumMusic);
            InfernumModeMusic = infernumMusic;

            On_SoundEngine.PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback += StopCataclysmicFabricationsIntro;
        }

        private SlotId StopCataclysmicFabricationsIntro(On_SoundEngine.orig_PlaySound_refSoundStyle_Nullable1_SoundUpdateCallback orig, ref SoundStyle style, Microsoft.Xna.Framework.Vector2? position, SoundUpdateCallback updateCallback)
        {
            if (style.SoundPath == "InfernumMode/Assets/Sounds/Custom/ExoMechs/ExoMechIntro" && (SundownerConfig.Instance.OverrideExoMechs || InfernumModeMusic == null))
                return SlotId.Invalid;

            return orig(ref style, position, updateCallback);
        }

        public static bool CheckInfernum(bool careAboutMode = false) => InfernumMode != null && (!careAboutMode || (bool)InfernumMode.Call("GetInfernumActive"));
   
    }
}
