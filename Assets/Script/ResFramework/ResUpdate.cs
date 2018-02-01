using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace ResFramework
{
    public class ResUpdate : MonoBehaviour
    {
        public static ResUpdate Instance;

        [SerializeField]
        private string m_res_server_url = string.Empty;

        Dictionary<string, ResConfig> m_server_res_config = new Dictionary<string, ResConfig>();

        private List<ResConfig> m_need_download_res = new List<ResConfig>();

        private TextAsset m_server_res = null;

        private Action m_complete = null;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad( this );
        }

        public void CheckUpdate( Action _complete )
        {
            m_complete = _complete;
            StartCoroutine( _downloadResList() );
        }

        private IEnumerator _downloadResList()
        {
            string url = string.Format( "{0}/res_list.assetbundle", m_res_server_url );
            UnityWebRequest www = UnityWebRequest.GetAssetBundle( url );
            yield return www.SendWebRequest();
            if ( www.error != null )
            {
                Debug.LogErrorFormat( "下载最新资源列表失败 {0} {1}", url, www.error );
                if ( m_complete != null )
                    m_complete();
                yield break;
            }
            AssetBundle ab = DownloadHandlerAssetBundle.GetContent( www );
            m_server_res = ab.LoadAsset<TextAsset>( "res_list" );
            ResManager.Deserialize( m_server_res.bytes, m_server_res_config, null );
            foreach( var res in m_server_res_config )
            {
                ResConfig config = ResManager.Instance.GetResConfig( res.Key );
                if( config == null || config.Md5.CompareTo( res.Value.Md5 ) != 0 )
                {
                    m_need_download_res.Add( res.Value );
                }
            }
            if( m_need_download_res.Count == 0 && m_complete != null )
                m_complete();
            else
                StartCoroutine( _downloadRes() );
        }

        private IEnumerator _downloadRes()
        {
            string url;
            for ( int i = 0; i < m_need_download_res.Count; )
            {
                url = string.Format( "{0}/{1}", m_res_server_url, m_need_download_res[i].BundleName );
                if( Caching.IsVersionCached( m_need_download_res[i].BundleName, Hash128.Parse( m_need_download_res[i].Md5 ) ) )
                {
                    m_need_download_res.RemoveAt( i );
                    continue;
                }
                Debug.LogFormat( "开始下载资源:{0}", url );
                using ( UnityWebRequest www = UnityWebRequest.GetAssetBundle( url, Hash128.Parse( m_need_download_res[i].Md5 ), 0 ) )
                {
                    yield return www.SendWebRequest();
                    if ( www.isHttpError || www.isNetworkError )
                    {
                        Debug.LogErrorFormat( "资源:{0}下载失败 {1}", url, www.error );
                        ++i;
                        continue;
                    }
                    Debug.LogFormat( "资源:{0}下载完成", url );
                    Caching.ClearOtherCachedVersions( System.IO.Path.GetFileNameWithoutExtension( m_need_download_res[i].BundleName ), Hash128.Parse( m_need_download_res[i].Md5 ) );
                    m_need_download_res.RemoveAt( i );
                }
            }
            if ( m_need_download_res.Count == 0 && m_complete != null )
            {
                ResManager.Instance.SaveResList( m_server_res.bytes );
                ResManager.Instance.SetAllConfig( m_server_res_config );
                m_complete();
            }
            else
                StartCoroutine( _downloadRes() );
        }
    }
}
