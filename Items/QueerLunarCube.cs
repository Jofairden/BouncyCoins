using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheDeconstructor.Items
{
    internal sealed class QueerLunarCube : Cube
    {
		public override bool Autoload(ref string name, ref string texture, IList<EquipType> equips)
		{
			texture = $"{mod.Name}/Items/LunarCube";
			return base.Autoload(ref name, ref texture, equips);
		}

		public override void SetDefaults()
        {
            base.SetDefaults();
            item.name = "Queer Lunar Cube";
            item.width = 20;
            item.height = 28;
            item.rare = 10;
        }

        public override ModItem Clone()
             => CubeClone<QueerLunarCube>() as ModItem;

        internal override string TexturePath => 
            "Items/LunarCubeFrames";

        internal override int InvFMax =>
            7;
    }

}
