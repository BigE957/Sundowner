using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.Signus;
using Sundowner.Common;
using CalamityMod.Events;
using CalamityMod.NPCs;
using Sundowner.Common.Systems;

namespace Sundowner.Content.MusicScenes
{
    public class SignusScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/Signus");
        public override bool IsSceneEffectActive(Player player) => SundownerConfig.Instance.OverrideSignus && 
                                                                   NPC.AnyNPCs(ModContent.NPCType<Signus>()) && 
                                                                   (!ModCompat.CheckInfernum() || CalamityGlobalNPC.signus != -1) && 
                                                                   !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => ModCompat.CheckInfernum() ? (SceneEffectPriority)10 : SceneEffectPriority.BossHigh;
    }
}
