using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutateGenotype
{
    public static float NextGaussian()
    {
        float v1, v2, s;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }

    public static float NextGaussian(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }

    public static float NextGaussian(float mean, float standard_deviation, float min, float max)
    {
        float x;
        do
        {
            x = NextGaussian(mean, standard_deviation);
        } while (x < min || x > max);
        return x;
    }

    public static bool CoinFlip(float percent)
    {
        return Random.Range(0.0f, 1.0f) <= percent;
    }

    public static int RandomSign()
    {
        return Random.Range(0, 2) * 2 - 1;
    }

    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    public class MutationPreferenceSetting
    {
        public bool mutateMorphology = true;
        public bool mutateNeural = false;
        public float stdevSizeAdjustmentFactor = 0.3f;

        public struct MutationPreference
        {
            public float mutationChance;
            public float stdev;

            public MutationPreference(float mutationChance, float stdev)
            {
                this.mutationChance = mutationChance;
                this.stdev = stdev;
            }
        }

        float currentScaleFactor = 1f;
        public Dictionary<string, MutationPreference> mutationFrequencies = new Dictionary<string, MutationPreference>()
        {
            {"s_r", new MutationPreference(0.25f, 0.25f)}, // Red (byte)
            {"s_g", new MutationPreference(0.25f, 0.25f)}, // Green (byte)
            {"s_b", new MutationPreference(0.25f, 0.25f)}, // Blue (byte)
            {"s_rl", new MutationPreference(0.25f, 0.25f)}, // Recursive limit (byte, 0:15)
            {"s_dx", new MutationPreference(0.25f, 0.25f)}, // Dimension X (float, 0.05f:3f)
            {"s_dy", new MutationPreference(0.25f, 0.25f)}, // Dimension Y (float, 0.05f:3f)
            {"s_dz", new MutationPreference(0.25f, 0.25f)}, // Dimension Z (float, 0.05f:3f)
            {"s_jt", new MutationPreference(0.25f, 0.25f)}, // JointType (enum, 0:3)
            {"s_dest", new MutationPreference(0.25f, 0.25f)}, // Destination (byte, 1:255)
            {"s_a", new MutationPreference(0.25f, 0.25f)}, // Anchor (sadness)
            {"s_o", new MutationPreference(0.25f, 0.25f)}, // Orientation (float, 0f:360f)
            {"s_s", new MutationPreference(0.25f, 0.25f)}, // Scale (float, 0.2f:2f)
            {"s_reflected", new MutationPreference(0.25f, 0.25f)}, // Reflected (bool)
            {"s_t", new MutationPreference(0.25f, 0.25f)}, // Terminal-only (bool)
            {"s_addc", new MutationPreference(0.25f, 0.25f)}, // Add connection
            {"s_removec", new MutationPreference(0.25f, 0.25f)}, // Remove connection
            {"n_t", new MutationPreference(0.25f, 0.25f)}, // Type (byte, 0:22)
            {"n_w1", new MutationPreference(0.25f, 0.25f)}, // Weight 1 (float, -15:15)
            {"n_w2", new MutationPreference(0.25f, 0.25f)}, // Weight 2 (float, -15:15)
            {"n_w3", new MutationPreference(0.25f, 0.25f)}, // Weight 3 (float, -15:15)
            {"n_relocateinput", new MutationPreference(0.25f, 0.25f)}, // Relocate Input
        };

        public Dictionary<string, float[]> floatClamps = new Dictionary<string, float[]>() {
            {"s_dx", new float[2]{0.05f,3f}},
            {"s_dy", new float[2]{0.05f,3f}},
            {"s_dz", new float[2]{0.05f,3f}},
            {"s_o", new float[2]{0f,360f}},
            {"s_s", new float[2]{0.2f,2f}},
            {"n_w1", new float[2]{-15f,15f}},
            {"n_w2", new float[2]{-15f,15f}},
            {"n_w3", new float[2]{-15f,15f}},
        };

        public Dictionary<string, byte[]> byteClamps = new Dictionary<string, byte[]>() {
            {"s_r", new byte[2]{0,255}},
            {"s_g", new byte[2]{0,255}},
            {"s_b", new byte[2]{0,255}},
            {"s_rl", new byte[2]{0,15}},
            {"s_dest", new byte[2]{1,255}},
        };

        public bool CoinFlip(string parameter)
        {
            return CreatureUtil.CoinFlip(mutationFrequencies[parameter].mutationChance * currentScaleFactor);
        }

        public float ModifyFloat(float mean, string parameter)
        {
            return NextGaussian(mean, mutationFrequencies[parameter].stdev + Mathf.Abs(mean * stdevSizeAdjustmentFactor), floatClamps[parameter][0], floatClamps[parameter][1]);
        }

        public float ModifyFloatNoFactor(float mean, string parameter)
        {
            return NextGaussian(mean, mutationFrequencies[parameter].stdev, floatClamps[parameter][0], floatClamps[parameter][1]);
        }

        public byte ModifyByte(float mean, string parameter)
        {
            return (byte)Mathf.RoundToInt(NextGaussian(mean, mutationFrequencies[parameter].stdev + Mathf.Abs(mean * stdevSizeAdjustmentFactor), byteClamps[parameter][0], byteClamps[parameter][1]));
        }

        public byte ModifyByteNoFactor(float mean, string parameter)
        {
            return (byte)Mathf.RoundToInt(NextGaussian(mean, mutationFrequencies[parameter].stdev, byteClamps[parameter][0], byteClamps[parameter][1]));
        }

        public void SetFactor(float factor)
        {
            currentScaleFactor = factor;
        }
    }
}
