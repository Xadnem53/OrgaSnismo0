using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
using Gtk;
using Gdk;
using Pango;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
//using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;

//!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!!!!!!!!!!!!!!!!!!!!!!!!!
//
//  INICIADO 3/2/2017 para monodevelop
//
//!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!!!!!!!!!!!!!!!!!!!!!!!!!


namespace Organismo0
{
    public class Form1 
    {
		/*
        List<PointF> celulas; // Para guardar la lista de celulas vivas;
        List<PointF> celulasant;// Para guardar la lista anterior de celulas vivas
        List<PointF> proximasvivas; // Proximas celulas que naceran
        List<PointF> proximasmuertas; // Proximas celulas que moriran
        List<PointF> muertasanteriores; // Celulas que han muerto en el turno anterior
        List<PointF> vivasanteriores; // Celulas que han nacido en el turno anterior
        List<PointF> adyacentes; // Lista de puntos que estan alrededor de cada celula viva
            List<int> continuaviva; // Lista de la cantidad de celulas adyacentes para que una celula siga viva segun las reglas 
            List<int> nacera; // Lista de la cantidad de celulas adyacentes para que una celula nazca segun las reglas


        Bitmap bitmap; //  Para el gráfico permanente.
        Graphics grafico; // Objeto Graphics para dibujar en el picture box

        Pen linea; // Para la linea exterior de las celulas
        Pen lineamuerta; // Para la linea exterior de las celulas murtas

        float tamaño = 30; //Tamaño de cada célula por defecto

        int ciclo = 1000; // Tiempo entre ciclos por defecto
        int ciclos = 0; // Cantidad de ciclos desde el inicio de la forma.
        int ciclossesion = 0; // Cantidad de cilos desde el inicio de la sesion ( para diferenciar sobre los ciclos desde el inicio de la forma )

        string viva = "23"; // Valores por defecto para que una celula siga viva
        string nacida = "3"; // Valor por defecto para que una celula nazca
        string prefijo =""; // Prefijo introducido por el usuario para los nombres de las imagenes en guardado automatico

        PointF origen; // Origen de la ventana grafica

        TextBox tbTamaño; // Textbox para cuadro de dialogo del tamaño de la celula

        Form dialogotamaño; // Dialogo para el tamaño de la celula
        Form dialogoreglas; // Dialogo para el cambio de normas
        Form DialogoColores;
        TextBox tbNacida; // Caja de texto con la cantidad de celulas adyacentes para que una celula nazca
        TextBox tbViva; //Caja de texto con la cantidad de celulas adyacentes para que una celula permanezca viva
        Label lbColorLinea; // Para el dialogo de colores
        Label lbColorInterior; // Idem anterior
        Label lbColorFondo; // Idem
        Label lbColorInteriorM; // Interior de las celulas muertas en el turno
        Label lbColorInteriorV; // Interior de las nuevas celulas en el turno

        bool dibujando = false; // Para registrar cuando se esta en el modo de dibujo        
        bool enmarcha = false; // Para registrar cuando esta el timer en marcha
        bool disponible = false; // Para la sincronia entre hilos
        bool cicloautomatico = false; // Para registrar cuando el programa cambia el ciclo por problemas de rendimiento
        bool beep = false; // Para registrar cuando esta habilitado el beep y cuando no
        bool guardadoautomatico = false; // Para registrar cuando esta activado el guardado automatico de imagenes y cuando no

        int cuentapartidas = 0; // Para llevar la cuenta de las partidas jugadas;

        Color exterior = Color.Red; //Color de la linea alrrededor de la celula viva por defecto
        Color interior = Color.FromArgb(155, 250, 5); // Color interior de la celula viva por defecto
        Color fondo = Color.Black; // Color del fondo por defecto
        Color interiorM = Color.Black; // Color de las celulas muertas en el turno por defecto.
        Color interiorV = Color.FromArgb(155, 250, 5); // Color de las nuevas celulas en el turno por defecto

      //  List<List<PointF>> buffer; // Para guardar las listas de puntos calculados por el metodo evaluar vida
        Thread calcular; // Hilo que obtendra la lista de celulas en cada iteracion
        
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = ciclo;
            celulas = new List<PointF>();
            celulasant = new List<PointF>();
            proximasvivas = new List<PointF>();
            proximasmuertas = new List<PointF>();
            muertasanteriores = new List<PointF>();
            adyacentes = new List<PointF>();
            vivasanteriores = new List<PointF>();
            continuaviva = new List<int>(){2,3};
            nacera = new List<int>(){3};
            nudVelocidad.Value = ciclo;
            SoundPlayer reproductor = new SoundPlayer();
            reproductor.SoundLocation = Directory.GetCurrentDirectory() + "\\Rafaga-Organismo-Entrada.wav";
            reproductor.Play();
            lbEgo.ForeColor = fondo;
            lbEgo.BackColor = fondo;
            beepToolStripMenuItem.PerformClick();
            linea = new Pen(exterior, 1);
            lineamuerta = new Pen(fondo, 1);
        }
   
        /// <summary>
        /// 
        /// MODIFICA LA POSICION DEL LOS BOTONES Y EL TAMAÑO DEL PICTURE BOX EN FUNCION DEL TAMAÑO DEL FORMULARIO
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void Maximized(object sender, EventArgs e)
        {
            pbFondo.Size = this.ClientSize;
            btArriba.Location = new Point(this.ClientSize.Width- 100, this.ClientSize.Height - 200);
            btDerecha.Location = new Point(this.ClientSize.Width - 50, this.ClientSize.Height - 150);
            btAbajo.Location = new Point(this.ClientSize.Width - 100, this.ClientSize.Height - 100);
            btIzquierda.Location = new Point(this.ClientSize.Width - 150, this.ClientSize.Height - 150);

            btZoomMenos.Location = new Point(this.btIzquierda.Location.X, this.ClientSize.Height - 50);
            btZoomMas.Location = new Point(this.btDerecha.Location.X, btZoomMenos.Location.Y);

            nudVelocidad.Location = new Point(20, this.ClientSize.Height - 30);
            lbRotulonud.Location = new Point(nudVelocidad.Location.X, nudVelocidad.Location.Y - lbRotulonud.Height - 2);
            lbEgo.Location = new Point(this.ClientSize.Width - lbEgo.Width - 5, lbEgo.Height + 5);
        }

		/// <summary>
        /// 
        /// MODIFICA LA POSICION DEL LOS BOTONES Y EL TAMAÑO DEL PICTURE BOX EN FUNCION DEL TAMAÑO DEL FORMULARIO
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void Maximized(object sender, EventArgs e)
        {
            pbFondo.Size = this.ClientSize;
            btArriba.Location = new Point(this.ClientSize.Width- 100, this.ClientSize.Height - 200);
            btDerecha.Location = new Point(this.ClientSize.Width - 50, this.ClientSize.Height - 150);
            btAbajo.Location = new Point(this.ClientSize.Width - 100, this.ClientSize.Height - 100);
            btIzquierda.Location = new Point(this.ClientSize.Width - 150, this.ClientSize.Height - 150);

            btZoomMenos.Location = new Point(this.btIzquierda.Location.X, this.ClientSize.Height - 50);
            btZoomMas.Location = new Point(this.btDerecha.Location.X, btZoomMenos.Location.Y);

            nudVelocidad.Location = new Point(20, this.ClientSize.Height - 30);
            lbRotulonud.Location = new Point(nudVelocidad.Location.X, nudVelocidad.Location.Y - lbRotulonud.Height - 2);
            lbEgo.Location = new Point(this.ClientSize.Width - lbEgo.Width - 5, lbEgo.Height + 5);
        }

        /// <summary>
        /// 
        ///  OPCION PARA DIBUJAR LA FORMA INICIAL
        /// 
        /// </summary>
        ///

        private void rbDibujar_Click(object sender, EventArgs e)
        {
            rbDibujar.Hide();
            pbFondo.Update();
            tamañoToolStripMenuItem_Click(new object(), new EventArgs());
            OcultarControles();
            nudVelocidad.Visible = true; ;
            lbRotulonud.Visible = true;
            cambiarPeriodoToolStripMenuItem.Checked = true;
            dibujando = true;
        }



        ///////////////////////////
        ///
        /// MUESTRA EL CUADRO DE DIALOGO PARA INTRODUCIR EL TAMAÑO DE LAS CELULAS
        ///
        ///////////////////////////////////////////////
        ///
        private void tamañoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogotamaño = new Form();
            dialogotamaño.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialogotamaño.Size = new Size(190, 130);
            dialogotamaño.BackColor = Color.SeaGreen;
            dialogotamaño.ControlBox = false;
            dialogotamaño.TopMost = true;
            Label lbTamaño = new Label();
            lbTamaño.AutoSize = true;
            lbTamaño.Font = new Font("Dejavu Sans", 10, FontStyle.Underline);
            lbTamaño.Text = "Tamaño de la célula.";
            lbTamaño.Location = new Point(20, 20);
            lbTamaño.Visible = true;
            tbTamaño = new TextBox();
            tbTamaño.Size = new Size(100, 50);
            tbTamaño.Location = new Point(40, 50);
            tbTamaño.Visible = true;
            tbTamaño.Text = "30";
            tbTamaño.SelectAll();
            tbTamaño.TextAlign = HorizontalAlignment.Center;
            Button btTamaño = new Button();
            btTamaño.Text = "Aceptar";
            btTamaño.Location = new Point(55, 90);
            btTamaño.Visible = true;
            btTamaño.BackColor = Color.Chartreuse;
            btTamaño.Click += btTamaño_Clicked;
            dialogotamaño.Controls.Add(tbTamaño);
            dialogotamaño.Controls.Add(lbTamaño);
            dialogotamaño.Controls.Add(btTamaño);
            dialogotamaño.StartPosition = FormStartPosition.Manual;
            dialogotamaño.Location = this.Location;
            dialogotamaño.AcceptButton = btTamaño;
            dialogotamaño.Show();
        }

        /// <summary>
        /// 
        /// MANEJADORES PARA LOS BOTONES DE OPCION
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        private void btTamaño_Clicked(object sender, EventArgs e)
        {
            string tamaños = tbTamaño.Text;
            if (tamaños == "")
                tamaños = "20";
            tamaño = Single.Parse(tamaños);
            dialogotamaño.Close();

            // Iniciar el bitmap asocieado al picture box
            bitmap = new Bitmap(pbFondo.Size.Width,pbFondo.Size.Height);
            //Vincular el bitmap al picture box.
            pbFondo.Image = bitmap;
            // Iniciar el objeto graphics vinculado al bitmap
          //  grafico = pbFondo.CreateGraphics();
            grafico = Graphics.FromImage(bitmap);
            grafico.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            
            Pen linea = new Pen(Color.FromArgb(50, 240, 100, 50));

            // Dibujar la cuadricula para dibujar la forma
            for (float i = 0; i < pbFondo.Width; i += tamaño)
            {
                grafico.DrawLine(linea, new PointF(i, 0), new PointF(i, pbFondo.Height));
            }
            for (float i = 0; i < pbFondo.Height; i += tamaño)
            {
                grafico.DrawLine(linea, new PointF(0, i), new PointF(pbFondo.Width, i));
            }
            pbFondo.MouseClick += DibujarForma; // Para dibujar las celulas en los puntos pulsados
            btAceptar.Location = new Point(this.ClientSize.Width - btAceptar.Size.Width - 5, this.ClientSize.Height - btAceptar.Size.Height - 5);
            btAceptar.BackColor = Color.Chartreuse;
            btAceptar.Text = "Aceptar";
            btAceptar.ForeColor = Color.Black;
            btAceptar.Show();
            btAceptar.Click -= btPausa_Click;
            btAceptar.Click += btAceptar_Click;
            nudVelocidad.Visible = true;
            lbRotulonud.Visible = true;
        }
        ///////////////////////////
        ///
        /// DIBUJA UN CELULA EN EL PUNTO DE LA CUADRICULA PULSADO CUANDO SE HA ELEGIDO LA OPCION DE
        /// DIBUJAR LA FORMA INICIAL
        ///
        /////////////////////////////////
        ///
        private void DibujarForma(object sender, MouseEventArgs e)
        {
            lbPoblacion.Show();

            PointF pulsado = e.Location;
            PointF celula = new PointF(((int)(pulsado.X / tamaño)) * tamaño, ((int)(pulsado.Y / tamaño)) * tamaño);
            if (!celulas.Contains(celula))
            {
                celulas.Add(celula);
                grafico.FillRectangle(new SolidBrush(interior), celula.X, celula.Y, tamaño, tamaño);
            }
            else
            {
                celulas.Remove(celula);
                grafico.FillRectangle(new SolidBrush(Color.Black), celula.X, celula.Y, tamaño, tamaño);
            }
            lbPoblacion.Text = "Población: " + celulas.Count.ToString();
            pbFondo.Invalidate();
        }

        ///
        /// METODOS PARA PRUEBAS
        /// 
/*
        private void IniciarOrganismo()
        {
            grafico = pbFondo.CreateGraphics();
            
            if (glider)
            {
                celulas.Add(new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2));
                celulas.Add(new PointF(celulas[0].X + 10, celulas[0].Y));
                celulas.Add(new PointF(celulas[1].X + 10, celulas[1].Y));
                celulas.Add(new PointF(celulas[2].X, celulas[2].Y - 10));
                celulas.Add(new PointF(celulas[3].X - 10, celulas[3].Y - 10));
                celulas.Add(new PointF(celulas[4].X + 10, celulas[3].Y - 10));
                lbCiclos.Visible = true;
                lbCiclos.Text = "Ciclos: " + ciclos.ToString();
                lbPoblacion.Visible = true;
                lbPoblacion.Text = "Población:" + celulas.Count.ToString();
                Pen linea = new Pen(Color.Red, 1);
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }

                timer1.Start();
                //btPruebas.Visible = true;
            
            }
            
        }
 
        private void btPruebas_Click(object sender, EventArgs e)
        {
            timer1_Tick(new object(), new EventArgs());
        }

            */
       
		/*
        ///////////////////////////
        ///
        /// LLAMA AL METODO EVALUARVIDA Y LO INICIA CON LA FORMA DIBUJADA
        ///
        ///////////////////////////////////////////////
        ///
        private void btAceptar_Click(object sender, EventArgs e)
        {
            btAceptar.BackColor = Color.Black;
            btAceptar.ForeColor = Color.Silver;
            btAceptar.Text = "Pausa";
            btAceptar.Location = new Point(this.ClientSize.Width / 2, btAceptar.Location.Y);
            btAceptar.Click -= btAceptar_Click;
            btAceptar.Click += btPausa_Click;
            pbFondo.MouseClick -= DibujarForma;
            MostrarControles();

            if (cuentapartidas == 0)
            {
                botonesZoomToolStripMenuItem.PerformClick();
                cambiarPeriodoToolStripMenuItem.PerformClick();
                cambiarPeriodoToolStripMenuItem.PerformClick();
            }
            grafico.Clear(fondo);
            lbCiclos.Visible = true;
            lbCiclos.Text = "Ciclos: " + ciclos.ToString();
            lbPoblacion.Visible = true;
            lbPoblacion.Text = "Población: " + celulas.Count.ToString();
            // Pintar las celulas 
            PintarCelulas();
            //Eliminar el manejador para dibujar las celulas en los puntos pulsados
            pbFondo.MouseClick -= DibujarForma;
            //Crear el hilo que evalua el estado de las celulas
            calcular = new Thread(EvaluarVida);
            calcular.Start();
            //Poner en marcha el timer 
            timer1.Start();
            enmarcha = true;
            cuentapartidas++;
            dibujando = false;
        }

        /// <summary>
        /// 
        /// REINICIA EL PROGRAMA
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            rbFicticio.Checked = true;
            rbDibujar.Show();
            if (grafico != null)
                grafico.Clear(Color.Black);
            if (pbFondo != null)
            {
                pbFondo.MouseClick -= DibujarForma;
                pbFondo.MouseClick -= DibujarForma;
            }
            celulas.Clear();
            celulasant.Clear();
            proximasmuertas.Clear();
            proximasvivas.Clear();
            muertasanteriores.Clear();
            vivasanteriores.Clear();
            adyacentes.Clear();

            btAceptar.Hide();
            lbCiclos.Hide();
            lbPoblacion.Hide();
            ciclos = 0;
            if (dialogotamaño != null)
                dialogotamaño.Hide();
            OcultarControles();
            cicloautomatico = false;
            guardadoautomatico = false;
            nuevoToolStripMenuItem.Checked = false;
            ciclo = 1000;
            nudVelocidad.BackColor = Color.White;
            nudVelocidad.Value = ciclo;
            pbFondo.Invalidate();
            ciclossesion = 0;
            

        }

        /// <summary>
        /// 
        /// ACTUALIZA LAS CELULAS VIVAS Y MUERTAS CADA CICLO Y PARA EN CASO DE QUE EL ORGANISMO MUERA O SE 
        /// ESTABILICE
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void timer1_Tick(object sender, EventArgs e)
        {
            while (disponible == false)
            {
                Monitor.Wait(this);
            }
            //Añadir a la lista de celulas vivas las nuevas celulas vivas y eliminar las muertas
            // MessageBox.Show("proximas vivas: " + proximasvivas.Count.ToString() + "       proximas muertas: " + proximasmuertas.Count.ToString());

            foreach (PointF p in proximasvivas)
                if (!celulas.Contains(p))
                    celulas.Add(p);

            foreach (PointF p in proximasmuertas)
            {
                bool quedanrepetidas = true;
                while (quedanrepetidas)
                    quedanrepetidas = celulas.Remove(p);
            }


            // Poner el intervalo minimo en funcion de la cantidad de celulas

            if (celulas.Count > 1000 && !cicloautomatico && ciclo < celulas.Count)
            {
                cicloautomatico = true;
                nudVelocidad.Enabled = false;
                nudVelocidad.BackColor = Color.Silver;
                nudVelocidad.Font = new Font(nudVelocidad.Font.FontFamily, nudVelocidad.Font.Size, FontStyle.Bold);
                nudVelocidad.ForeColor = Color.Coral;
            }
            if (cicloautomatico)
            {
                if(celulas.Count <= 3500)
                ciclo = celulas.Count + (((int)(celulas.Count / 1000)) * 3000);//antes 3750
                else if(celulas.Count <= 7000)
                    ciclo = celulas.Count + (((int)(celulas.Count / 1000)) * 8000);//antes 7500
                else
                    ciclo = celulas.Count + (((int)(celulas.Count / 1000)) * 20000);//antes 10000
                if (ciclo < 1000)
                    ciclo = 1000;

                nudVelocidad.Value = ciclo;
            }

            // Mover el origen del grafico al punto de origen establecido
            grafico.TranslateTransform(origen.X, origen.Y);

            lbCiclos.Text = "Ciclos: " + ciclos.ToString();
            lbPoblacion.Text = "Poblaciòn: " + celulas.Count;

            // Dibujar las celulas vivas
            PintarCelulas();

            //Guardar el bitmap si la opcion de guardado automatico esta activa
            if(guardadoautomatico)
            {
                int indice = prefijo.LastIndexOf('.');
                string nombre = prefijo.Substring(0, indice);
                nombre += "--" + ciclos.ToString() + ".bmp";
                bitmap.Save(nombre);
            }
             
            // Incrementar la cantidad de ciclos y los ciclos de la sesion
            ciclos++;
            ciclossesion++;

            /// Guardar la lista de celulas vivas antes de modificarla
            celulasant.Clear();
            for (int i = 0; i < celulas.Count; i++)
                celulasant.Add(celulas[i]);

            // Guardar la lista de celulas vivas y muertas anteriores antes de modificarla
            muertasanteriores.Clear();
            vivasanteriores.Clear();
            foreach (PointF p in proximasmuertas)
                muertasanteriores.Add(p);
            foreach (PointF p in proximasvivas)
                vivasanteriores.Add(p);


            //Resetear la lista de celulas adyacentes
            adyacentes.Clear();

            //Llenar la lista de celulas adyacentes ( se incluiran las celulas vivas tambien )
            foreach (PointF p in celulas)
            {
                adyacentes.Add(new PointF(p.X - tamaño, p.Y - tamaño));
                adyacentes.Add(new PointF(p.X - tamaño, p.Y));
                adyacentes.Add(new PointF(p.X - tamaño, p.Y + tamaño));
                adyacentes.Add(new PointF(p.X, p.Y + tamaño));
                adyacentes.Add(new PointF(p.X + tamaño, p.Y + tamaño));
                adyacentes.Add(new PointF(p.X + tamaño, p.Y));
                adyacentes.Add(new PointF(p.X + tamaño, p.Y - tamaño));
                adyacentes.Add(new PointF(p.X, p.Y - tamaño));
            }


            // Lanzar el calculo del estado de las celulas en un proceso aparte y usar un mecanismo de
            // sincronizacion con el pintado de las celulas.
            calcular = new Thread(EvaluarVida);
            disponible = false;
            Monitor.PulseAll(this);
            calcular.Start();

            /// Si no quedan celulas  notificarlo
            if (ciclossesion > 1 && celulas.Count == 0)
            {
                timer1.Stop();
                MessageBox.Show("Organismo muerto.");
                return;
            }

            // Si el organismo se ha estabilizado , notificarlo
            if (ciclossesion > 1 && proximasmuertas.Count == 0 && proximasvivas.Count == 0)
            {
                btPausa_Click(new object(), new EventArgs());
                MessageBox.Show("Organismo estabilizado.");
                return;
            }


            if(beep && celulas.Count > 1000)
             Console.Beep(400, 1000);
           
        }


        /// <summary>
        /// 
        ///    MODIFICA LA LISTA DE CELULAS VIVAS APLICANDO LAS REGLAS: 
        ///    CELULA MUERTA VIVE EN SIGUIENTE TURNO SI TOCA CON TRES CELULAS VIVAS
        ///    CELULA VIVA CONTINUA VIVA SI TOCA CON DOS O TRES CELULAS VIVAS, EN OTRO CASO MUERE
        ///    
        /// </summary>
        /// 
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void EvaluarVida()
        {
            // Esperar a que el metodo Timer1_Tick termine 
            while (disponible == true)
            {
                Monitor.Wait(this);
            }
/*
            //Resetear la lista de celulas adyacentes
            adyacentes.Clear();

            //Llenar la lista de celulas adyacentes ( se incluiran las celulas vivas tambien )
            foreach (PointF p in celulas)
            {
                adyacentes.Add(new PointF(p.X - tamaño, p.Y - tamaño));
                adyacentes.Add(new PointF(p.X - tamaño, p.Y));
                adyacentes.Add(new PointF(p.X - tamaño, p.Y + tamaño));
                adyacentes.Add(new PointF(p.X, p.Y + tamaño));
                adyacentes.Add(new PointF(p.X + tamaño, p.Y + tamaño));
                adyacentes.Add(new PointF(p.X + tamaño, p.Y));
                adyacentes.Add(new PointF(p.X + tamaño, p.Y - tamaño));
                adyacentes.Add(new PointF(p.X, p.Y - tamaño));
            }
*/
			/*
            // Añadir en una lista de puntos, los de la lista anterior que se repitan 3 veces
            // ( nuevas celulas que estaran vivas el situiente turno )
            // Y en otra lista , los puntos que se reptitan menos de dos o mas de tres veces
            // ( celulas que moriran el siguiente turno )

            //Limpiar la lista de proximas celulas vivas y muertas
            proximasvivas.Clear();
            proximasmuertas.Clear();

            int contador = 0; // Para contar la cantidad de veces que se repite la aparicion de una celula adyacente

            for (int i = 0; i < adyacentes.Count; i++)
            {
                PointF acontar = adyacentes[i];
                for (int j = 0; j < adyacentes.Count; j++)
                {
                    if (adyacentes[j] == acontar)
                    {
                        contador++;
                    }
                }

                if (nacera.Contains(contador) && !celulas.Contains(acontar) && !proximasvivas.Contains(acontar))
                {
                    proximasvivas.Add(acontar);
                }
                else if (celulas.Contains(acontar) && !continuaviva.Contains(contador))
                    proximasmuertas.Add(acontar);

                contador = 0;
            }

            // Eliminar las celulas que esten aisladas

            foreach (PointF p in celulas)
            {
                bool aislada = true;
          
               // List<PointF> entorno = new List<PointF>();
              //  entorno.Add(new PointF(p.X - tamaño, p.Y - tamaño));
              //  entorno.Add(new PointF(p.X - tamaño, p.Y));
               // entorno.Add(new PointF(p.X - tamaño, p.Y + tamaño));
              //  entorno.Add(new PointF(p.X, p.Y + tamaño));
              //  entorno.Add(new PointF(p.X + tamaño, p.Y + tamaño));
               // entorno.Add(new PointF(p.X + tamaño, p.Y));
              //  entorno.Add(new PointF(p.X + tamaño, p.Y - tamaño));
             //   entorno.Add(new PointF(p.X, p.Y - tamaño));
              //  foreach (PointF pf in entorno)
                  //  if (celulas.Contains(pf))
                      //  aislada = false;
                 
                if (celulas.Contains(new PointF(p.X - tamaño, p.Y - tamaño)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X - tamaño, p.Y)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X - tamaño, p.Y + tamaño)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X, p.Y + tamaño)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X + tamaño, p.Y + tamaño)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X + tamaño, p.Y)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X + tamaño, p.Y - tamaño)))
                    aislada = false;
                else if (aislada && celulas.Contains(new PointF(p.X, p.Y - tamaño)))
                    aislada = false;

                if (aislada && !continuaviva.Contains(0))
                    proximasmuertas.Add(p);
            }

/*
            //Añadir a la lista de celulas vivas las nuevas celulas vivas y eliminar las muertas
            // MessageBox.Show("proximas vivas: " + proximasvivas.Count.ToString() + "       proximas muertas: " + proximasmuertas.Count.ToString());

            foreach (PointF p in proximasvivas)
                if (!celulas.Contains(p))
                    celulas.Add(p);

            foreach (PointF p in proximasmuertas)
            {
                bool quedanrepetidas = true;
                while (quedanrepetidas)
                    quedanrepetidas = celulas.Remove(p);
            }
*/
			/*
            disponible = true;
            Monitor.PulseAll(this);


            //   }
        }




        ////////////// MANEJADORES /////////////

        /// <summary>
        /// 
        /// MUESTRA U OCULTA LOS BOTONES DE ZOOM Y CHEQUEA O DESCHEQUEA LA OPCION DEL MENU
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void botonesZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dibujando)
                return;
            if (!botonesZoomToolStripMenuItem.Checked)
            {
                btZoomMas.Show();
                btZoomMenos.Show();
                botonesZoomToolStripMenuItem.Checked = true;
            }
            else
            {
                btZoomMas.Hide();
                btZoomMenos.Hide();
                botonesZoomToolStripMenuItem.Checked = false;
            }
        }

        /// <summary>
        /// 
        /// ZOOM MAS
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void ampliarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            grafico.ScaleTransform(1.25F, 1.25F);   
             grafico.Clear(fondo);
            Pen linea = new Pen(Color.Red, 1);
            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch(InvalidOperationException)
            {
                return;
            }
            pbFondo.Invalidate();
        }

        /// <summary>
        /// 
        /// ZOOM MENOS
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void reducirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            grafico.ScaleTransform(0.75F, 0.75F);
            grafico.Clear(fondo);

            grafico.TranslateTransform(origen.X, origen.Y + (tamaño * 10));
            Pen linea = new Pen(Color.Red, 1);
            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            pbFondo.Invalidate();
        }

        /// <summary>
        /// 
        /// MUESTRA U OCULTA LOS BOTONES DE DESPLAZAMIENTO Y CHEQUEA O DESCHEQUEA LAS OPCIONES DEL MENU
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void botonesDeDesplazamientoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dibujando)
                return;

            if (!botonesDeDesplazamientoToolStripMenuItem.Checked)
            {
                btArriba.Show();
                btAbajo.Show();
                btDerecha.Show();
                btIzquierda.Show();
                botonesDeDesplazamientoToolStripMenuItem.Checked = true;
            }
            else
            {
                btArriba.Hide();
                btAbajo.Hide();
                btDerecha.Hide();
                btIzquierda.Hide();
                botonesDeDesplazamientoToolStripMenuItem.Checked = false;
            }
        }
        /// <summary>
        /// 
        /// DESPLAZAMIENTO A LA IZQUIERDA
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btIzquierda_Click(object sender, EventArgs e)
        {
            grafico.Clear(fondo);

            grafico.TranslateTransform(origen.X-(tamaño*5), origen.Y);
            Pen linea = new Pen(Color.Red, 1);
            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            pbFondo.Invalidate(); 
        }

        /// <summary>
        /// 
        /// DESPLAZAMIENTO ARRIBA
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btArriba_Click(object sender, EventArgs e)
        {
            grafico.Clear(fondo);

            grafico.TranslateTransform(origen.X, origen.Y - (tamaño * 5));
            Pen linea = new Pen(Color.Red, 1);
            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            pbFondo.Invalidate();
        }

        /// <summary>
        /// 
        /// DESPLAZAMIENTO A LA DERECHA
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDerecha_Click(object sender, EventArgs e)
        {
            grafico.Clear(fondo);

            grafico.TranslateTransform(origen.X + (tamaño * 5), origen.Y);
            Pen linea = new Pen(Color.Red, 1);

            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            pbFondo.Invalidate();
        }

        /// <summary>
        /// 
        /// DESPLAZAMIENTO ABAJO
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAbajo_Click(object sender, EventArgs e)
        {
            grafico.Clear(fondo);

            grafico.TranslateTransform(origen.X, origen.Y + (tamaño * 5));
            Pen linea = new Pen(Color.Red, 1);
            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
            pbFondo.Invalidate();
        }

        /// <summary>
        /// 
        /// GUARDAR LA FORMA EN SU ESTADO ACTUAL
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
        //    Estado state = new Estado(celulas, ciclos);
            string fichero;
            FileStream fs;
            BinaryWriter bw;

            SaveFileDialog dialogosalvar = new SaveFileDialog();
            dialogosalvar.Filter = "Archivos de forma (*.fm)|*.fm";

            if (dialogosalvar.ShowDialog() == DialogResult.OK)
            {
                fichero = dialogosalvar.FileName;
                fs = new FileStream(fichero, FileMode.Create, FileAccess.Write);
                bw = new BinaryWriter(fs);
            
                try
                {
                    foreach (PointF p in celulas)
                    {
                        bw.Write(p.X);
                        bw.Write(p.Y);
                    }
                  //  MessageBox.Show(ciclos.ToString());
                    string aux = ciclos.ToString();
                    float cic = Single.Parse(aux);
                    bw.Write(cic);
                }
                catch (IOException er)
                {
                    MessageBox.Show("Error de escritura." + er.Message);
                }
                finally
                {
                    fs.Close();
                    bw.Close();
                }
            }
            else
                return;
        }

        /// <summary>
        /// 
        /// CARGAR UNA FORMA PREVIAMENTE GUARDADA
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Resetear el ciclo automatico , el guardado automatico, los ciclos de la sesion ,las listas y parar el timer
            cicloautomatico = false;
            guardadoautomatico = false;
            guardadoAutomaticoDeImagenenesToolStripMenuItem.Checked = false;
            timer1.Stop();
            celulas.Clear();
            ciclossesion = 0;

            if (muertasanteriores != null)
                muertasanteriores.Clear();
            if (proximasmuertas != null)
                proximasmuertas.Clear();
            if (proximasvivas != null)
                proximasvivas.Clear();
            if (vivasanteriores != null)
                vivasanteriores.Clear();
            adyacentes.Clear();

            // Restaurar el ciclo por defecto
            ciclo = 1000;
            nudVelocidad.BackColor = Color.White;
            nudVelocidad.Value = ciclo;

            //Resetear los graficos
            bitmap = new Bitmap(pbFondo.Width, pbFondo.Height);
            grafico = Graphics.FromImage(bitmap);
            grafico.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            pbFondo.Image = bitmap;
            grafico.Clear(fondo);
            pbFondo.Invalidate();

            //Lanzar el FileDialog para abrir el fichero
            OpenFileDialog dialogoabrir = new OpenFileDialog();
            dialogoabrir.Filter = "Archivos de forma (*.fm)|*.fm";
            if (dialogoabrir.ShowDialog() == DialogResult.OK)
            {
                string fichero = dialogoabrir.FileName;
                FileStream fs = new FileStream(fichero, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                List<float> lecturas = new List<float>();
                try
                {
                    do
                    {
                        lecturas.Add(br.ReadSingle());
                    }
                    while (true);
                }
                catch (EndOfStreamException)
                {
                    fs.Close();
                    br.Close();
                }
                catch (IOException err)
                {
                    MessageBox.Show("Error de lectura." + err);
                }
                finally
                {
                    fs.Close();
                    br.Close();
                }
                for (int i = 0; i < lecturas.Count - 1; i += 2)
                    celulas.Add(new PointF(lecturas[i], lecturas[i + 1]));
                // MessageBox.Show(lecturas[lecturas.Count - 1].ToString());
                ciclos = (int)lecturas[lecturas.Count - 1];
                celulasant = new List<PointF>();
                // Ocultar y mostrar los controles adecuados
                rbDibujar.Hide();
                lbPoblacion.Show();
                lbCiclos.Show();
                //Dibujar las celulas
                lock (celulas)
                {
                    foreach (PointF p in celulas)
                    {
                        grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                        grafico.DrawRectangle(Pens.Red, p.X, p.Y, tamaño, tamaño);
                    }
                }
              
                MostrarControles();

                // Cambiar el color , texto y manejador del boton Aceptar
                if (cuentapartidas == 0)
                {
                    btAceptar.Click -= btAceptar_Click;
                    btAceptar.Click += btPausa_Click;
                }
                btAceptar.Visible = true;
                btAceptar.BackColor = Color.Black;
                btAceptar.ForeColor = Color.Silver;
                btAceptar.Text = "Pausa";
                btAceptar.Location = new Point(this.ClientSize.Width / 2, this.ClientSize.Height - btAceptar.Size.Height - 5);
                cuentapartidas++;
                if (cuentapartidas == 0)
                btAceptar.PerformClick();
 
                disponible = true;
                linea = new Pen(exterior, 1);
                lineamuerta = new Pen(fondo, 1);
                timer1_Tick(new object(), new EventArgs());
                timer1.Start();
            }
            else
                return;
          
        }

        /// <summary>
        /// 
        /// MANEJADOR QUE HACE APARECER EL NUMERIC UP & DOWN PARA CAMBIAR EL PERIODO ENTRE CICLOS
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void cambiarPeriodoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cambiarPeriodoToolStripMenuItem.Checked)
            {
                nudVelocidad.Hide();
                lbRotulonud.Hide();
                cambiarPeriodoToolStripMenuItem.Checked = false;
            }
            else
            {
                nudVelocidad.Show();
                lbRotulonud.Hide();
                cambiarPeriodoToolStripMenuItem.Checked = true;
            }
        }

        /// <summary>
        /// 
        /// CAMBIA EL VALOR DEL PERIODO SEGUN EL VALOR EN EL NUMERIC UP & DOWN
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void nudVelocidad_ValueChanged(object sender, EventArgs e)
        {
            ciclo = (int)nudVelocidad.Value;
           // timer1.Stop();
            timer1.Interval = ciclo;
           // timer1.Start();
        }

        /// <summary>
        /// 
        /// MUESTRA EL DIALOGO PARA CAMBIAR LAS REGLAS DEL JUEGO
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cambiarReglasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Creacion del formulario para el cuadro de dialogo
            dialogoreglas = new Form();
            dialogoreglas.Size = new Size(700, 300);
            dialogoreglas.Owner = this;
            dialogoreglas.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            dialogoreglas.Text = "Cambio de las reglas por defecto (23/3)";
            dialogoreglas.BackColor = Color.SeaGreen;
            dialogoreglas.Show();
            dialogoreglas.StartPosition = FormStartPosition.Manual;
            dialogoreglas.Location = new Point(this.Location.X + 10, this.Location.Y + 10);
            // Etiqueta para la cantidad de celulas adyacentes para que una celula permanezca viva
            Label lbViva = new Label();
            dialogoreglas.Controls.Add(lbViva);
            lbViva.Font = new Font("Dejavu-Sans", 12, FontStyle.Underline);
            lbViva.AutoSize = true;
            lbViva.Visible = true;
            lbViva.Location = new Point(10, 20);
            lbViva.Text = "Cantidad de células adyacentes para que una célula permanezca viva: ";
            // Caja de texto para la cantidad de celulas adyacentes para que una celula permanezca viva
            tbViva = new TextBox();
            tbViva.Name = "tbViva";
            tbViva.AutoSize = false;
            dialogoreglas.Controls.Add(tbViva);
            tbViva.Font = new Font("Dejavu-Sans", 12);
            tbViva.TextAlign = HorizontalAlignment.Center;
            tbViva.Size = new Size(100, 25);
            tbViva.Location = new Point(lbViva.Location.X + lbViva.Width + 5, lbViva.Location.Y);
            tbViva.KeyPress += CajasReglas_KeyPress;
            tbViva.Focus();
            // Etiqueta para la cantidad de celulas adyacentes para que una celula nazca
            Label lbNacida = new Label();
            dialogoreglas.Controls.Add(lbNacida);
            lbNacida.Font = lbViva.Font;
            lbNacida.AutoSize = true;
            lbNacida.Visible = true;
            lbNacida.Location = new Point(lbViva.Location.X, lbViva.Location.Y + lbViva.Height + 30);
            lbNacida.Text = "Cantidad de células adyacentes para que una célula nazca: ";
            // Caja de texto para la cantidad de celulas adyacentes para que una celula nazca
            tbNacida = new TextBox();
            tbNacida.Name = "tbNacida";
            dialogoreglas.Controls.Add(tbNacida);
            tbNacida.Font = tbViva.Font;
            tbNacida.TextAlign = HorizontalAlignment.Center;
            tbNacida.Size = tbViva.Size;
            tbNacida.Location = new Point(lbNacida.Location.X + lbNacida.Width + 5, lbNacida.Location.Y);
            tbNacida.KeyPress += CajasReglas_KeyPress;
       //Etiqueta con los ejemplos;
            Label lbEjemplos = new Label();
            lbEjemplos.Font = new Font("Dejavu-Sans", 9);
            lbEjemplos.Location = new Point(lbNacida.Location.X, lbNacida.Location.Y + 40);
            lbEjemplos.AutoSize = true;
            lbEjemplos.Visible = true;
            dialogoreglas.Controls.Add(lbEjemplos);
            lbEjemplos.Text = " Ejemplos: \n 01234567 / 3 ---> Crecimiento moderado.\n 23 / 36 ---> Hight life.\n 1357 / 1357 ---> Replicantes.Crecimiento rápido.\n 235678 / 3678 ---> Rombos, crecimiento rápido.\n 34 / 34 ---> Estable.\n 4 / 2 ---> Crecimiento moderado.\n 51 / 346 ---> Vida media. ";
         
        }

        /// <summary>
        ///  
        /// CONTROLA QUE SOLO SE ESCRIBEN NUMEROS DEL 0 AL 8 Y EL TEXTO NO ES MAS LARGO DE 8 DIGITOS
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CajasReglas_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox caja = (TextBox)sender;
            if (caja.Text.Length < 8 && e.KeyChar >= '0' && e.KeyChar <= '8')
                e.Handled = false;
            else if (e.KeyChar == 8) // Tecla retroceso
                e.Handled = false;
            else if( e.KeyChar == 13)// Tecla enter
            {
                if (caja.Text.Length == 0)
                {
                    MessageBox.Show("Hay que introducir algun valor.");
                    e.Handled = true;
                }
                else if (caja.Name == "tbViva")
                {
                    tbNacida.Focus();
                    e.Handled = true;
                }
                else if( caja.Name == "tbNacida")
                {
                    viva = tbViva.Text;
                    nacida = tbNacida.Text;
                    // Lista de la cantidad de celulas adyacentes para que una celula siga viva segun las reglas 
                    continuaviva.Clear();
                    for (int i = 0; i < viva.Length; i++)
                        continuaviva.Add(Int32.Parse(viva[i].ToString()));
                    // Lista de la cantidad de celulas adyacentes para que una celula nazca segun las reglas
                    nacera.Clear();
                    for (int i = 0; i < nacida.Length; i++)
                        nacera.Add(Int32.Parse(nacida[i].ToString()));
                    this.Text = "OrgaSnismo0      ( Daniel Santos version )                      ( Mantenerse viva: " + viva + "  Nacer: " + nacida + " )"; 
                    dialogoreglas.Close();
                }
            }
            else if (caja.Text.Length >= 8)
            {
                MessageBox.Show("No se pueden introducir más de 8 digitos");
                e.Handled = true;
            }
            else
                e.Handled = true;
                
        }


        /// <summary>
        /// 
        /// HACE SONAR LA RAFAGA DE SALIDA DEL PROGRAMA
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            SoundPlayer reproductor = new SoundPlayer();
            reproductor.SoundLocation = Directory.GetCurrentDirectory() + "\\Rafaga-Organismo-Salida.wav";
            reproductor.Play();
            // Hacer una pausa para dar tiempo a que suene la rafaga
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(400);
            }
            this.Dispose();
            System.Environment.Exit(0);
        }

        private void volverAReglasPorDefectoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            viva = "23";
            nacida = "3";
            //Limpiar las listas de valores correspondientes a las reglas
            continuaviva.Clear();
            nacera.Clear();
            //Llenar las listas anteriores con los valores de las reglas por defecto
            foreach (char c in viva)
                continuaviva.Add(Int32.Parse(c.ToString()));
            foreach (char c in nacida)
                nacera.Add(Int32.Parse(c.ToString()));
            // Cambiar el rotulo del formulario  y establecer los colores por defecto
            this.Text = "OrgaSnismo0      ( Daniel Santos edition )                      ( Mantenerse viva: " + viva + "  Nacer: " + nacida + " )";
            exterior = Color.Red; //Color de la linea alrrededor de la celula viva por defecto
            interior = Color.FromArgb(155, 250, 5); // Color interior de la celula viva por defecto
            interiorV = Color.FromArgb(155, 250, 5); // Color interior de la nueva celula en el turno por defecto
            fondo = Color.Black; // Color del fondo por defecto
            grafico.Clear(fondo);
            interiorM = Color.Black; // Color de las celulas muertas en el turno por defecto.
            lineamuerta.Color = fondo;
            try
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(Color.FromArgb(155, 250, 5)), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

         /// <summary>
        /// 
        /// MUESTRA TODOS LOS CONTROLES
        /// 
        /// </summary>

        private void MostrarControles()
        {
            btZoomMas.Visible = true;
            btZoomMenos.Visible = true;
            botonesZoomToolStripMenuItem.Checked = true;

            btIzquierda.Visible = true;
            btDerecha.Visible = true;
            btArriba.Visible = true;
            btAbajo.Visible = true;
            botonesDeDesplazamientoToolStripMenuItem.Checked = true;

            nudVelocidad.Visible = true;
            lbRotulonud.Visible = true;
            cambiarPeriodoToolStripMenuItem.Checked = true;

        }


        /// <summary>
        /// 
        /// OCULTA TODOS LOS CONTROLES
        /// 
        /// </summary>

        private void OcultarControles()
        {
            btZoomMas.Visible = false;
            btZoomMenos.Visible = false;
            botonesZoomToolStripMenuItem.Checked = false;


            btIzquierda.Visible = false;
            btDerecha.Visible = false;
            btArriba.Visible = false;
            btAbajo.Visible = false;
            botonesDeDesplazamientoToolStripMenuItem.Checked = false;

            nudVelocidad.Visible = false;
            lbRotulonud.Visible = false;
            cambiarPeriodoToolStripMenuItem.Checked = false;
        }

        private void btPausa_Click(object sender, EventArgs e)
        {
            if (enmarcha)
            {
                btAceptar.BackColor = Color.Chartreuse;
                btAceptar.ForeColor = Color.Black;
                btAceptar.Text = "Continuar";
                enmarcha = false;
              
                timer1.Stop();
            }
            else
            {
                btAceptar.BackColor = Color.Black;
                btAceptar.Text = "Pausa";
                btAceptar.ForeColor = Color.Silver;
                enmarcha = true;
                timer1.Start();

            }
        }

        /// <summary>
        /// 
        /// MUESTRA EL CUADRO DE DIALOGO PARA CAMBIAR LOS COLORES
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cambiarColoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogoColores = new Form();
            DialogoColores.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            DialogoColores.BackColor = Color.SeaGreen;
            DialogoColores.Show();
            DialogoColores.Owner = this;
            DialogoColores.AutoSize = false;
          
            DialogoColores.Text = "Cambiar Colores.";
            lbColorLinea = new Label();
            lbColorLinea.Visible = true;
            lbColorLinea.Text = "Color de la línea.";
            lbColorLinea.Font = new Font("Dejavu-Sans", 16,FontStyle.Bold);
            DialogoColores.Controls.Add(lbColorLinea);
            lbColorLinea.Location = new Point(10, 15);
            lbColorLinea.BackColor = Color.SeaGreen;
            lbColorLinea.AutoSize = true;
            lbColorLinea.ForeColor = exterior;
            lbColorLinea.Click += CambiarColorLinea;

            lbColorInterior = new Label();
            lbColorInterior.Visible = true;
            lbColorInterior.Text = "Color de las células vivas.";
            lbColorInterior.Font = new Font("Dejavu-Sans", 16, FontStyle.Bold);
            DialogoColores.Controls.Add(lbColorInterior);
            lbColorInterior.Location = new Point(lbColorLinea.Location.X, lbColorLinea.Location.Y+lbColorLinea.Height+10 );
            lbColorInterior.BackColor = Color.SeaGreen;
            lbColorInterior.AutoSize = true;
            lbColorInterior.ForeColor = interior;
            lbColorInterior.Click += CambiarColorInterior;

            lbColorInteriorV = new Label();
            lbColorInteriorV.Visible = true;
            lbColorInteriorV.Text = "Color de las nuevas células en el turno.";
            lbColorInteriorV.Font = new Font("Dejavu-Sans", 16, FontStyle.Bold);
            DialogoColores.Controls.Add(lbColorInteriorV);
            lbColorInteriorV.Location = new Point(lbColorLinea.Location.X, lbColorInterior.Location.Y + lbColorLinea.Height + 10);
            lbColorInteriorV.BackColor = Color.SeaGreen;
            lbColorInteriorV.AutoSize = true;
            lbColorInteriorV.ForeColor = interiorV;
            lbColorInteriorV.Click += CambiarColorInteriorV;

            lbColorInteriorM = new Label();
            lbColorInteriorM.Visible = true;
            lbColorInteriorM.Text = "Color de las células muertas en el turno.";
            lbColorInteriorM.Font = new Font("Dejavu-Sans", 16, FontStyle.Bold);
            DialogoColores.Controls.Add(lbColorInteriorM);
            lbColorInteriorM.Location = new Point(lbColorLinea.Location.X, lbColorInteriorV.Location.Y + lbColorLinea.Height + 10);
            lbColorInteriorM.BackColor = Color.SeaGreen;
            lbColorInteriorM.AutoSize = true;
            lbColorInteriorM.ForeColor = interiorM;
            lbColorInteriorM.Click += CambiarColorInteriorM;

            lbColorFondo = new Label();
            lbColorFondo.Visible = true;
            lbColorFondo.Text = "Color del fondo";
            lbColorFondo.Font = new Font("Dejavu-Sans", 16, FontStyle.Bold);
            DialogoColores.Controls.Add(lbColorFondo);
            lbColorFondo.Location = new Point(lbColorLinea.Location.X, lbColorInteriorM.Location.Y + lbColorInterior.Height + 10);
            lbColorFondo.BackColor = Color.SeaGreen;
            lbColorFondo.AutoSize = true;
            lbColorFondo.ForeColor = fondo;
            lbColorFondo.Click += CambiarColorFondo;

            DialogoColores.Size = new Size(lbColorInteriorM.Width + 100 , 270);
        }

        private void CambiarColorLinea(object sender,EventArgs e)
        {
            ColorDialog dialogo = new ColorDialog();
            if (dialogo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                exterior = dialogo.Color;
                lbColorLinea.ForeColor = exterior;
            }
        }
        private void CambiarColorInterior(object sender, EventArgs e)
        {
            ColorDialog dialogo = new ColorDialog();
            if (dialogo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                interior = dialogo.Color;
                lbColorInterior.ForeColor = interior;
            }

        }
        private void CambiarColorInteriorV(object sender, EventArgs e)
        {
            ColorDialog dialogo = new ColorDialog();
            if (dialogo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                interiorV = dialogo.Color;
                lbColorInteriorV.ForeColor = interiorV;
            }

        }
        private void CambiarColorInteriorM(object sender, EventArgs e)
        {
            ColorDialog dialogo = new ColorDialog();
            if (dialogo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                interiorM = dialogo.Color;
                lbColorInteriorM.ForeColor = interiorM;
            }

        }
        private void CambiarColorFondo(object sender, EventArgs e)
        {
            ColorDialog dialogo = new ColorDialog();
            if (dialogo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fondo = dialogo.Color;
                interiorM = fondo;
                lineamuerta.Color = fondo;
                lbColorFondo.ForeColor = fondo;
            }
            grafico.Clear(fondo);
           
            lock(celulas)
            {
                foreach (PointF p in celulas)
                {
                    grafico.FillRectangle(new SolidBrush(interior), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
            }
        }

        /// <summary>
        /// 
        /// PINTA LA CELULAS NUEVAS CON LOS COLORES ESCOGIDOS Y LAS MUERTAS DE COLOR NEGRO
        /// 
        /// </summary>
        /// 
        private void PintarCelulas()
        {
                if ( proximasvivas.Count == 0 && proximasmuertas.Count == 0)
                {
                    grafico.Clear(fondo);
                    lock (celulas)
                    {
                        foreach (PointF p in celulas)
                        {
                            grafico.FillRectangle(new SolidBrush(interior), p.X, p.Y, tamaño, tamaño);
                            grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                        }
                    }
                }
                
                 
            
                  foreach (PointF p in muertasanteriores) // Pintar las celulas muertas ya antes del color del fondo
                  {
                      grafico.FillRectangle(new SolidBrush(fondo), p.X, p.Y, tamaño, tamaño);
                      grafico.DrawRectangle(lineamuerta, p.X, p.Y, tamaño, tamaño);
                  }

                  foreach (PointF p in vivasanteriores) // Pintar las celulas vivas ya antes del color del interior de las celulas
                  {
                      grafico.FillRectangle(new SolidBrush(interior), p.X, p.Y, tamaño, tamaño);
                      grafico.DrawRectangle(lineamuerta, p.X, p.Y, tamaño, tamaño);
                  }
            
                foreach(PointF p in proximasmuertas) // Pintar las celulas que mueren este turno del color elegido
                { 
                    grafico.FillRectangle(new SolidBrush(interiorM), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(lineamuerta, p.X, p.Y, tamaño, tamaño);
                }
                foreach (PointF p in proximasvivas) // Pintar las nuevas celulas 
                {
                    grafico.FillRectangle(new SolidBrush(interiorV), p.X, p.Y, tamaño, tamaño);
                    grafico.DrawRectangle(linea, p.X, p.Y, tamaño, tamaño);
                }
                 
            pbFondo.Invalidate();
        }

        private void lbEgo_MouseEnter(object sender, EventArgs e)
        {
            lbEgo.BackColor = Color.SeaGreen;
            lbEgo.ForeColor = Color.Black;
        }

        private void lbEgo_MouseLeave(object sender, EventArgs e)
        {
            lbEgo.BackColor = fondo;
            lbEgo.ForeColor = fondo;
        }

        private void beepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!beep)
            {
                beepToolStripMenuItem.Checked = true;
                beep = true;
            }
            else
            {
                beepToolStripMenuItem.Checked = false;
                beep = false;
            }
        }
        /// <summary>
        /// 
        /// GUARDA EL BITMAP EN LA UBICACION Y NOMBRE ELEGIDO POR EL USUARIO
        /// CUANDO SE PULSA LA OPCION DEL MENU
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guardarImagenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Filter = "Bipmap (*.bmp)|*.bmp";
            if(dialogo.ShowDialog() == DialogResult.OK)
            {
                string ruta = dialogo.FileName;
                bitmap.Save(ruta);
            }
        }

        /// <summary>
        /// 
        /// ACTIVA LA OPCION DE GUARDADO AUTOMATICO DE IMAGENES QUE GUARDA CADA CICLO EL BITMAP EN LA UBICACION
        /// ELEGIDA POR EL USUARIO CON EL NOMBRE ELEGIDO POR EL USUARIO MAS EL CICLO
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guardadoAutomaticoDeImagenenesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!guardadoautomatico)
            {
                SaveFileDialog dialogo = new SaveFileDialog();
                dialogo.Filter = "Bitmap: (*.bmp)|*.bmp";
                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    prefijo = dialogo.FileName;
                }
                guardadoAutomaticoDeImagenenesToolStripMenuItem.Checked = true;
                guardadoautomatico = true;
            }
            else
            {
                guardadoautomatico = false;
                guardadoAutomaticoDeImagenenesToolStripMenuItem.Checked = false;
            }
        }
		*/

    }
}
