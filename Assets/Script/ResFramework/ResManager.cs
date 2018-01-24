using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ResFramework
{
    public enum ResLoadMode
    {
        ResLoadMode_Editor,
        ResLoadMode_Bundle,
    }

    public class ResManager
    {
        public static ResManager Instance = new ResManager();

        private ResLoadMode m_res_load_mode = ResLoadMode.ResLoadMode_Bundle;

        private Dictionary<string,ResConfig> m_res_config = new Dictionary<string, ResConfig>();

        private Dictionary<string,ResData> m_res_datas = new Dictionary<string, ResData>();

        private Dictionary<string, string> m_res_bundle_path = new Dictionary<string, string>();

        public void Init( ResLoadMode _mode )
        {
            m_res_load_mode = _mode;
            if( m_res_load_mode == ResLoadMode.ResLoadMode_Bundle )
            {
                LoadResList();
            }
        }

        public void LoadResList()
        {
            AssetBundle ab = AssetBundle.LoadFromFile( string.Format( "{0}/{1}", Application.streamingAssetsPath, "res_list.assetbundle" ) );
            TextAsset text = ab.LoadAsset<TextAsset>( "res_list" );
            try
            {
                using( MemoryStream memory_stream = new MemoryStream( text.bytes ) )
                {
                    using( StreamReader reader = new StreamReader( memory_stream, Encoding.UTF8 ) )
                    {
                        if( !Equals( Encoding.UTF8, reader.CurrentEncoding ) )
                        {
                            Debug.LogErrorFormat( "res_list文件 {0} 编码不是UTF-8!", reader.CurrentEncoding.EncodingName );
                            return;
                        }
                        if( reader.EndOfStream )
                            return;
                        Int32 line_count = 0;
                        while( !reader.EndOfStream )
                        {
                            String line = reader.ReadLine();
                            if( String.IsNullOrEmpty( line ) )
                                continue;
                            line_count++;
                            String[] parts = line.Split( '\t' );
                            ResConfig config = new ResConfig();
                            config.BundleName = parts[0];
                            config.Size = Int64.Parse( parts[1], NumberStyles.None );
                            config.Version = uint.Parse( parts[2] );
                            config.Md5 = parts[3];
                            string[] depen = parts[4].Split( ',' );
                            for( int i = 0; i < depen.Length; i++ )
                            {
                                if( depen[i] != string.Empty )
                                    config.Dependencies.Add( depen[i] );
                            }
                            string[] asset = parts[5].Split( ',' );
                            for( int i = 0; i < asset.Length; i++ )
                            {
                                if( asset[i] != string.Empty )
                                {
                                    config.Assets.Add( asset[i] );
                                    if( m_res_bundle_path.ContainsKey( asset[i] ) )
                                    {
                                        Debug.LogErrorFormat( "Asset:{0} 被打入到了多个Bundle {1}", asset[i], config.BundleName );
                                    }
                                    else
                                    {
                                        m_res_bundle_path.Add( asset[i], config.BundleName );
                                    }
                                }
                            }
                            m_res_config.Add( config.BundleName, config );
                        }
                    }
                }
            }
            catch( Exception e )
            {
                Debug.LogErrorFormat( "解析 res_list 文件失败!\tMessage: {0}\nStackTrace: {1} {2}", e.Message, e.StackTrace, e.ToString() );
            }
            ab.Unload( true );
        }

        public void LoadAsset( string _asset_path, Action<ResData, UnityEngine.Object> _action, bool async = true )
        {
#if UNITY_EDITOR
            if( m_res_load_mode == ResLoadMode.ResLoadMode_Editor )
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( _asset_path );
                if( _action != null )
                {
                    _action( null, obj );
                }
                return;
            }
#endif
            if ( m_res_load_mode == ResLoadMode.ResLoadMode_Editor )
                return;
            if( !m_res_bundle_path.ContainsKey( _asset_path ) )
            {
                Debug.LogErrorFormat( "加载Asset失败， {0}没有对应的Bundle！" );
                return;
            }
            LoadAssetBundleAndAsset( m_res_bundle_path[_asset_path], _asset_path, _action, async );
        }

        public void LoadAssetBundleAndAsset( string _bundle_name, string _asset_name, Action<ResData, UnityEngine.Object> _action, bool async = true )
        {
            ResConfig config = GetResConfig( _bundle_name );
            if( config == null )
                return;
            ResData data = GetResData( config, true );
            data.LoadAssetBundle( _asset_name, _action, async );
        }

        public void UnloadAssetBundle( string _bundle_name, bool _all_asset = false )
        {
            if( m_res_datas.ContainsKey( _bundle_name ) )
            {
                m_res_datas[_bundle_name].Unload( _all_asset );
                m_res_datas.Remove( _bundle_name );
            }
        }

        public Dictionary<string, ResConfig> GetAllConfig()
        {
            return m_res_config;
        }

        public ResConfig GetResConfig( string _bundle_name )
        {
            if( !m_res_config.ContainsKey( _bundle_name ) )
            {
                Debug.LogErrorFormat( "ResConfig中没有{0}", _bundle_name );
                return null;
            }
            return m_res_config[_bundle_name];
        }

        public ResData GetResData( ResConfig _config, bool _create = false )
        {
            if( m_res_datas.ContainsKey( _config.BundleName ) )
                return m_res_datas[_config.BundleName];
            if( _create )
            {
                ResData data = new ResData( _config );
                m_res_datas[_config.BundleName] = data;
                return data;
            }
            return null;
        }
    }

}
