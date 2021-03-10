using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronClientes
{
    public class Logs
    {
        public void CrearLog()
        {
            string path = "C:\\";
            Directory.CreateDirectory(path + "/Barronvieyra");
            path = path.Replace(@"\", "/");
            string finalpath = path + "Barronvieyra";
            Console.WriteLine("Loading API Zoho CRM...");
            Console.WriteLine(finalpath);
            Directory.CreateDirectory(finalpath + "/logs/Rotacion_Cliente");
            Directory.CreateDirectory(finalpath + "/logs/Rotacion_Cliente/" + DateTime.Now.ToString("MM-dd-yyyy"));
            string ruta_log = finalpath + "/logs/Rotacion_Cliente/" + DateTime.Now.ToString("MM-dd-yyyy") + "/ Log (" + DateTime.Now.ToString("MM-dd-yyyy HH_mm_ss") + ").txt";
            FileStream filestream = new FileStream(ruta_log, FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);
        }
    }
}
