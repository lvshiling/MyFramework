using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    public class ScrollRectListItem
    {
        public Int32 Index;
        public GameObject Go;

        public void SetItem( Int32 _index, GameObject _go )
        {
            Index = _index;
            Go = _go;
        }
    }

    public class ScrollRectList : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_item_prefab = null;

        [SerializeField]
        private ScrollRect m_scroll_rect = null;

        [SerializeField]
        private RectTransform m_container = null;

        private bool m_vertical = true;

        private float m_item_length = 0f;

        private float m_mask_length = 0f;

        private Int32 m_max_visible_count = 0;

        private Int32 m_max_item_count = 0;

        private List<GameObject> m_all_items = new List<GameObject>();

        private LinkedList<ScrollRectListItem> m_items = new LinkedList<ScrollRectListItem>();

        public Action<GameObject> OnItemCreate;

        public Action<GameObject, Int32> OnItemVisible;

        public void Init()
        {
            m_vertical = m_scroll_rect.vertical;
            m_item_length = m_vertical ? m_item_prefab.GetComponent<RectTransform>().sizeDelta.y : m_item_prefab.GetComponent<RectTransform>().sizeDelta.x;
            m_mask_length = m_vertical ? m_scroll_rect.GetComponent<RectTransform>().sizeDelta.y : m_scroll_rect.GetComponent<RectTransform>().sizeDelta.x;
            m_max_visible_count = Mathf.CeilToInt( m_mask_length / m_item_length ) + 1;
            m_scroll_rect.onValueChanged.AddListener( _onValueChange );

            if( m_vertical )
            {
                m_container.anchorMin = new Vector2( 0.5f, 1f );
                m_container.anchorMax = new Vector2( 0.5f, 1f );
                m_container.pivot = new Vector2( 0.5f, 1f );
            }
            else
            {
                m_container.anchorMin = new Vector2( 0f, 0.5f );
                m_container.anchorMax = new Vector2( 0f, 0.5f );
                m_container.pivot = new Vector2( 0f, 0.5f );
            }

            for( int i = 0; i < m_max_visible_count; i++ )
            {
                GameObject go = _createItem();
                if( OnItemCreate != null )
                {
                    OnItemCreate( go );
                }
                m_all_items.Add( go );
            }
        }

        public void SetMaxItemCount( Int32 _count )
        {
            if( m_max_item_count == _count )
            {
                var node = m_items.First;
                Int32 index = 0;
                while( node != null )
                {
                    _setItemInfo( node.Value, index, node.Value.Go );
                    index++;
                    node = node.Next;
                }
                return;
            }
            m_max_item_count = _count;
            if( m_vertical )
            {
                m_container.sizeDelta = new Vector2( m_item_prefab.GetComponent<RectTransform>().sizeDelta.x,
                    m_max_item_count * m_item_length );
            }
            else
            {
                m_container.sizeDelta = new Vector2( m_max_item_count * m_item_length,
                    m_item_prefab.GetComponent<RectTransform>().sizeDelta.y );
            }
            m_items.Clear();
            for( int i = 0; i < m_all_items.Count; i++ )
            {
                if( i >= m_max_item_count )
                {
                    m_all_items[i].SetActive( false );
                    continue;
                }
                m_all_items[i].SetActive( true );
                ScrollRectListItem item = new ScrollRectListItem();
                _setItemInfo( item, i, m_all_items[i] );
                m_items.AddLast( item );
            }
        }

        private void _onValueChange( Vector2 _pos )
        {
            if( m_items.Count <= 0 )
                return;

            float container_pos = _getContainerPos();
            Int32 first_visible_index = (Int32)( container_pos / m_item_length );
            if( first_visible_index < 0 )
                return;

            Int32 end_visible_index = first_visible_index + m_max_visible_count - 1;
            if( end_visible_index >= m_max_item_count )
                return;

            Int32 cur_first_visible_index = m_items.First.Value.Index;
            if( first_visible_index == cur_first_visible_index )
                return;

            //移动在一个单元内
            if( Mathf.Abs( first_visible_index - cur_first_visible_index ) == 1 )
            {
                //把头部移到尾部
                if( first_visible_index > cur_first_visible_index )
                {
                    ScrollRectListItem item = m_items.First.Value;
                    _setItemInfo( item, end_visible_index, item.Go );
                    m_items.RemoveFirst();
                    m_items.AddLast( item );
                }
                //把尾部移到头部
                if( first_visible_index < cur_first_visible_index )
                {
                    ScrollRectListItem item = m_items.Last.Value;
                    _setItemInfo( item, first_visible_index, item.Go );
                    m_items.RemoveLast();
                    m_items.AddFirst( item );
                }
            }
            //极速移动超过一个单元
            else
            {
                LinkedListNode<ScrollRectListItem> node = m_items.First;
                Int32 count = 0;
                while( node != null )
                {
                    _setItemInfo( node.Value, first_visible_index + count, node.Value.Go );
                    count++;
                    node = node.Next;
                }
            }
        }

        private GameObject _createItem()
        {
            GameObject go = Instantiate( m_item_prefab );
            RectTransform rect = go.GetComponent<RectTransform>();
            if( m_vertical )
            {
                rect.anchorMin = new Vector2( 0.5f, 1f );
                rect.anchorMax = new Vector2( 0.5f, 1f );
                rect.pivot = new Vector2( 0.5f, 1f );
            }
            else
            {
                rect.anchorMin = new Vector2( 0f, 0.5f );
                rect.anchorMax = new Vector2( 0f, 0.5f );
                rect.pivot = new Vector2( 0f, 0.5f );
            }
            go.transform.SetParent( m_container, false );
            go.SetActive( false );
            return go;
        }

        private float _getContainerPos()
        {
            return m_vertical ? m_container.anchoredPosition.y : -m_container.anchoredPosition.x;
        }

        private void _setItemInfo( ScrollRectListItem _item, Int32 _index, GameObject _go )
        {
            _go.name = _index.ToString();
            if( m_vertical )
                _go.GetComponent<RectTransform>().anchoredPosition = new Vector2( 0, -( _index * m_item_length ) );
            else
                _go.GetComponent<RectTransform>().anchoredPosition = new Vector2( _index * m_item_length, 0 );
            _item.SetItem( _index, _go );
            if( OnItemVisible != null )
                OnItemVisible( _go, _index );
        }
    }
}
