using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	public partial class TerryGame : Sandbox.Game
	{
		public GameTypes gameType;

		public enum GameTypes
		{
			BasicElimination,
			RGLight,
			CookieCut,
			TugOfWar,
			Marbles,
			GlassBridge,
			Squid
		}

		//List<GameTimer> timers = new List<GameTimer>();

		public bool StartedCountdown;

		public float SecondsLeft = 60;

		public List<string> eliminatedPlayers = new List<string>();

		[Net]
		public int TotalPlayersAlive { get; set; }

		string[] LevelPlaylist;

		int currentMap = 0;



		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new SquidPlayer();

			client.Pawn = player;

			CheckEliminatedPlayers();

			if ( eliminatedPlayers.Contains( client.Name.ToString() ) )
			{
				player.Died = true;
			}
			else
			{
				TotalPlayersAlive++;
			}

			player.Respawn();
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			camSetup.ZNear = 2.5f;
		}

		public void AddEliminatedPlayer(string name )
		{
			if ( !eliminatedPlayers.Contains( name ) )
			{
				eliminatedPlayers.Add( name );
				FileSystem.Data.WriteAllText( "Eliminations.txt", String.Join( ",", eliminatedPlayers ) );
			}
		}

		public void CheckEliminatedPlayers()
		{
			if ( FileSystem.Data.FileExists( "Eliminations.txt" ) )
			{
				eliminatedPlayers = FileSystem.Data.ReadAllText( "Eliminations.txt" ).Split(",").ToList();
			}
		}

		public override void Shutdown()
		{
			if ( IsServer )
			{
				if ( currentMap + 1 >= LevelPlaylist.Length )
				{
					if ( FileSystem.Data.FileExists( "CurrentMap.txt" ) )
					{
						FileSystem.Data.DeleteFile( "CurrentMap.txt" );
					}
					if ( FileSystem.Data.FileExists( "Eliminations.txt" ) )
					{
						FileSystem.Data.DeleteFile( "Eliminations.txt" );
					}
				}
			}
			base.Shutdown();
		}

		public void ConfigureLevelPlaylist()
		{
			if ( FileSystem.Data.FileExists( "LevelPlaylist.txt" ) )
			{
				Log.Trace( "Playlist found!" );

				LevelPlaylist = FileSystem.Data.ReadAllText( "LevelPlaylist.txt" ).Split( "\n" );

				Log.Trace( "Playlist loaded: \n" + FileSystem.Data.ReadAllText( "LevelPlaylist.txt" ) );

				if ( FileSystem.Data.FileExists( "CurrentMap.txt" ) ) {

					currentMap = int.Parse( FileSystem.Data.ReadAllText( "CurrentMap.txt" ) );
				}
				else
				{
					for ( int i = 0; i < LevelPlaylist.Length; i++ )
					{
						if(Global.MapName == LevelPlaylist[i].Split(",")[0] )
						{
							currentMap = i;
							break;
						}
					}
					FileSystem.Data.WriteAllText( "CurrentMap.txt", currentMap.ToString() );
				}
			}
			else
			{
				FileSystem.Data.WriteAllText( "LevelPlaylist.txt", "tg_rglight_courtyard,120\ntg_rglight_courtyard,120" );
				ConfigureLevelPlaylist();
			}
		}

		public override void Spawn()
		{
			CheckEliminatedPlayers();
			ConfigureLevelPlaylist();

			SecondsLeft = float.Parse(LevelPlaylist[currentMap].Split( "," )[1]);

			base.Spawn();
		}

		float countdownToNextMap = 5f;

		bool NextLevelCountdown;

		/// <summary>
		/// Called each tick.
		/// Serverside: Called for each client every tick
		/// Clientside: Called for each tick for local client. Can be called multiple times per tick.
		/// </summary>
		public override void Simulate( Client cl )
		{
			if ( !cl.Pawn.IsValid() ) return;

			// Block Simulate from running clientside
			// if we're not predictable.
			if ( !cl.Pawn.IsAuthority ) return;

			cl.Pawn.Simulate( cl );

			if ( IsServer )
			{
				if ( StartedCountdown )
				{
					SecondsLeft -= Time.Delta;

					if ( SecondsLeft <= 0f && SecondsLeft > -1f)
					{

						SecondsLeft = 0f;
						StartedCountdown = false;
						NextLevelCountdown = true;
						
					}
				}

				if ( NextLevelCountdown )
				{
					countdownToNextMap -= Time.Delta;
					if ( countdownToNextMap <= 0f )
					{
						if ( currentMap + 1 < LevelPlaylist.Length )
						{
							FileSystem.Data.WriteAllText( "CurrentMap.txt", (currentMap + 1).ToString() );
							Global.ChangeLevel( LevelPlaylist[currentMap + 1].Split( "," )[0] );
						}
						else
						{
							SecondsLeft = 9999;
							Log.Trace( "Ran out of maps to play! Final map ended." );
						}
					}
				}
			}
		}

		/// <summary>
		/// Called each frame on the client only to simulate things that need to be updated every frame. An example
		/// of this would be updating their local pawn's look rotation so it updates smoothly instead of at tick rate.
		/// </summary>
		public override void FrameSimulate( Client cl )
		{
			Host.AssertClient();

			if ( !cl.Pawn.IsValid() ) return;

			// Block Simulate from running clientside
			// if we're not predictable.
			if ( !cl.Pawn.IsAuthority ) return;

			cl.Pawn?.FrameSimulate( cl );
		}

		/// <summary>
		/// Called right after the level is loaded and all entities are spawned.
		/// </summary>
		public override void PostLevelLoaded()
		{
			CheckEliminatedPlayers();
			ConfigureLevelPlaylist();

			foreach (Entity ent in Entity.All )
			{
				if ( ent is not GameTypeBase)
				{
					continue;
				}

				if(ent is TerryBot )
				{
					gameType = GameTypes.RGLight;
					Log.Trace( "Found Terrybot! Setting gametype to " + gameType.ToString() );
				}

				if ( ent is GlassController )
				{
					gameType = GameTypes.GlassBridge;
					Log.Trace( "Found GlassController! Setting gametype to " + gameType.ToString() );
				}

				/*if (ent is GameTimer )
				{
					if ( !timers.Contains( (GameTimer)ent ) )
					{
						timers.Add((GameTimer)ent);
					}
				}*/
			}

			if(gameType == GameTypes.BasicElimination )
			{
				
				if ( float.Parse( LevelPlaylist[currentMap].Split( "," )[1] ) > 0 )
				{
					Log.Trace( "No Terry Game Entities found, defaulting to timed survival!" );
					StartedCountdown = true;
				}
				else
				{
					Log.Trace( "No Terry Game Entities found, defaulting to last man standing!" );
					StartedCountdown = false;
				}
			}
		}
	}
}
