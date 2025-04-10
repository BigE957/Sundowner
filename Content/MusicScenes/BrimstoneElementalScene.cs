using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.BrimstoneElemental;
using Sundowner.Common;
using CalamityMod.Events;

namespace Sundowner.Content.MusicScenes
{
    public class BrimstoneElementalScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/BrimstoneElemental");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideBrimstoneElemental && NPC.AnyNPCs(ModContent.NPCType<BrimstoneElemental>()) && !BossRushEvent.BossRushActive;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
    }
}
