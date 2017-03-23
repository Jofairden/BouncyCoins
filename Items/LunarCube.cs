using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheDeconstructor.Items
{
	internal sealed class LunarCube : Cube
	{
        public override void SetDefaults()
		{
            base.SetDefaults();
            item.name = "Lunar Cube";
            item.width = 20;
			item.height = 28;
			item.rare = 9;
		}

        public override ModItem Clone() 
            => CubeClone<LunarCube>() as ModItem;

        internal override string TexturePath =>
            "Items/LunarCubeFrames";

        internal override int InvFMax =>
            7;
    }
}
