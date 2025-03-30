using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.Signus;
using Sundowner.Common;

namespace Sundowner.Content.MusicScenes
{
    public class SignusScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/Signus");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideSignus && NPC.AnyNPCs(ModContent.NPCType<Signus>());
        public override SceneEffectPriority Priority => (SceneEffectPriority)9;
    }
}
