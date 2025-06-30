using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Cairo;

namespace CairoWarp
{
	public class CairoStreamWriter
	{
		private static Type[] _ctorTypes = new[] { typeof(IntPtr), typeof(bool) };
		private static ConstructorInfo _ctor = typeof(ImageSurface).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, _ctorTypes, null);

		private BinaryWriter _writer;

		private CairoStreamWriter(Stream stream)
			: this(new BinaryWriter(stream))
		{
		}

		private CairoStreamWriter(BinaryWriter writer)
		{
			if (writer == null) throw new ArgumentNullException("writer");

			_writer = writer;
		}

		private Status do_write(IntPtr closure, IntPtr inbuf, uint length)
		{
			//Console.WriteLine("closure = {0} - lenght: {1}", closure, length);


			var tmp = new byte[(int)length];
			Marshal.Copy(inbuf, tmp, 0, (int)length);
			_writer.Write(tmp);

			return Status.Success;
		}

		public static void SurfaceToPngStream(Surface surface, BinaryWriter writer)
		{
			var obj = new CairoStreamWriter(writer);
			var fn = new cairo_write_func_t(obj.do_write);
			var status = cairo_surface_write_to_png_stream(surface.Handle, fn, IntPtr.Zero);

			if (status != Status.Success)
				throw new InvalidOperationException("Status: " + status);
		}

		public static void SurfaceToPngStream(Surface surface, Stream stream)
		{
			SurfaceToPngStream(surface, new BinaryWriter(stream));
		}

		public static byte[] SurfaceToPngBytes(Surface surface)
		{
			using (var ms = new MemoryStream())
			{
				SurfaceToPngStream(surface, ms);
				return ms.ToArray();
			}
		}

		#region Native Calls

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status cairo_write_func_t(IntPtr closure, IntPtr data, uint length);

#if NET40
		[DllImport("libcairo-2.dll")]
		private static extern Status cairo_surface_write_to_png_stream(
			IntPtr surface,
			[MarshalAs(UnmanagedType.FunctionPtr)] cairo_write_func_t cb,
			IntPtr closure
		);
#else
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate Status d_cairo_surface_write_to_png_stream(IntPtr surface, cairo_write_func_t write_func, IntPtr closure);
		internal static d_cairo_surface_write_to_png_stream cairo_surface_write_to_png_stream =
			NativeMethods.LoadFunction<d_cairo_surface_write_to_png_stream>("cairo_surface_write_to_png_stream");
#endif
		#endregion
	}
}
