using System;//libreria di base per funzionalita generali
using System.Text;//Utilizzato per la gestione dell'encoding UTF-8
using System.Windows.Forms;//Fornisce il supporto per l’interfaccia grafica Windows Forms.
using System.Net.Http;//Permette di inviare richieste HTTP al server.
using Newtonsoft.Json;

namespace CLIENT_PARRUCCHIERIA
{
    public partial class form_registrazione: Form
    {
        private bool chiusuraNormale = false; //chiusura senza uscire dall'app. questo attributo serve per chiudere il form senza chiudere l'applicazione
        public form_registrazione()
        {
            InitializeComponent();//È un metodo generato automaticamente che inizializza tutti gli elementi grafici del form
        }

        private async void buttonRegister_Click(object sender, EventArgs e)// Indica che il metodo eseguirà operazioni asincrone (come la richiesta HTTP). sender= Indica chi ha attivato l’evento (in questo caso, il pulsante di registrazione. e= parametri dell'evento
        {
            //adesso prendo i valori inseriti dall'utente nei campi di input(Trim() rimuove eventuali spazi all'inizio e alla fine del testo)
            string nome = textBoxNome.Text.Trim();
            string cognome = textBoxCognome.Text.Trim();
            string email = textBoxEmail.Text.Trim();
            string password = textBoxPassword.Text;
            string confermaPassword = textBoxConfPassword.Text;

            //CONTROLLO CHE NESSUN CAMPO SIA VUOTO
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cognome) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confermaPassword)) {
                MessageBox.Show("Tutti i campi sono obbligatori", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Blocca l'esecuzione se ci sono campi vuoti
            }

            //CONTROLLO SE L'EMAIL HA UN FORMATO VALIDO
            if (!email.Contains("@") || !email.Contains(".com")) {
                MessageBox.Show("Inserisci una mail valida", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //CONTROLLO CHE LA PASSWORD ABBIA ALMENO 8 CARATTERI
            if (password.Length < 8) {
                MessageBox.Show("La password deve avere almeno 8 caratteri", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confermaPassword) {
                MessageBox.Show("Le password non coincidono", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Registrazione UserData = new Registrazione(nome, cognome, email, password); //sto creando un oggetto della classe registrazione 

            //CONVERTIAMO L'OGGETTO IN FORMATO JSON PER INVIARLO AL SERVER
            string jsonData = JsonConvert.SerializeObject(UserData);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");//Questa riga di codice crea il corpo (payload) della richiesta HTTP POST che verrà inviata al server. encoding specifica che il testo deve essere codificato in formato UTF-8. l'altro usato per comunicare con il server e indica al server che i dati inviati sono in formato json quindi il server sapra che devono essere interpretati come json
            try
            {
                //invio della richiesta HTTP POST AL SERVER C++
                HttpResponseMessage response = await ApiClient.client.PostAsync("register", content);//qui metto solo register per l'uri è definito in apiclient. Invia una richiesta HTTP POST al server (PostAsync).Attende la risposta dal server senza bloccare l'interfaccia grafica (UI).Quando la risposta arriva, la variabile response la contiene. Senza await, il programma continuerebbe subito all'istruzione successiva senza aspettare la risposta del server, creando problemi. await viene usato per fermare momentaneamente l'esecuzione SOLO di questo metodo, senza bloccare l’interfaccia utente
                string result = await response.Content.ReadAsStringAsync();//legge il contenuto della risposta http dal server e lo converte in una stringa. l'await serve per aspettare che il contenuto venga letto completamente prima di continuare con il codice successivo

                //decofica il json di risposta
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);//qui converto una stringa json ricevuta dal server in un oggetto utilizzabile in c#. JsonConvert è una classe statica della libreria newtonsoft.json che chiama il metodo statico che converte una stringa json in un oggetto c# e tramite dynamic(guarda pdf 2 c#) non faccio i controlli a tempo di compilazione ma a runtime perche non so che di tipo si tratta. a runtime poi dovrebbe restituire un oggetto JObject della libreria Newtonsoft.json. quindi si converte la stringa json result in un oggetto navigabile json. A runtime, dynamic diventa un oggetto Newtonsoft.Json.Linq.JObject.

                //controllo il campo "success" nel json
                if (jsonResponse.success == true)
                {
                    MessageBox.Show("registrazione effettuata con successo", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Recupero il form_login "padre" e lo mostro di nuovo
                    if (this.Owner != null)
                    {
                        this.Owner.Show();//a questo punto mostro il form login che avevo nascosto
                    }
                    chiusuraNormale = true;//ponendolo uguale a true appena faccio this.Close() allora lui chiama il metodo formclosing ma siccome è settato a true non chiude l'applicazione!
                    
                    this.Close(); // chiudo il form di registrazione
                }
                else {
                    MessageBox.Show(jsonResponse.message.ToString(), "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }catch (Exception ex){//se c'è errore di connessione allora
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEsci_Click(object sender, EventArgs e)
        { 
            //mostriamo per prima cosa un messaggio per confermare l'uscita
            DialogResult result = MessageBox.Show("sei sicuro/a di voler uscire?", "CONFERMA", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {//se l'utente ha cliccato si 
                AppState.Chiudendo = true; //siccome application.exit chiama formclosing impostiamo questa variabile a true in modo tale che application.exit entrando in form_registrazione_FormClosing non fa rivedere il message box
                Application.Exit();//allora si chiude l'applicazione.
            }
        }

        private void form_registrazione_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppState.Chiudendo == false && chiusuraNormale == false)//mettiamo questa cosa perche senza di esso, se avessi chiamato l'application.exit avrebbe richiamato questo metodo per chiudere tutto e quindi in quel modo mi appariva due volte lo stesso messagebox 
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
