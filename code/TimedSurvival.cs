using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_timedsurvival", Description = "Timed survival game settings" )]
	[Hammer.EditorModel( "models/arrow.vmdl" )]
	public partial class TimedSurvival : GameTypeBase
	{
		[Property(Name = "Start Immediately", Title = "Start Immediately, if false hook up \"StartGameTimer\" input to a trigger." )]
		public bool StartImmediately { get; set; }

		[Property( Name = "Forced Time Limit", Title = "Forced Time Limit, set to 0 to allow playlist-dictated time limits." )]
		public float ForcedTime { get; set; }

		public override void Initialize()
		{
			if(ForcedTime != 0f )
			{
				(Game.Current as TerryGame).SecondsLeft = ForcedTime;
			}

			if ( StartImmediately )
			{
				Started = true;
				(Game.Current as TerryGame).StartedCountdown = true;
			}

			base.Initialize();
		}

		bool Started;

		[Input( Name = "StartGameTimer" )]
		public void StartGameTimer()
		{
			if ( !Started )
			{
				Started = true;
				(Game.Current as TerryGame).StartedCountdown = true;
			}
		}
	}
}
