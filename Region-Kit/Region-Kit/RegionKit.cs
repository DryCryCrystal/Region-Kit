using BepInEx;

namespace RegionKit {
    [BepInPlugin("Deltatime", "RegionKit", "1.0.0")]
    class RegionKit : BaseUnityPlugin {
        RegionKit() {
            //On hooks
            RoomLoader.Patch();
        }
    }
}
