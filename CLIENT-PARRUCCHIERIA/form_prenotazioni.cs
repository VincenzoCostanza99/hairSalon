using System;//libreria per funzionalita di base
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks; //Libreria per gestire operazioni asincrone con "await"
using System.Windows.Forms; // Libreria per creare interfacce grafiche con Windows Forms
using System.Net.Http;//libreria per effettuare richieste http
using Newtonsoft.Json; // Libreria per gestire JSON con oggetti JObject

namespace CLIENT_PARRUCCHIERIA
{
    public partial class form_prenotazioni: Form
    {
        private string email_utente;//serve per memorizzare l'email dell'utente loggato
        private bool chiusuraNormale = false; //chiusura senza uscire dall'app

        //costruttore che riceve l'email dell'utente
        public form_prenotazioni(string email)
        {
            InitializeComponent();
            email_utente = email;//salviamo l'email dell'utente passato dal form di login/registrazione
            labelBenvenuto.Text = $"Benvenuto, {email_utente}";
            //associo l'evento load per chiamare caricadisponibilita in modo asincrono con await perche aspetta che dia un risultato questo metodo asincrono. non posso mettere nel costruttore direttamente await caricadisponibilita ma devo fare in questo modo:
            this.Load += async(s,e) => await CaricaDisponibilita();//(s sta per object sender mentre e è l'eventArgs e). this.load è l'evento del form attuale quando viene creato il form stesso. è un evento che cosi associo all'evento load il metodo asincrono caricadisponibilita.qui è come se mettessi this.Load += new eventHandler(async(s,e) => await CaricaDisponibilita()); infatti questa è una lambda expressione che è del tipo private async void nome_metodo. qui è giusto mettere async void perche si mette solo negli eventhandler ed esso lo è. metto await caricadisponibilita cosi mi metto in attesa del risultato prima di andare avanti
            AssociaEventiCheckbox();//associa gli eventi a tutti i checkbox
        }

        //facciamo un metodo asincrono per recuperare le disponibilità dal server
        private async Task CaricaDisponibilita(){
            try {
                //usiamo apiclient.client per fare la richiesta get al server c++
                HttpResponseMessage response = await ApiClient.client.GetAsync("disponibilita");//qui metto solo disponibilita perche l'uri del server è messo in apiclient
                //leggo la risposta come stringa json
                string result = await response.Content.ReadAsStringAsync();//legge il contenuto della risposta http dal server e lo converte in una stringa. l'await serve per aspettare che il contenuto venga letto completamente prima di continuare con il codice successivo

                //decodifico il json di risposta
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);//spiegazione di questa riga in form_registrazione
                Console.WriteLine(jsonResponse.GetType()); // Stampa il tipo a runtime

                //primo controllo: errore sql o connessione
                if (jsonResponse.success == false) {
                    MessageBox.Show(jsonResponse.message.ToString(), "Errore del server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                //mappa dei checkbox per ogni giorno della settimana
                Dictionary<string, List<CheckBox>> checkBoxMap = new Dictionary<string, List<CheckBox>> {
                    {"Martedi",new List<CheckBox>{ checkBoxMartedi1,checkBoxMartedi2,checkBoxMartedi3,checkBoxMartedi4,checkBoxMartedi5} },
                    {"Mercoledi",new List<CheckBox>{checkBoxMercoledi1,checkBoxMercoledi2,checkBoxMercoledi3,checkBoxMercoledi4,checkBoxMercoledi5 } },
                    {"Giovedi", new List<CheckBox>{ checkBoxGiovedi1,checkBoxGiovedi2,checkBoxGiovedi3,checkBoxGiovedi4,checkBoxGiovedi5} },
                    {"Venerdi", new List<CheckBox>{ checkBoxVenerdi1,checkBoxVenerdi2,checkBoxVenerdi3,checkBoxVenerdi4,checkBoxVenerdi5} },
                    {"Sabato",new List<CheckBox>{ checkBoxSabato1,checkBoxSabato2,checkBoxSabato3,checkBoxSabato4,checkBoxSabato5} }
                };

                //andiamo a resettare ogni checkbox
                foreach (var giorno in checkBoxMap.Keys) {
                    foreach (var checkBox in checkBoxMap[giorno]) {
                        checkBox.Text = "";//inizializzo a testo vuoto
                        checkBox.Enabled = true;//rendo cliccabile
                        checkBox.Checked = false;//deseleziono
                        checkBox.Tag = giorno;//assegnamo il giorno come tag del checkbox che ci servira per prenderci il nome del giorno in cui l'utente vuole prenotarsi quindi lo useremo per mandare il dato al server quando manderemo la richiesta di prenotazione
                    }                    
                }

                Dictionary<string, List<string>> disponibilitaPerGiorno = new Dictionary<string, List<string>>();
                
                //inizializziamo il dizionario mettendo le chiavi e per ognuna di essa una lista di stringhe
                foreach (var giorno in checkBoxMap.Keys) {
                    disponibilitaPerGiorno[giorno] = new List<string>();
                }

                //adesso andiamo a riempire effettivamente il dizionario
                foreach(var item in jsonResponse.disponibilita){
                    string giorno = item.giorno_disponibile.ToString();
                    string ora = item.ora_disponibile.ToString();

                    if (disponibilitaPerGiorno.ContainsKey(giorno)) {
                        disponibilitaPerGiorno[giorno].Add(ora);                        
                    }
                }

                //andiamo ad inserire gli orari disponibili ai checkbox corrispondenti con i relativi controlli del caso
                foreach (var giorno in checkBoxMap.Keys) {
                    var checkBoxList = checkBoxMap[giorno];//lista dei checkbox oer il giorno
                    var orari = disponibilitaPerGiorno[giorno];//orari disponibili
                    
                    if (orari.Count == 0)
                    {//se non ci sono orari disponibili per quel giorno, segno ogni checkbox con "già occupato" 
                        foreach (var checkBox in checkBoxList)
                        {
                            checkBox.Text = "orario non disponibile";
                            checkBox.Enabled = false;
                        }
                    }else {//altrimenti ci sono due casi che andiamo a trattare che sono: ci sono tutti gli orari disponibili, ci sono solo alcuni orari disponibili(per quel giorno ovviamente) 
                        //assegno gli orari disponibili ai checkbox
                        for(int i = 0; i < checkBoxList.Count; i++){
                            if (i<orari.Count){//controllo utile per capire se sono finiti gli orari disponibili
                                checkBoxList[i].Text = orari[i];//assegna l'orario
                                checkBoxList[i].Enabled = true;//abilita il checkbox
                            }else{
                                checkBoxList[i].Text = "orario non disponibile";
                                checkBoxList[i].Enabled = false;//disabilito il checkbox
                            }
                        }

                    }
                }
                //Dopo aver caricato le disponibilità, disabilitiamo i giorni passati
                DisabilitaCheckBoxPrecedenti();
            }
            catch (Exception ex) {
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //metodo per disabilitare i checkbox dei giorni precedenti al giorno attuale
        private void DisabilitaCheckBoxPrecedenti()
        {
            //ottengo per prima cosa il giorno attuale in italiano
            string giornoAttuale = DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("it-IT")).ToLower();

            // Creiamo una lista con i giorni validi per la prenotazione (da Martedì a Sabato)
            List<string> giorniSettimana = new List<string> { "martedì", "mercoledì", "giovedì", "venerdì", "sabato" };

            // Troviamo l'indice del giorno attuale nella lista dei giorni validi
            int indiceGiornoAttuale = giorniSettimana.IndexOf(giornoAttuale);//quindi martedi sara 0, mercoledi 1 e cosi via

            // Se il giorno attuale non è presente nella lista (es. Domenica o Lunedì), interrompiamo la funzione
            if (indiceGiornoAttuale == -1) return;

            // Creiamo un dizionario che associa ogni giorno della settimana a un pannello specifico
            Dictionary<string, Panel> panelMap = new Dictionary<string, Panel>
            {
                { "martedì", panelMartedi },
                { "mercoledì", panelMercoledi },
                { "giovedì", panelGiovedi },
                { "venerdì", panelVenerdi },
                { "sabato", panelSabato }
            };

            // Iteriamo su tutti i giorni precedenti al giorno attuale
            for (int i = 0; i < indiceGiornoAttuale; i++)//se è martedi non entra proprio nel for
            {
                // Otteniamo il nome del giorno da disabilitare
                string giorno = giorniSettimana[i]; //per i=0 assume martedi ecc
                // Verifichiamo se il giorno esiste nel dizionario (per sicurezza)
                if (panelMap.ContainsKey(giorno))
                {
                    // Recuperiamo il pannello associato a quel giorno
                    Panel panel = panelMap[giorno];

                    //Disabilitiamo tutti i checkbox presenti all'interno del pannello
                    foreach (Control subcontrol in panel.Controls) // Iteriamo solo sui controlli del pannello specifico
                    {
                        if (subcontrol is CheckBox checkBox)
                        {
                            checkBox.Enabled = false; // Disabilitiamo il checkbox
                        }
                    }
                    //Se il pannello ha già un messaggio, lo rimuoviamo per evitare duplicati
                    if (panel.Tag is Label)
                    {
                        this.Controls.Remove(((Label)panel.Tag)); // Rimuovo il messaggio precedente
                        ((Label)panel.Tag).Dispose(); // Libero la memoria della vecchia label
                    }
                    //creo una nuova etichetta per il messaggio di avviso
                    Label lblMessaggio = new Label();
                    lblMessaggio.Text = "Non è possibile prenotare per questo giorno.";
                    lblMessaggio.ForeColor = Color.Red; // Colore rosso per il messaggio di errore
                    lblMessaggio.Font = new Font("Arial", 6, FontStyle.Bold); // Font in grassetto per evidenziare
                    lblMessaggio.AutoSize = true; // Adatta la dimensione automaticamente al testo

                    //Posizioniamo il messaggio sotto il pannello corrispondente
                    lblMessaggio.Location = new Point(panel.Left, panel.Bottom + 5);

                    // Aggiungiamo la label al form
                    this.Controls.Add(lblMessaggio);

                    //Memorizzo la label nel Tag del pannello per una futura rimozione
                    panel.Tag = lblMessaggio;
                }
            }
        }

        //metodo che associa l'evento checkedChanged a tutti i checkbox presenti nel form
        private void AssociaEventiCheckbox(){
            foreach (Control control in this.Controls) {//this.Controls è una collezione che contiene tutti i controlli presenti nel form(TUTTI quindi bottoni,checkbox,label ecc)
                //siccome i checkbox sono dentro panel allora devo entrare all'interno di ogni panelpanel e poi cercare i checkbox
                if (control is Panel){// questo è un controllo di matching, se è checkbox allora associo il metodo Checkbox_Changed all'evento checkedChanged di ogni checkbox ogni volta che ne trovo uno
                    foreach (Control subcontroll in control.Controls) {//vado a scorrere tutti controlli contenuti in panel
                        if(subcontroll is CheckBox){
                            ((CheckBox)subcontroll).CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
                        }    
                    }
                }                
            }
        }

        //metodo che viene eseguito quando l'utente seleziona un checkbox presente nel form
        private async void CheckBox_CheckedChanged(object sender, EventArgs e)
        {// qui metto async void perche di conseguenza chiamo async Task InviaPrenotazioneAlServer e quindi per usare await lo devo mettere. per gli eventhandler si mette async void, per tutto il resto async Task
            //convertiamo il sender in checkbox 
            CheckBox checkBox = sender as CheckBox;//converto il sender in checkbox
            if (checkBox != null)
            {
                if (checkBox.Checked)
                { //controllo se il checkbox è stato cliccato
                    string servizio = MostraSceltaServizio();//Mostra la finestra di selezione del servizio
                    if (string.IsNullOrEmpty(servizio))
                    {//se l'utente non ha selezionato alcun servizio andiamo a deselezionare il checkbox
                        checkBox.Checked = false;
                        return;
                    }
                    string giorno_prenotazione = checkBox.Tag as string;
                    string ora_prenotazione = checkBox.Text; 
                    if (giorno_prenotazione != null) {
                        //altrimenti ha scelto un servizio e chiediamo conferma all'utente prima di procedere con la prenotazione
                        DialogResult result = MessageBox.Show($"Confermi la prenotazione per {servizio} alle {checkBox.Text}?", "Conferma Prenotazione", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            await InviaPrenotazioneAlServer(email_utente, giorno_prenotazione, checkBox.Text, servizio, checkBox);//invia i dati al server
                        }
                        else
                        {
                            checkBox.Checked = false;
                        }
                    }
                }
            }
        }

        //metodo per inviare la prenotazione al server
        private async Task InviaPrenotazioneAlServer(string email, string giorno_prenotazione, string orario, string servizio, CheckBox checkBox)
        {
            double prezzo;
            if (servizio=="Piega")
            {
                prezzo = 20;
            }else if (servizio=="Taglio")
            {
                prezzo = 60;
            }else if (servizio == "Colore")
            {
                prezzo = 80;
            }
            else
            {
                prezzo = 0;
            }
                
            //creiamo un oggetto di classe prenotazione
            Prenotazione dati = new Prenotazione(email, giorno_prenotazione, orario, servizio, prezzo);

            string json = JsonConvert.SerializeObject(dati);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await ApiClient.client.PostAsync("Add_prenotazione", content);
                string result = await response.Content.ReadAsStringAsync();

                //decodifica il json di risposta 
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);//qui converto una stringa json ricevuta dal server in un oggetto utilizzabile in c#. JsonConvert è una classe statica della libreria newtonsoft.json che chiama il metodo statico che converte una stringa json in un oggetto c# e tramite dynamic(guarda pdf 2 c#) non faccio i controlli a tempo di compilazione ma a runtime perche non so che di tipo si tratta. a runtime poi dovrebbe restituire un oggetto JObject della libreria Newtonsoft.json. quindi si converte la stringa json result in un oggetto navigabile json. A runtime, dynamic diventa un oggetto Newtonsoft.Json.Linq.JObject.

                //controllo il campo success nel json
                if (jsonResponse.success==true){
                    MessageBox.Show("Prenotazione effettuata con successo", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //deseleziono il checkbox
                    checkBox.Checked = false;
                    //rimuovo l'orario prenotato dalla lista delle disponibilità
                    await CaricaDisponibilita();
                }
                else{
                    MessageBox.Show("Errore nella prenotazione"+jsonResponse.message);
                    checkBox.Checked = false;
                }
            }catch (Exception ex){//se c'è errore di connessione allora
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

        }

        private void buttonEsci_Click(object sender, EventArgs e)
        {
            if (AppState.Chiudendo == false)//mettiamo questa cosa perche senza di esso, se avessi chiamato l'application.exit avrebbe richiamato questo metodo per chiudere tutto e quindi in quel modo mi appariva due volte lo stesso messagebox 
            {//se è la prima volta che l'evento viene chiamato allora
             //mostriamo per prima cosa un messaggio per confermare l'uscita
                DialogResult result = MessageBox.Show("sei sicuro/a di voler uscire?", "CONFERMA", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {//se l'utente ha cliccato si 
                    AppState.Chiudendo = true;
                    ApiClient.Shutdown();//chiamo il dispose di apiclient
                    Application.Exit();//allora si chiude l'applicazione
                }
            }
        }

        //metodo per mostrare i servizi
        private string MostraSceltaServizio(){
            string[] servizi = { "Taglio", "Colore", "Piega" };
            Form form = new Form{
                Text = "Seleziona un servizio",
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                Width = 300,
                Height = 300
            };
            ListBox listBox = new ListBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Arial", 12, FontStyle.Regular),
                BackColor = Color.LemonChiffon,
                Height = 300,
                Margin = new Padding(10),
            };//sto creando una list box che va a coprire tutto il form

            listBox.Items.AddRange(servizi);

            //associamo il metodo all'evento
            listBox.SelectedIndexChanged += new EventHandler(ListBox_SelectedIndexChanged);//qui Registra un metodo (ListBox_SelectedIndexChanged) per gestire l'evento SelectedIndexChanged. += è usato per aggiungere un gestore di eventi. SelectedIndexChanged è un evento -> si attiva quando cambia la selezione nella ListBox e collega il metodo ListBox_SelectedIndexChanged all'eventoUn EventHandler è un delegato, cioè un "puntatore a un metodo".new EventHandler(ListBox_SelectedIndexChanged) significa: "Quando cambia la selezione, esegui il metodo ListBox_SelectedIndexChanged. qui gli metto questo metodo dentro le parentesi () perche significa che questo metodo restituisce un void e ha come parametri object sender e eventsArgs e perche il delegate event handler è definito come public delegate void EventHandler(object sender, EventArgs e); e quindi puo può contenere qualsiasi metodo con questa firma: void MetodoDiProva(object sender, EventArgs e) { }  

            //adesso aggiungo la listbox ai controlli del form quindi lo metto dentro il form
            form.Controls.Add(listBox);

            //mostra la finestra e si attende che l'utente selezioni un'opzione
            form.ShowDialog();//qui mettiamo showdialog e non show perche cosi si mostra il form ma blocca l'esecuzione del codice fino a quando la finestra non viene chiusa. Quando il Form viene chiuso, il codice continua da dove era rimasto bloccato. con show il codice continua immediatamente, anche se la finestra è ancora aperta

            //recuperiamo la scelta dell'utente dal tag del form perche il risultato viene messo nel tag.
            string scelta;
            if (form.Tag != null)
            {
                scelta = form.Tag.ToString();
            }else{
                scelta = "";
            }

            //liberiamo la memoria eliminando il form
            form.Dispose();

            //restiuisce la scelta dell'utente
            return scelta;
        }

        //metodo che viene chiamato quando l'utente seleziona un opzione nella listbox(tramite new eventhandler--> eventhandler è un delegato che associa il verificarsi dell'evento al metodo che lo gestisce. il delegato EVENTHANDLER è SEMPRE PRESENTE, OGNI VOLTA CHE CLICCHIAMO UN BOTTONE LUI DI DEFAULT CI COLLEGA AL METODO CHE GESTISCE TALE EVENTO!!)
        private void ListBox_SelectedIndexChanged(object sender, EventArgs e) {
            //convertiamo l'oggetto sender in listbox(se non è null)
            ListBox listBox = sender as ListBox;
            //controllo che la listbox non sia null e che appartenga ad un form
            if (listBox != null && listBox.Parent is Form && listBox.SelectedItem != null)
            {
            //memorizzo la scelta dell'utente nel tag del form
            ((Form)listBox.Parent).Tag = listBox.SelectedItem.ToString();
            //chiudo il form dopo la selezione
            ((Form)listBox.Parent).Close();
            }
        }

        private async void buttonVisualizzaPrenotazioni_Click(object sender, EventArgs e)
        {
            try {
                //effettuo una richiesta GET al server per ottenere le prenotazioni dell'utente
                HttpResponseMessage response = await ApiClient.client.GetAsync($"prenotazioni?email={email_utente}");
                string result = await response.Content.ReadAsStringAsync();

                //decodifico il json di risposta ricevuto dal server
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);

                //controllo se la richiesta del server non è andata a buon fine
                if (jsonResponse.success == false)
                {
                    MessageBox.Show(jsonResponse.message.ToString(), "Errore del server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //controllo se la richiesta del server non ha dato nessun risultato e quindi non ci sono prenotazioni per quell'utente
                if (jsonResponse.success==true && jsonResponse.vuoto==true)
                {
                    MessageBox.Show(jsonResponse.message.ToString(),"AVVISO", MessageBoxButtons.OK);
                    return;
                }

                //creiamo una lista di prenotazioni da mostrare nella finestra
                List<Prenotazione> prenotazioni = new List<Prenotazione>();//abbiamo creato una lista di Prenotazione che è la nostra classe

                //iteriamo su ogni prenotazione ricevuta dal server e formattiamo le informazioni
                foreach (var prenotazione in jsonResponse.prenotazioni)
                {
                    Prenotazione nuovaPrenotazione = new Prenotazione(
                        email_utente,
                        prenotazione.giorno_prenotazione.ToString(),
                        prenotazione.ora_prenotazione.ToString(),
                        prenotazione.servizio.ToString()
                    );
                    prenotazioni.Add(nuovaPrenotazione);//aggiungo la prenotazione alla lista
                }
                //apro la finestra per mostrare le prenotazioni all'utente
                MostraPrenotazioni(prenotazioni);
            }catch (Exception ex) {
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string RimuoviAccentoI(string testo)
        {
            return testo.Replace("ì", "i");//controlla la stringa e sostituisce la lettera ì con i, se è presente, altrimenti rimane invariata
        }

        //metodo per creare e mostrare uan finestra con le prenotazioni
        private void MostraPrenotazioni(List<Prenotazione> prenotazioni)
        {
            //otteniamo il giorno attuale in italiano
            string giornoAttuale = RimuoviAccentoI(DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("it-IT")).ToLower());
            
            //lista dei giorni validi per questa settimana
            List<string> giorniSettimana=new List<string> { "martedi", "mercoledi", "giovedi", "venerdi", "sabato" };

            //se è domenica blocchiamo tutto. è un controllo per rendere il tutto uniforme ma non permettiamo l'accesso agli utenti la domenica
            if (giornoAttuale == "domenica")
            {
                MessageBox.Show("non puoi accedere alle prenotazioni di domenica", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<Prenotazione> prenotazioniFiltrate = new List<Prenotazione>();

            //se è lunedi, mostriamo tutte le prenotazioni, se ci sono, della settimana che sta per entrare
            if (giornoAttuale == "lunedi")
            {
                foreach(var prenotazione in prenotazioni)
                {
                    prenotazioniFiltrate.Add(prenotazione);//qui mettiamo tutto il contenuto di prenotazioni in prenotazioniFiltrate per evitare di scrivere codice inerente al form due volte(uno nell'if e uno nell'else)
                }
            }
            else{ //altrimenti entriamo qui quando è da martedi a sabato e mostriamo solo le prenotazioni del giorno corrente e dei giorni successivi 
                //troviamo l'indice del giorno attuale nella lista dei giorni validi
                int indiceGiornoAttuale = giorniSettimana.IndexOf(giornoAttuale);
                
                foreach(var prenotazione in prenotazioni)
                {
                    string giornoPrenotazione = prenotazione.Giorno_prenotazione.ToLower();
                    int indiceGiornoPrenotazione = giorniSettimana.IndexOf(giornoPrenotazione);

                    //se la prenotazione è per il giorno attuale o per un giorno successivo al giorno corrente, la aggiungo alla lista filtrata
                    if (indiceGiornoPrenotazione >= indiceGiornoAttuale)
                    {
                        prenotazioniFiltrate.Add(prenotazione);
                    }
                }
            }

            //se, dopo il filtro, non ci sono prenotazioni da mostrare , informiamo l'utente di cio
            if (prenotazioniFiltrate.Count == 0)
            {
                MessageBox.Show("Non ci sono prenotazioni disponibili.", "Nessuna prenotazione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //creazione del form
            Form form = new Form
            {
                Text = "Le tue prenotazioni",
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                Width = 400,
                Height = 400
            };

            // Creo una ListView per mostrare le prenotazioni con le icone
            ListBox listBox = new ListBox
            {
                Dock = DockStyle.Fill,//occupa tutto lo spazio disponibile nel form
                Font = new Font("Arial", 12, FontStyle.Regular),
                BackColor = Color.LemonChiffon,
                BorderStyle = BorderStyle.None
            };

            //vado a "popolare" la listbox  con le prenotazioni
            foreach (var prenotazione in prenotazioniFiltrate)
            {
                listBox.Items.Add($"{prenotazione.Giorno_prenotazione} - {prenotazione.Ora_prenotazione} - {prenotazione.Servizio}");
            }

            //aggiungo i controlli al form 
            form.Controls.Add(listBox);

            //mostro la finestra con showdialog per bloccare l'esecuzione fino alla chiusura 
            form.ShowDialog();
            form.Dispose();

        }

        private async void buttonRimoviPrenotazione_Click(object sender, EventArgs e)
        {
            try
            {
                //effettuo una richiesta GET al server per ottenere le prenotazioni dell'utente
                HttpResponseMessage response = await ApiClient.client.GetAsync($"prenotazioni?email={email_utente}");
                string result = await response.Content.ReadAsStringAsync();

                //decodifico il json di risposta ricevuto dal server
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);

                //controllo se la richiesta del server non ha dato risultati(nessuna prenotazione)
                if (jsonResponse.success == true && jsonResponse.vuoto == true)
                {
                    MessageBox.Show("Non ci sono prenotazioni da poter rimuovere", "Nessuna prenotazione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //creo una lista di prenotazioni da mostrare nella finestra
                List<Prenotazione> prenotazioni = new List<Prenotazione>();

                //itero su oni prenotazione ricevuta dal server e formattiamo le informazioni
                foreach (var prenotazione in jsonResponse.prenotazioni) {
                    Prenotazione nuovaPrenotazione = new Prenotazione(
                        email_utente,
                        prenotazione.giorno_prenotazione.ToString(),
                        prenotazione.ora_prenotazione.ToString(),
                        prenotazione.servizio.ToString()
                    );
                    prenotazioni.Add(nuovaPrenotazione);
                }

                //mostro una finestra con la lista delle prenotazioni selezionabili
                MostraSelezionePrenotazioni(prenotazioni);

            }
            catch (Exception ex) {
                MessageBox.Show("errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostraSelezionePrenotazioni(List<Prenotazione> prenotazioni) {
            //otteniamo il giorno attuale, rimuovendo eventuali accenti
            string giornoAttuale = RimuoviAccentoI(DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("it-IT")).ToLower());

            //lista dei giorni validi per questa settimana
            List<string> giorniSettimana = new List<string> { "martedi", "mercoledi", "giovedi", "venerdi", "sabato" };

            //se è domenica blocchiamo tutto. è un controllo per rendere il tutto uniforme ma non permettiamo l'accesso agli utenti la domenica
            if (giornoAttuale == "domenica")
            {
                MessageBox.Show("non puoi rimuovere le prenotazioni di domenica", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<Prenotazione> prenotazioniFiltrate = new List<Prenotazione>();

            //se è lunedi, mostriamo tutte le prenotazioni, se ci sono, della settimana che sta per entrare
            if (giornoAttuale == "lunedi")
            {
                foreach (var prenotazione in prenotazioni)
                {
                    prenotazioniFiltrate.Add(prenotazione);//qui mettiamo tutto il contenuto di prenotazioni in prenotazioniFiltrate per evitare di scrivere codice inerente al form due volte(uno nell'if e uno nell'else)
                }
            }
            else
            { //altrimenti entriamo qui quando è da martedi a sabato e mostriamo solo le prenotazioni del giorno corrente e dei giorni successivi 
              //troviamo l'indice del giorno attuale nella lista dei giorni validi
                int indiceGiornoAttuale = giorniSettimana.IndexOf(giornoAttuale);

                foreach (var prenotazione in prenotazioni)
                {
                    string giornoPrenotazione = prenotazione.Giorno_prenotazione.ToLower();
                    int indiceGiornoPrenotazione = giorniSettimana.IndexOf(giornoPrenotazione);

                    //se la prenotazione è per il giorno attuale o per un giorno successivo al giorno corrente, la aggiungo alla lista filtrata
                    if (indiceGiornoPrenotazione >= indiceGiornoAttuale)
                    {
                        prenotazioniFiltrate.Add(prenotazione);
                    }
                }
            }

            //se, dopo il filtro, non ci sono prenotazioni da mostrare , informiamo l'utente di cio
            if (prenotazioniFiltrate.Count == 0)
            {
                MessageBox.Show("Non ci sono prenotazioni da poter rimuovere", "Nessuna prenotazione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //creo per prima cosa un form
            Form form = new Form
            {
                Text = "Seleziona una prenotazione da rimuovere",
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                Width = 400,
                Height = 400
            };
            //adesso creo una listview
            ListView listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details, //mostra i dettagli con le colonne
                FullRowSelect = true, // Seleziona l'intera riga quando si clicca su un elemento
                BackColor = Color.LemonChiffon
            };

            listView.Columns.Add("Prenotazioni",400);//400 quindi prende l'intera riga

            //vado a "popolare" la listview con le prenotazioni
            foreach (var prenotazione in prenotazioniFiltrate)
            {
                ListViewItem item = new ListViewItem();
                item.Text= $"{prenotazione.Giorno_prenotazione} - {prenotazione.Ora_prenotazione} - {prenotazione.Servizio}";//ho aggiunto nel text i dettagli della prenotazioni
                item.Tag = prenotazione;//salvo l'oggetto intero nel tag per recuperare tutti i dati in seguito
                listView.Items.Add(item);
            }
            //volevamo mettere un evento per ciascun item ma non si puo fare, si deve assegnare a listview e in funzione dei metodi trovare l'item che si desidera eliminare
            //qui associamo un evento al listview in totale non singolo item della listview perche non è possibile farlo, poi tramite il metodo andro a recuperare l'item desiderato che voglio eliminare!! abbiamo messo mouseeventhandler (che associa un evento al click) al posto di eventhandler per far si che se voglio eliminare una prenotazione devo cliccare sulla riga desiderata
            listView.MouseClick += new MouseEventHandler(listView_MouseClick);

            //creo un pulsante per chiudere
            Button btnEsci = new Button
            {
                Text = "Indietro",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.Tan,
                FlatStyle = FlatStyle.Flat
            };

            btnEsci.Click += new EventHandler(BtnEsci_Click);

            //aggiungo i controlli
            form.Controls.Add(listView);
            form.Controls.Add(btnEsci);
            
            form.ShowDialog();
            form.Dispose();
        }

        //button esci di rimozione prenotazione
        private void BtnEsci_Click(object sender, EventArgs e)
        {
            //convertiamo l'oggetto sender in listbox(se non è null)
            Button bottone = sender as Button;
            //controllo che il button non sia null e che appartenga ad un form
            if (bottone != null && bottone.Parent is Form)
            {
                //chiudo il form
                ((Form)bottone.Parent).Close();
            }
        }

        //metodo di gestione dell'evento mouseclick
        private async void listView_MouseClick(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null)
            {
                //visto che non possiamo associare un click di ogni item ad un metodo ma solo all'intera listview dobbiamo trovare la riga listviewitem su cui l'utente ha cliccato facendo
                ListViewItem itemSelezionato = listView.GetItemAt(e.X, e.Y);//cosi ottengo l'elemento selezionato. questo metodo restituisce l'item selezionato-> Se non è presente alcun elemento in corrispondenza della posizione specificata, viene restituito null.
                if (itemSelezionato != null){  
                    Prenotazione prenotazioneSelezionata = itemSelezionato.Tag as Prenotazione;//recupero l'oggetto inerente alla prenotazione, se sbaglio conversione restituisce null
                    if (prenotazioneSelezionata != null)
                    {
                        DialogResult result = MessageBox.Show($"Sei sicuro di voler eliminare la prenotazione: {prenotazioneSelezionata.Giorno_prenotazione} - {prenotazioneSelezionata.Ora_prenotazione} - {prenotazioneSelezionata.Servizio}?",
                        "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        //se ha messo yes allora eliminiamo e quindi chiamiamo il metodo che si occupa di cio
                        if (result == DialogResult.Yes)
                        {
                            await EliminaPrenotazione(prenotazioneSelezionata, listView);//chiamo il metodo per eliminare che comunica con il server per eliminare dal db l'elemento. passo la prenotazione e la listview per i motivi spiegati nella funzione
                        }
                    }
                }
            }
        }

        private async Task EliminaPrenotazione(Prenotazione prenotazione_da_eliminare, ListView listView)
        {
            try
            {
                //converto la prenotazione in json
                string json = JsonConvert.SerializeObject(prenotazione_da_eliminare);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //effettuo una richiesta post al server per eliminare la prenotazione
                HttpResponseMessage response = await ApiClient.client.PostAsync("Delete_prenotazione", content);
                string result = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);

                //controllo se la prenotazione è stata eliminata con successo
                if (jsonResponse.success == true)
                {
                    MessageBox.Show("Prenotazione eliminata con successo", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //a questo punto rimuovo la prenotazione dalla lista delle prenotazioni dell'utente
                    foreach (ListViewItem item in listView.Items)
                    {
                        if (item.Tag is Prenotazione)
                        {
                            if (((Prenotazione)item.Tag).Giorno_prenotazione == prenotazione_da_eliminare.Giorno_prenotazione && ((Prenotazione)item.Tag).Ora_prenotazione == prenotazione_da_eliminare.Ora_prenotazione && ((Prenotazione)item.Tag).Servizio == prenotazione_da_eliminare.Servizio)
                            {
                                listView.Items.Remove(item);
                                await RipristinaDisponibilita(prenotazione_da_eliminare);
                                break;//esco immediatamente dal foreach perche ho trovato l'unico elemento e l'ho rimosso
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Errore durante l'eliminazione: " + jsonResponse.message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RipristinaDisponibilita(Prenotazione prenotazione)
        {
            try
            {
                string json = JsonConvert.SerializeObject(prenotazione);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await ApiClient.client.PostAsync("aggiungi_disponibilita", content);
                string result = await response.Content.ReadAsStringAsync();

                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);
                if (jsonResponse.success == true)
                {
                    await CaricaDisponibilita(); // Aggiorno la disponibilità per riflettere la modifica
                }
                else
                {
                    MessageBox.Show("Errore nel ripristino della disponibilità: " + jsonResponse.message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonLogOut_Click(object sender, EventArgs e)
        {
            //mostriamo per prima cosa un messaggio per confermare l'uscita
            DialogResult result = MessageBox.Show("sei sicuro/a di voler uscire?", "CONFERMA", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {//se l'utente ha cliccato si 
                chiusuraNormale = true;//qui lo metto a true perche close chiama il formclosing per chiudere il form, impostandolo a true non entra nell'if e c'è la giusta esecuzione
                //Recupero il form login "padre" e lo mostro di nuovo
                if (this.Owner != null) {
                    this.Owner.Show();
                }
                this.Close();//non serve il dispose perche lo fa automaticamente, perche? Dispose verrà chiamato automaticamente se il modulo viene visualizzato usando il Show metodo.Se viene usato un altro metodo ShowDialog , o il modulo non viene mai visualizzato, è necessario chiamarsi Dispose all'interno dell'applicazione. 
            }
        }

        private void form_prenotazioni_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppState.Chiudendo==false && chiusuraNormale == false)//mettiamo questa cosa perche senza di esso, se avessi chiamato l'application.exit avrebbe richiamato questo metodo per chiudere tutto e quindi in quel modo mi appariva due volte lo stesso messagebox 
            {//se è la prima volta che l'evento viene chiamato allora
             //mostriamo per prima cosa un messaggio per confermare l'uscita
                DialogResult result = MessageBox.Show("sei sicuro/a di voler uscire?", "CONFERMA", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {//se l'utente ha cliccato si 
                    AppState.Chiudendo = true;
                    ApiClient.Shutdown();//chiamo il dispose di apiclient
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
