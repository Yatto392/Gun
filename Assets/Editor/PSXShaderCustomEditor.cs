#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class PSXShaderCustomEditor : ShaderGUI
{
    private bool showPSXSettings = true;
    private bool showLightingControl = true;
    private bool showColorGrading = true;
    private bool showNormalSettings = true;
    private bool showTerrainSettings = true;
    private bool showAdvancedSettings = false;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;

        EditorGUILayout.LabelField("PSX Shader Custom Properties", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // PSX Settings Section
        showPSXSettings = EditorGUILayout.Foldout(showPSXSettings, "PSX Color Effects", true);
        if (showPSXSettings)
        {
            EditorGUI.indentLevel++;
            DrawPropertyToggle(materialEditor, properties, "_EnableColorPosterize", "Color Posterize");
            if (targetMat.IsKeywordEnabled("_ENABLE_PSX_COLOR_POSTERIZE"))
            {
                DrawProperty(materialEditor, properties, "_ColorPosterizeSteps");
            }

            EditorGUILayout.Space(5);

            DrawPropertyToggle(materialEditor, properties, "_EnableEdgeDetection", "Edge Detection");
            if (targetMat.IsKeywordEnabled("_ENABLE_PSX_EDGE_DETECTION"))
            {
                DrawProperty(materialEditor, properties, "_EdgeThreshold");
                DrawProperty(materialEditor, properties, "_EdgeColor");
            }

            EditorGUILayout.Space(5);

            DrawPropertyToggle(materialEditor, properties, "_EnableDithering", "Dithering");
            if (targetMat.IsKeywordEnabled("_ENABLE_PSX_DITHERING"))
            {
                DrawProperty(materialEditor, properties, "_DitherPattern");
                DrawProperty(materialEditor, properties, "_DitherScale");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // Lighting Control Section
        showLightingControl = EditorGUILayout.Foldout(showLightingControl, "Lighting Control", true);
        if (showLightingControl)
        {
            EditorGUI.indentLevel++;
            DrawPropertyToggle(materialEditor, properties, "_EnableCustomLighting", "Custom Lighting");
            if (targetMat.IsKeywordEnabled("_ENABLE_CUSTOM_LIGHTING"))
            {
                DrawProperty(materialEditor, properties, "_LightingContrast");
                DrawProperty(materialEditor, properties, "_LightingBrightness");
                DrawProperty(materialEditor, properties, "_ShadowThreshold");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // Color Grading Section
        showColorGrading = EditorGUILayout.Foldout(showColorGrading, "Color Grading", true);
        if (showColorGrading)
        {
            EditorGUI.indentLevel++;
            DrawPropertyToggle(materialEditor, properties, "_EnableColorGrading", "Color Grading");
            if (targetMat.IsKeywordEnabled("_ENABLE_COLOR_GRADING"))
            {
                DrawProperty(materialEditor, properties, "_Saturation");
                DrawProperty(materialEditor, properties, "_Hue");
                DrawProperty(materialEditor, properties, "_Contrast");
                DrawProperty(materialEditor, properties, "_Brightness");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // Normal Map Settings Section
        showNormalSettings = EditorGUILayout.Foldout(showNormalSettings, "Normal Map Settings", true);
        if (showNormalSettings)
        {
            EditorGUI.indentLevel++;
            DrawPropertyToggle(materialEditor, properties, "_EnableNormalIntensity", "Normal Intensity Control");
            if (targetMat.IsKeywordEnabled("_ENABLE_NORMAL_INTENSITY"))
            {
                DrawProperty(materialEditor, properties, "_NormalIntensity");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // Terrain Settings Section
        showTerrainSettings = EditorGUILayout.Foldout(showTerrainSettings, "Terrain Settings", true);
        if (showTerrainSettings)
        {
            EditorGUI.indentLevel++;
            DrawProperty(materialEditor, properties, "_EnableHeightBlend");
            DrawProperty(materialEditor, properties, "_HeightTransition");
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // Advanced Settings Section
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Terrain Properties", true);
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            DrawProperty(materialEditor, properties, "_EnableInstancedPerPixelNormal");
            
            // Terrain Layers
            EditorGUILayout.LabelField("Metallic Values", EditorStyles.label);
            DrawProperty(materialEditor, properties, "_Metallic0");
            DrawProperty(materialEditor, properties, "_Metallic1");
            DrawProperty(materialEditor, properties, "_Metallic2");
            DrawProperty(materialEditor, properties, "_Metallic3");

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Smoothness Values", EditorStyles.label);
            DrawProperty(materialEditor, properties, "_Smoothness0");
            DrawProperty(materialEditor, properties, "_Smoothness1");
            DrawProperty(materialEditor, properties, "_Smoothness2");
            DrawProperty(materialEditor, properties, "_Smoothness3");
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // Preset Buttons
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Retro Preset", GUILayout.Height(30)))
        {
            ApplyRetroPreset(targetMat);
        }
        if (GUILayout.Button("Vivid Preset", GUILayout.Height(30)))
        {
            ApplyVividPreset(targetMat);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Pastel Preset", GUILayout.Height(30)))
        {
            ApplyPastelPreset(targetMat);
        }
        if (GUILayout.Button("Reset", GUILayout.Height(30)))
        {
            ResetToDefaults(targetMat);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        materialEditor.RenderQueueField();
    }

    private void DrawProperty(MaterialEditor editor, MaterialProperty[] properties, string propertyName)
    {
        MaterialProperty prop = FindProperty(propertyName, properties);
        if (prop != null)
        {
            editor.ShaderProperty(prop, prop.displayName);
        }
    }

    private void DrawPropertyToggle(MaterialEditor editor, MaterialProperty[] properties, string propertyName, string label)
    {
        MaterialProperty prop = FindProperty(propertyName, properties);
        if (prop != null)
        {
            Material targetMat = editor.target as Material;
            string keyword = propertyName.Remove(0, 1); // Remove the underscore

            EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(prop, label);
            if (EditorGUI.EndChangeCheck())
            {
                // Toggle keyword based on property value
                if (prop.floatValue > 0.5f)
                {
                    targetMat.EnableKeyword(keyword);
                }
                else
                {
                    targetMat.DisableKeyword(keyword);
                }
            }
        }
    }

    private void ApplyRetroPreset(Material mat)
    {
        mat.SetFloat("_EnableColorPosterize", 1f);
        mat.EnableKeyword("_ENABLE_PSX_COLOR_POSTERIZE");
        mat.SetFloat("_ColorPosterizeSteps", 16f);
        
        mat.SetFloat("_EnableDithering", 1f);
        mat.EnableKeyword("_ENABLE_PSX_DITHERING");
        mat.SetFloat("_DitherPattern", 1f);
        mat.SetFloat("_DitherScale", 2f);
        
        mat.SetFloat("_EnableEdgeDetection", 0f);
        mat.DisableKeyword("_ENABLE_PSX_EDGE_DETECTION");
        
        EditorUtility.SetDirty(mat);
    }

    private void ApplyVividPreset(Material mat)
    {
        mat.SetFloat("_EnableColorGrading", 1f);
        mat.EnableKeyword("_ENABLE_COLOR_GRADING");
        mat.SetFloat("_Saturation", 1.5f);
        mat.SetFloat("_Contrast", 1.2f);
        mat.SetFloat("_Brightness", 0.1f);
        
        mat.SetFloat("_EnableColorPosterize", 0f);
        mat.DisableKeyword("_ENABLE_PSX_COLOR_POSTERIZE");
        
        EditorUtility.SetDirty(mat);
    }

    private void ApplyPastelPreset(Material mat)
    {
        mat.SetFloat("_EnableColorGrading", 1f);
        mat.EnableKeyword("_ENABLE_COLOR_GRADING");
        mat.SetFloat("_Saturation", 0.6f);
        mat.SetFloat("_Brightness", 0.2f);
        mat.SetFloat("_Contrast", 0.8f);
        
        mat.SetFloat("_EnableDithering", 0f);
        mat.DisableKeyword("_ENABLE_PSX_DITHERING");
        
        EditorUtility.SetDirty(mat);
    }

    private void ResetToDefaults(Material mat)
    {
        mat.SetFloat("_EnableColorPosterize", 0f);
        mat.DisableKeyword("_ENABLE_PSX_COLOR_POSTERIZE");
        mat.SetFloat("_ColorPosterizeSteps", 16f);
        
        mat.SetFloat("_EnableEdgeDetection", 0f);
        mat.DisableKeyword("_ENABLE_PSX_EDGE_DETECTION");
        mat.SetFloat("_EdgeThreshold", 0.5f);
        mat.SetColor("_EdgeColor", Color.black);
        
        mat.SetFloat("_EnableDithering", 0f);
        mat.DisableKeyword("_ENABLE_PSX_DITHERING");
        mat.SetFloat("_DitherPattern", 0f);
        mat.SetFloat("_DitherScale", 1f);
        
        mat.SetFloat("_EnableCustomLighting", 0f);
        mat.DisableKeyword("_ENABLE_CUSTOM_LIGHTING");
        mat.SetFloat("_LightingContrast", 1f);
        mat.SetFloat("_LightingBrightness", 0f);
        mat.SetFloat("_ShadowThreshold", 0.5f);
        
        mat.SetFloat("_EnableColorGrading", 0f);
        mat.DisableKeyword("_ENABLE_COLOR_GRADING");
        mat.SetFloat("_Saturation", 1f);
        mat.SetFloat("_Hue", 0f);
        mat.SetFloat("_Contrast", 1f);
        mat.SetFloat("_Brightness", 0f);
        
        mat.SetFloat("_EnableNormalIntensity", 0f);
        mat.DisableKeyword("_ENABLE_NORMAL_INTENSITY");
        mat.SetFloat("_NormalIntensity", 1f);
        
        EditorUtility.SetDirty(mat);
    }
}
#endif