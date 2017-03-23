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
            item.toolTip = "Right click to seal materials";
            item.width = 26;
            item.height = 38;
            State = CubeState.Open;
        }

        public override ModItem Clone()
             => CubeClone<QueerLunarCube>() as ModItem;

        public override string TexturePath => 
            "Items/QueerLunarCubeFrames";

        public override int InvFMax =>
            7;
    }

}
