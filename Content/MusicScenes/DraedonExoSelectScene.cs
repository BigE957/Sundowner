using CalamityMod.Events;
using CalamityMod.NPCs.ExoMechs;
using Sundowner.Common;
using Sundowner.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Sundowner.Content.MusicScenes
{
    public class DraedonExoSelectScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(ModContent.GetInstance<CalamityMod.CalamityMod>(), "Sounds/Music/DraedonExoSelect");
        public override bool IsSceneEffectActive(Player player) => SundownerConfig.Instance.OverrideExoMechs && 
                                                                   NPC.AnyNPCs(ModContent.NPCType<Draedon>()) && 
                                                                   !BossRushEvent.BossRushActive && 
                                                                   (((Draedon)Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Draedon>())].ModNPC).DefeatTimer <= 0 || ModCompat.InfernumModeMusic == null);
        public override SceneEffectPriority Priority => ModCompat.CheckInfernum() ? (SceneEffectPriority)11 : 0;
    }
}
