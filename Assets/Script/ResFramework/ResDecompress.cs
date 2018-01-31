using System;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

namespace ResFramework
{
    public class ResDecompress
    {
        public static ResDecompress Instance = new ResDecompress();

        private List<string> m_compress_res = new List<string>();

        private int m_compress_count = 0;

        private Action m_complete_action = null;

        public void Init()
        {
            
        }

        public void UnInit()
        {

        }

        public void DecompressAssetBundle( Action _complete )
        {
            Caching.compressionEnabled = false;
            m_compress_res.Clear();
            var configs = ResManager.Instance.GetAllConfig();
            foreach( var res in configs )
            {
                if( Caching.IsVersionCached( res.Value.BundleName, Hash128.Parse( res.Value.Md5 ) ) )
                    continue;
                m_compress_res.Add( res.Value.BundleName );
                Caching.ClearAllCachedVersions( System.IO.Path.GetFileNameWithoutExtension( res.Value.BundleName ) );
            }
            if( m_compress_res.Count > 0 )
            {
                m_compress_count = m_compress_res.Count;
                UIManager.Instance.ShowUI( "ui_loading_panel", "正在解压资源，请稍候！" );
                for( int i = 0; i < m_compress_count; i++ )
                {
                    Debug.LogFormat( "开始解压bundle: {0}", m_compress_res[i] );
                    ResManager.Instance.LoadAssetBundleAndAsset( m_compress_res[i], String.Empty, _onBundleLoaded );
                }
                m_complete_action = _complete;
            }
            else
            {
                _complete();
            }
        }

        private void _onBundleLoaded( ResData _data, UnityEngine.Object _object )
        {
            Debug.LogFormat( "bundle: {0}解压完成", _data.GetBundleName() );
            m_compress_count--;
            if( m_compress_count == 0 )
            {
                UIManager.Instance.HideUI( "ui_loading_panel" );
                for( int i = 0; i < m_compress_res.Count; i++ )
                {
                    ResManager.Instance.UnloadAssetBundle( m_compress_res[i], true );
                }
                m_compress_res.Clear();
                m_complete_action();
            }
        }
    }

}
