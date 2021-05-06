#include <stdio.h>

#include <stdlib.h>

#include <sys/types.h>

#include <sys/socket.h>

#include <netinet/in.h>

#include <unistd.h>

#include<string.h>

#include <mysql.h>

#include <pthread.h>

#include <my_global.h>

////////////////////////

//Structs

typedef struct{

	char name[20];

	int socket;

}Client;

typedef struct{

	Client list[100];

	int num;

}ConnectedClients;

pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;

////////////////////////

//functions

void StartServer(struct sockaddr_in* serverAddress);//Initializes the server values;

int WaitForClient();//WaitsForAClientAndReturnsItsSocket

void CreateNewThreadForClient(int* socket);

void LockThread();

void AddNewUser(int socket);

void UnlockThread();

void* ClientLoop(void* socket);

int CookRequest(char* request, int socket);

int GetIndexOf (int socket);

int DeleteUser (int index);

void AskToDataBase(char* request, char* reply);

int CheckCredentials(char* username, char* password);

void GetOnlineUsers (char * reply);

void UpdateOnlineUsersAllClients();

////////////////////////

//Global Variables

int serverSocket;

ConnectedClients connectedClients;//Lista global en donde se guardan los conectados actuales.

////////////////////////

int main(int argc, char *argv[]) {

	struct sockaddr_in serverAddress;

	StartServer(&serverAddress);

	for(;;){

		printf("Listening\n");

		int incomingSocket = WaitForClient();

		CreateNewThreadForClient(&incomingSocket);

	}

	return 0;

}

void StartServer( struct sockaddr_in* serverAddress ){

	serverSocket = socket(AF_INET,SOCK_STREAM,0);

	(*serverAddress).sin_family = AF_INET;

	(*serverAddress).sin_port = htons(50006);/////////////////////////////////////////////////////////////////////////////

	(*serverAddress).sin_addr.s_addr = INADDR_ANY;

	if(bind(serverSocket, (struct sockaddr*) &(*serverAddress), sizeof(*serverAddress)) < 0)

		printf("Error en el bind\n");

	memset(&serverAddress, 0, sizeof(*serverAddress));

	listen(serverSocket, 100);

	connectedClients.num = 0;

}

int WaitForClient(){

	int socket =  accept(serverSocket,NULL,NULL);

	return socket;

}

void CreateNewThreadForClient(int * socket){

	pthread_t thread;

	LockThread();

	printf("Creating Thread with socket: %d\n", *socket);

	pthread_create(&thread,NULL, ClientLoop,socket);

	UnlockThread();

}

void LockThread(){

	pthread_mutex_lock(&mutex);

}

void AddNewUser(int s){

	connectedClients.list[connectedClients.num].socket = s;

	connectedClients.num++;

}

void* ClientLoop(void* socket){

	

	int clientSocketID = *((int*) socket);

	for(;;){

		printf("Inicio del thread\n");

		char rawRequest[500];

		int lenghtInBytes = read(clientSocketID, rawRequest, sizeof(rawRequest));	

		rawRequest[lenghtInBytes] = '\0';

		CookRequest(rawRequest, clientSocketID);

	}

	printf("Final del thread de este usuario\n");

}

int CookRequest(char* rawRequest, int clientSocketID){

	char* tempString = strtok(rawRequest,"/");

	int requestID = atoi(tempString);

	char DBRequest[500];

	char reply[500];

	char email[50];

	char username[50];

	char password[50];

	char newEmail[50];

	char temp[500];

	int socketBuffer = clientSocketID;

	printf("\nWe stored the soket : %d", socketBuffer);

	switch(requestID){

	case 0 ://Disconnect

		LockThread();

		DeleteUser(GetIndexOf(clientSocketID));

		UnlockThread();

		close(clientSocketID);

		UpdateOnlineUsersAllClients();

		pthread_exit(0);

		break;

	case 1 ://sign up

		tempString = strtok(NULL,"/");

		strcpy(username,tempString);

		tempString = strtok(NULL,"/");

		strcpy(email,tempString);

		tempString = strtok(NULL,"/");

		strcpy(password,tempString);

		sprintf(DBRequest, "INSERT INTO PLAYERS VALUES (NULL,'%s','%s','%s');", username,email, password);

		AskToDataBase(DBRequest, reply);

		sprintf(reply, "%d/1", requestID);

		write(clientSocketID, reply, strlen(reply));

		UpdateOnlineUsersAllClients();

		//Lo que se envia es un string diciendo que todo ha ido bien o que ha fallado la operaci?n

		break;

	case 2 : //sign in

		tempString = strtok(NULL,"/");

		strcpy(username,tempString);

		tempString = strtok(NULL,"/");

		strcpy(password,tempString);

		char credentialsCheck[19];

		sprintf(DBRequest, "SELECT NICKNAME FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);

		AskToDataBase(DBRequest, credentialsCheck);

		char* a = strtok(credentialsCheck, "/");

		char c[40];

		strcpy(c,a);

		if(strcmp(c, username) == 0){

			printf("Vamos a guardar");

			LockThread();

			connectedClients.list[connectedClients.num].socket = socketBuffer;

			strcpy(connectedClients.list[connectedClients.num].name, username);

			connectedClients.num++;

			UnlockThread();

			printf("Socket %d guardado en el ?ndice : %d", connectedClients.list[connectedClients.num - 1].socket, connectedClients.num - 1);

			sprintf(reply, "%d/0", requestID);

			write(socketBuffer, reply, strlen(reply));

			UpdateOnlineUsersAllClients();

		}

		else{

			printf("error al guardar");

			sprintf(reply, "%d/-1", requestID);

			write(socketBuffer, reply, strlen(reply));

		}

		

		break;

	case 3 : //change password ///Yo quitar??a esta funci??n, no tenemos sistema para evitar robos de cuentas

		tempString = strtok(NULL, "/");

		strcpy(email,tempString);

		tempString = strtok(NULL, "/");

		strcpy(password, tempString);

		sprintf (DBRequest, "UPDATE PLAYERS SET PASSWORD = REPLACE(PASSWORD, PASSWORD, '%s') WHERE PLAYERS.EMAIL = '%s'", password, email);

		break;

	case 4 : //change email //Tambi??n la quitar??a, no tiene mucho sentido

		tempString = strtok(NULL, "/");

		strcpy(email, tempString);

		tempString = strtok(NULL, "/");

		strcpy(newEmail, tempString);

		tempString = strtok(NULL, "/");

		strcpy(password, tempString);

		sprintf (DBRequest, "UPDATE PLAYERS SET EMAIL = REPLACE(EMAIL, EMAIL, '%s') WHERE PLAYERS.PASSWORD = '%s' AND PLAYERS.EMAIL = '%s'", newEmail, password, email);

		break;

	case 5 : //Get Recent Players Faced By a Player Given As a Parameter

		tempString= strtok(NULL, "/");

		strcpy(username, tempString);

		sprintf(DBRequest, "SELECT NICKNAME FROM PLAYERS WHERE ID IN(SELECT ID FROM PLAYERS WHERE PLAYERS.NICKNAME != '%s' "

				"AND ID IN(SELECT PLAYER1 FROM RECORD WHERE GAME_ID IN(SELECT GAME_ID FROM RECORD WHERE PLAYER1 IN (SELECT ID FROM PLAYERS WHERE NICKNAME = '%s'))));", username, username);

		char temp[250];

		sprintf(temp, "%d/", requestID);

		AskToDataBase(DBRequest, reply);

		strcat(temp, reply);

		break;

		default : ;

	}

}

void UnlockThread(){

	pthread_mutex_unlock(&mutex);

}

void UpdateOnlineUsersAllClients(){

	char temp[300];

	GetOnlineUsers(temp);

	char reply[300];

	sprintf(reply, "6/%s",temp);

	for(int i = 0; i < connectedClients.num; i++){

		write(connectedClients.list[i].socket, reply, strlen(reply));

	}

}

int DeleteUser (int index){

	if (index == -1)

		return -1;

	else

	{

		for (int i=index; i < connectedClients.num - 1; i++)

		{

			connectedClients.list[i] = connectedClients.list[i+1];

		}

		printf("\nWe seleted user with index : %d", index);

		connectedClients.num--;

		return 0;

	}

}

int GetIndexOf (int socket){

	int i = 0;

	int find = 0;

	while (( i < connectedClients.num) && !find)

	{

		printf("Buscando usuario con socket : %d\n", socket);

		connectedClients.list[i].socket == socket ? find = 1 : i++;

	}

	if (find)

		return i;

	else{

		printf("Not found\n");

		return -1;

	}

}

void GetOnlineUsers (char* reply){

	int i;

	strcpy(reply,"\0");

	for (i=0; i < connectedClients.num; i++)

	{

		if(connectedClients.list[i].name != NULL){

			strcat(reply, connectedClients.list[i].name);	

			strcat(reply,"/");

		}

	}

}

void AskToDataBase(char* request, char* reply){

	printf("\nComando para la base de datos : %s\n", request);

	MYSQL *DBConn = mysql_init(NULL);

	MYSQL_RES* DBResponse;

	DBConn = mysql_real_connect (DBConn, "shiva2.upc.es","root", "mysql", "T6_DCDB", 0, NULL, 0); 

	mysql_query(DBConn, request);

	DBResponse = mysql_store_result(DBConn);

	MYSQL_ROW row;

	if (DBResponse == NULL){

		//No hemos tenido respuesta, comprobamos si se trata de un error o no

		if(mysql_field_count(DBConn) == 0)

		{

			strcpy(reply, "No hay resultados. Todo correcto en principio");

			// query does not return data

			// (it was not a SELECT)

		}

		else{

			//It sould have returned data, error

			printf("Ha ocurrido un error: %s",mysql_error(DBConn));

		}

		return;

	}

	printf("\n Row no ha sido nulo\n");

	row = mysql_fetch_row (DBResponse);

	strcpy(reply,"\0");

	int totalColumns = mysql_field_count(DBConn);

	int totalRows =  mysql_num_rows(DBResponse);

	for(int i = 0;i < totalRows;i++){

		for(int j = 0;j < totalColumns;j++){

			strcat(reply,row[j]);

			if(j != totalColumns)

				strcat(reply,"/");//indica salto de columna

		}

		row = mysql_fetch_row (DBResponse);

	}

	printf("\n SALIDO DEL BUCLE\n");

	//devolver string con la respuestas

	mysql_free_result(DBResponse);

	mysql_close(DBConn);

}