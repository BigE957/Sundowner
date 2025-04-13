﻿using CalamityMod.Items;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using MonoStereoMod;
using Terraria.Audio;
using Sundowner.Common.Systems;
using MonoStereo;

namespace Sundowner.Content.Items.Debug
{
    public class MonoStereoTest : ModItem
    {
        public override string Texture => "CalamityMod/Items/LabFinders/YellowSeekingMechanism";

        public override void SetDefaults()
        {
            Item.useAnimation = Item.useTime = 22;
            Item.width = 42;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.useTurn = true;
        }
        public override bool? UseItem(Player player)
        {
            MonoStereoSystem.CheckTrack = !MonoStereoSystem.CheckTrack;
            //AudioManager.MasterMixer.Volume = 0;
            return true;
        }
    }
}
