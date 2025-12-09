import tkinter as tk # Libreria Tkinter per l'interfaccia grafica
from tkinter import messagebox  # Per mostrare messaggi a schermo
from client_request import login  # Importiamo la funzione di login dal server
import re #libreria per il controllo del formato email 
import dashboard  # Importiamo la dashboard

#classe che gestisce la finestra di login 
class LoginWindow:
    def __init__(self,root):
        self.root=root # Salva la finestra principale in self.root
        self.root.title("LOGIN ADMIN") #titolo della finestra
        self.root.geometry("500x400") #dimensione della finestra
        self.root.configure(bg="tan")  # Imposta lo sfondo della finestra a "tan"

        # Frame centrale per contenere tutti i widget e centrarli
        frame_centrale = tk.Frame(master=root, bg="tan")
        frame_centrale.pack(expand=True, fill="both") 

        #passo alla creazione dell'interfaccia grafica
        tk.Label(master=frame_centrale,text="Email:", bg="tan", font=("Arial", 14)).pack(pady=10) #etichetta/label per il campo email. pady indica lo spazio verticale tra un widget e gli altri componenti della finestra
        self.email_entry=tk.Entry(master=frame_centrale, width=30, font=("Arial", 12)) #campo di input per l'email
        self.email_entry.pack(pady=5)

        tk.Label(master=frame_centrale,text="Password:", bg="tan", font=("Arial", 14)).pack(pady=10) #etichetta per la password
        self.password_entry=tk.Entry(master=frame_centrale,show="*", width=30, font=("Arial", 12)) #campo input per password in cui ogni carattere viene messo *
        self.password_entry.pack(pady=5)

        tk.Button(master=frame_centrale,text="ACCEDI", command=self.check_login, bg="lemon chiffon", fg="black", font=("Arial", 12), width=15).pack(pady=20) #pulsante per il login
        
        # Intercetta la chiusura con la "X"
        self.root.protocol("WM_DELETE_WINDOW", self.conferma_uscita) #WM_DELETE_WINDOW è un protocollo di Tkinter che viene attivato quando l'utente tenta di chiudere la finestra con la X. Viene sostituito il comportamento predefinito della chiusura con la funzione self.conferma_uscita 
        
        #Bottone per uscire
        self.btn_esci = tk.Button(master=frame_centrale, text="Esci", command=self.conferma_uscita, bg="lemon chiffon", fg="black", font=("Arial", 12), width=15)
        self.btn_esci.pack(pady=10)

    def conferma_uscita(self):
        """Mostra un messaggio di conferma prima di chiudere"""
        risposta = messagebox.askyesno("Conferma Uscita", "Sei sicuro di voler uscire?")
        if risposta:  # Se l'utente clicca si
            self.root.destroy()  #Chiude la finestra

    #funzione per verifica del login
    def check_login(self):
        email=self.email_entry.get() #ottengo l'email inserita
        password=self.password_entry.get() #ottengo la password
        
        #controllo se i campi sono vuoti
        if not email or not password:
            messagebox.showerror("Errore", "Tutti i campi sono obbligatori!")
            return  # Blocca l'esecuzione se ci sono campi vuoti

        #controllo il formato email con espressione regolare
        email_regex = r"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$"
        if not re.match(email_regex, email):
            messagebox.showerror("Errore", "Formato email non valido! Inserisci un'email corretta.")
            return  # Blocca l'esecuzione se l'email non è valida

        response=login(email,password) #invio i dati al server c++ per il login

        #se la risposta è none , ci sono problemi con il server
        if response is None:
            return

        if response.get("success"): #se success è true
            messagebox.showinfo("SUCCESSO", response["message"]) #mostro il messaggio del server che è andato a buon fine il login
            self.root.destroy() # chiudo la finestra di login ed apro la finestra per visualizzare le cose inerenti alla parrucchieria
            dashboard.open_dashboard() #apro la dashboard 
        else: #se è false
            messagebox.showerror("Errore",response["message"]) #mostra il messaggio di errore ricevuto al server

if __name__=="__main__":
    root=tk.Tk() #questo è il contenitore principale della GUI e quindi root è l'oggetto che rappresenta la finestra principale, tutte le cose(bottoni ecc) vengono posizionati dentro root. se io eseguo solo il codice import di tkinter, questa riga e root.mainloop() si apre una finestra vuota
    app=LoginWindow(root) #è una classe che gestisce l'interfaccia di login e usera root per costruire l'interfaccia dentro la finestra principale
    root.mainloop() # cosi faccio partire il loop principale di tkinter (gui interattiva) mantiene la finestra aperta e interattiva finche l'utente non la chiude