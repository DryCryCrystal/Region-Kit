﻿using UnityEngine;
using RegionKit.POM;
using static RegionKit.POM.PlacedObjectsManager;

namespace RegionKit.Objects {
    public class ColouredLightSource : UpdatableAndDeletable {
        public PlacedObject LocalPlacedObject;
        public LightSource LightSource;
        public PlacedObjectsManager.ManagedData Data;
        public bool flickering;

        private static readonly PlacedObjectsManager.ManagedField[] Fields = {
            new PlacedObjectsManager.ColorField("lightCol", Color.white, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Light Colour"),
            new PlacedObjectsManager.Vector2Field("radius", Vector2.up, PlacedObjectsManager.Vector2Field.VectorReprType.circle),
            new PlacedObjectsManager.FloatField("alphaChannel", 0f, 1f, 1f, displayName: "Alpha"),
            new PlacedObjectsManager.BooleanField("flatLight", false, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.button, "Flat"),
            new PlacedObjectsManager.FloatField("paletteDarkness", 0f, 1f, 0.5f, displayName: "Darkness Effect"),
            new PlacedObjectsManager.FloatField("flickIntensity", 0f, 1f, 0f, displayName: "Flicker Intensity"),
            new PlacedObjectsManager.FloatField("threshold", 0f, 1f, 0.5f, displayName: "Flicker Threshold")
        };

        public ColouredLightSource(PlacedObject pObj, Room room) {
            this.room = room;
            LocalPlacedObject = pObj;
            Data = pObj.data as PlacedObjectsManager.ManagedData;

            LightSource = new LightSource(LocalPlacedObject.pos, false, Data?.GetValue<Color>("lightCol") ?? Color.white, this);
            LightSource.affectedByPaletteDarkness = Data?.GetValue<float>("paletteDarkness") ?? 0.5f;
            room.AddObject(LightSource);
        }

        public override void Update(bool eu) {
            base.Update(eu);

            float rad = Data.GetValue<Vector2>("radius").magnitude;
            float alpha = Data.GetValue<float>("alphaChannel");
            bool flat = Data.GetValue<bool>("flatLight");
            Color col = Data.GetValue<Color>("lightCol");
            float darknessEffect = Data.GetValue<float>("paletteDarkness");

            if (!flickering) LightSource.setAlpha = alpha;
            LightSource.color = col;
            LightSource.setRad = rad;
            LightSource.setPos = LocalPlacedObject.pos;
            LightSource.flat = flat;
            LightSource.affectedByPaletteDarkness = darknessEffect;

            if (room.game.clock % 2 != 0) return;
            float noiseIntensity = Data.GetValue<float>("flickIntensity") * OneDimensionalPerlinNoise();
            if (noiseIntensity > Data.GetValue<float>("threshold")) {
                LightSource.setAlpha = 0f;
                flickering = true;
            }
            else {
                LightSource.setAlpha = alpha;
                flickering = false;
            }
        }

        //Based off of a wind direction algorithm online. If you know a better noise function, entertain me
        private float OneDimensionalPerlinNoise() {
            var aux = Random.value * Mathf.PI * 2f;
            var vectorX = Mathf.Cos(aux);
            var vectorY = Mathf.Sin(aux);

            float noiseValue = Mathf.Clamp01(Mathf.PerlinNoise(vectorX * Time.time, vectorY * Time.time));
            return noiseValue;
        }

        public static void RegisterAsFullyManagedObject() => PlacedObjectsManager.RegisterFullyManagedObjectType(Fields, typeof(ColouredLightSource));
    }
}