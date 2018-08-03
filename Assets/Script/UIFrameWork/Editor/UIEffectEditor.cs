using UnityEditor;
using UnityEngine;

namespace UIFramework
{
    [CustomEditor( typeof( UIEffect ) )]
    [CanEditMultipleObjects]
    public class UIEffectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bool changed = false;

            var tone_mode = serializedObject.FindProperty( "m_tone_mode" );
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField( tone_mode );
            changed |= EditorGUI.EndChangeCheck();
            if( tone_mode.intValue != (int)UIEffect.ToneMode.None )
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "m_tone_power" ) );
                EditorGUI.indentLevel--;
            }

            var pixel_mode = serializedObject.FindProperty( "m_pixel_mode" );
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField( pixel_mode );
            changed |= EditorGUI.EndChangeCheck();
            if( pixel_mode.intValue != (int)UIEffect.PixelMode.None )
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "m_pixel_power" ) );
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
