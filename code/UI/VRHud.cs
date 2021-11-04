using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace terrygame
{
	class VRHud : WorldPanel
	{
		public VRHud(int PlayerNumber = 0)
		{
			SetTemplate( "/UI/MinimalHud.html" );
			StyleSheet.Load( "MinimalHud.scss" );
			Add.Label( "Player number: " + PlayerNumber.ToString( "000" ), "bigtext" );
		}
	}
}
