using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    public static class UIEffectIns
    {
        public static Material GetMaterial( string _shader, Graphic _graphic )
        {
            Shader shader = GameFramework.ShaderManager.Instance.GetShader( _shader );
            if( shader == null )
            {
                Debug.LogErrorFormat( "没有shader:{0}", _shader );
                return null;
            }
            if( shader != _graphic.material.shader )
            {
                _graphic.material = new Material( shader );
            }
            return _graphic.material;
        }

        public static void SetGrayEffect( this Graphic _graphic, bool _enabled, float _power = 1f )
        {
            Material material = GetMaterial( "UI/UIEffect", _graphic );
            if( material == null )
                return;
            if( _enabled )
            {
                material.EnableKeyword( "TONE_GRAY" );
                material.SetFloat( "_TonePower", _power );
            }
            else
                material.DisableKeyword( "TONE_GRAY" );
        }

        public static void SetPixelEffect( this Graphic _graphic, bool _enabled, float _power = 1f )
        {
            Material material = GetMaterial( "UI/UIEffect", _graphic );
            if( material == null )
                return;
            if( _enabled )
            {
                material.DisableKeyword( "EFFECT_BLUR" );
                material.EnableKeyword( "EFFECT_PIXEL" );
                material.SetFloat( "_PixelPower", _power );
            }
            else
                material.DisableKeyword( "EFFECT_PIXEL" );
        }

        public static void SetBlurEffect( this Graphic _graphic, bool _enabled, float _power = 1f )
        {
            Material material = GetMaterial( "UI/UIEffect", _graphic );
            if( material == null )
                return;
            if( _enabled )
            {
                material.DisableKeyword( "EFFECT_PIXEL" );
                material.EnableKeyword( "EFFECT_BLUR" );
                material.SetFloat( "_PixelPower", _power );
            }
            else
                material.DisableKeyword( "EFFECT_BLUR" );
        }
    }
}
