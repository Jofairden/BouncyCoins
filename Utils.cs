using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace TheDeconstructor
{
	public static class Utils
	{
		public interface TagSerializable
		{
			void SerializeData(TagCompound tag);
		}

		public interface ITagDeserializer<out T> where T : TagSerializable
		{
			T Deserialize(TagCompound tag);
		}

		public sealed class TagDeserializer<T> : ITagDeserializer<T> where T : TagSerializable
		{
			private readonly Func<TagCompound, T> @delegate;

			public TagDeserializer(Func<TagCompound, T> @delegate)
			{
				this.@delegate = @delegate;
			}

			public T Deserialize(TagCompound tag)
			{
				return @delegate(tag);
			}
		}

		public static class TagSerializables
		{
			public static TagCompound Serialize(TagSerializable obj)
			{
				TagCompound tag = new TagCompound();
				tag["type"] = obj.GetType().FullName;

				TagCompound dataTag = new TagCompound();
				obj.SerializeData(dataTag);
				if (dataTag.Count != 0)
					tag["data"] = dataTag;
				return tag;
			}

			public static T Deserialize<T>(TagCompound tag) where T : TagSerializable
			{
				if (tag == null)
					return default(T);

				Type type = GetType(tag.GetString("type"));
				if (type == null)
					throw new TypeUnloadedException();

				FieldInfo deserializerField = type.GetField("DESERIALIZER");
				if (deserializerField == null)
					throw new Exception(string.Format("Missing deserializer for type {0}.", type.FullName));

				ITagDeserializer<T> deserializer = (ITagDeserializer<T>)deserializerField.GetValue(null);
				TagCompound dataTag = tag.HasTag("data") ? tag.GetCompound("data") : null;
				return deserializer.Deserialize(dataTag);
			}

			private static Type GetType(string name)
			{
				Type type = Type.GetType(name);
				if (type != null)
					return type;

				foreach (Mod mod in ModLoader.LoadedMods)
				{
					type = mod.Code.GetType(name);
					if (type != null)
						return type;
				}

				return null;
			}
		}
	}
}
