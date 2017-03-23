using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using TheDeconstructor.Items;


namespace TheDeconstructor
{
    internal class DeconstructorGUI : UIState
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
        internal UIItemCubePanel cubeItemPanel;
        internal UIItemSourcePanel sourceItemPanel;
        internal UIRecipeList recipeList;
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
            closeButton.Left.Set(basePanel.Width.Pixels - closeButton.Width.Pixels * 2f - vpadding * 4f, 0f);
            closeButton.Top.Set(closeButton.Height.Pixels / 2f, 0f);
            basePanel.Append(closeButton);

            cubeItemPanel = new UIItemCubePanel();
            cubeItemPanel.Top.Set(cubeItemPanel.Height.Pixels / 2f + vpadding / 2f, 0f);
            basePanel.Append(cubeItemPanel);

            sourceItemPanel = new UIItemSourcePanel();
            sourceItemPanel.Top.Set(cubeItemPanel.Top.Pixels + cubeItemPanel.Height.Pixels + vpadding / 2f, 0f);
            basePanel.Append(sourceItemPanel);

            var recipeInnerPanel = new UIPanel();
            recipeInnerPanel.Width.Set(basePanel.Width.Pixels - cubeItemPanel.Width.Pixels * 2f - vpadding * 2f, 0f);
            recipeInnerPanel.Height.Set(basePanel.Height.Pixels - cubeItemPanel.Top.Pixels * 2f - vpadding * 3f, 0f);
            recipeInnerPanel.Left.Set(cubeItemPanel.Width.Pixels + vpadding / 2f, 0f);
            recipeInnerPanel.Top.Set(cubeItemPanel.Top.Pixels, 0f);
            basePanel.Append(recipeInnerPanel);

            recipeList = new UIRecipeList(recipeInnerPanel.Width.Pixels, recipeInnerPanel.Height.Pixels);
            recipeList.SetPadding(0f);
            recipeList.Initialize();
            recipeInnerPanel.Append(recipeList);

            recipeScrollbar = new UIScrollbar();
            recipeScrollbar.Height.Set(recipeList.Height.Pixels - 4f * vpadding, 0F);
            recipeScrollbar.Left.Set(recipeList.Width.Pixels - recipeScrollbar.Width.Pixels * 2f - vpadding / 2f, 0f);
            recipeScrollbar.Top.Set(vpadding, 0f);
            recipeList.SetScrollbar(recipeScrollbar);
            recipeList.Append(recipeScrollbar);

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

        public void ToggleUI(bool force = false)
        {
            if (!sourceItemPanel.item.IsAir && (!visible || force))
            {
                //Main.LocalPlayer.GetItem(Main.myPlayer, sourceItemPanel.item.Clone()); // does not seem to generate item text
                Main.LocalPlayer.QuickSpawnItem(sourceItemPanel.item.type, sourceItemPanel.item.stack);
                sourceItemPanel.item.TurnToAir();
                recipeList.Clear();
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var info = Main.LocalPlayer.GetModPlayer<DeconPlayer>(TheDeconstructor.instance);

            if (info.UsedQueerCube) return;

            if (Math.Abs(info.DeconDist.X) > 12f * 16f || Math.Abs(info.DeconDist.Y) > 12f * 16f
                || Main.inputTextEscape || Main.LocalPlayer.dead || Main.gameMenu)
            {
                TheDeconstructor.instance.TryToggleGUI();
            }
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
                    matPanel.displayOnly = true;
                    materials.Add(matPanel);
                }
                UIItemPanel lastPanel = new UIItemPanel();
                lastPanel.Left.Set(0f, 0f);
                lastPanel.Top.Set(2f * lastPanel.Height.Pixels + vpadding, 0f);
                lastPanel.displayOnly = true;
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
                errorText = new UIText("You do not have enough gold!");
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

                        // Give the new bag item
                        Item bagItem = new Item();
                        bagItem.SetDefaults(TheDeconstructor.instance.ItemType<LunarCube>());
                        recipePanel.materials.ForEach(x => items.Add(x.item));
                        LunarCube cube = bagItem.modItem as LunarCube;
                        cube.SealedItems = new List<Item>(items);
                        cube.SealedSource = (Item)guiInst.sourceItemPanel.item.Clone();
                        cube.SealedSource.stack = (int)stackDiff;
                        cube.CanFail = recipePanel.canFail;
                        cube.State = Cube.CubeState.Sealed;
                        cube.item.value = recipePanel.materialsValue.RawValue;
                        Main.LocalPlayer.GetItem(Main.myPlayer, bagItem);

                        // Reset item panel if needed
                        if (guiInst.sourceItemPanel.item.stack <= 0)
                            guiInst.sourceItemPanel.item.SetDefaults(0);

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
                    var parentPanel = (Parent as UIRecipePanel);
                    Main.hoverItemName =
                        $"{hoverString}Click to receive recipe materials in a goodie bag" +
                        $"\nResult worth: {parentPanel?.resultValue}" +
                        $"\nRecipe worth: {parentPanel?.materialsValue}" +
                        $"\nDeconstruction cost: {parentPanel?.deconstructValue}";
                }
            }
        }

        // List holding recipe panels
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

        // Only takes cubes
        internal sealed class UIItemCubePanel : UIInteractableItemPanel
        {
            public UIItemCubePanel(int type = 0, int stack = 0, bool allowsAll = false, IEnumerable<short> allowedItems =null) : base(type, stack, allowsAll, allowedItems)
            {
                this.allowsAll = false;
                this.allowedItems = new short[]
                {
                    (short) TheDeconstructor.instance.ItemType<LunarCube>(),
                    (short) TheDeconstructor.instance.ItemType<QueerLunarCube>()
                }.ToList();
            }
        }

        // Is source panel (item to deconstruct)
        internal sealed class UIItemSourcePanel : UIInteractableItemPanel
        {
            public override void PostOnClick(UIMouseEvent evt, UIElement e)
            {
                DoUpdate();
            }

            public override void PostOnRightClick(UIMouseEvent evt, UIElement e)
            {
                DoUpdate();
            }

            private void DoUpdate()
            {
                TheDeconstructor.instance.deconGUI.recipeList.Clear();

                if (item.IsAir) return;

                currentRecipes = RecipeSearcher.FindRecipes(item);
                RecipeSearcher.FillWithRecipes(item, currentRecipes,
                    TheDeconstructor.instance.deconGUI.recipeList,
                    TheDeconstructor.instance.deconGUI.recipeScrollbar.Width.Pixels);
            }
        }

        // Allows user to put in / take out item
        internal class UIInteractableItemPanel : UIItemPanel
        {
            public bool allowsAll;
            public List<short> allowedItems;

            public UIInteractableItemPanel(int type = 0, int stack = 0, bool allowsAll = true, IEnumerable<short> allowedItems = null) : base(type, stack)
            {
                displayOnly = false; // not just for display
                base.OnClick += UIInteractableItemPanel_OnClick;
                base.OnRightClick += UIInteractableItemPanel_OnRightClick;
                this.allowsAll = allowsAll;
                this.allowedItems = allowedItems?.ToList() ?? null;
            }

            public virtual void PostOnRightClick(UIMouseEvent evt, UIElement e) { }
            public virtual void PostOnClick(UIMouseEvent evt, UIElement e) { }

            private void UIInteractableItemPanel_OnRightClick(UIMouseEvent evt, UIElement e)
            {
                try
                {
                    if (displayOnly) return;
                    if (!item.IsAir)
                    {
                        // Open inventory
                        Main.playerInventory = true;

                        // Handle stack splitting here
                        if (Main.stackSplit <= 1 && item.type != 0 &&
                            (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0))
                        {
                            int num2 = Main.superFastStack + 1;
                            for (int j = 0; j < num2; j++)
                            {
                                if ((Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0) &&
                                    item.stack > 0)
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
                                            Main.mouseItem.Prefix((int) item.prefix);
                                        }
                                        Main.mouseItem.stack = 0;
                                    }
                                    Main.mouseItem.stack++;
                                    item.stack--;
                                    Main.stackSplit = Main.stackSplit == 0 ? 15 : Main.stackDelay;

                                    if (item.stack <= 0)
                                    {
                                        TheDeconstructor.instance.deconGUI.recipeList.Clear();
                                        item.SetDefaults(0);
                                    }
                                }
                            }
                        }
                    }

                    PostOnRightClick(evt, e);
                }
                catch (Exception ex)
                {
                    Main.NewTextMultiline(ex.ToString());
                }
            }

            private void UIInteractableItemPanel_OnClick(UIMouseEvent evt, UIElement e)
            {
                try
                {
                    if (displayOnly
                        || (item.IsAir
                             && !allowsAll
                             && allowedItems != null
                             && !allowedItems.Contains((short)Main.mouseItem.type)))
                        return;

                    var panel = (e as UIItemPanel);
                    if (panel == null)
                        return;

                    Main.playerInventory = true;
                    // Panel has item, cursor has item
                    if (!panel.item.IsAir && !Main.mouseItem.IsAir)
                    {
                        // Attempt a swap
                        if (panel.item.type != Main.mouseItem.type)
                        {
                            var tempItem = Main.mouseItem.Clone();
                            var tempItem2 = panel.item.Clone();
                            panel.item = tempItem;
                            Main.mouseItem = tempItem2;
                        }
                        else
                        // Attempt increment stack
                        {
                            if (panel.item.maxStack <= 1) return;
                            panel.item.stack += Main.mouseItem.stack;
                            Main.mouseItem.TurnToAir();
                        }
                    }
                    // Panel has item
                    else if (!panel.item.IsAir)
                    {
                        Main.mouseItem = panel.item.Clone();
                        panel.item.TurnToAir();
                    }
                    // Mouse has item
                    else if (!Main.mouseItem.IsAir)
                    {
                        panel.item = Main.mouseItem.Clone();
                        Main.mouseItem.TurnToAir();
                    }

                    PostOnClick(evt, e);
                }
                catch (Exception ex)
                {
                    Main.NewTextMultiline(ex.ToString());
                }
            }
        }

        // Item panel which can display an item
        internal class UIItemPanel : UIPanel
        {
            internal const float panelwidth = 50f;
            internal const float panelheight = 50f;
            internal const float panelpadding = 0f;

            public Item item;
            public bool displayOnly;

            public UIItemPanel(int type = 0, int stack = 0)
            {
                base.Width.Set(panelwidth, 0f);
                base.Height.Set(panelheight, 0f);
                base.SetPadding(panelpadding);
                item = new Item();
                item.SetDefaults(type);
                item.stack = stack;
            }

            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                base.DrawSelf(spriteBatch);

                if (item == null || item.type == 0) return;

                if (base.IsMouseHovering)
                {
                    Main.hoverItemName = item.name;
                    Main.toolTip = item.Clone();
                    Main.toolTip.GetModInfo<DeconItemInfo>(TheDeconstructor.instance).addValueTooltip = true;
                    //ItemValue value = new ItemValue().SetFromCopperValue(item.value*item.stack);
                    Main.toolTip.name = $"{Main.toolTip.name}{Main.toolTip.modItem?.mod.Name.Insert((int)Main.toolTip.modItem?.mod.Name.Length, "]").Insert(0, " [")}";
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

                // Draw stack count
                if (this.item.stack > 1)
                {
                    spriteBatch.DrawString(Main.fontItemStack, Math.Min(9999, item.stack).ToString(), new Vector2(innerDimensions.Position().X + 10f * drawScale, innerDimensions.Position().Y + 26f * drawScale), Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
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
                        hoverString = "Deconstructing has a chance of destroying recipe materials!\n";
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
