using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_glasscontroller", Description = "Glass bridge game" )]
	[Hammer.EditorModel( "models/arrow.vmdl" )]
	public partial class GlassController : GameTypeBase
	{
		[Property( Title = "Beginning pos" )]
		public Vector3 Beginning { get; set; }

		[Property( Title = "Ending pos" )]
		public Vector3 End { get; set; }

		public bool Started;

		List<GlassPlate> plates = new List<GlassPlate>();

		float PlatesToSpawn = 10;

		Rotation SpawnDirection;

		

		public override void Spawn()
		{
			base.Spawn();

			if( plates.Count > 0)
			{
				for ( int i = 0; i < plates.Count; i++ )
				{
					plates[i].Delete();
				}
				plates.Clear();
			}

			SpawnDirection = (Beginning - End).EulerAngles.ToRotation();

			PlatesToSpawn = (Game.Current as TerryGame).TotalPlayersAlive;

			PlatesToSpawn = Math.Clamp( PlatesToSpawn, 8, 16);

			for ( int i = 0; i < PlatesToSpawn; i++ )
			{
				Vector3 spawnpos = Vector3.Lerp( Beginning, End, (i/PlatesToSpawn) + (0.5f/PlatesToSpawn) );

				GlassPlate plate1 = new GlassPlate();

				plate1.Position = spawnpos + SpawnDirection.Left * 67f;
				plate1.Spawn();
				plate1.EnableAllCollisions = true;

				GlassPlate plate2 = new GlassPlate();
				plate2.Position = spawnpos - SpawnDirection.Left * 67f;
				plate2.Spawn();
				plate2.EnableAllCollisions = true;

				plates.Add( plate1 );
				plates.Add( plate2 );

				if(Rand.Float() > 0.5f )
				{
					plate1.breakable = true;
					plate2.RenderColor = new Color( 0.95f, 0.9f, 1f, 1f );
				}
				else
				{
					plate2.breakable = true;
					plate1.RenderColor = new Color( 0.95f, 0.9f, 1f, 1f );
				}
			}

			KillOffPlayersTimer = plates.Count * 0.06f;
		}

		[Input( Name = "StartGameTimer" )]
		public void StartGameTimer()
		{
			if ( !Started )
			{
				Started = true;
			}
		}

		float KillOffPlayersTimer;

		[Event.Tick]
		void Tick()
		{
			if ( !IsServer || !Started )
			{
				return;
			}

			if ( Started )
			{
				(Game.Current as TerryGame).StartedCountdown = true;
			}

			if( (Game.Current as TerryGame).SecondsLeft <= 0 )
			{
				for ( int i = 0; i < plates.Count; i++ )
				{
					plates[i].DeleteAsync( i*0.05f );
				}

				KillOffPlayersTimer -= Time.Delta;

				if ( KillOffPlayersTimer <= 0 )
				{
					foreach ( var player in Entity.All )
					{
						if ( player is not SquidPlayer )
						{
							continue;
						}

						if(Vector3.DistanceBetween( player.Position, End + SpawnDirection.Forward) < Vector3.DistanceBetween( player.Position, End - SpawnDirection.Forward ) )
						{
							(Game.Current as TerryGame).AddEliminatedPlayer( player.Client.Name.ToString() );
							(player as SquidPlayer).Eliminated();
						}
					}
				}
			}
		}
	}
}
