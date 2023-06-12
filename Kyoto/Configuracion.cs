using System;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Kyoto
{
    public partial class Configuracion : Form
    {

        private bool hayDispositivo = false;
        private FilterInfoCollection misDispositivos;

        public Configuracion()
        {
            InitializeComponent();
        }

        private void Configuracion_Load(object sender, EventArgs e)
        {
            cargarDispositivos();

            cbFormato.Items.Add("JPG");
            cbFormato.Items.Add("PNG");
            cbFormato.SelectedIndex = 1;

            cbDectectarRostros.Items.Add("Activado");
            cbDectectarRostros.Items.Add("Desactivado");
            cbDectectarRostros.SelectedIndex = 0;
        }


        public void cargarDispositivos()
        {
            misDispositivos = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (misDispositivos.Count > 0)
            {
                hayDispositivo = true;                
                for (int i = 0; i < misDispositivos.Count; i++)
                {
                    CBDispositivo.Items.Add(misDispositivos[i].Name.ToString());
                    //CBDispositivo.Text = misDispositivos[0].Name.ToString();
                    //CBDispositivo.Text = Variables.Dispositivo;
                }
                CBDispositivo.SelectedIndex = 0;
                Globals.Dispositivo = CBDispositivo.SelectedItem.ToString();
            }
            else
            {
                hayDispositivo = false;
            }
            Globals.hayDispositivo = hayDispositivo;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Globals.Dispositivo = CBDispositivo.SelectedItem.ToString();
            Globals.apodoDispositivo = misDispositivos[CBDispositivo.SelectedIndex].MonikerString;
            Globals.formato = cbFormato.SelectedItem.ToString();

            var detectarRostros = true;
            if(cbDectectarRostros.SelectedIndex == 1)
            {
                detectarRostros = false;
            }
            Globals.detectarRostros = detectarRostros;

            MessageBox.Show("Cambios realizados", "Aviso");
        }
    }
}
