using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_tugofwarrope", Description = "Tug of war rope" )]
	[Hammer.EditorModel( "models/tugofwar_rope.vmdl" )]
	partial class TugOfWarRopeManager : GameTypeBase
	{
		[Property( Title = "Beginning pos" )]
		public Vector3 Beginning { get; set; }

		[Property( Title = "Ending pos" )]
		public Vector3 End { get; set; }

		int TerriesToSpawn = 0;

		int RoundsToPlay = 0;

		List<RopePullTerry> terries = new List<RopePullTerry>();

		List<RopePullTerry> terriesT1 = new List<RopePullTerry>();
		List<RopePullTerry> terriesT2 = new List<RopePullTerry>();

		Vector3 startpos;

		Rotation SpawnDirection;

		[Net]
		public bool RoundActive { get; set; }

		public override void Initialize()
		{
			base.Initialize();

			SetModel( "models/tugofwar_rope.vmdl" );

			startpos = Position;

			if ( terries.Count > 0 )
			{
				for ( int i = 0; i < terries.Count; i++ )
				{
					//statues[i].numberPlate.Delete();
					terries[i].Delete();
				}
				terries.Clear();

				terriesT1.Clear();
				terriesT2.Clear();
			}

			TerriesToSpawn = 7;//(int)MathF.Ceiling((Game.Current as TerryGame).TotalPlayersAlive/2f);

			RoundsToPlay = (int)MathF.Ceiling( (Game.Current as TerryGame).TotalPlayersAlive / 7f );

			Log.Trace( "Rounds to play: " + RoundsToPlay );

			SpawnDirection = (Beginning - End).EulerAngles.ToRotation();

			for ( int i = 0; i < TerriesToSpawn; i++ )
			{
				bool Side = (i % 2 == 1);

				Vector3 rawpos = Vector3.Lerp( Beginning, End, (float)i / (float)TerriesToSpawn );

				Vector3 spawnpos = rawpos +( SpawnDirection.Right * 7.5f * (Side?1:-1));

				RopePullTerry terr = new RopePullTerry();

				terr.Position = spawnpos;
				terr.Rotation = Rotation.LookAt(Position - spawnpos);
				terr.Spawn();

				//terr.RopePos = rawpos;

				terr.direction = -1;

				terr.EnableAllCollisions = true;

				terr.side = Side ? 1 : -1;

				terries.Add( terr );

				terriesT1.Add( terr );
			}

			for ( int i = 0; i < TerriesToSpawn; i++ )
			{

				bool Side = (i % 2 == 1);

				Vector3 rotatedPos = Vector3.Lerp( Beginning, End, (float)i / (float)TerriesToSpawn );

				rotatedPos = Vector3.Reflect( rotatedPos, SpawnDirection.Forward );

				Vector3 spawnpos = rotatedPos + (SpawnDirection.Right * 7.5f * (Side ? 1 : -1));

				RopePullTerry terr = new RopePullTerry();

				terr.Position = spawnpos;
				terr.Rotation = Rotation.LookAt( Position - spawnpos );
				terr.Spawn();

				terr.direction = 1;

				

				terr.EnableAllCollisions = true;

				terr.side = Side ? 1 : -1;

				terries.Add( terr );

				terriesT2.Add( terr );
			}

			PullStrength = 1f;//TODO: remove this.

			flipflop = (Rand.Float( 0, 1f ) > 0.5);
		}

		float PullStrength;

		float movement;

		bool flipflop;

		bool WaitForAccuracies;

		float roundTimer = 6f;

		int PullCyclesDone = 0;

		[Event.Tick.Server]
		void Tick()
		{
			if ( !RoundActive )
			{
				return;
			}

			if( PullStrength > 0 )
			{
				PullStrength -= Time.Delta;
			}

			if ( PullStrength <= 0 && !WaitForAccuracies )
			{ 
				PullStrength = 0f;
				flipflop = !flipflop;
				WaitForAccuracies = true;
			}

			if ( WaitForAccuracies )
			{
				roundTimer -= Time.Delta;
				if(roundTimer <= 0f )
				{
					PullStrength = 1f + Rand.Float( 0.5f, 2f );//TODO: Collect team data.

					float GroundedPercentage = 0f;

					if ( !flipflop )
					{
						for ( int i = 0; i < terriesT1.Count; i++ )
						{
							if ( terriesT1[i].Grounded )
							{
								GroundedPercentage++;
							}
						}
						GroundedPercentage /= terriesT1.Count;
					}
					else
					{
						for ( int i = 0; i < terriesT2.Count; i++ )
						{
							if ( terriesT2[i].Grounded )
							{
								GroundedPercentage++;
							}
						}
						GroundedPercentage /= terriesT2.Count;
					}

					PullStrength *= GroundedPercentage;

					WaitForAccuracies = false;

					roundTimer = 6f - (PullCyclesDone * 0.5f);
					roundTimer = MathX.Clamp( roundTimer,0.5f,6f);

					PullCyclesDone++;
				}
			}

			movement = MathX.LerpTo( movement, PullStrength * (flipflop?1:-1),0.1f);

			for ( int i = 0; i < terries.Count; i++ )
			{
				terries[i].RopeMovement = movement;
				terries[i].Position += SpawnDirection.Forward * movement;
			}

			Position += SpawnDirection.Forward * movement;
		}
	}
}
