using MonoStereo.Filters;
using MonoStereoMod;
using Sundowner.Content.Items.MusicBoxes;
using Sundowner.Content.Tiles.MusicBoxes;
using Terraria;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems
{
    public class MusicRegistry: ModSystem
    {
        private static void AddMusicBox(string musicFile, int itemID, int tileID)
        {
            Mod sundowner = SundownerMod.Instance;
            int musicID = MusicLoader.GetMusicSlot(sundowner, musicFile);
            MusicLoader.AddMusicBox(sundowner, musicID, itemID, tileID);
        }

        public override void PostSetupContent()
        {
            if (!Main.dedServ)
            {
                AddMusicBox("Assets/Music/BrimstoneElemental", ModContent.ItemType<BrimstoneElementalMusicBox>(), ModContent.TileType<BrimstoneElementalMusicBoxTile>());
                AddMusicBox("Assets/Music/CeaselessVoid", ModContent.ItemType<CeaselessVoidMusicBox>(), ModContent.TileType<CeaselessVoidMusicBoxTIle>());
                AddMusicBox("Assets/Music/Signus", ModContent.ItemType<SignusMusicBox>(), ModContent.TileType<SignusMusicBoxTile>());
                AddMusicBox("Assets/Music/StormWeaver", ModContent.ItemType<StormWeaverMusicBox>(), ModContent.TileType<StormWeaverMusicBoxTile>());
                AddMusicBox("Assets/Music/ExoMechs", ModContent.ItemType<ExoMechsMusicBox>(), ModContent.TileType<ExoMechsMusicBoxTile>());
            }
        }
    }
}
