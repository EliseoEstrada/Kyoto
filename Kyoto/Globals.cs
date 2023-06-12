using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyoto
{
    class Globals
    {
        static public string Dispositivo = "";          //Camara actual
        static public string apodoDispositivo = "";     //Apodo de camara
        static public bool hayDispositivo = false;      //Hay camara conectada?
        static public bool detectarRostros = true;      //Funcion de detectar rostros
        static public string formato = "PNG";           //Formato de imagen
        
        static public string filtroActual = "";         

        static public string actualDocumento = "ninguno";   //imagen o video
        static public float canalR = 1.0f;
        static public float canalG = 1.0f;
        static public float canalB = 1.0f;
    }


}
