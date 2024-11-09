using UnityEngine;

namespace Script
{
    public static class HexMetrics
    {
        public static Vector3[] corners =
        {
            new Vector3(0.0f, 0.0f, outerRadius),
            new Vector3(innerRadius, 0.0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0.0f, -0.5f * outerRadius),
            new Vector3(0.0f, 0.0f, -outerRadius),
            new Vector3(-innerRadius, 0.0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0.0f, 0.5f * outerRadius),
            new Vector3(0.0f, 0.0f, outerRadius)
        };
        
        public static Vector3 GetFirstSolidCorner(HexDirection dir)
        {
            return corners[(int)dir] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection dir)
        {
            return corners[(int)dir + 1] * solidFactor;
        }
        
        public static Vector3 GetFirstCorner(HexDirection dir)
        {
            return corners[(int)dir];
        }

        public static Vector3 GetSecondCorner(HexDirection dir)
        {
            return corners[(int)dir + 1];
        }

        public static Vector3 GetFirstWaterCorner(HexDirection dir)
        {
            return corners[(int)dir] * waterFactor;
        }
        public static Vector3 GetSecondWaterCorner(HexDirection dir)
        {
            return corners[(int)dir + 1] * waterFactor;
        }

        public static Vector3 GetWaterBridge(HexDirection dir)
        {
            return (corners[(int)dir] + corners[(int)dir + 1]) * waterBlendFactor;
        }

        public static Vector3 GetBridge(HexDirection dir)
        {
            return (corners[(int)dir] + corners[(int)dir + 1]) * blendFactor;
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * horTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;

            float v = ((step + 1) / 2) * HexMetrics.verTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }
        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * horTerraceStepSize;
            return Color.Lerp(a, b, h);
        }

        public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            if (elevation1 == elevation2)
                return HexEdgeType.Flat;

            int d = Mathf.Abs(elevation1 - elevation2);
            if (d == 1)
                return HexEdgeType.Slope;

            return HexEdgeType.Cliff;
        }

        public static Vector4 SampleNoise(Vector3 pos)
        {
            return noiseSource.GetPixelBilinear(pos.x * noiseScale, pos.z * noiseScale);
        }
        
        public static Vector3 Perturb(Vector3 pos)
        {
            Vector4 sample = SampleNoise(pos);
        
            pos.x += (sample.x * 2f - 1f) * cellPerturbStrength;
            pos.z += (sample.z * 2f - 1f) * cellPerturbStrength;
            return pos;
        }
        
        public static Vector3 GetSolidEdgeMiddle(HexDirection dir)
        {
            return (corners[(int)dir] + corners[(int)dir + 1]) * (0.5f * solidFactor);
        }

        public static void InitializeHashGrid(int seed)
        {
            hashGrid = new HexHash[hashGridSize * hashGridSize];
            Random.State curState = Random.state;
            Random.InitState(seed);
            for (int i = 0; i < hashGrid.Length; i++)
            {
                hashGrid[i] = HexHash.Create();
            }

            Random.state = curState;
        }

        public static HexHash SampleHashGrid(Vector3 pos)
        {
            int x = (int)(pos.x * hashGridScale) % hashGridSize;
            if (x < 0)
            {
                x += hashGridSize;
            }
            
            int z = (int)(pos.z * hashGridScale) % hashGridSize;
            if (z < 0)
            {
                z += hashGridSize;
            }
            return hashGrid[x + z * hashGridSize];
        }

        public const int chunkSizeX = 5, chunkSizeZ = 5;

        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;
        
        public const float outerRadius = 10.0f;
        public const float innerRadius = outerRadius * outerToInner;
        
        public const float solidFactor = 0.8f;
        public const float blendFactor = 1f - solidFactor;

        public const float elevationStep = 3f;

        public const int terracesPerSlope = 2;
        public const int terraceSteps = terracesPerSlope * 2 + 1;
        public const float horTerraceStepSize = 1f / terraceSteps;
        public const float verTerraceStepSize = 1f / (terracesPerSlope + 1);

        public static Texture2D noiseSource;
        public const float cellPerturbStrength = 4f;
        public const float elevationPerturbStrength = 1.5f;
        public const float noiseScale = 0.003f;

        public const float streamBedElevationOffset = -1.75f;
        public const float waterElevationOffset = -0.5f;
        public const float waterFactor = 0.5f;
        public const float waterBlendFactor = 1.0f - waterFactor;

        public const int hashGridSize = 256;
        public const float hashGridScale = 0.25f;
        private static HexHash[] hashGrid;

        private static float[][] featureThresholds =
        {
            new float[] { 0.0f, 0.0f, 0.4f },
            new float[] { 0.0f, 0.4f, 0.6f },
            new float[] { 0.4f, 0.6f, 0.8f }
        };

        public static Color[] colors;
        public static HexFeatureCollection[] featureCollections;
        public static HexUnit[] unitCollection;
        public static float[] GetFeatureThreshold(int level)
        {
            return featureThresholds[Mathf.Clamp(level,0, 2)];
        }
    }
}