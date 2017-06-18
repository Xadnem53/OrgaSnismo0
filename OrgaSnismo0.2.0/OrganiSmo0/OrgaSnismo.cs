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
	public class OrgaSnismo:OrgaSnismoGui//,IDisposable
	{
		public struct estado // Estructura para encapsular el estado de un organismo
		{
			public List<PointD> celulasvivas;
			public List<PointD> proximasvivas;
			public List <PointD> proximasmuertas;
			public int tiempocalculo;

			public estado( List <PointD> vivas, List<PointD> pvivas, List <PointD> pmuertas,int tiempocal)
			{
				celulasvivas = vivas;
				proximasvivas = pvivas;
				proximasmuertas = pmuertas;
				tiempocalculo = tiempocal;
			}
		}

	    estado [] estados;
		int indiceconsumidor = 0; 
		int indiceproductor = 0;
		Thread hiloproductor;
		estado estadoanterior; // Para guardar el estado anterior al nuevo estado producido
 
		List <PointD> celulas; // Lista de las celulas iniciales
		List<int> continuaviva; // Lista de la cantidad de celulas adyacentes para que una celula siga viva segun las reglas 
		List<int> nacera; // Lista de la cantidad de celulas adyacentes para que una celula nazca segun las reglas


		int ciclos = 0; // Cantidad de ciclos desde el inicio de la forma.
		int ciclo = 100; // Tiempo entre ciclos
		int tiempocalculo = 0; // Para registrar el tiempo que se tarda en hacer los calculos
		double escala = 1; // Escala de visualizacion por defecto
		double desplx = 0;// Desplazamiento en sentido X del origen del area grafica
		double desply = 0;// Desplazamiento en sentido Y del origen del area grafica

		DateTime tinicio; // Para el tiempo de calculo
		DateTime tfinal;
		DateTime tcalculando; //Para mostrar el tiempo de calculo por desbordamiento

		bool pausa = false; // Para registrar cuando el organismo esta en pausa
		bool iniciado = false; // Para registrar cuando se ha iniciado el OrgaSnismo
		bool finalizado = false; // Para registrar cuando se ha de finalizar.
		bool cicloautomatico = false; // Para registrar cuando se entra en modo de ciclo automatico
		bool cargado = false; // Para registrar cuando se ha cargado una forma desde el disco y dar tiempo para que el hilo productor saque ventaja del consumidor.
		bool Anglo = false; // Para registrar cuando el idioma es Ingles.


		string rutasalvado; // Para guardar la ruta del fichero correspondiente a una forma


		/// <summary>
		/// 
		/// CONSTRUCTOR 
		/// 
		/// </summary>
		/// 
		public OrgaSnismo()
		{
			pnZoom.Visible = false;
			pnDesplazamiento.Visible = false;
			btAceptar.Visible = false;
			sbPeriodo.Visible = false;
			areagrafica.Visible = false;
			vbMetadatos.Visible = false;
			nacera = new List<int> () { 3 }; // Cantidad de celulas vivas adyacentes para que una celula nazca
			continuaviva = new List <int>() {2,3}; //Cantidad de celulas vivas adyacentes para que una celula permanezca viva
			estados = new estado[50000];
			hiloproductor = new Thread (ActualizarOrganismo);
			areagrafica.SetSizeRequest (1300, 592);
			celulas = new List<PointD> ();
			areagrafica.AddEvents (1);
			areagrafica.ButtonPressEvent += Areagrafica_ButtonPressEvent;
			vbCrono.Visible = false;
		}
			

		/// <summary>
		/// 
		/// PARA DIBUJAR UNA NUEVA FORMA AL PULSAR EL BOTON
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public override void DibujarFormaInicial (object sender, EventArgs e)
		{
			//Ocultar la etiqueta del Ego si estaba visible
			lbEgo.Text = " ";
			//Ocultar y visualizar los controles correspondientes
			alrb.Hide ();
			hbContinuar.HeightRequest += 10;
			almenu.HeightRequest = 40;
			sbPeriodo.Visible = true;
			btAceptar.Visible = true;
			vbMetadatos.Visible = true;
			lbCiclos.Visible = false;
			lbPoblacion.Visible = false;
			vbCrono.Show ();
			cbManual.Active = true;
			lbRegresivo.Visible = false;
			//Forzar el redibujado del area grafica para que se vea la cuadricula
			areagrafica.Show ();
			// Si la forma se ha cargado, mostrar el tiempo de calculo de la primera iteracion
			if (cargado) {
				sbPeriodo.Value = tiempocalculo;
				cbManual.Active = false;
			}
		}

		/// <summary>
		/// 
		/// MANEJADOR DEL EVENTO BUTTONPRESS QUE DIBUJA UNA CELULA Y LA AÑADE A LA LISTA
		/// DE CELULAS EN LA UBICACION DONDE EL USUARIO PULSA SOBRE LA CUADRICULA SI NO 
		/// EXISTIA YA ESA CELULA. SI LA CELULA YA EXISTIA, LA BORRA DE LA CUADRICULA Y LA
		/// ELIMINA DE LA LISTA DE CUADRICULAS
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		/// 
		public new void Areagrafica_ButtonPressEvent (object sender, ButtonPressEventArgs args)
		{
			if (!cargado) {//Si es una forma cargada, imposibilitar el dibujado de nuevas celulas
				contexto = Gdk.CairoHelper.Create (areagrafica.GdkWindow);
				int x = 0;
				int y = 0;
				areagrafica.GetPointer (out x, out y);
				//Obtener el vertice superior izquierdo de la cuadricula sobre la que el usuario
				//ha pulsado.
				double posicionh = (int)(x / tamaño) * tamaño;
				double posicionv = (int)(y / tamaño) * tamaño;
				PointD celula = new PointD (posicionh, posicionv);
				//Pintar la celula en la cuadricula y añadirla a la lista de celulas vivas
				if (!celulas.Contains (celula)) {
					contexto.MoveTo (new PointD (posicionh, posicionv));
					contexto.LineTo (new PointD (posicionh + tamaño, posicionv));
					contexto.LineTo (new PointD (posicionh + tamaño, posicionv + tamaño));
					contexto.LineTo (new PointD (posicionh, posicionv + tamaño));
					contexto.LineTo (new PointD (posicionh, posicionv));
					contexto.SetSourceColor (new Cairo.Color (1, 1, 0.1));
					contexto.Fill ();
					celulas.Add (celula);
					//Borrar la celula anteriormente dibujada y eliminarla de la lista de celulas vivas
				} else {
					contexto.MoveTo (celula);
					contexto.LineTo (new PointD (posicionh + tamaño, posicionv));
					contexto.LineTo (new PointD (posicionh + tamaño, posicionv + tamaño));
					contexto.LineTo (new PointD (posicionh, posicionv + tamaño));
					contexto.LineTo (celula);
					contexto.SetSourceColor (new Cairo.Color (0, 0, 0));
					contexto.Fill ();
					celulas.Remove (celula);
				}
			}
		}

		/// <summary>
		/// 
		/// PONE EN MARCHA LOS CICLOS DE VIDA DEL ORGANISMO PREVIAMENTE DIBUJADO O CARGADO
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		///
		internal override void BtAceptar_Clicked(object sender, EventArgs e)
		{
			if (!iniciado && celulas.Count > 0) {
				// Cambiar el color y la leyenda del boton continuar
				if (!Anglo)
					btAceptar.Label = "Activo";
				else
					btAceptar.Label = "Active";
				btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (10, 240, 100));
				//Eliminar el evento para dibujar celulas en el area grafica
				areagrafica.ButtonPressEvent -= Areagrafica_ButtonPressEvent;
				iniciado = true;
				// Meter el estado inicial en la lista de estados
				estados [0] = new estado (new List<PointD> (), new List<PointD> (), new List<PointD> (), 0);
				foreach (PointD p in celulas)
					estados [0].celulasvivas.Add (new PointD (p.X, p.Y));
				// Mostrar los controles correspondientes
				pnZoom.Visible = true;
				pnDesplazamiento.Visible = true;
				lbPoblacion.Visible = true;
				lbCiclos.Visible = true;
				lbRegresivo.Visible = true;
				//Fijar el ciclo segun el valor del spinbutton
				ciclo = (int)sbPeriodo.Value;
				//Crear un objeto estadoanterior para evitar errores en el metodo EvaluarVida
				estadoanterior = new estado (new List<PointD> (), new List<PointD> (), new List<PointD> (), 0);
				for (int i = 0; i < estados [0].celulasvivas.Count; i++)
					estadoanterior.celulasvivas.Add (new PointD (estados [0].celulasvivas [i].X, estados [0].celulasvivas [i].Y));
				for (int i = 0; i < estados [0].proximasvivas.Count; i++)
					estadoanterior.proximasvivas.Add (new PointD (estados [0].proximasvivas [i].X, estados [0].proximasvivas [i].Y));
				for (int i = 0; i < estados [0].proximasmuertas.Count; i++)
					estadoanterior.proximasmuertas.Add (new PointD (estados [0].proximasmuertas [i].X, estados [0].proximasvivas [i].Y));
				/*
				if (cargado) {
					sbPeriodo.Value = tiempocalculo;
					ciclo = tiempocalculo;
				}
				*/
				//Registrar el momento e iniciar el calculo
				tinicio = DateTime.Now;
				tcalculando = DateTime.Now;
				hiloproductor.Start ();
			} else if (iniciado) { // Activar y desactivar la pausa
				if (!pausa) {
					btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (240, 20, 0));
					if (!Anglo)
						btAceptar.Label = "En pausa";
					else
						btAceptar.Label = "Paused";
					pausa = true;
				} else {
					btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (10, 240, 100));
					if (!Anglo)
						btAceptar.Label = "Activo";
					else
						btAceptar.Label = "Active";
					pausa = false;
					PulsarTimer ();
				}
			}
		}

		/// <summary>
		/// 
		/// PARA FORZAR EL REDIBUJADO DEL AREA GRAFICA
		/// 
		/// </summary>
		/// 
		private void PulsarTimer()
		{
			areagrafica.Hide ();
			areagrafica.Show();
		}

		/// <summary>
		/// 
		/// MUESTRA EN PANTALLA LA FORMA INICIAL DIBUJADA POR EL USUARIO O DIBUJA EL ESTADO
		/// DEL ORGANISMO EN CADA CICLO
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		/// 
		internal override void Areagrafica_ExposeEvent (object sender, ExposeEventArgs args)
		{
			//Crear el Gtk.Context para dibujar en la ventana del DrawingArea
			contexto = Gdk.CairoHelper.Create (areagrafica.GdkWindow);
			//Desplazar el origen de coordenadas al punto central de la pantalla
			contexto.Translate (650, 298);
			if (!iniciado) { 
				// Pintar la rejilla para dibujar la forma inicial
				contexto.SetSourceColor (new Cairo.Color (0, 0, 0));
				contexto.Paint ();
				contexto.LineWidth = 1;
				contexto.Scale (escala, escala);
				contexto.SetSourceColor (new Cairo.Color (1, 0, 0));
				contexto.MoveTo (0, 0);

				for (double x = -850; x < 850; x += tamaño) {
					if (x % 2 == 0) {
						contexto.MoveTo (x, -298);
						contexto.LineTo (x, 298);
					} else {
						contexto.MoveTo (x, 298);
						contexto.LineTo (x, -298);
					}
				}
				for (double y = -298; y < 298; y += tamaño) {
					if (y % 2 == 0) {
						contexto.MoveTo (-850, y);
						contexto.LineTo (850, y);
					} else {
						contexto.MoveTo (850, y);
						contexto.LineTo (-850, y);
					}
				}
				contexto.Stroke ();
				if (celulas.Count > 0) {
					contexto.Translate (-650, -298);
					foreach (PointD p in celulas) {
						contexto.SetSourceColor (new Cairo.Color (0, 0.4D, 1));
						contexto.MoveTo (p);
						contexto.LineTo (new PointD (p.X + tamaño, p.Y));
						contexto.LineTo (new PointD (p.X + tamaño, p.Y + tamaño));
						contexto.LineTo (new PointD (p.X, p.Y + tamaño));
						contexto.LineTo (p.X, p.Y);
						contexto.Fill ();
					}
				}
			} else if (iniciado) {
				bool pasovacio = false; // Para registrar si no hay un nuevo objeto estado disponibles
				// Borrar el area grafica 
				contexto.SetSourceColor(fondo);
				contexto.Paint ();
				contexto.Scale (escala, escala);
				contexto.Translate (-650 + desplx, -298 + desply);
			
				//Obtener el estado a dibujar
				estado aconsumir = default(estado);
				// Si hay un nuevo objeto estado disponible
				if (indiceconsumidor < indiceproductor-1) {
					aconsumir = estados [indiceconsumidor];
					//Actualizar el momento de obtencion del nuevo objeto estado
					tcalculando = DateTime.Now;
					// Poner el color de la etiqueta del contador regresivo en azul
					lbRegresivo.ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
					// Si el tiempo de calculo es mayor que el periodo fijado,poner el ciclo
					// en automatico
					if (aconsumir.tiempocalculo > (int)sbPeriodo.Value) {
						cbManual.Active = false;
						if (!Anglo)
							lbTituloCiclos.Text = "Periodo en milisegundos   auto";
						else
							lbTituloCiclos.Text = "Period in milliseconds   auto";
						sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (100, 100, 100));
						sbPeriodo.Value = (int)((double)aconsumir.tiempocalculo) ;
						ciclo = (int)sbPeriodo.Value;
						cicloautomatico = true;
						// Si el periodo fijado es mayor que el tiempo de calculo.
					} else if (aconsumir.tiempocalculo <= (int)sbPeriodo.Value) { 
						if (cbManual.Active) {// Si la opcion de ciclo manual esta activada
							if (!Anglo)
								lbTituloCiclos.Text = "Periodo en milisegundos   manual";
							else
								lbTituloCiclos.Text = "Period in milliseconds   manual";
							ciclo = (int)sbPeriodo.Value;
							sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (0, 240, 0));
						} else {// Si la opcion de ciclo manual esta desactivada.
							if (!Anglo)
								lbTituloCiclos.Text = "Periodo en milisegundos  auto";
							else
								lbTituloCiclos.Text = "Period in milliseconds   auto";
							ciclo = (int)(((double)aconsumir.tiempocalculo));
							sbPeriodo.Value = (double)aconsumir.tiempocalculo;
							sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (100, 100, 100));
						}
					}
				} else { // Si no hay un nuevo objeto estado disponible
						pasovacio = true;
						aconsumir = estados [indiceconsumidor];
						TimeSpan elapsed1 = DateTime.Now - tcalculando;
						int tiempo1 = (int)elapsed1.TotalMilliseconds;
						int restante = (int)Math.Ceiling ((double)(tiempo1) / 1000);
					if(!Anglo)
						lbRegresivo.Text = "Calculando: " + restante.ToString () + " s ( " + (restante / 60).ToString () + " m )";
					else
						lbRegresivo.Text = "Calculating: " + restante.ToString () + " s ( " + (restante / 60).ToString () + " m )";
					lbRegresivo.ModifyFg (StateType.Normal, new Gdk.Color (250, 0, 0));
					}

				// Actualizar las etiquetas de ciclos y poblacion
				if (!Anglo) {
					lbCiclos.Text = "Ciclos: " + ciclos.ToString ();
					lbPoblacion.Text = "Poblaciòn: " + aconsumir.celulasvivas.Count.ToString ();
				} else {
					lbCiclos.Text = "Cycles: " + ciclos.ToString ();
					lbPoblacion.Text = "Population: " + aconsumir.celulasvivas.Count.ToString ();
				}

				//Dibujar las celulas vivas
				foreach (PointD p in aconsumir.celulasvivas) {
					contexto.SetSourceColor (interior);
					contexto.MoveTo (p);
					contexto.LineTo (new PointD (p.X + tamaño, p.Y));
					contexto.LineTo (new PointD (p.X + tamaño, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y));

					contexto.Fill ();

					contexto.SetSourceColor (exterior);
					contexto.LineWidth = 1.5;
					contexto.MoveTo (p);
					contexto.LineTo (new PointD (p.X + tamaño, p.Y));
					contexto.LineTo (new PointD (p.X + tamaño, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y + tamaño));
					contexto.LineTo (p.X, p.Y);
					contexto.Stroke ();
				}

				//Pintar las celulas muertas
				foreach (PointD p in aconsumir.proximasmuertas) {
					contexto.SetSourceColor (interiorM);
					contexto.MoveTo (p);
					contexto.LineTo (new PointD (p.X + tamaño, p.Y));
					contexto.LineTo (new PointD (p.X + tamaño, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y));
					contexto.Fill ();
				}
				//Pintar las celulas nacidas
				foreach (PointD p in aconsumir.proximasvivas) {
					contexto.SetSourceColor (interiorV);
					contexto.MoveTo (p);
					contexto.LineTo (new PointD (p.X + tamaño, p.Y));
					contexto.LineTo (new PointD (p.X + tamaño, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y));
					contexto.Fill ();
					contexto.SetSourceColor (exterior);
					contexto.MoveTo (p);
					contexto.LineTo (new PointD (p.X + tamaño, p.Y));
					contexto.LineTo (new PointD (p.X + tamaño, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y + tamaño));
					contexto.LineTo (new PointD (p.X, p.Y));
					contexto.Stroke ();
				}
					
				//Comprobar si a pasado el tiempo establecido para los ciclos
				tfinal = DateTime.Now;
				TimeSpan elapsed = tfinal - tinicio;
				int tiempo = (int)elapsed.TotalMilliseconds;

				//Mostrar el tiempo restante en el contador regresivo si procede
				if (!pasovacio) {
					if (ciclo < 1000)
						lbRegresivo.Visible = false;
					else if ((ciclo >= 1000) && (ciclo < 60000)) {
						lbRegresivo.Visible = true;
						int segundos = (int)Math.Ceiling ((double)(ciclo - tiempo) / 1000);
						if (!Anglo)
							lbRegresivo.Text = "Proximo ciclo: (segundos): " + segundos.ToString ();
						else
							lbRegresivo.Text = "Next cycle: (seconds): " + segundos.ToString ();
					} else {
						lbRegresivo.Visible = true;
						double minutos = Math.Round ((((double)ciclo - (double)tiempo) / 60000D), 2);
						if (!Anglo)
							lbRegresivo.Text = "Proximo ciclo: (minutos): " + minutos.ToString ();
						else
							lbRegresivo.Text = "Next cycle: (minutes): " + minutos.ToString ();
					}

					// Comprobar si el organismo se ha estabilizado y si es asi, notificarlo y parar los ciclos
					if (indiceconsumidor > 2 && estados [indiceconsumidor - 1].celulasvivas.Count == estados [indiceconsumidor + 1].celulasvivas.Count &&
					   estados [indiceconsumidor - 1].proximasvivas.Count == estados [indiceconsumidor + 1].proximasvivas.Count &&
					   estados [indiceconsumidor - 1].proximasmuertas.Count == estados [indiceconsumidor + 1].proximasmuertas.Count) {

						if (!Anglo) {
							MessageDialog mensaje2 = new MessageDialog (ventana, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organismo estabilizado. ", new object [0]);
							mensaje2.Show ();
						}
						else {
							MessageDialog mensaje2 = new MessageDialog (ventana, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organism stabilized. ", new object [0]);
							mensaje2.Show ();
						}
						finalizado = true;
					}
					// Si no quedan celulas vivas notificarlo y parar
					if (aconsumir.celulasvivas.Count == 0) {
						if (!Anglo) {
							MessageDialog mensaje2 = new MessageDialog (ventana, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organismo muerto. ", new object [0]);
							mensaje2.Show ();
						}
						else  {
							MessageDialog mensaje2 = new MessageDialog (ventana, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Organism died. ", new object [0]);
							mensaje2.Show ();
						}

						finalizado = true;
					}
				}

				// Si ha pasado el tiempo establecido para un nuevo ciclo
				if (tiempo >= ciclo) {
					tinicio = DateTime.Now;
					// Incrementar la cantidad de ciclos y los ciclos de la sesion
					if (!pasovacio) {
						// Incrementar el indice del hilo consumidor y los ciclos
						indiceconsumidor++;
						ciclos++;
						//Si se ha activado la opcion de guardado automatico de imagenes
						if (guardadoautomatico)
							Guardarimagen_Activated (new object (), new EventArgs ());
					}
				}

				//Forzar un nuevo redibujado
				if (!finalizado && !pausa)
					PulsarTimer ();
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
		internal override void Abrir_Onactivated (object sender,EventArgs e)
		{
			//Activar la pausa para parar los ciclos
			if(!pausa)
			pausa = true;
		    // Parar el hilo productor y dar tiempo para que pare
			hiloproductor.Abort ();
			Thread.Sleep (200);
			// Ocultar los controles correspondientes
			pnZoom.Visible = false;
			pnDesplazamiento.Visible = false;
			btAceptar.Visible = false;
			sbPeriodo.Visible = false;
			areagrafica.Visible = false;
			vbMetadatos.Visible = false;
			vbCrono.Visible = false;
			// Resetear los contadores del hilo productor y consumidor
			indiceproductor = 0;
			indiceconsumidor = 0;
			// Resetear las lista de celulas vivas y la matriz de objetos estado
			celulas.Clear ();
			estados.Initialize ();
			// Visualizar el boton para dibujar una nueva forma
			alrb.Visible = true;
			// Habilitar el manejador para dibujar celulas
			areagrafica.ButtonPressEvent += Areagrafica_ButtonPressEvent;
			// Crear de nuevo el hilo productor
			hiloproductor = new Thread (ActualizarOrganismo);
			// Inicializar los marcadores de estado del objeto
			iniciado = false;
			finalizado = false;
			pausa = false;
			cargado = false;
			cicloautomatico = false;
			// Adecuar el boton Aceptar y el spinbutton de los ciclos
			if (!Anglo)
				btAceptar.Label = "Aceptar";
			else
				btAceptar.Label = "Accept";
			btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (0, 240, 30));
			if (!Anglo)
				lbTituloCiclos.Text = "Periodo en milisegundos   manual";
			else
				lbTituloCiclos.Text = "Period in milliseconds   manual";
			sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (0, 240, 0));
			hbContinuar.HeightRequest -= 10;
			// Inicializar zoom y el desplazamiento del area grafica
			escala = 1;
			desplx = 0;
			desply = 0;
			// Inicializar los ciclos
			ciclo = 100;
			ciclos = 0;
			sbPeriodo.Value = ciclo;
		}

		/// <summary>
		/// 
		/// MANEJADOR DEL EVENTO DE CERRAR EL PROGRAMA
		/// 
		/// </summary>
		/// <param name="o">O.</param>
		/// <param name="args">Arguments.</param>
		/// 
		public override void Ventana_DeleteEvent (object o, DeleteEventArgs args)
		{
			hiloproductor.Abort ();
			Thread.Sleep (200);
			System.Environment.Exit (0);
			Application.Quit ();
		}


		/// <summary>
		/// 
		/// ACTUALIZA LAS CELULAS VIVAS Y MUERTAS CADA CICLO Y FINAIZA EN CASO DE QUE EL
		/// ORGANISMO MUERA O SE ESTABILICE
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// 
		//
		private void ActualizarOrganismo()
		{
			while (!finalizado) {
				// Parar el hiloproductor antes de que se complete la matriz de objetos estado
				if (indiceproductor == estados.Length - 5)
					hiloproductor.Abort ();
				//Obtener la hora de inicio para obtener el tiempo de calculo al final
				DateTime iniciocalculo = DateTime.Now;
				//Añadir a la lista de celulas vivas las nuevas celulas vivas y eliminar las muertas
				foreach (PointD p in estadoanterior.proximasvivas)
					if (!estadoanterior.celulasvivas.Contains (p))
						estadoanterior.celulasvivas.Add (new PointD (p.X, p.Y));
				foreach (PointD p in estadoanterior.proximasmuertas) {
					bool quedanrepetidas = true;
					while (quedanrepetidas)
						quedanrepetidas = estadoanterior.celulasvivas.Remove (p);
				}
				// Calcular un nuevo estado
				EvaluarVida ();

				// Asignar el nuevo estado al siguiente elemento vacio de la matriz estados
				estados [indiceproductor + 1] = new estado (new List< PointD> (), new List<PointD> (), new List <PointD> (), 0);
				foreach (PointD p in estadoanterior.celulasvivas)
					estados [indiceproductor + 1].celulasvivas.Add (new PointD (p.X, p.Y));
				foreach (PointD p in estadoanterior.proximasvivas)
					estados [indiceproductor + 1].proximasvivas.Add (new PointD (p.X, p.Y));
				foreach (PointD p in estadoanterior.proximasmuertas)
					estados [indiceproductor + 1].proximasmuertas.Add (new PointD (p.X, p.Y));
				// Obtener el tiempo que se invertido en calcular el nuevo estado
				DateTime finalcalculo = DateTime.Now;
				tiempocalculo = (int)(finalcalculo - iniciocalculo).TotalMilliseconds;
				// Asignar el tiempo de calculo del nuevo estado al estado anterior sumandole un 25%
				if (indiceproductor > 0)
					estados [indiceproductor - 1].tiempocalculo = tiempocalculo+ (int)((double)tiempocalculo * 0.25D);
				//Incrementar el indice de objetos estado disponibles
				indiceproductor++;
				//Eliminar el ultimo objeto estado utilizado para ahorrar memoria
				if (indiceconsumidor > 1)
					estados [indiceconsumidor - 2] = default(estado);
			}
		}

		/// <summary>
		/// 
		///    MODIFICA LA LISTA DE CELULAS VIVAS APLICANDO LAS REGLAS
		///    POR DEFECTO: 
		///    CELULA MUERTA VIVE EN SIGUIENTE TURNO SI TOCA CON TRES CELULAS VIVAS
		///    CELULA VIVA CONTINUA VIVA SI TOCA CON DOS O TRES CELULAS VIVAS, EN OTRO CASO MUERE
		///    
		/// </summary>
		/// 
		//[MethodImpl(MethodImplOptions.Synchronized)]
		private void EvaluarVida()
		{
			// Crear diccionario de tipo punto / repeticiones para almacenar las veces
			// que se repiten los puntos alrededor de cada celula viva
			Dictionary<PointD,int> puntosrep = new Dictionary<PointD, int> ();
			//Si los puntos correspondientes a las celulas adyacentes a cada celula viva
			//ya esta en el diccionario, incrementar las repeticiones, si no lo esta, añadirlo.
			foreach (PointD p in estadoanterior.celulasvivas) {
				PointD punto1 = new PointD (p.X - tamaño, p.Y - tamaño);
				if (puntosrep.ContainsKey (punto1))
					puntosrep [punto1]++;
				else
					puntosrep.Add (punto1, 1);
				PointD punto2 = new PointD (p.X - tamaño, p.Y);
				if (puntosrep.ContainsKey (punto2))
					puntosrep [punto2]++;
				else
					puntosrep.Add (punto2, 1);
				PointD punto3 = new PointD (p.X - tamaño, p.Y + tamaño);
				if (puntosrep.ContainsKey (punto3))
					puntosrep [punto3]++;
				else
					puntosrep.Add (punto3, 1);
				PointD punto4 = new PointD (p.X, p.Y + tamaño);
				if (puntosrep.ContainsKey (punto4))
					puntosrep [punto4]++;
				else
					puntosrep.Add (punto4, 1);
				PointD punto5 = new PointD (p.X + tamaño, p.Y + tamaño);
				if (puntosrep.ContainsKey (punto5))
					puntosrep [punto5]++;
				else
					puntosrep.Add (punto5, 1);
				PointD punto6 = new PointD (p.X + tamaño, p.Y);
				if (puntosrep.ContainsKey (punto6))
					puntosrep [punto6]++;
				else
					puntosrep.Add (punto6, 1);
				PointD punto7 = new PointD (p.X + tamaño, p.Y - tamaño);
				if (puntosrep.ContainsKey (punto7))
					puntosrep [punto7]++;
				else
					puntosrep.Add (punto7, 1);
				PointD punto8 = new PointD (p.X, p.Y - tamaño);
				if (puntosrep.ContainsKey (punto8))
					puntosrep [punto8]++;
				else
					puntosrep.Add (punto8, 1);
			}
				
			//Resetear la lista de proximas celulas vivas y muertas
			estadoanterior.proximasvivas.Clear ();
			estadoanterior.proximasmuertas.Clear ();

			// Contar las repeticiones de las celulas alrededor de las celulas vivas
			foreach (KeyValuePair <PointD,int> k in puntosrep) {
				if (nacera.Contains (k.Value) && !estadoanterior.celulasvivas.Contains (k.Key) && !estadoanterior.proximasvivas.Contains (k.Key))
					estadoanterior.proximasvivas.Add (new PointD (k.Key.X, k.Key.Y));
				else if (estadoanterior.celulasvivas.Contains (k.Key) && !continuaviva.Contains (k.Value))
					estadoanterior.proximasmuertas.Add (new PointD (k.Key.X, k.Key.Y));
			}

			// Eliminar las celulas que esten aisladas
			foreach (PointD p in estadoanterior.celulasvivas) {
				bool aislada = true;

				if (estadoanterior.celulasvivas.Contains (new PointD (p.X - tamaño, p.Y - tamaño)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X - tamaño, p.Y)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X - tamaño, p.Y + tamaño)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X, p.Y + tamaño)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X + tamaño, p.Y + tamaño)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X + tamaño, p.Y)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X + tamaño, p.Y - tamaño)))
					aislada = false;
				else if (aislada && estadoanterior.celulasvivas.Contains (new PointD (p.X, p.Y - tamaño)))
					aislada = false;

				if (aislada && !continuaviva.Contains (0))
					estadoanterior.proximasmuertas.Add (new PointD (p.X, p.Y));
			}
				
			//Añadir a la lista de celulas vivas las nuevas celulas vivas y eliminar las muertas

			foreach (PointD p in estadoanterior.proximasvivas)
				if (!estadoanterior.celulasvivas.Contains (p))
					estadoanterior.celulasvivas.Add (new PointD (p.X, p.Y));

			foreach (PointD p in estadoanterior.proximasmuertas) {
				bool quedanrepetidas = true;
				while (quedanrepetidas)
					quedanrepetidas = estadoanterior.celulasvivas.Remove (p);
			}
		}

		//////////////// MANEJADORES DEL MENU Y DE LOS CONTROLES ////////////////////////

		/// <summary>
		/// 
		/// MUESTRA LA ETIQUETA DEL EGO CUANDO SE PASA EL MOUSE POR ENCIMA
		/// 
		/// </summary>
		/// <param name="o">O.</param>
		/// <param name="args">Arguments.</param>
		/// 
		public override void LbEgo_QueryTooltip (object o, QueryTooltipArgs args)
		{
			lbEgo.Text ="Xadnem";
		}

		/// <summary>
		/// 
		/// PONE EL ORGANISMO EN PAUSA
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void BtPausa_Clicked(object sender,EventArgs e)
		{
			if (!pausa) {
				btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (240, 20, 0));
				if (!Anglo)
					btAceptar.Label = "En pausa";
				else
					btAceptar.Label = "Paused";
				pausa = true;
			} else {
				btAceptar.ModifyBg (StateType.Normal, new Gdk.Color (10, 240, 100));
				if (!Anglo)
					btAceptar.Label = "Activo";
				else
					btAceptar.Label = "Active";
				pausa = false;
			    PulsarTimer ();
			}
		}

		/// <summary>
		/// 
		/// MANEJADORES PARA LOS BOTONES DE DESPLAZAMIENTO Y ZOOM
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public override void BtZoomMas_Clicked(object sender, EventArgs e)
		{
			escala *= 1.25;
			areagrafica.Hide ();
			areagrafica.Show ();
		}

		public override void BtZoomMenos_Clicked(object sender, EventArgs e)
		{
			escala *= (1/1.25);
			areagrafica.Hide ();
			areagrafica.Show ();
		}

		internal override void BtIzquierda_Clicked(object sender,EventArgs e)
		{
			desplx -= 60 / escala;
			areagrafica.Hide ();
			areagrafica.Show ();
		}

		internal override void BtDerecha_Clicked(object sender, EventArgs e)
		{
			desplx += 60 / escala;
			areagrafica.Hide ();
			areagrafica.Show ();
		}

		internal override void BtArriba_Clicked(object sender, EventArgs e)
		{
			desply -= 60 /  escala;
			areagrafica.Hide ();
			areagrafica.Show ();
		}
		internal override void BtAbajo_Clicked(object sender, EventArgs e)
		{
			desply += 60 / escala;
			areagrafica.Hide ();
			areagrafica.Show ();
		}
			

		/// <summary>
		/// 
		/// GUARDAR UNA FORMA EN DISCO
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void Guardar_Activated (object sender, EventArgs e)
		{
			//Mostrar el dialogo para salvar
			if (!Anglo)
				dialogosalvar = new FileChooserDialog ("Salvar forma a disco", ventana, FileChooserAction.Save, "Cancelar", ResponseType.Cancel, "Aceptar", ResponseType.Accept);
			else
				dialogosalvar = new FileChooserDialog ("Save shape to disc", ventana, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Accept", ResponseType.Accept);
			//dialogosalvar.AddButton ("Salvar", ResponseType.Accept);
			dialogosalvar.Visible = true;
			FileFilter filtro = new FileFilter ();
			filtro.AddPattern ("*.fm");
			dialogosalvar.Filter = filtro;
			if (dialogosalvar.Run () == (int)ResponseType.Accept) {
				rutasalvado = dialogosalvar.Filename;
				Btaceptarfichero_Clicked ();
			} else
				dialogosalvar.Hide ();
		}

		// Metodo auxiliar del metodo anterior
		private void Btaceptarfichero_Clicked ()
		{
			if ( !rutasalvado.EndsWith(".fm"))
				rutasalvado += ".fm";
			FileStream fs = new FileStream (rutasalvado, FileMode.Create, FileAccess.Write);
			BinaryWriter bw = new BinaryWriter (fs);
			List < PointD > aguardar = new List < PointD> ();
			if (!iniciado)
				aguardar = celulas;
			else
				aguardar = estados [indiceconsumidor].celulasvivas;
			try {
				// Guardar el tiempo de calculo
				bw.Write(tiempocalculo);
				//Guardar la escala
				bw.Write(escala);
				//Guardar el ciclo
				bw.Write(sbPeriodo.Text);
				// Guardar los ciclos
				  bw.Write(ciclos);
				//Guardar las reglas
				string digitos ="";
				foreach(int i in nacera)
					digitos += i.ToString();
				bw.Write(Int32.Parse(digitos));
				digitos ="";
				foreach(int i in continuaviva)
					digitos += i.ToString();
				bw.Write(Int32.Parse(digitos));
				// Guardar los colores
				bw.Write(interior.R);
				bw.Write(interior.G);
				bw.Write(interior.B);
				bw.Write(exterior.R);
				bw.Write(exterior.G);
				bw.Write(exterior.B);
				bw.Write(fondo.R);
				bw.Write(fondo.G);
				bw.Write(fondo.B);
				bw.Write(interiorM.R);
				bw.Write(interiorM.G);
				bw.Write(interiorM.B);
				bw.Write(interiorV.R);
				bw.Write(interiorV.G);
				bw.Write(interiorV.B);
				// Guardar los puntos
				foreach (PointD p in aguardar) {
					double x = p.X;
					double y = p.Y;
					bw.Write (x);
					bw.Write (y);
				}
				//Cerrar el stream
				bw.Close ();
				fs.Close ();
			} catch (IOException error) {
				MessageDialog mensaje3 = new MessageDialog (ventana, new DialogFlags (), MessageType.Info, ButtonsType.Ok, "Error de escritura.  \n" + error.Message, new object [0]);
				mensaje3.Show ();
			} finally {
				bw.Close ();
				fs.Close (); 
			}
			dialogosalvar.Hide ();
		}

		/// <summary>
		/// 
		/// CARGA UNA FORMA GUARDADA EN DISCO PREVIAMENTE
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void Cargar_Activated (object sender, EventArgs e)
		{ 
			Abrir_Onactivated (new object (), new EventArgs ());
			Gtk.FileChooserDialog dialogocargar;
			if(!Anglo)
			dialogocargar = new FileChooserDialog ("Cargar forma guardada", ventana, FileChooserAction.Open, "Cargar", ResponseType.Accept, "Cancelar", ResponseType.Cancel);
			else
			 dialogocargar = new FileChooserDialog("Load saved shape",ventana,FileChooserAction.Open,"Load",ResponseType.Accept,"Cancel",ResponseType.Cancel);
			dialogocargar.Visible = true;
			FileFilter filtro = new FileFilter ();
			dialogocargar.AddFilter (filtro);
			filtro.AddPattern ("*.fm");
			if (dialogocargar.Run () == (int)ResponseType.Accept) {
				string forma = dialogocargar.Filename;
				FileStream fs = new FileStream (forma, FileMode.Open, FileAccess.Read);
				BinaryReader br = new BinaryReader (fs);
				try {
					// Cargar el tiempo de calculo
					tiempocalculo = br.ReadInt32 ();
					//Cargar la escala
					escala = br.ReadDouble ();
					//Cargar el ciclo
					ciclo = Int32.Parse(br.ReadString ());
					sbPeriodo.Text = ciclo.ToString();
					iniciado = false;
					//Cargar los ciclos
					ciclos = br.ReadInt32 ();
					// Cargar las reglas
					nacera.Clear ();
					continuaviva.Clear ();
					string naceras = (br.ReadInt32 ()).ToString ();
					foreach (char c in naceras)
						nacera.Add (Int32.Parse (c.ToString ()));
					string continuavivas = (br.ReadInt32 ()).ToString ();
					foreach (char c in continuavivas)
						continuaviva.Add (Int32.Parse (c.ToString ()));
					// Mostrar las reglas
					ventana.Title = "OrgaSnimo0            Twister edition              ";
					foreach (int i in continuaviva)
						ventana.Title += i.ToString ();
					ventana.Title += " / ";
					foreach (int i in nacera)
						ventana.Title += i.ToString ();
					// Cargar los colores
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
					fondo = new Cairo.Color (fr, fg, fb);
					double imr = br.ReadDouble ();
					double img = br.ReadDouble ();
					double imb = br.ReadDouble ();
					interiorM = new Cairo.Color (imr, img, imb);
					double ivr = br.ReadDouble ();
					double ivg = br.ReadDouble ();
					double ivb = br.ReadDouble ();
					interiorV = new Cairo.Color (ivr, ivg, ivb);
					while (true) {
						//Cargar los puntos
						double x = br.ReadDouble ();
						double y = br.ReadDouble ();
						celulas.Add (new PointD (x, y));
					}
				} catch (EndOfStreamException) {
					// Cerrar el stream
					br.Close ();
					fs.Close ();
				} finally {
					fs.Close ();
					br.Close ();
				}
				dialogocargar.Hide ();
				btAceptar.Visible = true;
				cargado = true;
				DibujarFormaInicial (new object (), new EventArgs ());

			} else
				dialogocargar.Hide();
		}
					
		/// <summary>
		/// 
		/// GUARDA LA IMAGEN DE LA FORMA ACTUAL DEL ORGASNISMO EN FORMATO PNG EN EL
		/// DIRECTORIO DONDE ESTÁ EL EJECUTABLE DEL PROGRAMA
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void Guardarimagen_Activated (object sender, EventArgs e)
		{
			Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable ((Drawable)areagrafica.GdkWindow, areagrafica.Colormap, 0, 0, 0, 0, 1300, 552);
			DateTime ahora = DateTime.Now;
			string directorio = AppDomain.CurrentDomain.BaseDirectory;
			string fichero = "";
			if(!Anglo)
			fichero = directorio + "/Imagenes/imagorg" + lbPoblacion.Text + " Ciclos: " + ciclos.ToString () + ".png";
			else
				fichero = directorio + "/Imagenes/imagorg" + lbPoblacion.Text + " Cycles: " + ciclos.ToString () + ".png";	
			if (!File.Exists(fichero))
			pixbuf.Save (fichero, "png");
		}	

		/// <summary>
		/// 
		/// MUESTRA EL DIALOGO PARA CAMBIAR LAS REGLAS DEL JUEGO Y MUESTRA EJEMPLOS
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void Cambiarreglas_Activated (object sender, EventArgs e)
		{
			Gtk.Window dialogo;
			if (!Anglo)
				dialogo = new Gtk.Window ("Cambiar reglas    ( por defecto 23/3 )");
			else
				dialogo = new Gtk.Window ("Change rules    ( by defect 23/3 )");
			dialogo.SetSizeRequest(700,385);
			dialogo.Visible = true;
			dialogo.ModifyBg (StateType.Normal, new Gdk.Color (10, 200, 50));

			VBox vbReglas = new VBox (false, 0);
			vbReglas.Visible = true;

			HBox hbVivas = new HBox (false, 0);
			hbVivas.Visible = true;

			Gtk.Alignment allbVivas = new Gtk.Alignment (0, 0.5F, 0, 0);
			allbVivas.Visible = true;
			Label lbVivas;
			if (!Anglo)
				lbVivas = new Label ("Cantidad de celulas adyacentes para que una célula permanezca viva: \n( Enteros positivos entre 1 y 8 sin espacios )");
			else
				lbVivas = new Label ("Number of adjacent cells for a cell to remain alive: \n( Positive integers between 1 & 8 without spaces )"); 
			lbVivas.Visible = true;
			lbVivas.ModifyFont(Pango.FontDescription.FromString("Dejavu-Sans OBLIQUE 11"));
			allbVivas.Add (lbVivas);
			hbVivas.Add (allbVivas);

			Gtk.Alignment altbVivas = new Gtk.Alignment (0, 0.5F, 0, 0);
			altbVivas.Visible = true;
			Entry tbVivas = new Entry ();
			tbVivas.Visible = true;
			tbVivas.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans NORMAL 13"));
			tbVivas.SetSizeRequest (100, 35);
			tbVivas.KeyPressEvent += CajasReglas_KeyPress;
			tbVivas.FocusOutEvent += TbVivas_FocusOutEvent;
			tbVivas.Activated += TbVivas_Activated;
			foreach (int i in continuaviva)
				tbVivas.Text += i.ToString ();
			altbVivas.Add (tbVivas);
			hbVivas.Add (altbVivas);

			HBox hbNacidas = new HBox ();
			hbNacidas.Visible = true;

			Gtk.Alignment allbNacidas = new Gtk.Alignment (0, 0.5F, 0, 0);
			allbNacidas.Visible = true;
			Label lbNacidas;
			if (!Anglo)
				lbNacidas = new Label ("Cantidad de células adyacentes para que una célula nazca: \n( Enteros positivos entre 0 y 8 sin espacios )");
			else
				lbNacidas = new Label ("Number of adjacent cells for a cell to born: \n( Positive integers between 0 & 8 without spaces )");
			lbNacidas.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans OBLIQUE 11"));
			lbNacidas.Visible = true;
			allbNacidas.WidthRequest = allbVivas.WidthRequest;
			allbNacidas.Add (lbNacidas);
			hbNacidas.Add (allbNacidas);

			Gtk.Alignment altbNacidas = new Gtk.Alignment (0, 0.5F, 0, 0);
			altbNacidas.Visible = true;
			altbNacidas.LeftPadding = 80;
			Entry tbNacidas = new Entry ();
			tbNacidas.Visible = true;
			tbNacidas.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans NORMAL 13"));
			tbNacidas.WidthRequest= tbVivas.WidthRequest;
			foreach (int i in nacera)
				tbNacidas.Text += i.ToString ();
			tbNacidas.KeyPressEvent += CajasReglas_KeyPress;
			tbNacidas.FocusOutEvent += TbNacidas_FocusOutEvent;
			tbNacidas.Activated += TbNacidas_Activated;
			altbNacidas.Add (tbNacidas);
			hbNacidas.Add (altbNacidas);

			Gtk.Alignment allbEjemplos = new Gtk.Alignment (0, 1F, 0, 0);
			allbEjemplos.Visible = true;
			Label lbEjemplos;
			if (!Anglo) {
				lbEjemplos = new Label ("Ejemplos:\n\n01234567 / 3  ---> Crecimiento moderado\n" +
				"23 / 36 ------------> Hight life\n" +
				"1357 / 1357 -----> Replicantes, crecimiento rápido\n" +
				"235678 / 3678 --> Rombos, crecimiento rápido\n" +
				"34 / 34 ------------> Estable\n" +
				"4 / 2 ---------------> Crecimiento moderado\n" +
				"51 / 346 ----------> Vida media\n");
			} else {
				lbEjemplos = new Label ("Examples:\n\n01234567 / 3  ---> Moderate growth\n" +
				"23 / 36 ------------> Hight life\n" +
				"1357 / 1357 -----> Replicants, quick growth\n" +
				"235678 / 3678 --> Diamonds, quick growth\n" +
				"34 / 34 ------------> Stable\n" +
				"4 / 2 ---------------> Moderate growth\n" +
				"51 / 346 ----------> Average life time\n");
			}
			lbEjemplos.ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans OBLIQUE BOLD 17"));
			lbEjemplos.Visible = true;
			allbEjemplos.Add (lbEjemplos);

			vbReglas.PackEnd(allbEjemplos);
			vbReglas.Add (hbVivas);
			vbReglas.Add (hbNacidas);
			dialogo.Add (vbReglas);
		}
		//METODOS AUXILIARES AL METODO ANTERIOR
		void TbNacidas_Activated (object sender, EventArgs e)
		{
			nacera.Clear ();
			Entry caja = (Entry)sender;
			foreach (char c in caja.Text)
				nacera.Add (Int32.Parse (c.ToString ()));
			ventana.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in continuaviva)
				ventana.Title += i.ToString ();
			ventana.Title += " / ";
			foreach (int i in nacera)
				ventana.Title += i.ToString ();
		}

		void TbNacidas_FocusOutEvent (object o, FocusOutEventArgs args)
		{
			nacera.Clear ();
			Entry caja = (Entry)o;
			foreach (char c in caja.Text)
				nacera.Add (Int32.Parse (c.ToString ()));

			ventana.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in continuaviva)
				ventana.Title += i.ToString ();
			ventana.Title += " / ";
			foreach (int i in nacera)
				ventana.Title += i.ToString ();
		}

		void TbVivas_Activated (object sender, EventArgs e)
		{
			continuaviva.Clear ();
			Entry caja = (Entry)sender;
			foreach (char c in caja.Text)
				continuaviva.Add (Int32.Parse (c.ToString ()));

			ventana.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in continuaviva)
				ventana.Title += i.ToString ();
			ventana.Title += " / ";
			foreach (int i in nacera)
				ventana.Title += i.ToString ();
		}

		void TbVivas_FocusOutEvent (object o, FocusOutEventArgs args)
		{
			continuaviva.Clear ();
			Entry caja = (Entry)o;
			foreach (char c in caja.Text)
				continuaviva.Add (Int32.Parse (c.ToString ()));

			ventana.Title = "OrgaSnimo0            Twister edition              ";
			foreach (int i in continuaviva)
				ventana.Title += i.ToString ();
			ventana.Title += " / ";
			foreach (int i in nacera)
				ventana.Title += i.ToString ();
		}
		// Controla que solo se introduzcan digitos en las cajas de texto de las reglas
		[GLib.ConnectBefore()]
		private void CajasReglas_KeyPress(object sender,KeyPressEventArgs e)
		{
			if ( e.Event.Key == Gdk.Key.Key_9 ||  e.Event.Key != Gdk.Key.BackSpace && !char.IsDigit ((char)e.Event.KeyValue)) {
			{
					e.RetVal = true;
				}
			}
		}
			
		/// <summary>
		/// 
		/// RESTABLECE LAS REGLAS POR DEFECTO
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void Volveravalorespordefecto_Activated (object sender, EventArgs e)
		{
			continuaviva.Clear ();
			continuaviva.Add (2);
			continuaviva.Add (3);
			nacera.Clear ();
			nacera.Add (3);

		    exterior = new Cairo.Color (1, 0, 0); //Color de la linea alrrededor de la celula viva por defecto
			interior = new Cairo.Color(0.65D, 1, 0.1D); // Color interior de la celula viva por defecto
		    fondo = new Cairo.Color(0,0,0); // Color negro del fondo por defecto
			interiorM = new Cairo.Color(0,0,0); // Color de las celulas muertas en el turno por defecto.
			interiorV =new Cairo.Color(0.65D,1,0.1D); // Color de las nuevas celulas en el turno por defecto
			ventana.Title = "OrgaSnimo0            Twister edition              ";
		}

		// Atributos para los dialogos de cambio de color
		ColorSelectionDialog dialogol;
		ColorSelectionDialog dialogov;
		ColorSelectionDialog dialogon;
		ColorSelectionDialog dialogom;
		ColorSelectionDialog dialogof;
		Button btLineas;
		Button btCelulasVivas;
		Button btCelulasNacidas;
		Button btCelulasMuertas;
		Button btFondo;


		/// <summary>
		/// 
		/// MUESTRA EL CUADRO DE DIALOGO PARA CAMBIAR LOS COLORES
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		internal override void Cambiarcolores_Activated (object sender, EventArgs e)
		{
			Gtk.Window dialogo;
			if (!Anglo)
				dialogo = new Gtk.Window ("Cambiar colores.");
			else
				dialogo = new Gtk.Window ("Change colors.");
			dialogo.Visible = true;
			dialogo.SetSizeRequest(400,250);

			VBox vbEtiquetas = new VBox (true, 0);
			vbEtiquetas.Visible = true;

			Gtk.Alignment albtLineas = new Gtk.Alignment (0, 0, 0, 0);
			albtLineas.Visible = true;
			if (!Anglo)
				btLineas = new Button ("Color de la linea.");
			else
				btLineas = new Button ("Line color.");
			btLineas.Children[0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btLineas.ModifyBg (StateType.Normal, CairoColorToGdk (exterior));
			btLineas.Visible = true;
			btLineas.Clicked += BtLineas_Clicked;
			albtLineas.Add (btLineas);

 			Gtk.Alignment albtCelulasVivas = new Gtk.Alignment (0, 0, 0, 0);
			albtCelulasVivas.Visible = true;
			if (!Anglo)
				btCelulasVivas = new Button ("Color de las celulas vivas");
			else
				btCelulasVivas = new Button ("Alive cells color.");
			btCelulasVivas.Children[0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			Gdk.Color traducido = CairoColorToGdk(interior);
			btCelulasVivas.ModifyBg (StateType.Normal, traducido);
			btCelulasVivas.Visible = true;
			btCelulasVivas.Clicked += BtVivas_Clicked;
			albtCelulasVivas.Add (btCelulasVivas);

			Gtk.Alignment albtCelulasNacidas = new Gtk.Alignment (0, 0, 0, 0);
			albtCelulasNacidas.Visible = true;
			if (!Anglo)
				btCelulasNacidas = new Button ("Color de las nuevas células en el turno");
			else
				btCelulasNacidas = new Button ("New cells color");
			btCelulasNacidas.Children[0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btCelulasNacidas.ModifyBg (StateType.Normal,CairoColorToGdk(interiorV));
			btCelulasNacidas.Visible = true;
			btCelulasNacidas.Clicked += BtCelulasNacidas_Clicked;
			albtCelulasNacidas.Add (btCelulasNacidas);

			Gtk.Alignment albtCelulasMuertas = new Gtk.Alignment (0, 0, 0, 0);
			albtCelulasMuertas.Visible = true;
			if (!Anglo)
				btCelulasMuertas = new Button ("Color de las células muertas en el turno.");
			else
				btCelulasMuertas = new Button ("Dead cells color.");
			btCelulasMuertas.Children[0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btCelulasMuertas.ModifyBg (StateType.Normal,CairoColorToGdk(interiorM));
			btCelulasMuertas.Visible = true;
			btCelulasMuertas.Clicked += BtCelulasMuertas_Clicked;
			albtCelulasMuertas.Add (btCelulasMuertas);

			Gtk.Alignment albtFondo = new Gtk.Alignment (0, 0, 0, 0);
			albtFondo.Visible = true;
			if (!Anglo)
				btFondo = new Button ("Color del fondo.");
			else
				btFondo = new Button ("Background color.");
			btFondo.Children[0].ModifyFont (Pango.FontDescription.FromString ("Dejavu-Sans BOLD 13"));
			btFondo.ModifyBg (StateType.Normal,CairoColorToGdk(fondo));
			btFondo.Visible = true;
			btFondo.Clicked += BtFondo_Clicked;
			albtFondo.Add (btFondo);

			vbEtiquetas.Add (albtLineas);
			vbEtiquetas.Add (albtCelulasVivas);
			vbEtiquetas.Add (albtCelulasNacidas);
			vbEtiquetas.Add (albtCelulasMuertas);
			vbEtiquetas.Add (albtFondo);
			dialogo.Add (vbEtiquetas);
		}
		// Metodos auxiliares del metodo anterior
		void BtFondo_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogof = new ColorSelectionDialog ("Color del fondo.");
			else
				dialogof = new ColorSelectionDialog ("Background color.");
			dialogof.Show ();
			dialogof.OkButton.Clicked += AceptarColorFondo;
			dialogof.CancelButton.Clicked += Dialogof_CancelButton_Clicked;
		}

		void Dialogof_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogof.Hide ();
		}

		void AceptarColorFondo(object sender,EventArgs e)
		{
			Gdk.Color elegido = dialogof.ColorSelection.CurrentColor;
			btFondo.ModifyBg (StateType.Normal, elegido);
			fondo = GdkToCairoColor (elegido);
			dialogof.Hide ();
		}


		void BtCelulasMuertas_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogom = new ColorSelectionDialog ("Color de las células muertas");
			else
				dialogom = new ColorSelectionDialog ("Dead cells color.");
			dialogom.Show ();
			dialogom.OkButton.Clicked += AceptarColorMuertas;
			dialogom.CancelButton.Clicked += Dialogom_CancelButton_Clicked;
		}

		void Dialogom_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogom.Hide ();
		}

		void AceptarColorMuertas(object sender,EventArgs e)
		{
			Gdk.Color elegido = dialogom.ColorSelection.CurrentColor;
			btCelulasMuertas.ModifyBg (StateType.Normal, elegido);
			interiorM = GdkToCairoColor (elegido);
			dialogom.Hide ();
		}


		void BtCelulasNacidas_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogon = new ColorSelectionDialog ("Color de las nuevas células");
			else
				dialogon = new ColorSelectionDialog ("New cells color.");
			dialogon.Show ();
			dialogon.OkButton.Clicked += AceptarColorNacidas;
			dialogon.CancelButton.Clicked += Dialogon_CancelButton_Clicked;
		}

		void Dialogon_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogon.Hide ();
		}

		void AceptarColorNacidas(object sender,EventArgs e)
		{
			Gdk.Color elegido = dialogon.ColorSelection.CurrentColor;
			btCelulasNacidas.ModifyBg (StateType.Normal, elegido);
			interiorV = GdkToCairoColor (elegido);
			dialogon.Hide ();
		}

		void BtLineas_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogol = new ColorSelectionDialog ("Color de la linea");
			else
				dialogol = new ColorSelectionDialog ("Line color.");
			dialogol.Show ();
			dialogol.OkButton.Clicked += AceptarColorLinea;
			dialogol.CancelButton.Clicked += Dialogol_CancelButton_Clicked;
		}

		void Dialogol_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogol.Hide ();
		}

		void AceptarColorLinea(object sender,EventArgs e)
		{
			Gdk.Color elegido = dialogol.ColorSelection.CurrentColor;
			btLineas.ModifyBg (StateType.Normal, elegido);
			exterior = GdkToCairoColor (elegido);
			dialogol.Hide ();
		}

		void BtVivas_Clicked (object sender, EventArgs e)
		{
			if (!Anglo)
				dialogov = new ColorSelectionDialog ("Color interior de las células vivas");
			else
				dialogov = new ColorSelectionDialog ("Inner color of alive cells.");
			dialogov.Show ();
			dialogov.OkButton.Clicked += AceptarColorVivas;
			dialogov.CancelButton.Clicked += Dialogov_CancelButton_Clicked;
		}

		void Dialogov_CancelButton_Clicked (object sender, EventArgs e)
		{
			dialogov.Hide ();
		}

		void AceptarColorVivas(object sender,EventArgs e)
		{
			Gdk.Color elegido = dialogov.ColorSelection.CurrentColor;
			btCelulasVivas.ModifyBg (StateType.Normal, elegido);
			interior = GdkToCairoColor(elegido);
			dialogov.Hide ();
		}

		/// <summary>
		/// 
		/// METODO PARA CONVERTIR UN GDK COLOR EN CAIRO COLOR 
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
		/// METODO PARA CONVERTIR DE UN CAIRO COLOR A GDK COLOR
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


		/// <summary>
		/// 
		/// MANEJADOR DE LA OPCION DEL MENU PARA EL GUARDADO AUTOMATICO DE IMAGENES
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public override void Guardadoautomaticoimagenes_Activated ( object sender, EventArgs e)
		{
			if (guardadoautomatico) {
				guardadoautomatico = false;
				guardadoautomaticoimagenes.Deselect ();
			} else {
				guardadoautomatico = true;
				guardadoautomaticoimagenes.Select ();
			}
		}


		public override void SbPeriodo_ValueChanged (object sender, EventArgs e)
		{
			ciclo = (int)sbPeriodo.Value;
		}


		/// <summary>
		/// 
		/// MANEJADOR PARA PASAR DE CICLO AUTOMATICO A MANUAL 
		/// 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		/// 
		public override void CbManual_Clicked (object sender, EventArgs e)
		{
			if (cbManual.Active) {
				lbTituloCiclos.ModifyFg (StateType.Normal, new Gdk.Color (0, 250, 0));
				if (!Anglo)
					lbTituloCiclos.Text = "Periodo en milisegundos  manual.";
				else
					lbTituloCiclos.Text = "Period in milliseconds   manual.";
				sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (0, 250, 0));
			} else {
				lbTituloCiclos.ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
				if (!Anglo)
					lbTituloCiclos.Text = "Periodo en milisegundos  auto";
				else
					lbTituloCiclos.Text = "Period in milliseconds   auto";
				sbPeriodo.ModifyBase (StateType.Normal, new Gdk.Color (100, 100, 100));
			}
		}


		public override void English_Activated(object sender,EventArgs e)
		{
			menu.Remove (archivo);
			archivo.Dispose ();
			archivo = new MenuItem ("Archive");
			archivo.Visible = true;
			menu.Append (archivo);

			archivomenu = new Menu ();
			archivo.Submenu = archivomenu;

			nuevo.Dispose ();
			archivomenu.Remove (nuevo);
			nuevo = new MenuItem ("New");
			nuevo.Visible = true;
			nuevo.Activated += Abrir_Onactivated;
			archivomenu.Append (nuevo);

			guardar.Dispose ();
			guardar = new MenuItem ("Save");
			guardar.Visible = true;
			guardar.Activated += Guardar_Activated;
			archivomenu.Append (guardar);

			cargar.Dispose ();
			cargar = new MenuItem ("Load");
			cargar.Visible = true;
			cargar.Activated += Cargar_Activated;
			archivomenu.Append (cargar);

			guardarimagen.Dispose ();
			guardarimagen = new MenuItem ("Save image");
			guardarimagen.Visible = true;
			guardarimagen.Activated += Guardarimagen_Activated;
			archivomenu.Append (guardarimagen);

			menu.Remove (opciones);
			opciones.Dispose ();
			opciones = new MenuItem ("Options");
			opciones.Visible = true;
			menu.Append (opciones);

			opcionesmenu = new Menu ();
			opciones.Submenu = opcionesmenu;

			cambiarreglas.Dispose ();
			cambiarreglas = new MenuItem ("Change rules");
			cambiarreglas.Visible = true;
			cambiarreglas.Activated += Cambiarreglas_Activated;
			opcionesmenu.Append (cambiarreglas);

			cambiarcolores.Dispose ();
			cambiarcolores = new MenuItem ("Change colors");
			cambiarcolores.Visible = true;
			cambiarcolores.Activated += Cambiarcolores_Activated;
			opcionesmenu.Append (cambiarcolores);


			volveravalorespordefecto.Dispose ();
			volveravalorespordefecto = new MenuItem ("Back to values by defect");
			volveravalorespordefecto.Visible = true;
			volveravalorespordefecto.Activated += Volveravalorespordefecto_Activated;
			opcionesmenu.Append (volveravalorespordefecto);

			guardadoautomaticoimagenes.Dispose ();
			guardadoautomaticoimagenes = new MenuItem ("Autosave images");
			guardadoautomaticoimagenes.Visible = true;
			guardadoautomaticoimagenes.Activated += Guardadoautomaticoimagenes_Activated;
			opcionesmenu.Append (guardadoautomaticoimagenes);

			idioma.Dispose ();
			idioma = new MenuItem ("Language");
			idioma.Visible = true;
			opcionesmenu.Append (idioma);

			Menu idiomas = new Menu ();
			idioma.Submenu = idiomas;

			Español.Dispose ();
			Español = new MenuItem ("Spanish");
			Español.Visible = true;
			Español.Activated += Español_Activated;
			idiomas.Append (Español);

			English.Dispose ();
			English = new MenuItem ("English");
			English.Visible = true;
			English.Activated += English_Activated;
			idiomas.Append (English);

			menu.Remove (ayuda);
			ayuda.Dispose ();
			ayuda = new MenuItem ("Help");
			ayuda.Activated += Ayuda_Activated;
			ayuda.Visible = true;
			menu.Append (ayuda);

			Anglo = true;

			btDibujarForma.Label = "Draw initial shape";
			lbTituloCiclos.Text = "Period in milliseconds   manual";
			cbManual.Label = "Manual cycle";
			cbManual.Children[0].ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
			cbManual.Children [0].ModifyFg (StateType.Active, new Gdk.Color (0, 250, 0));
			lbRegresivo.Text = "Next cycle: ";

			if (!iniciado)
				btAceptar.Label = "Accept";
			else
				btAceptar.Label = "Pause";

			lbPoblacion.Text = "Population: ";
			lbCiclos.Text = "Cycles: ";

		}

		public override void Español_Activated (object sender, EventArgs e)
		{
			menu.Remove (archivo);
			archivo.Dispose ();
			archivo = new MenuItem ("Archivo");
			archivo.Visible = true;
			menu.Append (archivo);

			archivomenu = new Menu ();
			archivo.Submenu = archivomenu;

			nuevo.Dispose ();
			archivomenu.Remove (nuevo);
			nuevo = new MenuItem ("Nuevo");
			nuevo.Visible = true;
			nuevo.Activated += Abrir_Onactivated;
			archivomenu.Append (nuevo);

			guardar.Dispose ();
			guardar = new MenuItem ("Guardar");
			guardar.Visible = true;
			guardar.Activated += Guardar_Activated;
			archivomenu.Append (guardar);

			cargar.Dispose ();
			cargar = new MenuItem ("Cargar");
			cargar.Visible = true;
			cargar.Activated += Cargar_Activated;
			archivomenu.Append (cargar);

			guardarimagen.Dispose ();
			guardarimagen = new MenuItem ("Guardar imagen");
			guardarimagen.Visible = true;
			guardarimagen.Activated += Guardarimagen_Activated;
			archivomenu.Append (guardarimagen);

			menu.Remove (opciones);
			opciones.Dispose ();
			opciones = new MenuItem ("Opciones");
			opciones.Visible = true;
			menu.Append (opciones);

			opcionesmenu = new Menu ();
			opciones.Submenu = opcionesmenu;

			cambiarreglas.Dispose ();
			cambiarreglas = new MenuItem ("Cambiar reglas");
			cambiarreglas.Visible = true;
			cambiarreglas.Activated += Cambiarreglas_Activated;
			opcionesmenu.Append (cambiarreglas);

			volveravalorespordefecto.Dispose ();
			volveravalorespordefecto = new MenuItem ("Volver a valores por defecto");
			volveravalorespordefecto.Visible = true;
			volveravalorespordefecto.Activated += Volveravalorespordefecto_Activated;
			opcionesmenu.Append (volveravalorespordefecto);

			cambiarcolores.Dispose ();
			cambiarcolores = new MenuItem ("Cambiar colores");
			cambiarcolores.Visible = true;
			cambiarcolores.Activated += Cambiarcolores_Activated;
			opcionesmenu.Append (cambiarcolores);

			guardadoautomaticoimagenes.Dispose ();
			guardadoautomaticoimagenes = new MenuItem ("Guardado automático de imágenes");
			guardadoautomaticoimagenes.Visible = true;
			guardadoautomaticoimagenes.Activated += Guardadoautomaticoimagenes_Activated;
			opcionesmenu.Append (guardadoautomaticoimagenes);

			idioma.Dispose ();
			idioma = new MenuItem ("Idioma");
			idioma.Visible = true;
			opcionesmenu.Append (idioma);

			Menu idiomas = new Menu ();
			idioma.Submenu = idiomas;

			Español.Dispose ();
			Español = new MenuItem ("Español");
			Español.Visible = true;
			Español.Activated += Español_Activated;
			idiomas.Append (Español);

			English.Dispose ();
			English = new MenuItem ("English");
			English.Visible = true;
			English.Activated += English_Activated;
			idiomas.Append (English);

			menu.Remove (ayuda);
			ayuda.Dispose ();
			ayuda = new MenuItem ("Ayuda");
			ayuda.Activated += Ayuda_Activated;
			ayuda.Visible = true;
			menu.Append (ayuda);

			Anglo = false;

			btDibujarForma.Label = "Dibujar forma inicial";
			lbTituloCiclos.Text = "Periodo en milisegundos    manual";
			cbManual.Label = "Ciclo manual";
			cbManual.Children[0].ModifyFg (StateType.Normal, new Gdk.Color (0, 100, 250));
			cbManual.Children [0].ModifyFg (StateType.Active, new Gdk.Color (0, 250, 0));
			lbRegresivo.Text = "Próximo ciclo: ";

			if (!iniciado)
				btAceptar.Label = "Aceptar";
			else
				btAceptar.Label = "Pausa";

			lbPoblacion.Text = "Población: ";
			lbCiclos.Text = "Ciclos: ";
		}


		public override void Ayuda_Activated (object sender, EventArgs e)
		{
			Gtk.Window ventanaayuda = new Gtk.Window ("");
			ventanaayuda.Parent = ventana;
			ventanaayuda.SetSizeRequest (1050, 510);
			ventanaayuda.Visible = true;
			ventanaayuda.ShowAll ();

			ScrolledWindow swventanaayuda = new ScrolledWindow ();
			swventanaayuda.ModifyBase (StateType.Normal, new Gdk.Color (0, 160, 50));
			swventanaayuda.SetSizeRequest (1000, 510);
			swventanaayuda.Visible = true;
			ventanaayuda.Add (swventanaayuda);

			TextView tvTexto = new TextView ();
			tvTexto.Visible = true;
			TextBuffer buffer = new TextBuffer (new TextTagTable ());
			tvTexto.Buffer = buffer;
			tvTexto.ModifyBase (StateType.Normal, new Gdk.Color (0, 160, 50));
			swventanaayuda.Add(tvTexto);

			TextTag subrayado = new TextTag ("sub");
			subrayado.Font = "Dejavu-Sans ITALIC BOLD 17";
			subrayado.Underline = Pango.Underline.Single;
			subrayado.ForegroundGdk = new Gdk.Color (0, 0, 0);
			buffer.TagTable.Add (subrayado);

			TextTag normal = new TextTag ("normal");
			normal.Font = "Dejavu-Sans NORMAL 13";
			buffer.TagTable.Add (normal);

		

			if (!Anglo) {
				buffer.Text = "Funcionamiento básico:\n\n";
				int sub1 = buffer.Text.Length;

				buffer.Text += "OrgaSnismo0, está basado en el juego de la vida, diseñado por el matématico John Conway. Puede encontrarse\n extensa información en Wikipedia.\n";
				buffer.Text += "\nCuando se inicia el programa, se muestra la barra de menú, cuyas opciones se comentan más adelante, y el botón\n'Dibujar forma incicial'.\nPulsando el botón, aparecen los siguientes elementos:\n\n " +
				"\tRejilla: Al pulsar sobre un cuadrado, se crea una nueva célula, la cual se muestra en color amarillo.\n\t            Para borrar una célula, basta con volver a pulsar sobre ella.\n\n" +
				"\tCheck button 'ciclo manual': Chequeado por defecto, sirve para habilitar el establecimiento del tiempo de ciclo\n\t\t\t\t\t\t\t por parte del usuario.\n\t\t\t\t\t\t\t Cuando no está chequeado, tanto el check button como el spin button cambian de\n\t\t\t\t\t\t\t color, y el tiempo de ciclo se determina dinámicamente de forma automática en\n\t\t\t\t\t\t\t función del tiempo que la CPU necesite para calcular la siguiente forma." +
				"\n\n\tSpin button:  Muestra el tiempo de ciclo establecido. Por defecto, está fijado en 100 milisegundos.\n\t\t\t\tEste tiempo se puede cambiar si el check button 'ciclo manual' está chequeado.\n\t\t\t\tSi el tiempo que la CPU necesita para calcular la siguiente forma, es mayor que el tiempo de ciclo\n\t\t\t\testablecido, el programa cambia a ciclo automatico." +
				"\n\n\tBotón 'Aceptar': Para comenzar los ciclos una vez que se ha dibujado la forma deseada y ajustado el tiempo de \n\t\t\t\t     ciclo en su caso." +
				"\n\nUna vez iniciado el juego, la forma dibujada irá cambiando en cada ciclo en base a las reglas.\nPor defecto, las reglas son las establecidas por Conway, es decir:\nUna célula se mantiene viva si toca con dos células vivas, en otro caso muere en el siguiente ciclo.\nUna célula nace, si toca con tres células vivas." +
				"\n\nEn la esquina inferior izquierda, se muestra la población ( cantidad de células ) y la cantidad de ciclos transcurrida." +
				"\nTambién aparecen los botones: zoom , desplazamiento y pausa.\n\nSi la forma se estabiliza o desaparece, se muestra un cuadro de dialogo y el juego finaliza.";
				int norm= buffer.Text.Length;
				buffer.Text +="\n\nBarra de menú";
				int sub2 = buffer.Text.Length;
				buffer.Text += "\n\nArchivo:" +
				"\n\n\tNuevo: Inicia un nuevo juego." +
				"\n\n\tGuardar: Guarda la forma, reglas y colores actuales en un fichero *.fm.\n\t\t\tSe puede guardar una forma tanto si se han iniciado los ciclos como si no. Si se han iniciado los ciclos, se \n\t\t\trecomienda realizar el proceso de guardado con el juego en pausa." +
				"\n\n\tCargar: Carga una forma, con las reglas y colores guardados anteriormente en un fichero *.fm" +
				"\n\n\tGuardar imagen: Guarda una imagen *.png correspondiente a la forma en pantalla.\n\t\t\t\t      El fichero de imagen, se guarda en un directorio llamado 'imagenes' que se crea, si no existe, en \n\t\t\t\t      el directorio donde está el ejecutable de la aplicación." +
				"\n\nOpciones:" +
				"\n\n\tCambiar reglas: Aparece una ventana para cambiar las reglas. Si se inicia un nuevo juego, las reglas se mantienen.\n\t\t\t\t    En la parte inferior de la ventana, se muestran algunos ejemplos y los efectos que producen." +
				"\n\n\tCambiar colores: Se muestra una ventana con 5 botones correspondientes a los colores de:\n\t\t\t\t      La línea perimetral de las células ( de color rojo por defecto ),\n\t\t\t\t      el interior de las células vivas ( amarillo por defecto ),\n\t\t\t\t      el interior de las células nacidas en el turno ( amarillo por defecto ) \n\t\t\t\t      el interior de las células muertas en el turno ( color negro por defecto ) y \n\t\t\t\t      el fondo ( negro por defecto )." +
				"\n\n\tVolver a valores por defecto: Vuelve a las reglas y colores por defecto." +
				"\n\n\tGuardado automático de imágenes: Guarda la imagen *.png de la nueva forma cada ciclo.\n\t\t\t\t\t\t\t\t      Las imagenes se guardan en una carpeta llamada 'imagenes', que se crea si no\n\t\t\t\t\t\t\t\t      existe, en el directorio donde esté el ejecutable de la aplicación.\n\t\t\t\t\t\t\t\t      El nombre de cada imagen contiene la cantidad de ciclos y población." +
				"\n\n\tIdioma: Se puede elegir entre Ingles y Castellano."; 

				int norm2 = buffer.Text.Length;
				buffer.ApplyTag (subrayado, buffer.StartIter, buffer.GetIterAtOffset (sub1));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub1), buffer.GetIterAtOffset (norm));
				buffer.ApplyTag (subrayado, buffer.GetIterAtOffset (norm), buffer.GetIterAtOffset (sub2));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub2), buffer.GetIterAtOffset (norm2));
			}
			else  {
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

				int norm= buffer.Text.Length;
				buffer.Text +="\n\nMenu bar";
				int sub2 = buffer.Text.Length;
				buffer.Text += "\n\nFile:" +
					"\n\n\tNew: To start a new game." +
					"\n\n\tSave: Save the current shape, rules and colors in a *.fm file\n\t\t   One can save a shape, either that the cycles have started or not. If the cycles are running, it is convenient \n\t\t   to do the file saving process with the game paused." +
					"\n\n\tLoad: Load a shape with the rules and colour previously saved in a *.fm file." +
					"\n\n\tSave image: Save a *.png image of the current screen.\n\t\t\t      The png file, is saved in a directory called 'imagenes' which is created , if it doesn't exist, into \n\t\t\t      the directory where the application executable is." +
					"\n\nOptions:" +
					"\n\n\tChange rules: A window appears an allow to change the rules. The rules will remain if a new game is started.\n\t\t\t\t At the change rules window bottom, there are some rules examples with a brief description of the\n\t\t\t\t respective effects." +
					"\n\n\tChange colors: The 5 buttons shown, control respectively the following colours:\n\t\t\t\t  The perimeter cell line ( red by defect ),\n\t\t\t\t  the alive cell interior ( yellow by defect ),\n\t\t\t\t  the new cells born interior ( yellow by defect ) \n\t\t\t\t  the died cells interior ( black by defect ) and \n\t\t\t\t  the background ( black by defect )." +
					"\n\n\tBack to values by defect: Return to the rules and colours by defect ." +
					"\n\n\tAutosave images: Save a *.png image of the new shape each cycle.\n\t\t\t\t       The png images are saved at the directory called 'imagenes', which is created if it doesn't \n\t\t\t\t       exist, into the directory where the application executable is.\n\t\t\t\t       The file name of each png saved image, contains the population and cycles information." +
					"\n\n\tLanguage: Spanish and English languages are available."; 

				int norm2 = buffer.Text.Length;
				buffer.ApplyTag (subrayado, buffer.StartIter, buffer.GetIterAtOffset (sub1));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub1), buffer.GetIterAtOffset (norm));
				buffer.ApplyTag (subrayado, buffer.GetIterAtOffset (norm), buffer.GetIterAtOffset (sub2));
				buffer.ApplyTag (normal, buffer.GetIterAtOffset (sub2), buffer.GetIterAtOffset (norm2));
			}

		}

	}
}

