using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Cairo;

namespace CairoWarp
{
	public class CairoStreamReader
	{
		private static Type[] _ctorTypes = new[] { typeof(IntPtr), typeof(bool) };
		private static ConstructorInfo _ctor = typeof(ImageSurface).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, _ctorTypes, null);

		private BinaryReader _reader;
		private byte[] _data;
		private int offset;

		private CairoStreamReader(byte[] data)
		{
			if (data == null) throw new ArgumentNullException("data");

			_data = data;
		}

		private CairoStreamReader(BinaryReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			_reader = reader;
		}

		private Status do_read(IntPtr closure, IntPtr outbuf, uint length)
		{
			//Console.WriteLine("closure = {0} - lenght: {1}", closure, length);

			if (_data != null)
			{
				Marshal.Copy(_data, offset, outbuf, (int)length);
				offset += (int)length;
			}
			else
			{
				var tmp = _reader.ReadBytes((int)length);
				Marshal.Copy(tmp, 0, outbuf, (int)length);
			}

			return Status.Success;
		}

		public static ImageSurface ImageSurfaceFromPng(byte[] data)
		{
			var obj = new CairoStreamReader(data);
			var fn = new cairo_read_func_t(obj.do_read);
			var surface = cairo_image_surface_create_from_png_stream(fn, IntPtr.Zero);
			return (ImageSurface)_ctor.Invoke(new object[] { surface, false });
		}

		public static ImageSurface ImageSurfaceFromPng(BinaryReader reader)
		{
			var obj = new CairoStreamReader(reader);
			var fn = new cairo_read_func_t(obj.do_read);
			var surface = cairo_image_surface_create_from_png_stream(fn, IntPtr.Zero);
			return (ImageSurface)_ctor.Invoke(new object[] { surface, false });
		}

		public static ImageSurface ImageSurfaceFromPng(Stream stream)
		{
			return ImageSurfaceFromPng(new BinaryReader(stream));
		}

		#region Native Calls

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status cairo_read_func_t(IntPtr closure, IntPtr data, uint length);

#if NET40
		[DllImport("libcairo-2.dll")]
		private static extern IntPtr cairo_image_surface_create_from_png_stream(
			[MarshalAs(UnmanagedType.FunctionPtr)] cairo_read_func_t cb,
			IntPtr closure
		);
#else
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr d_cairo_image_surface_create_from_png_stream(cairo_read_func_t read_func, IntPtr closure);
		internal static d_cairo_image_surface_create_from_png_stream cairo_image_surface_create_from_png_stream =
			NativeMethods.LoadFunction<d_cairo_image_surface_create_from_png_stream>("cairo_image_surface_create_from_png_stream");
#endif
		#endregion
	}
}
