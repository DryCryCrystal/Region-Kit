using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;

namespace RegionKit.Machinery
{

    public class PistonData : PlacedObjectsManager.ManagedData, ICanBringDataToKin<PistonData> 
    {
        [BackedByField("opmode")]
        internal OperationMode opmode;
        [PlacedObjectsManager.FloatField("rot", 0f, 360f, 0f, increment: 1f, displayName: "Direction", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float rotation;
        [PlacedObjectsManager.FloatField("amp", 0f, 120f, 20f, increment: 1f, displayName: "Amplitude", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float amplitude;
#warning sharpFac not affecting anything
        [PlacedObjectsManager.FloatField("shFac", 0f, 1f, 0f, increment: 0.05f, displayName: "Sharpening Factor", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float sharpFac;
        [PlacedObjectsManager.BooleanField("align_rot", true, displayName: "Straight angles only", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.button)]
        internal bool align;
        [PlacedObjectsManager.FloatField("phase", -5f, 5f, 0f, displayName: "Phase", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float phase;
        [PlacedObjectsManager.FloatField("frequency", 0.05f, 2f, 1f, displayName: "frequency", increment: 0.05f)]
        internal float frequency;
        internal Vector2 forcePos;

        public PistonData(PlacedObject owner) : base(owner, new PlacedObjectsManager.ManagedField[] { new PlacedObjectsManager.EnumField("opmode", typeof(OperationMode), OperationMode.Sinal, displayName: "Operation mode"), })
        {

        }

        public void CopyToKin(PistonData other)
        {
            other.opmode = this.opmode;
            other.rotation = this.rotation;
            other.amplitude = this.amplitude;
            other.sharpFac = this.sharpFac;
            other.align = this.align;
            other.phase = this.phase;
            other.frequency = this.frequency;
            other.forcePos = this.forcePos;
        }
    }

    public class PistonArrayData : PlacedObjectsManager.ManagedData
    {
        [PlacedObjectsManager.IntegerField("count", 1, 35, 3, displayName:"Piston count")]
        internal int pistonCount;
        [PlacedObjectsManager.FloatField("relrot", 0f, 180f, 0f, increment:0.5f, displayName:"relative rotation:")]
        internal float relativeRotation;
        [BackedByField("point2")]
        internal Vector2 point2;
        [PlacedObjectsManager.FloatField("amp", 0f, 120f, 20f, increment: 1f, displayName: "Amplitude", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float amplitude;
        [PlacedObjectsManager.FloatField("shFac", 0f, 1f, 0f, increment: 0.05f, displayName: "Sharpening Factor", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float sharpFac;
        [PlacedObjectsManager.BooleanField("align_rot", true, displayName: "Straight angles only", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.button)]
        internal bool align;
        [PlacedObjectsManager.FloatField("phaseInc", -5f, 5f, 0f, displayName: "Phase increment", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float phaseInc;
        [PlacedObjectsManager.FloatField("frequency", 0.05f, 2f, 1f, displayName: "Frequency", increment: 0.05f)]
        internal float frequency;


        public PistonArrayData(PlacedObject owner) : base(owner, new PlacedObjectsManager.ManagedField[] { new PlacedObjectsManager.Vector2Field("point2", owner.pos, PlacedObjectsManager.Vector2Field.VectorReprType.line)})
        {

        }

        
    }
}
