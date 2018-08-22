using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace ResFramework
{
    public class AssetBundleBuilder
    {
        public static string Path = "Assets/StreamingAssets";
        public static string ResListPath = "Assets/res_list.txt";

        public static BuildAssetBundleOptions Options = BuildAssetBundleOptions.None | BuildAssetBundleOptions.DisableWriteTypeTree;

#if UNITY_STANDALONE
    public static BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
        public static BuildTarget TargetPlatform = BuildTarget.Android;
#elif UNITY_IOS
    public static BuildTarget TargetPlatform = BuildTarget.iOS;
#endif

        private static Dictionary<string, Int32> m_res_ref = new Dictionary<string, int>();

        public static void ClearAllAssetBundleName()
        {
            string[] names = AssetDatabase.GetAllAssetBundleNames();
            for( int i = 0; i < names.Length; i++ )
            {
                AssetDatabase.RemoveAssetBundleName( names[i], true );
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        [MenuItem( "打包/打包所有" )]
        public static void Build()
        {
            long begin = System.DateTime.Now.Ticks;
            ClearAllAssetBundleName();
            List<AssetBundleBuild> builds = BuildRule.GetBuilds();
            if( builds == null || builds.Count == 0 )
                return;
            BuildPipeline.BuildAssetBundles( Path, builds.ToArray(), Options, TargetPlatform );
            BuildResList( null );
            TimeSpan elapsedSpan = new TimeSpan( System.DateTime.Now.Ticks - begin );
            Debug.LogFormat( "打包所有完成 用时{0}秒", elapsedSpan.TotalSeconds );
        }

        //打包单个只是为了方便修改某个资源后不用打全包就能马上在手机上测试 但是没有收集依赖 
        [MenuItem( "打包/打包选中" )]
        public static void BuildSelected()
        {
            UnityEngine.Object[] objs = Selection.GetFiltered<UnityEngine.Object>( SelectionMode.DeepAssets );
            if( objs == null || objs.Length == 0 )
            {
                EditorUtility.DisplayDialog( "提示", "请选中要打包的资源", "确定" );
                return;
            }
            if( File.Exists( string.Format( "{0}/{1}", Application.dataPath, ResListPath ) ) )
            {
                EditorUtility.DisplayDialog( "提示", "当前没有完整的res_list请先打个全包！", "确定" );
                return;
            }
            Dictionary<string, ResConfig> res_config = new Dictionary<string, ResConfig>();
            Dictionary<string, string> res_path = new Dictionary<string, string>();
            using( FileStream file_stream = new FileStream( ResListPath, FileMode.Open, FileAccess.Read ) )
            {
                Byte[] bytes = new Byte[file_stream.Length];
                file_stream.Read( bytes, 0, (int)file_stream.Length );
                ResManager.Deserialize( bytes, res_config, res_path );
            }
            List<AssetBundleBuild> res_build = new List<AssetBundleBuild>();
            for( int i = 0; i < objs.Length; ++i )
            {
                string path = AssetDatabase.GetAssetPath( objs[i] );
                AssetImporter import = AssetImporter.GetAtPath( path );
                if( import == null )
                {
                    Debug.LogErrorFormat( "{0} 不是AssetImporter不能打包！", path );
                    continue;
                }
                string name;
                if( res_path.ContainsKey( path ) )
                {
                    name = res_path[path];
                }
                else
                {
                    name = string.Format( "temp/{0}/{1}.assetbundle", System.IO.Path.GetDirectoryName( path ), System.IO.Path.GetFileNameWithoutExtension( path ) );
                }
                res_build.Add( new AssetBundleBuild() { assetBundleName = name, assetNames = new[] { path } } );
            }
            if( res_build.Count == 0 )
            {
                Debug.LogErrorFormat( "没有满足打包条件的资源 打包失败！" );
                return;
            }
            ClearAllAssetBundleName();
            BuildPipeline.BuildAssetBundles( Path, res_build.ToArray(), Options, TargetPlatform );
            BuildResList( res_config );
            Debug.Log( "打包选中完成" );
        }

        [MenuItem( "打包/清空缓存" )]
        public static void ClearCache()
        {
            Caching.ClearCache();
            File.Delete( Application.persistentDataPath + "/res_list" );
        }

        public static void BuildResList( Dictionary<string, ResConfig> _res_list )
        {
            AssetBundle ab = AssetBundle.LoadFromFile( string.Format( "{0}/StreamingAssets", Application.streamingAssetsPath ) );
            AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" );
            string[] bundles = manifest.GetAllAssetBundles();
            if( _res_list == null )
                _res_list = new Dictionary<string, ResConfig>();
            for( int i = 0; i < bundles.Length; i++ )
            {
                ResConfig config;
                if( _res_list.ContainsKey( bundles[i] ) )
                {
                    config = _res_list[bundles[i]];
                    config.Md5 = manifest.GetAssetBundleHash( bundles[i] ).ToString();
                }
                else
                {
                    config = new ResConfig();
                    config.BundleName = bundles[i];
                    config.Md5 = manifest.GetAssetBundleHash( bundles[i] ).ToString();
                    string[] depen = manifest.GetAllDependencies( bundles[i] );
                    config.Dependencies.AddRange( depen );
                    config.Size = GetAssetBundleSize( string.Format( "{0}/{1}", Application.streamingAssetsPath, bundles[i] ) );
                    if( config.Size == 0 )
                    {
                        Debug.LogErrorFormat( "AB包 {0}的大小居然为0 请检查！", bundles[i] );
                    }
                    string[] paths = BuildRule.GetAssetPathByBundle( config.BundleName );
                    //string[] paths = AssetDatabase.GetAssetPathsFromAssetBundle( config.BundleName );
                    for( int j = 0; j < paths.Length; j++ )
                    {
                        config.Assets.Add( paths[j] );
                    }
                    _res_list.Add( config.BundleName, config );
                }
            }
            ab.Unload( true );
            SaveResList( ResListPath, _res_list );
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles( Path,
                                            new AssetBundleBuild[] { new AssetBundleBuild() { assetBundleName = "res_list.assetbundle", assetNames = new[] { ResListPath } } },
                                            Options, TargetPlatform );
            if( File.Exists( string.Format( "{0}/StreamingAssets", Application.streamingAssetsPath ) ) )
                File.Delete( string.Format( "{0}/StreamingAssets", Application.streamingAssetsPath ) );
            var alls = GetFiles( new DirectoryInfo( Path ) );
            foreach( var file in alls )
            {
                if( file.EndsWith( ".manifest" ) )
                    File.Delete( file );
            }
            File.Delete( Application.persistentDataPath + "/res_list" );
            AssetDatabase.Refresh();
        }

        public static Int64 GetAssetBundleSize(String _file_path)
        {
            if( !File.Exists( _file_path ) )
                throw new FileNotFoundException( String.Format( "file {0} not exist!", _file_path ) );
            using( FileStream file_stream = new FileStream( _file_path, FileMode.Open, FileAccess.Read ) )
            {
                return file_stream.Length;
            }
        }

        public static void SaveResList(string _path, Dictionary<string, ResConfig> _res_list)
        {
            using( FileStream file_stream = new FileStream( _path, FileMode.Create ) )
            {
                StringBuilder stringbuilder = new StringBuilder( 102400 );
                foreach( var res in _res_list )
                {
                    stringbuilder.AppendFormat( "{0}\t", res.Value.BundleName );
                    stringbuilder.AppendFormat( "{0}\t", res.Value.Size );
                    stringbuilder.AppendFormat( "{0}\t", res.Value.Version );
                    stringbuilder.AppendFormat( "{0}\t", res.Value.Md5 );
                    foreach( var depen in res.Value.Dependencies )
                    {
                        stringbuilder.AppendFormat( "{0},", depen );
                    }
                    stringbuilder.Append( "\t" );
                    foreach( var asset in res.Value.Assets )
                    {
                        stringbuilder.AppendFormat( "{0},", asset );
                    }
                    stringbuilder.AppendLine();
                }
                Byte[] bytes = Encoding.UTF8.GetBytes( stringbuilder.ToString() );
                file_stream.Write( bytes, 0, bytes.Length );
                file_stream.Flush();
            }
        }

        static List<string> GetFiles(DirectoryInfo dir)
        {
            List<string> ret = new List<string>();
            DirectoryInfo[] subs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();

            foreach( FileInfo file in files )
            {
                if( file.Extension.EndsWith( "meta" ) )
                    continue;
                ret.Add( file.FullName.Replace( "\\", "/" ) );
            }

            foreach( DirectoryInfo sub in subs )
            {
                ret.AddRange( GetFiles( sub ) );
            }

            return ret;
        }

        public static string LuaDir = "Assets/test/";
        //public static string LuaDir = "Assets/Script/LuaScript/";
        public static string LuaGenDir = "Assets/LuaScriptTxt/";
        //最开始所有lua都是.lua 但是不能对.lua打包 所以打包时转换为.txt但是发现转换后打包出来的包的hash都变了 所以就干脆让所有lua都是.txt格式算了
        //[MenuItem( "打包/生成lua文本格式" )]
        public static void CreateLuaTxt()
        {
            if( Directory.Exists( LuaGenDir ) )
                Directory.Delete( LuaGenDir, true );
            Directory.CreateDirectory( LuaGenDir );

            List<string> luas = GetFiles( new DirectoryInfo( LuaDir ) );
            float step = luas.Count > 0 ? ( 1.0f / luas.Count ) : 1.0f;
            float prog = 0.0f;

            foreach( string lua in luas )
            {
                prog += step;
                EditorUtility.DisplayProgressBar( "Copy LUA", lua, prog );

                string gen = lua.Replace( LuaDir, LuaGenDir ) + ".txt";
                string path = System.IO.Path.GetDirectoryName( gen );
                if( !Directory.Exists( path ) )
                    Directory.CreateDirectory( path );
                File.Copy( lua, gen, true );
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        //[MenuItem( "打包/生成lua二进制格式" )]
        public static void CreateLuaByte()
        {
            if( Directory.Exists( LuaGenDir ) )
                Directory.Delete( LuaGenDir, true );
            Directory.CreateDirectory( LuaGenDir );

            List<string> luas = GetFiles( new DirectoryInfo( LuaDir ) );
            float step = luas.Count > 0 ? ( 1.0f / luas.Count ) : 1.0f;
            float prog = 0.0f;

            int length = Application.dataPath.Length;
            foreach( string lua in luas )
            {
                prog += step;
                EditorUtility.DisplayProgressBar( "Copy LUA", lua, prog );

                string gen = lua.Replace( LuaDir, LuaGenDir ) + ".bytes";
                string path = System.IO.Path.GetDirectoryName( gen );
                if( !Directory.Exists( path ) )
                    Directory.CreateDirectory( path );

                FileStream fs = new FileStream( gen, FileMode.OpenOrCreate );
                BinaryWriter binWriter = new BinaryWriter( fs );
                binWriter.Write( System.IO.File.ReadAllBytes( lua ), 0, 100 );

                binWriter.Close();
                fs.Close();
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        //[MenuItem( "打包/打包lua" )]
        public static void PackLua()
        {
            List<string> luas = GetFiles( new DirectoryInfo( LuaGenDir ) );
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            int length = Application.dataPath.Length;
            foreach( string lua in luas )
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = string.Format( "lua/{0}.assetbundle", System.IO.Path.GetFileNameWithoutExtension( lua ) );
                build.assetNames = new string[] { string.Format( "Assets{0}", lua.Substring( length ) ) };
                builds.Add( build );
            }

            BuildPipeline.BuildAssetBundles( Path, builds.ToArray(), Options, TargetPlatform );
        }
    }
}
