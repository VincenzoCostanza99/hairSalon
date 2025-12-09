import tkinter as tk
from tkinter import ttk, messagebox
import matplotlib.pyplot as plt
from client_request import get_prenotazioni, aggiungi_disponibilita,elimina_vecchie_prenotazioni_e_vecchie_disponibilita,get_clienti  # Funzioni per comunicare col server
from datetime import datetime  # Per controllare il giorno corrente

#classe che gestisce la finestra della dashboard
class Dashboard:
    def __init__(self,root):
        self.root=root # Salva la finestra principale in self.root
        self.root.title("Dashboard Amministratore") #titolo della finestra
        self.root.geometry("800x500") #dimensione della finestra
        self.root.configure(bg="tan")  # Imposta lo sfondo color tan
        
        #frame principale per contenere il tutto
        self.frame_centrale= tk.Frame(master=root, bg="tan")
        self.frame_centrale.pack(expand=True, fill="both")

        #Titolo iniziale
        self.label=tk.Label(master=self.frame_centrale,text="Cosa desideri fare?", font=("Arial",16), bg="tan") 
        self.label.pack(pady=20)

        #creo un bottone per mostrare le prenotazioni
        self.btn_prenotazioni=tk.Button(master=self.frame_centrale,text="Mostra Prenotazioni", command=self.mostra_prenotazioni, font=("Arial", 12), bg="lemon chiffon", fg="black", width=40, height=2)
        self.btn_prenotazioni.pack(pady=5)

        #creo un bottone per mostrare i clienti
        self.btn_clienti=tk.Button(master=self.frame_centrale, text="Mostra Clienti", command=self.mostra_clienti, font=("Arial", 12), bg="lemon chiffon", fg="black", width=40, height=2)
        self.btn_clienti.pack(pady=5)# aggiungo il bottono per mostrare i clienti

        #creo un bottone per le statistiche
        self.btn_statistiche_torta = tk.Button(master=self.frame_centrale, text="Mostra percentuale prenotazione dei servizi", command=self.mostra_grafico_torta, font=("Arial", 12), bg="lemon chiffon", fg="black", width=40, height=2)
        self.btn_statistiche_torta.pack(pady=5)

        #creo un bottone per le statistiche
        self.btn_statistiche_barre = tk.Button(master=self.frame_centrale, text="Mostra percentuale numero prenotazioni per giorno", command=self.mostra_grafico_barre, font=("Arial", 12), bg="lemon chiffon", fg="black", width=40, height=2)
        self.btn_statistiche_barre.pack(pady=5)

        # Crea un bottone per aggiungere disponibilità
        self.btn_add_disponibilita = tk.Button(master=self.frame_centrale, text="Aggiungi Disponibilita'", command=self.add_disponibilita, font=("Arial", 12), bg="lemon chiffon", fg="black", width=40, height=2)
        self.btn_add_disponibilita.pack(pady=5)

        # Intercetta la chiusura con la "X"
        self.root.protocol("WM_DELETE_WINDOW", self.conferma_uscita) #WM_DELETE_WINDOW è un protocollo di Tkinter che viene attivato quando l'utente tenta di chiudere la finestra con la X. Viene sostituito il comportamento predefinito della chiusura con la funzione self.conferma_uscita 
        
        #Bottone per uscire
        self.btn_esci = tk.Button(master=self.frame_centrale, text="Esci", command=self.conferma_uscita, font=("Arial", 12), bg="lemon chiffon", fg="black", width=40, height=2)
        self.btn_esci.pack(pady=5)
        
        #variabili per la tabella
        self.frame_tabella=None
        self.tree=None
        self.search_entry=None
    
    def conferma_uscita(self):
        """Mostra un messaggio di conferma prima di chiudere"""
        risposta = messagebox.askyesno("Conferma Uscita", "Sei sicuro di voler uscire?")
        if risposta:  # Se l'utente clicca si
            self.root.destroy()  #Chiude la finestra

    def mostra_grafico_barre(self):
        """Genero un grafico a barre per mostrare il numero di prenotazioni per giorno della settimana"""
        response = get_prenotazioni()

        #controllo se ci sono problemi nel recupero dati
        if response is None:
            return

        #controllo se la tabella è vuota 
        if response.get("vuoto"):
            messagebox.showinfo("Informazione","Nessuna prenotazione disponibile.")
            return

        #Disabilita temporaneamente la finestra principale (effetto "modale")
        self.root.attributes("-disabled", True)#disabilita tutti i widget della finestra principale (nessun bottone risponde).

        giorni_settimana = ["Martedi", "Mercoledi", "Giovedi", "Venerdi", "Sabato"]
        #setto a 0 il conyeggio per ogni giorno
        conteggio_giorni = {giorno: 0 for giorno in giorni_settimana}

        for prenotazione in response.get("prenotazioni"):#itero su tutte le prenotazioni
            giorno = prenotazione["giorno_prenotazione"] #prendo il giorno
            if giorno in conteggio_giorni: 
                conteggio_giorni[giorno] += 1 #incremento di 1 il conteggio
        
        #creo il grafico
        fig=plt.figure(figsize=(8,6)) #imposto le dimensioni del gradico e ottengo la figura corrente
        fig.canvas.manager.set_window_title("grafico Prenotazioni") #.canvas con esso accedo alla struttura interna della fifgura, .manager controlla la finestra della gui e poi setto il titolo
        #creo un grafico a barre verticali dove nell'asse x metti i giorni della settimana che sono le chiavi di conteggio_giorni, nell'asse y il numero di prenotazioni che sono i valori delle rispettive chiavi
        plt.bar(conteggio_giorni.keys(),conteggio_giorni.values(),color="purple")# values() restituisce la lista dei conteggi
        plt.xlabel("Giorni della settimana")
        plt.ylabel("Numero delle prenotazioni")
        plt.title("Numero di prenotazioni per giorno")

        #mostro solo numeri interi sull'asse y
        plt.yticks(range(0,max(conteggio_giorni.values())+1,1)) # qui parto da 0 e incremento di uno e prendo il massimo della lista che mi viene restituita da values di valori del dizionario conteggio giorni
        
        def on_close(event):
            self.root.attributes("-disabled", False)  #riabilita la finestra principale quando chiudi il grafico

        fig.canvas.mpl_connect("close_event", on_close)
        
        plt.show() #il grafico comparira ma anche se non chiudo la dashboard continua a funzionare tranquillamente penso perche tkinter essendo inetrattiva permette di farlo
        
    def mostra_grafico_torta(self):
        """Genero un grafico a torta per visualizzare le prenotazioni dei servizi"""

        response=get_prenotazioni() #Recupera le prenotazioni dal server

        #controllo se ci sono problemi nel recupero dati
        if response is None:
            return

        #controllo se la tabella ? vuota 
        if response.get("vuoto"):
            messagebox.showinfo("Informazione","Nessuna prenotazione disponibile.")
            return

        #Disabilita temporaneamente la finestra principale (effetto "modale")
        self.root.attributes("-disabled", True)#disabilita tutti i widget della finestra principale (nessun bottone risponde).

        #conteggio delle prenotazioni per servizio
        conteggio_servizi={ "Taglio": 0, "Colore": 0, "Piega": 0} #dizionario per tenere traccia del numero di prenotazioni per ogni servizio
        
        for prenotazione in response.get("prenotazioni"): #itero su tutte le prenotazioni ricevute dal server
            servizio=prenotazione["servizio"] #prendo il servizio
            if servizio in conteggio_servizi:
                conteggio_servizi[servizio] +=1 #incremento di 1 il conteggio del servizio corrispondente
        
        #adesso creo un nuovo dizionario andando a prendere solo i servizi che hanno un conteggio maggiore di 0
        #questo lo faccio con un dictionary comprehension in cui definisco chiave e valore e poi itero sul vecchio dizionario
        #tramite items che mi da chiave e valore solo se il valore(conteggio) ? maggiore di 0 quindi filtro solo le chiavi con valore maggiore di 0
        dati_filtrati={chiave:valore for chiave,valore in conteggio_servizi.items() if valore>0}

        #creo un grafico a torta e ottengo la figura corrente
        fig=plt.figure(figsize=(6,6)) #imposto le dimensioni del grafico
        fig.canvas.manager.set_window_title("grafico Servizi") #.canvas con esso accedo alla struttura interna della fifgura, .manager controlla la finestra della gui e poi setto il titolo 
        #faccio il grafico a torta
        plt.pie( 
            dati_filtrati.values(), #uso i valori dei servizi come quantita. values restituisce una lista di valori
            labels=dati_filtrati.keys(), #uso i nomi dei servizi come etichette
            autopct='%1.1f%%', #mostra le percentuali con 1 decimale. il primo % indica che in quella posizione verra inserito un valore formattato, 1.1f specifica il formato numerico-> f formato float 1 larghezza minima del numero, .1 numero di cifre decimali, %% stampa il simbolo %, siccome % ha un significato speciale nella formattazione, dobbiamo scriverlo due volte 
            startangle=90, #ruota il grafico per una migliore visualizzaizone nel senso che parte dalle ore 12 cioe 90 perche di default parte dalle ore 3 cioe 0 gradi
            colors=["red","blue","green"] #colori per distinguere i servizi
            )
        plt.title("Distribuzione delle Prenotazioni per Servizio")  # Titolo del grafico
        
        def on_close(event):
            self.root.attributes("-disabled", False)  #riabilita la finestra principale quando chiudi il grafico

        fig.canvas.mpl_connect("close_event", on_close)

        plt.show()  # Mostra il grafico a torta. il grafico comparira ma anche se non chiudo la dashboard continua a funzionare tranquillamente
        

    def mostra_clienti(self):
        """mostra una tabella con tutti i clienti registrati"""
        response= get_clienti()
        if response is None: #recupera la lista dei clienti dal server
            return

        if response.get("vuoto"): #se non ci sono clienti
            messagebox.showwarning("Attenzione",response.get("message"))
            return

        #nascondo i bottoni
        self.frame_centrale.pack_forget()

        # Se esiste gia una tabella, la distrugge per evitare sovrapposizioni
        if self.frame_tabella:
            self.frame_tabella.destroy()

        # Creazione del frame che conterra la tabella
        self.frame_tabella = tk.Frame(master=self.root, bg="tan")
        self.frame_tabella.pack(pady=10, fill="both", expand=True)

        # Creazione della barra di ricerca
        search_frame = tk.Frame(master=self.frame_tabella, bg="tan")
        search_frame.pack(pady=5, fill="x")

        right_frame = tk.Frame(master=search_frame, bg="tan")
        right_frame.pack(side="right", padx=5)

        tk.Label(master=right_frame, text="Cerca per email:", bg="tan").pack(side="left", padx=5)
        self.search_entry = tk.Entry(master=right_frame)  # Campo per inserire l'email
        self.search_entry.pack(side="left", padx=5)

        btn_search = tk.Button(master=right_frame, text="Cerca", command=self.filtra_clienti, bg="lemon chiffon")
        btn_search.pack(side="left", padx=5)

        btn_back = tk.Button(master=search_frame, text="Torna indietro", command=self.torna_indietro, bg="lemon chiffon")
        btn_back.pack(side="left", padx=5)

        # Creazione della tabella con i clienti
        self.tree = ttk.Treeview(master=self.frame_tabella, columns=("Email", "Nome", "Cognome"), show="headings")
        self.tree.heading("Email", text="Email")
        self.tree.heading("Nome", text="Nome")
        self.tree.heading("Cognome", text="Cognome")

        # Imposta la dimensione delle colonne
        self.tree.column("Email", anchor="center", width=200)
        self.tree.column("Nome", anchor="center", width=150)
        self.tree.column("Cognome", anchor="center", width=150)

        self.tree.pack(pady=10, fill="both", expand=True)

        #riempio la tabella con i dati
        self.riempi_tabella(response.get("clienti"))

    def riempi_tabella(self,clienti):
        """riempie la tabella con i dati dei clienti"""
        #svuoto prima la tabella prima di riempirla
        for row in self.tree.get_children():
            self.tree.delete(row)

        # Inserisco i dati dei clienti nella tabella
        for cliente in clienti:
            self.tree.insert("", "end", values=(
                cliente["email"],
                cliente["nome"],
                cliente["cognome"]
            ))

    def filtra_clienti(self):
        """fiiltro i clienti in base allemail inserita nella barra di ricerca"""
        email_ricercata=self.search_entry.get().strip().lower() #qui ho aggiunto pure Strip rispetto a prima, perchè così se metto spazi all'inizio e alla fine della stringa me li leva

        #recupero tutti i clienti dal server
        response=get_clienti()
        if response is None:
            return

        if response.get("vuoto"):
            messagebox.showwarning("Attenzione", response.get("message"))
            return
        
        clienti = response.get("clienti")#clienti è una lista di dizionari

        #se il campo è vuoto, mostra tutti i clienti e interrompe l'esecuzione
        if not email_ricercata:
            messagebox.showerror("Errore","Inserisci un'email da cercare")
            self.riempi_tabella(clienti)
            return

        #cerco il cliente per email, filtro i clienti per l'email ricercata e trovo, se  presente, il desiderato. next scorre tutti gli elementi della lista e appena ne trova uno lo restituisce immediatamente, se non ne trova allora sara None
        cliente_trovato = next((c for c in clienti if email_ricercata==c["email"].lower()),None)# è un generatore
        #cliente trovato sarà un singolo dizionario (e anche unico) della lista di dizionari, oppure sarà None
        if cliente_trovato is None:
            messagebox.showinfo("Attenzione", f"Nessun cliente trovato con email '{email_ricercata}'")
            return
        
        #mostro solo il cliente trovato
        self.riempi_tabella([cliente_trovato]) #gli sto passando una lista che sto creando qui con un solo elemento che è cliente_trovato che avra nome, cognome e email

    def add_disponibilita(self):
        oggi=datetime.today().strftime('%A') #ottengo il giorno corrente. converto una data nel nome del giorno

        #controllo se ? domenica
        if oggi.lower()=="sunday":
            risposta=messagebox.askyesno("Conferma", "E' domenica. Vuoi aggiungere le disponibilita' della settimana che sta per iniziare?")
            if risposta:
                response=elimina_vecchie_prenotazioni_e_vecchie_disponibilita() #funzione per eliminare le prenotazioni della settimana ormai passata se ci sono ovviamente e per eliminare le restanti disponbilita se ci sono ovviamente
                if response is None:
                    return
                self.scelta_giorni()
            else:
                messagebox.showinfo("Annullato", "Operazione annullata")
        else:
            messagebox.showwarning("Attenzione", "puoi aggiungere le disponbilita solo di domenica!")

    def scelta_giorni(self):
        #Nascondo gli elementi principali della dashboard e quindi la frame centrale che contiene tutto
        self.frame_centrale.pack_forget()

        if self.frame_tabella:
            self.frame_tabella.destroy() #elimino eventuali tabelle esistenti, questo lo uso perche potrebbe far apparire tabelle vecchie nel form, solo per questo perche quando riscrivo sel.frame_tabella dovrebbe puntare ad un altro oggetto 
        
        #creo una frame per scegliere la modalita 
        self.frame_tabella=tk.Frame(master=self.root,bg="tan")
        self.frame_tabella.pack(pady=10,fill="both",expand=True)

        tk.Label(master=self.frame_tabella, text="Come vuoi aggiungere le disponibilita'?", font=("Arial", 14),bg="tan").pack(pady=10)

        tk.Button(master=self.frame_tabella, text="Aggiungi intera settimana", width=25, command=self.aggiungi_settimana_intera,bg="lemon chiffon").pack(pady=5)
        tk.Button(master=self.frame_tabella, text="Seleziona giorni specifici", width=25, command=self.aggiungi_giorni_specifici,bg="lemon chiffon").pack(pady=5)

    def aggiungi_settimana_intera(self):
        for giorno in ["Martedi","Mercoledi","Giovedi","Venerdi","Sabato"]:
            response=aggiungi_disponibilita(giorno)
            if response is None:
                return #qui ritorno alla funzione chiamante
        messagebox.showinfo("Successo","Disponibilita' per tutta la settimana aggiunte!")
        self.torna_indietro()

    def aggiungi_giorni_specifici(self):
        #nascondo gli elementi precedenti
        self.frame_tabella.pack_forget()

        self.frame_giorni=tk.Frame(master=self.root, bg="tan")
        self.frame_giorni.pack(fill="both",expand=True)
        
        #adesso creo un dizionario comprehension giorni  e ogni giorno della settimana viene usato come chiave e a ciascuna chiave associo un'istanza della classe intvar() che è una variabile speciale di tkinter  utilizzata solitamente con widget interattivi. questo intvar() è una classe quindi creo un'istanza della classe e questo serve perche ad ogni giorno associo un checkbox e se il checkbox è selezionato intvar dovrebbe assumere il valore 1 altrimenti 0 e cosi raccolgo tutti i checkbox selezionati 
        giorni={giorno: tk.IntVar() for giorno in ["Martedi","Mercoledi","Giovedi","Venerdi","Sabato"]}
        

        #ora vado a iterare il dizionario e associo ad ogni elemento un checkbutton che ha come testo il giorno e quindi la chiave e come variabile l'istanza e questo ci permettera di verificare se la checkbox è stata selezionata o meno
        for giorno,var in giorni.items():
            tk.Checkbutton(master=self.frame_giorni, text=giorno,variable=var, font=("Arial", 14), bg="tan").pack(side="left", padx=10) 
        
        #faccio una funzione annidata per aumentare la leggibilita del codice e utilizzo diretto delle variabili
        def conferma():
            selezionati=[giorno for giorno,var in giorni.items() if var.get()==1]
            if not selezionati:
                messagebox.showwarning("Errore", "Seleziona almeno un giorno!")
                return

            for giorno in selezionati:
                response= aggiungi_disponibilita(giorno)
                if response is None:
                    self.frame_giorni.destroy()
                    self.torna_indietro()
                    return #qui ritorno alla funzione chiamante

            messagebox.showinfo("Successo","Disponibilita' selezionate aggiunte")
            self.frame_giorni.destroy()
            self.torna_indietro()

        tk.Button(master=self.frame_giorni,text="Conferma",command=conferma, font=("Arial", 14, "bold"), bg="lemon chiffon", width=15).pack(side="left",padx=20)  #se spostassi la creazione del bottone prima della definizione della funzione annidata conferma, otterresti un errore, e il codice non funzionerebbe.quindi si mette sempre dopo(vale per tutto). In Python, quando associ una funzione a un bottone (o a qualsiasi widget), quella funzione deve essere gi? definita prima del suo utilizzo.  
 


    def mostra_prenotazioni(self):
        #controllo per prima cosa se ci sono prenotazioni e, se presenti, mostra la tabella e la barra di ricerca
        response=get_prenotazioni()
        #se la risposta è none , ci sono problemi con il server
        if response is None:
            return

        #se il campo vuoto è true, significa che non ci sono prenotazioni disponibili
        if response.get("vuoto"):
            messagebox.showwarning("Attenzione",response.get("message"))
            return

        #nascondo tutto
        self.frame_centrale.pack_forget()

        #se siamo arrivati qui allora success=true e vuoto=false e allora creo il frame per la tabella(se non esiste gia). se gia esiste la distruggo e ne creo una nuova
        if self.frame_tabella:
            self.frame_tabella.destroy() #elimino eventuali tabelle esistenti, questo lo uso perche potrebbe far apparire tabelle vecchie nel form, solo per questo perche quando riscrivo self.frame_tabella dovrebbe puntare ad un altro oggetto.
        
        self.frame_tabella= tk.Frame(master=self.root,bg="tan") #Crea un nuovo contenitore (Frame) per altri elementi dell'interfaccia. self.root	Specifica che il Frame sara dentro la finestra principale (self.root)
        self.frame_tabella.pack(pady=10,fill="both",expand=True) #fill="both" e expand serve per prendere tutto il form e quindi riempire tutto

        #creo una barra di ricerca per cercare eventuali prenotazioni in funzione dell'email
        search_frame=tk.Frame(master=self.frame_tabella, bg="tan")#creo un contenitore dentro frame tabella
        search_frame.pack(pady=5, fill="x") #fill="x": Il widget occupa tutto lo spazio disponibile orizzontalmente.
        
        # Frame per posizionare barra ricerca a destra quindi creo una frame che ingloba al suo interno la ricerca
        right_frame = tk.Frame(master=search_frame,bg="tan")
        right_frame.pack(side="right", padx=5)

        tk.Label(master=right_frame,text="Cerca per servizio:", bg="tan").pack(side="left", padx=5)#creo un'etichetta con il testo seguente e lo metto dentro frame. con padx 5 posiziono l'etichetta con 2 pixel di spazio a dx e sx
        self.search_entry=tk.Entry(master=right_frame) # creo una casella di input entry per scrivere il servizio da cercare
        self.search_entry.pack(side="left",padx=5)#si posiziona a sinistra con side left
        
        btn_Search= tk.Button(master=right_frame, text="Cerca", command=self.filtra_prenotazioni, bg="lemon chiffon")
        btn_Search.pack(side="left", padx=5) #creo un pulsante cerca che chiama filtra_prenotazioni quando viene premuto
        btn_back=tk.Button(master=search_frame, text="Torna indietro", command=self.torna_indietro, bg="lemon chiffon")#il bottono back si trova sempre nella frame search_frame
        btn_back.pack(side="left", padx=5)#creo pulsante indietro

        #creo la tabella
        self.tree=ttk.Treeview(master=self.frame_tabella,columns=("Giorno", "Ora", "Servizio", "Email"), show="headings") # questo show headings serve perche senza di esso crea una colonna nascosta sulla sinistra, quindi cosi disabilitiamo cio
        
        #imposto gli header della tabella
        self.tree.heading("Giorno", text="Giorno")
        self.tree.heading("Ora", text="Ora")
        self.tree.heading("Servizio", text="Servizio")
        self.tree.heading("Email", text="Email")
        
        #aggiunta dei bordi alle colonne
        self.tree.column("Giorno", anchor="center", width=100)
        self.tree.column("Ora", anchor="center", width=100)
        self.tree.column("Servizio", anchor="center", width=150)
        self.tree.column("Email", anchor="center", width=200)
        
        #inseriamola tabella nel frame
        self.tree.pack(pady=10, fill="both", expand=True)
        
        #riempio la tabella
        self.riempi_tabella_prenotazioni(response.get("prenotazioni"))

    def riempi_tabella_prenotazioni(self,prenotazioni):
        """riempie la tabella con le prenotazioni"""
        #svuoto prima la tabella prima di riempirla
        for row in self.tree.get_children():
            self.tree.delete(row)
        
        #inserisco i dati in tabella
        for prenotazione in prenotazioni:
            self.tree.insert("","end", values=(  #"" significa la riga viene aggiunta alla radice della tabella cioe non come sottoriga di un altro elemento, end vuol dire che la riga viene aggiunta alla fine
                prenotazione["giorno_prenotazione"],
                prenotazione["ora_prenotazione"],
                prenotazione["servizio"],
                prenotazione["email"]
            ))

    def torna_indietro(self):#distruggo la tabella e rimetto la label e il bottone
        self.frame_tabella.destroy()
        self.frame_centrale.pack(expand=True, fill="both")

    def filtra_prenotazioni(self):
        """filtro le prenotazioni nella tabella in base al servizio inserito"""
        servizio_ricercato=self.search_entry.get().strip().lower()

        #recupero tutte le prenotazioni
        response=get_prenotazioni()

        #se la risposta è none , ci sono problemi con il server
        if response is None:
            return

        #se il campo vuoto è true, significa che non ci sono prenotazioni disponibili
        if response.get("vuoto"):
            messagebox.showwarning("Attenzione",response.get("message"))
            return

        prenotazioni= response.get("prenotazioni")
        
        #controllo se il campo è vuoto
        if not servizio_ricercato:
            messagebox.showerror("Errore", "Inserisci un servizio da cercare!")
            self.riempi_tabella_prenotazioni(prenotazioni)
            return
        
        # controllo se il servizio è valido
        if servizio_ricercato not in ["taglio", "colore", "piega"]:
            messagebox.showerror("Errore", "Il servizio deve essere 'taglio', 'colore' o 'piega'!")
            self.riempi_tabella_prenotazioni(prenotazioni)
            return

        #vado a prendermi le prenotazioni filtrate
        prenotazioni_filtrate=[p for p in prenotazioni if servizio_ricercato in p["servizio"].lower()]

        #se non ci sono prenotazioni per quel servizio
        if not prenotazioni_filtrate:
            messagebox.showinfo("Attenzione", f"Nessuna prenotazione trovata per il servizio '{servizio_ricercato}'")
            self.riempi_tabella_prenotazioni(prenotazioni)
            return

        self.riempi_tabella_prenotazioni(prenotazioni_filtrate)

# Funzione per avviare la dashboard
def open_dashboard():
    """Crea la finestra principale e avvia la dashboard."""
    root = tk.Tk()
    app = Dashboard(root)
    root.mainloop()