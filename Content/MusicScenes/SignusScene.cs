using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.Signus;
using Sundowner.Common;
using CalamityMod.Events;
using CalamityMod.NPCs;

namespace Sundowner.Content.MusicScenes
{
    public class SignusScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/Signus");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideSignus && 
                                                                   NPC.AnyNPCs(ModContent.NPCType<Signus>()) && 
                                                                   (!ModLoader.TryGetMod("InfernumMode", out _) || CalamityGlobalNPC.signus != -1) && 
                                                                   !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => ModLoader.TryGetMod("InfernumMode", out _) ? (SceneEffectPriority)10 : SceneEffectPriority.BossHigh;
    }
}
