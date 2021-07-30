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
        internal OperationMode opmode = OperationMode.Sinal;
        [PlacedObjectsManager.FloatField("rot", 0f, 360f, 0f, increment: 1f, displayName: "Direction", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float rotation = 0f;
        [PlacedObjectsManager.FloatField("amp", 0f, 120f, 20f, increment: 1f, displayName: "Amplitude", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float amplitude = 20f;
        //[PlacedObjectsManager.FloatField("shFac", 1f, 5f, 1f, increment: 0.05f, displayName: "Sharpening Factor", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float sharpFac = 1f;
        [PlacedObjectsManager.BooleanField("align_rot", true, displayName: "Straight angles only", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.button)]
        internal bool align = false;
        [PlacedObjectsManager.FloatField("phase", -5f, 5f, 0f, displayName: "Phase", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float phase = 0f;
        [PlacedObjectsManager.FloatField("frequency", 0.05f, 2f, 1f, displayName: "Drequency", increment: 0.05f)]
        internal float frequency = 1f;
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
        [PlacedObjectsManager.FloatField("relrot", 0f, 180f, 0f, increment:0.5f, displayName:"Relative rotation")]
        internal float relativeRotation;
        [BackedByField("point2")]
        internal Vector2 point2;
        [PlacedObjectsManager.FloatField("amp", 0f, 120f, 20f, increment: 1f, displayName: "Amplitude", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
        internal float amplitude;
        //[PlacedObjectsManager.FloatField("shFac", 1f, 5f, 1f, increment: 0.05f, displayName: "Sharpening Factor", control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider)]
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

    public class MachineryCustomizer : PlacedObjectsManager.ManagedData
    {
        [PlacedObjectsManager.StringField("element", "pixel", "Atlas element")]
        internal string elementName = "pixel";
        [PlacedObjectsManager.StringField("shader", "Basic", displayName:"Shader")]
        internal string shaderName = "Basic";
        [PlacedObjectsManager.FloatField("scX", 0f, 35f, 1f, increment:0.1f, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, displayName:"X scale")]
        internal float scX = 1f;
        [PlacedObjectsManager.FloatField("scY", 0f, 35f, 1f, increment: 0.1f, PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, displayName:"Y scale")]
        internal float scY = 1f;
        [PlacedObjectsManager.FloatField("addRot", 0f, 180f, 0f, increment:0.5f, displayName:"Additional rotation")]
        internal float addRot = 0f;
        [BackedByField("sCol")]
        internal Color spriteColor = Color.red;
        [BackedByField("amID")]
        internal MachineryID affectedMachinesID;

        public MachineryCustomizer(PlacedObject owner) : 
            base(owner, 
                new PlacedObjectsManager.ManagedField[] {
                    new PlacedObjectsManager.ColorField("sCol", Color.red, displayName:"Color"),
                    new PlacedObjectsManager.EnumField("amID", typeof(MachineryID), MachineryID.Piston, displayName:"Affected machinery")
                })
        { }
    }
}
