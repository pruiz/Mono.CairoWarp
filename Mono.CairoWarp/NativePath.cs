using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

using Cairo;

namespace CairoWarp
{
	public static class NativePath
	{
		private static Int32 _point_sz = Marshal.SizeOf(typeof(cairo_path_data_points_t));
		private static FieldInfo _handleField = typeof(Cairo.Path).GetField("handle", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

		#region Native structs
		public enum cairo_path_data_type_t
		{
			CAIRO_PATH_MOVE_TO,
			CAIRO_PATH_LINE_TO,
			CAIRO_PATH_CURVE_TO,
			CAIRO_PATH_CLOSE_PATH
		};

		public struct cairo_path_data_header_t
		{
			public cairo_path_data_type_t type;
			public Int32 length;
		}

		public struct cairo_path_data_points_t
		{
			public double X;
			public double Y;
		}

		public struct cairo_path_t
		{
			public Status status;
			public IntPtr data;
			public int num_data;
		}
		#endregion

		public static cairo_path_data_header_t GetPathHeader(this cairo_path_t path, int offset)
		{
			var hdr_ptr = new IntPtr(path.data.ToInt32() + (_point_sz * offset));
			return (cairo_path_data_header_t)Marshal.PtrToStructure(hdr_ptr, typeof(cairo_path_data_header_t));
		}

		public static PointD GetPathPoint(this cairo_path_t path, int offset)
		{
			var ptr = new IntPtr(path.data.ToInt32() + (_point_sz * offset));
			var points = (cairo_path_data_points_t)Marshal.PtrToStructure(ptr, typeof(cairo_path_data_points_t));

			return new PointD(points.X, points.Y);
		}

		public static cairo_path_t GetPath(this Path path)
		{
			var handle = (IntPtr)_handleField.GetValue(path);
			return (cairo_path_t)Marshal.PtrToStructure(handle, typeof(cairo_path_t));
		}

	}
}
