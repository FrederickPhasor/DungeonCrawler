
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
int CookRequest(char* request, char* reply, int socket);
int GetIndexOf (int socket);
int DeleteUser (int index);
void AskToDataBase(char* request, char* reply);
int CheckCredentials(char* username, char* password);
void OnlineUsers (char * reply);
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
	(*serverAddress).sin_port = htons(9003);/////////////////////////////////////////////////////////////////////////////
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
	printf("Before creating Socket %d\n", *socket);
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
	
	int socketConnection = *((int*) socket);
	for(;;){
		char rawRequest[100];
		printf("Thread Waiting\n");
		int lenghtInBytes = read(socketConnection, rawRequest, sizeof(rawRequest));	
		rawRequest[lenghtInBytes] = '\0'; //End of line
		char request[100];
		char reply[100];
		int ans = CookRequest(rawRequest, request, socketConnection);//returns 1 if we must not return data from database, -1 if disconnect
		if(ans == -1){
			printf("El usurio quiere desconectarse\n");
			LockThread();
			int res = DeleteUser(GetIndexOf(socketConnection));
			UnlockThread();
			close(socketConnection);
			printf("Thread cerrado\n");
			
			//WIP
			exit(0);
		}
		else{
			if(ans == 0){
				//enviarle la request a la base de datos
				LockThread();
				AskToDataBase(request, reply);
				UnlockThread();
			}
			write(socketConnection, reply, strlen(reply));
		}
		
	}
}
int CookRequest(char* rawRequest, char* request, int socket){
	char username[22];
	char email[22];
	char password[16];
	char newEmail[22];
	char* string = strtok(rawRequest,"/");
	int number = atoi(string);
	switch(number){
	case 0 ://disconect
		return -1;
	case 1 ://sign up
		string = strtok(NULL,"/");
		strcpy(username,string);
		string = strtok(NULL,"/");
		strcpy(email,string);
		string = strtok(NULL,"/");
		strcpy(password,string);
		sprintf(request, "INSERT INTO PLAYERS VALUES (NULL,'%s','%s','%s');", username,email, password);
		return 0;
	case 2 : //sign in
		//work in progress
		string = strtok(NULL,"/");
		strcpy(username,string);
		string = strtok(NULL,"/");
		strcpy(password,string);
		char credentialsCheck[10];
		sprintf(request, "SELECT NICKNAME FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);
		AskToDataBase(request, credentialsCheck);
		printf("La resputesta es : %s\n", credentialsCheck);
		char* a = strtok(credentialsCheck, "/");
		char c[20];
		strcpy(c,a);
		printf("El usuario encontrado es %s",c);
		if(strcmp(c, username) == 0){
			printf("Se ha iniciado sesi'on");
			LockThread();
			connectedClients.list[connectedClients.num].socket = socket;
			strcpy(connectedClients.list[connectedClients.num].name, username);
			connectedClients.num++;
			for(int i = 0; i < connectedClients.num ;i++){
				printf("%s",connectedClients.list[i].name);
			}
			UnlockThread();
		}
		break;
	case 3 : //change password
		string = strtok(NULL, "/");
		strcpy(email,string);
		string = strtok(NULL, "/");
		strcpy(password, string);
		sprintf (request, "UPDATE PLAYERS SET PASSWORD = REPLACE(PASSWORD, PASSWORD, '%s') WHERE PLAYERS.EMAIL = '%s'", password, email);
		return 0;
	case 4 : //change email
		string = strtok(NULL, "/");
		strcpy(email, string);
		string = strtok(NULL, "/");
		strcpy(newEmail, string);
		string = strtok(NULL, "/");
		strcpy(password, string);
		sprintf (request, "UPDATE PLAYERS SET EMAIL = REPLACE(EMAIL, EMAIL, '%s') WHERE PLAYERS.PASSWORD = '%s' AND PLAYERS.EMAIL = '%s'", newEmail, password, email);
		return 0;
	case 5 : //Get Recent Players Faced By a Player Given As a Parameter
		string= strtok(NULL, "/");
		strcpy(username, string);
		sprintf(request, "SELECT NICKNAME FROM PLAYERS WHERE ID IN(SELECT ID FROM PLAYERS WHERE PLAYERS.NICKNAME != '%s' "
				"AND ID IN(SELECT PLAYER1 FROM RECORD WHERE GAME_ID IN(SELECT GAME_ID FROM RECORD WHERE PLAYER1 IN (SELECT ID FROM PLAYERS WHERE NICKNAME = '%s'))));", username, username);
		return 0;
	case 6 : //returns as a string all curent players online
		printf("Se ha pedido lista de usuarios");
		char test[100];
		OnlineUsers(test);
		strcpy(request, test);
		return 1;
		default : ;
	}
}
void UnlockThread(){
	pthread_mutex_unlock(&mutex);
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
		connectedClients.num--;
		return 0;
	}
}
int GetIndexOf (int socket){
	int i = 0;
	int find = 0;
	while (( i < connectedClients.num) && !find)
	{
		connectedClients.list[i].socket == socket ? find = 1 : i++;
	}
	if (find)
		return i;
	else
		return -1;
}
void OnlineUsers (char* reply){
	int i;
	strcpy(reply,"\0");
	for (i=0; i < connectedClients.num; i++)
	{
		strcat(reply, connectedClients.list[i].name);
		strcat(reply,"/");
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
			strcpy(reply, "No hay resultados.");
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
			strcat(reply,"/");//indica salto de columna
		}
		row = mysql_fetch_row (DBResponse);
	}
	//devolver string con la respuestas
	mysql_free_result(DBResponse);
	mysql_close(DBConn);
}
