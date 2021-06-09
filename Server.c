#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include<string.h>
#include <mysql.h>
#include <pthread.h>
#include <time.h>
//Core functions, do not touch
void StartServer(struct sockaddr_in* serverAddress);
void CreateNewThreadForPlayer(int* socket);
void *PlayerLoop(void* socket);
void AskToDataBase(char* request, char* reply);
void LockThread();
void UnlockThread();
int WaitForPlayer();
//PetitionsHandler
void CookRequest(char* request, int PlayerSocketID);
//Utility functions
int GetIndex(int socket);
int GetSocket(char* username);
void NotifyEveryone(char* notification);
void NotifyGroup(int groupIndex, char* notification);
void StartGame();


//Structs Definitions
typedef struct{
	char username[20];
	int socket;
	int currentGroupIndex;
}Player;
typedef struct{
	Player list[100];
	int num;
}ConnectedPlayers;

typedef struct{
	int partnersSockets[4];
	int num;
}Group;
typedef struct{
	Group list[100];
	int num;
}AllGroups;
typedef struct{
	int groupsIndexes[4];//lista de indices de grupos en partida
	int groupsNum;//numero Grupos en partida
}Game;
typedef struct{
	Game list[100];//Lista partidas activas
	int gamesNum;//Num partidas activas
}AllGames;

//Global Variables
int serverSocket;
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
ConnectedPlayers connectedPlayers;
AllGroups allGroups;
AllGames allGames;
int main(int argc, char *argv[]) {
	struct sockaddr_in serverAddress;
	printf("Starting Server\n");
	StartServer(&serverAddress);
	printf("Listening...\n");
	srand ( time(NULL) );
	

	for(;;){
		int incomingSocket = WaitForPlayer();
		printf("We have a new user\n");
		CreateNewThreadForPlayer(&incomingSocket);
	}
	return 0;
}
void StartServer( struct sockaddr_in* serverAddress ){
	serverSocket = socket(AF_INET,SOCK_STREAM,0);
	(*serverAddress).sin_family = AF_INET;
	(*serverAddress).sin_port = htons(7003);
	(*serverAddress).sin_addr.s_addr = INADDR_ANY;
	if(bind(serverSocket, (struct sockaddr*) &(*serverAddress), sizeof(*serverAddress)) < 0)
		printf("Binding Error\n");
	memset(&serverAddress, 0, sizeof(*serverAddress));
	listen(serverSocket, 100);
	connectedPlayers.num = 0;
	allGroups.num = 0;
	allGames.gamesNum = 0;
}
void CreateNewThreadForPlayer(int * socket){
	pthread_t thread;
	LockThread();
	printf("Creating thread for the user with socket: %d\n", *socket);
	pthread_create(&thread,NULL, PlayerLoop, socket);
	UnlockThread();
}
void* PlayerLoop(void* socket){
			int PlayerSocketID = *((int*) socket);
			for(;;){
				printf("Waiting for socket %d  action\n", PlayerSocketID);
				char rawRequest[500];
				int lenghtInBytes = read(PlayerSocketID, rawRequest, sizeof(rawRequest));	
				rawRequest[lenghtInBytes] = '\0';
				if(strlen(rawRequest) >= 2){
					CookRequest(rawRequest, PlayerSocketID);	
				}
			}
		}
void AskToDataBase(char* request, char* reply){
	MYSQL *DBConn = mysql_init(NULL);
	MYSQL_RES* DBResponse;
	DBConn = mysql_real_connect (DBConn, "localhost","root", "mysql", "T6_DCDB", 0, NULL, 0); 
	mysql_query(DBConn, request);
	DBResponse = mysql_store_result(DBConn);
	MYSQL_ROW row;
	if (DBResponse == NULL){
		//No hemos tenido respuesta, comprobamos si se trata de un error o no
		if(mysql_field_count(DBConn) == 0)
		{
			strcpy(reply, "Nothing found, but no errors found\n");
			// query does not return data
			// (it was not a SELECT)
		}
		else{
			//It sould have returned data, error
			printf("Ha ocurrido un error: %s\n",mysql_error(DBConn));
		}
		return;
	}
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
	mysql_free_result(DBResponse);
	mysql_close(DBConn);
}
void LockThread(){
	pthread_mutex_lock(&mutex);
}
void UnlockThread(){
	pthread_mutex_unlock(&mutex);
}	
int WaitForPlayer(){
	int socket =  accept(serverSocket,NULL,NULL);
	return socket;
}
///////////////////////////////////////////////////////
void CookRequest(char* request, int PlayerSocketID){
	printf("We got the request : %s\n", request);
	int mySocket = PlayerSocketID;
	char* tempString = strtok(request,"/");
	int requestID = atoi(tempString);
	if(requestID == 0){//Error code from atoi();
		printf("Ha ocurrido un error con el formato del mensaje recibido\n");
		return;
	}
	switch(requestID){
	case -1:{
		//Desconectar
		//Tell my everyone im out
		char notificationToGroup[350];
		strcpy(notificationToGroup, "5");//add names
		int myGroup = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
		for(int i = 0; i < 4;i++){
			int socket = allGroups.list[myGroup].partnersSockets[i];
			if(socket != -1 && socket != mySocket){
				strcat(notificationToGroup, "/");
				strcat(notificationToGroup,connectedPlayers.list[GetIndex(socket)].username);
			}
		}
		for(int i=0 ; i < 4; i++){
			if(allGroups.list[myGroup].partnersSockets[i] != -1 && allGroups.list[myGroup].partnersSockets[i] != mySocket){
				printf("Telling my guys im out : %s \n", notificationToGroup);
				write(allGroups.list[myGroup].partnersSockets[i], notificationToGroup, sizeof(notificationToGroup));
			}
		}
		LockThread();
		int currentPlayersOnline = connectedPlayers.num;
		int index = GetIndex(mySocket);
		if(index != currentPlayersOnline - 1){
			for (int i = index; i < connectedPlayers.num - 2; i++)
			{
				connectedPlayers.list[i] = connectedPlayers.list[i+1];
			}
		}
		int groupsNum = allGroups.num;
		if(groupsNum - 1 == index){
			allGroups.num--;
		}
		else{
			for(int i = index; i < groupsNum - 2;i++){
				allGroups.list[i] = allGroups.list[i + 1];
			}
		}
		connectedPlayers.num--;
		UnlockThread();
		close(mySocket);
		pthread_exit(0);
		break;
	}
	case 1: //Registro
		break;
	case 2: //Inicio de sesión
		{
		char username[50], password[50],DBRequest[500];
	char credentialsCheck[20], c[40];
	tempString = strtok(NULL,"/");
	strcpy(username,tempString);
	tempString = strtok(NULL,"/");
	strcpy(password,tempString);
	sprintf(DBRequest, "SELECT COUNT(NICKNAME) FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);
	AskToDataBase(DBRequest, credentialsCheck);
	int ans = atoi(credentialsCheck);
	if (ans == 1 && connectedPlayers.num < 100) {
		printf("Se ha iniciado sesión\n");
		//Tell to the new user all the connected players if there are
		//And tell the others that this one connected
		sprintf(DBRequest,"1/%d",connectedPlayers.num);
		if(connectedPlayers.num > 0){
			sprintf(password, "4/1/%s", username);
			for(int i = 0; i < connectedPlayers.num;i++){
				strcat(DBRequest, "/");
				strcat(DBRequest, connectedPlayers.list[i].username);
				//Tell everyone this players is now online
				
				printf("Telling now %s with socket %d that %s is connected\n", connectedPlayers.list[i].username,connectedPlayers.list[i].socket ,username);
				write(connectedPlayers.list[i].socket, password, sizeof(password));
			}
			//Tell this players the connected players
			
		}
		write(mySocket, DBRequest, sizeof(DBRequest));
		//Actually add this user to the connectedList
		LockThread();
		connectedPlayers.list[connectedPlayers.num].socket = mySocket;
		strcpy(connectedPlayers.list[connectedPlayers.num].username, username);
		connectedPlayers.list[connectedPlayers.num].currentGroupIndex = connectedPlayers.num;
		printf("We stored the user called %s with socket %d and group %d\n", username, mySocket, connectedPlayers.num);
		//Create a group for player with the same index
		//The socket index should always be the same as the group index and this group index will be de 
		//default group of this player.
		allGroups.list[allGroups.num].partnersSockets[0] = mySocket;
		connectedPlayers.list[connectedPlayers.num].currentGroupIndex = connectedPlayers.num;
		//Free places are market with a -1
		allGroups.list[allGroups.num].partnersSockets[1] = -1;
		allGroups.list[allGroups.num].partnersSockets[2] = -1;
		allGroups.list[allGroups.num].partnersSockets[3] = -1;
		allGroups.list[allGroups.num].num = 1;
		connectedPlayers.num++;
		allGroups.num++;
		UnlockThread();
	}
	break;
	}
		
	case 3: //Borrar usuario de base de datos
		break;
	case 4://Invitación a grupo
		{
		tempString = strtok(NULL,"/");//Nombre de a quien hemos invitado
		char whoWeInvitedUsername[50];
		strcpy(whoWeInvitedUsername, tempString);
		char invitationString[100];
		int myIndex = GetIndex(mySocket);
		if(myIndex != -1){
			sprintf(invitationString, "2/%s", connectedPlayers.list[myIndex].username);
			int targetSocket = GetSocket(whoWeInvitedUsername);
			if(targetSocket != -1){
				printf("El usuario %s ha invitado a %s que tiene socket %d\n", connectedPlayers.list[myIndex].username, whoWeInvitedUsername, targetSocket);
				write(targetSocket, invitationString, sizeof(invitationString));
			}
		}
	}
		break;
	case 5:{ //Respuesta invitación a grupo
		char ans[5];
		char whoInvitedMeUsername[50];
		tempString = strtok(NULL, "/");
		strcpy(ans, tempString);
		if(strcmp(ans, "Y") == 0){//Accepted
			//Add to group
			tempString = strtok(NULL, "/");
			strcpy(whoInvitedMeUsername, tempString);
			int whoInvitedMeSocket = GetSocket(whoInvitedMeUsername);
			if(whoInvitedMeSocket != -1){
				int whoInvitedMeGroupIndex = connectedPlayers.list[GetIndex(whoInvitedMeSocket)].currentGroupIndex;
				if(allGroups.list[whoInvitedMeGroupIndex].num < 4){
					LockThread();	
					for(int i = 0; i < 4;i++){
						if(allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i] == -1){
							allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i] = mySocket;
							allGroups.list[whoInvitedMeGroupIndex].num++;
							break;
						}
					}
					connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex = whoInvitedMeGroupIndex;
					UnlockThread();
				}
				char notificationToGroup[350];
				strcpy(notificationToGroup, "5");//add names
				for(int i = 0; i < 4;i++){
					int socket = allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i];
					if(socket != -1){
						strcat(notificationToGroup, "/");
						strcat(notificationToGroup,connectedPlayers.list[GetIndex(socket)].username);
					}
				}
				for(int i=0 ; i < 4; i++){
					if(allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i] != -1){
						write(allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i], notificationToGroup, sizeof(notificationToGroup));
					}
				}

			}
			//Communicate the group the new member and the member the new Group
		}
		else{
			//work for later boy
		}
		break;
	}
	case 6: //Mensaje de chat
		tempString = strtok(NULL, "/");
		int type = atoi(tempString);//Type of message
		switch(type){
		case 0 : break;
		case 1 :{//Global message
			tempString = strtok(NULL, "/");//Actual message
			char newMessage[250];
			char actualMessage[250];
			strcpy(actualMessage, tempString);
			sprintf(newMessage,"3/%s/%s/%s","1", connectedPlayers.list[GetIndex(mySocket)].username, actualMessage);
			printf("The message sent is : %s\n", newMessage);
			for(int i = 0; i < connectedPlayers.num;i++){
				write(connectedPlayers.list[i].socket, newMessage, sizeof(newMessage));
			}
			break;
		}

		case 2 : break;
		case 3 : break;
		case 4 : break;
		case 5 : break;
		}
		
		break;
	case 7: {//Buscar partida
		int groupIndex = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
		LockThread();
		Game waitingGame = allGames.list[allGames.gamesNum];
		int added = 0;
		int alreadyGroupsNum = 0;
		if(waitingGame.groupsNum == 0){//No tenemos grupos en la partida aun
			printf("we created a game for the group : %d\n", groupIndex);
			waitingGame.groupsIndexes[0] = groupIndex;
			waitingGame.groupsIndexes[1] = -1;
			waitingGame.groupsIndexes[2] = -1;
			waitingGame.groupsIndexes[3] = -1;
			waitingGame.groupsNum++;
			allGames.list[allGames.gamesNum] = waitingGame;
			NotifyGroup(groupIndex, "9/0/");
		}
		else{
			for(int i = 0;i<4;i++){
				if(waitingGame.groupsIndexes[i] == -1 && added ==0){//Look for an empty group, which value is -1 and replace it with out groupIndex
					printf("We are now storing the new group in the game\n");
					waitingGame.groupsIndexes[i] = groupIndex;
					waitingGame.groupsNum++;
					allGames.list[allGames.gamesNum] = waitingGame;
					added = 1;						
				}
				if(waitingGame.groupsIndexes[i] != -1 && waitingGame.groupsIndexes[i] != groupIndex){//Tell the groups that are already waiting that one more group just got added
					char notificationToGroup[250];
					printf("We are notifying a group that already is in the game\n");
					int targetGroup = waitingGame.groupsIndexes[i];
					strcpy(notificationToGroup, "9/1/");
					NotifyGroup(targetGroup, notificationToGroup);
					alreadyGroupsNum++;					
				}				
			}
			if (allGames.list[allGames.gamesNum].groupsNum == 4){
				printf("Starting game\n");
				StartGame();
			}
			char notificationToOwnGroup[50];
			sprintf(notificationToOwnGroup, "9/%d/",alreadyGroupsNum);
			NotifyGroup(groupIndex, notificationToOwnGroup);
		}
		UnlockThread();
		break;
	}
	case 8 :{//Start the Game
		StartGame();
		break;
	}
	}//Final del switch
	
}
	


void StartGame()
{
	int seed = rand() % 999999 + 100000;
	printf("Seed:%d\n", seed);
	char notificationToGroups[150];
	for (int i = 0; i<4; i++) {
		int targetGroupIndex = allGames.list[allGames.gamesNum].groupsIndexes[i];
		if (targetGroupIndex != -1) {
			sprintf(notificationToGroups, "10/%d/", seed);
			NotifyGroup(targetGroupIndex, notificationToGroups);
		}
	}
	LockThread();
	allGames.gamesNum++;
	UnlockThread();
}

int GetIndex(int socket){
	for(int i = 0; i < connectedPlayers.num;i++){
		if(socket == connectedPlayers.list[i].socket)
			return i;
	}
	return -1;
}
int GetSocket(char* username){
	for(int i = 0; i < connectedPlayers.num;i++){
		if(strcmp(connectedPlayers.list[i].username, username) == 0)
			return connectedPlayers.list[i].socket;
	}
	return -1;
}
void NotifyGroup(int groupIndex, char* notification){
	printf("We are sending : %s to group %d\n", notification, groupIndex);
	Group targetGroup = allGroups.list[groupIndex];
	for(int i=0;i<4;i++){
		int targetSocket = targetGroup.partnersSockets[i];
		if(targetSocket != -1){
			printf("Sent to : %s\n", connectedPlayers.list[GetIndex(targetSocket)].username);
			write(targetSocket, notification,sizeof(notification));	
		}
	}
}
///////////////////////////////////////////////////////


