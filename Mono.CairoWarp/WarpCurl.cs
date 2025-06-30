using System;

using Cairo;

namespace CairoWarp
{
	public class WarpCurl : Warp
	{
		private Rectangle _rect;
		private TextExtents _extents;

		public WarpCurl(TextExtents text, Rectangle rect)
		{
			_rect = rect;
			_extents = text;
		}

		protected override PointD WarpPoint(PointD point)
		{
			var textWidth = _extents.Width;
			var xn = point.X - textWidth / 2;
			//var yn = y - Textheight/2;
			var xnew = xn;
			var ynew = Math.Pow(point.Y + xn, 3) / Math.Pow((textWidth / 2), 3) * 50;
			var result = new PointD(xnew + _rect.Width / 2, ynew + _rect.Height * 2 / 5);

			//Console.WriteLine("X:{0} - Y:{1} ==> X:{2} - Y:{3}", point.X, point.Y, result.X, result.Y);

			return result;
		}
	}
}
