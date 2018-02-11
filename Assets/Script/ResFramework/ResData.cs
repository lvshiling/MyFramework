using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ResFramework
{
    public enum ResDataState
    {
        Init,
        LoadingDependencies,
        LoadingSelf,
        BundleLoaded,
        ReadyUnload,
    }

    public class ResData
    {
        class RequestAssetData
        {
            public string Name;
            public bool Async = true;
            public List<Action<ResData, UnityEngine.Object>> CompleteActions = new List<Action<ResData, UnityEngine.Object>>();
        }

        private ResDataState m_state = ResDataState.Init;

        private ResConfig m_res_config = null;

        private AssetBundle m_bundle = null;

        private bool m_bundle_async = true;

        private int m_loaded_dependencies_count = 0;

        private int m_reference_count = 0;

        private Dictionary<string, RequestAssetData> m_request_assets = new Dictionary<string, RequestAssetData>();

        public ResData( ResConfig _config )
        {
            m_state = ResDataState.Init;
            m_res_config = _config;
        }

        public void UnInit()
        {
            m_state = ResDataState.Init;
            if( m_bundle != null )
            {
                m_bundle.Unload( false );
            }
            m_bundle = null;
            m_bundle_async = true;
            m_loaded_dependencies_count = 0;
            m_reference_count = 0;
            m_request_assets.Clear();
            ResManager.Instance.RemoveResData( m_res_config.BundleName );
            m_res_config = null;
        }

        public void LoadAssetBundle( string _asset_name, Action<ResData,UnityEngine.Object> _action, bool _async = true )
        {
            switch( m_state )
            {
                case ResDataState.Init:
                    m_bundle_async = _async;
                    _addRequestAssets( _asset_name, _action, _async );
                    _loadDependencies( _async );
                    break;
                case ResDataState.LoadingDependencies:
                    m_bundle_async = _async;
                    _addRequestAssets( _asset_name, _action, _async );
                    break;
                case ResDataState.LoadingSelf:
                    _addRequestAssets( _asset_name, _action, _async );
                    break;
                case ResDataState.BundleLoaded:
                case ResDataState.ReadyUnload:
                    if( _asset_name == String.Empty )
                    {
                        m_reference_count++;
                        if( _action != null )
                            _action( this, null );
                    }
                    else
                    {
                        _addRequestAssets( _asset_name, _action, _async );
                        _loadAsset( _asset_name );
                    }
                    break;
            }
        }

        private void _loadDependencies( bool _async )
        {
            if ( m_res_config.Dependencies.Count == 0 )
            {
                _loadSelfBundle( _async );
            }
            else
            {
                Debug.LogFormat( "开始加载bundle:{0}的依赖！", m_res_config.BundleName );
                m_state = ResDataState.LoadingDependencies;
                for ( int i = 0; i < m_res_config.Dependencies.Count; i++ )
                {
                    ResManager.Instance.LoadAssetBundleAndAsset( m_res_config.Dependencies[i], string.Empty, _dependenciesLoaded, _async );
                }
            }
        }

        private void _dependenciesLoaded( ResData _data, UnityEngine.Object _obj )
        {
            m_loaded_dependencies_count++;
            if ( m_loaded_dependencies_count == m_res_config.Dependencies.Count )
            {
                _loadSelfBundle( m_bundle_async );
            }
        }

        private void _loadSelfBundle( bool _async )
        {
            m_state = ResDataState.LoadingSelf;
            AssetBundleLoader.Instance.LoadAssetbundle( this, _async );
        }

        public void OnAssetBundleLoaded( AssetBundle _bundle )
        {
            if( _bundle == null )
            {
                m_reference_count = 0;
                return;
            }
            m_state = ResDataState.BundleLoaded;
            m_bundle = _bundle;
            foreach ( var data in m_request_assets )
            {
                if( data.Key == string.Empty )
                {
                    for ( int i = 0; i < data.Value.CompleteActions.Count; )
                    {
                        Action<ResData, UnityEngine.Object> action = data.Value.CompleteActions[i];
                        data.Value.CompleteActions.RemoveAt( i );
                        action( this, null );
                    }
                    continue;
                }
                _loadAsset( data.Key );
            }
        }

        private void _loadAsset( string _name )
        {
            if( m_request_assets.ContainsKey( _name ) )
            {
                if( m_request_assets[_name].Async )
                    AssetLoader.Instance.LoadAssetAsync( this, _name );
                else
                    AssetLoader.Instance.LoadAssetSync( this, _name );
            }
            else
            {
                Debug.LogErrorFormat( "没有把asset {0}添加到包 {1}的资源请求表中", _name, m_res_config.BundleName );
            }
        }

        public void OnAssetLoaded( string _name, UnityEngine.Object _obj )
        {
            if ( m_request_assets.ContainsKey( _name ) )
            {
                for ( int i = 0; i < m_request_assets[_name].CompleteActions.Count; )
                {
                    Action<ResData, UnityEngine.Object> action = m_request_assets[_name].CompleteActions[i];
                    m_request_assets[_name].CompleteActions.RemoveAt( i );
                    action( this, _obj );
                }
            }
        }

        private void _addRequestAssets( string _asset_name, Action<ResData, UnityEngine.Object> _action, bool async = true )
        {
            if( _action == null )
                return;
            m_reference_count++;
            if( m_request_assets.ContainsKey( _asset_name ) )
            {
                m_request_assets[_asset_name].CompleteActions.Add( _action );
                return;
            }
            RequestAssetData data = new RequestAssetData();
            data.Name = _asset_name;
            data.Async = async;
            data.CompleteActions.Add( _action );
            m_request_assets.Add( _asset_name, data );
        }

        public void Unload()
        {
            m_reference_count--;
            if( m_reference_count <= 0 && m_state != ResDataState.ReadyUnload )
            {
                AssetBundleLoader.Instance.StartCoroutine( _readyUnload() );
            }
        }

        private IEnumerator _readyUnload()
        {
            m_state = ResDataState.ReadyUnload;
            yield return new WaitForEndOfFrame();
            Debug.LogFormat( "bundle:{0}开始卸载", m_res_config.BundleName );
            for( int i = 0; i < m_res_config.Dependencies.Count; i++ )
            {
                ResData data = ResManager.Instance.GetResData( m_res_config.Dependencies[i] );
                if( data == null )
                    continue;
                data.Unload();
            }
            UnInit();
        }

        private bool _hasRequestAssets()
        {
            foreach( var data in m_request_assets )
            {
                if( data.Value.CompleteActions.Count > 0 )
                    return true;
            }
            return false;
        }

        public AssetBundle GetBundle()
        {
            return m_bundle;
        }

        public ResConfig GetResConfig()
        {
            return m_res_config;
        }

        public string GetBundleName()
        {
            return m_res_config.BundleName;
        }

        public string GetBundlePath()
        {
            return string.Format( "{0}/{1}", Application.streamingAssetsPath, m_res_config.BundleName );
        }

        public uint GetBundleVersion()
        {
            return m_res_config.Version;
        }
    }
}
