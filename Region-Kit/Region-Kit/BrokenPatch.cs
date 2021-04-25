namespace RegionKit {
    class BrokenPatch {
        public static void Patch() {
            On.SuperStructureFuses.ctor += SuperStructureFuses_ctor;
        }

        public static void Disable() {
            On.SuperStructureFuses.ctor -= SuperStructureFuses_ctor;
        }

        private static void SuperStructureFuses_ctor(On.SuperStructureFuses.orig_ctor orig, SuperStructureFuses self, PlacedObject placedObject, RWCustom.IntRect rect, Room room) {
            self.placedObject = placedObject;
            self.pos = placedObject.pos;
            self.rect = rect;
            self.lights = new float[rect.Width * 2, rect.Height * 2, 5];
            self.depth = 0;
            for (int i = rect.left; i <= rect.right; i++) {
                for (int j = rect.bottom; j <= rect.top; j++) {
                    if (!room.GetTile(i, j).Solid && ((!room.GetTile(i, j).wallbehind) ? 2 : 1) > self.depth) {
                        self.depth = ((!room.GetTile(i, j).wallbehind) ? 2 : 1);
                    }
                }
            }
            self.broken = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.CorruptionSpores);
            if (room.world.region == null || room.world.region.name != "SS" && room.world.region.name != "UW") {
                self.broken = 1f;
            }
            self.gravityDependent = (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f);
            self.power = 1f;
            self.powerFlicker = 1f;
        }
    }
}
