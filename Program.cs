using System;
using Gtk;

namespace OrganiSmo0
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			OrgaSnismo organismo = new OrgaSnismo();
			Console.WriteLine (organismo.ToString ());
			Application.Run ();
		}
	}
}
