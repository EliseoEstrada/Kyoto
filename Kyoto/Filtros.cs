using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kyoto
{
    public partial class Filtros : Form
    {
        Principal padre = null;
        public Filtros()
        {
            InitializeComponent();
        }

        private void Filtros_Load(object sender, EventArgs e)
        {
            padre = this.Owner as Principal;

            cbFiltros.Items.Add("Gaussiano");
            cbFiltros.Items.Add("Sal y pimienta");
            cbFiltros.Items.Add("Enfocar");
            cbFiltros.Items.Add("Laplaciano");
            cbFiltros.Items.Add("Sobel (x, y)");
            cbFiltros.Items.Add("Sobel (derivada en x)");
            cbFiltros.Items.Add("Sobel (derivada en y)");
            cbFiltros.Items.Add("Direccional Norte");
            cbFiltros.Items.Add("Canales RGB");
            cbFiltros.SelectedIndex = 0;


            Globals.canalR = 1.0f;
            Globals.canalG = 1.0f;
            Globals.canalB = 1.0f;
        }

        private void btnAplicarFiltro_Click(object sender, EventArgs e)
        {

            if (padre != null)
            {
                Bitmap imagen = (Bitmap)padre.pbImagen.Image;
                if (imagen != null)
                {

                    padre.pbResultado.Image = null;
                    string filtro = cbFiltros.SelectedItem.ToString();
                    padre.pbResultado.Image = Filters.AplicarFiltro(imagen, filtro);
                    Globals.filtroActual = filtro;
                }

                

            }
        }

        /// //////////GAUSS
        //https://softwarebydefault.com/2013/06/08/calculating-gaussian-kernels/


        private void btnRevertir_Click(object sender, EventArgs e)
        {
            if(padre.pbImagen.Image != null)
            {
                padre.pbResultado.Image = padre.pbImagen.Image;
                Globals.filtroActual = "";
            }
        }

        private void trackBarR_ValueChanged(object sender, EventArgs e)
        {
            float r = ((float)trackBarR.Value / 10.0f);
            labelR.Text = r.ToString();
            Globals.canalR = r;
        }

        private void trackBarG_ValueChanged(object sender, EventArgs e)
        {
            float g = (trackBarG.Value / 10.0f);
            labelG.Text = g.ToString();
            Globals.canalG = g;
        }

        private void trackBarB_ValueChanged(object sender, EventArgs e)
        {
            float b = (trackBarB.Value / 10.0f);
            labelB.Text = b.ToString();
            Globals.canalB = b;
        }

        private void cbFiltros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbFiltros.SelectedItem == "Canales RGB")
            {
                gb_canales.Visible = true;
            }
            else
            {
                gb_canales.Visible = false;
            }
        }
    }
}
