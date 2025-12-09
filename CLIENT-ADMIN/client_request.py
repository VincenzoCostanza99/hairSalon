import requests #importiamo la libreria requests per fare richieste http http al server
from tkinter import messagebox  # Importiamo messagebox per mostrare errori all'utente

#url di base del server c++ con crow
base_url="http://localhost:8080"

#funzione per eseguire il login dell'admin
def login(email,password):
    url=f"{base_url}/login_admin" #endpoint del server per il login
    try:
        response = requests.post(url,json={"Email": email, "Password": password}) #invia la richiesta POST con i dati al server
        return response.json() #restituisce la risposta json ricevuta dal server   
    except requests.exceptions.ConnectionError:
        messagebox.showerror("Errore","Il server non attivo. Avvialo e riprova.")
    except requests.exceptions.JSONDecodeError as e:
        messagebox.showerror("Errore", f"Errore nel parsing JSON: {e}\nRisposta server: {response.text}")
    except requests.exceptions.RequestException as e:
        messagebox.showerror("Errore",f" Errore di connessione: {e}")
    return None

#funzione per eliminare le vecchie prenotazioni
def elimina_vecchie_prenotazioni_e_vecchie_disponibilita():
    url=f"{base_url}/EliminaTutto"
    try:
        response=requests.post(url)#invio la richiesta get
        return response.json() #restituisce la risposta json ricevuta dal server
    except requests.exceptions.ConnectionError:
        messagebox.showerror("Errore","Il server non attivo. Avvialo e riprova.")
    except requests.exceptions.JSONDecodeError as e:
        messagebox.showerror("Errore", f"Errore nel parsing JSON: {e}\nRisposta server: {response.text}")
    except requests.exceptions.RequestException as e:
        messagebox.showerror("Errore",f" Errore di connessione: {e}")
    return None

#funzione per ottenere l'elenco delle prenotazioni dal server
def get_prenotazioni():
    url=f"{base_url}/Allprenotazioni" #endpoint del server per ottenere le prenotazioni
    try:
        response=requests.get(url)#invio la richiesta get
        return response.json() #restituisce la risposta json ricevuta dal server
    except requests.exceptions.ConnectionError:
        messagebox.showerror("Errore","Il server non attivo. Avvialo e riprova.")
    except requests.exceptions.JSONDecodeError as e:
        messagebox.showerror("Errore", f"Errore nel parsing JSON: {e}\nRisposta server: {response.text}")
    except requests.exceptions.RequestException as e:
        messagebox.showerror("Errore",f" Errore di connessione: {e}")
    return None

#funzione per prendere i clienti registrati
def get_clienti():
    url=f"{base_url}/clienti" #endpoint per prendere tutti i clienti
    try:
        response=requests.get(url) #invio la richiesta GET al server c++
        return response.json() #restituisce la risposta json ricevuta dal server
    except requests.exceptions.ConnectionError:
        messagebox.showerror("Errore","Il server non attivo. Avvialo e riprova.")
    except requests.exceptions.JSONDecodeError as e:
        messagebox.showerror("Errore", f"Errore nel parsing JSON: {e}\nRisposta server: {response.text}")
    except requests.exceptions.RequestException as e:
        messagebox.showerror("Errore",f" Errore di connessione: {e}")
    return None

#funzione per aggiungere una nuova disponibilita di orario
def aggiungi_disponibilita(giorno):
    url=f"{base_url}/aggiungi_disponibilita_admin" #endpoint per aggiungere disponibilita
    try:
        response=requests.post(url, json={"Giorno_prenotazione":giorno}) #invio la richiesta POST al server c++
        return response.json() #restituisce la risposta json ricevuta dal server
    except requests.exceptions.ConnectionError:
        messagebox.showerror("Errore","Il server non attivo. Avvialo e riprova.")
    except requests.exceptions.JSONDecodeError as e:
        messagebox.showerror("Errore", f"Errore nel parsing JSON: {e}\nRisposta server: {response.text}")
    except requests.exceptions.RequestException as e:
        messagebox.showerror("Errore",f" Errore di connessione: {e}")
    return None