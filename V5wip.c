#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include<string.h>
#include <mysql.h>
#include <pthread.h>
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

typedef struct{
	Client players[4];
	int num;
}Grupo;
typedef struct{
	Grupo grupos[100];
	int num;
}TodosLosGrupos;
typedef struct{
	Grupo Grupos[12];
	int num;
	int numpersonas; //<12
}Partida;
typedef struct{
	Partida list[100];
	int num;
}ListaPartidasActivas;

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
int GetIndexOf2(char* name);
int DeleteUser (int index);
void AskToDataBase(char* request, char* reply);
int CheckCredentials(char* username, char* password);
void GetOnlineUsers (char * reply);
void UpdateOnlineUsersAllClients();
////////////////////////
//Global Variables
int serverSocket;
ConnectedClients connectedClients;//Lista global en donde se guardan los conectados actuales.
TodosLosGrupos todosLosGrupos;
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
	(*serverAddress).sin_port = htons(9064);/////////////////////////////////////////////////////////////////////////////
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
		char rawRequest[900];
		int lenghtInBytes = read(clientSocketID, rawRequest, sizeof(rawRequest));	
		printf("\n Antes del fin");
		rawRequest[lenghtInBytes] = '\0';
		printf("\n despu'es del fin");
		CookRequest(rawRequest, clientSocketID);
	}
	printf("Final del thread de este usuario\n");
}
int CookRequest(char* rawRequest, int clientSocketID){
	printf("\ninicio del fuiltro");
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
		//Lo que se envia es un string diciendo que todo ha ido bien o que ha fallado la operaciï¿³n
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
			//Se pone en la lista de conectados
			connectedClients.list[connectedClients.num].socket = socketBuffer;
			strcpy(connectedClients.list[connectedClients.num].name, username);
			//se crea un grupo para el solo con su mismo indice
			todosLosGrupos.grupos[connectedClients.num].players[0].socket = socketBuffer;//Metemos en la posicion 0 el socket del cliente recien conectado
			todosLosGrupos.grupos[connectedClients.num].num++;
			todosLosGrupos.num++;
			connectedClients.num++;
			UnlockThread();
			printf("Socket %d guardado en el ï¿­ndice : %d", connectedClients.list[connectedClients.num - 1].socket, connectedClients.num - 1);
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
	case 3 : //change password ///Yo quitarï¿ƒï¾­a esta funciï¿ƒï¾³n, no tenemos sistema para evitar robos de cuentas
		tempString = strtok(NULL, "/");
		strcpy(email,tempString);
		tempString = strtok(NULL, "/");
		strcpy(password, tempString);
		sprintf (DBRequest, "UPDATE PLAYERS SET PASSWORD = REPLACE(PASSWORD, PASSWORD, '%s') WHERE PLAYERS.EMAIL = '%s'", password, email);
		break;
	case 4 : //change email //Tambiï¿ƒï¾©n la quitarï¿ƒï¾­a, no tiene mucho sentido
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
	case 7: // Respuesta a invitación 7/Y/NombreDelQueMeHaInvitado
		printf("Caso 7 ");
		tempString= strtok(NULL, "/");
		strcpy(username, tempString); //Tomamos la respuesta
		
		if(strcmp(username, "Y") == 0){
			printf("El jugador ha aceptado");
			//Ha dicho que si quiere jugar
			tempString= strtok(NULL, "/");
			strcpy(username, tempString); //Tomamos el nombre de a quien le ha aceptado
			printf("El usuario es %s", username);
			int indexDelInvitador = GetIndexOf2(username);
			//printf("\nEl usuario : %s ha aceptado una solicitud de : %s", connectedClients.list[GetIndexOf(socketBuffer)].name, username);
			//Accedemos al grupo del invitador
			if(todosLosGrupos.grupos[indexDelInvitador].num < 4){//Comprobamos que tenga espacio para almenos uno mas
				printf("Tenemos un hueco");
				LockThread();
				todosLosGrupos.grupos[indexDelInvitador].players[todosLosGrupos.grupos[indexDelInvitador].num].socket = socketBuffer;//Metemos nuestro socket en el Grupo
				strcpy(todosLosGrupos.grupos[indexDelInvitador].players[todosLosGrupos.grupos[indexDelInvitador].num].name, connectedClients.list[socketBuffer].name);//Metemos nuestro nombre
				todosLosGrupos.grupos[indexDelInvitador].num++;
				UnlockThread();
				//Notificamos a todos los del grupo que se ha unido alguien con 8/player1/player2/etc
				char notification[100];
				strcpy(notification, "8/");
				printf("Tenemos un hueco2");
				for(int i = 0; i < todosLosGrupos.grupos[indexDelInvitador].num; i++){
					strcat(notification,todosLosGrupos.grupos[indexDelInvitador].players[i].name);
					strcat(notification,"/");
					
				}
				strcat(notification, '\0');
				//Enviamos a todos el string con los integrantes
				for(int j = 0; j < todosLosGrupos.grupos[indexDelInvitador].num; j++){
					write(todosLosGrupos.grupos[indexDelInvitador].players[j].socket, notification, sizeof(notification));
				}
			}
			else{
				//Enviar al invitado el mensaje de que el grupo esta lleno.
			}
			
			
		}
	case 8: // Invitación   8/a quien se ha invitado
		tempString= strtok(NULL, "/");
		strcpy(username, tempString); //Tomamos el nombre del invitado
		//Le pasamos al invitado 7/(nombre(socketbuffer))
		int socketInvitado = GetIndexOf2(username);
		sprintf(DBRequest, "7/%s\0",connectedClients.list[GetIndexOf(socketBuffer)].name);
		printf("\nSe ha enviado una invitacion a : %s",connectedClients.list[GetIndexOf(socketInvitado)].name);
		write(socketInvitado, DBRequest, sizeof(DBRequest));
	}
}
void notificarSobreJugadores(int Grupo) {
	
}

// FUNCION NOTIFICACION
//Hacer un string con  todos los integrantes del grupo
//Almacenar cada socket en un int sockets[]
//Hacer un loop en los sockets y enviarles a cada uno el string con todos los integrantes
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

int GetIndexOf2 (char name[20]){
	int i = 0;
	int find = 0;
	while (( i < connectedClients.num) && !find)
	{
		if (strcmp(connectedClients.list[i].name, name) == 0)
			return connectedClients.list[i].socket;
	}
	printf("Not found\n");
	return -1;
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
	DBConn = mysql_real_connect (DBConn, "localhost","root", "mysql", "DCDB", 0, NULL, 0); 
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

