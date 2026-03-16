using CalamityMod.NPCs.CeaselessVoid;
using Microsoft.Xna.Framework;
using Sundowner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems.DynamicMusic;

public class CeaselessVoidDynamicMusic : LayeredDynamicMusic
{
    private PlaybackSpeedEffect _playbackSpeed;
    private bool DyingAPainfulDeath = false;

    protected override bool CanUpdate() => NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>()) || DyingAPainfulDeath || Main.curMusic == MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/CeaselessVoid");

    protected override int GetDesiredLayerIndex() => 0;

    protected override string[] GetLayerPaths() =>
    [
        "Assets/Music/CeaselessVoid.ogg"
    ];

    protected override void PostMusicLoad()
    {
        _playbackSpeed = new PlaybackSpeedEffect(LayeredTrack) { Speed = 1f };
        LayeredTrack.AddEffect(_playbackSpeed);
    }

    protected override void PostUpdate()
    {
        int slot = MusicLoader.GetMusicSlot(SundownerMod.Instance, "Assets/Music/CeaselessVoid");
        if (Main.musicBox2 == slot)
        {
            _playbackSpeed.Speed = 1f;
            DyingAPainfulDeath = false;
            return;
        }

        if (Main.musicFade[slot] == 0f)
        {
            _playbackSpeed.Speed = 1f;
            DyingAPainfulDeath = false;
            return;
        }

        int VoidIndex = NPC.FindFirstNPC(ModContent.NPCType<CeaselessVoid>());

        if (VoidIndex != -1)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<DarkEnergy>()))
                ((CeaselessVoid)Main.npc[VoidIndex].ModNPC).playedbuildsound = false;

            if (((CeaselessVoid)Main.npc[VoidIndex].ModNPC).playedbuildsound)
            {
                _playbackSpeed.Speed += 0.004f;
                DyingAPainfulDeath = true;
            }
            else
            {
                if (_playbackSpeed.Speed != 1f)
                    _playbackSpeed.Speed = 1f;
            }
        }
        else if (DyingAPainfulDeath)
        {
            if (_playbackSpeed.Speed < 0.005f)
            {
                _playbackSpeed.Speed = 0;
                DyingAPainfulDeath = false;
            }
            else
                _playbackSpeed.Speed *= 0.975f;
        }
    }
}