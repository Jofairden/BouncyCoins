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

namespace TheDeconstructor.UI
{
	//internal sealed class DeconEntityInstance : DeconInstance
	//{
	//	public Item sourceItem;
	//	public Item cubeItem;

	//	public DeconEntityInstance(int id, int? player = null, Item source = null, Item cube = null) : base(id, player)
	//	{
	//		sourceItem = source ?? new Item();
	//		cubeItem = cube ?? new Item();
	//	}
	//}

	//internal abstract class DeconInstance
	//{
	//	public bool justUpdated = false;
	//	public int? requestedPlayerID;
	//	public int? ID;

	//	protected DeconInstance(int id, int? player)
	//	{
	//		ID = id;
	//		requestedPlayerID = player;
	//	}
	//}

	internal sealed class DeconstructorGUI : UIState
	{
		//internal Dictionary<int, DeconEntityInstance> TEInstances = new Dictionary<int, DeconEntityInstance>();
		//internal int? currentInstance = null;
		internal Point16? currentTEPosition = null;
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
		internal FixedUIScrollbar recipeScrollbar;

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
			basePanel.OnMouseUp += BasePanel_OnMouseUp;
			basePanel.OnMouseDown += BasePanel_OnMouseDown;
			basePanel.CopyStyle(this);
			basePanel.SetPadding(vpadding);
			_UIView.Append(basePanel);

			baseTitle = new UIText("Deconstructor", 0.85f, true);
			basePanel.Append(baseTitle);

			closeButton = new UIImageButton(TheDeconstructor.instance.GetTexture("closeButton"));
			closeButton.OnClick += CloseButton_OnClick;
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
			recipeList.Width.Set(400, 0f);
			recipeList.Height.Set(basePanel.Height.Pixels - cubeItemPanel.Top.Pixels * 2f - vpadding * 3f, 0f);
			recipeList.Left.Set(cubeItemPanel.Width.Pixels + vpadding / 2f, 0f);
			recipeList.Top.Set(cubeItemPanel.Top.Pixels, 0f);

			recipeList.SetPadding(0);
			recipeList.Initialize();
			basePanel.Append(recipeList);

			recipeScrollbar = new FixedUIScrollbar(TheDeconstructor.instance.deconUI);
			recipeScrollbar.Height.Set(recipeList.Height.Pixels - vpadding * 1.5f, 0F);
			recipeScrollbar.Left.Set(-vpadding * 3f, 1f);
			recipeScrollbar.Top.Set(closeButton.Top.Pixels + closeButton.Height.Pixels + vpadding / 2f, 0f);
			recipeList.SetScrollbar(recipeScrollbar);
			basePanel.Append(recipeScrollbar);

			sourceItemPanel.DoUpdate();
		}

		private void CloseButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			TheDeconstructor.instance.TryToggleGUI(false);
		}

		private void BasePanel_OnMouseUp(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target != recipeScrollbar)
			{
				_Recalculate(evt.MousePosition);
				dragging = false;
			}
		}

		private void BasePanel_OnMouseDown(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target != recipeScrollbar)
			{
				offset = new Vector2(evt.MousePosition.X - _UIView.Left.Pixels, evt.MousePosition.Y - _UIView.Top.Pixels);
				dragging = true;
			}
		}

		public void _Recalculate(Vector2 mousePos, float precent = 0f)
		{
			_UIView.Left.Set(Math.Max(-vpadding * 2f, Math.Min(mousePos.X - offset.X, Main.screenWidth - basePanel.Width.Pixels + vpadding * 2f)), precent);
			_UIView.Top.Set(Math.Max(-vpadding * 2f, Math.Min(mousePos.Y - offset.Y, Main.screenHeight - basePanel.Height.Pixels + vpadding * 2f)), precent);
			Recalculate();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (visible)
			{
				Vector2 mousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);

				if (_UIView.ContainsPoint(mousePosition))
				{
					Main.LocalPlayer.mouseInterface = true;
				}

				if (dragging)
				{
					_Recalculate(mousePosition);
				}
			}
		}

		//protected override void DrawChildren(SpriteBatch spriteBatch)
		//{
		//	base.DrawChildren(spriteBatch);
		//	Rectangle hitbox = GetInnerDimensions().ToRectangle();
		//	//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Red * 0.6f);

		//	//hitbox = basePanel.GetInnerDimensions().ToRectangle();
		//	////hitbox.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);
		//	//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.LightCyan * 0.6f);

		//	hitbox = baseTitle.GetOuterDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.AliceBlue * 0.6f);

		//	hitbox = cubeItemPanel.GetOuterDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Yellow * 0.6f);

		//	hitbox = sourceItemPanel.GetOuterDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Yellow * 0.6f);

		//	hitbox = recipeList.GetOuterDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Beige * 0.3f);

		//	hitbox = recipeScrollbar.GetOuterDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Red * 0.6f);
		//}

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
				Main.LocalPlayer.QuickSpawnClonedItem(sourceItemPanel.item, sourceItemPanel.item.stack);
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
				Main.LocalPlayer.QuickSpawnClonedItem(cubeItemPanel.item, cubeItemPanel.item.stack);
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

			//if (currentInstance != null)
			//{
			//	var instance = TEInstances[currentInstance.Value];
			//	if (!instance.justUpdated)
			//	{
			//		bool b = false;
			//		if (sourceItemPanel.item != instance.sourceItem)
			//		{
			//			instance.sourceItem = sourceItemPanel.item.Clone();
			//			b = true;
			//		}
			//		if (cubeItemPanel.item != instance.cubeItem)
			//		{
			//			instance.cubeItem = cubeItemPanel.item.Clone();
			//			b = true;
			//		}

			//		instance.justUpdated = b;
			//	}
			//	else
			//		instance.justUpdated = false;

			//	sourceItemPanel.BindItem(instance);
			//	cubeItemPanel.BindItem(instance);
			//}

			if (currentTEPosition == null)
				return;

			// Get tile entity from tile data (top left 0, 0 frame of tile)
			var TE = TileEntity.ByPosition[currentTEPosition.Value] as DeconstructorTE;
			// Close UI if too far from tile
			if (
				Math.Abs(TE.playerDistances[Main.myPlayer].X) > 12f * 16f
				|| Math.Abs(TE.playerDistances[Main.myPlayer].Y) > 12f * 16f
				|| Main.LocalPlayer.dead || Main.gameMenu)
			{
				//TE.isCurrentlyActive = false;
				//TE.player = -1;
				TheDeconstructor.instance.TryToggleGUI(false);
			}
		}

		internal static class RecipeSearcher
		{
			public static List<Recipe> FindRecipes(Item item)
			{
				return Main.recipe.Where(recipe => !recipe.createItem.IsAir && recipe.createItem.type == item.type && recipe.createItem.stack <= item.stack).ToList();
			}

			public static void FillWithRecipes(Item source)
			{
				if (!currentRecipes.Any())
				{
					TheDeconstructor.instance.deconGUI.recipeList.Clear();
					TheDeconstructor.instance.deconGUI.recipeList.Initialize();
					TheDeconstructor.instance.deconGUI.recipeList.Add(new DogePanel(TheDeconstructor.instance.deconGUI.recipeList));
				}
				else
				{
					foreach (var recipe in currentRecipes)
					{
						// Setup new recipe panel
						var recipePanel = new UIRecipePanel(TheDeconstructor.instance.deconGUI.recipeList.Width.Pixels, 200f);
						recipePanel.Initialize();
						recipePanel.embeddedRecipe = recipe; // set embedded recipe

						// Set item values
						recipePanel.materialsValue = new ItemValue();
						recipePanel.resultValue = new ItemValue().SetFromCopperValue(source.value * source.stack).ToSellValue();
						recipePanel.deconstructValue = new ItemValue();

						int totalStack = source.stack;
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
							totalStack += mats[i].item.stack;

							// Add material values
							recipePanel.materialsValue.AddValues(mats[i].item.value * mats[i].item.stack);
						}

						recipePanel.materialsValue.ToSellValue();

						// Values to use
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
							hoverString = "Sealed content might be lost when unsealing!\n";
						}
						else
						{
							recipePanel.deconstructValue.SetFromCopperValue((int)(diffValue * 1.2f));
						}

						if (recipePanel.deconstructValue.RawValue <= 0)
							recipePanel.deconstructValue.SetFromCopperValue(totalStack);
						recipePanel.deconstructValue.ApplyDiscount(Main.LocalPlayer);
						TheDeconstructor.instance.deconGUI.recipeList.Add(recipePanel);
					}
				}

			}
		}
	}
}
