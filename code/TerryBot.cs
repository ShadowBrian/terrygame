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
		float[] times = {2f,3f,4.8f,6f,7f };

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/terrybot.vmdl" );
			UsePhysicsCollision = true;
			StartRot = Rotation;
			NoLookRot = StartRot * new Vector3( -1, 0, 0 ).EulerAngles.ToRotation();

			KillOffPlayersTimer = 1.5f;

			WalkDirection = (StartLineCenter - FinishLineCenter).EulerAngles.ToRotation();
		}

		float KillOffPlayersTimer;

		[Input( Name = "StartRGLightGame" )]
		public void StartRGLightGame()
		{
			if ( !Started )
			{
				Started = true;
				RotateTime = 0;
			}
		}

		float RotateTime = 1f;

		bool looking;

		[Event.Tick]
		void Tick()
		{
			

			if ( !IsServer || !Started)
			{
				return;
			}

			if ( Started )
			{
				(Game.Current as TerryGame).StartedCountdown = true;
			}

			if ( (Game.Current as TerryGame).SecondsLeft <= 0 )
			{

				KillOffPlayersTimer -= Time.Delta;

				if ( KillOffPlayersTimer <= 0 )
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
						}
					}
				}
			}

			RotateTime -= Time.Delta;

			if(RotateTime <= 0f )
			{
				looking = !looking;

				//SetInteractsExclude(CollisionLayer.PLAYER_CLIP)

				SetAnimBool( "looking", looking );

				if ( !GetAnimBool( "active" ) )
				{
					SetAnimBool( "active", true );
					Rotation = NoLookRot;
				}

				

				//Rotation = !looking ? NoLookRot : StartRot;

				if ( !looking )
				{
					int chosenIndex = Rand.Int( 0, 4 );

					RotateTime = times[chosenIndex];

					Sound snd = Sound.FromWorld( audioclips[chosenIndex], Position + (Vector3.Up * (64 * 4)) );

					snd.SetPosition( Position + (Vector3.Up * (64 * 4)) );


					/*RotateTime = 2f + Rand.Float() * 4.5f;

					Sound snd = Sound.FromWorld( "rglight_korean4-8", Position + (Vector3.Up * (78 * 4)) );

					float pitchmult = 5.5f / RotateTime;

					float pitchcomp = RotateTime / 5.5f;

					snd.SetPitch( pitchmult );
					
					snd.SetVolume( 50f );

					RotateTime -= 1.4f * pitchcomp;

					RotateTime = RotateTime / 2f;*/

					Log.Trace( chosenIndex + " Played!" );

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

					if ( CrossedStart ) // crossed start line
					{
						Trace LookRay = Trace.Ray( Position + (Vector3.Up * (64 * 4)), ((SquidPlayer)player).headpos );

						//DebugOverlay.Line( Position + (Vector3.Up * (64 * 4)), ((SquidPlayer)player).headpos );

						TraceResult result = LookRay.Run();
						if ( !CrossedFinish && ((SquidPlayer)player).moving && player.Health > 0 && (result.Entity == player || result.Entity == ((SquidPlayer)player).TerryPuppet) )
						{
							//player.Health = -1;
							//player.OnKilled();
							(Game.Current as TerryGame).AddEliminatedPlayer(player.Client.Name.ToString());
							(player as SquidPlayer).Eliminated();
							
						}
						//Log.Trace( "Player " + player.Owner.NetworkIdent + " crossed the start line! at " + StartLineCenter );
					}

					//Log.Trace( "Found Player " + client.Owner.NetworkIdent );
				}
			}
			//Log.Trace( "TerrybotTick!" );
		}
	}
}
