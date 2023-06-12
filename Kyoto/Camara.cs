using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
namespace Kyoto
{
    public partial class Camara : Form
    {
        private string camara = "";
        private bool hayDispositivo = false;
        private FilterInfoCollection misDispositivos;
        private VideoCaptureDevice miWebCam;
        static readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("resources/haarcascade_frontalface_alt_tree.xml");

        private bool camaraEncendida = false;
        private int contadorPersonas = 0;

        public Camara()
        {
            InitializeComponent();

            if (Globals.hayDispositivo)
            {
                hayDispositivo = true;
            }

            if (!Globals.detectarRostros)
            {
                label3.Text = "Detectar: Desactivado";
                label2.Visible = false;
                label_personasDetectadas.Visible = false;
            }
        }


        private void btnCapturar_Click(object sender, EventArgs e)
        {
            if (miWebCam != null && miWebCam.IsRunning)
            {
                Principal padre = this.Owner as Principal;
                if(padre != null)
                {
                    padre.pbImagen.Image = pbCamara.Image;
                    padre.pbResultado.Image = pbCamara.Image;
                    Globals.actualDocumento = "imagen";
                }
                
            }
        }

 


        private void btnEncender_Click(object sender, EventArgs e)
        {
            if (camaraEncendida)
            {
                btnEncender.Text = "Encender";
                camaraEncendida = false;
                cerrarWebCam();
                //Contar personas
                label_personasDetectadas.Text = contadorPersonas.ToString();
            }
            else
            {
                btnEncender.Text = "Apagar";
                camaraEncendida = true;

                //int i = CBDispositivo.SelectedIndex;
                //camara = misDispositivos[i].MonikerString;

                camara = Globals.apodoDispositivo;
                contadorPersonas = 0;
                label_personasDetectadas.Text = contadorPersonas.ToString();

                miWebCam = new VideoCaptureDevice(camara);
                miWebCam.NewFrame -= new NewFrameEventHandler(capturando);
                miWebCam.NewFrame += new NewFrameEventHandler(capturando);
                miWebCam.Start();
            }

            
        }

        private void capturando(object sender, NewFrameEventArgs eventArgs)
        {
            //Aforge
            Bitmap imagen = (Bitmap)eventArgs.Frame.Clone();
            if (Globals.detectarRostros)
            {
                int auxPersonas = 0;
                //Emgu
                Image<Bgr, byte> grayImage = new Image<Bgr, byte>(imagen);
                Rectangle[] rectangles = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 3);
                foreach (Rectangle rectangle in rectangles)
                {
                    //Application.Idle -= new EventHandler(contarPersonas);
                    //Application.Idle += new EventHandler(contarPersonas);

                    
                    using (Graphics graphics = Graphics.FromImage(imagen))
                    {
                        using (Pen pen = new Pen(Color.Red, 2))
                        {
                            graphics.DrawRectangle(pen, rectangle);

                        }
                    }

                    auxPersonas++;
                }

                if(auxPersonas > contadorPersonas)
                {
                    contadorPersonas = auxPersonas;
                }
            }

            pbCamara.Image = imagen;

        }

        public void cerrarWebCam()
        {
            if (miWebCam != null && miWebCam.IsRunning)
            {
                miWebCam.SignalToStop();
                miWebCam = null;
                pbCamara.Image = null;
            }
        }


        private void Camara_FormClosed(object sender, FormClosedEventArgs e)
        {
            cerrarWebCam();
        }
    }
}
