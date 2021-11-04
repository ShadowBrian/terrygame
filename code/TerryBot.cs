using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_terrybot", Description = "Red/green light game" )]
	[Hammer.EditorModel( "models/terrybot.vmdl" )]
	public partial class TerryBot : GameTypeBase
	{
		[Property( Title = "Start Line Center" )]
		public Vector3 StartLineCenter { get; set; }

		[Property( Title = "Finish Line Center" )]
		public Vector3 FinishLineCenter { get; set; }

		Rotation WalkDirection;

		Rotation StartRot;

		Rotation NoLookRot;

		public bool Started;

		string[] audioclips = { "rglight_korean2-0", "rglight_korean3-0", "rglight_korean4-8", "rglight_korean6-0", "rglight_korean7-0" };

		string[] gunclips = { "gunshot1", "gunshot2" };

		float[] times = {2f,3f,4.8f,6f,7f };

		float KillOffPlayersTimer;

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/terrybot.vmdl" );
			UsePhysicsCollision = true;
			StartRot = Rotation;
			NoLookRot = StartRot * new Vector3( -1, 0, 0 ).EulerAngles.ToRotation();

			KillOffPlayersTimer = 0.5f;

			WalkDirection = (StartLineCenter - FinishLineCenter).EulerAngles.ToRotation();
		}

		[Input( Name = "StartRGLightGame" )]
		public void StartRGLightGame()
		{
			if ( !Started )
			{
				Started = true;
				RotateTime = 0;
				looking = true;
			}
		}

		float RotateTime = 1f;

		bool looking;

		bool KilledOffLeftoverPlayers;

		List<SquidPlayer> PlayersTokill = new List<SquidPlayer>();

		[Event.Tick]
		void Tick()
		{
			if ( !IsServer || !Started || ((Game.Current as TerryGame).SecondsLeft <= 0 && KilledOffLeftoverPlayers) )
			{
				return;
			}

			if ( Started )
			{
				(Game.Current as TerryGame).StartedCountdown = true;
			}

			if ( (Game.Current as TerryGame).SecondsLeft <= 0 )
			{
				if (!KilledOffLeftoverPlayers )
				{
					foreach ( var player in Entity.All )
					{
						if ( player is not SquidPlayer )
						{
							continue;
						}

						bool CrossedFinish = Vector3.DistanceBetween( player.Position, FinishLineCenter + WalkDirection.Forward ) > Vector3.DistanceBetween( player.Position, FinishLineCenter - WalkDirection.Forward );

						if ( !CrossedFinish )
						{
							(Game.Current as TerryGame).AddEliminatedPlayer( player.Client.Name.ToString() );
							(player as SquidPlayer).Eliminated();

							Sound snd = Sound.FromWorld( gunclips[Rand.Int(0,gunclips.Length-1)], Position + (Vector3.Up * (64 * 4)) );
							snd.SetVolume( 10f );
							snd.SetPosition( Position + (Vector3.Up * (64 * 4)) );
						}
					}
					KilledOffLeftoverPlayers = true;
				}
			}

			RotateTime -= Time.Delta;

			if(RotateTime <= 0f )
			{
				looking = !looking;

				SetAnimBool( "looking", looking );

				if ( !GetAnimBool( "active" ) )
				{
					SetAnimBool( "active", true );
					Rotation = NoLookRot;
				}

				if ( !looking )
				{
					int chosenIndex = Rand.Int( 0, 4 );

					RotateTime = times[chosenIndex];

					Sound snd = Sound.FromWorld( audioclips[chosenIndex], Position + (Vector3.Up * (64 * 4)) );

					snd.SetPosition( Position + (Vector3.Up * (64 * 4)) );
					PlayersTokill.Clear();
				}
				else
				{
					RotateTime = 1f + Rand.Float() * 1.5f;
				}
			}

			if ( looking )
			{
				foreach ( var player in Entity.All )
				{
					if ( player is not SquidPlayer )
					{
						continue;
					}

					bool CrossedFinish = Vector3.DistanceBetween(player.Position, FinishLineCenter + WalkDirection.Forward) > Vector3.DistanceBetween( player.Position, FinishLineCenter - WalkDirection.Forward );
					bool CrossedStart = Vector3.DistanceBetween( player.Position, StartLineCenter + WalkDirection.Forward ) > Vector3.DistanceBetween( player.Position, StartLineCenter - WalkDirection.Forward ); ;

					if ( CrossedStart )
					{
						Trace LookRay = Trace.Ray( Position + (Vector3.Up * (64 * 4)), ((SquidPlayer)player).headpos );

						TraceResult result = LookRay.Run();
						if ( !CrossedFinish && ((SquidPlayer)player).moving && player.Health > 0 && !((SquidPlayer)player).Died && (result.Entity == player || result.Entity == ((SquidPlayer)player).TerryPuppet) )
						{
							if ( !PlayersTokill.Contains( (player as SquidPlayer) ) )
							{
								PlayersTokill.Add( (player as SquidPlayer) );
							}
						}
					}
				}

				KillOffPlayersTimer -= Time.Delta;

				if ( KillOffPlayersTimer <= 0 )
				{
					foreach ( var player in PlayersTokill )
					{
						(Game.Current as TerryGame).AddEliminatedPlayer( player.Client.Name.ToString() );
						(player as SquidPlayer).Eliminated();

						int pickedClip = Rand.Int( 0, gunclips.Length - 1 );
						Sound snd = Sound.FromEntity( gunclips[pickedClip], player );

						
					}
					PlayersTokill.Clear();

					KillOffPlayersTimer = Rand.Float();
				}
			}
		}
	}
}
