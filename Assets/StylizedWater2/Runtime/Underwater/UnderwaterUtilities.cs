//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;

namespace StylizedWater2
{
    public static class UnderwaterUtilities
    {
        private const float VERTEX_DISTANCE = 0.02f; //50 subdivisions (100 tris)
        
        private const int planeLengthSegments = 1;
        private const float SCALE = 1f; //Unit rectangle

        private static Mesh _WaterLineMesh;
        public static Mesh WaterLineMesh
        {
            get
            {
                if (!_WaterLineMesh) _WaterLineMesh = CreateMaskMesh();

                return _WaterLineMesh;
            }
        }
        
        private static Mesh CreateMaskMesh()
        {
            int subdivisions = Mathf.FloorToInt(SCALE / VERTEX_DISTANCE);
        
            int xCount = subdivisions + 1;
            int yCount = planeLengthSegments + 1;
            int numTriangles = subdivisions * planeLengthSegments * 6;
            int numVertices = xCount * yCount;
            
            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[numTriangles];
            Vector2[] uvs = new Vector2[numVertices];
            
            float scaleX = SCALE / subdivisions;
            float scaleY = SCALE / planeLengthSegments;
            
            int index = 0;
            for (int z = 0; z < yCount; z++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    vertices[index] = new Vector3(x * scaleX - (SCALE * 0.5f), z * scaleY - (SCALE * 0.5f), 0f);

                    uvs[index] = new Vector2(x * scaleX, z * scaleY);

                    index++;
                }
            }

            index = 0;
            for (int z = 0; z < planeLengthSegments; z++)
            {
                for (int x = 0; x < subdivisions; x++)
                {
                    triangles[index] = (z * xCount) + x;
                    triangles[index + 1] = ((z + 1) * xCount) + x;
                    triangles[index + 2] = (z * xCount) + x + 1;

                    triangles[index + 3] = ((z + 1) * xCount) + x;
                    triangles[index + 4] = ((z + 1) * xCount) + x + 1;
                    triangles[index + 5] = (z * xCount) + x + 1;
                    index += 6;
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            //Temp, so test mesh doesn't get culled
            //mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 5000f);

            #if SWS_DEV
            //Debug.Log("Created waterline mesh");
            #endif
            
            return mesh;
        }
		
		private static float GetNearPlaneHeight(Camera camera)
		{
			return camera.projectionMatrix.inverse.m11;
		}
      
        public static Vector3 GetNearPlaneBottomPosition(Camera targetCamera, float offset = 0f)
        {
            return targetCamera.transform.position + 
                (targetCamera.transform.forward * (targetCamera.nearClipPlane + offset)) - 
                (targetCamera.transform.up * (targetCamera.nearClipPlane + offset) * GetNearPlaneHeight(targetCamera));
        }
        
        public static Vector3 GetNearPlaneTopPosition(Camera targetCamera, float offset = 0f)
        {
            return targetCamera.transform.position + 
                   (targetCamera.transform.forward * (targetCamera.nearClipPlane + offset)) + 
                   (targetCamera.transform.up * (targetCamera.nearClipPlane + offset) * GetNearPlaneHeight(targetCamera));
        }
        
#if URP
        public static void ToggleUnderwaterKeyword(bool value)
        {
            if (value) Shader.EnableKeyword(UnderwaterRenderer.Keyword);
            else Shader.DisableKeyword(UnderwaterRenderer.Keyword);
        }
#endif
    }
}
