using System;
using Gtk;
using Organismo0;

namespace OrganiSmo0
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			OrgaSnismo organismo = new OrgaSnismo();
			Application.Run ();
		}
	}
}
