using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace Sundowner.Content.Items.MusicBoxes
{
    public class ExoMechsMusicBox : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override string Texture => "Sundowner/Assets/MusicBoxes/ExoMechs/Item";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
        }

        public override void SetDefaults() => Item.DefaultToMusicBox(ModContent.TileType<Tiles.MusicBoxes.ExoMechsMusicBoxTile>(), 0);
    }
}
