using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ResFramework;
using Scene = UnityEngine.SceneManagement.Scene;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using GameFramework;

namespace UIFramework
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField]
        private Canvas m_root_canvas = null;

        [SerializeField]
        private Camera m_main_camera = null;

        public Camera UICamera{ get { return m_main_camera; } }

        [SerializeField]
        private EventSystem m_event_system = null;

        [SerializeField]
        private Transform[] m_layers = null;

        [SerializeField]
        private Int32 m_max_layer_sorting_order = 2000;

        /// <summary>
        /// 所有已经被加载的界面
        /// </summary>
        private List<UIBase> m_loaded_ui = new List<UIBase>();

        /// <summary>
        /// windows的stack
        /// </summary>
        private Stack<String> m_windows_name_stack = new Stack<String>();

        /// <summary>
        /// dialog的list
        /// </summary>
        private LinkedList<String> m_dialog_name_list = new LinkedList<String>();

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad( this );
            Initialize();
        }

        public void Initialize()
        {
            SceneManager.sceneUnloaded += _onSceneUnload;
        }

        public void UnInitialize()
        {
            SceneManager.sceneUnloaded -= _onSceneUnload;
            m_loaded_ui.RemoveAll
            (
                _ui_base => 
                {
                    if( _ui_base.IsShow() )
                    {
                        _ui_base.OnHide( null );
                    }
                    _ui_base.OnUnload();
                    DestroyImmediate( _ui_base.gameObject, true );
                    return true;
                } 
            );
            m_windows_name_stack.Clear();
            m_dialog_name_list.Clear();
        }

        /// <summary>
        /// 开启或禁用所有UI响应
        /// </summary>
        public void SetEventSystemEnable( bool _enabled )
        {
            m_event_system.enabled = _enabled;
        }

        public void ShowUI( string _name, params object[] _args )
        {
            UIBase ui_obj = _getLoadedUI( _name );
            if( ui_obj != null && ui_obj.IsShow() )
            {
                ui_obj.transform.SetAsLastSibling();
                _resetLayerSortingOrder( ui_obj.Layer );
                ui_obj.PreShow( null, _args );
                return;
            }
            if ( ui_obj == null )
            {
                _createUI( _name, _base =>
                {
                    if( _base.Type == UIType.Window )
                        StartCoroutine( _showWindow( _base, _args ) );
                    else if( _base.Type == UIType.Dialog )
                        StartCoroutine( _showDialog( _base, _args ) );
                } );
            }
            else
            {
                if( ui_obj.Type == UIType.Window )
                    StartCoroutine( _showWindow( ui_obj, _args ) );
                else if( ui_obj.Type == UIType.Dialog )
                    StartCoroutine( _showDialog( ui_obj, _args ) );
            }
        }

        public void HideUI( String _name )
        {
            UIBase obj = _getLoadedUI( _name );
            if ( obj == null )
            {
                Debug.LogFormat( "HideUI: 需要隐藏的界面 [{0}]不存在!", _name );
                return;
            }
            HideUI( obj );
        }

        public void HideUI( UIBase _ui_object )
        {
            if ( _ui_object == null )
            {
                return;
            }
            if ( _ui_object.Type == UIType.Window )
            {
                Debug.LogErrorFormat( "HideUI: Window不能使用HideUI接口!" );
                return;
            }
            if ( _ui_object.Type == UIType.Dialog )
            {
                StartCoroutine( _hideDialog( _ui_object ) );
            }
        }

        public void ReturnLastWindow()
        {
            StartCoroutine( _returnLastWindow() );
        }

        private UIBase _getLoadedUI( string _name )
        {
            UIBase find_ob = m_loaded_ui.Find( ( _v ) => { return _v.gameObject.name.Equals( _name ); } );
            return find_ob;
        }

        private void _createUI( string _name, Action<UIBase> _action )
        {
            UIBase ui_base = null;
            ResManager.Instance.LoadAsset( string.Format( "Assets/Res/UI/Prefab/{0}.prefab", _name ), ( res_data, prefab ) =>
            {
                if ( prefab == null )
                {
                    Debug.LogErrorFormat( "ui: {0} 不存在!", _name );
                    return;
                }
                GameObject ui_object = Instantiate( prefab ) as GameObject;
                if( res_data != null )
                    res_data.Unload();
                ui_object.SetActive( false );
                ui_object.name = _name;

                // 放置ui到对应的层
                ui_base = ui_object.GetComponent<UIBase>();
                ui_object.transform.SetParent( m_layers[( Int32 )ui_base.Layer], false );
                m_loaded_ui.Add( ui_base );

                if ( ui_base.GetType() == typeof( UIBaseToLua ) )
                {
                    LuaManager.Instance.RequireLua( string.Format( "UI/{0}", _name ), ( _obj ) => 
                    {
                        ui_base.Initialize( _obj );
                        _action( ui_base );
                    } );
                }
                else
                {
                    ui_base.Initialize( null );
                    _action( ui_base );
                }
            }, false );
        }

        private void _resetLayerSortingOrder( UILayer _layer )
        {
            Int32 layer = (Int32)_layer;
            Int32 count = m_layers[layer].childCount;
            Transform tf = null;
            UIBase ui = null;
            Int32 active_count = 0;
            for ( Int32 i = 0; i < count; i++ )
            {
                tf = m_layers[layer].GetChild( i );
                ui = tf.GetComponent<UIBase>();
                if ( ui.IsShow() )
                {
                    ui.Canvas.overrideSorting = true;
                    Int32 new_order = layer * m_max_layer_sorting_order + active_count * 100;
                    if( ui.Canvas.sortingOrder != new_order )
                    {
                        ui.Canvas.sortingOrder = new_order;
                        ui.OnSortingOrderChange( new_order );
                    }
                    active_count++;
                }
            }
        }

        private IEnumerator _showWindow( UIBase _ui_object, params object[] _args )
        {
            UIBase current_win_obj = null;
            //播放上一个界面的退场动画和事件回调
            if ( m_windows_name_stack.Count != 0 )
            {
                current_win_obj = _getLoadedUI( m_windows_name_stack.Peek() );
                if ( current_win_obj != null )
                {
                    Single exit_time = current_win_obj.PlayExitAnimation();

                    if( exit_time > Single.Epsilon )
                        yield return new WaitForSeconds( exit_time );

                    _handleUIHide( current_win_obj );
                }
            }
            //播放当前界面的入场动画和事件回调
            _showOrHide( _ui_object, true );
            _ui_object.transform.SetAsLastSibling();
            _resetLayerSortingOrder( _ui_object.Layer );
            _ui_object.PreShow( current_win_obj, _args );

            Single entrance_time = _ui_object.PlayEntranceAnimation();
            m_windows_name_stack.Push( _ui_object.name );

            if ( entrance_time > Single.Epsilon )
                yield return new WaitForSeconds( entrance_time );

            _ui_object.OnShow( current_win_obj, _args );
        }

        private IEnumerator _showDialog( UIBase _ui_dialog, params object[] _args )
        {
            _showOrHide( _ui_dialog, true );
            _ui_dialog.transform.SetAsLastSibling();
            _resetLayerSortingOrder( _ui_dialog.Layer );
            _ui_dialog.PreShow( null, _args );

            Single entrance_time = _ui_dialog.PlayEntranceAnimation();
            m_dialog_name_list.AddLast( _ui_dialog.name );

            if ( entrance_time > float.Epsilon )
                yield return new WaitForSeconds( entrance_time );

            _ui_dialog.OnShow( null, _args );
        }

        private IEnumerator _returnLastWindow()
        {
            if ( m_windows_name_stack.Count <= 1 )
            {
                Debug.LogErrorFormat( "Windows型界面的Stack已经到底，不能再返回" );
                yield break;
            }
            // 先处理当前窗口的隐藏
            UIBase hide_win_obj = _getLoadedUI( m_windows_name_stack.Pop() );
            if( hide_win_obj == null )
            {
                Debug.LogErrorFormat( "当前Window为空" );
                yield break;
            }
            UIBase show_win_obj = _getLoadedUI( m_windows_name_stack.Peek() );
            if( show_win_obj == null )
            {
                _createUI( m_windows_name_stack.Peek(), _base => { show_win_obj = _base; } );
            }
            Single exit_time = hide_win_obj.PlayExitAnimation();
            if( exit_time > Single.Epsilon )
            {
                yield return new WaitForSeconds( exit_time );
            }
            _handleUIHide( hide_win_obj );

            yield return new WaitUntil( () => show_win_obj != null );

            // 处理新窗口的显示
            _showOrHide( show_win_obj, true );
            show_win_obj.transform.SetAsLastSibling();
            _resetLayerSortingOrder( show_win_obj.Layer );
            show_win_obj.PreShow( hide_win_obj );

            Single entrance_time = show_win_obj.PlayEntranceAnimation();
            if( entrance_time > Single.Epsilon )
            {
                yield return new WaitForSeconds( entrance_time );
            }
            show_win_obj.OnShow( hide_win_obj );
        }

        private IEnumerator _hideDialog( UIBase _ui_dialog )
        {
            var target_node = m_dialog_name_list.Find( _ui_dialog.name );
            if ( null != target_node )
            {
                m_dialog_name_list.Remove( target_node );
            }

            float exit_time = _ui_dialog.PlayExitAnimation();
            if ( exit_time > float.Epsilon )
                yield return new WaitForSeconds( exit_time );

            _handleUIHide( _ui_dialog );
        }

        private void _showOrHide( UIBase _ui, bool _show )
        {
            if( _ui.HideMode == UIHideMode.ActiveFalse )
            {
                _ui.gameObject.SetActive( _show );
            }
            else if( _ui.HideMode == UIHideMode.ChangeLayer )
            {
                if( _show )
                {
                    if( !_ui.gameObject.activeSelf )
                    {
                        _ui.gameObject.SetActive( true );
                    }
                    _ui.gameObject.layer = UnityEngine.LayerMask.NameToLayer( "UI" );
                }
                else
                {
                    _ui.gameObject.layer = 31;
                }
            }
        }

        private bool _handleUIHide( UIBase _ui )
        {
            _showOrHide( _ui, false );
            _ui.OnHide( null );
            if ( _ui.DestroyMode == UIDestroyMode.Hide )
            {
                _ui.OnUnload();
                m_loaded_ui.Remove( _ui );
                DestroyImmediate( _ui.gameObject, true );
                return true;
            }
            return false;
        }

        private void _onSceneUnload( Scene _scene )
        {
            if ( _scene.buildIndex < 0 )
                return;
            UIBase ui = null;
            for ( int i = 0; i < m_loaded_ui.Count; )
            {
                ui = m_loaded_ui[i];
                if( ui.IsShow() && ui.SceneChangeAutoHide && _handleUIHide( ui ) )
                {
                    continue;
                }
                if ( ui.DestroyMode == UIDestroyMode.SceneChange )
                {
                    ui.OnUnload();
                    m_loaded_ui.RemoveAt( i );
                    DestroyImmediate( ui.gameObject, true );
                    continue;
                }
                i++;
            }
            m_windows_name_stack.Clear();
            m_dialog_name_list.Clear();
        }
    }
}
