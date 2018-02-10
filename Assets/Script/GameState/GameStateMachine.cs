using Utility;

namespace GameFramework
{
    static class GameStateName
    {
        public const string Startup = "Startup";
        public const string Update = "Update";
        public const string Main = "Main";
    }

    internal class GameStateMachine : StateMachine<GameStateMachine>
	{
		public override GameStateMachine GetOwner()
		{
			return this;
		}

		protected override void InitializeState()
		{
			AddState<GameStartupState>( GameStateName.Startup, true );
            AddState<GameUpdateState>( GameStateName.Update, false );
            AddState<GameMainState>( GameStateName.Main, false );
        }
	}
}