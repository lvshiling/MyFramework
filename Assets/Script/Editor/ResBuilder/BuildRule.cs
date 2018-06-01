using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResFramework
{
    public abstract class BuildRule
    {
        //所有被打包的资源 包括包里所有的资源名字
        protected static List<string> packedAssets = new List<string>();
        protected static List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        static List<BuildRule> rules = new List<BuildRule>();
        //key被依赖的资源名 value引用这个依赖的资源名
        static Dictionary<string, List<string>> allDependencies = new Dictionary<string, List<string>>();

        public string searchPath;
        public string searchPattern;
        public SearchOption searchOption = SearchOption.AllDirectories;
        public string bundleName;

        static BuildRule()
        {

        }

        public static string[] GetAssetPathByBundle( string name )
        {
            List<string> assets = new List<string>();
            for( int i = 0; i < builds.Count; ++i )
            {
                if( builds[i].assetBundleName.Equals( name ) )
                {
                    assets.AddRange( builds[i].assetNames );
                }
            }
            return assets.ToArray();
        }

        public static List<AssetBundleBuild> GetBuilds()
        {
            packedAssets.Clear();
            builds.Clear();
            rules.Clear();
            allDependencies.Clear();

            const string rulesini = "Assets/res_rules.txt";
            if( File.Exists( rulesini ) )
            {
                LoadRules( rulesini );
            }
            else
            {
                EditorUtility.DisplayDialog( "提示", "打包配置不存在！", "确定" );
                return null;
            }

            foreach( var item in rules )
            {
                CollectDependencies( GetFilesWithoutDirectories( item.searchPath, item.searchPattern, item.searchOption ) );
            }

            foreach( var item in rules )
            {
                item.Build();
            }

            BuildDependenciesAssets();

            EditorUtility.ClearProgressBar();

            return builds;
        }

        static void LoadRules( string rulesini )
        {
            using( var s = new StreamReader( rulesini ) )
            {
                rules.Clear();

                string line = null;
                while( ( line = s.ReadLine() ) != null )
                {
                    if( line == string.Empty || line.StartsWith( "#", StringComparison.CurrentCulture ) || line.StartsWith( "//", StringComparison.CurrentCulture ) )
                    {
                        continue;
                    }
                    if( line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']' )
                    {
                        var name = line.Substring( 1, line.Length - 2 );
                        var searchPath = s.ReadLine().Split( '=' )[1];
                        var searchPattern = s.ReadLine().Split( '=' )[1];
                        var searchOption = s.ReadLine().Split( '=' )[1];
                        var bundleName = s.ReadLine().Split( '=' )[1];
                        var type = typeof( BuildRule ).Assembly.GetType( "ResFramework." + name );
                        if( type != null )
                        {
                            var rule = Activator.CreateInstance( type ) as BuildRule;
                            rule.searchPath = searchPath;
                            rule.searchPattern = searchPattern;
                            rule.searchOption = (SearchOption)Enum.Parse( typeof( SearchOption ), searchOption );
                            rule.bundleName = bundleName.ToLower();
                            rules.Add( rule );
                        }
                    }
                }
            }
        }

        static List<string> GetFilesWithoutDirectories( string prefabPath, string searchPattern, SearchOption searchOption )
        {
            var files = Directory.GetFiles( prefabPath, searchPattern, searchOption );
            List<string> items = new List<string>();
            foreach( var item in files )
            {
                var assetPath = item.Replace( '\\', '/' );
                if( !Directory.Exists( assetPath ) )
                {
                    items.Add( assetPath );
                }
            }
            return items;
        }

        protected static void BuildDependenciesAssets()
        {
            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            foreach( var item in allDependencies )
            {
                var assetPath = item.Key;
                if( packedAssets.Contains( assetPath ) )
                {
                    continue;
                }
                if( item.Value.Count > 1 )
                {
                    var name = string.Format( "{0}.assetbundle", BuildAssetBundleNameWithAssetPath( assetPath ).Replace( "assets/", string.Empty ) );
                    List<string> list = null;
                    if( !bundles.TryGetValue( name, out list ) )
                    {
                        list = new List<string>();
                        bundles.Add( name, list );
                    }
                    if( !list.Contains( assetPath ) )
                    {
                        list.Add( assetPath );
                        packedAssets.Add( assetPath );
                    }
                }
            }
            foreach( var item in bundles )
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = item.Key;
                build.assetNames = item.Value.ToArray();
                builds.Add( build );
            }
        }

        protected static List<string> GetDependenciesWithoutShared( string item )
        {
            var assets = AssetDatabase.GetDependencies( item );
            List<string> assetNames = new List<string>();
            foreach( var assetPath in assets )
            {
                if( assetPath.Equals( item ) || packedAssets.Contains( assetPath ) || assetPath.EndsWith( ".cs", StringComparison.CurrentCulture ) )
                {
                    continue;
                }
                if( allDependencies[assetPath].Count == 1 )
                {
                    assetNames.Add( assetPath );
                }
            }
            return assetNames;
        }

        protected static void CollectDependencies( List<string> files )
        {
            for( int i = 0; i < files.Count; i++ )
            {
                var item = files[i];
                var dependencies = AssetDatabase.GetDependencies( item );
                if( EditorUtility.DisplayCancelableProgressBar( string.Format( "Collecting... [{0}/{1}]", i, files.Count ), item, i * 1f / files.Count ) )
                {
                    break;
                }

                foreach( var assetPath in dependencies )
                {
                    if( assetPath.EndsWith( ".cs", StringComparison.CurrentCulture ) || assetPath.Equals( item ) )
                        continue;
                    if( !allDependencies.ContainsKey( assetPath ) )
                    {
                        allDependencies[assetPath] = new List<string>();
                    }

                    if( !allDependencies[assetPath].Contains( item ) )
                    {
                        allDependencies[assetPath].Add( item );
                    }
                }
            }
        }

        protected static List<string> GetFilesWithoutPacked( string searchPath, string searchPattern, SearchOption searchOption )
        {
            var files = GetFilesWithoutDirectories( searchPath, searchPattern, searchOption );
            return files;
        }

        protected static string BuildAssetBundleNameWithAssetPath(string assetPath)
        {
            return Path.Combine( Path.GetDirectoryName( assetPath ), Path.GetFileNameWithoutExtension( assetPath ) ).Replace( '\\', '/' ).ToLower();
        }

        protected BuildRule()
        {

        }

        protected BuildRule(string path, string pattern, SearchOption option)
        {
            searchPath = path;
            searchPattern = pattern;
            searchOption = option;
        }

        public abstract void Build();

        public abstract string GetAssetBundleName(string assetPath);
    }

    public class BuildAssetsWithAssetBundleName : BuildRule
    {
        public BuildAssetsWithAssetBundleName()
        {

        }

        public override string GetAssetBundleName(string assetPath)
        {
            return bundleName;
        }

        public BuildAssetsWithAssetBundleName(string path, string pattern, SearchOption option, string assetBundleName) : base( path, pattern, option )
        {
            bundleName = assetBundleName;
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked( searchPath, searchPattern, searchOption );
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = files.ToArray();
            builds.Add( build );
            packedAssets.AddRange( files );
        }
    }

    public class BuildAssetsWithDirectroyName : BuildRule
    {
        public BuildAssetsWithDirectroyName()
        {

        }

        public BuildAssetsWithDirectroyName(string path, string pattern, SearchOption option) : base( path, pattern, option )
        {
        }

        public override string GetAssetBundleName(string assetPath)
        {
            return BuildAssetBundleNameWithAssetPath( Path.GetDirectoryName( assetPath ) );
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked( searchPath, searchPattern, searchOption );

            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            for( int i = 0; i < files.Count; i++ )
            {
                var item = files[i];
                if( EditorUtility.DisplayCancelableProgressBar( string.Format( "Collecting... [{0}/{1}]", i, files.Count ), item, i * 1f / files.Count ) )
                {
                    break;
                }
                var path = Path.GetDirectoryName( item );
                if( !bundles.ContainsKey( path ) )
                {
                    bundles[path] = new List<string>();
                }
                bundles[path].Add( item );
            }

            int count = 0;
            foreach( var item in bundles )
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath( item.Key ) + "_" + item.Value.Count;
                build.assetNames = item.Value.ToArray();
                packedAssets.AddRange( build.assetNames );
                builds.Add( build );
                if( EditorUtility.DisplayCancelableProgressBar( string.Format( "Packing... [{0}/{1}]", count, bundles.Count ), build.assetBundleName, count * 1f / bundles.Count ) )
                {
                    break;
                }
                count++;
            }
        }
    }

    public class BuildAssetsWithFilename : BuildRule
    {
        public BuildAssetsWithFilename()
        {

        }

        public override string GetAssetBundleName(string assetPath)
        {
            return BuildAssetBundleNameWithAssetPath( assetPath );
        }

        public BuildAssetsWithFilename(string path, string pattern, SearchOption option) : base( path, pattern, option )
        {
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked( searchPath, searchPattern, searchOption );

            for( int i = 0; i < files.Count; i++ )
            {
                var item = files[i];
                if( EditorUtility.DisplayCancelableProgressBar( string.Format( "Packing... [{0}/{1}]", i, files.Count ), item, i * 1f / files.Count ) )
                {
                    break;
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = string.Format( "{0}.assetbundle", BuildAssetBundleNameWithAssetPath( item ).Replace( "assets/", string.Empty ) );
                build.assetNames = new[] { item };
                packedAssets.AddRange( build.assetNames );
                builds.Add( build );
            }
        }
    }


    public class BuildAssetsWithScenes : BuildRule
    {
        #region implemented abstract members of BuildRule

        public override string GetAssetBundleName(string assetPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        public BuildAssetsWithScenes()
        {

        }

        public BuildAssetsWithScenes(string path, string pattern, SearchOption option) : base( path, pattern, option )
        {

        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked( searchPath, searchPattern, searchOption );

            for( int i = 0; i < files.Count; i++ )
            {
                var item = files[i];
                if( EditorUtility.DisplayCancelableProgressBar( string.Format( "Packing... [{0}/{1}]", i, files.Count ), item, i * 1f / files.Count ) )
                {
                    break;
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = string.Format( "{0}.assetbundle", BuildAssetBundleNameWithAssetPath( item ).Replace( "assets/", string.Empty ) );
                build.assetNames = new[] { item };
                packedAssets.AddRange( build.assetNames );
                builds.Add( build );
            }
        }
    }
}
