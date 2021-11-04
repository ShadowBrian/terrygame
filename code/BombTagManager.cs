using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_bombtagmanager", Description = "bomb tag game manager" )]
	[Hammer.EditorModel( "models/arrow.vmdl" )]
	public partial class BombTagManager : GameTypeBase
	{

		public override void Initialize()
		{
			(Game.Current as TerryGame).StartedCountdown = true;
			base.Initialize();
		}

		bool KilledOffPlayers;

		string[] explosionclips = { "explosion1", "explosion2", "explosion3" };

		[Event.Tick]
		void Tick()
		{
			if ( !IsServer )
			{
				return;
			}

			if( (Game.Current as TerryGame).SecondsLeft <= 0  && !KilledOffPlayers )
			{
				foreach ( var player in Entity.All )
				{
					if ( player is not SquidPlayer )
					{
						continue;
					}

					if((player as SquidPlayer).Bomb.IsValid() )
					{
						(Game.Current as TerryGame).AddEliminatedPlayer( player.Client.Name.ToString() );
						(player as SquidPlayer).Eliminated();
						Particles explosion = Particles.Create( "particles/explosion.vpcf" );
						explosion.SetPosition(0, player.Position );
						

						int pickedClip = Rand.Int( 0, explosionclips.Length - 1 );
						Sound snd = Sound.FromEntity( explosionclips[pickedClip], player );
					}
				}

				KilledOffPlayers = true;
			}
		}
	}
}
