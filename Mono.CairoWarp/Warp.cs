using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cairo;

namespace CairoWarp
{
	public abstract class Warp
	{
		public Warp()
		{
		}

		protected abstract PointD WarpPoint(PointD point);

		public void Execute(Context ctx)
		{
			PointD point;
			var first = true;

			using (var mpath = ctx.CopyPath())
			{
				var path = mpath.GetPath();

				for (var i = 0; i < path.num_data; )
				{
					var hdr = path.GetPathHeader(i); //hdr.Dump();

					switch (hdr.type)
					{
						case NativePath.cairo_path_data_type_t.CAIRO_PATH_MOVE_TO:
							if (first)
							{
								ctx.NewPath();
								first = false;
							}
							point = path.GetPathPoint(i + 1);
							ctx.MoveTo(WarpPoint(point));
							break;
						case NativePath.cairo_path_data_type_t.CAIRO_PATH_LINE_TO:
							point = path.GetPathPoint(i + 1);
							ctx.LineTo(WarpPoint(point));
							break;
						case NativePath.cairo_path_data_type_t.CAIRO_PATH_CURVE_TO:
							var p1 = WarpPoint(path.GetPathPoint(i + 1));
							var p2 = WarpPoint(path.GetPathPoint(i + 2));
							var p3 = WarpPoint(path.GetPathPoint(i + 3));
							ctx.CurveTo(p1, p2, p3);
							break;
						case NativePath.cairo_path_data_type_t.CAIRO_PATH_CLOSE_PATH:
							ctx.ClosePath();
							break;
					}

					i += hdr.length;
				}
			}
		}
	}
}
