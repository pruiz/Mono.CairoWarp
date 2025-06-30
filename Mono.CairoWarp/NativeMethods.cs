#if NET8_0_OR_GREATER
using System;
using System.Reflection;

using Cairo;

namespace CairoWarp
{
	internal static partial class NativeMethods
	{
		#region Fields

		private static readonly Lazy<Type> __funcLoaderType =
			new(() => typeof(Surface).Assembly.GetType("FuncLoader", true));

		/// <summary>
		/// https://github.com/GtkSharp/GtkSharp/blob/b7303616129ab5a0ca64def45649ab522d83fa4a/Source/Libs/Shared/FuncLoader.cs#L110
		/// </summary>
		private static readonly Lazy<MethodInfo> __loadFunctionMethodInfo =
			new(() => __funcLoaderType.Value.GetMethod("LoadFunction", BindingFlags.Public | BindingFlags.Static));

		/// <summary>
		/// https://github.com/GtkSharp/GtkSharp/blob/b7303616129ab5a0ca64def45649ab522d83fa4a/Source/Libs/Shared/FuncLoader.cs#L94
		/// </summary>
		private static readonly Lazy<MethodInfo> __getProcAddressMethodInfo =
			new(() => __funcLoaderType.Value.GetMethod("GetProcAddress", BindingFlags.Public | BindingFlags.Static));

		/// <summary>
		/// https://github.com/GtkSharp/GtkSharp/blob/b7303616129ab5a0ca64def45649ab522d83fa4a/Source/Libs/Shared/GLibrary.cs#L37
		/// </summary>
		private static readonly Lazy<MethodInfo> __gLibraryLoadMethodInfo =
			new(() => typeof(Surface).Assembly.GetType("GLibrary", true).GetMethod("Load", BindingFlags.Public | BindingFlags.Static));

		/// <summary>
		/// https://github.com/GtkSharp/GtkSharp/blob/b7303616129ab5a0ca64def45649ab522d83fa4a/Source/Libs/Shared/Library.cs#L6
		/// </summary>
		private static readonly Lazy<object> _libCairo =
			new(() => Enum.Parse(typeof(Surface).Assembly.GetType("Library", true), "Cairo"));

		#endregion

		public static T LoadFunction<T>(string function)
		{
			var library = __gLibraryLoadMethodInfo.Value.Invoke(null, [_libCairo.Value]);
			var proc = __getProcAddressMethodInfo.Value.Invoke(null, [library, function]);

			return (T)__loadFunctionMethodInfo.Value.MakeGenericMethod(typeof(T)).Invoke(null, [proc]);
		}
	}
}
#endif