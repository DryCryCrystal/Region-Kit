using System;
using System.IO;
using RWCustom;
using UnityEngine;
using RegionKit.POM;

namespace RegionKit.Objects {
    public class Drawable : CosmeticSprite {
        public static PlacedObjectsManager.ManagedField[] Fields = {
            new PlacedObjectsManager.Vector2ArrayField("quad", 4, true, PlacedObjectsManager.Vector2ArrayField.Vector2ArrayRepresentationType.Polygon, Vector2.zero, Vector2.right * 20f, (Vector2.right + Vector2.up) * 20f, Vector2.up * 20f),
            new PlacedObjectsManager.StringField("spriteName", "Futile_White", "Decal Name"),
            new PlacedObjectsManager.FloatField("depth", 0f, 1f, 1f, displayName: "Depth"),
            new PlacedObjectsManager.StringField("shader", "Basic", "Shader"),
            new PlacedObjectsManager.EnumField("container", typeof(FContainer), FContainer.Foreground, displayName: "FContainer"),
            new PlacedObjectsManager.IntegerField("alpha", 1, 255, 255, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Alpha"),
            new PlacedObjectsManager.BooleanField("useColour", false, displayName: "Use Colour"),
            new PlacedObjectsManager.ColorField("colour", Color.white, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Colour")
        };

        public Drawable(PlacedObject pObj, Room room) {
            this.room = room;
            LocalPlacedObject = pObj;
        }

        public enum FContainer {
            Shadows,
            BackgroundShortcuts,
            Background,
            Midground,
            Items,
            Foreground,
            ForegroundLights,
            Shortcuts,
            Water,
            GrabShaders,
            Bloom,
            HUD,
            HUD2
        }

        public PlacedObjectsManager.ManagedData Data => LocalPlacedObject.data as PlacedObjectsManager.ManagedData;

        public Vector2 PlacedObjectTile => LocalPlacedObject.pos;


        public PlacedObject LocalPlacedObject { get; }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            base.InitiateSprites(sLeaser, rCam);
            TriangleMesh.Triangle[] triangles = new TriangleMesh.Triangle[2];
            triangles[0] = new TriangleMesh.Triangle(0, 1, 2);
            triangles[1] = new TriangleMesh.Triangle(1, 2, 3);
            TriangleMesh mesh = new TriangleMesh("Futile_White", triangles, true) {
                UVvertices = {
                    [0] = new Vector2(0, 0),
                    [1] = new Vector2(1, 0),
                    [2] = new Vector2(0, 1),
                    [3] = new Vector2(1, 1)
                }
            };
            sLeaser.sprites = new FSprite[] { mesh };
        }

        public Vector2[] Quad => new[] { Data.GetValue<Vector2[]>("quad")[0], Data.GetValue<Vector2[]>("quad")[1], Data.GetValue<Vector2[]>("quad")[3], Data.GetValue<Vector2[]>("quad")[2] };

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].alpha = Data.GetValue<int>("alpha") / 255f;
            rCam.ReturnFContainer(Data.GetValue<FContainer>("container").ToString())
                .AddChildAtIndex(sLeaser.sprites[0],
                    Mathf.FloorToInt(
                        Data.GetValue<float>("depth") *
                        rCam.ReturnFContainer(Data.GetValue<FContainer>("container").ToString())
                            .GetChildCount()));
            try {
                sLeaser.sprites[0].SetElementByName(Data.GetValue<string>("spriteName"));
            }
            catch (FutileException) {
                try {
                    WWW www = new WWW(string.Concat("file:///", Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Decals", Path.DirectorySeparatorChar, Data.GetValue<string>("spriteName"), ".png"));
                    Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false) {
                        wrapMode = TextureWrapMode.Clamp,
                        anisoLevel = 0,
                        filterMode = FilterMode.Point
                    };
                    www.LoadImageIntoTexture(tex);
                    HeavyTexturesCache.LoadAndCacheAtlasFromTexture(Data.GetValue<string>("spriteName"), tex);
                    sLeaser.sprites[0].SetElementByName(Data.GetValue<string>("spriteName"));
                }
                catch (Exception e) when (e is FutileException) {
                    //ignored
                }
                catch (Exception e) when (e is IOException) {
                    //ignored
                }
            }

            for (int i = 0; i < 4; i++) {
                ((TriangleMesh)sLeaser.sprites[0]).MoveVertice(i, PlacedObjectTile + Quad[i] - camPos);
            }

            if (rCam.game.rainWorld.Shaders.ContainsKey(Data.GetValue<string>("shader"))) {
                sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[Data.GetValue<string>("shader")];
            }

            var col = Data.GetValue<Color>("colour");
            col.a = Data.GetValue<int>("alpha") / 255f;
            sLeaser.sprites[0].color = Data.GetValue<bool>("useColour") ? col : new Color(Color.white.r, Color.white.g, Color.white.b, Data.GetValue<int>("alpha") / 255f);
        }

        public static void Register() => PlacedObjectsManager.RegisterFullyManagedObjectType(Fields, typeof(Drawable), "FreeformDecalOrSprite");
    }
}