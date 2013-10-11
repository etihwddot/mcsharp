using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Logos.Utility;

namespace MCSharp
{
	public sealed class NbtCompound : Nbt
	{
		public NbtCompound(string name, IEnumerable<Nbt> tags) 
			: base(NbtKind.Compound, name)
		{
			m_tags = tags.ToList().AsReadOnly();
		}

		public ReadOnlyCollection<Nbt> Tags
		{ 
			get { return m_tags; }
		}

		public override string ToString()
		{
			return string.Join("\n", ToStrings());
		}

		private IEnumerable<string> ToStrings()
		{
			yield return base.ToString();

			foreach (string str in Tags.SelectMany(x => x is NbtCompound ?
				((NbtCompound)x).ToStrings() :
				new[] { x.ToString() }))
			{
				yield return "  " + str;
			}
		}

		ReadOnlyCollection<Nbt> m_tags;
	}
}
