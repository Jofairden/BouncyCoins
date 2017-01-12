using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;


namespace TheDeconstructor
{
	public class DeconstructorGUI : UIState
	{
		internal bool visible = false;
		internal bool dragging = false;
		private Vector2 offset;

		internal const float vpadding = 10;
		internal const float vwidth = 600;
		internal const float vheight = 400;

		private readonly UIElement _UIView;
		internal UIPanel basePanel;
		internal UIText baseTitle;
		internal UIImageButton closeButton;
		internal UIItemPanel deconItemPanel;
		internal UIRecipeList recipeList;
		internal UIScrollbar recipeScrollbar;

		internal List<Recipe> currentRecipes;

		public DeconstructorGUI()
		{
			base.SetPadding(vpadding);
			base.Width.Set(vwidth, 0f);
			base.Height.Set(vheight, 0f);
			_UIView = new UIElement();
			_UIView.CopyStyle(this);
			_UIView.Left.Set(Main.screenWidth/2f - _UIView.Width.Pixels/2f, 0f);
			_UIView.Top.Set(Main.screenHeight/2f - _UIView.Height.Pixels/2f, 0f);
			base.Append(_UIView);
		}

		public override void OnInitialize()
		{
			basePanel = new UIPanel();
			basePanel.OnMouseUp += (s, e) =>
			{
				_Recalculate(s.MousePosition);
				dragging = false;
			};
			basePanel.OnMouseDown += (s, e) =>
			{
				offset = new Vector2(s.MousePosition.X - _UIView.Left.Pixels, s.MousePosition.Y - _UIView.Top.Pixels);
				dragging = true;
			};
			basePanel.CopyStyle(this);
			_UIView.Append(basePanel);

			baseTitle = new UIText("Deconstructor", 0.85f, true);
			basePanel.Append(baseTitle);

			closeButton = new UIImageButton(TheDeconstructor.instance.GetTexture("closeButton"));
			closeButton.OnClick += (s, e) =>
			{
				visible = false;
			};
			closeButton.Width.Set(20f, 0f);
			closeButton.Height.Set(20f, 0f);
			closeButton.Left.Set(basePanel.Width.Pixels - closeButton.Width.Pixels * 2f - vpadding * 4f, 0f);
			closeButton.Top.Set(closeButton.Height.Pixels/2f, 0f);
			basePanel.Append(closeButton);

			deconItemPanel = new UIItemPanel();
			deconItemPanel.OnClick += (s, e) =>
			{
				Main.playerInventory = true;
				var panel = (e as UIItemPanel);
				if (panel?.item.type != 0 && Main.mouseItem.type != 0)
				{
					if (panel?.item.type != Main.mouseItem.type)
					{
						var tempItem = Main.mouseItem.Clone();
						var tempItem2 = panel.item.Clone();
						panel.item = tempItem;
						Main.mouseItem = tempItem2;
					}
					else
					{
						if (panel.item.maxStack <= 1) return;
						panel.item.stack += Main.mouseItem.stack;
						Main.mouseItem.SetDefaults(0);
					}
				}
				else if (panel?.item.type != 0)
				{
					Main.mouseItem = panel?.item.Clone();
					panel?.item.SetDefaults(0);
				}
				else if(Main.mouseItem != null)
				{
					panel.item = Main.mouseItem.Clone();
					Main.mouseItem.SetDefaults(0);
				}

				recipeList.Clear();

				if (panel?.item.type != 0)
				{
					currentRecipes = RecipeSearcher.FindRecipes(panel?.item);
					RecipeSearcher.FillWithRecipes(panel?.item, currentRecipes, recipeList, recipeScrollbar.Width.Pixels);
				}
			};
			deconItemPanel.Top.Set(deconItemPanel.Height.Pixels/2f + vpadding / 2f, 0f);
			basePanel.Append(deconItemPanel);


			var recipeInnerPanel = new UIPanel();
			recipeInnerPanel.Width.Set(basePanel.Width.Pixels - deconItemPanel.Width.Pixels*2f - vpadding * 2f, 0f);
			recipeInnerPanel.Height.Set(basePanel.Height.Pixels - deconItemPanel.Top.Pixels * 2f - vpadding * 3f, 0f);
			recipeInnerPanel.Left.Set(deconItemPanel.Width.Pixels + vpadding/2f, 0f);
			recipeInnerPanel.Top.Set(deconItemPanel.Top.Pixels, 0f);
			basePanel.Append(recipeInnerPanel);

			recipeList = new UIRecipeList(recipeInnerPanel.Width.Pixels, recipeInnerPanel.Height.Pixels);
			recipeList.SetPadding(0f);
			recipeList.Initialize();
			recipeInnerPanel.Append(recipeList);

			recipeScrollbar = new UIScrollbar();
			recipeScrollbar.Height.Set(recipeList.Height.Pixels - 4f * vpadding, 0F);
			recipeScrollbar.Left.Set(recipeList.Width.Pixels - recipeScrollbar.Width.Pixels * 2f - vpadding/2f, 0f);
			recipeScrollbar.Top.Set(vpadding, 0f);
			recipeList.SetScrollbar(recipeScrollbar);
			recipeList.Append(recipeScrollbar);



			//for (int i = 0; i < 7; i++)
			//{
			//	var testP = new UIRecipePanel(recipeList.Width.Pixels - 3f*vpadding - recipeScrollbar.Width.Pixels, 200f);
			//	//testP.Top.Set(testP.Top.Pixels + vpadding, 0f);
			//	//testP.Left.Set(testP.Left.Pixels + vpadding, 0f);
			//	recipeList.Add(testP);
			//}
		}

		public void _Recalculate(Vector2 mousePos, float precent = 0f)
		{
			_UIView.Left.Set(Math.Max(0, Math.Min(mousePos.X - offset.X, Main.screenWidth - basePanel.Width.Pixels)), precent);
			_UIView.Top.Set(Math.Max(0, Math.Min(mousePos.Y - offset.Y, Main.screenHeight - basePanel.Height.Pixels)), precent);
			Recalculate();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Vector2 mousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);

			if (basePanel.ContainsPoint(mousePosition))
			{
				Main.LocalPlayer.mouseInterface = true;
			}

			if (dragging)
			{
				_Recalculate(mousePosition);
			}
		}

		internal class UIRecipePanel : UIPanel
		{
			internal Recipe embeddedRecipe;
			internal float stackDiff;
			public List<UIItemPanel> materials;
			public UIRecipeBag recipeBag;

			public UIRecipePanel(float width, float height, float left = 0f, float top = 0f)
			{
				materials = new List<UIItemPanel>();
				for (int i = 0; i < 14; i++)
				{
					UIItemPanel matPanel = new UIItemPanel();
					matPanel.Left.Set((matPanel.Width.Pixels + vpadding/2f)*(i%7), 0f);
					matPanel.Top.Set(i < 7 ? 0f : matPanel.Height.Pixels + vpadding/2f, 0f);
					materials.Add(matPanel);
				}
				UIItemPanel lastPanel = new UIItemPanel();
				lastPanel.Left.Set(0f, 0f);
				lastPanel.Top.Set(2f*lastPanel.Height.Pixels + vpadding, 0f);
				materials.Add(lastPanel);

				base.Width.Set(width, 0f);
				base.Height.Set(height, 0f);
				base.Left.Set(left, 0f);
				base.Top.Set(top, 0f);

				recipeBag = new UIRecipeBag(TheDeconstructor.instance.GetTexture("DeconstructBagItem"));
				recipeBag.Parent = this;
				recipeBag.Width.Set(30f, 0f);
				recipeBag.Height.Set(40f, 0f);
				recipeBag.Top.Set(lastPanel.Top.Pixels + recipeBag.Height.Pixels/4f, 0f);
				recipeBag.Left.Set(lastPanel.Width.Pixels + vpadding, 0f);
				base.Append(recipeBag);
			}

			public override void OnInitialize()
			{
				foreach (var panel in materials)
				{
					base.Append(panel);
				}
			}
		}

		internal class UIRecipeBag : UIImageButton
		{
			public UIRecipeBag(Texture2D texture) : base(texture)
			{
				base.OnClick += (s, e) =>
				{
					if (Parent != null)
					{
						var parentPanel = (Parent as UIRecipePanel);
						var guiInst = TheDeconstructor.instance.deconGUI;
						var items = new List<Item>();

						// Remove stacks from panel item based on recipe cost
						var stack = guiInst.deconItemPanel.item.stack;
						var stackDiff = (float)stack / (float)parentPanel.embeddedRecipe.createItem.stack;
						stackDiff *= parentPanel.embeddedRecipe.createItem.stack;
						guiInst.deconItemPanel.item.stack -= (int)stackDiff;

						// Give the new bag item
						Item bagItem = new Item();
						bagItem.SetDefaults(TheDeconstructor.instance.ItemType<DeconstructBagItem>());
						parentPanel?.materials.ForEach(x => items.Add(x.item));
						var deconItem = (bagItem.modItem as DeconstructBagItem);
						deconItem.bagItems = items;
						deconItem.sourceItem = guiInst.deconItemPanel.item.Clone();
						deconItem.sourceItem.stack = (int)stackDiff;
						Main.LocalPlayer.GetItem(Main.myPlayer, bagItem);

						// Reset item panel if needed
						if (guiInst.deconItemPanel.item.stack <= 0)
							guiInst.deconItemPanel.item.SetDefaults(0);

						// Clear recipe list, reset dragging, otherwise UI starts draggin
						guiInst.recipeList?.Clear();
						guiInst.dragging = false;
					}
				};
			}

			public override void Update(GameTime gameTime)
			{
				if (base.IsMouseHovering)
				{
					var guiInst = TheDeconstructor.instance.deconGUI;
					var parentPanel = (Parent as UIRecipePanel);
					float materialsPrice = 0f;
					for (int i = 0; i < parentPanel?.materials.Count; i++)
					{
						materialsPrice += parentPanel.materials[i].item.value * parentPanel.materials[i].item.stack;
					}
					Main.hoverItemName = $"Click to receive recipe materials in a goodie bag\nResult worth: {guiInst.deconItemPanel.item.value * guiInst.deconItemPanel.item.stack} copper\nRecipe worth: {materialsPrice} copper";
				}
			}
		}

		internal class UIRecipeList : UIList
		{
			public List<UIRecipePanel> recipes;

			public UIRecipeList(float width, float height, float left = 0f, float top = 0f)
			{
				recipes = new List<UIRecipePanel>();
				base.Width.Set(width, 0f);
				base.Height.Set(height, 0f);
				base.Left.Set(left, 0f);
				base.Top.Set(top, 0f);
			}
		}

		internal class UIItemPanel : UIPanel
		{
			internal const float panelwidth = 50f;
			internal const float panelheight = 50f;
			internal const float panelpadding = 0f;
			private bool rightClicking = false;
			public Item item;

			public UIItemPanel(int type = 0, int stack = 0)
			{
				base.Width.Set(panelwidth, 0f);
				base.Height.Set(panelheight, 0f);
				base.SetPadding(panelpadding);
				item = new Item();
				item.SetDefaults(type);
				item.stack = stack;
			}

			public override void Update(GameTime gameTime)
			{
				rightClicking = Main.mouseRight && base.IsMouseHovering;

				if (rightClicking && item.type != 0)
				{
					Main.playerInventory = true;

					if (Main.stackSplit <= 1 && item.type != 0 && (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0))
					{
						int num2 = Main.superFastStack + 1;
						for (int j = 0; j < num2; j++)
						{
							if ((Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0) && item.stack > 0)
							{
								if (j == 0)
								{
									Main.PlaySound(18, -1, -1, 1);
								}
								if (Main.mouseItem.type == 0)
								{
									Main.mouseItem.netDefaults(item.netID);
									if (item.prefix != 0)
									{
										Main.mouseItem.Prefix((int)item.prefix);
									}
									Main.mouseItem.stack = 0;
								}
								Main.mouseItem.stack++;
								item.stack--;
								TheDeconstructor.instance.deconGUI.recipeList.Clear();
								RecipeSearcher.FillWithRecipes(item, TheDeconstructor.instance.deconGUI.currentRecipes,
									TheDeconstructor.instance.deconGUI.recipeList, TheDeconstructor.instance.deconGUI.recipeScrollbar.Width.Pixels);
								if (Main.stackSplit == 0)
								{
									Main.stackSplit = 15;
								}
								else
								{
									Main.stackSplit = Main.stackDelay;
								}

								if (item.stack <= 0)
								{
									TheDeconstructor.instance.deconGUI.recipeList.Clear();
									item.SetDefaults(0);
								}
							}
						}
					}
				}
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				base.DrawSelf(spriteBatch);

				if (item == null || item.type == 0) return;

				if (base.IsMouseHovering)
				{
					Main.hoverItemName = item.name;
					Main.toolTip = item.Clone();
					Main.toolTip.name = Main.toolTip.name + (Main.toolTip.modItem != null ? $" [{Main.toolTip.modItem.mod.Name}]" : "");
				}

				CalculatedStyle innerDimensions = base.GetInnerDimensions();
				Texture2D texture2D = Main.itemTexture[this.item.type];
				Rectangle frame;
				if (Main.itemAnimations[item.type] != null)
				{
					frame = Main.itemAnimations[item.type].GetFrame(texture2D);
				}
				else
				{
					frame = texture2D.Frame(1, 1, 0, 0);
				}
				float drawScale = 1f;
				float num2 = (float)innerDimensions.Width * 1f;
				if ((float)frame.Width > num2 || (float)frame.Height > num2)
				{
					if (frame.Width > frame.Height)
					{
						drawScale = num2 / (float)frame.Width;
					}
					else
					{
						drawScale = num2 / (float)frame.Height;
					}
				}
				Vector2 drawPosition = new Vector2(innerDimensions.X, innerDimensions.Y);
				drawPosition.X += (float)innerDimensions.Width * 1f / 2f - (float)frame.Width * drawScale / 2f;
				drawPosition.Y += (float)innerDimensions.Height * 1f / 2f - (float)frame.Height * drawScale / 2f;

				this.item.GetColor(Color.White);
				spriteBatch.Draw(texture2D, drawPosition, new Rectangle?(frame), this.item.GetAlpha(Color.White), 0f,
					Vector2.Zero, drawScale, SpriteEffects.None, 0f);
				if (this.item.color != default(Color))
				{
					spriteBatch.Draw(texture2D, drawPosition, new Rectangle?(frame), this.item.GetColor(Color.White), 0f,
						Vector2.Zero, drawScale, SpriteEffects.None, 0f);
				}

				if (this.item.stack > 1)
				{
					spriteBatch.DrawString(Main.fontItemStack, this.item.stack.ToString(), new Vector2(innerDimensions.Position().X + 10f * drawScale, innerDimensions.Position().Y + 26f * drawScale), Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
				}
			}
		}

		internal static class RecipeSearcher
		{
			public static List<Recipe> FindRecipes(Item item)
			{
				return Main.recipe.Where(recipe => recipe.createItem.type == item.type && recipe.createItem.stack <= item.stack).ToList();
			}

			public static void FillWithRecipes(Item source, List<Recipe> recipes, UIRecipeList list, float offset)
			{
				foreach (var recipe in recipes)
				{
					var recipePanel = new UIRecipePanel(list.Width.Pixels - 3f * vpadding - offset, 200f);
					recipePanel.Initialize();
					recipePanel.embeddedRecipe = recipe;
					for (int i = 0; i < recipePanel.materials.Count; i++)
					{
						recipePanel.materials[i].item = recipe.requiredItem[i].Clone();

						float stackDiff = (float) (source.stack - recipe.createItem.stack)/(float) recipe.createItem.stack;
						if (stackDiff > 0)
							recipePanel.materials[i].item.stack += recipePanel.materials[i].item.stack * (int)stackDiff;
					}
					list.Add(recipePanel);
				}
			}
		}
	}
}
