using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    public enum UILayer
    {
        /// <summary>
        /// 背景层(比如主界面)
        /// </summary>
        Background = 0,
        /// <summary>
        /// 默认层(比如装备界面 物品界面)
        /// </summary>
        Default = 1,
        /// <summary>
        /// 弹出窗口层(比如提示框)
        /// </summary>
        Popup = 2,
    }

    public enum UIType
    {
        /// <summary>
        /// 打开时会关闭上一个Window界面 点关闭或返回可返回上一个Window
        /// </summary>
        Window,
        /// <summary>
        /// 直接在最上层打开 不会关闭其他界面
        /// </summary>
        Dialog,
    }


    public enum UIDestroyMode
    {
        /// <summary>
        /// 常驻 退出游戏时才销毁
        /// </summary>
        Never,
        /// <summary>
        /// 切换场景时销毁
        /// </summary>
        SceneChange,
        /// <summary>
        /// 界面关闭时销毁
        /// </summary>
        Hide,
    }

    public enum UIHideMode
    {
        ActiveFalse,        

        ChangeLayer,
    }

    public class UIBase : MonoBehaviour
    {
        [SerializeField]
        private UILayer m_ui_layer = UILayer.Default;

        public UILayer Layer
        {
            get { return m_ui_layer; }
        }

        [SerializeField]
        private UIType m_ui_type = UIType.Dialog;

        public UIType Type
        {
            get { return m_ui_type; }
        }

        [SerializeField]
        private UIDestroyMode m_ui_destroy_mode = UIDestroyMode.Never;

        public UIDestroyMode DestroyMode
        {
            get { return m_ui_destroy_mode; }
        }

        [SerializeField]
        private UIHideMode m_ui_hide_mode = UIHideMode.ActiveFalse;

        public UIHideMode HideMode
        {
            get { return m_ui_hide_mode; }
        }

        [SerializeField]
        private bool m_scene_change_auto_hide = true;

        public bool SceneChangeAutoHide
        {
            get { return m_scene_change_auto_hide; }
        }

        private Canvas m_canvas = null;
        public Canvas Canvas
        {
            get { return m_canvas; }
        }

        private Animator m_animator = null;
        public Animator Animator
        {
            get{ return m_animator; }
        }

        /// <summary>
        /// 入场动画时长
        /// </summary>
        private Single m_entrance_anim_time;

        /// <summary>
        /// 出场动画时长
        /// </summary>
        private Single m_exit_anim_time;

#if UNITY_EDITOR
        void Awake()
        {
            if( UIManager.Instance == null )
                OnLoaded();
        }

        void Start()
        {
            if ( UIManager.Instance == null )
                PreShow( null );
        }
#endif

        public static void AddEventTrigger( GameObject _go, EventTriggerType _event_trigger_type, UnityAction<BaseEventData> _event )
        {
            EventTrigger event_trigger = _go.GetComponent<EventTrigger>();
            if( event_trigger == null )
                event_trigger = _go.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = _event_trigger_type;
            entry.callback.AddListener( _event );
            event_trigger.triggers.Add( entry );
        }

        public void Initialize()
        {
            m_canvas = GetComponent<Canvas>();
            if ( m_canvas == null )
            {
                m_canvas = gameObject.AddComponent<Canvas>();
            }
            if( GetComponent<GraphicRaycaster>() == null )
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
            m_animator = GetComponent<Animator>();
            if( m_animator != null && m_animator.runtimeAnimatorController != null && m_animator.runtimeAnimatorController.animationClips.Length >= 2 )
            {
                m_entrance_anim_time = m_animator.runtimeAnimatorController.animationClips[0].length;
                m_exit_anim_time = m_animator.runtimeAnimatorController.animationClips[1].length;
            }
            OnLoaded();
        }

        /// <summary>
        /// 整个界面被加载完成时的回调 相当于Awake 为了方便管理不想在子类中用Awake
        /// </summary>
        public virtual void OnLoaded()
        {

        }

        /// <summary>
        /// 入场动画播放之前的回调
        /// </summary>
        /// <param name="_pre_ui">上一个界面</param>
        /// <param name="_args">其他界面调用时给过来的参数列表, 和传递给OnShow的参数相同</param>
        public virtual void PreShow( UIBase _pre_ui, params object[] _args )
        {
            
        }

        /// <summary>
        /// 入场动画播放完后的回调
        /// </summary>
        /// <param name="_pre_ui">上一个界面</param>
        /// <param name="_args">其他界面调用时给过来的参数列表，和传递给PreShow的参数相同</param>
        public virtual void OnShow( UIBase _pre_ui, params object[] _args )
        {

        }

        /// <summary>
        /// 已被隐藏
        /// <param name="_next_ui">下一个要显示的界面</param>
        /// </summary>
        public virtual void OnHide( UIBase _next_ui )
        {

        }

        /// <summary>
        /// 当前界面被销毁前由UIManager触发
        /// </summary>
        public virtual void OnUnload()
        {

        }

        public virtual void OnSortingOrderChange( Int32 _newOrder )
        {
            
        }

        /// <summary>
        /// 播放入场动画
        /// </summary>
        public virtual Single PlayEntranceAnimation()
        {
            if ( Animator == null )
                return Single.Epsilon;
            if( m_entrance_anim_time <= Single.Epsilon )
            {
                return Single.Epsilon;
            }
            Animator.CrossFade( "Base Layer.Enter", 0f, 0, 0f );
            return m_entrance_anim_time;
        }

        /// <summary>
        /// 播放退场动画
        /// </summary>
        public virtual Single PlayExitAnimation()
        {
            if ( Animator == null )
                return Single.Epsilon;
            if ( m_exit_anim_time <= Single.Epsilon )
            {
                return Single.Epsilon;
            }
            Animator.CrossFade( "Base Layer.Exit", 0f, 0, 0f );
            return m_exit_anim_time;
        }

        public bool IsShow()
        {
            if( m_ui_hide_mode == UIHideMode.ActiveFalse )
                return gameObject.activeSelf;
            return gameObject.activeSelf && gameObject.layer == LayerMask.NameToLayer( "UI" );
        }

        public void HideSelf()
        {
            UIManager.Instance.HideUI( this );
        }
    }
}
