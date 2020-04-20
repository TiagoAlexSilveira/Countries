using Countries.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Serviços
{
    public class NetworkService //só tem 1 método para ver se estamos liagados à internet
    {
        public Response CheckConnection() // método para ver se temos conexão à internet
        {
            var client = new WebClient();

            try  //vai tentar ligar à internet
            {
                using (client.OpenRead("http://clients3.google.com/generate_204")) // se o ping a este website correr bem
                {
                    return new Response //retorna uma reposta nova
                    {
                        IsSucess = true
                    };
                }
            }
            catch //se não conseguir ligar à internet
            {
                return new Response
                {
                    IsSucess = false,
                    Message = "Configure a sua ligação à Internet"
                };
            }
        }
    }
}
