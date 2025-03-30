using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Sundowner.Common;

[BackgroundColor(128, 68, 69, 216)]
public class SundownerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("SongOverrides")]

    [BackgroundColor(220, 20, 60, 192)]
    [DefaultValue(true)]
    public bool OverrideBrimstoneElemental { get; set; }

    [BackgroundColor(238, 130, 238, 192)]
    [DefaultValue(true)]
    public bool OverrideStormWeaver { get; set; }

    [BackgroundColor(75, 0, 130, 192)]
    [DefaultValue(true)]
    public bool OverrideCeaselessVoid { get; set; }

    [BackgroundColor(186, 85, 211, 192)]
    [DefaultValue(true)]
    public bool OverrideSignus { get; set; }

    [BackgroundColor(127, 255, 212, 192)]
    [DefaultValue(true)]
    public bool OverrideExoMechs { get; set; }

}
