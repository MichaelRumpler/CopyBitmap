I have a big `SKBitmap` which I need to scroll up and down.
I do this by copying a part of the bitmap to a different position within itself.
But when the source and target rectangles overlap, then this does not work with `SKCanvas.DrawBitmap`.
That method always starts to copy at the beginning.
If the target is below the start, then it already overwrote those pixels with those from the start
once it reaches it. As a result the area between source and target is repeated all over until the end.

As a workaround I wrote several methods which also copy those pixels, but check for overlaps.
If the source and target rectangles overlap and the target is below the start,
then it has to copy from the end to the start.

To choose the best method, I needed benchmarks and that's what this repository is all about.

First a short explanation of the various methods I tested and how they work.

|   Method | Main functionality | Unsafe |
|----------|-------------|---------|
| SkiaCopy | `SKCanvas.DrawBitmap` | No |
| ManualCopy | Manually iterate over the pixels and copy them as uint (4 bytes at a time) | Yes |
| BufferCopy | `Buffer.MemoryCopy` | Yes |
| UnsafeCopy | `Unsafe.CopyBlockUnaligned` | Yes |
| SpanCopy | `Span<byte>.CopyTo` | No |
| SpanUIntCopy | `Span<uint>.CopyTo` | No |
| GenericSpanCopy | `Span<uint>.CopyTo` | No |

I tested each of those methods with four bitmap sizes.

SkiaCopyDown is the fastest, but unfortunately this is also the one with the bug so we cannot use it.
SkiaCopyUp is 2-3x slower. I guess that's because the memory is already cached when SkiaCopyDown copies
the same memory again and SkiaCopyUp cannot use the cache.
The big surprise was, that the other methods (except the manual copying) were all faster than SkiaCopyUp.
I thought SkiaSharp would use the GPU and be faster than my code which copied with the CPU.
Maybe I have to try `WglContext` as was explained in [this issue](https://github.com/mono/SkiaSharp/issues/745).

I currently write this code for .NET Core WPF, but the goal is to also use it on iOS and Android later.


    BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17763.503 (1809/October2018Update/Redstone5)
    Intel Core i7-4790 CPU 3.60GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
    .NET Core SDK=3.0.100-preview5-011568
      [Host]     : .NET Core 3.0.0-preview5-27626-15 (CoreCLR 4.6.27622.75, CoreFX 4.700.19.22408), 64bit RyuJIT
      Job-GWNIZA : .NET Core 3.0.0-preview5-27626-15 (CoreCLR 4.6.27622.75, CoreFX 4.700.19.22408), 64bit RyuJIT DEBUG

    BuildConfiguration=Debug  Toolchain=.NET Core 3.0

|              Method |         Bitmap |         Mean |      Error |     StdDev |       Median |
|-------------------- |--------------- |-------------:|-----------:|-----------:|-------------:|
|        SkiaCopyDown |   SmallAligned |     4.908 us |  0.0371 us |  0.0329 us |     4.902 us |
|          SkiaCopyUp |   SmallAligned |    10.476 us |  0.1237 us |  0.1157 us |    10.422 us |
|      ManualCopyDown |   SmallAligned |    21.009 us |  0.0442 us |  0.0369 us |    20.998 us |
|        ManualCopyUp |   SmallAligned |    20.821 us |  0.0808 us |  0.0756 us |    20.828 us |
|      BufferCopyDown |   SmallAligned |     3.376 us |  0.0080 us |  0.0071 us |     3.378 us |
|        BufferCopyUp |   SmallAligned |     3.017 us |  0.0082 us |  0.0073 us |     3.016 us |
|      UnsafeCopyDown |   SmallAligned |     3.009 us |  0.0286 us |  0.0268 us |     3.000 us |
|        UnsafeCopyUp |   SmallAligned |     3.327 us |  0.0080 us |  0.0067 us |     3.325 us |
|        SpanCopyDown |   SmallAligned |     3.415 us |  0.0119 us |  0.0100 us |     3.418 us |
|          SpanCopyUp |   SmallAligned |     2.958 us |  0.0336 us |  0.0314 us |     2.940 us |
|    SpanUIntCopyDown |   SmallAligned |     3.239 us |  0.0345 us |  0.0322 us |     3.229 us |
|      SpanUIntCopyUp |   SmallAligned |     2.789 us |  0.0138 us |  0.0129 us |     2.790 us |
| GenericSpanCopyDown |   SmallAligned |     3.342 us |  0.0796 us |  0.1007 us |     3.346 us |
|   GenericSpanCopyUp |   SmallAligned |     2.821 us |  0.0571 us |  0.0679 us |     2.788 us |
|        SkiaCopyDown | SmallUnaligned |     3.612 us |  0.0223 us |  0.0197 us |     3.616 us |
|          SkiaCopyUp | SmallUnaligned |     9.471 us |  0.0970 us |  0.0907 us |     9.494 us |
|      ManualCopyDown | SmallUnaligned |    20.891 us |  0.1667 us |  0.1559 us |    20.844 us |
|        ManualCopyUp | SmallUnaligned |    21.093 us |  0.4546 us |  0.7470 us |    20.718 us |
|      BufferCopyDown | SmallUnaligned |     3.413 us |  0.0166 us |  0.0156 us |     3.416 us |
|        BufferCopyUp | SmallUnaligned |     2.902 us |  0.0366 us |  0.0343 us |     2.902 us |
|      UnsafeCopyDown | SmallUnaligned |     3.169 us |  0.0159 us |  0.0141 us |     3.166 us |
|        UnsafeCopyUp | SmallUnaligned |     3.781 us |  0.1723 us |  0.5079 us |     3.503 us |
|        SpanCopyDown | SmallUnaligned |     3.441 us |  0.0108 us |  0.0101 us |     3.438 us |
|          SpanCopyUp | SmallUnaligned |     2.939 us |  0.0076 us |  0.0068 us |     2.939 us |
|    SpanUIntCopyDown | SmallUnaligned |     3.221 us |  0.0241 us |  0.0201 us |     3.214 us |
|      SpanUIntCopyUp | SmallUnaligned |     2.796 us |  0.0186 us |  0.0174 us |     2.791 us |
| GenericSpanCopyDown | SmallUnaligned |     3.237 us |  0.0126 us |  0.0105 us |     3.237 us |
|   GenericSpanCopyUp | SmallUnaligned |     2.812 us |  0.0079 us |  0.0070 us |     2.812 us |
|        SkiaCopyDown |     BigAligned |   299.259 us |  3.5699 us |  3.3393 us |   298.503 us |
|          SkiaCopyUp |     BigAligned | 1,033.395 us |  4.1271 us |  3.8604 us | 1,034.089 us |
|      ManualCopyDown |     BigAligned | 4,760.555 us | 13.4351 us | 12.5672 us | 4,756.548 us |
|        ManualCopyUp |     BigAligned | 4,757.071 us | 20.3521 us | 19.0373 us | 4,761.822 us |
|      BufferCopyDown |     BigAligned |   602.108 us | 12.0335 us | 15.6469 us |   605.725 us |
|        BufferCopyUp |     BigAligned |   437.092 us |  3.8328 us |  3.5852 us |   438.279 us |
|      UnsafeCopyDown |     BigAligned |   563.782 us |  7.2468 us |  6.0514 us |   562.621 us |
|        UnsafeCopyUp |     BigAligned |   458.057 us |  8.4470 us |  7.4880 us |   457.258 us |
|        SpanCopyDown |     BigAligned |   558.054 us |  6.3051 us |  5.8978 us |   558.202 us |
|          SpanCopyUp |     BigAligned |   448.735 us |  4.8929 us |  4.5768 us |   448.256 us |
|    SpanUIntCopyDown |     BigAligned |   603.250 us |  7.2002 us |  6.3828 us |   604.168 us |
|      SpanUIntCopyUp |     BigAligned |   425.036 us |  8.7149 us |  8.5592 us |   423.739 us |
| GenericSpanCopyDown |     BigAligned |   522.114 us |  3.9506 us |  3.6954 us |   521.185 us |
|   GenericSpanCopyUp |     BigAligned |   422.335 us |  3.5259 us |  3.1256 us |   421.268 us |
|        SkiaCopyDown |   BigUnaligned |   288.895 us |  2.3055 us |  2.1566 us |   288.946 us |
|          SkiaCopyUp |   BigUnaligned | 1,015.885 us |  2.2037 us |  2.0614 us | 1,015.795 us |
|      ManualCopyDown |   BigUnaligned | 4,725.147 us | 12.2954 us | 10.8996 us | 4,722.299 us |
|        ManualCopyUp |   BigUnaligned | 4,732.498 us | 10.4943 us |  9.3029 us | 4,733.028 us |
|      BufferCopyDown |   BigUnaligned |   529.409 us |  6.2030 us |  5.1798 us |   530.856 us |
|        BufferCopyUp |   BigUnaligned |   427.576 us |  2.4786 us |  2.3185 us |   428.167 us |
|      UnsafeCopyDown |   BigUnaligned |   857.725 us |  8.7404 us |  8.1758 us |   859.711 us |
|        UnsafeCopyUp |   BigUnaligned |   420.193 us |  4.1418 us |  3.8742 us |   421.488 us |
|        SpanCopyDown |   BigUnaligned |   513.901 us |  4.4968 us |  3.9863 us |   514.295 us |
|          SpanCopyUp |   BigUnaligned |   406.438 us |  3.5717 us |  3.3410 us |   405.529 us |
|    SpanUIntCopyDown |   BigUnaligned |   525.433 us |  6.4331 us |  6.0175 us |   526.007 us |
|      SpanUIntCopyUp |   BigUnaligned |   455.637 us |  9.0382 us | 20.9473 us |   467.463 us |
| GenericSpanCopyDown |   BigUnaligned |   522.548 us | 10.2592 us | 11.8145 us |   520.438 us |
|   GenericSpanCopyUp |   BigUnaligned |   452.831 us |  9.0563 us | 24.6382 us |   465.673 us |
