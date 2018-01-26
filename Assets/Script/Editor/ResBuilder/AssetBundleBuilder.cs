using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ResFramework;
using UnityEngine;
using UnityEditor;

public class AssetBundleBuilder
{
    public static string Path = "Assets/StreamingAssets";
    public static string ResListPath = "Assets/res_list.txt";

    public static string LuaDir = "Assets/Script/LuaScript/";
    public static string LuaGenDir = "Assets/LuaScriptTxt/";

    public static BuildAssetBundleOptions Options = BuildAssetBundleOptions.None;

#if UNITY_STANDALONE
    public static BuildTarget TargetPlatform = BuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
    public static BuildTarget TargetPlatform = BuildTarget.Android;
#elif UNITY_IOS
    public static BuildTarget TargetPlatform = BuildTarget.iOS;
#endif

    private static Dictionary<string,Int32> m_res_ref = new Dictionary<string, int>();

    public static void BuildUi()
    {
        if( !Directory.Exists( "Assets/res/ui/prefab" ) )
        {
            Debug.LogError( "ui prefab 文件夹不存在" );
            return;
        }
        ClearAllAssetBundleName();
        m_res_ref.Clear();
        string[] ui_prefabs = Directory.GetFiles( "Assets/res/ui/prefab" );
        for( Int32 i = 0; i < ui_prefabs.Length; ++i )
        {
            if( !ui_prefabs[i].EndsWith( ".prefab" ) )
                continue;
            var ai = AssetImporter.GetAtPath( ui_prefabs[i] );
            if( ai == null )
                continue;
            string name = ai.assetPath.Replace( "Assets/res/ui/prefab/", string.Empty );
            name = name.Replace( ".prefab", string.Empty );
            ai.assetBundleName = string.Format( "ui/prefab_{0}.assetbundle", name );
            string[] dependencies = AssetDatabase.GetDependencies( ui_prefabs[i], false );
            for( int j = 0; j < dependencies.Length; j++ )
            {
                if( dependencies[j].EndsWith( ".cs" ) )
                    continue;
                if( m_res_ref.ContainsKey( dependencies[j] ) )
                    m_res_ref[dependencies[j]] += 1;
                else
                {
                    m_res_ref.Add( dependencies[j], 1 );
                }
            }
        }
        foreach( var res_ref in m_res_ref )
        {
            if ( res_ref.Value <= 1 )
                continue;
            var ai = AssetImporter.GetAtPath( res_ref.Key );
            if ( ai == null )
                continue;
            if ( ai.assetPath.EndsWith( ".fontsettings" ) )
            {
                ai.assetBundleName = "ui/font_image.assetbundle";
            }
            else if( ai.assetPath.EndsWith( ".ttf" ) )
            {
                ai.assetBundleName = "ui/font_common.assetbundle";
            }
            else if( ai.assetPath.EndsWith( ".controller" ) )
            {
                ai.assetBundleName = "ui/animator.assetbundle";
            }
            else if( ai.assetPath.EndsWith( ".mat" ) )
            {
                string[] dependencies = AssetDatabase.GetDependencies( ai.assetPath, false );
                for( int i = 0; i < dependencies.Length; i++ )
                {
                    if( dependencies[i].EndsWith( ".png" ) || ai.assetPath.EndsWith( ".jpg" ) )
                    {
                        var sub_ai = AssetImporter.GetAtPath( dependencies[i] );
                        string name = sub_ai.assetPath.Remove( 0, sub_ai.assetPath.LastIndexOf( "/" ) + 1 );
                        name = name.Remove( name.LastIndexOf( "." ) );
                        sub_ai.assetBundleName = string.Format( "ui/texture_{0}.assetbundle", name );
                    }
                }
            }
            else if( ai.assetPath.EndsWith( ".png" ) || ai.assetPath.EndsWith( ".jpg" ) )
            {
                string name = ai.assetPath.Remove( 0, ai.assetPath.LastIndexOf( "/" ) + 1 );
                name = name.Remove( name.LastIndexOf( "." ) );
                ai.assetBundleName = string.Format( "ui/texture_{0}.assetbundle", name );
            }
        }
        BuildPipeline.BuildAssetBundles( Path, Options, TargetPlatform );
    }

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
        //CreateLuaTxt();
        BuildPipeline.BuildAssetBundles( Path, Options, TargetPlatform );
        BuildResList();
        //Directory.Delete( LuaGenDir, true );
        //AssetDatabase.Refresh();
    }

    [MenuItem( "打包/清空缓存" )]
    public static void ClearCache()
    {
        Caching.ClearCache();
    }

    public static void CreateLuaTxt()
    {
        if ( Directory.Exists( LuaGenDir ) )
            Directory.Delete( LuaGenDir, true );
        Directory.CreateDirectory( LuaGenDir );

        List<string> luas = GetFiles( new DirectoryInfo( LuaDir ) );
        float step = luas.Count > 0 ? ( 1.0f / luas.Count ) : 1.0f;
        float prog = 0.0f;

        List<string> res = new List<string>();
        foreach( string lua in luas )
        {
            prog += step;
            EditorUtility.DisplayProgressBar( "Copy LUA", lua, prog );

            string gen = lua.Replace( LuaDir, LuaGenDir ) + ".txt";
            string path = System.IO.Path.GetDirectoryName( gen );
            if ( !Directory.Exists( path ) )
                Directory.CreateDirectory( path );
            File.Copy( lua, gen, true );
            res.Add( gen );
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        int length = 0;
        foreach( string lua in res )
        {
            if ( length == 0 )
                length = lua.IndexOf( LuaGenDir );
            AssetImporter import = AssetImporter.GetAtPath( lua.Substring( length ) );
            string bundle_name = lua.Substring( length + LuaGenDir.Length );
            bundle_name = bundle_name.Replace( ".lua.txt", "" );
            import.assetBundleName = string.Format( "lua/{0}.assetbundle", bundle_name );
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
    
    public static void BuildResList()
    {
        AssetBundle ab = AssetBundle.LoadFromFile( string.Format( "{0}/StreamingAssets", Application.streamingAssetsPath ) );
        AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" );
        string[] bundles = manifest.GetAllAssetBundles();
        List<ResConfig> res_list = new List<ResConfig>();
        for( int i = 0; i < bundles.Length; i++ )
        {
            ResConfig config = new ResConfig();
            config.BundleName = bundles[i];
            config.Md5 = manifest.GetAssetBundleHash( bundles[i] ).ToString();
            string[] depen = manifest.GetAllDependencies( bundles[i] );
            config.Dependencies.AddRange( depen );
            config.Size = GetAssetBundleSize( string.Format( "{0}/{1}", Application.streamingAssetsPath, bundles[i] ) );
            if( config.Size == 0 )
            {
                Debug.LogErrorFormat( "AB包 {0}的大小居然为0 请检查！", bundles[i] );
            }
            string[] paths = AssetDatabase.GetAssetPathsFromAssetBundle( bundles[i] );
            for( int j = 0; j < paths.Length; j++ )
            {
                //string[] array = paths[j].Split( '/' );
                config.Assets.Add( paths[j] );
            }
            res_list.Add( config );
        }
        ab.Unload( true );
        SaveResList( ResListPath, res_list );
        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles( Path,
                                        new AssetBundleBuild[] { new AssetBundleBuild() { assetBundleName = "res_list.assetbundle", assetNames = new[] { ResListPath } } },
                                        Options, TargetPlatform );
        if( File.Exists( string.Format( "{0}/StreamingAssets", Application.streamingAssetsPath ) ) )
            File.Delete( string.Format( "{0}/StreamingAssets", Application.streamingAssetsPath ) );
        if( File.Exists( string.Format( "{0}/StreamingAssets.manifest", Application.streamingAssetsPath ) ) )
            File.Delete( string.Format( "{0}/StreamingAssets.manifest", Application.streamingAssetsPath ) );
        AssetDatabase.Refresh(); 
    }

    public static Int64 GetAssetBundleSize( String _file_path )
    {
        if( !File.Exists( _file_path ) )
            throw new FileNotFoundException( String.Format( "file {0} not exist!", _file_path ) );
        using( FileStream file_stream = new FileStream( _file_path, FileMode.Open, FileAccess.Read ) )
        {
            return file_stream.Length;
        }
    }

    public static void SaveResList( string _path, List<ResConfig> _res_list )
    {
        using( FileStream file_stream = new FileStream( _path, FileMode.Create ) )
        {
            StringBuilder stringbuilder = new StringBuilder( 102400 );
            foreach( ResConfig res in _res_list )
            {
                stringbuilder.AppendFormat( "{0}\t", res.BundleName );
                stringbuilder.AppendFormat( "{0}\t", res.Size );
                stringbuilder.AppendFormat( "{0}\t", res.Version );
                stringbuilder.AppendFormat( "{0}\t", res.Md5 );
                foreach( var depen in res.Dependencies )
                {
                    stringbuilder.AppendFormat( "{0},", depen );
                }
                stringbuilder.Append( "\t" );
                foreach( var asset in res.Assets )
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

    static List<string> GetFiles( DirectoryInfo dir )
    {
        List<string> ret = new List<string>();
        DirectoryInfo[] subs = dir.GetDirectories();
        FileInfo[] files = dir.GetFiles();

        foreach( FileInfo file in files )
        {
            if (file.Extension.EndsWith( "meta" ))
                continue;
            ret.Add( file.FullName.Replace( "\\", "/" ) );
        }

        foreach( DirectoryInfo sub in subs )
        {
            ret.AddRange( GetFiles( sub ) );
        }

        return ret;
    }
}
