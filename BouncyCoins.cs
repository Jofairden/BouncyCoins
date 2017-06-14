using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace BouncyCoins
{
	public class BouncyCoins : Mod
	{
		// (c) jofairden
		// version 0.1.3

		// Use a boolean to check if the appropriate mod is loaded
		public bool LoadedFKTModSettings;

		public static int[] VanillaCoinSet =
		{
			ItemID.CopperCoin, ItemID.SilverCoin, ItemID.GoldCoin, ItemID.PlatinumCoin
		};

		internal static BouncyCoins instance;

		public BouncyCoins()
		{
			Properties = ModProperties.AutoLoadAll;
		}

		public override void Load()
		{
			instance = this;
			Version reqVer = new Version(0, 8, 3, 5);
			if (ModLoader.version < reqVer)
			{
				string message = "\nBouncy Coins uses a functionality only present in tModLoader version " + reqVer.ToString() + " or higher. " +
				                 "Please update tModLoader to use this mod.\n\n";
				throw new Exception(message);
			}
			// Mod Settings support
			LoadedFKTModSettings = ModLoader.GetMod("FKTModSettings") != null;
			if (LoadedFKTModSettings)
			{
				// Needs to be in a method otherwise it throws a namespace error
				try { LoadModSettings(); }
				catch { }
			}
		}

		private void LoadModSettings()
		{
			FKTModSettings.ModSetting setting = FKTModSettings.ModSettingsAPI.CreateModSettingConfig(this);

			// Enable auto-saving between sessions
			setting.EnableAutoConfig();

			setting.AddComment("Here you can configure settings for Bouncy Coins." +
			                   " Please keep in mind these settings will only reflect on your client." +
			                   " Also please keep in my the changes are only visual, to coin locations are never changed." +
			                   " The settings are saved with the player.", 1f);

			setting.AddComment("This setting will control the amplitude of the coin bounce.");
			setting.AddDouble(
				"bounceAmplitude",
				"Bounce amplitude",
				-100d,
				100d,
				true);

			setting.AddComment("This setting will control the amplitude multiplier. Use this for even more finetuning");
			setting.AddDouble(
				"bounceAmpMult",
				"Bounce amplitude multiplier",
				-10d,
				10d,
				true);

			setting.AddComment("Decides the speed of the coin bounce.");
			setting.AddDouble(
				"bounceSpeed",
				"Bounce speed",
				-100f,
				100f,
				true);

			setting.AddComment("Decides the offset of the bounce.");
			setting.AddDouble(
				"bounceOffset",
				"Bounce offset",
				-80f,
				80f,
				true);

			setting.AddComment("Decides whether the coins bounce evenly or individually.");
			setting.AddBool(
				"bounceEvenly", 
				"Bounce coins evenly?", 
				true);

			setting.AddComment("Decides whether the bounce is calculated using cosine or sine.");
			setting.AddBool(
				"bounceByCosine",
				"Bounce coins by cosine (true) or sine (false)?",
				true);

			setting.AddComment("Decides whether modded coins are affected or not.");
			setting.AddBool(
				"bounceModItems",
				"Allow modded coins to bounce?",
				true);
		}

		public override void PostUpdateInput()
		{
			if (LoadedFKTModSettings && !Main.gameMenu)
			{
				// Needs to be in a method otherwise it throws a namespace error
				try
				{
					UpdateModSettings();
				}
				catch (Exception e)
				{
					Main.NewTextMultiline(e.ToString(), c: Color.Red);
				}
			}
		}

		private void UpdateModSettings()
		{
			FKTModSettings.ModSetting setting;
			if (FKTModSettings.ModSettingsAPI.TryGetModSetting(this, out setting))
			{
				CoinPlayer player = CoinPlayer.GetModPlayer(Main.LocalPlayer);
				setting.Get("bounceAmplitude", ref player.amplitude);
				setting.Get("bounceAmpMult", ref player.ampMult);
				setting.Get("bounceSpeed", ref player.speed);
				setting.Get("bounceOffset", ref player.bounceOffset);
				setting.Get("bounceEvenly", ref player.bounceEvenly);
				setting.Get("bounceModItems", ref player.bounceModItems);
				setting.Get("bouncebyCosine", ref player.byCosine);
			}
		}
	}

	public class CoinItem : GlobalItem
	{
		/*
		 * Is it a vanilla coin?
		 */
		internal static bool IsVanillaCoin(int type)
			=> BouncyCoins.VanillaCoinSet.Any(x => x == type);

		/*
		 * Bouncy coin logic here
		 */
		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			CoinPlayer player = CoinPlayer.GetModPlayer(Main.LocalPlayer);

			// Do not bounce if any of these conditions are true
			if ((!player.bounceModItems && item.modItem != null)
				|| !player.bouncyItems.Contains(item.type)
				|| item.velocity.Length() > 0f)
				return true;
			
			// Get coin texture
			Texture2D texture = Main.itemTexture[item.type];

			// Get appropiate keyframe
			var keyFrame = (player.keyFrameActions.ContainsKey(item.type) && player.keyFrameActions[item.type] != null)
				? player.keyFrameActions[item.type].Invoke(item, whoAmI)
				: CoinPlayer.coinKeyFrameAction(item, whoAmI);

			// Logic
			Texture2D animTexture = keyFrame.AnimationTexture;
			rotation = keyFrame.Rotation;
			float offsetY = item.height - texture.Height;
			float offsetX = item.width * 0.5f - texture.Width * 0.5f;
			int frameHeight = keyFrame.FrameHeight;
			// What is our angle for bouncing?
			// If we bounce evenly, all coins bounce based on gametime
			// If not, they will bounce based on their spawntime and whoAmI
			int angle = 
				player.bounceEvenly 
				? (int)Main.time 
				: whoAmI % 60 + item.spawnTime;
			//angle += (int)player.universalOffset;

			Rectangle? frameRect = new Rectangle?(
				new Rectangle(
					0,
					Main.itemFrame[whoAmI] * frameHeight + 1,
					texture.Width,
					frameHeight
				));

			// Do we want to bounce by cosine or sine?
			double bounceByAngle =
				player.byCosine
					? Math.Cos(angle * player.speed)
					: Math.Sin(angle * player.speed);
			bounceByAngle *= player.amplitude * player.ampMult;

			// Drawing
			Vector2 center = new Vector2(0f, frameHeight * 0.5f);
			Vector2 offset = new Vector2(0f, (float)bounceByAngle - (float)player.amplitude + (float)player.bounceOffset);

			Vector2 pos = new Vector2(item.position.X - Main.screenPosition.X + animTexture.Width * 0.5f + offsetX,
							item.position.Y - Main.screenPosition.Y + frameHeight * 0.5f + offsetY)
						  - center + offset;

			Vector2 origin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);

			Main.spriteBatch.Draw(animTexture, pos, frameRect, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
	}

	public class CoinPlayer : ModPlayer
	{
		public static CoinPlayer GetModPlayer(Player player) 
			=> player.GetModPlayer<CoinPlayer>(BouncyCoins.instance);

		public delegate KeyFrameActionResult keyFrameActionDelegate(Item item, int whoAmI);

		internal static keyFrameActionDelegate coinKeyFrameAction 
			=> GenerateBasicKeyFrameActionDelegate(1, 5, 8);

		public class KeyFrameActionResult
		{
			public KeyFrameActionResult(Texture2D text, float rot, int height)
			{
				AnimationTexture = text;
				Rotation = rot;
				FrameHeight = height;
			}

			public Texture2D AnimationTexture { get; protected set; }
			public float Rotation { get; protected set; }
			public int FrameHeight { get; protected set; }
		}

		/// <summary>
		/// Will generate a basic keyframeaction delegate for you, but if it's not a vanilla coin the offset returned will be 0.
		/// </summary>
		/// <param name="frameIncrement">How fast the itemFrameCounter increments</param>
		/// <param name="maxFrameCount">The amount of frames itemFrameCounter will count to</param>
		/// <param name="maxFrames">The maximum amount of frames</param>
		/// <returns></returns>
		public static keyFrameActionDelegate GenerateBasicKeyFrameActionDelegate(int frameIncrement, int maxFrameCount, int maxFrames)
		{
			return delegate (Item item, int whoAmI)
			{
				Main.itemFrameCounter[whoAmI] += frameIncrement;
				if (Main.itemFrameCounter[whoAmI] > maxFrameCount)
				{
					Main.itemFrameCounter[whoAmI] = 0;
					Main.itemFrame[whoAmI] = (Main.itemFrame[whoAmI] + 1) % maxFrames;
				}
				return CoinItem.IsVanillaCoin(item.type) 
				? new KeyFrameActionResult(Main.coinTexture[item.type - 71], item.velocity.X* 0.2f, Main.coinTexture[item.type - 71].Height / maxFrames)
				: new KeyFrameActionResult(Main.itemTexture[item.type], 0f, Main.itemTexture[item.type].Height / maxFrames);
			};
		}

		public void AddBouncyItem(int type, keyFrameActionDelegate keyFrameAction)
		{
			if (bouncyItems.Contains(type)) return;
			bouncyItems.Add(type);
			keyFrameActions.Add(type, keyFrameAction);
		}

		public bool RemoveBouncyItem(int type)
		{
			if (!bouncyItems.Contains(type)) return false;
			bool b = bouncyItems.Remove(type);
			return keyFrameActions.Remove(type) && b;
		}

		internal bool bounceEvenly; // bounce evenly?
		internal bool bounceModItems; // do not bounce moditems?
		internal bool byCosine; // bounce by cosine?
		internal List<int> bouncyItems; // list of items that will bounce
		internal double amplitude; // amp of bounce
		internal double ampMult; // amp of bounce
		internal double speed; // total speed of bounce
		internal double bounceOffset; // some offset added to angle
		internal Dictionary<int, keyFrameActionDelegate> keyFrameActions; // keyframing

		public override void Initialize()
		{
			bounceEvenly = false;
			bounceModItems = true;
			byCosine = true;
			bouncyItems = new List<int>(BouncyCoins.VanillaCoinSet);
			amplitude = 10d;
			ampMult = 1d;
			speed = 0.075d;
			bounceOffset = 5d;
			keyFrameActions = new Dictionary<int, keyFrameActionDelegate>();
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				["bounceEvenly"] = bounceEvenly,
				["bounceModItems"] = bounceModItems,
				["byCosine"] = byCosine,
				["bouncyItems"] = new List<int>(bouncyItems),
				["amplitude"] = amplitude,
				["ampMult"] = ampMult,
				["speed"] = speed,
				["bounceOffset"] = bounceOffset
			};
		}

		public override void Load(TagCompound tag)
		{
			bounceEvenly = tag.Get<bool>("bounceEvenly");
			bounceModItems = tag.Get<bool>("bounceModItems");
			byCosine = tag.Get<bool>("byCosine");
			bouncyItems = new List<int>(tag.GetList<int>("bouncyItems"));
			amplitude = tag.Get<double>("amplitude");
			ampMult = tag.Get<double>("ampMult");
			speed = tag.Get<double>("speed");
			bounceOffset = tag.Get<double>("bounceOffset");
		}
	}
}
