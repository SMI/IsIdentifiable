using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HarmonyLib;
using InteropDotNet;

namespace IsIdentifiable;

/// <summary>
/// Work around legacy Tesseract Interop code relying on old libdl.so
/// </summary>
public class TesseractLinuxLoaderFix {
  private static Dictionary<string, IntPtr> loadedAssemblies;

  public static void Patch()
  {
    if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		  return; // Only apply patch on Linux
      var harmony = new Harmony("uk.ac.dundee.hic.tesseract");
      var ll = typeof(LibraryLoader);
      var self = typeof(TesseractLinuxLoaderFix);
      loadedAssemblies=ll.GetField("loadedAssemblies").GetValue(LibraryLoader.Instance) as Dictionary<string, IntPtr>;
      harmony.Patch(ll.GetMethod("LoadLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(LoadLibraryPatch))));
      harmony.Patch(ll.GetMethod("GetProcAddress"), prefix: new HarmonyMethod(self.GetMethod(nameof(GetProcAddressPatch))));
      harmony.Patch(ll.GetMethod("FreeLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(FreeLibraryPatch))));
  }

  public static bool LoadLibraryPatch(string fileName, string platformName, ref IntPtr __result)
  {
    try {
      __result = UnixLoadLibrary($"{AppDomain.CurrentDomain.BaseDirectory}/x64/lib{fileName}.so",2);
    } catch (EntryPointNotFoundException e) {
      __result = DLLoadLibrary($"{AppDomain.CurrentDomain.BaseDirectory}/x64/lib{fileName}.so",2);
    }
    if (__result!=IntPtr.Zero)
      loadedAssemblies.Add(fileName,__result);
    Console.Error.WriteLine($"Loading library '{AppDomain.CurrentDomain.BaseDirectory}/x64/lib{fileName}.so', result {__result}");
    return false;
  }

  public static bool GetProcAddressPatch(IntPtr dllHandle, string name, ref IntPtr __result)
  {
    try {
      __result = UnixGetProcAddress(dllHandle, name);
    } catch (EntryPointNotFoundException e) {
      __result = DLGetProcAddress(dllHandle, name);
    }
    if (__result==IntPtr.Zero)
      Console.Error.WriteLine($"WARN:Failed looking for function '{name}' in module at address {dllHandle}");
    return false;
  }

  public static bool FreeLibraryPatch(string fileName,ref bool __result)
  {
    if (loadedAssemblies.Remove(fileName,out var handle))
      try {
        __result=UnixFreeLibrary(handle)!=0;
      } catch (EntryPointNotFoundException e) {
        __result=DLFreeLibrary(handle)!=0;
      }
    else {
      __result=false;
      Console.Error.WriteLine($"WARNING:Ignoring attempt to unload unknown library {fileName}");
    }
    return false;
  }


  [DllImport("dl", EntryPoint = "dlopen")]
  private static extern IntPtr DLLoadLibrary(string fileName, int flags);

  [DllImport("dl", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern int DLFreeLibrary(IntPtr handle);

  [DllImport("dl", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr DLGetProcAddress(IntPtr handle, string symbol);

  [DllImport("dl", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr DLGetLastError();

  [DllImport("c", EntryPoint = "dlopen")]
  private static extern IntPtr UnixLoadLibrary(string fileName, int flags);

  [DllImport("c", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern int UnixFreeLibrary(IntPtr handle);

  [DllImport("c", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr UnixGetProcAddress(IntPtr handle, string symbol);

  [DllImport("c", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private static extern IntPtr UnixGetLastError();
}
