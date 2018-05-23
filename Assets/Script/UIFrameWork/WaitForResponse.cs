using System;

namespace UIFramework
{
	class WaitForResponse
	{
		private static Int32 s_reference_count = 0;
		public static void Reset()
		{
			s_reference_count = 0;
		}

		public static void Retain()
		{
			s_reference_count++;
			//UIManager.Instance.SetEventSystemEnable( false );
			UIManager.Instance.ShowUI( "ui_wait_response_panel" );
		}

		public static void Release()
		{
			s_reference_count--;
			if( s_reference_count > 0 )
				return;
			s_reference_count = 0;
			//UIManager.Instance.SetEventSystemEnable( true );
			UIManager.Instance.HideUI( "ui_wait_response_panel" );
		}
	}
}
