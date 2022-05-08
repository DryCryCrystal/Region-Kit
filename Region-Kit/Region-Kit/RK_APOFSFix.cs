using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.IO;
using UnityEngine;
using RegionKit.Utils;


//Made by Slime_Cubed and Doggo
public static class RK_APOFSFix
{
    public static EventInfo redirectHooks;

    private static MethodInfo apofs = apofs = typeof(SaveState).GetMethod("AbstractPhysicalObjectFromString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
    public static event On.SaveState.hook_AbstractPhysicalObjectFromString On_SaveState_AbstractPhysicalObjectFromString
    {
        add
        {
            if (CountPatches() >= 2)
            {
                if (redirectHooks == null)
                {
                    PetrifiedWood.WriteLine($"{nameof(RK_APOFSFix)} using custom hook manager from {Assembly.GetExecutingAssembly().FullName}");
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            Type t = asm.GetType(nameof(RK_APOFSFix), false);
                            if (t == null) continue;
                            FieldInfo rh = t.GetField(nameof(redirectHooks));
                            if (rh == null) continue;
                            rh.SetValue(null, typeof(RK_APOFSFix).GetEvent(nameof(On_SaveState_AbstractPhysicalObjectFromString)));
                        }
                        catch (Exception) { }
                    }
                }

                if (redirectHooks.DeclaringType != typeof(RK_APOFSFix))
                    redirectHooks.AddEventHandler(null, value);

                if (apofsDetour == null)
                {
                    PetrifiedWood.WriteLine("Creating native detour for SaveState.AbstractPhysicalObjectFromString...");
                    apofsDetour = new NativeDetour(apofs, typeof(RK_APOFSFix).GetMethod(nameof(Hook_AbstractPhysicalObjectFromString)));
                }

                APOFSHook hk = new APOFSHook(value);
                if (baseApofsHook != null)
                    baseApofsHook.last = hk;
                hk.next = baseApofsHook;
                baseApofsHook = hk;
            }
            else
            {
                // A plain hook can be used when no issue is found
                On.SaveState.AbstractPhysicalObjectFromString += value;
            }
        }
        remove
        {
            if (CountPatches() >= 2)
            {
                if (redirectHooks.DeclaringType != typeof(RK_APOFSFix))
                    redirectHooks.RemoveEventHandler(null, value);

                APOFSHook hk = baseApofsHook;
                while (hk != null)
                {
                    if (hk.target == value)
                    {
                        hk.Remove();
                        break;
                    }
                    hk = hk.next;
                }
            }
            else
            {
                On.SaveState.AbstractPhysicalObjectFromString -= value;
            }
        }
    }

    // Act as a detour manager
    private static NativeDetour apofsDetour;
    private static APOFSHook baseApofsHook;

    public static AbstractPhysicalObject Hook_AbstractPhysicalObjectFromString(World world, string str)
    {
        if (baseApofsHook == null) return Orig_AbstractPhysicalObjectFromString(world, str);

        return baseApofsHook.Invoke(world, str);
    }

    public static AbstractPhysicalObject Orig_AbstractPhysicalObjectFromString(World world, string str)
    {
        // Generating a trampoline may cause a segfault
        apofsDetour.Undo();
        AbstractPhysicalObject ret = SaveState.AbstractPhysicalObjectFromString(world, str);
        apofsDetour.Apply();
        return ret;
    }

    private class APOFSHook
    {
        public On.SaveState.hook_AbstractPhysicalObjectFromString target;
        public APOFSHook next;
        public APOFSHook last;

        public APOFSHook(On.SaveState.hook_AbstractPhysicalObjectFromString target)
        {
            this.target = target;
        }

        public AbstractPhysicalObject Invoke(World world, string str)
        {
            // Find the next function down the chain
            On.SaveState.orig_AbstractPhysicalObjectFromString orig;
            if (next == null) orig = Orig_AbstractPhysicalObjectFromString;
            else orig = next.Invoke;

            // Invoke
            return target(orig, world, str);
        }

        public void Remove()
        {
            if (last == null)
                baseApofsHook = next;
            else
                last.next = next;

            if (next != null)
                next.last = last;
        }
    }

    public static int patchesApplied = -1;
    public static int CountPatches()
    {
        if (patchesApplied >= 0) return patchesApplied;

        // Check if any other assemblies have performed this check
        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly asm in asms)
        {
            // Don't throw
            try
            {
                Type t = asm.GetType(nameof(RK_APOFSFix), false);
                if (t == null) continue;
                FieldInfo pa = t.GetField(nameof(patchesApplied), BindingFlags.Public | BindingFlags.Static);
                if (pa.GetValue(null) is int otherPatchesApplied && otherPatchesApplied >= 0)
                    return patchesApplied = otherPatchesApplied;
            }
            catch (Exception) { }
        }

        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // This bug only happens in Partiality
        if (Path.GetFileName(dir) != "Mods")
            return patchesApplied = 0;

        // Count the number of patches enabled
        patchesApplied = 0;
        string[] metas = Directory.GetFiles(dir, "*.modMeta");

        for (int i = 0; i < metas.Length; i++)
        {
            bool patch = false;
            bool enabled = false;
            string[] lines = File.ReadAllLines(Path.Combine(dir, metas[i]));
            // Check whether this mod is a patch and is enabled
            for (int line = 0; line < lines.Length; line++)
            {
                if (lines[line] == "isEnabled: true") enabled = true;
                else if (lines[line] == "isPatch: true") patch = true;
                else continue;
                if (enabled && patch)
                {
                    patchesApplied++;
                    break;
                }
            }
        }

        return patchesApplied;
    }
}