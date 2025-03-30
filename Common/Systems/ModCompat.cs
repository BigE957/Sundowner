using Terraria.Localization;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems
{
    public class ModCompat : ModSystem
    {
        private static readonly string displayPath = "ModCompat.MusicDisplay.";
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
    }
}
