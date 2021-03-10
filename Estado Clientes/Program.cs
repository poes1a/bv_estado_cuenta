using CronClientes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;

namespace Estado_Clientes
{
    class Program
    {

        public class Facturas
        {
            public string C_digo_Auxiliar { get; set; }
            public DateTime Fecha { get; set; }
            public int Total { get; set; }

        }

        public class ResultLine
        {
            public string C_digo_Auxiliar { get; set; }
            public string Estado_Cuenta1 { get; set; }
            public int dias { get; set; }
            public int Total { get; set; }
            public long id { get; set; }
            public DateTime ultCompra { get; set; }
            public int contador { get; set; }

        }

        public class Clientes
        {
            public string C_digo_Auxiliar { get; set; }
            public long id { get; set; }
            public string Estado_Cuenta1 { get; set; }

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Rotación de Clientes ....");
            Logs log = new Logs();
            log.CrearLog();
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ZohoCRMTokens token = new ZohoCRMTokens();
                ZohoCRMController controlador = new ZohoCRMController();
                List<Object> responseClientes;
                String tokenAux = token.AccessToken();
                ///////////Extraer lista de Clientes en Zoho CRM//////////////////
                responseClientes = controlador.getBulk("Accounts", tokenAux);
                List<Clientes> lstClientes = new List<Clientes>();
                foreach (Object cli in responseClientes)
                {
                    var objectCliente = JObject.Parse(cli.ToString());
                    lstClientes.Add(new Clientes { C_digo_Auxiliar = objectCliente["C_digo_Auxiliar"].ToString(), id = Int64.Parse(objectCliente["id"].ToString()) , Estado_Cuenta1 = objectCliente["Estado_Cuenta1"].ToString() });
                    //Console.WriteLine(objectCliente["id"]+" - "+objectCliente["C_digo_Auxiliar"]);
                }

                Console.WriteLine("Paso 1");
                /////////////////////////////////////////////////////////////////

                ////////// Extraer Clientes con Facturas desde Softland ///////////
                SoftlandController cnn = new SoftlandController();
                List<Object> responseFacturas;
                responseFacturas = cnn.listar();
                //var aux = JsonConvert.SerializeObject(responseFacturas);
                List<Facturas> lst = new List<Facturas>();
                //Console.WriteLine("Paso 1.1");
                foreach (Object fac in responseFacturas)
                {
                    
                    var objectFactura = JObject.Parse(fac.ToString());
                    //Console.WriteLine("C_digo_Auxiliar:"+objectFactura["C_digo_Auxiliar"]+ "-Total:"+ objectFactura["Total"].ToString()+"-Fecha:"+ objectFactura["Fecha"].ToString());
                    string iDate = objectFactura["Fecha"].ToString();
                    DateTime oDate = Convert.ToDateTime(iDate);
                    Double auxTotal = Convert.ToDouble(objectFactura["Total"].ToString());
                    lst.Add(new Facturas
                    {
                        C_digo_Auxiliar = objectFactura["C_digo_Auxiliar"].ToString(),
                        Fecha = oDate,
                        Total = Convert.ToInt32(auxTotal)
                    });
                    //Console.WriteLine("Paso 1.1");
                    //Console.WriteLine(lst.ToString());
                    // Console.WriteLine(objectFactura["C_digo_Auxiliar"]);
                }
                Console.WriteLine("Paso 2");
                var joinClienteFactura = (from fac in lst
                                          join cli in lstClientes
                                          on fac.C_digo_Auxiliar equals cli.C_digo_Auxiliar
                                          select new { id = cli.id, CodAux = fac.C_digo_Auxiliar, Total = fac.Total, Fecha = fac.Fecha, Estado_Cliente = cli.Estado_Cuenta1 });

                List<ResultLine> results = (from line in joinClienteFactura
                                            group line by line.CodAux into g
                                            select new ResultLine
                                            {
                                                C_digo_Auxiliar = g.First().CodAux,
                                                id = g.First().id,
                                                Total = g.Sum(pc => pc.Total) / g.Count() >= 1 ? g.Sum(pc => pc.Total) / g.Count() : 1,
                                                dias = ((g.Max(pc => pc.Fecha) - g.Min(pc => pc.Fecha)).Days) / (g.Count()-1).CheckForZero(),
                                                ultCompra = g.Max(pc => pc.Fecha),
                                                Estado_Cuenta1 = g.First().Estado_Cliente,
                                                contador = g.Count()
                                            }).ToList();

                //var lista_facturas = joinClienteFactura.ToList();
                Console.WriteLine("Paso 3");
                List<Object> lstAccount = results.ToList<Object>();
                /*foreach (var resultado in results)
                {
                    lstAccount.Add(new ResultLine
                    { C_digo_Auxiliar = objectFactura["C_digo_Auxiliar"].ToString(),
                        Fecha = oDate,
                        Total = Int32.Parse(objectFactura["Total"].ToString())
                    });
                    //Console.WriteLine(lst.ToString());
                    Console.WriteLine(objectFactura["C_digo_Auxiliar"]);
                }*/


                List<object> arraString = new List<object>();

                int count = 0;
                int countCiclos = 0;
                foreach (var resultado in results)
                {
                    var accessData = new Dictionary<object, object>(); //Variable para mapear todos los campos
                                                                       //Variable para ingresar los distintos registros//
                    if (resultado.id != null)
                    {
                        accessData.Add("id", resultado.id);
                        /*
                        var lista_facturas = joinClienteFactura.Where(x => x.CodAux == resultado.C_digo_Auxiliar).OrderByDescending(s=>s.Fecha);
                        int count_aux = 0;
                        DateTime Fecha_1;
                        DateTime Fecha_2;
                        foreach (var factura in lista_facturas)
                        {
                            //validar las ultimas facturas
                            Console.WriteLine(factura.Fecha+"-"+factura.Fecha);
                            if (count == 0)
                            {
                                Fecha_1 = factura.Fecha;
                            }else if (count == 1)
                            {

                            }

                            if (factura.Fecha.AddDays(30) < DateTime.Now)
                            {
                                accessData.Add("Estado_Cuenta1", "Cliente Rojo");
                                accessData.Add("Estado_Cuenta1", "Perdido");
                            }
                            else
                            {
                                accessData.Add("Estado_Cuenta1", "Cliente Nuevo");
                            }



                        }*/
                        if (resultado.contador==1)
                        {
                            Console.WriteLine("Es Cliente Nuevo");
                            accessData.Add("Siguiente_compra", resultado.ultCompra.AddDays(30).ToString("yyyy-MM-dd"));
                            accessData.Add("Rotaci_n",30);
                            if (resultado.ultCompra.AddDays(resultado.dias) >= DateTime.Now)
                            {
                                Console.WriteLine("Opción -2");
                                accessData.Add("Estado_Cuenta1", "Cliente Nuevo");
                            }
                            else
                            {
                                Console.WriteLine("Cliente Rojo o Perido");
                                var dias = (DateTime.Now - resultado.ultCompra.AddDays(resultado.dias)).Days;
                                if (dias >= 90)
                                {
                                    Console.WriteLine("Opción -1");
                                    accessData.Add("Estado_Cuenta1", "Perdido");
                                }
                                else
                                {
                                    Console.WriteLine("Opción 0");
                                    accessData.Add("Estado_Cuenta1", "Cliente Rojo");
                                }
                                Console.WriteLine("Días : " + dias);
                            }
                        }
                        else
                        {
                            accessData.Add("Siguiente_compra", resultado.ultCompra.AddDays(resultado.dias).ToString("yyyy-MM-dd"));

                            if (resultado.contador == 2)
                            {
                                Console.WriteLine("Es Cliente Nuevo o Cliente Rojo o Perido");
                                Console.WriteLine(" Fechas ultima compra + dias : "+resultado.ultCompra.AddDays(resultado.dias));
                                Console.WriteLine(" Fechas Actual               : " + DateTime.Now);
                                if (resultado.ultCompra.AddDays(resultado.dias) >= DateTime.Now)
                                {
                                    
                                    if (resultado.Estado_Cuenta1 != "Cliente Nuevo" && resultado.Estado_Cuenta1 != "Cliente")
                                    {
                                        Console.WriteLine("Opción 1.1");
                                        accessData.Add("Estado_Cuenta1", "Recuperado");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Opción 1.2");
                                        accessData.Add("Estado_Cuenta1", "Cliente Nuevo");
                                    }



                                }
                                else if(resultado.Estado_Cuenta1 == "Cliente Nuevo")
                                {
                                    var dias = (DateTime.Now - resultado.ultCompra.AddDays(resultado.dias)).Days;
                                    if (dias >= 90)
                                    {
                                        Console.WriteLine("Opción 2");
                                        accessData.Add("Estado_Cuenta1", "Perdido");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Opción 3");
                                        accessData.Add("Estado_Cuenta1", "Cliente Rojo");
                                    }
                                    Console.WriteLine("Días : "+dias);
                                }
                                else
                                {
                                    Console.WriteLine("Cliente Rojo o Perido");
                                    var dias = (DateTime.Now - resultado.ultCompra.AddDays(resultado.dias)).Days;
                                    if (dias >= 90)
                                    {
                                        Console.WriteLine("Opción 4");
                                        accessData.Add("Estado_Cuenta1", "Perdido");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Opción 5");
                                        accessData.Add("Estado_Cuenta1", "Cliente Rojo");
                                    }
                                    Console.WriteLine("Días : " + dias);
                                }
                            }

                                accessData.Add("Rotaci_n", resultado.dias.CheckForZero());
                            

                            if (resultado.contador >= 3)
                            {
                                Console.WriteLine("Es Cliente , Recuperado , Cliente Rojo o Perdido");
                                Console.WriteLine(" Fechas ultima compra + dias : " + resultado.ultCompra.AddDays(resultado.dias));
                                Console.WriteLine(" Fechas Actual               : " + DateTime.Now);

                                if (resultado.ultCompra.AddDays(resultado.dias) >= DateTime.Now)
                                {
                                    
                                    if (resultado.Estado_Cuenta1 != "Cliente Nuevo" && resultado.Estado_Cuenta1 != "Cliente") {
                                        Console.WriteLine("Opción 6");
                                        accessData.Add("Estado_Cuenta1", "Recuperado");
                                        /*podriamos consultar por la fecha ingresada en crm y comprar si son distintas fehcas, podriamos
                                        calcular si es recuperado o cliente*/
                                    }
                                    else
                                    {
                                        Console.WriteLine("Opción 7");
                                        accessData.Add("Estado_Cuenta1", "Cliente");
                                    }

                                }
                                else if (resultado.Estado_Cuenta1 == "Cliente")
                                {
                                    
                                    var dias = (DateTime.Now - resultado.ultCompra.AddDays(resultado.dias)).Days;
                                    if (dias >= 90)
                                    {
                                        Console.WriteLine("Opción 8");
                                        accessData.Add("Estado_Cuenta1", "Perdido");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Opción 9");
                                        accessData.Add("Estado_Cuenta1", "Cliente Rojo");
                                    }
                                    Console.WriteLine("Días : " + dias);
                                }
                                else
                                {
                                    var dias = (DateTime.Now - resultado.ultCompra.AddDays(resultado.dias)).Days;
                                    if (dias >= 90)
                                    {
                                        Console.WriteLine("Opción 10");
                                        accessData.Add("Estado_Cuenta1", "Perdido");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Opción 11");
                                        accessData.Add("Estado_Cuenta1", "Cliente Rojo");
                                    }
                                    Console.WriteLine("Días : " + dias);
                                }


                            }

                        }                       
                        accessData.Add("Monto", resultado.Total);

                    }
                    arraString.Add(accessData);
                    //accessData.Clear();
                    count++;
                    countCiclos++;
                    Console.WriteLine("CodAux:" + resultado.C_digo_Auxiliar + " - id:" + resultado.id + " - Total:" + resultado.Total + " - " + resultado.dias + " - Fecha Ult Compra : " + resultado.ultCompra + " - Estado Cuenta : " + resultado.Estado_Cuenta1); ;

                    if (count == 90 || results.Count() == countCiclos)
                    {
                        controlador.ActualizarRegistro(arraString, tokenAux);
                        arraString.Clear();
                        //accessData.Clear();

                        count = 1;

                    }



                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR : "+ex);
            }

           
            Console.WriteLine("Paso 4");
            //////////////////////////////////////////////////////////////////
            Console.WriteLine("Enter para terminar");
            //Console.ReadKey();
        }
    }
}
