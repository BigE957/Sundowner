using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.StormWeaver;
using Sundowner.Common;

namespace Sundowner.Content.MusicScenes
{
    internal class StormWeaverScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/StormWeaver");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideStormWeaver && (NPC.AnyNPCs(ModContent.NPCType<StormWeaverHead>()) || NPC.AnyNPCs(ModContent.NPCType<StormWeaverBody>()) || NPC.AnyNPCs(ModContent.NPCType<StormWeaverTail>()));
        public override SceneEffectPriority Priority => (SceneEffectPriority)9;
    }
}
