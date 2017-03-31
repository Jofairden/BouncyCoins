using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using TheDeconstructor.Items;


namespace TheDeconstructor.UI
{
	/// <summary>
	/// Clickable, to deconstruct selected recipe
	/// </summary>
	internal class UIRecipeBag : UIImageButton
	{
		public UIRecipeBag(Texture2D texture) : base(texture)
		{
			base.OnClick += (s, e) =>
			{
				if (Parent != null)
				{
					var recipePanel = (Parent as UIRecipePanel);
					var guiInst = TheDeconstructor.instance.deconGUI;
					var items = new List<Item>();
					guiInst.dragging = false;

					if (guiInst.cubeItemPanel.item.IsAir)
					{
						SoundHelper.PlaySound(SoundHelper.SoundType.Decline);
						recipePanel.errorTime = 550f;
						recipePanel.errorText.SetText("Place in an unsealed cube first!");
						return;
					}

					if ((guiInst.cubeItemPanel.item.modItem as Cube)?.State == Cube.CubeState.Sealed)
					{
						SoundHelper.PlaySound(SoundHelper.SoundType.Decline);
						recipePanel.errorTime = 550f;
						recipePanel.errorText.SetText("The current cube is already sealed!");
						return;
					}

					// Tries to 'buy'
					if (!Main.LocalPlayer.BuyItemOld(recipePanel.deconstructValue.RawValue))
					{
						SoundHelper.PlaySound(SoundHelper.SoundType.Decline);
						recipePanel.errorTime = 550f;
						recipePanel.errorText.SetText("You do not have enough gold!");
						return;
					}

					SoundHelper.PlaySound(SoundHelper.SoundType.Receive);
					// Remove stacks from panel item based on recipe cost
					var stack = guiInst.sourceItemPanel.item.stack;
					var stackDiff = (float)stack / (float)recipePanel.embeddedRecipe.createItem.stack;
					stackDiff *= recipePanel.embeddedRecipe.createItem.stack;
					guiInst.sourceItemPanel.item.stack -= (int)stackDiff;

					// Generate sealed cube
					Cube cube = guiInst.cubeItemPanel.item.Clone().modItem as Cube;
					recipePanel.materials.ForEach(x => items.Add(x.item));
					cube.SealedItems = new List<Item>(items);
					cube.SealedSource = (Item)guiInst.sourceItemPanel.item.Clone();
					cube.SealedSource.stack = (int)stackDiff;
					cube.CanFail = recipePanel.canFail;
					cube.State = Cube.CubeState.Sealed;
					cube.item.value = recipePanel.materialsValue.RawValue;
					guiInst.cubeItemPanel.item = cube.item.Clone();

					// Reset item panel if needed
					if (guiInst.sourceItemPanel.item.stack <= 0)
						guiInst.sourceItemPanel.item.TurnToAir();

					guiInst.sourceItemPanel.DoUpdate();
				}
			};
		}

		public override void Update(GameTime gameTime)
		{
			if (base.IsMouseHovering)
			{
				var parentPanel = (Parent as UIRecipePanel);
				Main.hoverItemName =
					$"{DeconstructorGUI.hoverString}Click to seal recipe inside cube" +
					$"\nResult worth: {parentPanel?.resultValue}" +
					$"\nRecipe worth: {parentPanel?.materialsValue}" +
					$"\nDeconstruction cost: {parentPanel?.deconstructValue}";
			}
		}
	}
}
