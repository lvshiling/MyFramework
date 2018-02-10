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
    public enum eResLoadMode
    {
        Editor,
        Bundle,
    }

    public class ResManager
    {
        public static ResManager Instance = new ResManager();

        public eResLoadMode ResLoadMode { get; private set; }

        private Dictionary<string,ResConfig> m_res_config = new Dictionary<string, ResConfig>();

        private Dictionary<string,ResData> m_res_datas = new Dictionary<string, ResData>();

        private Dictionary<string, string> m_res_bundle_path = new Dictionary<string, string>();

        private string m_persistent_res_list = string.Empty;

        public void Init( eResLoadMode _mode )
        {
            m_persistent_res_list = string.Format( "{0}/{1}", Application.persistentDataPath, "res_list" );
            ResLoadMode = _mode;
            if( ResLoadMode == eResLoadMode.Bundle )
            {
                LoadResList();
            }
        }

        public void LoadResList()
        {
            if ( File.Exists( m_persistent_res_list ))
            {
                Debug.Log( "开始从persistent_path加载res_list" );
                Byte[] bytes;
                using ( FileStream file_stream = new FileStream( m_persistent_res_list, FileMode.Open, FileAccess.Read ) )
                {
                    bytes = new Byte[file_stream.Length];
                    Int32 num_bytes_to_read = (Int32)file_stream.Length;
                    Int32 num_bytes_read = 0;
                    while ( num_bytes_to_read > 0 )
                    {
                        Int32 n = file_stream.Read( bytes, num_bytes_read, num_bytes_to_read );
                        if ( n == 0 )
                            break;
                        num_bytes_read += n;
                        num_bytes_to_read -= n;
                    }
                }
                Deserialize( bytes, m_res_config, m_res_bundle_path );
            }
            else
            {
                Debug.Log( "开始从streaming_path加载res_list" );
                AssetBundle ab = AssetBundle.LoadFromFile( string.Format( "{0}/{1}", Application.streamingAssetsPath, "res_list.assetbundle" ) );
                if ( ab == null )
                    return;
                TextAsset text = ab.LoadAsset<TextAsset>( "res_list" );
                Deserialize( text.bytes, m_res_config, m_res_bundle_path );
                SaveResList( text.bytes );
                ab.Unload( true );
            }
        }

        public void SaveResList( byte[] _bytes )
        {
            if ( !Directory.Exists( Path.GetDirectoryName( m_persistent_res_list ) ))
            {
                Directory.CreateDirectory( Path.GetDirectoryName( m_persistent_res_list ) );
            }
            FileStream writer = new FileStream( m_persistent_res_list, FileMode.OpenOrCreate );
            writer.Write( _bytes, 0, _bytes.Length );
            writer.Flush();
            writer.Close();
        }

        public static void Deserialize( byte[] _bytes, Dictionary<string, ResConfig> _res_config, Dictionary<string, string> _res_path )
        {
            try
            {
                using ( MemoryStream memory_stream = new MemoryStream( _bytes ) )
                {
                    using ( StreamReader reader = new StreamReader( memory_stream, Encoding.UTF8 ) )
                    {
                        if ( !Equals( Encoding.UTF8, reader.CurrentEncoding ) )
                        {
                            Debug.LogErrorFormat( "res_list文件 {0} 编码不是UTF-8!", reader.CurrentEncoding.EncodingName );
                            return;
                        }
                        if ( reader.EndOfStream )
                            return;
                        Int32 line_count = 0;
                        while ( !reader.EndOfStream )
                        {
                            String line = reader.ReadLine();
                            if ( String.IsNullOrEmpty( line ) )
                                continue;
                            line_count++;
                            String[] parts = line.Split( '\t' );
                            ResConfig config = new ResConfig();
                            config.BundleName = parts[0];
                            config.Size = Int64.Parse( parts[1], NumberStyles.None );
                            config.Version = uint.Parse( parts[2] );
                            config.Md5 = parts[3];
                            string[] depen = parts[4].Split( ',' );
                            for (int i = 0; i < depen.Length; i++)
                            {
                                if (depen[i] != string.Empty)
                                    config.Dependencies.Add( depen[i] );
                            }
                            string[] asset = parts[5].Split( ',' );
                            for ( int i = 0; i < asset.Length; i++ )
                            {
                                if ( asset[i] != string.Empty )
                                {
                                    config.Assets.Add( asset[i] );
                                    if ( _res_path == null )
                                        continue;
                                    if ( _res_path.ContainsKey( asset[i] ) )
                                    {
                                        Debug.LogErrorFormat( "Asset:{0} 被打入到了多个Bundle {1}", asset[i], config.BundleName );
                                    }
                                    else
                                    {
                                        _res_path.Add( asset[i], config.BundleName );
                                    }
                                }
                            }
                            _res_config.Add( config.BundleName, config );
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                Debug.LogErrorFormat( "解析 res_list 文件失败!\tMessage: {0}\nStackTrace: {1} {2}", e.Message, e.StackTrace, e.ToString() );
            }
        }

        public void LoadAsset( string _asset_path, Action<ResData, UnityEngine.Object> _action, bool async = true )
        {
#if UNITY_EDITOR
            if( ResLoadMode == eResLoadMode.Editor )
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( _asset_path );
                if( _action != null )
                {
                    _action( null, obj );
                }
                return;
            }
#endif
            if ( ResLoadMode == eResLoadMode.Editor )
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

        public void RemoveResData( string _name )
        {
            m_res_datas.Remove( _name );
            Debug.LogFormat( "当前ResData个数：{0}", m_res_datas.Count );
        }

        public Dictionary<string, ResConfig> GetAllConfig()
        {
            return m_res_config;
        }

        public void SetAllConfig( Dictionary<string, ResConfig> _config )
        {
            m_res_config.Clear();
            m_res_config = _config;
        }

        public ResConfig GetResConfig( string _bundle_name )
        {
            if( !m_res_config.ContainsKey( _bundle_name ) )
            {
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

        public ResData GetResData( string _name )
        {
            if( m_res_datas.ContainsKey( _name ) )
                return m_res_datas[_name];
            return null;
        }
    }

}
