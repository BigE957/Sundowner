using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.ExoMechs;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using Sundowner.Common;
using CalamityMod.Events;
using Sundowner.Common.Systems;
using System.Reflection;
using System.Linq;
using Terraria.ModLoader.Core;
using System;
using Terraria.Audio;

namespace Sundowner.Content.MusicScenes
{
    public class ExoMechsScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/ExoMechs");
        public override bool IsSceneEffectActive(Player player)
        {
            bool exoMechPresent = NPC.AnyNPCs(ModContent.NPCType<Apollo>()) ||
                                  NPC.AnyNPCs(ModContent.NPCType<AresBody>()) ||
                                  NPC.AnyNPCs(ModContent.NPCType<Artemis>()) ||
                                  NPC.AnyNPCs(ModContent.NPCType<ThanatosHead>()) ||
                                  NPC.AnyNPCs(ModContent.NPCType<ThanatosBody1>()) ||
                                  NPC.AnyNPCs(ModContent.NPCType<ThanatosBody2>()) ||
                                  NPC.AnyNPCs(ModContent.NPCType<ThanatosTail>());

            bool isActive = SundownerConfig.Instance.OverrideExoMechs &&
                   !BossRushEvent.BossRushActive &&
                   (NPC.AnyNPCs(ModContent.NPCType<Draedon>()) || exoMechPresent) &&
                   CalamityGlobalNPC.draedonAmbience == -1 &&
                   ((Draedon)Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Draedon>())].ModNPC).DefeatTimer <= 0 &&
                   (!ModCompat.CheckInfernum() || exoMechPresent);

            return isActive;
        }

        public override SceneEffectPriority Priority => ModCompat.CheckInfernum() ? (SceneEffectPriority)12 : (SceneEffectPriority)9;

    }
}
