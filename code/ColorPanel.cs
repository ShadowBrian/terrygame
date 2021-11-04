using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_colorpanel", Description = "Color panel, will tint according to who touched it last" )]
	[Hammer.SupportsSolid()]
	public partial class ColorPanel : GameTypeBase
	{
		Color[] TeamColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };

		[Net]
		public int LastSteppedTeam { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = true;
			EnableTouch = true;

			ColorPanelManager.colorPanels.Add( this );
		}

		public override void StartTouch( Entity other )
		{
			if ( other is SquidPlayer )
			{
				SquidPlayer player = other as SquidPlayer;
				LastSteppedTeam = (player.PlayerNum - 1) % (Game.Current as TerryGame).TeamCount;
				RenderColor = (TeamColors[LastSteppedTeam] * 0.7f).WithAlpha( 1f );

				Sound.FromEntity( "beep3", this );
			}
		}
	}
}
