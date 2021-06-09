using Partiality.Modloader;
//TODO0(DELTATIME): Make logging that can be used for entire project
namespace RegionKit {
    public class RegionKit : PartialityMod {

        public const string modVersion = "1.0.0";
        public const string buildVersion = "61"; //Increments for every code change without a version change.

        public RegionKit() {
            ModID = "RegionKit";
            author = "Substratum Dev Team & More";
            Version = modVersion;
        }

        public override void OnEnable() {
            base.OnEnable();
            RoomLoader.Patch();
            BrokenPatch.Patch();
            CustomArenaDivisions.Patch();
            //Add new things here - remember to add them to OnDisable() as well!

            // Use this to enable the example managedobjecttypes for testing or debugging
            //ManagedObjectExamples.PlacedObjectsExample();
        }

        public override void OnDisable() {
            base.OnDisable();
            RoomLoader.Disable();
            BrokenPatch.Disable();
            //Add new things here- remember to add them to OnEnable() as well!
        }

        // Code for AutoUpdate support --------------------
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/7/0";
        // Version - increase this by 1 when you upload a new version of the mod.
        public int version = 7;
        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "pQTSWONMkz/+cDljDGQPVe33mzBTAjabsB8++ZF7h+5rx65KSpvqviESF8X6tKFZPQBxaQD+JwLK05kSt9lopcUsLe8T+Vxia4HXDnEGmAMuZg477vpib+JCgKP0pAMjwtLiD8GpvbI3kUcxD8qJ3+l6ULCTbT8Z120U2lae22AzMU5Tpz0Yvl/vATv3472roBYe7N9LA5mFaACPT+E+U36/hSoIhtVmIxtbOXCmCod/k4L3/CPDs4w34gb1Vo43GiLLo9jOSXVPhMTMkWHrYWnEWy4tu9Ujcj0KZcuHGylO6MYfV+dSJwdAgkcFuq4plNRHt+pmAnwbI0U5kcd2FlpkI2ihqVShvDyj4v3mFNmd/0YighTcBXmYQj3h06NKup9cPfNCPRdwP9CTNjtLljA+SWkl7z/j+z29lWFuE6a1xNiYZb+GGj4UbExUDgcZ1YFOqSgPeQPeoFGqY5nGBQN0UOv/9GmrdaxxWGDrgkbRL2+L/NwKV2uH8HVBzSu0VBRnTjz3JTzkKTBR+ai0LDfmez7BBvG8giTvgNrHi3LxxvVGUugj9GnbRxnTSY6By8JKSwKkgPztVb+irUPW+1lQv76Gyx6fh/8V/+EbrgKSVUEH/mF/Yg8MDQseRbF7X697ZNcfiHm/dGjV+zNcR8CL0Tvtj2mqdNPH4Eib/qE=";
        // ------------------------------------------------
    }
}
