using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public enum EnvArrangeType { LINEAR, PLANAR };
public enum GraphicsLevel { LOW, MEDIUM, HIGH };
public enum EnvCode { OCEAN };

[System.Serializable]
public abstract class EnvironmentSettings {
    public abstract EnvCode envCode { get; }
    public readonly static Dictionary<EnvCode, string> envString = new Dictionary<EnvCode, string>() { { EnvCode.OCEAN, "OceanEnv" } };
    public abstract EnvArrangeType envArrangeType { get; }
    public GraphicsLevel graphicsLevel { get; set;  }

    public virtual float sizeX { get { return 5; } }
    public virtual float sizeZ { get { return 5; } }
}

/// <summary>
/// Base class to control a single environment
/// </summary>
public abstract class Environment : MonoBehaviour
{
    public EnvCode envCode;
    public abstract void ResetEnv();
}
