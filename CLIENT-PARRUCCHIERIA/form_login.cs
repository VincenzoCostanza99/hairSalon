using System;
using System.Text; //usato per gestire l'encoding(UTF-8 PER JSON)
using System.Windows.Forms;//per creare interfacce grafiche windows form
using System.Net.Http;
using Newtonsoft.Json;//per inviare richieste http al server

namespace CLIENT_PARRUCCHIERIA
{
    public partial class form_login: Form// Dichiarazione della classe LoginForm, che eredita da Form
    {
        public form_login()
        {
            InitializeComponent();
        }


        //questo è un metodo che è chiamato quando l'utente preme il pulsante accedi
        private async void buttonAccedi_Click(object sender, EventArgs e)
        {
            //ottengo il giorno attuale e controllo se è domenica, se lo è, non faccio accedere perche do tempo all'admin di aggiungere le disponibilita
            string giorno_attuale = DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("it-IT"));//dddd è il giorno della settimana esteso

            //se oggi è domenica, blocca l'accesso
            if (giorno_attuale.ToLower()=="domenica")
            {
                MessageBox.Show("Per prenotarti devi aspettare domani, quando l'amministratore aggiornerà gli orari disponibili.","Accesso Bloccato",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }

            //prima recupero i dati inseriti dall'utente nei campi email e password
            string email = textBoxEmail.Text.Trim();//ricordiamo che trim serve per rimuovere gli spazi
            string password = textBoxPassword.Text;//qui non ho messo trim per evitare problemi con gli spazi

            //prima cosa controllo che i campi non siano vuoti
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
                MessageBox.Show("Tutti i campi sono obbligatori", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //controllo se l'email ha un formato valido
            if (!email.Contains("@") || !email.Contains(".com")) { 
                MessageBox.Show("Inserisci un'email valida", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //passo alla creazione dell'oggetto json con i dati dell'utente(questo è un oggetto della classe login)
            Login loginData = new Login(email,password);

            //lo converto in formato json
            string JsonData = JsonConvert.SerializeObject(loginData);

            //preparo il contenuto da inviare al server: Json con cofica utf-8
            var content = new StringContent(JsonData, Encoding.UTF8, "application/json");
            
            try {
                //invio della richiesta http post al server per effettuare il login
                HttpResponseMessage response = await ApiClient.client.PostAsync("login", content);//qui metto solo login perche l'uri l'ho definito nel costruttore di ApiClient

                //legge la risposta del server come string
                string result = await response.Content.ReadAsStringAsync();

                //la spiegazione di questa riga è nel form registrazione
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);

                //controllo che il campo success è true
                if (jsonResponse.success == true)
                {
                    MessageBox.Show("Login effettuato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form_prenotazioni prenotazioni = new form_prenotazioni(email);
                    prenotazioni.Owner = this;
                    prenotazioni.Show();//mi fa vedere il form di prenotazioni. chiamando show una volta terminato, di esso viene chiamato in modo automatico il dispose(visto in documentazione). dalla documentazione: Dispose verrà chiamato automaticamente se il modulo viene visualizzato usando il Show metodo .
                    this.Hide();//nascondo il form di login
                }else {
                    MessageBox.Show(jsonResponse.message.ToString(), "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }catch (Exception ex) {//se è presente un errore di connessione
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Metodo chiamato quando l'utente clicca sul pulsante registrati
        private void buttonRegistrazione_Click(object sender, EventArgs e)
        {
            form_registrazione form_reg = new form_registrazione();//crea un'istanza del form di registrazione
            form_reg.Owner = this;//owner è una proprietà e all'interno di essa metto il form login in modo tale che quando devo ritornare nel form login non serve creare un'altra istanza di form login ma mostro quella gia presente che era nascosta(hide). in questo caso this si riferisce all'istanza corrente del form
            form_reg.Show();//mi fa vedere il form di registrazione. chiamando show una volta terminato, di esso viene chiamato in modo automatico il dispose.
            this.Hide();//usiamo hide per nasconderlo 
        }

        private void buttonEsci_Click(object sender, EventArgs e){
            //mostriamo per prima cosa un messaggio per confermare l'uscita
            DialogResult result = MessageBox.Show("sei sicuro/a di voler uscire?", "CONFERMA", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {//se l'utente ha cliccato si 
                AppState.Chiudendo = true; //siccome application.exit chiama formclosing impostiamo questa variabile a true in modo tale che application.exit entrando in form_registrazione_FormClosing non fa rivedere il message box
                Application.Exit();//allora si chiude l'applicazione. 
            }
        }

        private void form_login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppState.Chiudendo == false)//mettiamo questa cosa perche senza di esso, se avessi chiamato l'application.exit avrebbe richiamato questo metodo per chiudere tutto e quindi in quel modo mi appariva due volte lo stesso messagebox 
            {//se è la prima volta che l'evento viene chiamato allora
             //mostriamo per prima cosa un messaggio per confermare l'uscita
                DialogResult result = MessageBox.Show("sei sicuro/a di voler uscire?", "CONFERMA", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {//se l'utente ha cliccato si 
                    AppState.Chiudendo = true;
                    Application.Exit();//allora si chiude l'applicazione
                }
                else
                {
                    e.Cancel = true;//annulla la richiesta
                }
            }
        }
    }
}
