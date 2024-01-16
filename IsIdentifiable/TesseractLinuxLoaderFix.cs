using HarmonyLib;
using InteropDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace IsIdentifiable;

/// <summary>
/// Work around legacy Tesseract Interop code relying on old libdl.so
/// In an ideal world we'd just implement ILibraryLoaderLogic and pass
/// this to LibraryLoader - but that's locked down as 'internal', so
/// we have to jump through some reflection hoops to get there.
/// </summary>
public sealed partial class TesseractLinuxLoaderFix
{
    private static Dictionary<string, IntPtr>? _loadedAssemblies;

    /// <summary>
    /// Install the patch, if running on Linux (NOP on other platforms)
    /// </summary>
    public static void Patch()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return; // Only apply patch on Linux

        var harmony = new Harmony("uk.ac.dundee.hic.tesseract");
        var ll = typeof(LibraryLoader);
        var self = typeof(TesseractLinuxLoaderFix);
        _loadedAssemblies = ll.GetField("loadedAssemblies", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(LibraryLoader.Instance) as Dictionary<string, IntPtr>;
        harmony.Patch(ll.GetMethod("LoadLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(LoadLibraryPatch), BindingFlags.NonPublic | BindingFlags.Static)));
        harmony.Patch(ll.GetMethod("GetProcAddress"), prefix: new HarmonyMethod(self.GetMethod(nameof(GetProcAddressPatch), BindingFlags.NonPublic | BindingFlags.Static)));
        harmony.Patch(ll.GetMethod("FreeLibrary"), prefix: new HarmonyMethod(self.GetMethod(nameof(FreeLibraryPatch), BindingFlags.NonPublic | BindingFlags.Static)));
    }

    // NOTE(rkm 2023-06-26) Parameter name required for Harmony LibraryLoader
    // ReSharper disable once InconsistentNaming
    private static bool LoadLibraryPatch(string fileName, string platformName, ref IntPtr __result)
    {
        var fullPath = $"{AppDomain.CurrentDomain.BaseDirectory}/runtimes/linux-x64/native/lib{fileName}.so";
        Console.WriteLine($"Attempting to load {fullPath}{(File.Exists(fullPath) ? " (present)" : " (MISSING)")}");
        try
        {
            __result = UnixLoadLibrary(fullPath, 2);
        }
        catch (EntryPointNotFoundException)
        {
            __result = DlLoadLibrary(fullPath, 2);
        }

        if (__result == IntPtr.Zero)
            throw new DllNotFoundException($"Failed to load '{fullPath}' - {(File.Exists(fullPath) ? "exists but could not load" : "file missing")}");

        _loadedAssemblies?.Add(fileName, __result);
        return false;
    }

    // ReSharper disable once InconsistentNaming
    private static bool GetProcAddressPatch(IntPtr dllHandle, string name, ref IntPtr __result)
    {
        try
        {
            __result = UnixGetProcAddress(dllHandle, name);
        }
        catch (EntryPointNotFoundException)
        {
            __result = DlGetProcAddress(dllHandle, name);
        }

        if (__result == IntPtr.Zero)
            Console.Error.WriteLine($"WARN:Failed looking for function '{name}' in module at address {dllHandle}");
        return false;
    }

    // ReSharper disable once InconsistentNaming - Harmony API requirement
    private static bool FreeLibraryPatch(string fileName, ref bool __result)
    {
        if (_loadedAssemblies?.Remove(fileName, out var handle) == true)
        {
            try
            {
                __result = UnixFreeLibrary(handle) != 0;
            }
            catch (EntryPointNotFoundException)
            {
                __result = DlFreeLibrary(handle) != 0;
            }
        }
        else
        {
            __result = false;
            Console.Error.WriteLine($"WARNING:Ignoring attempt to unload unknown library {fileName}");
        }

        return false;
    }


    [LibraryImport("dl", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    private static partial IntPtr DlLoadLibrary(string fileName, int flags);

    [LibraryImport("dl", EntryPoint = "dlclose")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial int DlFreeLibrary(IntPtr handle);

    [LibraryImport("dl", EntryPoint = "dlsym", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial IntPtr DlGetProcAddress(IntPtr handle, string symbol);

    [LibraryImport("dl", EntryPoint = "dlerror")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial IntPtr DlGetLastError();

    [LibraryImport("c", EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    private static partial IntPtr UnixLoadLibrary(string fileName, int flags);

    [LibraryImport("c", EntryPoint = "dlclose")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial int UnixFreeLibrary(IntPtr handle);

    [LibraryImport("c", EntryPoint = "dlsym", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial IntPtr UnixGetProcAddress(IntPtr handle, string symbol);

    [LibraryImport("c", EntryPoint = "dlerror")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial IntPtr UnixGetLastError();
}
