using System;
using System.Windows.Forms;

namespace CLIENT_PARRUCCHIERIA
{
    public static class AppState {
        public static bool Chiudendo = false;//serve per la chiusura dell'applicazione. definisco questo attributo statico perche cosi ne posso fare uso nei 3 form e condividere il suo valore. questo lo uso quando devo fare l'application exit() quindi chiudere l'applicazione. questo attributo ha risolto il problema del fatto che quando aprivo il form prenotazioni e cliccavo sulla x in alto a destra o su esci allora compariva due volte "sei sicuro di voler uscire?"
    }

    public class Registrazione
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
      
        public Registrazione(string nome_utente, string cognome_utente, string email_utente, string password_utente)
        {
            Nome = nome_utente;
            Cognome = cognome_utente;
            Email = email_utente;
            Password = password_utente;
        }
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public Login(string email_utente, string password_utente)
        {
            Email = email_utente;
            Password = password_utente;
        }
    }
    
    public class Prenotazione {
        public string Email { get; set; }
        public string Giorno_prenotazione { get; set; }
        public string Ora_prenotazione { get; set; }
        public string Servizio { get; set; }
        public double Prezzo { get; set; }

        public Prenotazione(string email_utente, string giorno, string ora, string servizio_prenotazione, double prezzo_prenotazione)
        {
            Email = email_utente;
            Giorno_prenotazione = giorno;
            Ora_prenotazione = ora;
            Servizio = servizio_prenotazione;
            Prezzo = prezzo_prenotazione;
        }

        public Prenotazione(string email_utente, string giorno, string ora, string servizio_prenotazione)
        {
            Email = email_utente;
            Giorno_prenotazione = giorno;
            Ora_prenotazione = ora;
            Servizio = servizio_prenotazione;
        }

        public Prenotazione(string giorno, string ora)
        {
            Giorno_prenotazione = giorno;
            Ora_prenotazione = ora;
        }
    }
    
    
    
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();//application è una classe sealed mentre questi sono metodi statici. questo serve per abilitare i visual style per i controlli di winform 
            Application.SetCompatibleTextRenderingDefault(false);//imposta il comportamento di rendering del testo per alcuni controlli di winform
            Application.Run(new form_login());
        }
    }
}
