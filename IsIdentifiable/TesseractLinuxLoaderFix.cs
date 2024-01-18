using InteropDotNet;
using System;
using System.Runtime.InteropServices;

namespace IsIdentifiable;

/// <summary>
/// Work around legacy Tesseract Interop code
/// </summary>
public static class TesseractLinuxLoaderFix
{
    /// <summary>
    /// Override .so search path used on Linux by Tesseract
    /// </summary>
    public static void Patch()
    {
        // Only apply patch on Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            LibraryLoader.Instance.CustomSearchPath = $"{AppDomain.CurrentDomain.BaseDirectory}/runtimes";
    }
}
