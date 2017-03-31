using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TheDeconstructor.Items;


namespace TheDeconstructor.UI
{
	// Is cube panel (only accepts unsealed cube)
	internal sealed class UIItemCubePanel : UIInteractableItemPanel
	{
		public UIItemCubePanel(int netID = 0, int stack = 0, Texture2D hintTexture = null, string hintText = null) :
			base(netID, stack, hintTexture, hintText)
		{
		}

		//public override void BindItem(DeconEntityInstance instance)
		//{
		//	item = instance.cubeItem.Clone();
		//}

		public override bool CanTakeItem(Item item)
		{
			return item.modItem is Cube
					&& ((Cube)item.modItem).State == Cube.CubeState.Open;
		}
	}
}
