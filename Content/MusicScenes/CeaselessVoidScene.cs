using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.CeaselessVoid;
using Sundowner.Common;
using CalamityMod.Events;
using CalamityMod.NPCs;

namespace Sundowner.Content.MusicScenes
{
    internal class CeaselessVoidScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/CeaselessVoid");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideCeaselessVoid && 
                                                                   NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>()) && 
                                                                   CalamityGlobalNPC.ghostBoss == -1 && 
                                                                   (!ModLoader.TryGetMod("InfernumMode", out _) || (CalamityGlobalNPC.voidBoss == -1 || Main.npc[CalamityGlobalNPC.voidBoss].ai[0] != 0f)) && 
                                                                   !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => ModLoader.TryGetMod("InfernumMode", out _) ? (SceneEffectPriority)10 : SceneEffectPriority.BossHigh;
    }
}
