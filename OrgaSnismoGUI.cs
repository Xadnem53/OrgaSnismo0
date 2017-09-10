using System;
using Gtk;
using Gdk;
using Pango;
using Cairo;
using OrganiSmo0;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace OrganiSmo0
{
	public class OrgaSnismoGui
	{
		OrgaSnismo organismo;  
		
		int cellsize = 25; //By defect cell size
		internal int cycles = 0; // Quantity of cycles from the start
		string savingpath; // File saving path
		bool initiated = false; // To register when the cycles have been started

		Gtk.Window win; // Gtk container window

		Label lbEgo;
		Label lbPopulation; //To  show the cell population count
		Label lbCycles; // To show the cycles count
		Label lbAutoCycle; // To show when the program is at the automatic cycle mode..
		Label lbRegresive; // To show the next cycle remaining time..

		bool autocycle;// = false; // To to register when the program is at the automatic cycle mode
	    bool autosaving = false; // To to register when the image autosaving option is active
		bool Anglo = true; // To register when the selected language is English ( default )

		Cairo.Color exterior = new Cairo.Color (1, 0, 0); //By defect cell external line color
		Cairo.Color interior = new Cairo.Color(0.65D, 1, 0.1D); // By defect  cell color
		Cairo.Color backgroundC = new Cairo.Color(0,0,0); // By defect background color
		Cairo.Color interiorD = new Cairo.Color(0,0,0); // By defect dead cells interior color.
		Cairo.Color interiorA = new Cairo.Color(0.65D,1,0.1D); // By defect born cells interior color

		FileChooserDialog savedialog; //To save the current shape

		MenuBar menu;

		Menu filemenu;
		MenuItem mfile;
		MenuItem mnew;
		MenuItem msave;
		MenuItem mload;
		MenuItem msaveimage;

		Menu menuoptions;
		MenuItem moptions;
		MenuItem mrules;
		MenuItem mbacktodefect;
		MenuItem mchangecolors;
		MenuItem mlanguage;
		MenuItem mSpanish;
		MenuItem mEnglish;
		MenuItem mimageautosave;

		MenuItem menuhelp;

		VBox vbMenu;
		internal VBox vbMetadata; //To the cycle spinbutton and the labels for population and cycles count
		internal VBox vbCrono; // To the manual cycle check box and the label for the regresive time counter

		HBox hbContinue; //To the continue button

		Gtk.Alignment almenu; //To the menu bar
		Gtk.Alignment alrb; //To the draw shape button
		Gtk.Alignment alGraphicArea; //To the DrawingArea
		Gtk.Alignment alEgo; //To the Ego label
		Gtk.Alignment alAccept; // To the Accept button
		Gtk.Alignment alPnDisplacement; // To the display displacement buttons
		Gtk.Alignment alPnZoom; // To the zoom buttons

		internal Fixed pnDisplacement; // Tor the display displacement buttons
		internal Fixed pnZoom; //To the zoom buttons

		internal Button btDrawShape;  // To draw the initial shape
		internal Button btPause; // To pause the game
		internal Button btAccept; // To accept the drawn shape
		Button btLeft; // To left scrolling
		Button btRight; // To right scrolling
		Button btUp; // To up scrolling
		Button btDown; // To down scrolling
		Button btZoomMore; //To increase the zoom
		Button btZoomLess; // To reduce the zoom

		internal CheckButton cbManual; // To manual cycle time selection

		internal SpinButton sbPeriod; // To stablish the cycle time

		internal DrawingArea graphicarea; // To the graphics

		Cairo.Context contexto;

		double scale = 1; // Visualization scale by defect
		double displx = 0;// X sense origin point displacement
		double desply = 0;//Y sense origin point displacement

		DateTime tfinal; // To get the process final time

		ColorSelectionDialog dialogol;
		ColorSelectionDialog dialogov;
		ColorSelectionDialog dialogon;
		ColorSelectionDialog dialogom;
		ColorSelectionDialog dialogof;

		Button btLines;
		Button btalivecells;
		Button btBornCells;
		Button btDeadCells;
		Button btbackgroundC;
		
		/// <summary>
		/// 
		/// BUILDER
		/// 
		/// </summary>
		/// <param name="organism">Organism.</param>
		///
	    public OrgaSnismoGui( OrgaSnismo organism )
		{
			organismo = organism;
			graphicarea = new DrawingArea ();
			graphicarea.SetSizeRequest (1300, 592);
			graphicarea.ExposeEvent += graphicarea_ExposeEvent;
			graphicarea.Events = EventMask.PointerMotionMask;
			graphicarea.Events = EventMask.ButtonPressMask;

			win = new Gtk.Window ("OrgaSnismo0                                Twister edition");
			win.SetSizeRequest (1000, 600);
			win.Maximize ();
			win.Visible = true;
			win.ModifyBg (StateType.Normal, new Gdk.Color (0, 0, 0));
			win.DeleteEvent += win_DeleteEvent;

			lbEgo = new Label ();
		
			//Menu bar
			menu = new MenuBar ();  

			//Menu file
			mfile = new MenuItem ("File");
			filemenu = new Menu (); 
			mfile.Submenu = filemenu;

			mnew = new MenuItem ("New");
			mnew.Visible = true;
			msave = new MenuItem ("Save");
			mload = new MenuItem ("Load");
			msaveimage = new MenuItem ("Save image");

			filemenu.Append (mnew);
			filemenu.Append (msave);
			filemenu.Append (mload);
			filemenu.Append (msaveimage);

			mnew.Activated += New_Onactivated;
			msave.Activated += Save_Onactivated;
			mload.Activated += mLoad_Onactivated;
			msaveimage.Activated += SaveImage_Onactivated;

			//Menu  Options
			menuoptions = new Menu ();
			moptions = new MenuItem ("Options");
			moptions.Submenu = menuoptions;

			mrules = new MenuItem ("Change rules.");
			mbacktodefect = new MenuItem ("Back to by defect values.");
			mchangecolors = new MenuItem ("Change colors.");
			mimageautosave = new MenuItem ("Automatic image saving.");
			mlanguage = new MenuItem ("Language");
			mSpanish = new MenuItem ("Spanish");
			mSpanish.Activated += mSpanish_Activated;
			mEnglish = new MenuItem ("English");
			mEnglish.Activated += mEnglish_Activated;

			menuoptions.Append (mrules);
			menuoptions.Append (mchangecolors);
			menuoptions.Append (mbacktodefect);
			menuoptions.Append (mimageautosave);
			menuoptions.Append (mlanguage);

			Menu mlanguages = new Menu ();
			mlanguage.Submenu = mlanguages;

			mlanguages.Append (mSpanish);
			mlanguages.Append (mEnglish);

			mrules.Activated += mrules_Activated;
			mbacktodefect.Activated += mbacktodefect_Activated;
			mchangecolors.Activated += mchangecolors_Activated;
			mimageautosave.Activated += mimageautosave_Activated;

			menuhelp = new MenuItem ("Help");
			menuhelp.Activated += menuhelp_Activated;

			menu.Append (mfile);
			menu.Append (moptions);
			menu.Append (menuhelp);

			vbMenu = new VBox ();
			almenu = new Gtk.Alignment (0, 0, 0, 0);
			vbMenu.PackStart (menu, false, false, 0);
			vbMenu.Add (almenu);

			alGraphicArea = new Gtk.Alignment (0, 0, 1, 1);
			alGraphicArea.Add (graphicarea); 
			vbMenu.Add (alGraphicArea);

			//To add the HBox with the Ego label and the scroll and zoom buttons
			alEgo = new Gtk.Alignment (0, 0, 0, 0);
			alEgo.HeightRequest = 205;
			hbContinue = new HBox ();
			hbContinue.HeightRequest = 205;
			hbContinue.PackEnd (alEgo);

			alAccept = new Gtk.Alignment (1F, 0.5F, 0, 0);
			alAccept.Visible = true;

			btAccept = new Button ();
			btAccept.Label = "Accept";
			btAccept.SetSizeRequest (100, 30);
			btAccept.ModifyBg (StateType.Normal, new Gdk.Color (0, 240, 30));
			btAccept.Visible = true;
			btAccept.Clicked += btAccept_Clicked;
			alAccept.Add (btAccept);
			hbContinue.PackEnd (alAccept);

			//To build the panel with the scrolling buttons
			alPnDisplacement = new Gtk.Alignment (0.5F, 0.5F, 0, 0);
			alPnDisplacement.Visible = true;

			btRight = new Button ();
			btRight.ModifyBg (StateType.Normal, new Gdk.Color (0, 0, 180));
			btRight.Label = char.ToString ('\u25B6');
			btRight.Visible = true;
			btRight.SetSizeRequest (50, 50);
			btRight.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btRight.Clicked += btRight_Clicked;

			btUp = new Button ();
			btUp.ModifyBg (StateType.Normal, new Gdk.Color (0, 240, 240));
			btUp.Label = char.ToString ('\u25B2');
			btUp.Visible = true;
			btUp.SetSizeRequest (50, 50);
			btUp.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btUp.Clicked += btUp_Clicked;

			btDown = new Button ();
			btDown.ModifyBg (StateType.Normal, new Gdk.Color (240, 0, 0));
			btDown.Label = char.ToString ('\u25BC');
			btDown.Visible = true;
			btDown.SetSizeRequest (50, 50);
			btDown.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btDown.Clicked += btDown_Clicked;

			btLeft = new Button ();
			btLeft.ModifyBg (StateType.Normal, new Gdk.Color (200, 0, 200));
			btLeft.Label = char.ToString ('\u25C0');
			btLeft.Visible = true;
			btLeft.SetSizeRequest (50, 50);
			btLeft.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btLeft.Clicked += btLeft_Clicked;

			pnDisplacement = new Fixed ();
			pnDisplacement.SetSizeRequest (150, 150);
			pnDisplacement.Visible = true;
			pnDisplacement.Put (btRight, 100, 50);
			pnDisplacement.Put (btUp, 50, 0);
			pnDisplacement.Put (btDown, 50, 100);
			pnDisplacement.Put (btLeft, 0, 50);

			alPnDisplacement.Add (pnDisplacement);
			hbContinue.PackEnd (alPnDisplacement);
			
			// To build the panel for the zoom buttons
			btZoomMore = new Button ();
			btZoomMore.SetSizeRequest (50, 50);
			btZoomMore.Label = "+";
			btZoomMore.Visible = true;
			btZoomMore.ModifyBg (StateType.Normal, new Gdk.Color (180, 180, 180));
			btZoomMore.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans 17"));
			btZoomMore.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 50));
			btZoomMore.Clicked += btZoomMore_Clicked;

			btZoomLess = new Button ();
			btZoomLess.SetSizeRequest (50, 50);
			btZoomLess.Label = "-";
			btZoomLess.Visible = true;
			btZoomLess.ModifyBg (StateType.Normal, new Gdk.Color (80, 80, 80));
			btZoomLess.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 29"));
			btZoomLess.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 240, 100));
			btZoomLess.Clicked += btZoomLess_Clicked;
			alPnZoom = new Gtk.Alignment (0.5F, 0.5F, 0, 0);
			alPnZoom.WidthRequest = 350;
			alPnZoom.Visible = true;

			pnZoom = new Fixed ();
			pnZoom.SetSizeRequest (150, 50);
			pnZoom.Visible = true;
			pnZoom.Put (btZoomMore, 100, 0);
			pnZoom.Put (btZoomLess, 0, 0);

			alPnZoom.Add (pnZoom);
			hbContinue.PackEnd (alPnZoom);

			vbCrono = new VBox (true, 10);
			vbCrono.Visible = true;
			vbCrono.WidthRequest = 300;

			cbManual = new CheckButton ("Manual cycle");
			cbManual.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
			cbManual.Children [0].ModifyFg (StateType.Active, new Gdk.Color (0, 250, 0));
			cbManual.Visible = true;
			cbManual.Clicked += CbManual_Clicked;

			lbRegresive = new Label ();
			lbRegresive.WidthRequest = 300;
			lbRegresive.Visible = true;
			lbRegresive.Text = "Next cycle: ";
			lbRegresive.ModifyFont (FontDescription.FromString ("Dejavu-Sans 13"));
			lbRegresive.ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));

			vbCrono.Add (cbManual);
			vbCrono.Add (lbRegresive);
			Gtk.Alignment alvbCrono = new Gtk.Alignment (0.5F, 0.5F, 0, 0);
			alvbCrono.Visible = true;
			alvbCrono.Add (vbCrono);

			hbContinue.PackEnd (alvbCrono);

			vbMetadata = new VBox ();
			vbMetadata.WidthRequest = 200;
			vbMetadata.Visible = true;

			Gtk.Alignment alCycleTitle = new Gtk.Alignment (0, 0.5F, 0, 0);
			alCycleTitle.Visible = true;
			lbAutoCycle = new Label ();
			lbAutoCycle.ModifyFont (FontDescription.FromString ("Dejavu-Sans 11"));
			lbAutoCycle.ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 240));
			lbAutoCycle.Visible = true;
			lbAutoCycle.SetSizeRequest (400, 30);
			lbAutoCycle.Text = "Period in milliseconds     manual";
			alCycleTitle.Add (lbAutoCycle);
			vbMetadata.Add (alCycleTitle);

			sbPeriod = new SpinButton (100, 10000000, 100);
			sbPeriod.ModifyFont (FontDescription.FromString ("Dejavu-Sans 17"));
			sbPeriod.Visible = true;
			sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (0, 240, 0));
			sbPeriod.ValueChanged += sbPeriod_ValueChanged;
			vbMetadata.Add (sbPeriod);

			Gtk.Alignment alPopulation = new Gtk.Alignment (0, 0.5F, 0, 0);
			alPopulation.Visible = true;

			lbPopulation = new Label ();
			lbPopulation.Visible = true;
			lbPopulation.Text = "Population: ";
			lbPopulation.ModifyFont (FontDescription.FromString ("Dejavu-Sans 13"));
			lbPopulation.ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 240));
			alPopulation.Add (lbPopulation);
			vbMetadata.Add (alPopulation);

			Gtk.Alignment alCycles = new Gtk.Alignment (0, 0.5F, 0, 0);
			alCycles.Visible = true;

			lbCycles = new Label ();
			lbCycles.Visible = true;
			lbCycles.Text = "Cycles: ";
			lbCycles.ModifyFont (FontDescription.FromString ("Dejavu-Sans 13"));
			lbCycles.ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 240));
			alCycles.Add (lbCycles);
			vbMetadata.Add (alCycles);
			hbContinue.PackEnd (vbMetadata);

			vbMenu.Add (hbContinue);

			//To add the  mouse left button click event to the DrawingArea
			graphicarea.AddEvents (1);
			graphicarea.ButtonPressEvent += graphicarea_ButtonPressEvent;

			btDrawShape = new Button ();
			btDrawShape.Visible = true;
			btDrawShape.ModifyBg (StateType.Normal, new Gdk.Color (0, 100, 200));
			btDrawShape.Label = "Draw initial shape.";
			btDrawShape.Clicked += DrawInitialShape;

			alrb = new Gtk.Alignment (0, 0F, 0, 0);
			alrb.SetSizeRequest (1000, 550);
			alrb.Add (btDrawShape);
			vbMenu.Add (alrb);

			hbContinue = new HBox ();
			hbContinue.Visible = true;
			hbContinue.SetSizeRequest (1200, 50);

			alEgo = new Gtk.Alignment (1, 1F, 0, 0);
			alEgo.SetSizeRequest (100, 50);
			alEgo.Visible = true;

			lbEgo = new Label ();
			lbEgo.HasTooltip = true;
			lbEgo.QueryTooltip += LbEgo_QueryTooltip;
			lbEgo.Text = " ";
			lbEgo.Visible = true;
			lbEgo.ModifyFg (StateType.Normal, new Gdk.Color (0, 200, 0));
			alEgo.Add (lbEgo);

			hbContinue.Add (alEgo);

			vbMenu.Add (hbContinue);
			vbMenu.Add (alEgo);
 
			win.Add (vbMenu);
			win.ShowAll ();

			Console.WriteLine (autocycle.ToString ());
		}

		///////////////////////////////////////////////
		///
		///   HANDLER FOR THE FORM CLOSING EVENT
		///
		///////////////////////////////////////////////////////////////////////////////
		///
		public void win_DeleteEvent (object o, DeleteEventArgs args)
		{
			organismo.Producer.Abort ();
			contexto = null;
			organismo.Dispose ();
			System.Environment.Exit (0);
			Application.Quit ();
		}

		/// <summary>
		/// 
		/// PROPERTY TO ACCESS TO THE cellsize ATTRIBUTE
		/// 
		/// </summary>
		/// <value>The size of the cell.</value>
		/// 
		public int CellSize {
			get {
				return cellsize;
			}
			set {
				cellsize = value;
			}
		}

		/// <summary>
		/// 
		/// HANDLER FOR THE btDrawShape WHICH ALLOWS
		/// TO DRAW A NEW SHAPE ON THE GRID
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void DrawInitialShape (object sender, EventArgs e)
		{
			//Create the initial shape state
			organismo.States [0] = new OrgaSnismo.state (new List<PointD> (), new List<PointD> (), new List<PointD> (), 0);
			//To hide the Ego label just in case
			lbEgo.Text = " ";
			//To show and hide the corresponding controls
			alrb.Hide ();
			hbContinue.HeightRequest += 10;
			almenu.HeightRequest = 40;
			sbPeriod.Visible = true;
			btAccept.Visible = true;
			vbMetadata.Visible = true;
			lbCycles.Visible = false;
			lbPopulation.Visible = false;
			vbCrono.Show ();
			cbManual.Active = true;
			lbRegresive.Visible = false;
			//Call to the DrawingArea show method
			graphicarea.Show ();
			// If the shape has been loaded, to show the cycle time
			if (organismo.Loaded)
				{
				sbPeriod.Value = organismo.Calculationtime;
				cbManual.Active = false;
			} 
		}

		/// <summary>
		/// 
		/// BUTTONPRESS EVENT HANDLER THAT DRAW A NEW CELL IN THE LOCATION  WHERE 
		/// THE USER CLICKED AND ADDS IT TO THE CELL LIST. IF THE CELL ALREADY EXISTS, IT 
		/// IS ERASED FROM THE LIST AND FROM THE GRID
		///
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		/// 
		public void graphicarea_ButtonPressEvent (object sender, ButtonPressEventArgs args)
		{
			//In case the shape to be a loaded shape, diable the new cells drawing
			if (organismo.Loaded) {
				return;
			}
			contexto = Gdk.CairoHelper.Create (graphicarea.GdkWindow);
			int x = 0;
			int y = 0;
			graphicarea.GetPointer (out x, out y);
			// To get the upper left vertex of the grid  square where the user clicked
			int hposition = 0;
			int vposition = 0;
			for (int i = 0; i < x; i += cellsize) {
				hposition = i;
			}
			for (int i = 0; i < y; i += cellsize) {
				vposition = i;
			}
			PointD cell = new PointD (hposition, vposition);
	
			//To draw the new cell on the grid and add it to the alives cells list
			//if (!organismo.Cells.Contains (cell)) {
			if (!organismo.States[0].AliveCells.Contains (cell)) {
			contexto.MoveTo (new PointD (hposition, vposition));
				contexto.LineTo (new PointD (hposition + cellsize, vposition));
				contexto.LineTo (new PointD (hposition + cellsize, vposition + cellsize));
				contexto.LineTo (new PointD (hposition, vposition + cellsize));
				contexto.LineTo (new PointD (hposition, vposition));
				contexto.SetSourceColor (new Cairo.Color (1, 1, 0.1));
				contexto.Fill ();
				organismo.States [0].AliveCells.Add (cell);
				//organismo.Cells.Add (cell);
			} else { // If the cell already exists to erase it from the grid and from the alive cells list
				contexto.MoveTo (cell);
				contexto.LineTo (new PointD (hposition + cellsize, vposition));
				contexto.LineTo (new PointD (hposition + cellsize, vposition + cellsize));
				contexto.LineTo (new PointD (hposition, vposition + cellsize));
				contexto.LineTo (cell);
				contexto.SetSourceColor (new Cairo.Color (0, 0, 0));
				contexto.Fill ();
				//organismo.Cells.Remove (cell);
				organismo.States [0].AliveCells.Remove (cell);
			}
		}
		

		/// <summary>
		/// 
		///  HANDLER FOR THE ACCEPT BUTTON WHICH STARTS THE CYCLES ON THE 
		///  PREVIOUSLY DRAWN OR LOADED SHAPE
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		///
		public void btAccept_Clicked(object sender, EventArgs e)
		{
			//if (!organismo.Initiated && organismo.Cells.Count > 0) { //If the cycles haven´t been started yet
			if (!organismo.Initiated && organismo.States [0].AliveCells.Count > 0) { //If the cycles haven´t been started yet
				// To change the accept button back color and label
				if (!Anglo) {
					btAccept.Label = "Activo";
				} else {
					btAccept.Label = "Active";
				}
				btAccept.ModifyBg (StateType.Normal, new Gdk.Color (10, 240, 100));
				//Remove the handler to draw a new shape
				graphicarea.ButtonPressEvent -= graphicarea_ButtonPressEvent;
				// To add the initial state into the states list
				organismo.AddInitialState ();
				initiated = true;
				// To show the corresponding controls
				pnZoom.Visible = true;
				pnDisplacement.Visible = true;
				lbPopulation.Visible = true;
				lbCycles.Visible = true;
				lbRegresive.Visible = true;
				// Stablish the cycle time according to the spinbutton value
				organismo.Cycle = (int)sbPeriod.Value;
			} else if (organismo.Initiated) { // Activate and desativate the pause
				if (!organismo.Pause) {
					btAccept.ModifyBg (StateType.Normal, new Gdk.Color (240, 20, 0));
					if (!Anglo) {
						btAccept.Label = "En pausa";
					} else {
						btAccept.Label = "Paused";
					}
					organismo.Pause = true;
				} else {
					btAccept.ModifyBg (StateType.Normal, new Gdk.Color (10, 240, 100));
					if (!Anglo) {
						btAccept.Label = "Activo";
					} else {
						btAccept.Label = "Active";
					}
					organismo.Pause = false;
					organismo.PulseTimer ();
				}
			}
		}


		/// <summary>
		/// 
		/// SHOWS THE GRID FOR TO DRAW THE INITIAL SHAPE 
		/// OR SHOWS THE UPDATED SHAPE AT EVERY CYCLE
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		/// 
		private void graphicarea_ExposeEvent (object sender, ExposeEventArgs args)
		{
			//Create the Gtk.Context 
			contexto = Gdk.CairoHelper.Create (graphicarea.GdkWindow);

			//To move the coordinates origin point to the center of the DrawingArea
			int w = 1;
			int h = 1;
			//win.GetSize (out w, out h);
			graphicarea.GdkWindow.GetSize (out w, out h);


			if (w % 2 != 0)
				w -= 1;
			if (h % 2 != 0)
				h -= 1;
			
			contexto.Translate ((w / 2), (h / 2));

			if (!initiated) { //Draw the grid if the cycles haven't been iniciated yet
				{
					DrawGrid (w - 8, h-3);
				}
			} else if (initiated) { //If the cycles have been started already
				bool emptystep = false; // To regist when there is not a new available shape state 
				//Reset the graphic area
				contexto.SetSourceColor (backgroundC);
				contexto.Paint ();
				//Set the visualization scale and the graphics origin point
				contexto.Scale (scale, scale);
				contexto.Translate (-w / 2 + displx, -h / 2 + desply);
				//To get the new shape state to be draw 
				OrgaSnismo.state tobeconsumed = default(OrgaSnismo.state);
				// Only if there is a new shape state available
				if (organismo.IndexConsumer < organismo.IndexProducer - 1) {
					tobeconsumed = organismo.States [organismo.IndexConsumer];
					//Update the new state obtaining time
					organismo.Tcalculating = DateTime.Now;
					//To change the regresive counter label color to blue
					lbRegresive.ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
					//Set the cycle mode to manual or auto function of the calculation time
					SetCycleMode (tobeconsumed);
				} else { // If there is no new shape state available yet
					emptystep = true;
					tobeconsumed = organismo.States [organismo.IndexConsumer];
					TimeSpan elapsed1 = DateTime.Now - organismo.Tcalculating;
					int time1 = (int)elapsed1.TotalMilliseconds;
					int timeleft = (int)Math.Ceiling ((double)(time1) / 1000);
					if (!Anglo) {
						lbRegresive.Text = "Calculando: " + timeleft.ToString () + " s ( " + (timeleft / 60).ToString () + " m )";
					} else {
						lbRegresive.Text = "Calculating: " + timeleft.ToString () + " s ( " + (timeleft / 60).ToString () + " m )";
					}
					lbRegresive.ModifyFg (StateType.Normal, new Gdk.Color (250, 0, 0));
				}
				//To update the cycles and population labels
				UpdateLabels (tobeconsumed);
				//Draw the current shape ( new if available or current if not )
				DrawShape (tobeconsumed);

				//To get the final calculation time
				tfinal = DateTime.Now;
				TimeSpan elapsed = tfinal - organismo.Tinitial;
				int lapse = (int)elapsed.TotalMilliseconds;

				//To show the new shape calculation left time if there´s a new available shape
				if (!emptystep) {
					ShowNextCycleTime (lapse);
					//To check if the shape has stabilized and notify and stop the cycles if so
					CheckForStop (tobeconsumed);
				}

				// If the cycle time has passed by
				if (lapse >= organismo.Cycle) {
					organismo.Tinitial = DateTime.Now;
					// To increment the game cycles and the sesion cycles 
					if (!emptystep) {
						// To increment the consumer index
						organismo.IndexConsumer++;
						cycles++;
						//If the automatic image saving is active
						if (autosaving) {
							SaveImage_Onactivated (new object (), new EventArgs ());
						}
					}
				}

				//To force a graphics redrawing
				if (!organismo.Finalized && !organismo.Pause)
					organismo.PulseTimer ();
			}
		}

		/// <summary>
		/// 
		/// DRAW THE GRID TO DRAW THE INITIAL SHAPE FROM THE COORDINARES ORIGIN POINT PASSED AS
		/// ARGUMENT
		/// 
		/// </summary>
		private void DrawGrid(int w, int h)
		{
			// Draw the grid lines
			contexto.SetSourceColor (new Cairo.Color (0, 0, 0));
			contexto.Paint ();
			contexto.LineWidth = 1;
			contexto.Scale (scale, scale);
			contexto.SetSourceColor (new Cairo.Color (1, 0, 0));
			contexto.MoveTo (0, 0);
			for (double x = -w; x < w; x += CellSize) {	//Vertical lines
				if (x % 2 == 0) {
					contexto.MoveTo (x, -h);
					contexto.LineTo (x, h);
				} else {
					contexto.MoveTo (x, h);
					contexto.LineTo (x, -h);
				}
			}
			for (double y = -h; y < h; y += cellsize) { //Horizontal lines
				if (y % 2 == 0) {
					contexto.MoveTo (-w, y);
					contexto.LineTo (w, y);
				} else {
					contexto.MoveTo (w, y);
					contexto.LineTo (-w, y);
				}
			}
			contexto.Stroke ();

			//Redraw the previous drawn shape
			if (organismo.States [0].AliveCells.Count > 0) {
				contexto.Translate (-w, -h);
				foreach (PointD p in organismo.States[0].AliveCells) {
					contexto.SetSourceColor (new Cairo.Color (0, 0.4D, 1));
					contexto.MoveTo (p);
					contexto.LineTo (new PointD (p.X + cellsize, p.Y));
					contexto.LineTo (new PointD (p.X + cellsize, p.Y + cellsize));
					contexto.LineTo (new PointD (p.X, p.Y + cellsize));
					contexto.LineTo (p.X, p.Y);
					contexto.Fill ();
				}
			}
		}

		/// <summary>
		/// 
		///  DRAW THE SHAPE STATE PASSED AS ARGUMENT
		/// 
		/// </summary>
		/// <param name="tobeconsumed">Tobeconsumed.</param>
		/// 
		public void DrawShape( OrgaSnismo.state tobeconsumed)
		{
			//To draw the alive cells
			foreach (PointD p in tobeconsumed.AliveCells) {
				contexto.SetSourceColor (interior);
				contexto.MoveTo (p);
				contexto.LineTo (new PointD (p.X + cellsize, p.Y));
				contexto.LineTo (new PointD (p.X + cellsize, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y));
				contexto.Fill ();
				contexto.SetSourceColor (exterior);
				contexto.LineWidth = 1.5;
				contexto.MoveTo (p);
				contexto.LineTo (new PointD (p.X + cellsize, p.Y));
				contexto.LineTo (new PointD (p.X + cellsize, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y + cellsize));
				contexto.LineTo (p.X, p.Y);
				contexto.Stroke ();
			}
			//To draw the dead cells
			foreach (PointD p in tobeconsumed.NextDead) {
				contexto.SetSourceColor (interiorD);
				contexto.MoveTo (p);
				contexto.LineTo (new PointD (p.X + cellsize, p.Y));
				contexto.LineTo (new PointD (p.X + cellsize, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y));
				contexto.Fill ();
			}
			//To draw the born cells
			foreach (PointD p in tobeconsumed.NextAlives) {
				contexto.SetSourceColor (interiorA);
				contexto.MoveTo (p);
				contexto.LineTo (new PointD (p.X + cellsize, p.Y));
				contexto.LineTo (new PointD (p.X + cellsize, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y));
				contexto.Fill ();
				contexto.SetSourceColor (exterior);
				contexto.MoveTo (p);
				contexto.LineTo (new PointD (p.X + cellsize, p.Y));
				contexto.LineTo (new PointD (p.X + cellsize, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y + cellsize));
				contexto.LineTo (new PointD (p.X, p.Y));
				contexto.Stroke ();
			}
		}

		/// <summary>
		/// 
		/// SET THE CICLE MODE TO MANUAL OR AUTO FUNCION OF THE CALCULATION TIME
		/// </summary>
		/// <param name="tobeconsumed">Tobeconsumed.</param>
		/// 
		private void SetCycleMode(OrgaSnismo.state tobeconsumed)
		{
			//If the calculation time is bigger than the stablished cycle time. change the cycle time mode to automatic
			if (tobeconsumed.Calculationtime > (int)sbPeriod.Value) {
				cbManual.Active = false;
				if (!Anglo) {
					lbAutoCycle.Text = "Periodo en milisegundos   auto";
				} else {
					lbAutoCycle.Text = "Period in milliseconds   auto";
				}
				sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (100, 100, 100));
				sbPeriod.Value = (int)((double)tobeconsumed.Calculationtime);
				organismo.Cycle = (int)sbPeriod.Value;
				autocycle = true;
			} else if (tobeconsumed.Calculationtime <= (int)sbPeriod.Value) { // If the cycle time is bigger than the calculation time, change the cycle time mode to manual if the option is active
				if (cbManual.Active) {// If the manual cycle option is active
					if (!Anglo) {
						lbAutoCycle.Text = "Periodo en milisegundos   manual";
					} else {
						lbAutoCycle.Text = "Period in milliseconds   manual";
					}
					organismo.Cycle = (int)sbPeriod.Value;
					sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (0, 240, 0));
				} else {// If the manual cycle option is not active
					if (!Anglo) {
						lbAutoCycle.Text = "Periodo en milisegundos  auto";
					} else {
						lbAutoCycle.Text = "Period in milliseconds   auto";
					}
					organismo.Cycle = (int)(((double)tobeconsumed.Calculationtime));
					sbPeriod.Value = (double)tobeconsumed.Calculationtime;
					sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (100, 100, 100));
				}
			}
		}

		/// <summary>
		/// /
		/// UPDATE THE CYCLES AND POPULATION LABELS
		/// 
		/// </summary>
		/// <param name="tobeconsumed">Tobeconsumed.</param>
		/// 
		private void UpdateLabels(OrgaSnismo.state tobeconsumed)
		{
			if (!Anglo) {
				lbCycles.Text = "cycles: " + cycles.ToString ();
				lbPopulation.Text = "Poblaciòn: " + tobeconsumed.AliveCells.Count.ToString ();
			} else {
				lbCycles.Text = "Cycles: " + cycles.ToString ();
				lbPopulation.Text = "Population: " + tobeconsumed.AliveCells.Count.ToString ();
			}
		}

		/// <summary>
		/// 
		/// SHOW THE TIME LEFT TO THE NEXT CYCLE 
		/// 
		/// </summary>
		/// <param name="lapse">Lapse.</param>
		/// 
		private void ShowNextCycleTime(double lapse)
		{
			if (organismo.Cycle < 1000)
				lbRegresive.Visible = false;
			else if ((organismo.Cycle >= 1000) && (organismo.Cycle < 60000)) {
				lbRegresive.Visible = true;
				int seconds = (int)Math.Ceiling ((double)(organismo.Cycle - lapse) / 1000);
				if (!Anglo) {
					lbRegresive.Text = "Proximo ciclo: (segundos): " + seconds.ToString ();
				} else {
					lbRegresive.Text = "Next cycle: (seconds): " + seconds.ToString ();
				}
			} else {
				lbRegresive.Visible = true;
				double minutes = Math.Round ((((double)organismo.Cycle - (double)lapse) / 60000D), 2);
				if (!Anglo) {
					lbRegresive.Text = "Proximo ciclo: (minutos): " + minutes.ToString ();
				} else {
					lbRegresive.Text = "Next cycle: (minutes): " + minutes.ToString ();
				}
			}
		}
	
		/// <summary>
		/// 
		/// CHECK IF THE SHAPE HAS STABILIZED OR DISAPEARED AND STOP THE CYCLES AND NOTIFY IF SO
		/// 
		/// </summary>
		/// <param name="tobeconsumed">Tobeconsumed.</param>
		/// 
		private void CheckForStop(OrgaSnismo.state tobeconsumed)
		{
			if (organismo.IndexConsumer > 2 && organismo.States [organismo.IndexConsumer - 1].AliveCells.Count == organismo.States [organismo.IndexConsumer + 1].AliveCells.Count &&
			    organismo.States [organismo.IndexConsumer - 1].NextAlives.Count == organismo.States [organismo.IndexConsumer + 1].NextAlives.Count &&
			    organismo.States [organismo.IndexConsumer - 1].NextDead.Count == organismo.States [organismo.IndexConsumer + 1].NextDead.Count) {

				if (!Anglo) {
					MessageDialog mensaje2 = new MessageDialog (win, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organismo estabilizado. ", new object [0]);
					mensaje2.Show ();
				} else {
					MessageDialog mensaje2 = new MessageDialog (win, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organism stabilized. ", new object [0]);
					mensaje2.Show ();
				}
				organismo.Finalized = true;
			}
			// To check if there are no alive cells and stop the cycles and notify it if so
			if (tobeconsumed.AliveCells.Count == 0) {
				if (!Anglo) {
					MessageDialog mensaje2 = new MessageDialog (win, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organismo muerto. ", new object [0]);
					mensaje2.Show ();
				} else {
					MessageDialog mensaje2 = new MessageDialog (win, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organism died. ", new object [0]);
					mensaje2.Show ();
				}
				organismo.Finalized = true;
			}
		}

		/// <summary>
		/// 
		/// MANEJADOR PARA INICIAR UNA NUEVA PARTIDA
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void New_Onactivated (object sender,EventArgs e)
		{
			//Stop the cycles
			if (!organismo.Pause) {
				organismo.Pause = true;
			}
			// Stop the producer thread
			organismo.Producer.Abort ();
			Thread.Sleep (200);
			// To hide the correponding controls
			pnZoom.Visible = false;
			pnDisplacement.Visible = false;
			btAccept.Visible = false;
			sbPeriod.Visible = false;
			graphicarea.Visible = false;
			vbMetadata.Visible = false;
			vbCrono.Visible = false;
			//To reset the producer and consumer indexes
			organismo.IndexProducer = 0;
			organismo.IndexConsumer = 0;
			//To reset the alive cell list and the shape state array
			organismo.States.Initialize ();
			// To show the new shape button
			alrb.Visible = true;
			// Enable the new shape drawing handler
			graphicarea.ButtonPressEvent += graphicarea_ButtonPressEvent;
			// Create  the producer thread again
			organismo.Producer = new Thread (organismo.OrganismoUpdate);
			//Reset the game status markers
			organismo.Initiated = false;
			organismo.Finalized = false;
			organismo.Pause = false;
			organismo.Loaded = false;
			autocycle = false;
			//Arrange the accept button and the cycles spinbutton
			if (!Anglo) {
				btAccept.Label = "Aceptar";
			} else {
				btAccept.Label = "Accept";
			}
			btAccept.ModifyBg (StateType.Normal, new Gdk.Color (0, 240, 30));
			if (!Anglo) {
				lbAutoCycle.Text = "Periodo en milisegundos   manual";
			} else {
				lbAutoCycle.Text = "Period in milliseconds   manual";
			}
			sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (0, 240, 0));
			hbContinue.HeightRequest -= 10;
			//Reset the zoom and the origin point
			scale = 1;
			displx = 0;
			desply = 0;
			// Reset the cycle time and the cycles count
			organismo.Cycle = 100;
			cycles = 0;
			sbPeriod.Value = organismo.Cycle;
		}

		/// <summary>
		/// 
		/// SAVE A SHAPE TO DISC
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void Save_Onactivated (object sender, EventArgs e)
		{
			//To show the file save dialog
			if (!Anglo) {
				savedialog = new FileChooserDialog ("Salvar shape a disco", win, FileChooserAction.Save, "Cancelar", ResponseType.Cancel, "Aceptar", ResponseType.Accept);
			} else {
				savedialog = new FileChooserDialog ("Save shape to disc", win, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Accept", ResponseType.Accept);
			}
			savedialog.Visible = true;
			FileFilter filter = new FileFilter ();
			filter.AddPattern ("*.fm");
			savedialog.Filter = filter;
			if (savedialog.Run () == (int)ResponseType.Accept) {
				savingpath = savedialog.Filename;
				btAcceptfichero_Clicked ();
			} else
				savedialog.Hide ();
		}

		// AUXILIARY METHOD OF THE  PREVIOUS ONE
		private void btAcceptfichero_Clicked ()
		{
			if (!savingpath.EndsWith (".fm")) {
				savingpath += ".fm";
			}
			FileStream fs = new FileStream (savingpath, FileMode.Create, FileAccess.Write);
			BinaryWriter bw = new BinaryWriter (fs);
			List < PointD > amsave = new List < PointD> ();
			if (!organismo.Initiated) {
				//amsave = organismo.Cells;
				amsave = organismo.States [0].AliveCells;
			} else {
				amsave = organismo.States [organismo.IndexConsumer].AliveCells;
			}
			try {
				// To save the calculation time
				bw.Write (organismo.Calculationtime);
				//To save the visualization scale
				bw.Write (scale);
				//To save the cycle time
				bw.Write (sbPeriod.Text);
				//To save the cycles
				bw.Write (cycles);
				//To save the rules
				string ndigits = "";
				foreach (int i in organismo.Born) {
					ndigits += i.ToString ();
				}
				bw.Write (Int32.Parse (ndigits));
				ndigits = "";
				foreach (int i in organismo.Alive) {
					ndigits += i.ToString ();
				}
				bw.Write (Int32.Parse (ndigits));
				// To save the colors
				bw.Write (interior.R);
				bw.Write (interior.G);
				bw.Write (interior.B);
				bw.Write (exterior.R);
				bw.Write (exterior.G);
				bw.Write (exterior.B);
				bw.Write (backgroundC.R);
				bw.Write (backgroundC.G);
				bw.Write (backgroundC.B);
				bw.Write (interiorD.R);
				bw.Write (interiorD.G);
				bw.Write (interiorD.B);
				bw.Write (interiorA.R);
				bw.Write (interiorA.G);
				bw.Write (interiorA.B);
				//Save the PointsD
				foreach (PointD p in amsave) {
					double x = p.X;
					double y = p.Y;
					bw.Write (x);
					bw.Write (y);
				}
				//Close the stream
				bw.Close ();
				fs.Close ();
			} catch (IOException error) {
				MessageDialog mensaje3 = new MessageDialog (win, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Disc writing error.  \n" + error.Message, new object [0]);
				mensaje3.Show ();
			} finally {
				bw.Close ();
				fs.Close (); 
			}
			savedialog.Hide ();
		}

		/// <summary>
		/// 
		/// LOADS A SHAPE PREVIOUSLY SAVED
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void mLoad_Onactivated (object sender, EventArgs e)
		{
			New_Onactivated (new object (), new EventArgs ());
			Gtk.FileChooserDialog dialogomload;
			if (!Anglo) {
				dialogomload = new FileChooserDialog ("mload shape guardada", win, FileChooserAction.Open, "mload", ResponseType.Accept, "Cancelar", ResponseType.Cancel);
			} else {
				dialogomload = new FileChooserDialog ("Load saved shape", win, FileChooserAction.Open, "Load", ResponseType.Accept, "Cancel", ResponseType.Cancel);
			}
			dialogomload.Visible = true;
			FileFilter filter = new FileFilter ();
			dialogomload.AddFilter (filter);
			filter.AddPattern ("*.fm");
			if (dialogomload.Run () == (int)ResponseType.Accept) {
				string shape = dialogomload.Filename;
				FileStream fs = new FileStream (shape, FileMode.Open, FileAccess.Read);
				BinaryReader br = new BinaryReader (fs);
				try {
					//Load the calculation time
					organismo.Calculationtime = br.ReadInt32 ();
					//Load the visualization scale
					scale = br.ReadDouble ();
					//Load the cycle time
					organismo.Cycle = Int32.Parse (br.ReadString ());
					sbPeriod.Text = organismo.Cycle.ToString ();
					organismo.Initiated = false;
					//Load the cycles
					cycles = br.ReadInt32 ();
					//Load the rules
					organismo.Born.Clear ();
					organismo.Alive.Clear ();
					string toborn = (br.ReadInt32 ()).ToString ();
					foreach (char c in toborn) {
						organismo.Born.Add (Int32.Parse (c.ToString ()));
					}
					string continuealive = (br.ReadInt32 ()).ToString ();
					foreach (char c in continuealive) {
						organismo.Alive.Add (Int32.Parse (c.ToString ()));
					}
					// To show the rules
					win.Title = "OrgaSnimo0            Twister edition              ";
					foreach (int i in organismo.Alive) {
						win.Title += i.ToString ();
					}
					win.Title += " / ";
					foreach (int i in organismo.Born) {
						win.Title += i.ToString ();
					}
					// Load the colors
					double ri = br.ReadDouble ();
					double gi = br.ReadDouble ();
					double bi = br.ReadDouble ();
					interior = new Cairo.Color (ri, gi, bi);
					double re = br.ReadDouble ();
					double ge = br.ReadDouble ();
					double be = br.ReadDouble ();
					exterior = new Cairo.Color (re, ge, be);
					double fr = br.ReadDouble ();
					double fg = br.ReadDouble ();
					double fb = br.ReadDouble ();
					backgroundC = new Cairo.Color (fr, fg, fb);
					double imr = br.ReadDouble ();
					double img = br.ReadDouble ();
					double imb = br.ReadDouble ();
					interiorD = new Cairo.Color (imr, img, imb);
					double ivr = br.ReadDouble ();
					double ivg = br.ReadDouble ();
					double ivb = br.ReadDouble ();
					interiorA = new Cairo.Color (ivr, ivg, ivb);
					while (true) {
						//Load the points
						double x = br.ReadDouble ();
						double y = br.ReadDouble ();
						organismo.States [0].AliveCells.Add (new PointD (x, y));
					}
				} catch (EndOfStreamException) {
					// Close the stream
					br.Close ();
					fs.Close ();
				} finally {
					fs.Close ();
					br.Close ();
				}
				dialogomload.Hide ();
				btAccept.Visible = true;
				organismo.Loaded = true;
				DrawInitialShape (new object (), new EventArgs ());
			} else
				dialogomload.Hide ();
		}

		/// <summary>
		/// 
		/// SAVES A PNG IMAGE INTO THE DIRECTORY WHERE IS THE PROGRAM  EXECUTABLE
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void SaveImage_Onactivated (object sender, EventArgs e)
		{
			Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable ((Drawable)graphicarea.GdkWindow, graphicarea.Colormap, 0, 0, 0, 0, 1300, 552);
			DateTime nowadays = DateTime.Now;
			string directorio = AppDomain.CurrentDomain.BaseDirectory;
			string fileto = "";
			if (!Anglo) {
				fileto = directorio + "/Imagenes/imagorg" + lbPopulation.Text + " cycles: " + cycles.ToString () + ".png";
			} else {
				fileto = directorio + "/Imagenes/imagorg" + lbPopulation.Text + " Cycles: " + cycles.ToString () + ".png";	
			}
			if (!File.Exists (fileto)) {
				pixbuf.Save (fileto, "png");
			}
		}

		/////////////////////////////////////////////
		///
		/// CHANGE THE PROGRAM LANGUAGE TO SPANISH
		///
		//////////////////////////////////////////////////////////////////////////////
		///
		public void mSpanish_Activated (object sender, EventArgs e)
		{
			menu.Remove (mfile);
			mfile.Dispose ();
			mfile = new MenuItem ("Archivo");
			mfile.Visible = true;
			menu.Append (mfile);

			filemenu = new Menu ();
			mfile.Submenu = filemenu;

			mnew.Dispose ();
			filemenu.Remove (mnew);
			mnew = new MenuItem ("Nuevo");
			mnew.Visible = true;
			mnew.Activated += New_Onactivated;
			filemenu.Append (mnew);

			msave.Dispose ();
			msave = new MenuItem ("Guardar");
			msave.Visible = true;
			msave.Activated += Save_Onactivated;
			filemenu.Append (msave);

			mload.Dispose ();
			mload = new MenuItem ("Cargar");
			mload.Visible = true;
			mload.Activated += Save_Onactivated;
			filemenu.Append (mload);

			msaveimage.Dispose ();
			msaveimage = new MenuItem ("Guardar imagen");
			msaveimage.Visible = true;
			msaveimage.Activated += SaveImage_Onactivated;
			filemenu.Append (msaveimage);

			menu.Remove (moptions);
			moptions.Dispose ();
			moptions = new MenuItem ("Opciones");
			moptions.Visible = true;
			menu.Append (moptions);

			menuoptions = new Menu ();
			moptions.Submenu = menuoptions;

			mrules.Dispose ();
			mrules = new MenuItem ("Cambiar reglas");
			mrules.Visible = true;
			mrules.Activated += mrules_Activated;
			menuoptions.Append (mrules);

			mbacktodefect.Dispose ();
			mbacktodefect = new MenuItem ("Volver a valores por defecto");
			mbacktodefect.Visible = true;
			mbacktodefect.Activated += mbacktodefect_Activated;
			menuoptions.Append (mbacktodefect);

			mchangecolors.Dispose ();
			mchangecolors = new MenuItem ("Cambiar colores");
			mchangecolors.Visible = true;
			mchangecolors.Activated += mchangecolors_Activated;
			menuoptions.Append (mchangecolors);

			mimageautosave.Dispose ();
			mimageautosave = new MenuItem ("Guardado automático de imágenes");
			mimageautosave.Visible = true;
			mimageautosave.Activated += mimageautosave_Activated;
			menuoptions.Append (mimageautosave);

			mlanguage.Dispose ();
			mlanguage = new MenuItem ("Idioma");
			mlanguage.Visible = true;
			menuoptions.Append (mlanguage);

			Menu mlanguages = new Menu ();
			mlanguage.Submenu = mlanguages;

			mSpanish.Dispose ();
			mSpanish = new MenuItem ("Castellano");
			mSpanish.Visible = true;
			mSpanish.Activated += mSpanish_Activated;
			mlanguages.Append (mSpanish);

			mEnglish.Dispose ();
			mEnglish = new MenuItem ("Inglés");
			mEnglish.Visible = true;
			mEnglish.Activated += mEnglish_Activated;
			mlanguages.Append (mEnglish);

			menu.Remove (menuhelp);
			menuhelp.Dispose ();
			menuhelp = new MenuItem ("Ayuda");
			menuhelp.Activated += menuhelp_Activated;
			menuhelp.Visible = true;
			menu.Append (menuhelp);

			Anglo = false;

			btDrawShape.Label = "Dibujar forma inicial";
			lbAutoCycle.Text = "Periodo en milisegundos    manual";
			cbManual.Label = "ciclo manual";
			cbManual.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
			cbManual.Children [0].ModifyFg (StateType.Active, new Gdk.Color (0, 250, 0));
			lbRegresive.Text = "Próximo ciclo: ";

			if (!organismo.Initiated) {
				btAccept.Label = "Aceptar";
			} else {
				btAccept.Label = "Pausa";
			}
			lbPopulation.Text = "Población: ";
			lbCycles.Text = "Ciclos: ";
		}

		//////////////////////////////////////////////////
		///
		/// CHANGES THE  PROGRAM LANGUAGE TO ENGLISH
		///
		//////////////////////////////////////////////////////////////////////////
		///
		public void mEnglish_Activated(object sender,EventArgs e)
		{
			menu.Remove (mfile);
			mfile.Dispose ();
			mfile = new MenuItem ("Archive");
			mfile.Visible = true;
			menu.Append (mfile);

			filemenu = new Menu ();
			mfile.Submenu = filemenu;

			mnew.Dispose ();
			filemenu.Remove (mnew);
			mnew = new MenuItem ("New");
			mnew.Visible = true;
			mnew.Activated += New_Onactivated;
			filemenu.Append (mnew);

			msave.Dispose ();
			msave = new MenuItem ("Save");
			msave.Visible = true;
			msave.Activated += Save_Onactivated;
			filemenu.Append (msave);

			mload.Dispose ();
			mload = new MenuItem ("Load");
			mload.Visible = true;
			mload.Activated += Save_Onactivated;
			filemenu.Append (mload);

			msaveimage.Dispose ();
			msaveimage = new MenuItem ("Save image");
			msaveimage.Visible = true;
			msaveimage.Activated += SaveImage_Onactivated;
			filemenu.Append (msaveimage);

			menu.Remove (moptions);
			moptions.Dispose ();
			moptions = new MenuItem ("Options");
			moptions.Visible = true;
			menu.Append (moptions);

			menuoptions = new Menu ();
			moptions.Submenu = menuoptions;

			mrules.Dispose ();
			mrules = new MenuItem ("Change rules");
			mrules.Visible = true;
			mrules.Activated += mrules_Activated;
			menuoptions.Append (mrules);

			mchangecolors.Dispose ();
			mchangecolors = new MenuItem ("Change colors");
			mchangecolors.Visible = true;
			mchangecolors.Activated += mchangecolors_Activated;
			menuoptions.Append (mchangecolors);


			mbacktodefect.Dispose ();
			mbacktodefect = new MenuItem ("Back to values by defect");
			mbacktodefect.Visible = true;
			mbacktodefect.Activated += mbacktodefect_Activated;
			menuoptions.Append (mbacktodefect);

			mimageautosave.Dispose ();
			mimageautosave = new MenuItem ("Autosave images");
			mimageautosave.Visible = true;
			mimageautosave.Activated += mimageautosave_Activated;
			menuoptions.Append (mimageautosave);

			mlanguage.Dispose ();
			mlanguage = new MenuItem ("Language");
			mlanguage.Visible = true;
			menuoptions.Append (mlanguage);

			Menu mlanguages = new Menu ();
			mlanguage.Submenu = mlanguages;

			mSpanish.Dispose ();
			mSpanish = new MenuItem ("Spanish");
			mSpanish.Visible = true;
			mSpanish.Activated += mSpanish_Activated;
			mlanguages.Append (mSpanish);

			mEnglish.Dispose ();
			mEnglish = new MenuItem ("mEnglish");
			mEnglish.Visible = true;
			mEnglish.Activated += mEnglish_Activated;
			mlanguages.Append (mEnglish);

			menu.Remove (menuhelp);
			menuhelp.Dispose ();
			menuhelp = new MenuItem ("Help");
			menuhelp.Activated += menuhelp_Activated;
			menuhelp.Visible = true;
			menu.Append (menuhelp);

			Anglo = true;

			btDrawShape.Label = "Draw initial shape";
			lbAutoCycle.Text = "Period in milliseconds   manual";
			cbManual.Label = "Manual cycle";
			cbManual.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
			cbManual.Children [0].ModifyFg (StateType.Active, new Gdk.Color (0, 250, 0));
			lbRegresive.Text = "Next cycle: ";

			if (!organismo.Initiated) {
				btAccept.Label = "Accept";
			} else {
				btAccept.Label = "Pause";
			}

			lbPopulation.Text = "Population: ";
			lbCycles.Text = "Cycles: ";
		}

		/// <summary>
		/// 
		/// SHOWS THE DIALOG TO CHANGE THE GAME RULES
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void mrules_Activated (object sender, EventArgs e)
		{
			Gtk.Window dialogo;
			if (!Anglo)
				dialogo = new Gtk.Window ("Cambiar reglas    ( por defecto 23/3 )");
			else
				dialogo = new Gtk.Window ("Change rules    ( by defect 23/3 )");
			dialogo.SetSizeRequest (700, 385);
			dialogo.Visible = true;
			dialogo.ModifyBg (StateType.Normal, new Gdk.Color (10, 200, 50));

			VBox vbRules = new VBox (false, 0);
			vbRules.Visible = true;

			HBox hbalives = new HBox (false, 0);
			hbalives.Visible = true;

			Gtk.Alignment allbalives = new Gtk.Alignment (0, 0.5F, 0, 0);
			allbalives.Visible = true;
			Label lbalives;
			if (!Anglo)
				lbalives = new Label ("Cantidad de cells adyacentes para que una célula permanezca alive: \n( Enteros positivos entre 1 y 8 sin espacios )");
			else
				lbalives = new Label ("Number of adjacent cells for a cell to remain alive: \n( Positive integers between 1 & 8 without spaces )");
			lbalives.Visible = true;
			lbalives.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans OBLIQUE 11"));
			allbalives.Add (lbalives);
			hbalives.Add (allbalives);

			Gtk.Alignment altbalives = new Gtk.Alignment (0, 0.5F, 0, 0);
			altbalives.Visible = true;
			Entry tbalives = new Entry ();
			tbalives.Visible = true;
			tbalives.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans NORMAL 13"));
			tbalives.SetSizeRequest (100, 35);
			tbalives.KeyPressEvent += RulesEntry_KeyPress;
			tbalives.FocusOutEvent += Tbalives_FocusOutEvent;
			tbalives.Activated += Tbalives_Activated;
			foreach (int i in organismo.Alive)
				tbalives.Text += i.ToString ();
			altbalives.Add (tbalives);
			hbalives.Add (altbalives);

			HBox hbborns = new HBox ();
			hbborns.Visible = true;

			Gtk.Alignment allbborns = new Gtk.Alignment (0, 0.5F, 0, 0);
			allbborns.Visible = true;
			Label lbborns;
			if (!Anglo)
				lbborns = new Label ("Cantidad de células adyacentes para que una célula nazca: \n( Enteros positivos entre 0 y 8 sin espacios )");
			else
				lbborns = new Label ("Number of adjacent cells for a cell to born: \n( Positive integers between 0 & 8 without spaces )");
			lbborns.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans OBLIQUE 11"));
			lbborns.Visible = true;
			allbborns.WidthRequest = allbalives.WidthRequest;
			allbborns.Add (lbborns);
			hbborns.Add (allbborns);

			Gtk.Alignment altbborns = new Gtk.Alignment (0, 0.5F, 0, 0);
			altbborns.Visible = true;
			altbborns.LeftPadding = 80;
			Entry tbborns = new Entry ();
			tbborns.Visible = true;
			tbborns.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans NORMAL 13"));
			tbborns.WidthRequest = tbalives.WidthRequest;
			foreach (int i in organismo.Born)
				tbborns.Text += i.ToString ();
			tbborns.KeyPressEvent += RulesEntry_KeyPress;
			tbborns.FocusOutEvent += Tbborns_FocusOutEvent;
			tbborns.Activated += Tbborns_Activated;
			altbborns.Add (tbborns);
			hbborns.Add (altbborns);

			Gtk.Alignment allbExamples = new Gtk.Alignment (0, 1F, 0, 0);
			allbExamples.Visible = true;
			Label lbExamples;
			if (!Anglo) {
				lbExamples = new Label ("Ejemplos:\n\n01234567 / 3  ---> Crecimiento moderado\n" +
				"23 / 36 ------------> Hight life\n" +
				"1357 / 1357 -----> Replicantes, crecimiento rápido\n" +
				"235678 / 3678 --> Rombos, crecimiento rápido\n" +
				"34 / 34 ------------> Estable\n" +
				"4 / 2 ---------------> Crecimiento moderado\n" +
				"51 / 346 ----------> Vida media\n");
			} else {
				lbExamples = new Label ("Examples:\n\n01234567 / 3  ---> Moderate growth\n" +
				"23 / 36 ------------> Hight life\n" +
				"1357 / 1357 -----> Replicants, quick growth\n" +
				"235678 / 3678 --> Diamonds, quick growth\n" +
				"34 / 34 ------------> Stable\n" +
				"4 / 2 ---------------> Moderate growth\n" +
				"51 / 346 ----------> Average life time\n");
			}
			lbExamples.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans OBLIQUE BOLD 17"));
			lbExamples.Visible = true;
			allbExamples.Add (lbExamples);

			vbRules.PackEnd (allbExamples);
			vbRules.Add (hbalives);
			vbRules.Add (hbborns);
			dialogo.Add (vbRules);
		}
			
		//AUXILIARY METHODS TO THE BEFORE ONE
		void Tbborns_Activated (object sender, EventArgs e)
		{
			organismo.Born.Clear ();
			Entry boxfrom = (Entry)sender;
			foreach (char c in boxfrom.Text)
				organismo.Born.Add (Int32.Parse (c.ToString ()));
			win.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in organismo.Alive)
				win.Title += i.ToString ();
			win.Title += " / ";
			foreach (int i in organismo.Born)
				win.Title += i.ToString ();
		}

		void Tbborns_FocusOutEvent (object o, FocusOutEventArgs args)
		{
			organismo.Born.Clear ();
			Entry boxfrom = (Entry)o;
			foreach (char c in boxfrom.Text)
				organismo.Born.Add (Int32.Parse (c.ToString ()));

			win.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in organismo.Alive)
				win.Title += i.ToString ();
			win.Title += " / ";
			foreach (int i in organismo.Born)
				win.Title += i.ToString ();
		}

		void Tbalives_Activated (object sender, EventArgs e)
		{
			organismo.Alive.Clear ();
			Entry boxfrom = (Entry)sender;
			foreach (char c in boxfrom.Text)
				organismo.Alive.Add (Int32.Parse (c.ToString ()));

			win.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in organismo.Alive)
				win.Title += i.ToString ();
			win.Title += " / ";
			foreach (int i in organismo.Born)
				win.Title += i.ToString ();
		}

		void Tbalives_FocusOutEvent (object o, FocusOutEventArgs args)
		{
			organismo.Alive.Clear ();
			Entry boxfrom = (Entry)o;
			foreach (char c in boxfrom.Text)
				organismo.Alive.Add (Int32.Parse (c.ToString ()));

			win.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in organismo.Alive)
				win.Title += i.ToString ();
			win.Title += " / ";
			foreach (int i in organismo.Born)
				win.Title += i.ToString ();
		}

		/// <summary>
		/// 
		///  COME BACK TO THE BY DEFECT VALUES
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void mbacktodefect_Activated (object sender, EventArgs e)
		{
			organismo.Alive.Clear ();
			organismo.Alive.Add (2);
			organismo.Alive.Add (3);
			organismo.Born.Clear ();
			organismo.Born.Add (3);

			exterior = new Cairo.Color (1, 0, 0); //Color de la linea alrrededor de la cell alive por defecto
			interior = new Cairo.Color (0.65D, 1, 0.1D); // Color interior de la cell alive por defecto
			backgroundC = new Cairo.Color (0, 0, 0); // Color negro del backgroundC por defecto
			interiorD = new Cairo.Color (0, 0, 0); // Color de las cells muertas en el turno por defecto.
			interiorA = new Cairo.Color (0.65D, 1, 0.1D); // Color de las nuevas cells en el turno por defecto
			win.Title = "OrgaSnimo0            Twister edition              ";
		}

		/// <summary>
		/// 
		/// SHOWS THE DIALOG TO CHANGE THE COLORS
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void mchangecolors_Activated (object sender, EventArgs e)
		{
			Gtk.Window dialogo;
			if (!Anglo)
				dialogo = new Gtk.Window ("Cambiar colores.");
			else
				dialogo = new Gtk.Window ("Change colors.");
			dialogo.Visible = true;
			dialogo.SetSizeRequest (400, 250);

			VBox vbLabels = new VBox (true, 0);
			vbLabels.Visible = true;

			Gtk.Alignment albtLines = new Gtk.Alignment (0, 0, 0, 0);
			albtLines.Visible = true;
			if (!Anglo)
				btLines = new Button ("Color de la linea.");
			else
				btLines = new Button ("Line color.");
			btLines.Children [0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btLines.ModifyBg (StateType.Normal, CairoColorToGdk (exterior));
			btLines.Visible = true;
			btLines.Clicked += BtLines_Clicked;
			albtLines.Add (btLines);

			Gtk.Alignment albtalivecells = new Gtk.Alignment (0, 0, 0, 0);
			albtalivecells.Visible = true;
			if (!Anglo)
				btalivecells = new Button ("Color de las cells alives");
			else
				btalivecells = new Button ("Alive cells color.");
			btalivecells.Children [0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			Gdk.Color translated = CairoColorToGdk (interior);
			btalivecells.ModifyBg (StateType.Normal, translated);
			btalivecells.Visible = true;
			btalivecells.Clicked += Btalives_Clicked;
			albtalivecells.Add (btalivecells);

			Gtk.Alignment albtBornCells = new Gtk.Alignment (0, 0, 0, 0);
			albtBornCells.Visible = true;
			if (!Anglo)
				btBornCells = new Button ("Color de las nuevas células en el turno");
			else
				btBornCells = new Button ("New cells color");
			btBornCells.Children [0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btBornCells.ModifyBg (StateType.Normal, CairoColorToGdk (interiorA));
			btBornCells.Visible = true;
			btBornCells.Clicked += BtBornCells_Clicked;
			albtBornCells.Add (btBornCells);

			Gtk.Alignment albtDeadCells = new Gtk.Alignment (0, 0, 0, 0);
			albtDeadCells.Visible = true;
			if (!Anglo)
				btDeadCells = new Button ("Color de las células muertas en el turno.");
			else
				btDeadCells = new Button ("Dead cells color.");
			btDeadCells.Children [0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btDeadCells.ModifyBg (StateType.Normal, CairoColorToGdk (interiorD));
			btDeadCells.Visible = true;
			btDeadCells.Clicked += BtDeadCells_Clicked;
			albtDeadCells.Add (btDeadCells);

			Gtk.Alignment albtbackgroundC = new Gtk.Alignment (0, 0, 0, 0);
			albtbackgroundC.Visible = true;
			if (!Anglo)
				btbackgroundC = new Button ("Color del fondo.");
			else
				btbackgroundC = new Button ("Background color.");
			btbackgroundC.Children [0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btbackgroundC.ModifyBg (StateType.Normal, CairoColorToGdk (backgroundC));
			btbackgroundC.Visible = true;
			btbackgroundC.Clicked += BtbackgroundC_Clicked;
			albtbackgroundC.Add (btbackgroundC);

			vbLabels.Add (albtLines);
			vbLabels.Add (albtalivecells);
			vbLabels.Add (albtBornCells);
			vbLabels.Add (albtDeadCells);
			vbLabels.Add (albtbackgroundC);
			dialogo.Add (vbLabels);
		}

		/// <summary>
		/// 
		/// AUTOMATIC IMAGE SAVING HANDLER
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void mimageautosave_Activated ( object sender, EventArgs e)
		{
			if (autosaving) {
				autosaving = false;
				mimageautosave.Deselect ();
			} else {
				autosaving = true;
				mimageautosave.Select ();
			}
		}

		public void menuhelp_Activated (object sender, EventArgs e)
		{
			Gtk.Window winmenuhelp = new Gtk.Window ("");
			winmenuhelp.Parent = win;
			winmenuhelp.SetSizeRequest (1050, 510);
			winmenuhelp.Visible = true;
			winmenuhelp.ShowAll ();

			ScrolledWindow swwinmenuhelp = new ScrolledWindow ();
			swwinmenuhelp.ModifyBase (StateType.Normal, new Gdk.Color (0, 160, 50));
			swwinmenuhelp.SetSizeRequest (1000, 510);
			swwinmenuhelp.Visible = true;
			winmenuhelp.Add (swwinmenuhelp);

			TextView tvTexto = new TextView ();
			tvTexto.Visible = true;
			TextBuffer buffer = new TextBuffer (new TextTagTable ());
			tvTexto.Buffer = buffer;
			tvTexto.ModifyBase (StateType.Normal, new Gdk.Color (0, 160, 50));
			swwinmenuhelp.Add (tvTexto);

			TextTag underlined = new TextTag ("sub");
			underlined.Font = "Dejavu-Sans ITALIC BOLD 17";
			underlined.Underline = Pango.Underline.Single;
			underlined.ForegroundGdk = new Gdk.Color (0, 0, 0);
			buffer.TagTable.Add (underlined);

			TextTag normal = new TextTag ("normal");
			normal.Font = "Dejavu-Sans NORMAL 13";
			buffer.TagTable.Add (normal);

			if (!Anglo) {
				buffer.Text = "Funcionamiento básico:\n\n";
				int sub1 = buffer.Text.Length;

				buffer.Text += "OrgaSnismo0, está basado en el juego de la vida, diseñado por el matématico John Conway. Puede encontrarse\n extensa información en Wikipedia.\n";
				buffer.Text += "\nCuando se inicia el programa, se muestra la barra de menú, cuyas opciones se comentan más adelante, y el botón\n'Dibujar forma incicial'.\nPulsando el botón, aparecen los siguientes elementos:\n\n " +
				"\tRejilla: Al pulsar sobre un cuadrado, se crea una nueva célula, la cual se muestra en color amarillo.\n\t            Para borrar una célula, basta con volver a pulsar sobre ella.\n\n" +
				"\tCheck button 'ciclo manual': Chequeado por defecto, sirve para habilitar el establecimiento del lapse de ciclo\n\t\t\t\t\t\t\t por parte del usuario.\n\t\t\t\t\t\t\t Cuando no está chequeado, tanto el check button como el spin button cambian de\n\t\t\t\t\t\t\t color, y el lapse de ciclo se determina dinámicamente de forma automática en\n\t\t\t\t\t\t\t función del lapse que la CPU necesite para calcular la siguiente forma." +
				"\n\n\tSpin button:  Muestra el lapse de ciclo establecido. Por defecto, está fijado en 100 milisegundos.\n\t\t\t\tEste lapse se puede cambiar si el check button 'ciclo manual' está chequeado.\n\t\t\t\tSi el lapse que la CPU necesita para calcular la siguiente shape, es mayor que el lapse de ciclo\n\t\t\t\testablecido, el programa cambia a ciclo automático." +
				"\n\n\tBotón 'Aceptar': Para comenzar los ciclos una vez que se ha dibujado la forma deseada y ajustado el lapse de \n\t\t\t\t     ciclo en su caso." +
				"\n\nUna vez iniciado el juego, la forma dibujada irá cambiando en cada ciclo en base a las reglas.\nPor defecto, las reglas son las establecidas por Conway, es decir:\nUna célula se mantiene viva si toca con dos células vivas, en otro caso muere en el siguiente ciclo.\nUna célula nace, si toca con tres células vivas." +
				"\n\nEn la esquina inferior izquierda, se muestra la población ( cantidad de células ) y la cantidad de ciclos transcurrida." +
				"\nTambién aparecen los botones: zoom , desplazamiento y pausa.\n\nSi la forma se estabiliza o desaparece, se muestra un cuadro de dialogo y el juego finaliza.";
				int norm = buffer.Text.Length;
				buffer.Text += "\n\nBarra de menú";
				int sub2 = buffer.Text.Length;
				buffer.Text += "\n\nArchivo:" +
				"\n\n\tNuevo: Inicia un nuevo juego." +
				"\n\n\tGuardar: Guarda la forma, reglas y colores actuales en un fichero *.fm.\n\t\t\tSe puede salvar una forma tanto si se han iniciado los ciclos como si no. Si se han iniciado los ciclos, se \n\t\t\trecomienda realizar el proceso de guardado con el juego en pausa." +
				"\n\n\tCargar: Carga una forma, con las reglas y colores guardados anteriormente en un fichero *.fm" +
				"\n\n\tGuardar imagen: Guarda una imagen *.png correspondiente a la forma en pantalla.\n\t\t\t\t      El fichero de imagen, se guarda en un directorio llamado 'imagenes' que se crea, si no existe, en \n\t\t\t\t      el directorio donde está el ejecutable de la aplicación." +
				"\n\nOpciones:" +
				"\n\n\tCambiar reglas: Aparece una ventana para cambiar las reglas. Si se inicia un nuevo juego, las reglas se mantienen.\n\t\t\t\t    En la parte inferior de la ventana, se muestran algunos ejemplos y los efectos que producen." +
				"\n\n\tCambiar colores: Se muestra una ventana con 5 botones correspondientes a los colores de:\n\t\t\t\t      La línea perimetral de las células ( de color rojo por defecto ),\n\t\t\t\t      el interior de las células vivas ( amarillo por defecto ),\n\t\t\t\t      el interior de las células nacidas en el turno ( amarillo por defecto ) \n\t\t\t\t      el interior de las células muertas en el turno ( color negro por defecto ) y \n\t\t\t\t      el fondo ( negro por defecto )." +
				"\n\n\tVolver a valores por defecto: Vuelve a las reglas y colores por defecto." +
				"\n\n\tGuardado automático de imágenes: Guarda la imagen *.png de la nueva forma en cada ciclo.\n\t\t\t\t\t\t\t\t      Las imagenes se guardan en una carpeta llamada 'imagenes', que se crea si no\n\t\t\t\t\t\t\t\t      existe, en el directorio donde esté el ejecutable de la aplicación.\n\t\t\t\t\t\t\t\t      El nombre de cada imagen contiene la cantidad de ciclos y población." +
				"\n\n\tIdioma: Se puede elegir entre Ingles y Castellano."; 

				int norm2 = buffer.Text.Length;
				buffer.ApplyTag (underlined, buffer.StartIter, buffer.GetIterAtOffset (sub1));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub1), buffer.GetIterAtOffset (norm));
				buffer.ApplyTag (underlined, buffer.GetIterAtOffset (norm), buffer.GetIterAtOffset (sub2));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub2), buffer.GetIterAtOffset (norm2));
			} else {
				buffer.Text = "Basic operation:\n\n";
				int sub1 = buffer.Text.Length;

				buffer.Text += "OrgaSnismo0, is based in the game of life, designed by the Mathematician John Conway.There's a extensive\n information in Wikipedia.\n";
				buffer.Text += "\nWhen the program starts, apart of a menu bar which is commented later, the button 'Draw initial shape' appears.\nAfter do click on this button, the following elements are shown:\n\n" +
				"\tGrid: A new yellow cell is created by clicking over a square. Clicking over a previously created cell, erase it\n\n" +
				"\t'manual cycle' check button: It is checked by defect, and is used to stablish the cycle time by the user.\n\t\t\t\t\t\t\t When is unchecked, the check button and the spinbutton change theirs colours and \n\t\t\t\t\t\t\t the cycle time is stablished automaticaly function the time taken by the CPU to\n\t\t\t\t\t\t\t calculate the next shape, i.e. as fast as the CPU can process the algorithm." +
				"\n\n\tSpin button:  Shows the established cycle time. It is set at 100 milliseconds by defect.\n\t\t\t\tThe cycle time can be changed when 'manual cycle' is checked.\n\t\t\t\tWhen the time that the CPU take to calculate a new shape is bigger than the cycle time \n\t\t\t\tsettled down, the program change to auto by itself." +
				"\n\n\t'Accept' button: To start the cycles once the desired shaped has been drawn or loaded and the cycle time established." +
				"\n\nOnce the game is started, the drawn shape is going changing according to the rules at every new cycle.\nBy defect, the rules are the ones established by Conway. i.e.:\nA cell remains alive if it is adjacent to two or three alive cells, otherwise, the cell dies at next cycle.\nA cell born, if it is adjacent to three alive cells." +
				"\n\nAt the screen bottom left, the population ( alive cells account ) and the cycles passed from the start is shown." +
				"\nAlso the : zoom , displacement and pause, buttons appear.\n\nIf the shape stabilizes or disappears, a message box is shown and the game is over.";

				int norm = buffer.Text.Length;
				buffer.Text += "\n\nMenu bar";
				int sub2 = buffer.Text.Length;
				buffer.Text += "\n\nmfile:" +
				"\n\n\tNew: To start a new game." +
				"\n\n\tSave: Save the current shape, rules and colors in a *.fm mfile\n\t\t   One can save a shape, either that the cycles have started or not. If the cycles are running, it is convenient \n\t\t   to do the mfile saving process with the game paused." +
				"\n\n\tLoad: Load a shape with the rules and colour previously saved in a *.fm mfile." +
				"\n\n\tSave image: Save a *.png image of the current screen.\n\t\t\t      The png mfile, is saved in a directory called 'imagenes' which is created , if it doesn't exist, into \n\t\t\t      the directory where the application executable is." +
				"\n\nOptions:" +
				"\n\n\tChange rules: A window appears an allow to change the rules. The rules will remain if a new game is started.\n\t\t\t\t At the change rules window bottom, there are some rules examples with a brief description of the\n\t\t\t\t respective effects." +
				"\n\n\tChange colors: The 5 buttons shown, control respectively the following colours:\n\t\t\t\t  The perimeter cell line ( red by defect ),\n\t\t\t\t  the alive cell interior ( yellow by defect ),\n\t\t\t\t  the new cells born interior ( yellow by defect ) \n\t\t\t\t  the died cells interior ( black by defect ) and \n\t\t\t\t  the background ( black by defect )." +
				"\n\n\tBack to values by defect: Return to the rules and colours by defect ." +
				"\n\n\tAutosave images: Save a *.png image of the new shape each cycle.\n\t\t\t\t       The png images are saved at the directory called 'imagenes', which is created if it doesn't \n\t\t\t\t       exist, into the directory where the application executable is.\n\t\t\t\t       The mfile name of each png saved image, contains the population and cycles information." +
				"\n\n\tLanguage: Spanish and mEnglish languages are available."; 

				int norm2 = buffer.Text.Length;
				buffer.ApplyTag (underlined, buffer.StartIter, buffer.GetIterAtOffset (sub1));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub1), buffer.GetIterAtOffset (norm));
				buffer.ApplyTag (underlined, buffer.GetIterAtOffset (norm), buffer.GetIterAtOffset (sub2));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub2), buffer.GetIterAtOffset (norm2));
			}
		}


		/////////////////////////////////////////////////////
		///
		/// RIGHT SCROLLING BUTTON HANDLER
		///
		///////////////////////////////////////////////////////////////////////////////////////
		///
		public void btRight_Clicked(object sender, EventArgs e)
		{
			displx += 60 / scale;
			graphicarea.Hide ();
			graphicarea.Show ();
		}

		/// <summary>
		/// 
		/// ZOOM INCREASING BUTTON HANDLER
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void btZoomMore_Clicked(object sender, EventArgs e)
		{
			scale *= 1.25;
			graphicarea.Hide ();
			graphicarea.Show ();
		}

		///////////////////////////////////////////////////
		///
		///   ZOOM REDUCTION BUTTON HANDLER
		///
		//////////////////////////////////////////////////////////////////////////////////////
		///
		public void btZoomLess_Clicked(object sender, EventArgs e)
		{
			scale *= (1 / 1.25);
			graphicarea.Hide ();
			graphicarea.Show ();
		}

		///////////////////////////////////////////
		///
		/// LEFT SCROLLING BUTTON HANDLER
		///
		//////////////////////////////////////////////////////////////////////////
		///
		internal void btLeft_Clicked(object sender,EventArgs e)
		{
			displx -= 60 / scale;
			graphicarea.Hide ();
			graphicarea.Show ();
		}

		///////////////////////////////////////////////////
		///
		///  UP SCROLLING BUTTON HANDLER
		///
		///////////////////////////////////////////////////////////////////////////
		///
		internal void btUp_Clicked(object sender, EventArgs e)
		{
			desply -= 60 / scale;
			graphicarea.Hide ();
			graphicarea.Show ();
		}
			
		/////////////////////////////////////////
		///
		///  DOWN SCROLLING BUTTON HANDLER
		///
		///////////////////////////////////////////////////////////////////////////////
		///
		internal void btDown_Clicked(object sender, EventArgs e)
		{
			desply += 60 / scale;
			graphicarea.Hide ();
			graphicarea.Show ();
		}

		/// <summary>
		/// 
		/// AUTOMATIC TO MANUAL CYCLE HANDLER
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public void CbManual_Clicked (object sender, EventArgs e)
		{
			if (cbManual.Active) {
				lbAutoCycle.ModifyFg (StateType.Normal, new Gdk.Color (0, 250, 0));
				if (!Anglo)
					lbAutoCycle.Text = "Periodo en milisegundos  manual.";
				else
					lbAutoCycle.Text = "Period in milliseconds   manual.";
				sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (0, 250, 0));
			} else {
				lbAutoCycle.ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
				if (!Anglo)
					lbAutoCycle.Text = "Periodo en milisegundos  auto";
				else
					lbAutoCycle.Text = "Period in milliseconds   auto";
				sbPeriod.ModifyBase (StateType.Normal, new Gdk.Color (100, 100, 100));
			}
		}

		////////////////////////////////////////////////////////
		////
		/// CYCLE TIME CHANGE HANDLER
		///
		//////////////////////////////////////////////////////////////////////////
		///
		public void sbPeriod_ValueChanged (object sender, EventArgs e)
		{
			organismo.Cycle = (int)sbPeriod.Value;
		}


		/// <summary>
		/// 
		/// SHOWS THE EGO LABEL WHEN THE MOUSE POINTER  IS OVER IT
		/// 
		/// </summary>
		/// <param name="o">O.</param>
		/// <param name="args">Arguments.</param>
		/// 
		public void LbEgo_QueryTooltip (object o, QueryTooltipArgs args)
		{
			lbEgo.Text ="Xadnem";
		}

		/// <summary>
		/// 
		/// PAUSES THE GAME
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal void btPause_Clicked(object sender,EventArgs e)
		{
			if (!organismo.Pause) {
				btAccept.ModifyBg (StateType.Normal, new Gdk.Color (240, 20, 0));
				if (!Anglo)
					btAccept.Label = "En pause";
				else
					btAccept.Label = "Paused";
				organismo.Pause = true;
			} else {
				btAccept.ModifyBg (StateType.Normal, new Gdk.Color (10, 240, 100));
				if (!Anglo)
					btAccept.Label = "Activo";
				else
					btAccept.Label = "Active";
				organismo.Pause = false;
				organismo.PulseTimer ();
			}
		}

		//////////////////////////////////////////////////////////////
		///
		///		TO ENSURE THAT NO OTHERS THAN DIGITS ARE INTRODUCED INTO THE RULES DIALOG BOXES
		///
		////////////////////////////////////////////////////////////////////////////////////
		///
		[GLib.ConnectBefore()]
		private void RulesEntry_KeyPress(object sender,KeyPressEventArgs e)
		{
			if ( e.Event.Key == Gdk.Key.Key_9 ||  e.Event.Key != Gdk.Key.BackSpace && !char.IsDigit ((char)e.Event.KeyValue)) {
				{
					e.RetVal = true;
				}
			}
		}

		/////////////////////////////////////////////////
		///
		///   RULES CHANGE DIALOG AUXILIARY METHODS
		///
		/////////////////////////////////////////////////////////////////////////////////////
		///
		void BtbackgroundC_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogof = new ColorSelectionDialog ("Color del backgroundC.");
			else
				dialogof = new ColorSelectionDialog ("Background color.");
			dialogof.Show ();
			dialogof.OkButton.Clicked += AcceptBackgroundColor;
			dialogof.CancelButton.Clicked += Dialogof_CancelButton_Clicked;
		}

		void Dialogof_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogof.Hide ();
		}

		void AcceptBackgroundColor(object sender,EventArgs e)
		{
			Gdk.Color choosen = dialogof.ColorSelection.CurrentColor;
			btbackgroundC.ModifyBg (StateType.Normal, choosen);
			backgroundC = GdkToCairoColor (choosen);
			dialogof.Hide ();
		}

		void BtDeadCells_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogom = new ColorSelectionDialog ("Color de las células muertas");
			else
				dialogom = new ColorSelectionDialog ("Dead cells color.");
			dialogom.Show ();
			dialogom.OkButton.Clicked += AcceptDeadCellsColor;
			dialogom.CancelButton.Clicked += Dialogom_CancelButton_Clicked;
		}

		void Dialogom_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogom.Hide ();
		}

		void AcceptDeadCellsColor(object sender,EventArgs e)
		{
			Gdk.Color choosen = dialogom.ColorSelection.CurrentColor;
			btDeadCells.ModifyBg (StateType.Normal, choosen);
			interiorD = GdkToCairoColor (choosen);
			dialogom.Hide ();
		}


		void BtBornCells_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogon = new ColorSelectionDialog ("Color de las nuevas células");
			else
				dialogon = new ColorSelectionDialog ("New cells color.");
			dialogon.Show ();
			dialogon.OkButton.Clicked += AcceptBornCellsColors;
			dialogon.CancelButton.Clicked += Dialogon_CancelButton_Clicked;
		}

		void Dialogon_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogon.Hide ();
		}

		void AcceptBornCellsColors(object sender,EventArgs e)
		{
			Gdk.Color choosen = dialogon.ColorSelection.CurrentColor;
			btBornCells.ModifyBg (StateType.Normal, choosen);
			interiorA = GdkToCairoColor (choosen);
			dialogon.Hide ();
		}

		void BtLines_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogol = new ColorSelectionDialog ("Color de la linea");
			else
				dialogol = new ColorSelectionDialog ("Line color.");
			dialogol.Show ();
			dialogol.OkButton.Clicked += AcceptLineColor;
			dialogol.CancelButton.Clicked += Dialogol_CancelButton_Clicked;
		}

		void Dialogol_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogol.Hide ();
		}

		void AcceptLineColor(object sender,EventArgs e)
		{
			Gdk.Color choosen = dialogol.ColorSelection.CurrentColor;
			btLines.ModifyBg (StateType.Normal, choosen);
			exterior = GdkToCairoColor (choosen);
			dialogol.Hide ();
		}

		void Btalives_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogov = new ColorSelectionDialog ("Color interior de las células vivas");
			else
				dialogov = new ColorSelectionDialog ("Inner color of alive cells.");
			dialogov.Show ();
			dialogov.OkButton.Clicked += AcceptAliveCellsColor;
			dialogov.CancelButton.Clicked += Dialogov_CancelButton_Clicked;
		}

		void Dialogov_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogov.Hide ();
		}

		void AcceptAliveCellsColor(object sender,EventArgs e)
		{
			Gdk.Color choosen = dialogov.ColorSelection.CurrentColor;
			btalivecells.ModifyBg (StateType.Normal, choosen);
			interior = GdkToCairoColor(choosen);
			dialogov.Hide ();
		}

		/// <summary>
		/// 
		/// TO CONVERT FROM GDK COLOR TO CAIRO COLOR
		/// 
		/// </summary>
		/// <returns>The to cairo color.</returns>
		/// <param name="color">Color.</param>
		/// 
		private static Cairo.Color GdkToCairoColor( Gdk.Color color)
		{
			return new Cairo.Color ((double)color.Red / ushort.MaxValue,
				(double)color.Green / ushort.MaxValue, (double)color.Blue / ushort.MaxValue);
		}

		/// <summary>
		/// 
		/// TO CONVERT FROM CAIRO COLOR TO GDK COLOR
		/// 
		/// </summary>
		/// <returns>The color to gdk.</returns>
		/// <param name="color">Color.</param>
		/// 
		private static Gdk.Color CairoColorToGdk(Cairo.Color color)
		{
			Gdk.Color c = new Gdk.Color ();
			c.Blue = (ushort)(color.B * ushort.MaxValue);
			c.Red = (ushort)(color.R * ushort.MaxValue);
			c.Green = (ushort)(color.G * ushort.MaxValue);
			return c;
		}

	

	}
}
