using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cairo;

using DotNetMatrix;

namespace CairoWarp
{
	// See: http://www.codeguru.com/cpp/g-m/gdi/gdi/article.php/c3679/Weird-Warps.htm
	public class WarpPerspective : Warp
	{
		private const double epsilon = 1.0-18;
		private PointD[] _pntSrc = new PointD[4];
		private GeneralMatrix _mxWarpFactors;

		public WarpPerspective(IEnumerable<PointD> destPoints, Rectangle srcRect)
		{
			if (destPoints == null || destPoints.Count() != 4)
				throw new ArgumentException("destPoints");

			if (srcRect == null)
				throw new ArgumentNullException("srcRect");

			_pntSrc[0].X = _pntSrc[2].X = srcRect.GetLeft();
			_pntSrc[1].X = _pntSrc[3].X = srcRect.GetRight();
			_pntSrc[0].Y = _pntSrc[1].Y = srcRect.GetTop();
			_pntSrc[2].Y = _pntSrc[3].Y = srcRect.GetBottom();

			PreCalc(destPoints.ToArray(), _pntSrc);
		}

		public WarpPerspective(IEnumerable<PointD> destPoints, IEnumerable<PointD> srcPoints)
		{
			PreCalc(destPoints.ToArray(), srcPoints.ToArray());
		}

		// In perspective mode, the warping functions are:
		//	x' = (a0 + a1 x + a2 y) / (c0 x + c1 y + 1)
		//	y' = (b0 + b1 x + b2 y) / (c0 x + c1 y + 1)
		//
		// The following calculates the factors a#, b# and c#.
		// We do this by creating a set of eight equations with a#, b# and c# as unknowns.
		// The equations are derived by:
		// 1. substituting the srcPoints for (x, y);
		// 2. substituting the corresponding destPoints for (x', y');
		// 3. solving the resulting set of equations, with the factors as unknowns.
		//
		// The equations are like these:
		//	a0	x a1	y a2	0		0		0		-xx'c0	-yx'c1	= x'
		//	0	0		0		b0		x b1	y b2	-xy'c0	-yy'c1  = y'
		// The known factors of left hand side ar put in the 8x8 matrix mxLeft for
		// all four point pairs, and the right hand side in the one column matrix mxRight.
		// After solving, m_mxWarpFactors contains a0, a1, a2, b0, b1, b2, c0, c1.
		private void PreCalc(PointD[] destPoints, PointD[] srcPoints)
		{
			var mxLeft = new GeneralMatrix(8, 8); //mxLeft.Null();
			var 	mxRight = new GeneralMatrix(8, 1);

			var row = 0;

			for (int i = 0; i < 4; i++)
			{
				mxLeft.Array[row][0] = 1.0;
				mxLeft.Array[row][1] = srcPoints[i].X;
				mxLeft.Array[row][2] = srcPoints[i].Y;

				mxLeft.Array[row][6] = - srcPoints[i].X * destPoints[i].X;
				mxLeft.Array[row][7] = - srcPoints[i].Y * destPoints[i].X;

				mxRight.Array[row][0] = destPoints[i].X;

				row++;

				mxLeft.Array[row][3] = 1.0f;
				mxLeft.Array[row][4] = srcPoints[i].X;
				mxLeft.Array[row][5] = srcPoints[i].Y;

				mxLeft.Array[row][6] = - srcPoints[i].X * destPoints[i].Y;
				mxLeft.Array[row][7] = - srcPoints[i].Y * destPoints[i].Y;

				mxRight.Array[row][0] = destPoints[i].Y;

				row++;
			}

			_mxWarpFactors = mxLeft.Solve(mxRight);
		}

		protected override PointD WarpPoint(PointD point)
		{
			var x = point.X;
			var y = point.Y;
			var num = _mxWarpFactors.Array[6][0] * x + _mxWarpFactors.Array[7][0] * y + 1.0;

			if (Math.Abs(num) < epsilon) throw new OverflowException();

			var newx = (_mxWarpFactors.Array[0][0] + _mxWarpFactors.Array[1][0] * x + _mxWarpFactors.Array[2][0] * y) / num;
			var newy = (_mxWarpFactors.Array[3][0] + _mxWarpFactors.Array[4][0] * x + _mxWarpFactors.Array[5][0] * y) / num;

			return new PointD(newx, newy);
		}
	}
}
