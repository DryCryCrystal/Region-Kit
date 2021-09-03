﻿using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using RegionKit.Utils;

using static RegionKit.POM.PlacedObjectsManager;

namespace RegionKit.Machinery
{
    public class BaseMachineryData : ManagedData
    {
        public BaseMachineryData(PlacedObject owner, ManagedField[] fields) : base(owner, fields) { }

        internal MachineryCustomizer assignedMC;
    }

    #region pistons
    public class PistonData : BaseMachineryData, ICanBringDataToKin<PistonData> 
    {
        [BackedByField("opmode")]
        internal OperationMode opmode = OperationMode.Sinal;
        [FloatField("rot", -180f, 180f, 0f, increment: 1f, displayName: "Direction", control: ManagedFieldWithPanel.ControlType.slider)]
        internal float rotation = 0f;
        [FloatField("amp", 0f, 120f, 20f, increment: 1f, displayName: "Amplitude", control: ManagedFieldWithPanel.ControlType.text)]
        internal float amplitude = 20f;
        [BooleanField("align_rot", true, displayName: "Straight angles only", control: ManagedFieldWithPanel.ControlType.button)]
        internal bool align = false;
        [FloatField("phase", -5f, 5f, 0f, displayName: "Phase", control: ManagedFieldWithPanel.ControlType.text)]
        internal float phase = 0f;
        [FloatField("frequency", 0.05f, 2f, 1f, displayName: "Frequency", increment: 0.05f, control: ManagedFieldWithPanel.ControlType.text)]
        internal float frequency = 1f;
        internal Vector2 forcePos;

        public PistonData(PlacedObject owner) : base(owner, new ManagedField[] { new EnumField("opmode", typeof(OperationMode), OperationMode.Sinal, displayName: "Operation mode"), })
        {

        }

        public void BringToKin(PistonData other)
        {
            other.opmode = this.opmode;
            other.rotation = this.rotation;
            other.amplitude = this.amplitude;
            //other.sharpFac = this.sharpFac;
            other.align = this.align;
            other.phase = this.phase;
            other.frequency = this.frequency;
            other.forcePos = this.forcePos;
        }
    }
    public class PistonArrayData : BaseMachineryData
    {
        [IntegerField("count", 1, 35, 3, displayName:"Piston count")]
        internal int pistonCount;
        [FloatField("relrot", -90f, 90f, 0f, increment:0.5f, displayName:"Relative rotation", control:ManagedFieldWithPanel.ControlType.text)]
        internal float relativeRotation;
        [BackedByField("point2")]
        internal Vector2 point2;
        [FloatField("amp", 0f, 120f, 20f, increment: 1f, displayName: "Amplitude", control: ManagedFieldWithPanel.ControlType.slider)]
        internal float amplitude;
        [BooleanField("align_rot", true, displayName: "Straight angles only", control: ManagedFieldWithPanel.ControlType.button)]
        internal bool align;
        [FloatField("phaseInc", -5f, 5f, 0f, displayName: "Phase increment", control: ManagedFieldWithPanel.ControlType.slider)]
        internal float phaseInc;
        [FloatField("frequency", 0.05f, 2f, 1f, displayName: "Frequency", increment: 0.05f)]
        internal float frequency;


        public PistonArrayData(PlacedObject owner) : base(owner, new ManagedField[] { new Vector2Field("point2", owner.pos, Vector2Field.VectorReprType.line)})
        {

        }

        
    }
    #endregion

    #region cogs
    public class SimpleCogData : BaseMachineryData
    {
        internal Vector2 forcepos;
        internal OperationMode opmode => GetValue<OperationMode>("opmode");
        [FloatField("AVSamp", 0.1f, 15f, 1f, increment:0.1f, displayName:"AV shift amplitude", control:ManagedFieldWithPanel.ControlType.text)]
        internal float angVelShiftAmp;
        [FloatField("AVSfrq", 0.1f, 3f, 1f, increment:0.05f, displayName:"AV shift frequency", control: ManagedFieldWithPanel.ControlType.text)]
        internal float angVelShiftFrq;
        //[FloatField("AVphs", -5f, 5f, 0f, increment:0.1f, displayName: "AV shift phase")]
        //internal float angVelShiftPhs;
        [FloatField("AVbase", -30f, 30f, 10f, increment:0.2f, displayName:"Angular velocity (AV)", control: ManagedFieldWithPanel.ControlType.text)]
        internal float baseAngVel;
        internal float rad;

        public SimpleCogData(PlacedObject owner) : base(owner, new ManagedField[] 
        {
            new EnumField("opmode", typeof(OperationMode), OperationMode.Cosinal, displayName:"Operation mode")
        })
        {

        }
    }

    #endregion

    public class MachineryCustomizer : ManagedData, ICanBringDataToKin<FSprite>
    {
        [StringField("element", "pixel", "Atlas element")]
        internal string elementName = "pixel";
        [StringField("shader", "Basic", displayName:"Shader")]
        internal string shaderName = "Basic";
        //[StringField("container", "Items", displayName:"rCam container")]
        internal string ContainerName => GetValue<ContainerCodes>("containerCode").ToString();
        [FloatField("scX", 0f, 35f, 1f, increment:0.1f, ManagedFieldWithPanel.ControlType.text, displayName:"X scale")]
        internal float scX = 1f;
        [FloatField("scY", 0f, 35f, 1f, increment: 0.1f, ManagedFieldWithPanel.ControlType.text, displayName:"Y scale")]
        internal float scY = 1f;
        [FloatField("addRot", -90f, 90f, 0f, increment:0.5f, displayName:"Additional rotation")]
        internal float addRot = 0f;
        //[BackedByField("sCol")]
        internal Color spriteColor => GetValue<Color>("sCol");
        //[BackedByField("amID")]
        internal MachineryID affectedMachinesID => GetValue<MachineryID>("amID");
        [FloatField("alpha", 0f, 1f, 1f, increment:0.01f, ManagedFieldWithPanel.ControlType.text, "Alpha")]
        internal float alpha;
        [FloatField("anchX", -10f, 10f, 0.5f, control:ManagedFieldWithPanel.ControlType.text, displayName:"X anchor")]
        internal float anchX;
        [FloatField("anchY", -10f, 10f, 0.5f, control: ManagedFieldWithPanel.ControlType.text, displayName: "Y anchor")]
        internal float anchY;

        internal bool AffectsInPoint(Vector2 p)
        {
            return (p - owner.pos).sqrMagnitude < GetValue<Vector2>("radius").magnitude;
        }

        public void BringToKin(FSprite other)
        {
            other.color = spriteColor;
            other.alpha = alpha;
            other.scaleX = scX;
            other.scaleY = scY;
            other.anchorX = anchX;
            other.anchorY = anchY;
            try { other.element = Futile.atlasManager.GetElementWithName(elementName); }
            catch { other.element = Futile.atlasManager.GetElementWithName("pixel"); }
            try { other.shader = MachineryStatic.rw.Shaders[shaderName]; }
            catch { other.shader = FShader.defaultShader; }
        }

        public MachineryCustomizer(PlacedObject owner) :
            base(owner,
                new ManagedField[] {
                    new ColorField("sCol", Color.red, displayName:"Color"),
                    new EnumField("amID", typeof(MachineryID), MachineryID.Piston, displayName:"Affected machinery"),
                    new Vector2Field("radius", new Vector2(30f, 0f), Vector2Field.VectorReprType.circle),
                    new EnumField("containerCode", typeof(ContainerCodes), ContainerCodes.Items, displayName:"Container")
                })
        { }
    }

    public class PowerManagerData : ManagedData
    {
        [FloatField("basePower", 0f, 1f, 1f, increment:0.02f, displayName:"Base power")]
        internal float basePowerLevel;

        public PowerManagerData(PlacedObject owner) : base(owner, new ManagedField[] { })
        {

        }
    }
}
