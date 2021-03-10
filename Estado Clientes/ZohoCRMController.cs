using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CronClientes
{
    public class ZohoCRMController
    {
        public static string endPointModules = "https://www.zohoapis.com/crm/v2/";


        public class ResultLine
        {
            public string C_digo_Auxiliar { get; set; }
            public int dias { get; set; }
            public int Total { get; set; }
            public long id { get; set; }
            public DateTime ultCompra { get; set; }

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////             METODO GET BULK           ////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////

        public List<Object> getBulk(string module,string token)
        {

            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token); //Access token 
            int count = 1;
            String contenido = "";
            List<Object> result = new List<Object>();
            try
            {
                do
                {
                    String parametros = "?fields=Id,C_digo_Auxiliar,Estado_Cuenta1&page=" + count + "&per_page=200";
                    var client = new RestClient(endPointModules + module + parametros); 
                    
                    IRestResponse response = client.Execute(request);
                    var responseContent = response.Content;
                    var objectContact = JObject.Parse(responseContent);

                    contenido = objectContact["data"].ToString();
                    foreach (object obj in objectContact["data"])
                    {
                        result.Add(obj);
                    }
                    count++;
                    
                } while (contenido != "");

                return result;
            }

            catch (Exception e)
            {
                return result;
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////METODO GET OARA BUSQUEDA ESPECIFICA////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////

        public string BuscarRegistro(string module, string field, string value, string token)
        {

            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token); //Access token 
            var searchParameters = field + ":equals:" + value;
            var client = new RestClient(endPointModules + module + "/search?criteria=(" + searchParameters + ")"); //_Nombre del Modulo (Nombre API)

            IRestResponse response = client.Execute(request);
            var responseContent = response.Content;

            try
            {
                var objectContact = JObject.Parse(responseContent);
                //Console.WriteLine("ok");
                return objectContact["data"][0]["id"].ToString();
                //return objectContact.ToString();
            }
            catch (Exception e)
            {
                //Console.WriteLine("ERROR DETECTADO");
                return "NO DATA " + e;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////METODO UPDATE//////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /////codaux, nombre, rut, mail, newToken

        public string ActualizarRegistro(List<Object> lst, String token)
        {

            var client = new RestClient(endPointModules + "Accounts"); //_Nombre del Modulo (Nombre API)
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token); //Access token 

            var bulk_data = new Dictionary<string, object>();
            //Variable para ingresar los distintos registros
            
            try
            {
                
                bulk_data.Add("data", lst);//Cuerpo de los parametros
                request.AddParameter("undefined", JsonConvert.SerializeObject(bulk_data), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                var responseContent = response.Content;
                var objectAccount = JObject.Parse(responseContent);
                //Console.WriteLine(objectAccount.ToString());
                //var responseContent = response.Content;
                //var objectAccount = JObject.Parse(responseContent);
                //return objectAccount["data"][0]["code"].ToString();
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "NO UPDATE " + e;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////METODO DELETE//////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// No se eliminara información de Zoho CRM, solo se cambiara el estado de los registros si es necesario
        /// </summary>
        /// <returns></returns>

        ////////////////////////////////////////////////////////////////////////////////////////////////////

    }
}
