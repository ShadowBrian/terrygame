using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_colorpanelmanager", Description = "color panel game manager" )]
	[Hammer.EditorModel( "models/arrow.vmdl" )]
	public partial class ColorPanelManager : GameTypeBase
	{
		public static List<ColorPanel> colorPanels = new List<ColorPanel>();

		List<int> TeamScores = new List<int>();

		Color[] TeamColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };

		string[] gunclips = { "gunshot1", "gunshot2" };

		public override void Initialize()
		{
			(Game.Current as TerryGame).StartedCountdown = true;
			TeamScores.Clear();

			for ( int i = 0; i < (Game.Current as TerryGame).TeamCount; i++ )
			{
				TeamScores.Add( 0 );
			}

			base.Initialize();
		}

		bool KilledOffPlayers;

		[Event.Tick]
		void Tick()
		{
			if ( !IsServer )
			{
				return;
			}

			if( (Game.Current as TerryGame).SecondsLeft <= 0  && !KilledOffPlayers )
			{
				foreach ( ColorPanel panel in colorPanels )
				{
					TeamScores[panel.LastSteppedTeam] += 1;
				}

				int LowestTeam = 0;

				int PanelCheck = colorPanels.Count();

				for ( int i = 0; i < TeamScores.Count; i++ )
				{
					if(TeamScores[i] < PanelCheck && TeamScores[i] != 0)
					{
						PanelCheck = TeamScores[i];
						LowestTeam = i;
					}
					Log.Trace("Team " + TeamColors[i].ToString() + " Panels: " + TeamScores[i]);
				}

				foreach ( var player in Entity.All )
				{
					if ( player is not SquidPlayer )
					{
						continue;
					}

					bool PartOfLowestTeam = (((player as SquidPlayer).PlayerNum - 1) % (Game.Current as TerryGame).TeamCount) == LowestTeam;

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
