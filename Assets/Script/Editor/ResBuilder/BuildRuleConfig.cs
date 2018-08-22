using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ResFramework
{
    public enum BuildRuleType
    {
        BuildAssetsWithAssetBundleName,
        BuildAssetsWithDirectroyName,
        BuildAssetsWithFilename,
        BuildAssetsWithScenes
    }
    [System.Serializable]
    public class BuildRuleFilter
    {
        public string des = "打包描述";
        public BuildRuleType type;
        public string searchPath;
        public string searchPattern = "*.prefab";
        public SearchOption searchOption = SearchOption.AllDirectories;
        public string bundleName;
    }
    public class BuildRuleConfig : ScriptableObject
    {
        public List<BuildRuleFilter> Filters = new List<BuildRuleFilter>();
    }
}
