using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using Accord.Video.FFMPEG;

namespace Kyoto
{
    public partial class Principal : Form
    {
        private Form activeForm = null;

        //Video
        VideoCapture grabber;
        Image<Bgr, Byte> currentFrame;
        double duracion;
        double FrameCount;
        bool pause = false;
        bool videoLoad = false;

        float canalR = 0;
        float canalG = 0;
        float canalB = 0;
        public Principal()
        {
            InitializeComponent();
            openChildForm(new Camara());

        }

        private void Principal_Load(object sender, EventArgs e)
        {
            FilterInfoCollection misDispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (misDispositivos.Count > 0)
            {
                for (int i = 0; i < misDispositivos.Count; i++)
                {
                    Globals.Dispositivo = misDispositivos[i].Name.ToString();
                    Globals.apodoDispositivo = misDispositivos[i].MonikerString;
                    break;
                }
            }
        }

        private void openChildForm(Form _formHijo)
        {
            if (activeForm != null) //si existe un form abierto lo cerramos
            {
                activeForm.Close();
            }
            AddOwnedForm(_formHijo);//Permitir que hijo pase datos
            activeForm = _formHijo; //guardar formulario activo
            _formHijo.TopLevel = false; //indicar que el formulario se comportara como un control
            _formHijo.FormBorderStyle = FormBorderStyle.None;
            _formHijo.Dock = DockStyle.Fill; //rellenar todo el panel
            panelContenedor.Controls.Add(_formHijo); //agregar al panel
            panelContenedor.Tag = _formHijo; //asociar el formulario con el panel
            _formHijo.BringToFront(); //mostrarlo al frente
            _formHijo.Show();
        }


        private void btnCamara_Click(object sender, EventArgs e)
        {
            openChildForm(new Camara());
        }

        private void btnFiltro_Click(object sender, EventArgs e)
        {
            openChildForm(new Filtros());
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            openChildForm(new Configuracion());
        }

        private void Principal_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (activeForm != null) //si existe un form abierto lo cerramos
            {
                activeForm.Close();
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {

            if(Globals.actualDocumento == "imagen")
            {
                if (pbResultado.Image != null)
                {
                    var filter = "";
                    if (Globals.formato == "JPG")
                    {
                        filter = "JPG|*.jpg";
                    }
                    else if (Globals.formato == "PNG")
                    {
                        filter = "PNG|*.png";
                    }

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = @filter;
                    saveFileDialog.FileName = "Resultado";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        pbResultado.Image.Save(saveFileDialog.FileName);
                    }

                }
            }

            if(Globals.actualDocumento == "video")
            {
                MessageBox.Show("Exportando video", "Aviso", MessageBoxButtons.OK);
                crearVideo();
            }
            
        }

        private void btnImagen_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = @"Files|*.jpg;*.jpeg;*.png";
                openFileDialog1.FileName = "Imagen";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string imagen = openFileDialog1.FileName;
                    pbImagen.Image = Image.FromFile(imagen);
                    pbResultado.Image = Image.FromFile(imagen);
                    pbResultado.BackgroundImage = null;
                    Globals.actualDocumento = "imagen";
                    gb_mediaControl.Visible = false;
                    openChildForm(new Filtros());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("El archivo seleccionado no es un tipo de imagen válido");
            }
        }

        private void btnVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Files (*.mp4)|*.mp4";
            openFileDialog1.FileName = "Video";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Globals.actualDocumento = "video";
                gb_mediaControl.Visible = true;
                videoLoad = true;
                openChildForm(new Filtros());

                grabber = new VideoCapture(openFileDialog1.FileName);
                grabber.QueryFrame();

                Mat m = new Mat();
                grabber.Read(m);

                currentFrame = new Image<Bgr, byte>(m.Bitmap);
                currentFrame.Resize(pbImagen.Width, pbImagen.Height, Inter.Cubic);

                //currentFrame = grabber.QueryFrame().Resize
                pbImagen.Image = currentFrame.Bitmap;
                pbResultado.Image = currentFrame.Bitmap;

                duracion = grabber.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                //CAP_PROP.CV_CAP_PROP_POS_FRAMES
                FrameCount = grabber.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames);
                videoLoad = true;
                pbImagen.BackgroundImage = null;
                pbResultado.BackgroundImage = null;

            }
        }

        private void reproducirVideo(object sender, EventArgs e)
        {
            if (!pause)
            {
                if (FrameCount < duracion - 2)
                {
                    Mat m = new Mat();
                    grabber.Read(m);

                    currentFrame = new Image<Bgr, byte>(m.Bitmap);
                    currentFrame.Resize(pbImagen.Width, pbImagen.Height, Inter.Cubic);
                    FrameCount = grabber.GetCaptureProperty(CapProp.PosFrames);
                }
                else
                {
                    FrameCount = 0;
                    grabber.SetCaptureProperty(CapProp.PosFrames, 0);
                }

                pbImagen.Image = currentFrame.Bitmap;

                pause = true;

                if (Globals.filtroActual != "")
                {
                    pbResultado.Image = Filters.AplicarFiltro(currentFrame.Bitmap, Globals.filtroActual);
                }
                else
                {
                    pbResultado.Image = currentFrame.Bitmap;
                }
                pause = false;
            }
           
        }
        //https://docs.rainmeter.net/tips/colormatrix-guide/
        //https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.colormatrix?view=windowsdesktop-5.0
        private void btn_play_pb1_Click(object sender, EventArgs e)
        {
            if (videoLoad)
            {
                Application.Idle -= new EventHandler(reproducirVideo);
                Application.Idle += new EventHandler(reproducirVideo);
                pause = false;
            }
        }

        private void btn_pause_pb1_Click(object sender, EventArgs e)
        {
            if (videoLoad)
            {
                pause = true;
            }
        }

        private void btn_stop_pb1_Click(object sender, EventArgs e)
        {
            detenerVideo();
        }

        private void detenerVideo()
        {
            if (videoLoad)
            {
                FrameCount = 0;
                grabber.SetCaptureProperty(CapProp.PosFrames, 0);
                Application.Idle -= new EventHandler(reproducirVideo);
            }
        }

        

        /*CREAR VIDEO*/

        private void CreateMovie()
        {
            int width = 320;
            int height = 240;


            // create instance of video writer
            VideoFileWriter writer = new VideoFileWriter();
            // create new video file
            string path = "..\\..\\..\\test.avi";
            writer.Open(path, width, height, 25, VideoCodec.MPEG4);
            // create a bitmap to save into the video file
            Bitmap image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            // write 1000 video frames
            for (int i = 0; i < 1000; i++)
            {
                image.SetPixel(i % width, i % height, Color.Red);
                writer.WriteVideoFrame(image);
            }

            writer.Close();
            //VideoFileSource videoSource = new VideoFileSource(writer.ToString());

            //start the video source
            //videoSource.Start();


            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.FileName = writer.ToString();
            //if (saveFileDialog.ShowDialog() == DialogResult.OK)
            //{
            //    pbResultado.Image.Save(saveFileDialog.FileName);
            //}
        }

        private void crearVideo()
        {

            VideoCapture vc;
            vc = new VideoCapture(openFileDialog1.FileName);
            vc.QueryFrame();

            Mat m = new Mat();
            vc.Read(m);

            //Obtener cantidad de frames
            //double cantidadFrames = vc.GetCaptureProperty(CapProp.FrameCount);

            int width = 1280;
            int height = 720;

            Image<Bgr, Byte> frameActual;

            // create instance of video writer
            VideoFileWriter writer = new VideoFileWriter();
            // create new video file
            string path = "video exportado.avi";
            writer.Open(path, width, height, 5, VideoCodec.MPEG4);
            //cantidadFrames
            // write 1000 video frames
            for (int i = 0; i < 25 - 2; i++)
            {

                frameActual = new Image<Bgr, byte>(m.Bitmap);
                frameActual.Resize(width, height, Inter.Cubic);
                Bitmap image;

                if (Globals.filtroActual != "")
                {
                    image = Filters.AplicarFiltro(frameActual.Bitmap, Globals.filtroActual);
                }
                else
                {
                    image = frameActual.Bitmap;
                }
                writer.WriteVideoFrame(image);

                //leer siguiente frame
                vc.Read(m);
            }

            writer.Close();

            MessageBox.Show("Video exportado con exito", "Aviso", MessageBoxButtons.OK);
        }


    }



}
