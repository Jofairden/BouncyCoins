using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace TheDeconstructor.Items
{
    internal sealed class QueerLunarCube : Cube
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            item.name = "Queer Lunar Cube";
            item.width = 26;
            item.height = 38;
            item.rare = 10;
        }

        public override ModItem Clone()
             => CubeClone<QueerLunarCube>() as ModItem;

        internal override string TexturePath => 
            "Items/QueerLunarCubeFrames";

        internal override int InvFMax =>
            7;
    }

}
