using Terraria.ID;
using Terraria.ModLoader;

namespace Sundowner.Content.Items.MusicBoxes
{
    public class BrimstoneElementalMusicBox : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override string Texture => "Sundowner/Assets/MusicBoxes/BrimstoneElemental/Item";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
        }

        public override void SetDefaults() => Item.DefaultToMusicBox(ModContent.TileType<Tiles.MusicBoxes.BrimstoneElementalMusicBoxTile>(), 0);
    }
}