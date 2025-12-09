#include <iostream> // Libreria standard C++ per input/output
#include <string> // Libreria standard C++ per la gestione delle stringhe
#include <crow.h> // Libreria Crow per la gestione delle REST API
#include <mysql_driver.h> // Include il driver MySQL
#include <mysql_connection.h> // Include la classe MySQL_Connection
#include <mysql_error.h> // Include la gestione degli errori MySQL
#include <regex> // Per usare le espressioni regolari
#include <cppconn/prepared_statement.h> // Include la classe MySQL_PreparedStatement

//connessione al DB
static sql::mysql::MySQL_Connection* connectToDatabase() {
    try {
        sql::mysql::MySQL_Driver* driver = sql::mysql::get_mysql_driver_instance(); // Ottiene l'istanza del driver MySQL, qui non serve fare il delete perche questa funzione implementa il pattern singleton cioe get_mysql_driver_istance() infatti esso non si crea un nuovo oggetto ogni volta, ma restituisce sempre la stessa istanza se è presente. in qualche parte del codice ci sara uno static che assicura che esista una sola istanza per tutta la durata del programma 
        sql::mysql::MySQL_Connection* conn = static_cast<sql::mysql::MySQL_Connection*>(driver->connect("tcp://127.0.0.1:3306", "pippo", "ciao")); // Crea la connessione al database
        conn->setSchema("parrucchieria"); // Imposta il database da utilizzare
        std::cout << "Connessione avvenuta con successo." << std::endl;
        return conn;
    }
    catch (sql::SQLException& e) { // Gestione delle eccezioni SQL.PRIMA CATTURO GLI ERRORI DI MYSQL E POI TUTTI GLI ALTRI VERRANNO CATTURATI DA EXCEPTION
        std::cerr << "Errore nella connessione al database: " << e.what() << std::endl;
        exit(EXIT_FAILURE); // Termina il programma in caso di errore
    }
    catch (std::exception& e) {
        std::cerr << "Errore generico: " << e.what() << std::endl;
        exit(EXIT_FAILURE);
    }
}
//controllo email valida
static bool isValidEmail(const std::string& email) {
    const std::regex pattern(R"(^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$)");
    return std::regex_match(email, pattern);
}

static std::string simpleHash(const std::string& password) {
    unsigned int hash = 0;//int può contenere numeri negativi, quindi se il valore diventa troppo grande, potrebbe diventare negativo (overflow aritmetico).unsigned int non ha valori negativi e permette di lavorare con numeri più grandi senza il rischio di segni negativi.
    for (char c : password) {
        hash = (hash * 31) + static_cast<unsigned int>(c);//moltiplico per 31 perche è un numero primo per evitare collisioni. aggiungiamo poi il valore ASCII del carattere tramite lo static cast(per esempio il carattere c è 99) 
        hash = hash ^ (hash >> 3);// qui si fa uso dello xor esclusivo e dello shift a destra e prima cosa si fa lo shift a destra di 3 posizioni dell'hash e poi si fa lo xor esclusivo tra hash e hash shiftato, se sono uguali si mette 0 altrimenti si mette 1 
    }
    return std::to_string(hash);//questo to_string restituisce l'hash in formato stringa
}

//SELEZIONIAMO TUTTI GLI ORARI DISPONIBILI
static crow::json::wvalue* handleDisponibilita(sql::mysql::MySQL_Connection* conn) {

    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;
    crow::json::wvalue* response = new crow::json::wvalue();//oggetto json per la risposta

    try {
        stmt = conn->prepareStatement("SELECT id, giorno_disponibile, ora_disponibile FROM disponibilita ORDER BY giorno_disponibile,ora_disponibile");
        res = stmt->executeQuery();

        std::vector<crow::json::wvalue> disponibilita;//vettore di disponibilita. in libreria ho using list = std::vector<wvalue>;

        //iteriamo su tutti i risultati ottenuti dalla query
        while (res->next()) {
            crow::json::wvalue entry;//qui creiamo un oggetto della classse wvalue
            entry["id"] = res->getInt("id");//qui cosa facciamo per prima cosa? tramite l'override dell'operatore [] io sto creando un campo nell'oggetto wvalue
            entry["giorno_disponibile"] = res->getString("giorno_disponibile");//Assegna la data
            entry["ora_disponibile"] = res->getString("ora_disponibile");//assegna l'ora

            disponibilita.push_back(std::move(entry));//aggiunge la data disponibile nel vettore di oggetti wvalue. con pushback aggiungo un elemento alla fine del vettore
        }
        
        delete res;
        delete stmt;

        //se non ci sono disponibilita, restituiamo un messaggio dicendo che è tutto occupato
        if (disponibilita.empty()) {
            (*response)["success"] = true;
            (*response)["message"] = "non ci sono posti disponibili";
            (*response)["disponibilita"] = crow::json::wvalue::list();
            return response;//qui ho messo una lista vuota che guardando l'implementazione di wvalue è un vettore di oggetti wvalue
        }

        //se invece ci sono disponibilita allora andiamo avanti e facciamo la seguente. questo response, come entry, con l'override delle [], è un oggetto di tipo wvalue 
        (*response)["success"] = true;
        (*response)["message"] = "ci sono posti";
        (*response)["disponibilita"] = std::move(disponibilita);

        return response;
    }catch (sql::SQLException& e) {
        delete stmt;
        delete res;

        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        (*response)["error"] = "generic_error";
        return response;
    }
}

//REGISTRAZIONE UTENTE
static crow::json::wvalue* handleRegistration(sql::mysql::MySQL_Connection* conn, const std::string& nome, const std::string& cognome, const std::string& email, const std::string& password) {
    crow::json::wvalue* response = new crow::json::wvalue();
    
    if (!isValidEmail(email)) {
        (*response)["success"] = false;
        (*response)["message"] = "Email non valida";
        return response;
    }
    if (password.length() < 8) {
        (*response)["success"] = false;
        (*response)["message"] = "La password deve contenere almeno 8 caratteri";
        return response;
    }

    sql::PreparedStatement* stmt = nullptr;//inizialmente inizializzati a nullptr cioe non puntano ad un oggetto, verra creato nel momento in cui li chiamiamo sotto(lo stesso per resultset )ovviamente e verra fatto il relativo new  
    sql::ResultSet* res = nullptr;//ho messo res come resultset perche restituisce un puntatore a resultset quella funzione che poi andro ad eseguire, che pero è una classe astratta. quindi il metodo next e getInt verranno implementati nella classi derivate di resultset che è mysql_Resultset. Una classe è astratta se dichiara almeno un metodo virtuale puro (cioè virtual ... = 0;). Nel tuo header quasi tutti i metodi sono dichiarati come = 0, quindi ResultSet non può essere istanziata direttamente.
    sql::Statement* transStmt = nullptr; // per START/COMMIT/ROLLBACK creati con createStatement(). perche serve? perche appena faccio conn->createStatement() allora viene creato un puntatore di tipo statement che punta ad un oggetto nell'heap e quindi se io non definissi un puntatore del genere e facessi direttamente conn->createStatement()->execute("START TRANSACTION"); allora avrei un memory leak cioe una parte di memoria invisibile e inaccessibile perche non chiamerei di conseguenza il delete.

    try {
        //inizio della transazione
        transStmt= conn->createStatement();
        transStmt->execute("START TRANSACTION");
        delete transStmt;

        //controllo se l'email esiste, bloccando la riga
        stmt = conn->prepareStatement("SELECT email FROM utenti WHERE email = ? FOR UPDATE");
        stmt->setString(1, email);//SOSTUISCE IL PRIMO ? CON email
        res = stmt->executeQuery();//esegue la query

        if (res->next()) { //MySQL_ResultSet eredita sql::ResultSet, quindi il puntatore res può contenere un oggetto MySQL_ResultSet, quindi saranno implementati in mysql_resultset.cpp.
            //se l'email esiste gia
            delete res;
            delete stmt;
            transStmt = conn->createStatement();
            transStmt->execute("ROLLBACK");
            delete transStmt; //annullo la transazione e faccio il delete
            (*response)["success"] = false;
            (*response)["message"] = "Email gia' registrata";
            return response;
        }

        delete res;
        delete stmt;
        
        //vado a crittografare la password con simpleHash
        std::string password_hashata = simpleHash(password);

        //arrivati a questo punto inserisco l'utente nel db, in particolare nella tabella utenti
        stmt = conn->prepareStatement("INSERT INTO utenti (nome, cognome, email, password) VALUES (?, ?, ?, ?)"); // Prepara una query SQL per inserire un nuovo utente
        stmt->setString(1, nome); // Imposta il primo parametro con il valore di nome 
        stmt->setString(2, cognome);
        stmt->setString(3, email);
        stmt->setString(4, password_hashata);
        stmt->executeUpdate(); // Esegue l'inserimento nel database
        delete stmt;
        
        //commit della transazione per salvare i dati
        // commit della transazione
        transStmt = conn->createStatement();
        transStmt->execute("COMMIT");
        delete transStmt;

        (*response)["success"] = true;
        (*response)["message"] = "Registrazione completata con successo";
        // Restituisce un oggetto di tipo wvalue
        return response;
    }
    catch (sql::SQLException& e) { // Gestione delle eccezioni SQL (cppconn/exception.h)
        //rollback in caso di errore
        transStmt = conn->createStatement();
        transStmt->execute("ROLLBACK");
        delete transStmt;
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        //rollback in caso di errore
        transStmt = conn->createStatement();
        transStmt->execute("ROLLBACK");
        delete transStmt;
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//LOGIN UTENTE
static crow::json::wvalue* handleLogin(sql::mysql::MySQL_Connection* conn, const std::string& email, const std::string& password) {
    crow::json::wvalue* response = new crow::json::wvalue();

    if (!isValidEmail(email)) {
        (*response)["success"] = false;
        (*response)["message"] = "Email non valida";
        return response;
    }

    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;

    try {
        stmt = conn->prepareStatement("SELECT password, nome, cognome FROM utenti WHERE email= ?");
        stmt->setString(1, email);
        res = stmt->executeQuery();

        if (!res->next()) {
            delete res;
            delete stmt;
            (*response)["success"] = false;
            (*response)["message"] = "Email o password errata";
            return response;
        }

        std::string password_db = res->getString("password");//si prende il valore della password
        std::string nome = res->getString("nome");
        std::string cognome = res->getString("cognome");

        if (password_db != simpleHash(password)) {
            delete res;
            delete stmt;
            (*response)["success"] = false;
            (*response)["message"] = "email o password errata";
            return response;
        }

        delete res;
        delete stmt;
        (*response)["success"] = true;
        (*response)["message"] = "Login riuscito";
        (*response)["nome"] = nome;
        (*response)["cognome"] = cognome;
        return response;
    }
    catch (sql::SQLException& e) {
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//LOGIN ADMIN
static crow::json::wvalue* handleLoginAdmin(sql::mysql::MySQL_Connection* conn, const std::string& email, const std::string& password) {
    crow::json::wvalue* response = new crow::json::wvalue();

    if (!isValidEmail(email)) {
        (*response)["success"] = false;
        (*response)["message"] = "Email non valida";
        return response;
    }

    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;

    try {
        stmt = conn->prepareStatement("SELECT password, nome, cognome FROM admin WHERE email= ?");
        stmt->setString(1, email);
        res = stmt->executeQuery();

        if (!res->next()) {
            delete res;
            delete stmt;
            (*response)["success"] = false;
            (*response)["message"] = "Email o password errata";
            return response;
        }

        std::string password_db = res->getString("password");//si prende il valore della password
        std::string nome = res->getString("nome");
        std::string cognome = res->getString("cognome");

        if (password_db != simpleHash(password)) {
            delete res;
            delete stmt;
            (*response)["success"] = false;
            (*response)["message"] = "email o password errata";
            return response;
        }

        delete res;
        delete stmt;
        (*response)["success"] = true;
        (*response)["message"] = "Login riuscito";
        (*response)["nome"] = nome;
        (*response)["cognome"] = cognome;
        return response;
    }
    catch (sql::SQLException& e) {
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete stmt;
        delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//FUNZIONE AGGIUNGI PRENOTAZIONE. TUTTI I CONTROLLI VENGONO FATTI NEL C# QUINDI NON HA SENSO RICONTROLLARE IL TUTTO
static crow::json::wvalue* handleAggiungiPrenotazione(sql::mysql::MySQL_Connection* conn, const std::string& email, const std::string& giorno_prenotazione, const std::string& ora_prenotazione, const std::string& servizio, const double& prezzo) {
    crow::json::wvalue* response = new crow::json::wvalue();

    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;
    sql::Statement* transStmt = nullptr;

    try {
        //INIZIO LA TRANsAZIONE
        transStmt = conn->createStatement();
        transStmt->execute("START TRANSACTION");
        delete transStmt;

        //CONTROLLO SE L'ORARIO è DISPONIBILE BLOCCANDOLO E QUINDI ACQUISENDO IL LOCK
        stmt = conn->prepareStatement("SELECT * FROM disponibilita WHERE giorno_disponibile = ? AND ora_disponibile = ? FOR UPDATE");//Il comando FOR UPDATE in SQL serve per bloccare la riga selezionata finché la transazione non è completata. Questo è utile per evitare problemi di concorrenza quando più utenti cercano di prenotare lo stesso orario contemporaneamente.
        stmt->setString(1, giorno_prenotazione);
        stmt->setString(2, ora_prenotazione);
        res = stmt->executeQuery();

        if (!res->next())
        {
            delete res;
            delete stmt;

            //annullo la transazione se l'orario non è disponibile
            stmt = conn->prepareStatement("ROLLBACK");
            stmt->execute();
            delete stmt;

            (*response)["success"] = false;
            (*response)["message"] = "l'orario per il momento sta per essere prenotato da qualcuno";
            return response;
        }

        delete res;
        delete stmt;
    
        //INSERISCO LA PRENOTAZIONE
        stmt = conn->prepareStatement("INSERT INTO prenotazioni (email,giorno_prenotazione,ora_prenotazione,servizio,prezzo) VALUES (?,?,?,?,?)");
        stmt->setString(1, email);
        stmt->setString(2, giorno_prenotazione);
        stmt->setString(3, ora_prenotazione);
        stmt->setString(4, servizio);
        stmt->setDouble(5, prezzo);
        stmt->executeUpdate();
        delete stmt;
        
        //RIMUOVO L'ORARIO DALLA DISPONIBILITA
        stmt = conn->prepareStatement("DELETE FROM disponibilita WHERE giorno_disponibile = ? AND ora_disponibile = ?");
        stmt->setString(1, giorno_prenotazione);
        stmt->setString(2, ora_prenotazione);
        int rowsAffected = stmt->executeUpdate();

        delete stmt;

        if (rowsAffected == 0) {
            transStmt = conn->createStatement();
            transStmt->execute("ROLLBACK");
            delete transStmt;
            (*response)["success"] = false;
            (*response)["message"] = "orario non trovato";
            return response;
        }

        //CONFERMO LA TRANSAZIONE
        transStmt = conn->createStatement();
        transStmt->execute("COMMIT");
        delete transStmt;

        (*response)["success"] = true;
        (*response)["message"] = "PRENOTAZIONE EFFETTUATA";
        return response;
    }
    catch (sql::SQLException& e) {
        //in caso di errore annullo tutto
        transStmt = conn->createStatement();
        transStmt->execute("ROLLBACK");
        delete transStmt;
        if (res) delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        //in caso di errore annullo tutto
        transStmt = conn->createStatement();
        transStmt->execute("ROLLBACK");
        delete transStmt;
        if (res) delete res;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//ELIMINO TUTTE LE PRENOTAZIONI e DISPONIBILITA
static crow::json::wvalue* handleEliminaTutteLePrenotazioniEDisponibilita(sql::mysql::MySQL_Connection* conn) {
    crow::json::wvalue* response = new crow::json::wvalue();

    sql::PreparedStatement* stmtPrenotazioni = nullptr;
    sql::PreparedStatement* stmtDisponibilita = nullptr;
    sql::Statement* transStmt = nullptr;

    try {
        //INIZIO LA TRANsAZIONE
        transStmt = conn->createStatement();
        transStmt->execute("START TRANSACTION");
        delete transStmt;
        
        // Elimina tutte le prenotazioni
        stmtPrenotazioni = conn->prepareStatement("DELETE FROM prenotazioni");
        stmtPrenotazioni->execute();
        delete stmtPrenotazioni;

        // Elimina tutte le disponibilità
        stmtDisponibilita = conn->prepareStatement("DELETE FROM disponibilita");
        stmtDisponibilita->execute();
        delete stmtDisponibilita;

        //CONFERMO LA TRANSAZIONE
        transStmt = conn->createStatement();
        transStmt->execute("COMMIT");
        delete transStmt;

        (*response)["success"] = true;
        (*response)["message"] = "Prenotazioni e disponibilità eliminate con successo.";
        return response;
    }catch(sql::SQLException& e){
        transStmt = conn->createStatement();
        transStmt->execute("ROLLBACK");
        delete transStmt;// Annulla tutte le operazioni in caso di errore

        delete stmtPrenotazioni;
        delete stmtDisponibilita;

        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        transStmt = conn->createStatement();
        transStmt->execute("ROLLBACK");
        delete transStmt;// Annulla tutte le operazioni in caso di errore

        delete stmtPrenotazioni;
        delete stmtDisponibilita;

        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//ELIMINA PRENOTAZIONE. TUTTI I CONTROLLI VENGONO FATTI NEL C# QUINDI NON HA SENSO RICONTROLLARE IL TUTTO
static crow::json::wvalue* handleEliminaPrenotazione(sql::mysql::MySQL_Connection* conn, const std::string& email, const std::string& giorno_prenotazione, const std::string& ora_prenotazione) {
    crow::json::wvalue* response = new crow::json::wvalue();

    sql::PreparedStatement* stmt = nullptr;
    try {
        stmt = conn->prepareStatement("DELETE FROM prenotazioni WHERE email=? AND giorno_prenotazione=? AND ora_prenotazione=?");
        stmt->setString(1,email);
        stmt->setString(2, giorno_prenotazione);
        stmt->setString(3, ora_prenotazione);
        stmt->executeUpdate();
        delete stmt;

        (*response)["success"] = true;
        (*response)["message"] = "PRENOTAZIONE ELIMINATA";
        return response;
    }
    catch (sql::SQLException& e) {
        delete stmt;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete stmt;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//Funzione per recuperare tutte le prenotazioni di un utente
static crow::json::wvalue* handleRecuperaPrenotazioni(sql::mysql::MySQL_Connection* conn, const std::string& email) {
    crow::json::wvalue* response = new crow::json::wvalue();
    
    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;

    try {
        //prepara la query per selezionare le prenotazioni dell'utente ordinandole per data e ora
        stmt = conn->prepareStatement("SELECT giorno_prenotazione, ora_prenotazione, servizio, email FROM prenotazioni WHERE email=? ORDER BY giorno_prenotazione, ora_prenotazione");
        stmt->setString(1, email);
        res = stmt->executeQuery();

        std::vector<crow::json::wvalue> prenotazioni;

        //itero sui risultati della query e costruisce il json di risposta
        while (res->next()) {
            crow::json::wvalue entry;
            entry["giorno_prenotazione"] = res->getString("giorno_prenotazione");
            entry["ora_prenotazione"] = res->getString("ora_prenotazione");
            entry["servizio"] = res->getString("servizio");
            entry["email"] = res->getString("email");

            prenotazioni.push_back(std::move(entry));// Aggiungo l'entry a prenotazioni
        }

        delete res;
        delete stmt;

        //se non ci sono prenotazioni, restituisco un messaggio inerente a cio
        if (prenotazioni.empty()) {
            (*response)["success"] = true;
            (*response)["vuoto"] = true;
            (*response)["message"] = "Nessuna prenotazione trovata";
            return response;
        }

        //costruisco la risposta json con le prenotazioni trovate
        (*response)["success"] = true;
        (*response)["vuoto"] = false;
        (*response)["message"] = "le prenotazioni sono state trovate";
        (*response)["prenotazioni"] = std::move(prenotazioni);

        return response;
    }catch (sql::SQLException& e) {
        delete res;
        delete stmt;
        (*response)["success"] = false;
        (*response)["vuoto"] = true;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete res;
        delete stmt;
        (*response)["success"] = false;
        (*response)["vuoto"] = true;
        (*response)["message"] = e.what();
        return response;
    }
}

//Funzione per recuperare tutte le prenotazioni
static crow::json::wvalue* handleRecuperaTutteLePrenotazioni(sql::mysql::MySQL_Connection* conn) {
    crow::json::wvalue* response = new crow::json::wvalue();

    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;

    try {
        //prepara la query per selezionare le prenotazioni dell'utente ordinandole per data e ora
        stmt = conn->prepareStatement("SELECT giorno_prenotazione, ora_prenotazione, servizio, email FROM prenotazioni ORDER BY giorno_prenotazione, ora_prenotazione");
        res = stmt->executeQuery();

        std::vector<crow::json::wvalue> prenotazioni;

        //itero sui risultati della query e costruisce il json di risposta
        while (res->next()) {
            crow::json::wvalue entry;
            entry["giorno_prenotazione"] = res->getString("giorno_prenotazione");
            entry["ora_prenotazione"] = res->getString("ora_prenotazione");
            entry["servizio"] = res->getString("servizio");
            entry["email"] = res->getString("email");

            prenotazioni.push_back(std::move(entry));// Aggiungo l'entry alla lista di prenotazioni
        }

        delete res;
        delete stmt;

        //se non ci sono prenotazioni, restituisco un messaggio inerente a cio
        if (prenotazioni.empty()) {
            (*response)["success"] = true;
            (*response)["vuoto"] = true;
            (*response)["message"] = "Nessuna prenotazione trovata";
            return response;
        }

        //costruisco la risposta json con le prenotazioni trovate
        (*response)["success"] = true;
        (*response)["vuoto"] = false;
        (*response)["message"] = "le prenotazioni sono state trovate";
        (*response)["prenotazioni"] = std::move(prenotazioni);

        return response;
    }
    catch (sql::SQLException& e) {
        delete res;
        delete stmt;
        (*response)["success"] = false;
        (*response)["vuoto"] = true;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete res;
        delete stmt;
        (*response)["success"] = false;
        (*response)["vuoto"] = true;
        (*response)["message"] = e.what();
        return response;
    }
}

//aggiungo disponibilita
static crow::json::wvalue* handleAggiungiDisponibilita(sql::mysql::MySQL_Connection* conn, const std::string& giorno, const std::string& ora) {
    crow::json::wvalue* response = new crow::json::wvalue();

    sql::PreparedStatement* stmt = nullptr;
    
    try {
        stmt = conn->prepareStatement("INSERT INTO disponibilita (giorno_disponibile, ora_disponibile) VALUES (?, ?)");
        stmt->setString(1, giorno);
        stmt->setString(2, ora);
        stmt->executeUpdate();

        delete stmt;
        (*response)["success"] = true;
        (*response)["message"] = "disponibilità aggiunta con successo";
        return response;
    }
    catch (sql::SQLException& e) {
        delete stmt;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete stmt;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}

//Recupero tutti i clienti registrati
static crow::json::wvalue* handleRecuperaClienti(sql::mysql::MySQL_Connection* conn) {
    crow::json::wvalue *response = new crow::json::wvalue();
    sql::PreparedStatement* stmt = nullptr;
    sql::ResultSet* res = nullptr;

    try {
        // Prepara la query per selezionare tutti i clienti ordinati per email
        stmt = conn->prepareStatement("SELECT email, nome, cognome FROM utenti");
        res = stmt->executeQuery();

        std::vector<crow::json::wvalue> clienti;

        // Itera sui risultati della query e costruisce il JSON di risposta
        while (res->next()) {
            crow::json::wvalue entry;
            entry["email"] = res->getString("email");
            entry["nome"] = res->getString("nome");
            entry["cognome"] = res->getString("cognome");

            clienti.push_back(std::move(entry)); // Aggiunge il cliente alla lista
        }

        delete res;
        delete stmt;

        // Se non ci sono clienti, restituisce un messaggio appropriato
        if (clienti.empty()) {
            (*response)["success"] = true;
            (*response)["vuoto"] = true;
            (*response)["message"] ="Nessun cliente trovato";
            return response;
        }

        // Costruisce la risposta JSON con i clienti trovati
        (*response)["success"] = true;
        (*response)["vuoto"] = false;
        (*response)["message"] = "Clienti trovati";
        (*response)["clienti"] = std::move(clienti);

        return response;
    }
    catch (sql::SQLException& e) {
        delete res;
        delete stmt;
        (*response)["success"] = false;
        (*response)["vuoto"] = true;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete res;
        delete stmt;
        (*response)["success"] = false;
        (*response)["vuoto"] = true;
        (*response)["message"] = e.what();
        return response;
    }
}

//aggiungo le disponibilita impartite dall'admin
static crow::json::wvalue* handleAggiungiDisponibilitaAdmin(sql::mysql::MySQL_Connection* conn, const std::string& giorno) {
    crow::json::wvalue* response = new crow::json::wvalue();
    sql::PreparedStatement* stmt = nullptr;
    std::vector<std::string> ore = { "09:00", "11:00", "15:00", "17:00", "19:00" }; // Ore predefinite per ogni giorno

    try {
        for (const auto& ora : ore) {
            stmt = conn->prepareStatement("INSERT INTO disponibilita (giorno_disponibile, ora_disponibile) VALUES (?, ?)");
            stmt->setString(1, giorno);
            stmt->setString(2, ora);
            stmt->executeUpdate();
            delete stmt;  
        }

        (*response)["success"] = true;
        (*response)["message"] = "Disponibilità aggiunte correttamente per il giorno scelto.";
        return response;
    }
    catch (sql::SQLException& e) {
        delete stmt;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
    catch (std::exception& e) {
        delete stmt;
        (*response)["success"] = false;
        (*response)["message"] = e.what();
        return response;
    }
}


int main() {
    sql::mysql::MySQL_Connection* raw_conn = connectToDatabase();

    //controllo della connessione
    if (!raw_conn) {
        std::cerr << "Impossibile connettersi al database. Terminazione del programma." << std::endl;
        return 1; // Esce con errore
    }

    crow::SimpleApp app; // Inizializza l'applicazione Crow (crow.h) cioe crea un oggetto app della classe crow::SimpleApp, che rappresenta un server web basato su Crow

    // Definizione dell'endpoint REST per la registrazione
    CROW_ROUTE(app, "/register").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body); // Carica il corpo della richiesta JSON (crow/json.h)
        if (!body) { // Controlla se il corpo JSON è valido (crow/json.h)
            return crow::response(400, "Formato JSON non valido."); // Restituisce un errore HTTP 400 (crow/response.h)
        }

        crow::json::wvalue* response = handleRegistration(raw_conn, body["Nome"].s(), body["Cognome"].s(), body["Email"].s(), body["Password"].s()); // Gestisce la registrazione utilizzando i dati JSON (crow/json.h)
        crow::response res(*response);
        delete response;
        return res;// Restituisce la risposta in formato JSON (crow/response.h)
    });

    //definizione dell'endpoint REST per il login
    CROW_ROUTE(app, "/login").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body);
        if (!body) {
            return crow::response(400, "Formato JSON non valido");
        }
        crow::json::wvalue* response = handleLogin(raw_conn, body["Email"].s(), body["Password"].s());
        crow::response res(*response);
        delete response;
        return res;
    });

    //definizione dell'endpoiny REST per il login dell'admin
    CROW_ROUTE(app, "/login_admin").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body);
        if (!body) {
            return crow::response(400, "Formato JSON non valido");
        }
        crow::json::wvalue* response = handleLoginAdmin(raw_conn, body["Email"].s(), body["Password"].s());
        crow::response res(*response);
        delete response;
        return res;
    });

    //AGGIUNGI PRENOTAZIONE
    CROW_ROUTE(app, "/Add_prenotazione").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body);
        if (!body) {
            return crow::response(400, "Formato JSON non valido");
        }
        crow::json::wvalue* response = handleAggiungiPrenotazione(raw_conn, body["Email"].s(), body["Giorno_prenotazione"].s(), body["Ora_prenotazione"].s(), body["Servizio"].s(), body["Prezzo"].d());
        crow::response res(*response);
        delete response;
        return res;
    });
  
    //ELIMINA PRENOTAZIONE
    CROW_ROUTE(app, "/Delete_prenotazione").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body);
        if (!body) {
            return crow::response(400, "Formato JSON non valido");
        }
        crow::json::wvalue* response = handleEliminaPrenotazione(raw_conn, body["Email"].s(), body["Giorno_prenotazione"].s(), body["Ora_prenotazione"].s());
        crow::response res(*response);
        delete response;
        return res;
    });

    //ELIMINO TUTTE LE PRENOTAZIONI
    CROW_ROUTE(app, "/EliminaTutto").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        crow::json::wvalue* response = handleEliminaTutteLePrenotazioniEDisponibilita(raw_conn);
        crow::response res(*response);
        delete response;
        return res;
    });

    //SELEZIONIAMO TUTTI GLI ORARI DISPONIBILI
    CROW_ROUTE(app, "/disponibilita").methods(crow::HTTPMethod::GET)([raw_conn]() {
        crow::json::wvalue* response = handleDisponibilita(raw_conn);
        crow::response res(*response);
        delete response;
        return res;
    });

    //definizione dell'endpoint REST per ottenere le prenotazioni di un utente
    CROW_ROUTE(app, "/prenotazioni").methods(crow::HTTPMethod::GET)([raw_conn](const crow::request& req) {
        auto query_params = crow::query_string(req.url_params);//recupero i parametri della query dall'url(in questo caso solo email)
        auto email = query_params.get("email");
        if (!email) {
            return crow::response(400, "Parametro email mancante");
        }

        crow::json::wvalue* response = handleRecuperaPrenotazioni(raw_conn, email);
        crow::response res(*response);
        delete response;
        return res;
    });
    
    //endpoint per restituire tutte le prenotazioni
    CROW_ROUTE(app, "/Allprenotazioni").methods(crow::HTTPMethod::GET)([raw_conn]() {
        crow::json::wvalue* response = handleRecuperaTutteLePrenotazioni(raw_conn);
        crow::response res(*response);
        delete response;
        return res;
    });

    //aggiungi disponibilita (lo usiamo in c#)
    CROW_ROUTE(app, "/aggiungi_disponibilita").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body);
        if (!body) {
            return crow::response(400, "Formato JSON non valido");
        }
        crow::json::wvalue* response = handleAggiungiDisponibilita(raw_conn, body["Giorno_prenotazione"].s(), body["Ora_prenotazione"].s());
        crow::response res(*response);
        delete response;
        return res;
    });

    //endpoint per aggiungere le disponibilita impartite dall'admin
    CROW_ROUTE(app, "/aggiungi_disponibilita_admin").methods(crow::HTTPMethod::POST)([raw_conn](const crow::request& req) {
        auto body = crow::json::load(req.body);
        if (!body) {
            return crow::response(400, "Formato JSON non valido");
        }
        crow::json::wvalue* response = handleAggiungiDisponibilitaAdmin(raw_conn, body["Giorno_prenotazione"].s());
        crow::response res(*response);
        delete response;
        return res;
    });

    // Endpoint REST per ottenere la lista dei clienti
    CROW_ROUTE(app, "/clienti").methods(crow::HTTPMethod::GET)([raw_conn]() {
        crow::json::wvalue* response = handleRecuperaClienti(raw_conn);
        crow::response res(*response);
        delete response;
        return res;
    });

    app.port(8080).multithreaded().run(); // Configura l'app per ascoltare sulla porta 8080 e supporta connessioni multithreaded (crow.h)

    //pulizia della memoria
    delete raw_conn;
    return 0; // Termina l'applicazione
}
