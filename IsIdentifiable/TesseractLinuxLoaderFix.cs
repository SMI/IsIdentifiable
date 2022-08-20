using System;
using System.Runtime.InteropServices;
using HarmonyLib;
using InteropDotNet;

namespace IsIdentifiable;

/// <summary>
/// Work around legacy Tesseract Interop code relying on old libdl.so
/// </summary>
public class TesseractLinuxLoaderFix {
    // TODO: Record load/unload in LibraryLoader.loadedAssemblies(string->intptr)
    public static void Patch()
    {
	if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		return; // Only apply patch on Linux
        var harmony = new Harmony("uk.ac.dundee.hic.tesseract");
        var ll = typeof(LibraryLoader);
        var self = typeof(TesseractLinuxLoaderFix);
        harmony.Patch(ll.GetMethod("LoadLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(LoadLibraryPatch))));
        harmony.Patch(ll.GetMethod("GetProcAddress"), prefix: new HarmonyMethod(self.GetMethod(nameof(GetProcAddressPatch))));
        harmony.Patch(ll.GetMethod("FreeLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(FreeLibraryPatch))));
    }

    public static bool LoadLibraryPatch(string fileName, string platformName, ref IntPtr __result)
    {
        __result = UnixLoadLibrary($"{AppDomain.CurrentDomain.BaseDirectory}/x64/lib{fileName}.so",2);
	Console.Error.WriteLine($"Loading library '{AppDomain.CurrentDomain.BaseDirectory}/x64/lib{fileName}.so', result {__result}");
        return false;
    }

    public static bool GetProcAddressPatch(IntPtr dllHandle, string name, ref IntPtr __result)
    {
        __result = UnixGetProcAddress(dllHandle, name);
	if (__result==IntPtr.Zero)
		Console.Error.WriteLine($"WARN:Failed looking for function '{name}' in module at address {dllHandle}");
        return false;
    }

    public static bool FreeLibraryPatch(string fileName,ref bool __result)
    {
        __result = true; // TODO: UnixFreeLibrary(handle) != 0;
        Console.Error.WriteLine($"TODO: Ignoring attempt to unload '{fileName}'");
	return false;
    }

    

  [DllImport("c", EntryPoint = "dlopen")]
  private static extern IntPtr UnixLoadLibrary(string fileName, int flags);

  [DllImport("c", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern int UnixFreeLibrary(IntPtr handle);

  [DllImport("c", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr UnixGetProcAddress(IntPtr handle, string symbol);

  [DllImport("c", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr UnixGetLastError();
}
