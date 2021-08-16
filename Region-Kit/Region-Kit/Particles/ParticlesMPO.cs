using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RegionKit.Utils;
using RWCustom;

//using static System.Math;
using static RegionKit.Utils.RKUtils;
using static UnityEngine.Mathf;
using static RWCustom.Custom;
using static PlacedObjectsManager;

namespace RegionKit.Particles
{
    public abstract class ParticleSystemData : ManagedData
    {
        [IntegerField("fadeIn", 0, 400, 80, ManagedFieldWithPanel.ControlType.text, displayName:"Fade-in frames")]
        public int fadeIn;
        [IntegerField("fadeInFluke", 0, 400, 0, ManagedFieldWithPanel.ControlType.text, displayName: "Fade-in fluke")]
        public int fadeInFluke;
        [IntegerField("fadeOut", 0, 400, 80, ManagedFieldWithPanel.ControlType.text, displayName: "Fade-out frames")]
        public int fadeOut;
        [IntegerField("fadeOutFluke", 0, 400, 80, ManagedFieldWithPanel.ControlType.text, displayName: "Fade-out fluke")]
        public int fadeOutFluke;
        [IntegerField("lt", 0, 15000, 2000, ManagedFieldWithPanel.ControlType.text, displayName: "Lifetime")]
        public int lifeTime;
        [IntegerField("ltFluke", 0, 15000, 0, ManagedFieldWithPanel.ControlType.text, displayName: "Lifetime fluke")]
        public int lifeTimeFluke;
        public float startDir => VecToDeg(GetValue<Vector2>("sdBase"));
        [FloatField("sdFluke", 0f, 180f, 0f, displayName:"Direction fluke (deg)")]
        public float startDirFluke;
        [FloatField("speed", 0f, 100f, 10f, control:ManagedFieldWithPanel.ControlType.text, displayName:"Speed")]
        public float startSpeed;
        [FloatField("speedFluke", 0f, 100f, 10f, control: ManagedFieldWithPanel.ControlType.text, displayName: "Speed fluke")]
        public float startSpeedFluke;

        public abstract List<IntVector2> GetSuitableTiles();

        public ParticleSystemData(PlacedObject owner, List<ManagedField> additionalFields)
            : base (owner,
                  additionalFields.AddRangeReturnSelf(new ManagedField[]
                  {
                      new Vector2Field("sdBase", new Vector2(30f, 30f), label:"Direction"),
                  }).ToArray())
        {
            
        }

        public PBehaviourState DataForNew()
        {
            var res = new PBehaviourState
            {
                dir = LerpAngle(startDir - startDirFluke, startDir + startDirFluke, UnityEngine.Random.value),
                speed = Clamp(Lerp(startSpeed - startSpeedFluke, startSpeed + startSpeedFluke, UnityEngine.Random.value), 0f, float.MaxValue),
                fadeIn = ClampedIntDeviation(fadeIn, fadeInFluke, minRes:0),
                fadeOut = ClampedIntDeviation(fadeOut, fadeOutFluke, minRes:0),
                lifetime = ClampedIntDeviation(lifeTime, lifeTimeFluke, minRes:0)
            };

            return res;
        }
    }

    public class RectParticleSpawnerData : ParticleSystemData
    {
        Vector2 RectBounds => GetValue<Vector2>("effRect");

        public RectParticleSpawnerData(PlacedObject owner) : base (owner, new List<ManagedField>
        {
            new Vector2Field("effRect", new Vector2(40f, 40f), Vector2Field.VectorReprType.rect)
        })
        {

        }

        public override List<IntVector2> GetSuitableTiles()
        {
            var res = new List<IntVector2>();
            IntVector2 orpos = (owner.pos / 20f).ToIV2();
            IntVector2 bounds = (RectBounds / 20f).ToIV2();
            IntRect area = IntRect.MakeFromIntVector2(orpos);
            area.ExpandToInclude(bounds);
            for (int x = area.left; x < area.right; x++)
            {
                for (int y = area.bottom; y < area.top; y++)
                {
                    res.Add(new IntVector2(x, y));
                }
            }
            return res;
        }
    }

    public class ParticleVisualCustomizer : ManagedData
    {
        public Color spriteColor => GetValue<Color>("sColBase");
        public Color spriteColorFluke => GetValue<Color>("sColFluke");
        public Color lightColor => GetValue<Color>("lColBase");
        public Color lightColorFluke => GetValue<Color>("lColFluke");
        [FloatField("lrminBase", 0f, float.MaxValue, 20f, displayName:"Light radius min")]
        public float lightRadMin;
        [FloatField("lrminFluke", 0f, float.MaxValue, 0f, displayName:"Lightradmin fluke")]
        public float lightRadMinFluke;
        [FloatField("lrmaxBase", 0f, float.MaxValue, 30f, displayName:"Light radius max")]
        public float lightRadMax;
        [FloatField("lrmaxFluke", 0f, float.MaxValue, 30f, displayName: "Lightradmax fluke")]
        public float lightRadMaxFluke;
        [FloatField("lIntBase", 0f, 1f, 0f, displayName:"Light intensity")]
        public float LightIntensity;
        [FloatField("lIntFluke", 0f, 1f, 0f, displayName:"Light intensity fluke")]
        public float LightIntensityFluke;
        [StringField("eName", "pixel", displayName:"Atlas element")]
        public string elmName;
        [StringField("shader", "Basic", displayName:"Shader")]
        public string shader;

        public ParticleVisualCustomizer(PlacedObject owner) : base(owner, new ManagedField[]
        {
            new ColorField("sColBase", new Color(1f, 1f, 1f), displayName:"Sprite color"),
            new ColorField("sColFluke", new Color(0f, 0f, 0f), displayName:"Sprite color fluke"),
            new ColorField("lColBase", new Color(1f, 1f,1f), displayName:"Light color"),
            new ColorField("lColFluke", new Color(0f, 0f, 0f), displayName:"Light color fluke"),
        })
        {
            
        }

        public PVisualState DataForNew()
        {
#warning finish pvs.datafornew
            var res = new PVisualState
            {
                sCol = spriteColor.RandDeviate(spriteColorFluke),
                lCol = lightColor.RandDeviate(lightColorFluke),
                lRadMin = ClampedFloatDeviation(lightRadMin, lightRadMinFluke, minRes: 0f),
                lRadMax = ClampedFloatDeviation(lightRadMax, lightRadMaxFluke, minRes: 0f),
                lInt = ClampedFloatDeviation(LightIntensity, LightIntensityFluke, minRes: 0f),
                aElm = elmName,
                shader = shader,
                container = ContainerCodes.Foreground
            };

            return res;
        }
    }

}