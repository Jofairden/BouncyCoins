using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TheDeconstructor.Items;
using TheDeconstructor.Tiles;


namespace TheDeconstructor
{
	internal sealed class DeconstructorGUI : UIState
	{
		internal Point16? tileData = null;
		internal bool hoveringChild = false;
		internal bool visible = false;
		internal bool dragging = false;
		private Vector2 offset;

		internal const float vpadding = 10;
		internal const float vwidth = 555;
		internal const float vheight = 400;

		private readonly UIElement _UIView;
		internal UIPanel basePanel;
		internal UIText baseTitle;
		internal UIImageButton closeButton;
		internal UIItemCubePanel cubeItemPanel;
		internal UIItemSourcePanel sourceItemPanel;
		internal UIList recipeList;
		internal UIScrollbar recipeScrollbar;

		internal static List<Recipe> currentRecipes;
		internal static List<short> failureTypes; // used for checking costs
		internal static string hoverString; // used for hovering stuff

		internal DeconstructorGUI()
		{
			base.SetPadding(vpadding);
			base.Width.Set(vwidth, 0f);
			base.Height.Set(vheight, 0f);
			_UIView = new UIElement();
			_UIView.CopyStyle(this);
			_UIView.Left.Set(Main.screenWidth / 2f - _UIView.Width.Pixels / 2f, 0f);
			_UIView.Top.Set(Main.screenHeight / 2f - _UIView.Height.Pixels / 2f, 0f);
			base.Append(_UIView);

			// Some reflection here, because item.potion only seems to be set for health potions
			// Moved this here because there's no reason to continously call it
			// Gets all types from the ItemID class which variable name contains "potion"
			ItemID itemIDInst = new ItemID();
			failureTypes = typeof(ItemID)
				.GetFields()
				.Where(field =>
				field.Name.ToUpper().Contains("POTION")
				|| (field.Name != "ToxicFlask" && field.Name.ToUpper().Contains("FLASK")))
				.Select(field => (short)field.GetValue(itemIDInst))
				.ToList();
			failureTypes.AddRange(new short[]
				{ItemID.FragmentSolar, ItemID.FragmentNebula, ItemID.FragmentStardust, ItemID.FragmentVortex});
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
			basePanel.SetPadding(vpadding);
			_UIView.Append(basePanel);

			baseTitle = new UIText("Deconstructor", 0.85f, true);
			basePanel.Append(baseTitle);

			closeButton = new UIImageButton(TheDeconstructor.instance.GetTexture("closeButton"));
			closeButton.OnClick += (s, e) =>
			{
				TheDeconstructor.instance.TryToggleGUI();
			};
			closeButton.Width.Set(20f, 0f);
			closeButton.Height.Set(20f, 0f);
			closeButton.Left.Set(basePanel.Width.Pixels - closeButton.Width.Pixels * 2 - vpadding * 4.75f, 0f);
			closeButton.Top.Set(closeButton.Height.Pixels / 2f, 0f);
			basePanel.Append(closeButton);

			cubeItemPanel =
				new UIItemCubePanel(0, 0,
						TheDeconstructor.instance.GetTexture("Items/CubeHint"),
						"Place an unsealed cube here");

			cubeItemPanel.Top.Set(cubeItemPanel.Height.Pixels / 2f + vpadding / 2f, 0f);
			basePanel.Append(cubeItemPanel);

			sourceItemPanel = new UIItemSourcePanel(0, 0,
				ModLoader.GetTexture("Terraria/Item_" + ItemID.WoodenSword),
				"Place an item you want to destruct here");
			sourceItemPanel.Top.Set(cubeItemPanel.Top.Pixels + cubeItemPanel.Height.Pixels + vpadding / 2f, 0f);
			basePanel.Append(sourceItemPanel);

			recipeList = new UIList();
			recipeList.Width.Set(basePanel.Width.Pixels - cubeItemPanel.Width.Pixels * 2f, 0f);
			recipeList.Height.Set(basePanel.Height.Pixels - cubeItemPanel.Top.Pixels * 2f - vpadding * 3f, 0f);
			recipeList.Left.Set(cubeItemPanel.Width.Pixels + vpadding / 2f, 0f);
			recipeList.Top.Set(cubeItemPanel.Top.Pixels, 0f);

			recipeList.SetPadding(0);
			recipeList.Initialize();
			basePanel.Append(recipeList);

			recipeScrollbar = new UIScrollbar();
			recipeScrollbar.Height.Set(recipeList.Height.Pixels - 2f * vpadding, 0F);
			recipeScrollbar.Left.Set(recipeList.Width.Pixels - recipeScrollbar.Width.Pixels * 2f - vpadding / 2f, 0f);
			recipeScrollbar.Top.Set(vpadding, 0f);
			recipeList.SetScrollbar(recipeScrollbar);
			recipeList.Append(recipeScrollbar);

			sourceItemPanel.DoUpdate();
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

		public void TryPutInCube(bool queer = false)
		{
			if (cubeItemPanel.item.IsAir)
			{
				cubeItemPanel.item = new Item();
				cubeItemPanel.item.SetDefaults(
					queer
					? TheDeconstructor.instance.ItemType<QueerLunarCube>()
					: TheDeconstructor.instance.ItemType<LunarCube>());
			}
		}

		public void TryGetSource(bool force = false)
		{
			if (!sourceItemPanel.item.IsAir && (!visible || force))
			{
				Main.LocalPlayer.GiveClonedItem(sourceItemPanel.item, sourceItemPanel.item.stack);
				//Main.LocalPlayer.GetItem(Main.myPlayer, sourceItemPanel.item.Clone()); // does not seem to generate item text
				//Main.LocalPlayer.QuickSpawnItem(sourceItemPanel.item.type, sourceItemPanel.item.stack);
				sourceItemPanel.item.TurnToAir();
			}
		}

		public void TryGetCube(bool force = false)
		{
			if (!cubeItemPanel.item.IsAir
				&& (!visible || force))
			{
				Main.LocalPlayer.GiveClonedItem(cubeItemPanel.item, cubeItemPanel.item.stack);
				//Main.LocalPlayer.GetItem(Main.myPlayer, cubeItemPanel.item.Clone());
				//Main.LocalPlayer.QuickSpawnItem(sourceItemPanel.item.type, sourceItemPanel.item.stack);
				cubeItemPanel.item.TurnToAir();
			}
		}

		public void ToggleUI(bool on = true)
		{
			if (!on)
			{
				recipeList.Clear();
				TryGetCube();
				TryGetSource();
			}
			else
			{
				sourceItemPanel.DoUpdate();
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (tileData == null)
				return;

			//// Get tile entity from tile data (top left 0, 0 frame of tile)
			//var TE = TileEntity.ByPosition[tileData.Value] as DeconstructorTE;
			//// Close UI if too far from tile
			//if (Math.Abs(TE.DistanceToLocalPlayer.X) > 12f * 16f || Math.Abs(TE.DistanceToLocalPlayer.Y) > 12f * 16f
			//	|| Main.inputTextEscape || Main.LocalPlayer.dead || Main.gameMenu)
			//{
			//	TheDeconstructor.instance.TryToggleGUI(false);
			//}
		}

		internal class UIRecipePanel : UIPanel
		{
			internal Recipe embeddedRecipe;
			internal float stackDiff;
			internal bool canFail;

			internal ItemValue materialsValue;
			internal ItemValue resultValue;
			internal ItemValue deconstructValue;

			public List<UIItemPanel> materials;
			public UIRecipeBag recipeBag;

			internal UIText errorText;
			internal float errorTime;

			public UIRecipePanel(float width, float height, float left = 0f, float top = 0f)
			{
				materials = new List<UIItemPanel>();
				for (int i = 0; i < 14; i++)
				{
					UIItemPanel matPanel = new UIItemPanel();
					matPanel.Left.Set((matPanel.Width.Pixels + vpadding / 2f) * (i % 7), 0f);
					matPanel.Top.Set(i < 7 ? 0f : matPanel.Height.Pixels + vpadding / 2f, 0f);
					materials.Add(matPanel);
				}
				UIItemPanel lastPanel = new UIItemPanel();
				lastPanel.Left.Set(0f, 0f);
				lastPanel.Top.Set(2f * lastPanel.Height.Pixels + vpadding, 0f);
				materials.Add(lastPanel);

				base.Width.Set(width, 0f);
				base.Height.Set(height, 0f);
				base.Left.Set(left, 0f);
				base.Top.Set(top, 0f);

				recipeBag = new UIRecipeBag(TheDeconstructor.instance.GetTexture("DeconstructBagItem")) { Parent = this };
				recipeBag.Width.Set(30f, 0f);
				recipeBag.Height.Set(40f, 0f);
				recipeBag.Top.Set(lastPanel.Top.Pixels + recipeBag.Height.Pixels / 4f, 0f);
				recipeBag.Left.Set(lastPanel.Width.Pixels + vpadding, 0f);
				base.Append(recipeBag);

				errorTime = 2;
				errorText = new UIText(" ");
				errorText.Width.Set(25f, 0f);
				errorText.Height.Set(25f, 0f);
				errorText.Top.Set(recipeBag.Top.Pixels + Main.fontMouseText.MeasureString(errorText.Text).Y / 2f, 0f);
				errorText.Left.Set(recipeBag.Left.Pixels + recipeBag.Width.Pixels + vpadding, 0f);
				base.Append(errorText);
			}

			public override void OnInitialize()
			{
				foreach (var panel in materials)
				{
					base.Append(panel);
				}
			}

			public override void Update(GameTime gameTime)
			{
				base.Update(gameTime);

				if (errorTime > 0f)
				{
					errorTime -= 1f;
					if (errorTime <= 0f)
					{
						errorText.SetText("");
					}
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
						$"{hoverString}Click to receive recipe materials in a goodie bag" +
						$"\nResult worth: {parentPanel?.resultValue}" +
						$"\nRecipe worth: {parentPanel?.materialsValue}" +
						$"\nDeconstruction cost: {parentPanel?.deconstructValue}";
				}
			}
		}

		// Is cube panel (only accepts unsealed cube)
		internal sealed class UIItemCubePanel : UIInteractableItemPanel
		{
			public UIItemCubePanel(int netID = 0, int stack = 0, Texture2D hintTexture = null, string hintText = null) :
				base(netID, stack, hintTexture, hintText)
			{
			}

			public override bool CanTakeItem(Item item)
			{
				return item.modItem is Cube
						&& (item.modItem as Cube).State == Cube.CubeState.Open;
			}
		}

		// Is source panel (item to deconstruct)
		internal sealed class UIItemSourcePanel : UIInteractableItemPanel
		{
			public UIItemSourcePanel(int netID = 0, int stack = 0, Texture2D hintTexture = null, string hintText = null) :
				base(netID, stack, hintTexture, hintText)
			{

			}

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
				currentRecipes = RecipeSearcher.FindRecipes(item);
				RecipeSearcher.FillWithRecipes(item, currentRecipes,
					ref TheDeconstructor.instance.deconGUI.recipeList,
					TheDeconstructor.instance.deconGUI.recipeScrollbar.Width.Pixels);
			}
		}

		// Allows user to put in / take out item
		internal class UIInteractableItemPanel : UIItemPanel
		{
			public UIInteractableItemPanel(int netID = 0, int stack = 0, Texture2D hintTexture = null, string hintText = null)
				: base(netID, stack, hintTexture, hintText)
			{
				base.OnClick += UIInteractableItemPanel_OnClick;
			}

			public override void Update(GameTime gameTime)
			{
				base.Update(gameTime);
				if (base.IsMouseHovering
					&& Main.mouseRight)
				{
					// Slot has an item
					if (!item.IsAir)
					{
						// Open inventory
						Main.playerInventory = true;

						// Mouseitem has to be the same as slot
						if (Main.stackSplit <= 1 &&
							(Main.mouseItem.type == item.type
							|| Main.mouseItem.IsAir))
						{
							int num2 = Main.superFastStack + 1;
							for (int j = 0; j < num2; j++)
							{
								// Mouseitem is air, or stack is smaller than maxstack, and slot has stack
								if (Main.mouseItem.IsAir
									|| (Main.mouseItem.stack < Main.mouseItem.maxStack)
									&& item.stack > 0)
								{
									// Play sound
									if (j == 0)
									{
										Main.PlaySound(18, -1, -1, 1);
									}
									// Mouseitem is air, clone item
									if (Main.mouseItem.IsAir)
									{
										Main.mouseItem = item.Clone();
										// If it has prefix, copy it
										if (item.prefix != 0)
										{
											Main.mouseItem.Prefix((int)item.prefix);
										}
										Main.mouseItem.stack = 0;
									}
									// Add to mouseitem stack
									Main.mouseItem.stack++;
									// Take from slot stack
									item.stack--;
									Main.stackSplit = Main.stackSplit == 0 ? 15 : Main.stackDelay;

									// Reset item
									if (item.stack <= 0)
									{
										item.TurnToAir();
									}
								}
							}
						}
					}

					PostOnRightClick();
				}
			}

			public virtual bool CanTakeItem(Item item) => true;
			public virtual void PostOnRightClick() { }
			public virtual void PostOnClick(UIMouseEvent evt, UIElement e) { }

			private void UIInteractableItemPanel_OnClick(UIMouseEvent evt, UIElement e)
			{
				// Slot has an item
				if (!item.IsAir)
				{
					// Only slot has an item
					if (Main.mouseItem.IsAir)
					{
						Main.playerInventory = true;
						Main.mouseItem = item.Clone();
						item.TurnToAir();
					}
					// Mouse has an item
					// Can take mouse item
					else if (CanTakeItem(Main.mouseItem))
					{
						Main.playerInventory = true;
						// Items are the same type
						if (item.type == Main.mouseItem.type)
						{
							// Attempt increment stack
							var newStack = item.stack + Main.mouseItem.stack;
							// Mouse item stack fits, increment
							if (item.maxStack >= newStack)
							{
								item.stack = newStack;
								Main.mouseItem.TurnToAir();
							}
							// Doesn't fit, set item to maxstack, set mouse item stack to difference
							else
							{
								var stackDiff = newStack - item.maxStack;
								item.stack = item.maxStack;
								Main.mouseItem.stack = stackDiff;
							}
						}
						// Items are not the same type
						else
						{
							// Swap mouse item and slot item
							var tmp = item.Clone();
							var tmp2 = Main.mouseItem.Clone();
							Main.mouseItem = tmp;
							item = tmp2;
						}

					}
				}
				// Slot has no item
				// Slot can take mouse item
				else if (CanTakeItem(Main.mouseItem))
				{
					Main.playerInventory = true;
					item = Main.mouseItem.Clone();
					Main.mouseItem.TurnToAir();
				}

				// PostClick
				PostOnClick(evt, e);
			}
		}

		// Item panel which can display an item
		internal class UIItemPanel : UIPanel
		{
			internal const float panelwidth = 50f;
			internal const float panelheight = 50f;
			internal const float panelpadding = 0f;

			public string HintText { get; set; }
			public Texture2D HintTexture { get; set; }
			public Item item;

			public UIItemPanel(int netID = 0, int stack = 0, Texture2D hintTexture = null, string hintText = null)
			{
				base.Width.Set(panelwidth, 0f);
				base.Height.Set(panelheight, 0f);
				base.SetPadding(panelpadding);
				this.item = new Item();
				this.item.netDefaults(netID);
				this.item.stack = stack;
				this.HintTexture = hintTexture;
				this.HintText = hintText;
			}

			protected override void DrawSelf(SpriteBatch spriteBatch)
			{
				base.DrawSelf(spriteBatch);

				Texture2D texture2D;
				CalculatedStyle innerDimensions = base.GetInnerDimensions();
				Color drawColor;

				if (HintTexture != null
					&& item.IsAir)
				{
					texture2D = HintTexture;
					drawColor = Color.LightGray * 0.5f;
					if (base.IsMouseHovering)
					{
						Main.hoverItemName = HintText ?? string.Empty;
					}
				}
				else if (item.IsAir)
					return;
				else
				{
					texture2D = Main.itemTexture[item.type];
					drawColor = this.item.GetAlpha(Color.White);
					if (base.IsMouseHovering)
					{
						Main.hoverItemName = item.name;
						Main.toolTip = item.Clone();
						Main.toolTip.GetModInfo<DeconItemInfo>(TheDeconstructor.instance).addValueTooltip = true;
						//ItemValue value = new ItemValue().SetFromCopperValue(item.value*item.stack);
						Main.toolTip.name =
							$"{Main.toolTip.name}{Main.toolTip.modItem?.mod.Name.Insert((int)Main.toolTip.modItem?.mod.Name.Length, "]").Insert(0, " [")}";
					}
				}

				var frame =
						!item.IsAir && Main.itemAnimations[item.type] != null
							? Main.itemAnimations[item.type].GetFrame(texture2D)
							: texture2D.Frame(1, 1, 0, 0);

				float drawScale = 1f;
				if ((float)frame.Width > innerDimensions.Width
					|| (float)frame.Height > innerDimensions.Width)
				{
					if (frame.Width > frame.Height)
					{
						drawScale = innerDimensions.Width / (float)frame.Width;
					}
					else
					{
						drawScale = innerDimensions.Width / (float)frame.Height;
					}
				}

				var unreflectedScale = drawScale;
				var tmpcolor = Color.White;
				// 'Breathing' effect
				ItemSlot.GetItemLight(ref tmpcolor, ref drawScale, item.type);

				Vector2 drawPosition = new Vector2(innerDimensions.X, innerDimensions.Y);

				drawPosition.X += (float)innerDimensions.Width * 1f / 2f - (float)frame.Width * drawScale / 2f;
				drawPosition.Y += (float)innerDimensions.Height * 1f / 2f - (float)frame.Height * drawScale / 2f;

				//todo: globalitem?
				if (item.modItem == null
					|| item.modItem.PreDrawInInventory(spriteBatch, drawPosition, frame, drawColor, drawColor, Vector2.Zero, drawScale))
				{
					spriteBatch.Draw(texture2D, drawPosition, new Rectangle?(frame), drawColor, 0f,
						Vector2.Zero, drawScale, SpriteEffects.None, 0f);

					if (this.item?.color != default(Color))
					{
						spriteBatch.Draw(texture2D, drawPosition, new Rectangle?(frame), drawColor, 0f,
							Vector2.Zero, drawScale, SpriteEffects.None, 0f);
					}
				}

				item.modItem?.PostDrawInInventory(spriteBatch, drawPosition, frame, drawColor, drawColor, Vector2.Zero, drawScale);

				// Draw stack count
				if (this.item?.stack > 1)
				{
					Utils.DrawBorderStringFourWay(
						spriteBatch,
						Main.fontItemStack,
						Math.Min(9999, item.stack).ToString(),
						innerDimensions.Position().X + 10f,
						innerDimensions.Position().Y + 26f,
						Color.White,
						Color.Black,
						Vector2.Zero,
						unreflectedScale * 0.8f);
				}
			}

		}

		internal sealed class DogePanel : UIPanel
		{
			public DogePanel(UIElement parent, float offset = 0f)
			{
				base.Width.Set(parent.Width.Pixels - offset, 0f);
				base.Height.Set(parent.Height.Pixels - vpadding / 2f, 0f);
				var suchEmpty = new UIImage(TheDeconstructor.DogeTexture);
				suchEmpty.Width.Set(160f, 0f);
				suchEmpty.Height.Set(160f, 0f);
				suchEmpty.HAlign = 0.5f;
				suchEmpty.VAlign = 0.30f;
				base.Append(suchEmpty);
				var text = new UIText("Wow, such empty", 1f, true);
				text.HAlign = 0.5f;
				text.VAlign = 0.85f;
				base.Append(text);
			}
		}

		internal static class RecipeSearcher
		{
			public static List<Recipe> FindRecipes(Item item)
			{
				return Main.recipe.Where(recipe => !recipe.createItem.IsAir && recipe.createItem.type == item.type && recipe.createItem.stack <= item.stack).ToList();
			}

			public static void FillWithRecipes(Item source, List<Recipe> recipes, ref UIList list, float offset)
			{
				if (!recipes.Any())
				{
					list.Clear();
					list.Initialize();
					list.Add(new DogePanel(list, TheDeconstructor.instance.deconGUI.cubeItemPanel.Width.Pixels));
				}
				else
				{
					foreach (var recipe in recipes)
					{
						// Setup new recipe panel
						var recipePanel = new UIRecipePanel(list.Width.Pixels - 3f * vpadding - offset, 200f);
						recipePanel.Initialize();
						recipePanel.embeddedRecipe = recipe; // set embedded recipe

						// Set item values
						recipePanel.materialsValue = new ItemValue();
						recipePanel.resultValue = new ItemValue().SetFromCopperValue(source.value * source.stack).ToSellValue();
						recipePanel.deconstructValue = new ItemValue();

						var mats = recipePanel.materials;
						var reqItems = recipe.requiredItem;
						// Loop all material slots
						for (int i = 0; i < recipePanel.materials.Count; i++)
						{
							if (reqItems[i].type == 0) break; // no more materials in this recipe

							mats[i].item = reqItems[i].Clone(); // clone material

							// calc stack diff and at it to the matarial stack
							float stackDiff = (float)(source.stack - recipe.createItem.stack) / (float)recipe.createItem.stack;
							mats[i].item.stack += recipePanel.materials[i].item.stack * (int)stackDiff;

							// Add material values
							recipePanel.materialsValue.AddValues(mats[i].item.value * mats[i].item.stack);
						}

						recipePanel.materialsValue.ToSellValue();

						// Values to usse
						int diffValue = (int)Math.Abs(recipePanel.resultValue.RawValue - recipePanel.materialsValue.RawValue);
						int combinedValue = (int)Math.Abs(recipePanel.resultValue.RawValue + recipePanel.materialsValue.RawValue);
						int sourcePrefixValue = (int)(recipePanel.resultValue.RawValue / 3f);
						hoverString = "";

						// Set proper deconstruct value
						if (source.Prefix(-3)) // -3 checks if item is prefixable, -2 forced random prefix (always get one), -1 random prefix
						{
							int useValue = sourcePrefixValue < combinedValue ? combinedValue : sourcePrefixValue;
							recipePanel.deconstructValue.SetFromCopperValue(useValue);
						}
						else if (source.potion || failureTypes.Contains((short)source.type))
						{
							recipePanel.canFail = true;
							recipePanel.deconstructValue.SetFromCopperValue(combinedValue);
							hoverString = "Sealed content might not survive seal destruction!\n";
						}
						else
						{
							recipePanel.deconstructValue.SetFromCopperValue(diffValue);
						}

						if (recipePanel.deconstructValue.RawValue <= 0)
							recipePanel.deconstructValue.SetFromCopperValue(Item.buyPrice(0, 0, 50, 30));
						recipePanel.deconstructValue.ApplyDiscount(Main.LocalPlayer);
						recipePanel.deconstructValue *= 1.2f;
						//recipePanel.resultValue.ApplyDiscount(Main.LocalPlayer).ToSellValue();
						//

						list.Add(recipePanel);
					}
				}

			}
		}
	}
}
