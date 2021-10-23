using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_glassnumbermanager", Description = "Glass bridge game number statue spawner" )]
	[Hammer.EditorModel( "models/arrow.vmdl" )]
	class GlassNumberStatueManager : GameTypeBase
	{
		int StatuesToSpawn = 0;

		List<NumberStatue> statues = new List<NumberStatue>();

		public override void Initialize()
		{
			base.Initialize();

			if(statues.Count > 0 )
			{
				for ( int i = 0; i < statues.Count; i++ )
				{
					statues[i].numberPlate.Delete();
					statues[i].Delete();
				}
				statues.Clear();
			}

			StatuesToSpawn = (Game.Current as TerryGame).TotalPlayersAlive;

			float radius = 2f;
			Vector3 vector = new Vector3( radius, 0, 0 );
			Vector3 center = Position;

			float angle = ((float)Math.PI / (float)StatuesToSpawn);

			for ( int i = 0; i < StatuesToSpawn; i++ )
			{
				float x = ((vector.x - center.x) * MathF.Cos( angle*i )) - ((center.y - vector.y) * MathF.Sin( angle*i ));
				float y = ((center.y - vector.y) * MathF.Cos( angle*i )) - ((vector.x - center.x) * MathF.Sin( angle*i ));

				Vector3 spawnpos = center + (new Vector3(x,-y,0) * 0.2f);

				NumberStatue statue1 = new NumberStatue();

				statue1.Position = spawnpos;
				statue1.Rotation = Rotation.LookAt(Position - spawnpos);
				statue1.Spawn();
				statue1.EnableAllCollisions = true;
				statue1.NumberToShow = i+1;

				statues.Add( statue1 );
			}
		}
	}
}
