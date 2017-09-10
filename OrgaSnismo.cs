using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Gdk;
using Cairo;
using Gtk;
using System.Diagnostics;

namespace OrganiSmo0
{
	public class OrgaSnismo
	{
		//////////////// STRUCT TO ENCAPSULATE A STATE OF THE SHAPE
		public struct state 
		{
			private List<PointD> alivecells;
			private List<PointD> nextalives;
			private List <PointD> nextdead;
			private int calculationtime;

			public state( List <PointD> alives, List<PointD> nalives, List <PointD> ndead,int calctime)
			{
				alivecells = alives;
				nextalives = nalives;
				nextdead = ndead;
				calculationtime = 0;
			}

			// Properties to attribute access
			public List <PointD>  AliveCells
			{
				get
				{
					return alivecells;
				}
				set
				{
					alivecells = value;
				}
			}
			
			public List <PointD> NextAlives
			{
				get
				{
					return nextalives;
				}
				set
				{
					nextalives = value;
				}
			}

			public List <PointD> NextDead
			{
				get
				{
					return nextdead;
				}
				set
				{
					nextdead = value;
				}
			}

			public int Calculationtime {
				get {
					return calculationtime;
				}
				set {
					calculationtime = value;
				}
			}
		}

		///////////////////////   MAIN CLASS START ///////////////////////////////////
		
		OrgaSnismoGui gui; // GUI class
	    state [] states; // To store the shape states produced by the producer thread
		int indexconsumer = 0;  // Index of the last consumed shape state
		int indexproducer = 0; // Index of the las produced shape state

		Thread producer; //Thread which makes the shape state calculation at every cycle

		state beforestate; // To save the current shape state before to produce a new shape state
 
		//List <PointD> cells; // Initial cell list
		List<int> alive; // Quatities of adjacent cells for a cell to remain alive  
		List<int> born; // Quantities of adjacent cells for a cell to born

		int cycle = 500; // By defect cycle time.
		int calculationtime = 0; // Time taken by the processor to calculate the next shape state

		DateTime tinitial; // To the calculation time
		DateTime tcalculating; //To show the overflow calculation time

		bool pause = false; // To register when the game is paused
		bool initiated = false; // To register when the cycles have been started
	    bool finalized = false; // To register when the cycles must stop
		bool loaded = false; // To register when the shape have been loaded from disc and give time to the producer thread to have advantage from the consumer thread..
		 
		/// <summary>
		/// 
		/// BUILDER
		/// 
		/// </summary>
		/// 
		public OrgaSnismo()
		{
		    gui = new OrgaSnismoGui (this);
			gui.pnZoom.Visible = false;
			gui.pnDisplacement.Visible = false;
			gui.btAccept.Visible = false;
			gui.sbPeriod.Visible = false;
			gui.graphicarea.Visible = false;
			gui.vbMetadata.Visible = false;
			born = new List<int> () { 3 }; 
			alive = new List <int>() {2,3}; 
			states = new state[50000];
			producer = new Thread (OrganismoUpdate);
			//cells = new List<PointD> ();
			gui.vbCrono.Visible = false;
			gui.sbPeriod.Value = cycle;
		}


		////////////////////////////////////////////////
		///
		///		ADDS THE CELLS IN THE DRAWN OR LOADED SHAPE AS FIRST SHAPE STATE IN THE STATE ARRAY
		///
		/////////////////////////////////////////////////////////////////////////
		///
		public void AddInitialState()
		{
					// Create a beforestate object in order to avoid errors at the Evaluate method
				beforestate = new  state (new List<PointD> (), new List<PointD> (), new List<PointD> (), 0);
					for (int i = 0; i < states [0].AliveCells.Count; i++)
						beforestate.AliveCells.Add (new PointD (states[0].AliveCells [i].X, states[0].AliveCells [i].Y));
					for (int i = 0; i < states [0].NextAlives.Count; i++)
						beforestate.NextAlives.Add (new PointD (states [0].NextAlives [i].X, states [0].NextAlives [i].Y));
					for (int i = 0; i < states [0].NextDead.Count; i++)
						beforestate.NextDead.Add (new PointD (states [0].NextDead [i].X, states [0].NextAlives [i].Y));
				    initiated = true;
				   //Register the start time and to start the producer thread
					tinitial = DateTime.Now;
					tcalculating = DateTime.Now;
					producer.Start ();
		}

		public void Dispose()
		{
			this.Dispose ();
			System.Environment.Exit (0);
			Application.Quit();
		}
		/// <summary>
		/// 
		/// UPDATES THE ALIVE AND DEAD CELL LIST AT EVERY CYCLE AND END THE GAME IN CASE THAT THE 
		/// SHAPE DISAPEAR OR STABILIZE
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// 
		//
		internal void OrganismoUpdate()
		{
			while (!finalized) {
				// Stop producing shape states in case that the states array be near to be full
				if (indexproducer == states.Length - 5)
					producer.Abort ();
				//To get the shape calculation start time
				DateTime calculationstart = DateTime.Now;
				//To add the new cells to the alive cell list and remove the dead ones
				foreach (PointD p in beforestate.NextAlives){
					if (!beforestate.AliveCells.Contains (p))
						beforestate.AliveCells.Add (new PointD (p.X, p.Y));
				}
				foreach (PointD p in beforestate.NextDead) {
					bool repeteatedexist = true;
					while (repeteatedexist)
						repeteatedexist = beforestate.AliveCells.Remove (p);
				}
				// To calculate a new shape state
				Evaluate ();
				// To assingn the states array next empty place to the new calculated shape state
				states [indexproducer + 1] = new state (new List< PointD> (), new List<PointD> (), new List <PointD> (), 0);
				foreach (PointD p in beforestate.AliveCells)
					states [indexproducer + 1].AliveCells.Add (new PointD (p.X, p.Y));
				foreach (PointD p in beforestate.NextAlives)
					states [indexproducer + 1].NextAlives.Add (new PointD (p.X, p.Y));
				foreach (PointD p in beforestate.NextDead)
					states [indexproducer + 1].NextDead.Add (new PointD (p.X, p.Y));
				// To get the new shape calculation time
				DateTime calculationend = DateTime.Now;
				calculationtime = (int)(calculationend - calculationstart).TotalMilliseconds;
				//To assign the new shape calculation time to the before state increasing it a 25%
				if (indexproducer > 0)
					states [indexproducer - 1].Calculationtime = calculationtime+ (int)((double)calculationtime * 0.25D);
				//To increase the available states index
				indexproducer++;
				//Delete the last used shape state in order to save memory
				if (indexconsumer > 1)
					states [indexconsumer - 2] = default(state);
			}
		}

		/////////////////////////////////////
		///
		///    PROPERTY TO ACCESS TO THE PRODUCER THREAD
		///
		/////////////////////////////////////////////////////////////
		///
		public Thread  Producer {
			get {
				return producer;
			}
			set {
				producer = value;
			}
		}
	
		///////////////////////////////////////
		/// 
		/// PROPERTY TO ACCESS TO THE BOOL finalized
		/// 
		/// ////////////////////////////////////////////////
		/// 
		public bool Finalized {
			get {
				return finalized;
			}
			set {
				finalized = value;
			}
		}
		///////////////////////////////////////
		/// 
		/// PROPERTY TO ACCESS TO THE born LIST ( READONLY )
		/// 
		/// ////////////////////////////////////////////////
		/// 
		public List <int>  Born {
			get {
				return born;
			}
		}

		///////////////////////////////////////
		/// 
		/// PROPERTY TO ACCESS TO THE alive LIST ( READONLY )
		/// 
		/// ////////////////////////////////////////////////
		/// 
		public List <int>  Alive {
			get {
				return alive;
			}
		}

		///////////////////////////////////////////////
		///
		/// PROPERTY TO ACCESS TO THE tcalculating ATTRIBUTE 
		///
		////////////////////////////////////////////////////////////////
		///
		public DateTime Tcalculating {
			get {
				return tcalculating;
			}
			set {
				tcalculating = value;
			}

		}
			
		///////////////////////////////////////////////////////
		///
		/// PROPERTY TO ACCESS TO THE tinitial ATTRIBUTE
		///
		///////////////////////////////////////////////////////////////////////////
		///
		public DateTime Tinitial
		{
		     get
			     {
				     return tinitial;
				  }
				  set
				  {
				     tinitial = value;
				   }
			}
				
		///////////////////////////////////////////////////
		///
		///		PROPERTY TO ACCESS TO THE beforestate ATTRIBUTE
		///
		//////////////////////////////////////////////////////////////////////////
		///
		public state Beforestate
		{
			get
			    {
					 return beforestate;
				}
				set
				{
				    beforestate = value;
				}
			}
			
		//////////////////////////////////////////////
		///
		///		PROPERTY TO ACCESS TO THE CYCLE TIME
		///
		//////////////////////////////////////////////////////////////////////////////////
		///
		public int Cycle
		{
			   get
			   {
				   return cycle;
				}
				set
				{
				    cycle = value;
				}
		}
		
		//////////////////////////////////////////
		///
		///		PROPERTY TO ACCESS TO THE STATES LIST ( READONLY)
		///
		///////////////////////////////////////////////////////////////////////////////////
		///
		public  state [] States
		{
		     get
			 {
			     return states;
			  }
		}
	
		/*
		/////////////////////////////////////////////////////
		///
		///		PROPERTY TO ACCES TO THE INITIAL CELL LIST ( READ ONLY )
		///
		////////////////////////////////////////////////////////////////////////////////////
		///
		public List <PointD> Cells 
		{
		   get
		       {
			       return cells;
			   }
		}
		*/
		////////////////////////////////////////////////////
		///
		/// PROPERTY TO ACCES TO THE bool initiated ATTRIBUTE 
		///
	    //////////////////////////////////////////////////////////////////////////////////
		///
		public bool Initiated
		{
			get
			   {
			       return initiated;
			   }
			   set
			   {
			      initiated = value;
				}
		  }
 
		////////////////////////////////////////////////////
		///
		/// PROPERTY TO ACCES TO THE loaded ATTRIBUTE ( READONLY )
		///
	    //////////////////////////////////////////////////////////////////////////////////
		///
		public bool Loaded
		{
			get
			   {
			       return loaded;
			   }
			set {
				loaded = value;
			}
		  }
	
	     ///////////////////////////////////////
		 ///
		 /// PROPERTY TO ACCESS TO THE pause ATTRIBUTE
		 ///
		 ////////////////////////////////////////////////////////////
		 ///
		 public bool  Pause
		 {
		  get
		      {
			      return pause;
			  }
			  set
			  {
			     pause = value;
			   }
			}

		///////////////////////////////////////
		///
		/// PROPERTY TO ACCESS TO THE indexproducer ATTRIBUTE
		///
		////////////////////////////////////////////////////////////
		///
		public int IndexProducer
		{
			get
			{
				return indexproducer;
			}
			set
			{
				indexproducer = value;
			}
		}

		///////////////////////////////////////
		///
		/// PROPERTY TO ACCESS TO THE indexconsumer ATTRIBUTE
		///
		////////////////////////////////////////////////////////////
		///
		public int IndexConsumer
		{
			get
			{
				return indexconsumer;
			}
			set
			{
				indexconsumer = value;
			}
		}

		/// <summary>
		/// 
		/// FORCE THE DRAWINGAREA REDRAW
		/// 
		/// </summary>
		/// 
		public void PulseTimer()
		{
			gui.graphicarea.Hide ();
			gui.graphicarea.Show();
		}

		//////////////////////////////
		/// 
		/// PROPERTY TO ACCES TO THE calculationtime ATTRIBUTE
		/// 
		/// ///////////////////////////////
		/// 
		public int Calculationtime {
			get {
				return calculationtime;
			}
			set {
				calculationtime = value;
			}
		}

		/// <summary>
		/// 
		///    UPDATES THE ALIVE AND DEAD CELLS LIST BY APPLYING THE CURRENT RULES
		/// </summary>
		/// 
		//[MethodImpl(MethodImplOptions.Synchronized)]
		private void Evaluate()
		{
			//To create a PointD / repetitions dictionary to count the repeated points around to each alive cell
			Dictionary<PointD,int> pointsrep = new Dictionary<PointD, int> ();
			//Increase the repetitions if the adjacent point already exists in the dictionary or add it if not
			foreach (PointD p in beforestate.AliveCells) {
				PointD point1 = new PointD (p.X - gui.CellSize, p.Y - gui.CellSize);
				if (pointsrep.ContainsKey (point1))
					pointsrep [point1]++;
				else
					pointsrep.Add (point1, 1);
				PointD point2 = new PointD (p.X - gui.CellSize, p.Y);
				if (pointsrep.ContainsKey (point2))
					pointsrep [point2]++;
				else
					pointsrep.Add (point2, 1);
				PointD point3 = new PointD (p.X - gui.CellSize, p.Y + gui.CellSize);
				if (pointsrep.ContainsKey (point3))
					pointsrep [point3]++;
				else
					pointsrep.Add (point3, 1);
				PointD point4 = new PointD (p.X, p.Y + gui.CellSize);
				if (pointsrep.ContainsKey (point4))
					pointsrep [point4]++;
				else
					pointsrep.Add (point4, 1);
				PointD point5 = new PointD (p.X + gui.CellSize, p.Y + gui.CellSize);
				if (pointsrep.ContainsKey (point5))
					pointsrep [point5]++;
				else
					pointsrep.Add (point5, 1);
				PointD point6 = new PointD (p.X + gui.CellSize, p.Y);
				if (pointsrep.ContainsKey (point6))
					pointsrep [point6]++;
				else
					pointsrep.Add (point6, 1);
				PointD point7 = new PointD (p.X + gui.CellSize, p.Y - gui.CellSize);
				if (pointsrep.ContainsKey (point7))
					pointsrep [point7]++;
				else
					pointsrep.Add (point7, 1);
				PointD point8 = new PointD (p.X, p.Y - gui.CellSize);
				if (pointsrep.ContainsKey (point8))
					pointsrep [point8]++;
				else
					pointsrep.Add (point8, 1);
			}
		    //To reset the next alive and dead cell lists
			beforestate.NextAlives.Clear ();
			beforestate.NextDead.Clear ();
			//To count the adjacent cells repetitions
			foreach (KeyValuePair <PointD,int> k in pointsrep) {
				if (born.Contains (k.Value) && !beforestate.AliveCells.Contains (k.Key) && !beforestate.NextAlives.Contains (k.Key))
					beforestate.NextAlives.Add (new PointD (k.Key.X, k.Key.Y));
				else if (beforestate.AliveCells.Contains (k.Key) && !alive.Contains (k.Value))
					beforestate.NextDead.Add (new PointD (k.Key.X, k.Key.Y));
			}
		    // Remove the isolated cells
			foreach (PointD p in beforestate.AliveCells) {
				bool isolated = true;
				if (beforestate.AliveCells.Contains (new PointD (p.X - gui.CellSize, p.Y - gui.CellSize)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X - gui.CellSize, p.Y)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X - gui.CellSize, p.Y + gui.CellSize)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X, p.Y + gui.CellSize)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X + gui.CellSize, p.Y + gui.CellSize)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X + gui.CellSize, p.Y)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X + gui.CellSize, p.Y - gui.CellSize)))
					isolated = false;
				else if (isolated && beforestate.AliveCells.Contains (new PointD (p.X, p.Y - gui.CellSize)))
					isolated = false;
				if (isolated && !alive.Contains (0))
					beforestate.NextDead.Add (new PointD (p.X, p.Y));
			}
		    //To add the new cells to the alive cells list and remove the dead ones
			foreach (PointD p in beforestate.NextAlives){
				if (!beforestate.AliveCells.Contains (p))
					beforestate.AliveCells.Add (new PointD (p.X, p.Y));
			}
			foreach (PointD p in beforestate.NextDead) {
				bool repeteatedexist = true;
				while (repeteatedexist)
					repeteatedexist = beforestate.AliveCells.Remove (p);
			}
			
		}
	

		/// <summary>
		/// 
		/// HANDLER FOR THE MAIN WINDOW CLOSING EVENT
		/// 
		/// </summary>
		/// <param name="o">O.</param>
		/// <param name="args">Arguments.</param>
		/// 
		public void win_DeleteEvent (object o, DeleteEventArgs args)
		{
			producer.Abort ();
			Thread.Sleep (200);
			System.Environment.Exit (0);
			Application.Quit ();
		}






	










	}
}

