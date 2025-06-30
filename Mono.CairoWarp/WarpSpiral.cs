using System;

using Cairo;

namespace CairoWarp
{
	public class WarpSpiral : Warp
	{
		private Rectangle _rect;

		public WarpSpiral(Rectangle rect)
		{
			_rect = rect;
		}

		protected override PointD WarpPoint(PointD point)
		{
			var width = _rect.Width;
			var height = _rect.Height;
			var theta0 = -Math.PI * 3 / 4;
			var theta = point.X / width * Math.PI * 2 + theta0;
			var radius = point.Y + 200 - point.X / 7;
			var xnew = radius * Math.Cos(theta);
			var ynew = radius * Math.Sin(-theta);
			var result = new PointD(xnew + width / 2, ynew + height / 2);

			//Console.WriteLine("X:{0} - Y:{1} ==> X:{2} - Y:{3}", point.X, point.Y, result.X, result.Y);

			return result;
		}
	}
}
