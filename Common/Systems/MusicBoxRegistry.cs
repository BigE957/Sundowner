using Terraria;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems
{
    public class MusicBoxRegistry: ModSystem
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
                AddMusicBox("Assets/Music/BrimstoneElemental", ModContent.ItemType<Content.Items.MusicBoxes.BrimstoneElementalMusicBox>(), ModContent.TileType<Content.Tiles.MusicBoxes.BrimstoneElementalMusicBox>());
                AddMusicBox("Assets/Music/CeaselessVoid", ModContent.ItemType<Content.Items.MusicBoxes.CeaselessVoidMusicBox>(), ModContent.TileType<Content.Tiles.MusicBoxes.CeaselessVoidMusicBox>());
                AddMusicBox("Assets/Music/Signus", ModContent.ItemType<Content.Items.MusicBoxes.SignusMusicBox>(), ModContent.TileType<Content.Tiles.MusicBoxes.SignusMusicBox>());
                AddMusicBox("Assets/Music/StormWeaver", ModContent.ItemType<Content.Items.MusicBoxes.StormWeaverMusicBox>(), ModContent.TileType<Content.Tiles.MusicBoxes.StormWeaverMusicBox>());
                AddMusicBox("Assets/Music/ExoMechs", ModContent.ItemType<Content.Items.MusicBoxes.ExoMechsMusicBox>(), ModContent.TileType<Content.Tiles.MusicBoxes.ExoMechsMusicBox>());
            }
        }
    }
}
