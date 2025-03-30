using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.CeaselessVoid;
using Sundowner.Common;

namespace Sundowner.Content.MusicScenes
{
    internal class CeaselessVoidScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/CeaselessVoid");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideCeaselessVoid && NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>());
        public override SceneEffectPriority Priority => (SceneEffectPriority)9;
    }
}
