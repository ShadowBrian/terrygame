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
			BasicElimination,//this automatically sorts into one of the two below.
			TimedSurvival,//Survive the timer running out- basic obstacle courses?
			LastManStanding,//Basic last man wins situation.
			RGLight,//Squid game ripoff.
			TugOfWar,//Squid game ripoff. Not sure about this one yet... lame gameplay?
			GlassBridge,//Squid game ripoff.
			BombTag,//Crab game ripoff, transfer bombs, half the players (rounded down) get bombs.
			KingOfTheHill,//Crab game ripoff, central point, being inside it means getting points, end of round above average points live.
			CrownKeeper,//Crab game ripoff, transfer crowns, half the players get crowns, end of round above average points live.
			ColorPanels //Crab game ripoff, split players up in teams, run around on grey panels, get colored to team, least colored panels gets killed.
		}

		public bool StartedCountdown;

		public float SecondsLeft = 60;

		public List<string> eliminatedPlayers = new List<string>();

		public List<string> playerNumbers = new List<string>();

		[Net]
		public int TotalPlayersAlive { get; set; }

		[Net]
		public int TeamCount { get; set; }

		string[] LevelPlaylist;

		int currentMap = 0;

		List<AnimEntity> bombs = new List<AnimEntity>();

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new SquidPlayer();

			client.Pawn = player;

			CheckEliminatedPlayers();
			CheckPlayerNumbers();

			if ( eliminatedPlayers.Contains( client.Name ) )
			{
				player.Died = true;
			}
			else
			{
				if( playerNumbers.Contains( client.Name ) )
				{
					player.PlayerNum = playerNumbers.IndexOf( client.Name ) + 1;
				}
				else
				{
					playerNumbers.Add( client.Name );
					player.PlayerNum = playerNumbers.IndexOf( client.Name ) + 1;
					UpdateNumberFile();
				}
			}

			if(gameType == GameTypes.ColorPanels )
			{
				TeamCount = Math.Clamp(TotalPlayersAlive,1,4);

				player.TeamsCount = TeamCount;

				player.TeamBased = true;
			}

			//Log.Trace( "Players alive: " + TotalPlayersAlive );

			foreach ( GameTypeBase gameObject in GameAssets )
			{
				gameObject.Initialize();
			}

			player.Respawn();

			if(gameType == GameTypes.BombTag && Rand.Float(0,100f) > 50f && !player.Died && bombs.Count <= MathF.Ceiling(TotalPlayersAlive/2f))
			{
				AnimEntity bombEnt = new AnimEntity();
				bombEnt.SetModel( "models/bombtag_bomb.vmdl" );
				bombEnt.SetParent( player, "hand_L" );


				bombEnt.LocalPosition = new Vector3(3f,0,5f);

				bombEnt.LocalRotation = new Angles( 0, 0, -90 ).ToRotation();

				//Particles sparks = Particles.Create( "particles/fuse_sparks.vpcf", bombEnt );

				bombs.Add( bombEnt );

				player.Bomb = bombEnt;
			}
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
				FileSystem.Data.WriteAllText( "tempfiles/Eliminations.txt", String.Join( ",", eliminatedPlayers ) );
				Log.Trace( "Player " + name + " #" + (playerNumbers.IndexOf(name) + 1).ToString("000") + " eliminated!" );
			}
		}

		public void CheckEliminatedPlayers()
		{
			if ( FileSystem.Data.FileExists( "tempfiles/Eliminations.txt" ) )
			{
				eliminatedPlayers = FileSystem.Data.ReadAllText( "tempfiles/Eliminations.txt" ).Split(",").ToList();
			}
			else
			{
				eliminatedPlayers = new List<string>();
			}
		}

		public void CheckPlayerNumbers()
		{
			if ( FileSystem.Data.FileExists( "tempfiles/Numbers.txt" ) )
			{
				playerNumbers = FileSystem.Data.ReadAllText( "tempfiles/Numbers.txt" ).Split( "," ).ToList();

				TotalPlayersAlive = playerNumbers.Count - eliminatedPlayers.Count;
			}
		}

		public void UpdateNumberFile()
		{
			FileSystem.Data.WriteAllText( "tempfiles/Numbers.txt", String.Join( ",", playerNumbers ) );
		}

		public void RespawnPlayers()
		{
			Global.ChangeLevel( LevelPlaylist[currentMap].Split( "," )[0] );
		}

		[AdminCmd( "tg_reset" )]
		public static void ResetAndRespawnPlayers()
		{
			if ( FileSystem.Data.FileExists( "tempfiles/CurrentMap.txt" ) )
			{
				FileSystem.Data.DeleteFile( "tempfiles/CurrentMap.txt" );
			}
			if ( FileSystem.Data.FileExists( "tempfiles/Eliminations.txt" ) )
			{
				FileSystem.Data.DeleteFile( "tempfiles/Eliminations.txt" );
			}
			if ( FileSystem.Data.FileExists( "tempfiles/Numbers.txt" ) )
			{
				FileSystem.Data.DeleteFile( "tempfiles/Numbers.txt" );
			}
			FileSystem.Data.CreateDirectory( "tempfiles" );
			(Game.Current as TerryGame).CheckEliminatedPlayers();
			(Game.Current as TerryGame).ConfigureLevelPlaylist();
			(Game.Current as TerryGame).CheckPlayerNumbers();
			(Game.Current as TerryGame).RespawnPlayers();
		}

		public override void DoPlayerNoclip( Client player )
		{
			//base.DoPlayerNoclip( player );
			//DoPlayerSuicide( player );
		}

		public override void Shutdown()
		{
			if ( IsServer )
			{
				if ( currentMap + 1 >= LevelPlaylist.Length )
				{
					if ( FileSystem.Data.FileExists( "tempfiles/CurrentMap.txt" ) )
					{
						FileSystem.Data.DeleteFile( "tempfiles/CurrentMap.txt" );
					}
					if ( FileSystem.Data.FileExists( "tempfiles/Eliminations.txt" ) )
					{
						FileSystem.Data.DeleteFile( "tempfiles/Eliminations.txt" );
					}
					if ( FileSystem.Data.FileExists( "tempfiles/Numbers.txt" ) )
					{
						FileSystem.Data.DeleteFile( "tempfiles/Numbers.txt" );
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

				if ( FileSystem.Data.FileExists( "tempfiles/CurrentMap.txt" ) ) {

					currentMap = int.Parse( FileSystem.Data.ReadAllText( "tempfiles/CurrentMap.txt" ) );
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
					FileSystem.Data.WriteAllText( "tempfiles/CurrentMap.txt", currentMap.ToString() );
				}
			}
			else
			{
				FileSystem.Data.WriteAllText( "LevelPlaylist.txt", "tg_rglight_courtyard\ntg_koth\ntg_colorpanels\ntg_bombtag\ntg_glassbridge" );

				//tg_colorpanels,120
				//tg_bombtag,60

				ConfigureLevelPlaylist();
			}
		}

		public override void Spawn()
		{
			//CheckEliminatedPlayers();
			//ConfigureLevelPlaylist();

			//SecondsLeft = float.Parse(LevelPlaylist[currentMap].Split( "," )[1]);

			if ( IsServer && !Input.VR.IsActive)
			{
				new MinimalHudEntity();
			}

			base.Spawn();
		}

		float countdownToNextMap = 10f;

		bool NextLevelCountdown;

		async Task MakeItRain(Entity player, int count, float Delay = 0.1f )
		{
			for ( int i = 0; i <= count; i++ )
			{
				await Task.DelaySeconds( Delay );
				ModelEntity MoneyEnt = new ModelEntity( );
				MoneyEnt.Position = player.Position + Vector3.Up * 100 + (new Vector3( 1f * Rand.Float( -10f, 10f ), 1f * Rand.Float( -10f, 10f ), 1f * Rand.Float(-10f,10f)));
				MoneyEnt.Rotation = Rotation.LookAt( player.Position - MoneyEnt.Position );
				MoneyEnt.SetModel( "models/winner_bill.vmdl" );
				MoneyEnt.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			}
		}

		[Event.Tick.Server]
		void Tick()
		{
			if ( StartedCountdown )
			{
				SecondsLeft -= Time.Delta;

				if ( SecondsLeft <= 0f && SecondsLeft > -1f )
				{
					SecondsLeft = 0f;
					StartedCountdown = false;
					NextLevelCountdown = true;
				}
			}

			if(!StartedCountdown && gameType == GameTypes.LastManStanding )
			{
				if(TotalPlayersAlive == 1 )
				{
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
						FileSystem.Data.WriteAllText( "tempfiles/CurrentMap.txt", (currentMap + 1).ToString() );
						Global.ChangeLevel( LevelPlaylist[currentMap + 1].Split( "," )[0] );
						
					}
					else
					{
						SecondsLeft = 9999;
						Log.Trace( "Ran out of maps to play! Final map ended." );
						foreach ( Client player in Client.All )
						{
							if ( !eliminatedPlayers.Contains( player.Name ) )
							{
								Log.Trace( player.Name + " Survived!" );
								MakeItRain( player.Pawn, Client.All.Count * 100 );
							}
						}
						NextLevelCountdown = false;
					}
				}
			}
		}

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

		List<GameTypeBase> GameAssets = new List<GameTypeBase>();

		/// <summary>
		/// Called right after the level is loaded and all entities are spawned.
		/// </summary>
		public override void PostLevelLoaded()
		{
			FileSystem.Data.CreateDirectory( "tempfiles" );
			CheckEliminatedPlayers();
			ConfigureLevelPlaylist();
			CheckPlayerNumbers();

			//SecondsLeft = float.Parse( LevelPlaylist[currentMap].Split( "," )[1] );

			foreach (Entity ent in Entity.All.OfType<GameTypeBase>())
			{
				GameAssets.Add( ent as GameTypeBase );

				if ( gameType == GameTypes.BasicElimination )
				{

					if ( ent is TerryBot )
					{
						gameType = GameTypes.RGLight;
						Log.Trace( "Found Terrybot! Setting gametype to " + gameType.ToString() );
					}

					if ( ent is GlassController )
					{
						gameType = GameTypes.GlassBridge;
						Log.Trace( "Found GlassController! Setting gametype to " + gameType.ToString() );
					}

					if ( ent is TugOfWarRopeManager )
					{
						gameType = GameTypes.TugOfWar;
						Log.Trace( "Found TugOfWarRopeManager! Setting gametype to " + gameType.ToString() );
					}

					if(ent is TimedSurvival )
					{
						gameType = GameTypes.TimedSurvival;
						Log.Trace( "Found TimedSurvival settings! Setting gametype to " + gameType.ToString() );
					}

					if ( ent is ColorPanelManager )
					{
						gameType = GameTypes.ColorPanels;
						Log.Trace( "Found ColorPanelManager! Setting gametype to " + gameType.ToString() );
					}

					if ( ent is BombTagManager )
					{
						gameType = GameTypes.BombTag;
						Log.Trace( "Found BombTagManager! Setting gametype to " + gameType.ToString() );
					}

					if ( ent is KingOfTheHillManager )
					{
						gameType = GameTypes.KingOfTheHill;
						Log.Trace( "Found KingOfTheHillManager! Setting gametype to " + gameType.ToString() );
					}
				}
			}

			if(gameType == GameTypes.BasicElimination )
			{
				
				if ( float.Parse( LevelPlaylist[currentMap].Split( "," )[1] ) > 0 )
				{
					Log.Trace( "No Terry Game Entities found, defaulting to timed survival!" );
					StartedCountdown = true;
					gameType = GameTypes.TimedSurvival;
				}
				else
				{
					Log.Trace( "No Terry Game Entities found, defaulting to last man standing!" );
					StartedCountdown = false;
					gameType = GameTypes.LastManStanding;
				}
			}
		}
	}
}
