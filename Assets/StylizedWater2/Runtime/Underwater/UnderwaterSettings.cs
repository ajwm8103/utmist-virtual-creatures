using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedWater2.UnderwaterRendering
{
#if URP
    [Serializable, VolumeComponentMenu("Stylized Water 2/Underwater")]
    public class UnderwaterSettings : VolumeComponent
    {
        [Header("Fog (distance from camera)")]
        public FloatParameter startDistance = new FloatParameter(0f);
        [Min(0f)]
        public FloatParameter fogDensity = new FloatParameter(20f);
        
        [Space]
        
        [Header("Fog (distance from water)")]
        [Min(0f)]
        public FloatParameter heightFogDepth = new FloatParameter(25f);
        [Min(0f)]
        public FloatParameter heightFogDensity = new FloatParameter(1f);
        public ClampedFloatParameter heightFogBrightness = new ClampedFloatParameter(0.6f, 0f, 1f);
        
        [Space]

        [Header("Multipliers")]
        [Min(0f)]
        public FloatParameter fogBrightness = new FloatParameter(1f);
        [Min(0f)]
        public FloatParameter subsurfaceStrength = new FloatParameter(0.35f);
        [Min(0f)]
        public FloatParameter causticsStrength = new FloatParameter(1f);
        
        [Space]

        [Header("Distortion")]
        public ClampedFloatParameter distortionStrength = new ClampedFloatParameter(0.25f, 0f, 1f);
        public ClampedFloatParameter distortionFrequency = new ClampedFloatParameter(0.75f, 0f, 1f);
        public ClampedFloatParameter distortionSpeed = new ClampedFloatParameter(0.5f, 0f, 1f);
    }
#endif
}