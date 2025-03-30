using Terraria.ModLoader;

namespace Sundowner
{
	public class SundownerMod : Mod
	{
        internal static SundownerMod Instance;
        public override void Load()
        {
            Instance = this;
        }
        public override void Unload()
        {
            Instance = null;
        }
    }
}
