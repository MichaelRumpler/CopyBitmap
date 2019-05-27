using System.Runtime.InteropServices;

using size_t = System.IntPtr;
using voidptr_t = System.IntPtr;
using sk_bitmap_t = System.IntPtr;

namespace CopyBitmap
{
	internal static unsafe class SkiaApi
	{
#if __TVOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __WATCHOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __IOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __ANDROID__
		private const string SKIA = "libSkiaSharp.so";
#elif __MACOS__
		private const string SKIA = "libSkiaSharp.dylib";
#elif __DESKTOP__
		private const string SKIA = "libSkiaSharp";
#elif WINDOWS_UWP
		private const string SKIA = "libSkiaSharp.dll";
#elif NET_STANDARD
		private const string SKIA = "libSkiaSharp";
#else
		private const string SKIA = "libSkiaSharp";
#endif

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static voidptr_t sk_bitmap_get_pixels(sk_bitmap_t b, out size_t length);


		// throws a System.Runtime.InteropServices.MarshalDirectiveException: Cannot marshal 'return value': Invalid managed/unmanaged type combination.
		// how can I cast a IntPtr to ref byte?
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sk_bitmap_get_pixels")]
		public extern static ref byte sk_bitmap_get_pixels_ref(sk_bitmap_t b, out size_t length);
	}
}