using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estado_Clientes
{
    public class SoftlandController
    {
        SqlConnection cnn = new SqlConnection("Data Source=SRVBC06; DataBase=BVI2017; User=sa; Password=Softland570;");
        //SqlConnection cnn = new SqlConnection("Data Source=DESKTOP-JJTFARH\\SRVBC06; DataBase=BARRONVIEYRA; User=sa; Password=Softland570;");
        


        public List<Object> listar()
        {
            DataTable rotacion = new DataTable();
            List<Object> result = new List<Object>();
            try
            {
                //String consulta = "select * from softland.iw_gsaen FULL OUTER JOIN softland.cwtauxi ON softland.cwtauxi.CodAux = softland.iw_gsaen.CodAux FULL OUTER JOIN softland.cwtcomu ON softland.cwtcomu.ComCod = softland.iw_gsaen.ComDch FULL OUTER JOIN softland.cwtpais ON softland.cwtpais.PaiCod = softland.iw_gsaen.PaiDch FULL OUTER JOIN softland.nw_nventa ON softland.nw_nventa.NVNumero = softland.iw_gsaen.nvnumero FULL OUTER JOIN softland.iw_gmovi ON softland.iw_gmovi.NroInt = iw_gsaen.NroInt and softland.iw_gmovi.Tipo = 'F' where softland.iw_gsaen.Tipo = 'F' AND cast(softland.iw_gsaen.FecHoraCreacion as date ) = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' order by softland.iw_gsaen.FecHoraCreacion asc; ";
                String consulta = "SELECT fa.CodAux, fa.Fecha, SUM(fa.Total) as total"+
                                    " FROM softland.iw_gsaen fa"+
                                    " where fa.Tipo = 'F' AND DateDiff(d, fa.Fecha , SysUtcDateTime()) < 547"+
                                    " group by fa.CodAux, fa.Fecha order by fa.CodAux,fa.Fecha";

                SqlCommand cmd = new SqlCommand(consulta, cnn);

                cmd.CommandType = CommandType.Text;
                cnn.Open();
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(rotacion);

                foreach (DataRow ren in rotacion.Rows)
                {
                    var accessData = new Dictionary<string, object>(); //Variable para mapear todos los campos
                    accessData.Add("C_digo_Auxiliar", ren["CodAux"].ToString());
                    accessData.Add("Fecha", ren["Fecha"].ToString());
                    accessData.Add("Total", ren["total"].ToString());
                    result.Add(JsonConvert.SerializeObject(accessData));

                }
            }
            catch (SqlException err)
            {
                Console.WriteLine("ERROR : " + DateTime.Now.ToString("HH:mm:ss") + " SQL EXCEPTION " + err);
            }
            finally
            {
                cnn.Close();
            }
            return result;
        }
    }
}
