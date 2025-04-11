using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.StormWeaver;
using Sundowner.Common;
using CalamityMod.Events;
using Sundowner.Common.Systems;

namespace Sundowner.Content.MusicScenes
{
    public class StormWeaverScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/StormWeaver");
        public override bool IsSceneEffectActive(Player player) => SundownerConfig.Instance.OverrideStormWeaver && 
                                                                   (NPC.AnyNPCs(ModContent.NPCType<StormWeaverHead>()) || NPC.AnyNPCs(ModContent.NPCType<StormWeaverBody>()) || NPC.AnyNPCs(ModContent.NPCType<StormWeaverTail>())) && 
                                                                   !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => ModCompat.CheckInfernum() ? (SceneEffectPriority)10 : SceneEffectPriority.BossHigh;
    }
}
