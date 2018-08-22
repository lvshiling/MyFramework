using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ResFramework
{
    public class AssetBundleBuilderPanel : EditorWindow
    {
        public static string BuildRuleConfigPath = "Assets/build_rule_config.asset";

        private BuildRuleConfig m_rule_config = null;

        private ReorderableList m_scroll_list;

        private Vector2 m_scroll_pos = Vector2.zero;

        [MenuItem("打包/打包配置面板")]
        private static void _openPanel()
        {
            GetWindow<AssetBundleBuilderPanel>( "打包配置面板", true );
        }

        AssetBundleBuilderPanel()
        {
        }

        private void _loadConfig()
        {
            m_rule_config = AssetDatabase.LoadAssetAtPath<BuildRuleConfig>( BuildRuleConfigPath );
            if( m_rule_config == null )
            {
                m_rule_config = CreateInstance<BuildRuleConfig>();
                AssetDatabase.CreateAsset( m_rule_config, BuildRuleConfigPath );
            }
        }

        private void _initReorderableList()
        {
            m_scroll_list = new ReorderableList( m_rule_config.Filters, typeof( BuildRuleFilter ) );
            m_scroll_list.drawHeaderCallback = _onHeaderGUI;
            m_scroll_list.onAddCallback = _addReorderableItem;
            m_scroll_list.drawElementCallback = _onListElementGUI;
        }

        private void _onHeaderGUI( Rect rect )
        {
            const float GAP = 5;
            Rect r = rect;
            r.xMin = rect.xMin + 12;
            r.width = 100;
            EditorGUI.LabelField( r, "打包内容" );

            r.xMin = r.xMax + GAP;
            r.width = 300;
            EditorGUI.LabelField( r, "资源路径" );

            r.xMin = r.xMax + GAP;
            r.width = 50;
            EditorGUI.LabelField( r, "修改路径" );

            r.xMin = r.xMax + GAP;
            r.width = 100;
            EditorGUI.LabelField( r, "打包资源类型" );

            r.xMin = r.xMax + GAP;
            r.width = 218;
            EditorGUI.LabelField( r, "打包类型" );

            r.xMin = r.xMax + GAP;
            r.width = 100;
            EditorGUI.LabelField( r, "包名" );

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            EditorGUI.LabelField( r, "查询方式" );
        }

        private void _onListElementGUI( Rect rect, int index, bool isactive, bool isfocused )
        {
            const float GAP = 5;

            BuildRuleFilter filter = m_rule_config.Filters[index];
            rect.y++;

            Rect r = rect;
            r.height = 18;
            r.xMin = rect.xMin;
            r.width = 100;
            filter.des = EditorGUI.TextField( r, filter.des );

            r.xMin = r.xMax + GAP;
            r.width = 300;
            GUI.enabled = false;
            filter.searchPath = EditorGUI.TextField( r, filter.searchPath );
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.width = 50;
            if( GUI.Button( r, "Select" ) )
            {
                var path = _selectFolder();
                if( path != null )
                    filter.searchPath = path;
            }

            r.xMin = r.xMax + GAP;
            r.width = 100;
            BuildAssetType type = (BuildAssetType)Enum.Parse( typeof( BuildAssetType ), filter.searchPattern.Substring( 2 ) );
            type = (BuildAssetType)EditorGUI.EnumPopup( r, type );
            filter.searchPattern = "*." + type.ToString();

            r.xMin = r.xMax + GAP;
            r.width = 218;
            filter.type = (BuildRuleType)EditorGUI.EnumPopup( r, (Enum)filter.type );

            r.xMin = r.xMax + GAP;
            r.width = 100;
            GUI.enabled = filter.type == BuildRuleType.BuildAssetsWithAssetBundleName;
            filter.bundleName = EditorGUI.TextField( r, filter.bundleName );
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            filter.searchOption = (SearchOption)EditorGUI.EnumPopup( r, (Enum)filter.searchOption );
        }

        private string _selectFolder()
        {
            string dataPath = Application.dataPath;
            string selectedPath = EditorUtility.OpenFolderPanel( "Path", dataPath, "" );
            if( !string.IsNullOrEmpty( selectedPath ) )
            {
                if( selectedPath.StartsWith( dataPath ) )
                {
                    return "Assets/" + selectedPath.Substring( dataPath.Length + 1 );
                }
                else
                {
                    ShowNotification( new GUIContent( "不能在Assets目录之外!" ) );
                }
            }
            return null;
        }

        private void _addReorderableItem( ReorderableList _list )
        {
            string path = _selectFolder();
            if( !string.IsNullOrEmpty( path ) )
            {
                BuildRuleFilter config = new BuildRuleFilter();
                config.searchPath = path;
                m_rule_config.Filters.Add( config );
            }
        }

        private void OnGUI()
        {
            if( m_rule_config == null )
            {
                _loadConfig();
            }
            if( m_scroll_list == null )
            {
                _initReorderableList();
            }
            m_scroll_pos = EditorGUILayout.BeginScrollView( m_scroll_pos );
            {
                m_scroll_list.DoLayoutList();
            }
            EditorGUILayout.EndScrollView();

            if( GUI.changed )
                EditorUtility.SetDirty( m_rule_config );
        }
    }
}
