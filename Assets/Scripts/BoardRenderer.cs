using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;


public class BoardRenderer : MonoBehaviour
{
    
    [Header("Material handling")]
    public Material minoMaterial;
    public Mesh[] minoMeshes;
    private Material[] minoAlphaLevels;
    private MaterialPropertyBlock[] minoPropertyBlocks;
    public Transform transformReference;
    public void RenderBlock(int x, int y, int textureID, float alpha = 1f)
    {
        RenderBlock(new int2(x, y), textureID, 0, 1f, alpha);
    }
    public void RenderBlock(int2 position, int textureID, float alpha = 1f) => RenderBlock((float2)position, textureID, 0, 1, alpha);
    public void RenderBlock(float2 position, int textureID, int meshID, float scale = 1, float alpha = 1)
    {
        Material material;
        if (alpha >= 1)
        {
            material = minoMaterial;
        }
        else
        {
            material = minoAlphaLevels[(int)math.floor(alpha * 16)];
        }
        if (textureID >= 0)
        {
            transformReference.localPosition = new Vector3(position.x, position.y, 0);
            transformReference.localScale = Vector3.one * scale;
            RenderParams renderParams = new RenderParams(material);
            renderParams.matProps = minoPropertyBlocks[textureID];
            Matrix4x4 matrix = transformReference.localToWorldMatrix;
            Graphics.RenderMesh(renderParams, minoMeshes[meshID], 0, matrix);
        }
    }
    public void RenderBlocks(int2[] blocks, int textureID)
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            RenderBlock(blocks[i], textureID);
        }
    }
    
    private void Start()
    {
        minoPropertyBlocks = new MaterialPropertyBlock[TextureUVs.UVs.Length];
        for (int i = 0; i < TextureUVs.UVs.Length; i++)
        {
            float2 uvs = TextureUVs.UVs[i];
            minoPropertyBlocks[i] = new MaterialPropertyBlock();
            minoPropertyBlocks[i].SetVector("_BaseMap_ST", new Vector4(1f / 4f, 1f / 10f, 1- uvs.x, 1-uvs.y));
        }
        minoAlphaLevels = new Material[15];
        for (int i = 1; i < 16; i++)
        {
            minoAlphaLevels[i-1] = Instantiate(minoMaterial);
            minoAlphaLevels[i-1].color = new Color(1, 1, 1, i / 16f);
        }
    }
}