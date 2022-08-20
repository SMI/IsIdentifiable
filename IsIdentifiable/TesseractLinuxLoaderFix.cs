using System;
using System.Runtime.InteropServices;
using HarmonyLib;
using InteropDotNet;

namespace IsIdentifiable;

/// <summary>
/// Work around legacy Tesseract Interop code relying on old libdl.so
/// </summary>
public class TesseractLinuxLoaderFix {
    public static void Patch()
    {
        var harmony = new Harmony("uk.ac.dundee.hic.tesseract");
        var ll = typeof(LibraryLoader);
        var self = typeof(TesseractLinuxLoaderFix);
        harmony.Patch(ll.GetMethod("LoadLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(LoadLibraryPatch))));
        harmony.Patch(ll.GetMethod("GetProcAddress"), prefix: new HarmonyMethod(self.GetMethod(nameof(GetProcAddressPatch))));
        harmony.Patch(ll.GetMethod("FreeLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(FreeLibraryPatch))));
    }

    static bool LoadLibraryPatch(string filename, string platform, ref IntPtr __result)
    {
        __result = UnixLoadLibrary(filename,2);
        return true;
    }

    static bool GetProcAddressPatch(IntPtr handle, string name, ref IntPtr __result)
    {
        __result = UnixGetProcAddress(handle, name);
        return true;
    }

    static bool FreeLibraryPatch(string name,ref bool __result)
    {
        __result = UnixFreeLibrary(name) != 0;
        return true;
    }

    // TODO: Patch IntPtr InteropDotNet.LibraryLoader.LoadLibrary(filename,platformname)
    // TODO: Patch IntPtr InteropDotNet.GetProcAddress(handle,name)
    // TODO: Patch bool FreeLibrary(filename)
    // TODO: Record load/unload in LibraryLoader.loadedAssemblies(string->intptr)
    

  [DllImport("c", EntryPoint = "dlopen")]
  private static extern IntPtr UnixLoadLibrary(string fileName, int flags);

  [DllImport("c", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern int UnixFreeLibrary(IntPtr handle);

  [DllImport("c", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr UnixGetProcAddress(IntPtr handle, string symbol);

  [DllImport("c", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr UnixGetLastError();
}