﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UCombineSkinnedMgr
{
    /// <summary>
    /// Only for merge materials.
    /// </summary>
    private const int COMBINE_TEXTURE_MAX = 512;

    private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";

    /// <summary>
    /// Combine SkinnedMeshRenderers together and share one skeleton.
    /// Merge materials will reduce the drawcalls, but it will increase the size of memory. 
    /// </summary>
    /// <param name="skeleton">combine meshes to this skeleton(a gameobject)</param>
    /// <param name="meshes">meshes need to be merged</param>
    /// <param name="combine">merge materials or not</param>
    public void CombineObject(GameObject skeleton, SkinnedMeshRenderer[] meshes, bool combine = false)
    {
        // 获取Skeleton上的所有骨骼节点
        List<Transform> transforms = new List<Transform>(); //所有骨骼的transform
        transforms.AddRange(skeleton.GetComponentsInChildren<Transform>(true));


        //CombineInstance: unity的合并网络的类

        List<Material> materials = new List<Material>(); //the list of materials
        List<CombineInstance> combineInstances = new List<CombineInstance>(); //the list of meshes
        List<Transform> bones = new List<Transform>(); //the list of bones

        // Below informations only are used for merge material(bool combine = true)
        List<Vector2[]> oldUV = null;
        Material newMaterial = null;
        Texture2D newDiffuseTex = null;

        // Collect information from meshes
        for (int i = 0; i < meshes.Length; i++)
        {
            SkinnedMeshRenderer smr = meshes[i];
            materials.AddRange(smr.materials); // 收集每个SkinnedMeshRenderer用到的材质
            
            
            // 收集网格mesh
            for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
            {
                //创建Unity合并网格的CombineInstance
                CombineInstance ci = new CombineInstance();
                ci.mesh = smr.sharedMesh;
                ci.subMeshIndex = sub;
                combineInstances.Add(ci);
            }

            // 收集SkinnedMeshRenderer中用到的骨骼
            for (int j = 0; j < smr.bones.Length; j++)
            {
                int tBase = 0;
                for (tBase = 0; tBase < transforms.Count; tBase++)
                {
                    if (smr.bones[j].name.Equals(transforms[tBase].name))
                    {
                        bones.Add(transforms[tBase]);
                        break;
                    }
                }
            }
        }

        // 合并材质 materials
        if (combine)
        {
            newMaterial = new Material(Shader.Find("Mobile/Diffuse"));
            oldUV = new List<Vector2[]>();
            // merge the texture
            List<Texture2D> Textures = new List<Texture2D>();
            for (int i = 0; i < materials.Count; i++)
            {
                Textures.Add(materials[i].GetTexture(COMBINE_DIFFUSE_TEXTURE) as Texture2D);
            }

            newDiffuseTex = new Texture2D(COMBINE_TEXTURE_MAX, COMBINE_TEXTURE_MAX, TextureFormat.RGBA32, true);
            Rect[] uvs = newDiffuseTex.PackTextures(Textures.ToArray(), 0);
            newMaterial.mainTexture = newDiffuseTex;

            // reset uv
            Vector2[] uva, uvb;
            for (int j = 0; j < combineInstances.Count; j++)
            {
                uva = (Vector2[])(combineInstances[j].mesh.uv); //获取网格所有顶点的UV信息
                uvb = new Vector2[uva.Length];
                for (int k = 0; k < uva.Length; k++)
                {
                    //合并贴图以后，原来网格中顶点的uv信息变化了，重新计算uv坐标
                    uvb[k] = new Vector2((uva[k].x * uvs[j].width) + uvs[j].x, (uva[k].y * uvs[j].height) + uvs[j].y);
                }

                oldUV.Add(combineInstances[j].mesh.uv);
                combineInstances[j].mesh.uv = uvb;
            }
        }

        // Create a new SkinnedMeshRenderer
        SkinnedMeshRenderer oldSKinned = skeleton.GetComponent<SkinnedMeshRenderer>();
        if (oldSKinned != null)
        {
            GameObject.DestroyImmediate(oldSKinned);
        }

        SkinnedMeshRenderer r = skeleton.AddComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), combine, false); // Combine meshes
        r.bones = bones.ToArray(); // Use new bones
        if (combine)
        {
            r.material = newMaterial; //将创建的新材质赋值给创建的skinnedMeshRenderer
            for (int i = 0; i < combineInstances.Count; i++)
            {
                combineInstances[i].mesh.uv = oldUV[i]; //还原加载出来的mesh的uv坐标信息
            }
        }
        else
        {
            r.materials = materials.ToArray();
        }
    }
}