using Countries.Modelos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Services
{
    public class ApiService
    {
        public async Task<Response> GetCountries(string urlBase, string controller) //método que tem como tarefa devolver um objecto do tipo Response (a classe que criámos)
        {
            try
            {
                var client = new HttpClient();      //criar http para fazer ligação externa

                client.BaseAddress = new Uri(urlBase);  //especificar onde está o endereço base da API

                var response = await client.GetAsync(controller);    //vai buscar o controlador da API

                var result = await response.Content.ReadAsStringAsync();  //guarda os resultados no formato de string para o objecto result

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSucess = false,
                        Message = result,
                    };
                }

                var rates = JsonConvert.DeserializeObject<List<Country>>(result); //Converter o json para uma lista de dados do tipo Rate

                return new Response
                {
                    IsSucess = true,
                    Result = rates
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucess = false,
                    Message = ex.Message
                };
            }

        }
    }
}   
