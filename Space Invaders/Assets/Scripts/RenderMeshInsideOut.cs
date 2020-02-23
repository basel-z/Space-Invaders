using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderMeshInsideOut : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // look for mesh model
        MeshFilter meshComponent = GetComponent<MeshFilter>();
        if (meshComponent == null)
        {
            Debug.LogError(gameObject.name + "(RenderMeshInsideOut.cs): looked for non-existent mesh model in RenderMeshInsideOut.cs");
            return;
        }
        Mesh meshModel = meshComponent.mesh;

        // flip the normals
        for (int i = 0, normalsAmount = meshModel.normals.Length; i < normalsAmount; ++i) {
            meshModel.normals[i] *= (-1);
        }

        //we also need to flip the triangles being rendered as well.
        for (int i = 0, subMeshCount = meshModel.subMeshCount; i < subMeshCount; ++i)
        {
            // if there are n triangles, then trianglesVertices will have 3*n vertices!
            int[] trianglesVertices = meshModel.GetTriangles(i);
            for (int j = 0, trianglesAmount = trianglesVertices.Length; j < trianglesAmount; j += 3)
            {
                int temp = trianglesVertices[j];
                trianglesVertices[j] = trianglesVertices[j + 1];
                trianglesVertices[j + 1] = temp;
            }

            meshModel.SetTriangles(trianglesVertices, i);
        }
    }

}
