using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cairo;

using DotNetMatrix;

namespace CairoWarp
{
	// See: http://www.codeguru.com/cpp/g-m/gdi/gdi/article.php/c3679/Weird-Warps.htm
	public class WarpBilinear : Warp
	{
		private PointD[] _pntSrc = new PointD[4];
		private GeneralMatrix _mxWarpFactors;

		public WarpBilinear(IEnumerable<PointD> destPoints, Rectangle srcRect)
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

		public WarpBilinear(IEnumerable<PointD> destPoints, IEnumerable<PointD> srcPoints)
		{
			PreCalc(destPoints.ToArray(), srcPoints.ToArray());
		}

		// In bilinear mode, the warping functions are:
		//	x' = a0 + a1 x y + a2 x + a3 y
		//	y' = b0 + b1 x y + b2 x + b3 y
		//
		// Here, we have two sets of four equations. In the first set, the a# factors
		// are the unknowns, in the second set the b# factors.
		// The equations are of the form:
		//	a0		+ xy a1		+ x a2		+ y a3	= x'
		// The left hand side is identical for both sets. The right hand side differs.
		// Therefore, we can solve them in one operation.
		// The left hand side factors are put in the 4x4 matrix mxLeft, the right side
		// factors are put in the 4x2 matrix mxRight.
		// After solving, the first column of m_mxWarpFactors contains a0, a1, a2, a3; the
		// second columne contains b0, b1, b2, b3.
		private void PreCalc(PointD[] destPoints, PointD[] srcPoints)
		{
			var mxLeft = new GeneralMatrix(4, 4);
			var 	mxRight = new GeneralMatrix(4, 2);

			for (int row = 0; row < 4; row++)
			{
				mxLeft.Array[row][0] = 1.0;
				mxLeft.Array[row][1] = srcPoints[row].X * srcPoints[row].Y;
				mxLeft.Array[row][2] = srcPoints[row].X;
				mxLeft.Array[row][3] = srcPoints[row].Y;

				mxRight.Array[row][0] = destPoints[row].X;
				mxRight.Array[row][1] = destPoints[row].Y;
			}

			_mxWarpFactors = mxLeft.Solve(mxRight);
		}

		protected override PointD WarpPoint(PointD point)
		{
			var x = point.X;
			var y = point.Y;
			var xy = x * y;

			var newx = _mxWarpFactors.Array[0][0] + _mxWarpFactors.Array[1][0] * xy + _mxWarpFactors.Array[2][0] * x + _mxWarpFactors.Array[3][0] * y;
			var newy = _mxWarpFactors.Array[0][1] + _mxWarpFactors.Array[1][1] * xy + _mxWarpFactors.Array[2][1] * x + _mxWarpFactors.Array[3][1] * y;

			return new PointD(newx, newy);
		}
	}
}
