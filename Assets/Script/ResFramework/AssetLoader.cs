using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ResFramework
{
    class AssetLoader : MonoBehaviour
    {
        class LoadRequireData
        {
            public ResData Resource;
            public String AssetName;
        }

        public static AssetLoader Instance { get; private set; }

        private Queue<LoadRequireData> m_waiting_asset = new Queue<LoadRequireData>();

        private LinkedList<LoadRequireData> m_loading_asset = new LinkedList<LoadRequireData>();

        private int m_max_loading_count = 3;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad( this );
        }

        public void LoadAssetAsync( ResData _resouce, String _asset_name )
        {
            if( _isWaiting( _resouce, _asset_name ) || _isLoading( _resouce, _asset_name ) )
                return;
            m_waiting_asset.Enqueue( new LoadRequireData() { Resource = _resouce, AssetName = _asset_name } );
        }

        public void LoadAssetSync( ResData _resouce, String _asset_name )
        {
            AssetBundle bundle = _resouce.GetBundle();
            if( bundle == null )
            {
                Debug.LogErrorFormat( "Bundle: {0}为空 不能加载asset: {1}", _resouce.GetResConfig().BundleName, _asset_name );
                return;
            }
            Debug.LogFormat( "开始同步加载asset: {0}", _asset_name );
            UnityEngine.Object asset = bundle.LoadAsset( _asset_name );
            _resouce.OnAssetLoaded( _asset_name, asset );
        }

        public void Update()
        {
            if( m_waiting_asset.Count == 0 || m_loading_asset.Count == m_max_loading_count )
                return;
            Int32 count = m_max_loading_count - m_loading_asset.Count;
            for( int i = 0; i < count; i++ )
            {
                if( m_waiting_asset.Count == 0 )
                    break;
                m_loading_asset.AddLast( m_waiting_asset.Dequeue() );
                StartCoroutine( _loadAsset( m_loading_asset.Last.Value ) );
            }
        }

        private IEnumerator _loadAsset( LoadRequireData _require_data )
        {
            AssetBundle bundle = _require_data.Resource.GetBundle();
            if ( bundle == null )
            {
                Debug.LogErrorFormat( "Bundle: {0}为空 不能加载asset: {1}", _require_data.Resource.GetResConfig().BundleName, _require_data.AssetName );
                m_loading_asset.Remove( _require_data );
                yield break;
            }
            Debug.LogFormat( "开始异步加载asset: {0}", _require_data.AssetName );
            AssetBundleRequest asset_load_request = bundle.LoadAssetAsync( _require_data.AssetName );
            yield return asset_load_request;
            if ( asset_load_request.asset == null )
            {
                Debug.LogErrorFormat( "加载bundle: {0} 中的asset: {1} 失败", _require_data.Resource.GetResConfig().BundleName, _require_data.AssetName );
                m_loading_asset.Remove( _require_data );
                yield break;
            }
            Debug.LogFormat( "asset: {0}异步加载完成", _require_data.AssetName );
            _require_data.Resource.OnAssetLoaded( _require_data.AssetName, asset_load_request.asset );
            m_loading_asset.Remove( _require_data );
        }

        private bool _isWaiting( ResData _resouce, String _asset_name )
        {
            foreach( LoadRequireData load_require_data in m_waiting_asset )
            {
                if( load_require_data.Resource == _resouce && _asset_name == load_require_data.AssetName )
                    return true;
            }
            return false;
        }

        private bool _isLoading( ResData _resouce, String _asset_name )
        {
            foreach( LoadRequireData load_require_data in m_loading_asset )
            {
                if( load_require_data.Resource == _resouce && _asset_name == load_require_data.AssetName )
                    return true;
            }
            return false;
        }
    }
}