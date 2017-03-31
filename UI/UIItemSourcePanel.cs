using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using TheDeconstructor.Items;


namespace TheDeconstructor.UI
{
	// Is source panel (item to deconstruct)
	internal sealed class UIItemSourcePanel : UIInteractableItemPanel
	{
		public UIItemSourcePanel(int netID = 0, int stack = 0, Texture2D hintTexture = null, string hintText = null) :
			base(netID, stack, hintTexture, hintText)
		{

		}

		//public override void BindItem(DeconEntityInstance instance)
		//{
		//	item = instance.sourceItem.Clone();
		//}

		public override bool CanTakeItem(Item item)
		{
			return (item.modItem as Cube)?.State != Cube.CubeState.Sealed;
		}

		public override void PostOnClick(UIMouseEvent evt, UIElement e)
		{
			DoUpdate();
		}

		public override void PostOnRightClick()
		{
			DoUpdate();
		}

		internal void DoUpdate()
		{
			TheDeconstructor.instance.deconGUI.recipeList.Clear();
			DeconstructorGUI.currentRecipes = DeconstructorGUI.RecipeSearcher.FindRecipes(item);
			DeconstructorGUI.RecipeSearcher.FillWithRecipes(item);
		}
	}
}
