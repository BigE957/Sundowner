using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.CeaselessVoid;
using Sundowner.Common;
using CalamityMod.Events;
using CalamityMod.NPCs;
using Sundowner.Common.Systems;

namespace Sundowner.Content.MusicScenes
{
    public class CeaselessVoidScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/CeaselessVoid");
        public override bool IsSceneEffectActive(Player player) => SundownerConfig.Instance.OverrideCeaselessVoid && 
                                                                   NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>()) && 
                                                                   CalamityGlobalNPC.ghostBoss == -1 && 
                                                                   (!ModCompat.CheckInfernum(true) || (CalamityGlobalNPC.voidBoss == -1 || Main.npc[CalamityGlobalNPC.voidBoss].ai[0] != 0f)) && 
                                                                   !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => ModCompat.CheckInfernum() ? (SceneEffectPriority)10 : SceneEffectPriority.BossHigh;
    }
}
