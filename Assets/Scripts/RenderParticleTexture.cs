// By Olli S.

using UnityEngine;
using System.IO;
using System;
using UnityEditor;


public class RenderParticleTexture : MonoBehaviour
{

    public Gradient gradientLook;

    private Vector2Int gradientMapDimensions = new Vector2Int(128, 32);

    public Texture2D particleTexture;

    public ParticleSystemRenderer targetParticleSystem;


    private ComputeShader compute;

    private int kernel_render;

    private int propResultTexture = Shader.PropertyToID("ResultTexture");
    private int propGradientTexture = Shader.PropertyToID("GradientTexture");
    private int propShapeID = Shader.PropertyToID("_ShapeID");

    private RenderTexture resultTexture;


    public enum ShaderType
    {
        Additive,
        AlphaBlended,
        Multiply,
        VertexlitBlended
    }

    public ShaderType shaderType;


    public enum TextureResolution
    {
        _32x32 = 32,
        _64x64 = 64,
        _128x128 = 128,
        _256x256 = 256,
        _512x512 = 512,
        _1024x1024 = 1024,
    }

    public TextureResolution textureResolution;


    public enum Shape
    {
        Circle = 0,
        Triangle = 1,
        Rectangle = 2,
        Pentagon = 3,
        Hexagon = 4,
        Heptagon = 5,
        Octagon = 6,
        Flower = 7,
        Blob = 8,
        Star = 9,
        SoftRect = 10,
        Blade = 11,
        Rhombus = 12,
    }

    public Shape shape;

    public string textureName;


    private void OnDisable()
    {
        if (resultTexture != null)
        {
            resultTexture.Release();
        }
    }


    private void OnValidate()
    {
        if (String.IsNullOrEmpty(textureName))
        {
            textureName = "particleTexture_" + DateTime.Now.ToString("yyyy_MM_dd");
        }
    }


    public void GenerateTexture()
    {
        AllocateResultTexture();
        particleTexture = CreateParticleTextureGPU();
        SetParticleTexture(particleTexture);
    }


    private void AllocateResultTexture()
    {
        if (resultTexture != null && resultTexture.width != (int)textureResolution)
        {
            resultTexture.Release();
            InitResultTexture();
        }
        else if (resultTexture != null && resultTexture.width == (int)textureResolution && resultTexture.height == (int)textureResolution)
        {
            if (resultTexture.IsCreated())
            {
                return;
            }
            else
            {
                InitResultTexture();
            }
        }
        else
        {
            InitResultTexture();
        }
    }


    private void InitResultTexture()
    {
        int width = (int)textureResolution;
        int height = (int)textureResolution;
        resultTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();
    }


    private Texture2D CreateParticleTextureGPU()
    {
        FindCompute();

        compute.SetTexture(kernel_render, propResultTexture, resultTexture);

        Texture2D gradToGPUtexture = BakeGradient(gradientLook);

        compute.SetTexture(kernel_render, propGradientTexture, gradToGPUtexture);

        compute.SetInt(propShapeID, (int)shape);

        int xDim = Mathf.Max(8, resultTexture.width / 8);
        int yDim = Mathf.Max(8, resultTexture.height / 8);
        int zDim = 1;

        compute.Dispatch(kernel_render, xDim, yDim, zDim);

        Texture2D texture = ReadGPUTexture(resultTexture.width, resultTexture.height);

        return texture;
    }


    private void FindCompute()
    {
        compute = (ComputeShader)Resources.Load("RenderParticle");
        kernel_render = compute.FindKernel("GPU_Render");
    }


    private Texture2D BakeGradient(Gradient gradient)
    {
        Texture2D texture = new Texture2D(gradientMapDimensions.x, gradientMapDimensions.y);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < gradientMapDimensions.x; x++)
        {
            Color color = gradient.Evaluate((float)x / (float)gradientMapDimensions.x);
            for (int y = 0; y < gradientMapDimensions.y; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }



    private Texture2D ReadGPUTexture(int height, int width)
    {
        RenderTexture.active = resultTexture;

        Texture2D readTarget = new Texture2D(width, height);
        readTarget.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        readTarget.Apply();

        return readTarget;
    }


    private void SetParticleTexture(Texture2D texture)
    {
        if (targetParticleSystem != null)
        {
            Shader shader = GetShader(shaderType);

            targetParticleSystem.sharedMaterial = new Material(shader);

            targetParticleSystem.sharedMaterial.mainTexture = texture;
        }
        else
        {
            Debug.Log(this.GetType() + " <color=red>No particle system assigned.</color>");
        }
    }


    private Shader GetShader(ShaderType shaderType)
    {
        if (shaderType == ShaderType.Additive)
        {
            return Shader.Find("Mobile/Particles/Additive");
        }
        else if (shaderType == ShaderType.AlphaBlended)
        {
            return Shader.Find("Mobile/Particles/Alpha Blended");
        }
        else if (shaderType == ShaderType.Multiply)
        {
            return Shader.Find("Mobile/Particles/Multiply");
        }
        else if (shaderType == ShaderType.VertexlitBlended)
        {
            return Shader.Find("Mobile/Particles/VertexLit Blended");
        }
        return null;
    }


    public void SaveCurrentTextureToDisk()
    {
        if (particleTexture != null)
        {
            byte[] bytes = particleTexture.EncodeToPNG();
            var path = Application.dataPath + "/ParticleTextures/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllBytes(path + textureName + ".png", bytes);
        }

        AssetDatabase.Refresh();
    }

}