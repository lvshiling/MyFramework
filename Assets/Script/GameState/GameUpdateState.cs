using System;
using Utility;

namespace GameFramework
{
	internal class GameUpdateState : State<GameStateMachine>
	{
        public GameUpdateState()
        {

        }

		public GameUpdateState( GameStateMachine _owner, GameStateMachine _fsm, String _name ) : base( _owner, _fsm, _name )
        {

        }

		public override void OnEnter()
		{
            
		}
	}
}