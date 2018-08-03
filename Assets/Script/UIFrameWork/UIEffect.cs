using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFramework
{
    [ExecuteInEditMode]
    [RequireComponent( typeof( Graphic ) )]
    public class UIEffect : MonoBehaviour
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        public enum ToneMode
        {
            None = 0,
            Gray,
        }

        public enum PixelMode
        {
            None = 0,
            Pixel,
            Blur,
        }
        private Graphic m_target_graphic;

        [SerializeField]
        private ToneMode m_tone_mode;

        [SerializeField]
        [Range( 0, 1 )]
        private float m_tone_power = 1;

        [SerializeField]
        private PixelMode m_pixel_mode;

        [SerializeField]
        [Range( 0, 1 )]
        private float m_pixel_power = 1;

        void Awake()
        {
            m_target_graphic = GetComponent<Graphic>();
        }

        void OnEnable()
        {
            if( Application.isPlaying )
            {
                if( m_tone_mode == ToneMode.None && m_pixel_mode == PixelMode.None )
                    return;
                Shader shader = GameFramework.ShaderManager.Instance.GetShader( "UI/UIEffect" );
                if( shader == null )
                {
                    Debug.LogErrorFormat( "没有shader:UI/UIEffect" );
                    return;
                }
                if( m_target_graphic.material == null || shader != m_target_graphic.material.shader )
                {
                    m_target_graphic.material = new Material( shader ); ;
                }
            }
            _updateMaterial();
        }

        void OnDisable()
        {
            m_target_graphic.material.DisableKeyword( "TONE_GRAY" );
            m_target_graphic.material.DisableKeyword( "EFFECT_PIXEL" );
            m_target_graphic.material.DisableKeyword( "EFFECT_BLUR" );
        }

        private void _updateMaterial()
        {
            switch( m_tone_mode )
            {
                case ToneMode.None:
                m_target_graphic.material.DisableKeyword( "TONE_GRAY" );
                break;
                case ToneMode.Gray:
                m_target_graphic.material.EnableKeyword( "TONE_GRAY" );
                m_target_graphic.material.SetFloat( "_TonePower", m_tone_power );
                break;
            }
            switch( m_pixel_mode )
            {
                case PixelMode.None:
                m_target_graphic.material.DisableKeyword( "EFFECT_PIXEL" );
                m_target_graphic.material.DisableKeyword( "EFFECT_BLUR" );
                break;
                case PixelMode.Pixel:
                m_target_graphic.material.DisableKeyword( "EFFECT_BLUR" );
                m_target_graphic.material.EnableKeyword( "EFFECT_PIXEL" );
                m_target_graphic.material.SetFloat( "_PixelPower", m_pixel_power );
                break;
                case PixelMode.Blur:
                m_target_graphic.material.DisableKeyword( "EFFECT_PIXEL" );
                m_target_graphic.material.EnableKeyword( "EFFECT_BLUR" );
                m_target_graphic.material.SetFloat( "_PixelPower", m_pixel_power );
                break;
            }
        }

#if UNITY_EDITOR
        void OnDestroy()
        {
            if( m_target_graphic )
            {
                m_target_graphic.material = null;
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if( !this || m_target_graphic == null )
                    return;
                if( Application.isPlaying )
                {
                    _updateMaterial();
                    return;
                }
                Shader shader = Shader.Find( "UI/UIEffect" );
                if( shader == null )
                {
                    Debug.LogErrorFormat( "没有shader:{0}", shader );
                    return;
                }
                if( m_target_graphic.material == null || shader != m_target_graphic.material.shader )
                {
                    m_target_graphic.material = new Material( shader ); ;
                }
                _updateMaterial();
            };
        }
#endif
    }
}
