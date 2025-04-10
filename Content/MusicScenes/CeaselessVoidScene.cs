using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.CeaselessVoid;
using Sundowner.Common;
using CalamityMod.Events;

namespace Sundowner.Content.MusicScenes
{
    internal class CeaselessVoidScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/CeaselessVoid");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideCeaselessVoid && NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>()) && !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
    }
}
