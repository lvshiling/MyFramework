using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Utility;

namespace UnityEngine.UI
{
    public class OnEnableEvent : MonoBehaviour
    {

        [SerializeField, SetProperty("useDOTweenRestart")]
        private bool _useDOTweenRestart = true;
        public bool useDOTweenRestat
        {
            get
            {
                return _useDOTweenRestart;
            }
            set
            {
                _useDOTweenRestart = value;
            }
        }


        [SerializeField, SetProperty("useDOTween")]
        private bool _useDOTween = false;
        public bool useDOTween
        {
            get
            {
                return _useDOTween;
            }
            set
            {
                _useDOTween = value;
            }
        }
                
        [SerializeField, SetProperty("refuseDOTween")]
        private List<DOTweenAnimation> _refuseDOTween = new List<DOTweenAnimation>();        

        [FormerlySerializedAs("onStarDOTween")]
        [SerializeField]
        private Button.ButtonClickedEvent _onStarDOTween;
        public Button.ButtonClickedEvent onStarDOTween
        {
            get
            {
                return _onStarDOTween;
            }
            set
            {
                _onStarDOTween = value;
            }
        }

        private DOTweenAnimation[] tweens;

        void Awake()
        {
            if(_useDOTweenRestart)
                tweens = GetComponents<DOTweenAnimation>();
        }

        void OnEnable()
        {
            if(useDOTween)
            {
                onStarDOTween.Invoke();                
            }
            if(_useDOTweenRestart && tweens != null)
            {
                for (int i = 0; i < tweens.Length; ++i)
                {
                    var elem = tweens[i];
                    bool isFind = false;
                    for (int j = 0; j < _refuseDOTween.Count; ++j)
                    {
                        if (elem == _refuseDOTween[j])
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        if( elem != null && elem.enabled )
                        {
                            if( elem.id == string.Empty )
                                elem.DORestart();
                            else
                                elem.DORestartById( elem.id );
                        }
                    }
                }                
            }
        }

        public void SetDeactiveSelf()
        {
            gameObject.SetActive(false);
        }
    }
}
