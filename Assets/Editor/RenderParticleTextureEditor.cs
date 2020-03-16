// By Olli S.

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RenderParticleTexture), true)]
public class RenderParticleTextureEditor : Editor
{

    RenderParticleTexture pCreate;

    SerializedProperty m_targetParticleSystemProp;
    SerializedProperty m_GradientProp;
    SerializedProperty m_ShapeProp;
    SerializedProperty m_TextureNameProp;


    void OnEnable()
    {
        pCreate = (RenderParticleTexture)target;

        m_targetParticleSystemProp = serializedObject.FindProperty("targetParticleSystem");
        m_GradientProp = serializedObject.FindProperty("gradientLook");
        m_ShapeProp = serializedObject.FindProperty("shape");
        m_TextureNameProp = serializedObject.FindProperty("textureName");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUIStyle LabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
        };

        EditorGUILayout.PropertyField(m_targetParticleSystemProp);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Gradient for particle (color/opacity)", LabelStyle);
        EditorGUILayout.PropertyField(m_GradientProp, new GUIContent(""));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Shader", LabelStyle);
        RenderParticleTexture.ShaderType newShaderValue = (RenderParticleTexture.ShaderType) EditorGUILayout.EnumPopup( pCreate.shaderType );
        if( newShaderValue != pCreate.shaderType )
        {
            pCreate.shaderType = newShaderValue;
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Resolution", LabelStyle);
        RenderParticleTexture.TextureResolution newResValue = (RenderParticleTexture.TextureResolution)EditorGUILayout.EnumPopup( pCreate.textureResolution );
        if( newResValue != pCreate.textureResolution )
        {
            pCreate.textureResolution = newResValue;
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Particle Shape", LabelStyle);
        EditorGUILayout.PropertyField(m_ShapeProp, new GUIContent(""));
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_TextureNameProp);
        EditorGUILayout.Space();

        LabelStyle.fontSize = 14;
        EditorGUILayout.LabelField("Particle Preview", LabelStyle);

        Texture2D texturePreview = UpdateTexturePreview();
        GUILayout.Label(texturePreview);

        if(GUILayout.Button("Generate Texture"))
        {
            pCreate.GenerateTexture();
            texturePreview = UpdateTexturePreview();
        }

        if(GUILayout.Button("Save Texture"))
        {
            pCreate.GenerateTexture();
            texturePreview = UpdateTexturePreview();
            pCreate.SaveCurrentTextureToDisk();
        }

        serializedObject.ApplyModifiedProperties();
    }


    private Texture2D UpdateTexturePreview()
    {
        if (pCreate.particleTexture != null)
        {
            Texture2D texturePreview = AssetPreview.GetAssetPreview(pCreate.particleTexture);
            return texturePreview;
        }
        return null;
    }

}