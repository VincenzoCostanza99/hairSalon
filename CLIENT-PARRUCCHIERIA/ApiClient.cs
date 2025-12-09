using System;
using System.Net.Http;

namespace CLIENT_PARRUCCHIERIA
{
    public static class ApiClient
    {
        //Creiamo un httpClient statico per evitare creazione di nuove istanze, mettiamo readonly perche una volta inizializzato non puo essere modificato
        public static readonly HttpClient client = new HttpClient();

        static ApiClient()
        {
            client.BaseAddress = new Uri("http://localhost:8080/");//questa è la base url del server
        }//in questo modo http client è condiviso tra tutti i form(senza bisogno di dichiararlo ovunque) e poi quando piu client si collegheranno ognuno avra il suo httpclient che sara unico per i 3 form

        public static void Shutdown()
        {
            client.Dispose();//di default cosi si prende come argomento true 
        }
    }
}
