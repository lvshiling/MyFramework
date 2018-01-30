using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ResFramework
{
	class AssetBundleLoader : MonoBehaviour
	{
		public static AssetBundleLoader Instance { get; private set; }

		private Queue<ResData> m_waiting_bundle = new Queue<ResData>();

        private LinkedList<ResData> m_loading_bundle = new LinkedList<ResData>();

	    private int m_max_loading_count = 1;

		void Awake()
		{
			Instance = this;
            DontDestroyOnLoad( this );
		}

		public void LoadAssetbundle( ResData _assetbundle, bool _async )
		{
            if( _async )
            {
                if ( m_waiting_bundle.Contains( _assetbundle ) || m_loading_bundle.Contains( _assetbundle ) )
                    return;
                m_waiting_bundle.Enqueue( _assetbundle );
            }
            else
            {
                if ( Caching.IsVersionCached( _assetbundle.GetBundleName(), Hash128.Parse( _assetbundle.GetResConfig().Md5 ) ) )
                {
                    Debug.LogFormat( "开始同步加载bundle: {0}", _assetbundle.GetBundleName() );
                    string name = string.Format( "{0}/{1}/{2}/__data", Caching.defaultCache.path, System.IO.Path.GetFileNameWithoutExtension( _assetbundle.GetBundleName() ), Hash128.Parse( _assetbundle.GetResConfig().Md5 ) );
                    AssetBundle bundle = AssetBundle.LoadFromFile( name );
                    if (bundle == null)
                    {
                        Debug.LogErrorFormat( "bundle: {0}加载错误", _assetbundle.GetBundleName() );
                        return;
                    }
                    Debug.LogFormat( "bundle: {0}加载完成", _assetbundle.GetBundleName() );
                    _assetbundle.OnAssetBundleLoaded( bundle );
                }
                else
                {
                    StartCoroutine( _loadAssetBundle( _assetbundle ) );
                }
            }
        }

        public void Update()
		{
            if( m_waiting_bundle.Count == 0 || m_loading_bundle.Count == m_max_loading_count )
                return;
            Int32 count = m_max_loading_count - m_loading_bundle.Count;
            for( int i = 0; i < count; i++ )
            {
                if( m_waiting_bundle.Count == 0 )
                    break;
                m_loading_bundle.AddLast( m_waiting_bundle.Dequeue() );
                StartCoroutine( _loadAssetBundle( m_loading_bundle.Last.Value ) );
            }
		}

		private IEnumerator _loadAssetBundle( ResData _res_data )
		{
            Debug.LogFormat( "开始异步加载bundle: {0} {1}", _res_data.GetBundleName(), Time.frameCount );
		    UnityWebRequest www = UnityWebRequest.GetAssetBundle( _res_data.GetBundleName(), Hash128.Parse( _res_data.GetResConfig().Md5 ), 0 );
		    yield return www.SendWebRequest();
		    if ( www.isHttpError )
		    {
		        Debug.LogErrorFormat( "bundle: {0}加载错误 {1}", _res_data.GetBundleName(), www.error );
		        m_loading_bundle.Remove( _res_data );
                yield break;
		    }
		    Debug.LogFormat( "bundle: {0}加载完成 {1}", _res_data.GetBundleName(), Time.frameCount );
            _res_data.OnAssetBundleLoaded( DownloadHandlerAssetBundle.GetContent( www ) );
		    m_loading_bundle.Remove( _res_data );
        }
	}
}