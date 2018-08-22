using System.Collections.Generic;
using Game;

namespace Utility
{
	public delegate void GameEventHandler( params object[] args );

	public class EventSystem
    {
        public static EventSystem Instance = new EventSystem();

        private Dictionary<eEvents, List<GameEventHandler>> m_event_dic = new Dictionary<eEvents, List<GameEventHandler>>();

        public void Init()
        {
            m_event_dic.Clear();
        }

        public void AddEvent( eEvents type, GameEventHandler handler )
        {
			List<GameEventHandler> handlerList = m_event_dic.ContainsKey( type ) ? m_event_dic[type] : null;
			if ( handlerList == null )
            {
                m_event_dic[type] = new List<GameEventHandler>();
			}
			if ( m_event_dic[type].Contains( handler ) )
				return;

            m_event_dic[type].Add(handler);
		}

		public void RemoveEvent( eEvents type, GameEventHandler handler )
        {
			List<GameEventHandler> handlerList = m_event_dic.ContainsKey( type ) ? m_event_dic[type] : null;
			if ( handlerList != null && handlerList.Contains( handler ) )
            {
				handlerList.Remove( handler );
			}
		}

		public void OnEvent( eEvents type, params object[] args )
        {
			List<GameEventHandler> handlerList = m_event_dic.ContainsKey( type ) ? m_event_dic[type] : null;
			if ( handlerList != null )
            {
				for ( short i = 0; i < handlerList.Count; i++ )
                {
					handlerList[i]( args );
				}
			}
		}
	}
}