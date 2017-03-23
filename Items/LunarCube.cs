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
            item.toolTip = "Right click to break seal";
            item.width = 20;
			item.height = 28;
			item.rare = 9;
            State = CubeState.Open;
		}

        public override ModItem Clone() 
            => CubeClone<LunarCube>() as ModItem;

        public override string TexturePath =>
            "Items/LunarCubeFrames";

        public override int InvFMax =>
            7;
    }
}
