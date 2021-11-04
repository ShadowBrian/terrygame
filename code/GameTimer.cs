using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_timer", Description = "Generic Timer" )]
	[Hammer.EditorModel( "models/timer.vmdl" )]
	partial class GameTimer : GameTypeBase
	{
		[Net,Change]
		public int time { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/timer.vmdl" );
			Tags.Add( "timer" );

		}

		public void OntimeChanged()
		{
			if(time <= 10 && time > 0 )
			{
				Sound snd = Sound.FromWorld( "beep2", Position );
			}else if(time == 0 )
			{
				Sound snd2 = Sound.FromWorld( "beep", Position);
			}
		}

		[Event.Tick]
		void Tick()
		{
			if ( IsServer )
			{
				TimeSpan span = TimeSpan.FromSeconds( (Game.Current as TerryGame).SecondsLeft );//Get SecondsLeft from the Game script server-side

				time = (span.Minutes * 100) + span.Seconds;//Convert to readable int

			}

			if( IsClient ) { 

				//Log.Trace( time );

				if ( this.SceneObject != null )
				{
					this.SceneObject.SetValue( "pnum", time );
				}
			}
		}
	}
}
