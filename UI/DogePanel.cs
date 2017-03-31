using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;


namespace TheDeconstructor.UI
{
	internal sealed class DogePanel : UIPanel
	{
		public DogePanel(UIElement parent)
		{
			base.Width.Set(parent.Width.Pixels, 0f);
			base.Height.Set(parent.Height.Pixels - DeconstructorGUI.vpadding/2f, 0f);
			var suchEmpty = new UIImage(TheDeconstructor.DogeTexture)
			{
				HAlign = 0.5f,
				VAlign = 0.30f
			};
			suchEmpty.Width.Set(160f, 0f);
			suchEmpty.Height.Set(160f, 0f);
			base.Append(suchEmpty);
			var text = new UIText("Wow, such empty", 1f, true)
			{
				HAlign = 0.5f,
				VAlign = 0.85f
			};
			base.Append(text);
		}
	}

}
