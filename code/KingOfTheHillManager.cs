using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_kingofthehill", Description = "koth point" )]
	[Hammer.EditorModel( "models/kothfield.vmdl" )]
	public partial class KingOfTheHillManager : GameTypeBase
	{
		int TotalPoints;

		Dictionary<string,int> Scores = new Dictionary<string, int>();

		List<string> playersTouching = new List<string>();

		string[] gunclips = { "gunshot1", "gunshot2" };

		public override void Initialize()
		{
			(Game.Current as TerryGame).StartedCountdown = true;

			foreach ( var player in Entity.All )
			{
				if ( player is not SquidPlayer || (player as SquidPlayer).Died)
				{
					continue;
				}
				if ( !Scores.ContainsKey( player.Client.Name ) )
				{
					Scores.Add(player.Client.Name,0);
				}
			}

			base.Initialize();
		}

		public override void Spawn()
		{
			SetModel( "models/kothfield.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = true;
			EnableTouch = true;
			base.Spawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( other is SquidPlayer && IsServer )
			{
				TotalPoints++;

				SquidPlayer player = other as SquidPlayer;

				if ( Scores.ContainsKey( player.Client.Name ) )
				{
					Scores[other.Client.Name] += 1;
				}

				playersTouching.Add( player.Client.Name );

				//Log.Trace( "StartTouch: " + player.Client.Name + " " + Scores[other.Client.Name] );
			}
		}

		public override void EndTouch( Entity other )
		{
			if ( other is SquidPlayer && IsServer)
			{
				SquidPlayer player = other as SquidPlayer;

				if ( Scores.ContainsKey( player.Client.Name ) )
				{
					Scores[other.Client.Name] += 1;
				}

				playersTouching.Remove( player.Client.Name );

				//Log.Trace("EndTouch: " + player.Client.Name + " " + Scores[other.Client.Name] );
			}
		}

		bool KilledOffPlayers;

		[Event.Tick.Server]
		void Tick()
		{

			foreach ( var playerName in playersTouching )
			{
				if ( Scores.ContainsKey( playerName ) )
				{
					Scores[playerName] += 1;
					TotalPoints++;
				}
			}

			if( (Game.Current as TerryGame).SecondsLeft <= 0  && !KilledOffPlayers )
			{
				foreach ( var player in Entity.All )
				{
					if ( player is not SquidPlayer )
					{
						continue;
					}

					bool PartOfLowestTeam = false;

					int Average = TotalPoints / Scores.Count;

					if ( Scores[player.Client.Name] < Average )
					{
						PartOfLowestTeam = true;
					}

					if ( PartOfLowestTeam )
					{
						(Game.Current as TerryGame).AddEliminatedPlayer( player.Client.Name.ToString() );
						(player as SquidPlayer).Eliminated();

						Sound.FromEntity( gunclips[Rand.Int( 0, gunclips.Length - 1 )], player );
					}
				}

				KilledOffPlayers = true;
			}
		}
	}
}
