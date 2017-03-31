using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;


namespace TheDeconstructor.UI
{
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
				matPanel.Left.Set((matPanel.Width.Pixels + DeconstructorGUI.vpadding / 2f) * (i % 7), 0f);
				matPanel.Top.Set(i < 7 ? 0f : matPanel.Height.Pixels + DeconstructorGUI.vpadding / 2f, 0f);
				materials.Add(matPanel);
			}
			UIItemPanel lastPanel = new UIItemPanel();
			lastPanel.Left.Set(0f, 0f);
			lastPanel.Top.Set(2f * lastPanel.Height.Pixels + DeconstructorGUI.vpadding, 0f);
			materials.Add(lastPanel);

			base.Width.Set(width, 0f);
			base.Height.Set(height, 0f);
			base.Left.Set(left, 0f);
			base.Top.Set(top, 0f);

			recipeBag = new UIRecipeBag(TheDeconstructor.instance.GetTexture("DeconstructBagItem")) { Parent = this };
			recipeBag.Width.Set(30f, 0f);
			recipeBag.Height.Set(40f, 0f);
			recipeBag.Top.Set(lastPanel.Top.Pixels + recipeBag.Height.Pixels / 4f, 0f);
			recipeBag.Left.Set(lastPanel.Width.Pixels + DeconstructorGUI.vpadding, 0f);
			base.Append(recipeBag);

			errorTime = 2;
			errorText = new UIText(" ");
			errorText.Width.Set(25f, 0f);
			errorText.Height.Set(25f, 0f);
			errorText.Top.Set(recipeBag.Top.Pixels + Main.fontMouseText.MeasureString(errorText.Text).Y / 2f, 0f);
			errorText.Left.Set(recipeBag.Left.Pixels + recipeBag.Width.Pixels + DeconstructorGUI.vpadding, 0f);
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
}
