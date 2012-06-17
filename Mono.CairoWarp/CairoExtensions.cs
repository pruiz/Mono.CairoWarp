using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cairo;

namespace CairoWarp
{
	internal static class CairoExtensions
	{
		public static double GetLeft(this Rectangle rect)
		{
			return rect.X;
		}

		public static double GetRight(this Rectangle rect)
		{
			return (rect.X + rect.Width);
		}

		public static double GetBottom(this Rectangle rect)
		{
			return (rect.Y + rect.Height);
		}

		public static double GetTop(this Rectangle rect)
		{
			return rect.Y;
		}
	}
}
