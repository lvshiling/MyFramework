using Net;
using System;
using Utility;

namespace GameFramework
{
	internal class GameStartupState : State<GameStateMachine>
	{
        public GameStartupState()
        {

        }

        public GameStartupState( GameStateMachine _owner, GameStateMachine _fsm, String _name ) : base( _owner, _fsm, _name )
        {

        }

		public override void OnEnter()
		{
            NetManager.Instance.Init();
        }
	}
}