using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using Sundowner.Common;
using tModPorter;
using CalamityMod.Events;

namespace Sundowner.Content.MusicScenes
{
    internal class ExoMechsScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/ExoMechs");
        public override bool IsSceneEffectActive(Player player) => ModContent.GetInstance<SundownerConfig>().OverrideExoMechs && !BossRushEvent.BossRushActive && (
            NPC.AnyNPCs(ModContent.NPCType<Draedon>()) ||
            NPC.AnyNPCs(ModContent.NPCType<Apollo>()) ||
            NPC.AnyNPCs(ModContent.NPCType<AresBody>()) ||
            NPC.AnyNPCs(ModContent.NPCType<Artemis>()) ||
            NPC.AnyNPCs(ModContent.NPCType<ThanatosHead>()) ||
            NPC.AnyNPCs(ModContent.NPCType<ThanatosBody1>()) ||
            NPC.AnyNPCs(ModContent.NPCType<ThanatosBody2>()) ||
            NPC.AnyNPCs(ModContent.NPCType<ThanatosTail>()))
            && CalamityGlobalNPC.draedonAmbience == -1;
        public override SceneEffectPriority Priority => ModLoader.TryGetMod("InfernumMode", out _) ? (SceneEffectPriority)11 : (SceneEffectPriority)9;
    }
}
