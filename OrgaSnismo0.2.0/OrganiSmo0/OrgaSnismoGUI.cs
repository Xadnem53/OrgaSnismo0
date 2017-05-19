using System;
using Gtk;
using Gdk;
using Pango;
using Cairo;

//!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!!!!!!!!!!!!!!!!!!!!!!!!!
//
//  INICIADO 3/2/2017 para monodevelop
//
//!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!!!!!!!!!!!!!!!!!!!!!!!!!


namespace OrganiSmo0
{
	public class OrgaSnismoGui
	{
		internal double tamaño = 25; //Tamaño de cada célula por defecto
		internal PointD origen = new PointD(0,0); // Punto de origen del grafico

		internal string viva = "23"; // Valores por defecto para que una celula siga viva
		internal string nacida = "3"; // Valor por defecto para que una celula nazca
		internal string prefijo =""; // Prefijo introducido por el usuario para los nombres de las imagenes en guardado automatico

		internal Gtk.Window ventana;

		internal Entry tbTamaño; // Caja de texto para cuadro de dialogo del tamaño de la celula
		internal Entry tbNacida; // Caja de texto con la cantidad de celulas adyacentes para que una celula nazca
		internal Entry tbViva; //Caja de texto con la cantidad de celulas adyacentes para que una celula permanezca viva

		internal MessageDialog dialogotamaño; // Dialogo para el tamaño de la celula
		internal MessageDialog dialogoreglas; // Dialogo para el cambio de normas
		internal MessageDialog DialogoColores;

		internal Label lbColorLinea; // Para el dialogo de colores
		internal Label lbColorInterior; // Idem anterior
		internal Label lbColorFondo; // Idem
		internal Label lbColorInteriorM; // Interior de las celulas muertas en el turno
		internal Label lbColorInteriorV; // Interior de las nuevas celulas en el turno
		internal Label lbEgo;
		internal Label lbPoblacion;
		internal Label lbCiclos;
		internal Label lbTituloCiclos; // Para mostrar cuando se entra en periodo automatico.
		internal Label lbRegresivo; // Para mostrar el tiempo que queda para el siguiente ciclo.

		internal bool cicloautomatico = false; // Para registrar cuando el programa cambia el ciclo por problemas de rendimiento
		internal bool beep = false; // Para registrar cuando esta habilitado el beep y cuando no
		internal bool guardadoautomatico = false; // Para registrar cuando esta activado el guardado automatico de imagenes y cuando no

		internal Cairo.Color exterior = new Cairo.Color (1, 0, 0); //Color de la linea alrrededor de la celula viva por defecto
		internal Cairo.Color interior = new Cairo.Color(0.65D, 1, 0.1D); // Color interior de la celula viva por defecto
		internal Cairo.Color fondo = new Cairo.Color(0,0,0); // Color negro del fondo por defecto
		internal Cairo.Color interiorM = new Cairo.Color(0,0,0); // Color de las celulas muertas en el turno por defecto.
		internal Cairo.Color interiorV = new Cairo.Color(0.65D,1,0.1D); // Color de las nuevas celulas en el turno por defecto

		internal FileChooserDialog dialogosalvar; //Dialogo para salvar las formas

	    internal MenuBar menu;

		internal Menu archivomenu;
	    internal MenuItem archivo;
		internal MenuItem nuevo;
		internal MenuItem guardar;
		internal MenuItem cargar;
		internal MenuItem guardarimagen;

		internal Menu opcionesmenu;
		internal MenuItem opciones;
		internal MenuItem cambiarreglas;
		internal MenuItem volveravalorespordefecto;
		internal MenuItem cambiarcolores;
		internal MenuItem idioma;
		internal MenuItem Español;
		internal MenuItem English;
		internal MenuItem guardadoautomaticoimagenes;

		internal MenuItem ayuda;

		internal VBox vbMenu;
		internal VBox vbMetadatos; //Para el spinbutton de los ciclos y las etiquetas de poblacion y ciclos
		internal VBox vbCrono; // Para el checkbox de ciclo manual y el contador regresivo

		internal HBox hbContinuar;

		internal Gtk.Alignment almenu;
		internal Gtk.Alignment alrb;
		internal Gtk.Alignment alAreaGrafica;
		internal Gtk.Alignment alEgo;
		internal Gtk.Alignment alAceptar;
		internal Gtk.Alignment alPnDesplazamiento;
		internal Gtk.Alignment alPnZoom;
		internal Gtk.Alignment alsbPeriodo;

		internal Fixed pnDesplazamiento;
		internal Fixed pnZoom;

		internal Button btDibujarForma;
		internal Button btPausa;
		internal Button btAceptar;
		Button btIzquierda;
		Button btDerecha;
		Button btArriba;
		Button btAbajo;
		Button btZoomMas;
		Button btZoomMenos;

		internal CheckButton cbManual; // Para registrar cuando se entra en modo de ciclo manual.

		internal SpinButton sbPeriodo;

	   internal DrawingArea areagrafica;

	    internal Cairo.Context contexto;

	public OrgaSnismoGui()
		{
			areagrafica = new DrawingArea ();
			areagrafica.ExposeEvent += Areagrafica_ExposeEvent;
			areagrafica.Events = EventMask.PointerMotionMask;
			areagrafica.Events = EventMask.ButtonPressMask;

			ventana = new Gtk.Window ("OrgaSnismo0                                Twister edition");
			ventana.SetSizeRequest (1000, 600);
			ventana.Maximize ();
			ventana.Visible = true;
			ventana.ModifyBg (StateType.Normal, new Gdk.Color (0, 0, 0));
			ventana.DeleteEvent += Ventana_DeleteEvent;

			lbEgo = new Label ();
		
			//Barra de menus
			menu = new MenuBar ();  

			//Menu archivo
			archivo = new MenuItem ("Archivo");
			archivomenu = new Menu (); 
			archivo.Submenu = archivomenu;

			nuevo = new MenuItem ("Nuevo");
			nuevo.Visible = true;
			guardar = new MenuItem ("Guardar");
			cargar = new MenuItem ("Cargar");
			guardarimagen = new MenuItem ("Guardar imagen");

			archivomenu.Append (nuevo);
			archivomenu.Append (guardar);
			archivomenu.Append (cargar);
			archivomenu.Append (guardarimagen);

			nuevo.Activated += Abrir_Onactivated;
			guardar.Activated += Guardar_Activated;
			cargar.Activated += Cargar_Activated;
			guardarimagen.Activated += Guardarimagen_Activated;

			//Menu de opciones
			opcionesmenu = new Menu ();
			opciones = new MenuItem ("Opciones");
			opciones.Submenu = opcionesmenu;

			cambiarreglas = new MenuItem ("Cambiar reglas.");
			volveravalorespordefecto = new MenuItem ("Volver a valores por defecto.");
			cambiarcolores = new MenuItem ("Cambiar colores.");
			guardadoautomaticoimagenes = new MenuItem ("Guardado automatico de imagenes.");
			idioma = new MenuItem ("Idioma");
			Español = new MenuItem ("Español");
			Español.Activated += Español_Activated;
			English = new MenuItem ("English");
			English.Activated += English_Activated;

			opcionesmenu.Append (cambiarreglas);
			opcionesmenu.Append (cambiarcolores);
			opcionesmenu.Append (volveravalorespordefecto);
			opcionesmenu.Append (guardadoautomaticoimagenes);
			opcionesmenu.Append (idioma);

			Menu idiomas = new Menu ();
			idioma.Submenu = idiomas;

			idiomas.Append (Español);
			idiomas.Append (English);


			cambiarreglas.Activated += Cambiarreglas_Activated;
			volveravalorespordefecto.Activated += Volveravalorespordefecto_Activated;
			cambiarcolores.Activated += Cambiarcolores_Activated;
			guardadoautomaticoimagenes.Activated += Guardadoautomaticoimagenes_Activated;

			ayuda = new MenuItem ("Ayuda");
			ayuda.Activated += Ayuda_Activated;

			menu.Append (archivo);
			menu.Append (opciones);
			menu.Append (ayuda);

			vbMenu = new VBox ();
			almenu = new Gtk.Alignment (0, 0, 0, 0);
			vbMenu.PackStart (menu, false, false, 0);
			vbMenu.Add (almenu);

			alAreaGrafica = new Gtk.Alignment (0, 0, 0, 0);
			alAreaGrafica.Add (areagrafica); 
			vbMenu.Add (alAreaGrafica);

			//Añadir el HBox con la etiqueta del Ego y los botones de desplazamiento y zoom
			alEgo = new Gtk.Alignment (0, 0, 0, 0);
			alEgo.HeightRequest = 205;
			hbContinuar = new HBox ();
			hbContinuar.HeightRequest = 205;
			hbContinuar.PackEnd (alEgo);

			alAceptar = new Gtk.Alignment (1F, 0.5F, 0, 0);
			alAceptar.Visible = true;

			btAceptar = new Button ();
			btAceptar.Label = "Aceptar";
			btAceptar.SetSizeRequest (100, 30);
			btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (0, 240, 30));
			btAceptar.Visible = true;
			btAceptar.Clicked += BtAceptar_Clicked;
			alAceptar.Add (btAceptar);
			hbContinuar.PackEnd (alAceptar);

			//Construir el panel con los botones de desplazamiento
			alPnDesplazamiento = new Gtk.Alignment (0.5F, 0.5F, 0, 0);
			alPnDesplazamiento.Visible = true;

			btDerecha = new Button ();
			btDerecha.ModifyBg (StateType.Normal, new Gdk.Color (0, 0, 180));
			btDerecha.Label = char.ToString ('\u25B6');
			btDerecha.Visible = true;
			btDerecha.SetSizeRequest (50, 50);
			btDerecha.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btDerecha.Clicked += BtDerecha_Clicked;

			btArriba = new Button ();
			btArriba.ModifyBg (StateType.Normal, new Gdk.Color (0, 240, 240));
			btArriba.Label = char.ToString ('\u25B2');
			btArriba.Visible = true;
			btArriba.SetSizeRequest (50, 50);
			btArriba.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btArriba.Clicked += BtArriba_Clicked;

			btAbajo = new Button ();
			btAbajo.ModifyBg (StateType.Normal, new Gdk.Color (240, 0, 0));
			btAbajo.Label = char.ToString ('\u25BC');
			btAbajo.Visible = true;
			btAbajo.SetSizeRequest (50, 50);
			btAbajo.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btAbajo.Clicked += BtAbajo_Clicked;

			btIzquierda = new Button ();
			btIzquierda.ModifyBg (StateType.Normal, new Gdk.Color (200, 0, 200));
			btIzquierda.Label = char.ToString ('\u25C0');
			btIzquierda.Visible = true;
			btIzquierda.SetSizeRequest (50, 50);
			btIzquierda.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 19"));
			btIzquierda.Clicked += BtIzquierda_Clicked;

			pnDesplazamiento = new Fixed ();
			pnDesplazamiento.SetSizeRequest (150, 150);
			pnDesplazamiento.Visible = true;
			pnDesplazamiento.Put (btDerecha, 100, 50);
			pnDesplazamiento.Put (btArriba, 50, 0);
			pnDesplazamiento.Put (btAbajo, 50, 100);
			pnDesplazamiento.Put (btIzquierda, 0, 50);

			alPnDesplazamiento.Add (pnDesplazamiento);
			hbContinuar.PackEnd (alPnDesplazamiento);
			// Construir el panel con los botones de zoom
			btZoomMas = new Button ();
			btZoomMas.SetSizeRequest (50, 50);
			btZoomMas.Label = "+";
			btZoomMas.Visible = true;
			btZoomMas.ModifyBg (StateType.Normal, new Gdk.Color (180, 180, 180));
			btZoomMas.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans 17"));
			btZoomMas.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 50));
			btZoomMas.Clicked += BtZoomMas_Clicked;

			btZoomMenos = new Button ();
			btZoomMenos.SetSizeRequest (50, 50);
			btZoomMenos.Label = "-";
			btZoomMenos.Visible = true;
			btZoomMenos.ModifyBg (StateType.Normal, new Gdk.Color (80, 80, 80));
			btZoomMenos.Children [0].ModifyFont (FontDescription.FromString ("Dejavu-Sans Bold 29"));
			btZoomMenos.Children [0].ModifyFg (StateType.Normal, new Gdk.Color (0, 240, 100));
			btZoomMenos.Clicked += BtZoomMenos_Clicked;
			alPnZoom = new Gtk.Alignment (0.5F, 0.5F, 0, 0);
			alPnZoom.WidthRequest = 350;
			alPnZoom.Visible = true;

			pnZoom = new Fixed ();
			pnZoom.SetSizeRequest (150, 50);
			pnZoom.Visible = true;
			pnZoom.Put (btZoomMas, 100, 0);
			pnZoom.Put (btZoomMenos, 0, 0);

			alPnZoom.Add (pnZoom);
			hbContinuar.PackEnd (alPnZoom);

			vbCrono = new VBox (true, 10);
			vbCrono.Visible = true;
			vbCrono.WidthRequest = 300;

			cbManual = new CheckButton ("Ciclo manual");
			cbManual.Children[0].ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
			cbManual.Children [0].ModifyFg (StateType.Active, new Gdk.Color (0, 250, 0));
			cbManual.Visible = true;
			cbManual.Clicked += CbManual_Clicked;

			lbRegresivo = new Label ();
			lbRegresivo.WidthRequest = 300;
			lbRegresivo.Visible = true;
			lbRegresivo.Text = "Próximo ciclo: ";
			lbRegresivo.ModifyFont (FontDescription.FromString ("Dejavu-Sans 13"));
			lbRegresivo.ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));

			vbCrono.Add (cbManual);
			vbCrono.Add (lbRegresivo);
			Gtk.Alignment alvbCrono = new Gtk.Alignment (0.5F, 0.5F, 0, 0);
			alvbCrono.Visible = true;
			alvbCrono.Add (vbCrono);

			hbContinuar.PackEnd (alvbCrono);


			vbMetadatos = new VBox ();
			vbMetadatos.WidthRequest = 200;
			vbMetadatos.Visible = true;

			Gtk.Alignment alTituloCiclos = new Gtk.Alignment (0, 0.5F, 0, 0);
			alTituloCiclos.Visible = true;
			lbTituloCiclos = new Label ();
			lbTituloCiclos.ModifyFont (FontDescription.FromString ("Dejavu-Sans 11"));
			lbTituloCiclos.ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 240));
			lbTituloCiclos.Visible = true;
			lbTituloCiclos.SetSizeRequest (400, 30);
			lbTituloCiclos.Text = "Periodo en milisegundos      manual";
			alTituloCiclos.Add (lbTituloCiclos);
			vbMetadatos.Add (alTituloCiclos);

			sbPeriodo = new SpinButton (100, 10000000, 100);
			sbPeriodo.ModifyFont (FontDescription.FromString ("Dejavu-Sans 17"));
			sbPeriodo.Visible = true;
			sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (0, 240, 0));
			sbPeriodo.ValueChanged += SbPeriodo_ValueChanged;
			vbMetadatos.Add (sbPeriodo);

			Gtk.Alignment alPoblacion = new Gtk.Alignment (0, 0.5F, 0, 0);
			alPoblacion.Visible = true;

			lbPoblacion = new Label ();
			lbPoblacion.Visible = true;
			lbPoblacion.Text = "Poblacion: ";
			lbPoblacion.ModifyFont (FontDescription.FromString ("Dejavu-Sans 13"));
			lbPoblacion.ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 240));
			alPoblacion.Add (lbPoblacion);
			vbMetadatos.Add (alPoblacion);

			Gtk.Alignment alCiclos = new Gtk.Alignment (0, 0.5F, 0, 0);
			alCiclos.Visible = true;

			lbCiclos = new Label ();
			lbCiclos.Visible = true;
			lbCiclos.Text = "Ciclos: ";
			lbCiclos.ModifyFont (FontDescription.FromString ("Dejavu-Sans 13"));
			lbCiclos.ModifyFg (StateType.Normal, new Gdk.Color (0, 150, 240));
			alCiclos.Add (lbCiclos);
			vbMetadatos.Add (alCiclos);
			hbContinuar.PackEnd (vbMetadatos);

			vbMenu.Add (hbContinuar);

			//Añadir el evento de pulsado del boton izquierdo del mouse al DrawingArea areagrafica
			areagrafica.AddEvents (1);
			areagrafica.ButtonPressEvent += Areagrafica_ButtonPressEvent;

			btDibujarForma = new Button ();
			btDibujarForma.Visible = true;
			btDibujarForma.ModifyBg (StateType.Normal, new Gdk.Color (0, 100, 200));
			btDibujarForma.Label = "Dibujar forma inicial";
			btDibujarForma.Clicked += DibujarFormaInicial;

			alrb = new Gtk.Alignment (0, 0F, 0, 0);
			alrb.SetSizeRequest (1000, 550);
			alrb.Add (btDibujarForma);
			vbMenu.Add (alrb);

			hbContinuar = new HBox ();
			hbContinuar.Visible = true;
			hbContinuar.SetSizeRequest (1200, 50);

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

			hbContinuar.Add (alEgo);

			vbMenu.Add (hbContinuar);
			vbMenu.Add (alEgo);
 
			ventana.Add (vbMenu);
			ventana.ShowAll ();
		
		}

	public virtual void Ayuda_Activated (object sender, EventArgs e)
	{
			
	}
			

	public virtual void English_Activated (object sender, EventArgs e)
	{
		
	}

	public virtual void Español_Activated (object sender, EventArgs e)
	{
		
	}

	public virtual void CbManual_Clicked (object sender, EventArgs e)
	{
		
	}

	public virtual void SbPeriodo_ValueChanged (object sender, EventArgs e)
	{
		
	}
			

	public virtual void Ventana_DeleteEvent (object o, DeleteEventArgs args)
	{
			System.Environment.Exit (0);
			Application.Quit ();
	}
			
		public virtual void BtZoomMenos_Clicked (object sender, EventArgs e)
		{
		   // Implementado
		}

		public virtual void BtZoomMas_Clicked (object sender, EventArgs e)
		{
		   //Implementado
		}
			
		public virtual void DibujarFormaInicial ( object sender, EventArgs e)
		{
			//Implementado
		}

		public virtual void LbEgo_QueryTooltip ( object sender, QueryTooltipArgs e)
		{
			//Implementado
		}

		public virtual void Areagrafica_ButtonPressEvent (object sender, ButtonPressEventArgs e)
		{
			//Implementado
		}
		internal virtual void BtAceptar_Clicked (object sender, EventArgs e)
		{
			//Implementado
		}

		internal virtual void Areagrafica_ExposeEvent(object sender,ExposeEventArgs e)
		{
			//Implementado
		}

		internal virtual void BtPausa_Clicked( object sender, EventArgs e)
		{
			//Implementado
		}

		internal virtual void BtIzquierda_Clicked (object sender, EventArgs e)
		{
			//Implementado
		}

		internal virtual void BtDerecha_Clicked (object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void BtAbajo_Clicked(object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void BtArriba_Clicked(object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void Cargar_Activated (object sender, EventArgs e)
		{ 
			//Implementado
		}

		internal virtual void Guardar_Activated (object sender, EventArgs e)
		{
			//Implementado
		}

		internal virtual void Abrir_Onactivated (object sender,EventArgs e)
		{
			//Implementado
		}
		public virtual void Guardadoautomaticoimagenes_Activated ( object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void Guardarimagen_Activated (object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void Cambiarcolores_Activated (object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void Volveravalorespordefecto_Activated (object sender, EventArgs e)
		{
			//Implementado
		}
		internal virtual void Cambiarreglas_Activated (object sender, EventArgs e)
		{
			//Implementado
		}

	



	}
}
