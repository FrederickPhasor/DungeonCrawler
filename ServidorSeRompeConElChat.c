#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include<string.h>
#include <mysql.h>
#include <pthread.h>
///////////////////////////////////////////////////////////////////////////////functions declaration
void* PlayerLoop(void* socket);
void StartServer(struct sockaddr_in* serverAddress);
void CreateNewThreadForPlayer(int* socket);
void LockThread();
void UnlockThread();
void AskToDataBase(char* request, char* reply);
void GetOnlineUsersConcatenated (char * reply);
void NotifyUpdateOnlineUsers();

void NotifyAllPlayerInGame(int gameIndex, char* notification);
void NotifyGroup(int groupindex, char*notification);
void Invite(int originSocket, int destinationSocket);
void AddNewConnectedPlayer(int socket, char* user);
void AddNewGroup(int socket, char* user);
void GetCurrentPlayersInGroupConcatenated(int groupIndex, char* playersList);
void DisconnectUser (int index);
void NotifyEveryone(char* notification);
void NotifyUpdateGroup(int groupIndex, char* newGroupNames);
void RestorePlayerGroup(int mySocket);
int RemovePlayerFromGroup(int playerSocket);
int GetCurrentGroupOf(int playerIndex);
int AddUserToGroup(int userToAddSocket, int groupIndex);
int WaitForPlayer();
int CookRequest(char* request, int socket);
int GetIndexOf (int socket);
int GetSocketOf(char* username);
int CheckCredentials(char* username, char* password);
//////////////////////////////////////////////////////Structs
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
	int leaderSocket;
	int partnersSockets[3];
	int partnersNum;
	int currentGameIndex;
}Group;
typedef struct{
	Group list[100];
	int num;
}AllGroups;
typedef struct{
	int groupsIndexes[6];
	int seed;
	int groupsNum;
}Game;
typedef struct{
	Game list[100];
	int gamesNum;
}AllGames;
////////////////////////////////////////////////////////////////Global Variables
int serverSocket;
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
ConnectedPlayers connectedPlayers;
AllGroups allGroups;
AllGames allGames;
////////////////////////////////////////////////////////////////////////////Main
int main(int argc, char *argv[]) {
	struct sockaddr_in serverAddress;
	printf("Starting Server\n");
	StartServer(&serverAddress);
	printf("Listening...\n");
	for(;;){
		int incomingSocket = WaitForPlayer();
		printf("We have a new user\n");
		CreateNewThreadForPlayer(&incomingSocket);
	}
	return 0;
}
////////////////////////////////////////////////Player Petition Manage////////////////////////////////////////
int CookRequest(char* rawRequest, int PlayerSocketID){
	printf("Cooking raw request : %s\n", rawRequest);
	char* tempString = strtok(rawRequest,"/");
	printf("A...\n");
	int requestID = atoi(tempString);
	printf("B...\n");
	int mySocket = PlayerSocketID;
	switch(requestID){
	case 0 ://Disconnect
		printf("User with socket : %d wants to disconnect from the server...\n", mySocket);
		LockThread();
		int playerIndex = GetIndexOf(mySocket);
		int currentGroupIndex = GetCurrentGroupOf(playerIndex);
		if( playerIndex != -1){
			DisconnectUser(playerIndex);
			RemovePlayerFromGroup(mySocket);
			RestorePlayerGroup(mySocket);
			UnlockThread();
			char newGroupNames[200];
			strcpy(newGroupNames, "4/");
			GetCurrentPlayersInGroupConcatenated(currentGroupIndex, newGroupNames);
			NotifyUpdateGroup(currentGroupIndex, newGroupNames);
			printf("User with socket : %d has successfully disconnected from server, closing now this thread...\n", mySocket);
		}
		if(connectedPlayers.num > 0){
			NotifyUpdateOnlineUsers();	
		}
		close(mySocket);
		pthread_exit(0);
		break;
	case 1 :{ //sign in
		printf("User with socket : %d wants to sign in, checking it's credentials...\n", mySocket);
		char user[50], password[50],DBRequest[500], notification[5];
		tempString = strtok(NULL,"/");
		strcpy(user,tempString);
		tempString = strtok(NULL,"/");
		strcpy(password,tempString);
		char credentialsCheck[20], c[40];
		sprintf(DBRequest, "SELECT NICKNAME FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", user, password);
		AskToDataBase(DBRequest, credentialsCheck);
		char* a = strtok(credentialsCheck, "/");
		strcpy(notification, "1/-1");
		strcpy(c,a);
		if(c != NULL){
			if(strcmp(c, user) == 0){
				printf("The username is %s\n", user);
				LockThread();
				AddNewConnectedPlayer(mySocket, user);
				AddNewGroup(mySocket, user);
				int myIndex = GetIndexOf(mySocket);
				connectedPlayers.list[myIndex].currentGroupIndex = myIndex;
				printf("Player registred is  :%s\n", connectedPlayers.list[myIndex].username);
				UnlockThread();
				strcpy(notification, "1/0");
				printf("User with socket : %d had a successfull sign in\n", mySocket);
				
			}
			else{
				printf("User with socket : %d introduced wrong credentials\n", mySocket);	
			}
			
		}
		else{
			printf("User with socket : %d introduced no valid credentials\n", mySocket);
		}
		write(mySocket, notification, strlen(notification));
		NotifyUpdateOnlineUsers();
		break;
	}
	case 3:{
			char whoInvitedMeUsername[50];
			tempString = strtok(NULL, "/");
			if(strcmp(tempString, "Y") == 0){
				tempString = strtok(NULL, "/");
				strcpy(whoInvitedMeUsername, tempString);
				printf("Player with socket %d accepted the %s invitation to play\n", mySocket,whoInvitedMeUsername);
				int invitorSocket = GetSocketOf(whoInvitedMeUsername);
				int invitorIndex = GetIndexOf(invitorSocket);
				if(invitorIndex != -1){
					LockThread();
					int result = AddUserToGroup(mySocket, invitorIndex);
					
					if(result == -1){
						
					}
					else{
						//its okay notify the Group
						char newGroupNames[200];
						strcpy(newGroupNames, "\0");
						connectedPlayers.list[GetIndexOf(mySocket)].currentGroupIndex = invitorIndex;
						GetCurrentPlayersInGroupConcatenated(invitorIndex, newGroupNames);
						printf("The current players concatenadted we got is : %s\n", newGroupNames);
						
							if(GetCurrentGroupOf(GetIndexOf(mySocket)) == connectedPlayers.list[invitorIndex].currentGroupIndex){
								printf("We are in the same group, no doubp about it\n");
							}
						NotifyUpdateGroup(invitorIndex, newGroupNames);
					}
					UnlockThread();
				}
			}
			
			break;
		}
	case 2:{ //   2/a quien se ha invitado
				
				char whoWeAreInvitingUsername[50];
				tempString= strtok(NULL, "/");
				strcpy(whoWeAreInvitingUsername, tempString);
				printf("User with socket : %d has invited %s to join the group, telling now %s...\n", mySocket, whoWeAreInvitingUsername, whoWeAreInvitingUsername);
				Invite(mySocket, GetSocketOf(whoWeAreInvitingUsername));
				break;
			}
	case 4:{ //leave group
					int currentGroupIndex = connectedPlayers.list[GetIndexOf(mySocket)].currentGroupIndex;
					printf("User with socket %d is leaving now the group with index : %d\n", mySocket, currentGroupIndex);
					LockThread();
					RemovePlayerFromGroup(mySocket); 
					RestorePlayerGroup(mySocket);
					UnlockThread();
					printf("Notifying the group that the user with socket %d is leaving now\n", mySocket);
					char newGroupNames[200];
					strcpy(newGroupNames, "4/");
					GetCurrentPlayersInGroupConcatenated(currentGroupIndex, newGroupNames);
					NotifyUpdateGroup(currentGroupIndex, newGroupNames);
					break;
				}
	case 5:{//Mensaje chat tipo:10/msgType/: msg
						printf("C...\n");
						int destinationSockets[connectedPlayers.num];
						printf("D...\n");
						char message[200];
						tempString= strtok(NULL, "/");
						int msgType = atoi(tempString);
						tempString = strtok(NULL, "/");
						strcpy(message, tempString);
						printf("E...\n");
						char mySignature[25];
						strcpy(mySignature, connectedPlayers.list[GetIndexOf(mySocket)].username);
						
						char reply[500];
						printf("Second switch number is : %d\n", msgType);
						switch (msgType){
						case 1://general Msg, send to everyone PLAYING with you
							//sprintf(reply, "5/1/%s",user);
							
							break;
						case 2:{//Whisper  : destination/msg
							char user[20];
							tempString = strtok(NULL, "/");
							strcpy(user, tempString);
							int res = GetSocketOf(user);
							if(res == -1){
								write(mySocket,"5/-1",sizeof("5/-1"));
							}
							else{
								sprintf(reply, "5/w/%s",message);
								write(res, message, sizeof(message));
							}
								sprintf(reply, "5/2/%s",user);
								
							break;
						}
						case 3 :{ //All players in group
								int currentGroup = connectedPlayers.list[GetIndexOf(mySocket)].currentGroupIndex;
								
								sprintf(reply,"5/3/%s/%s",mySignature,message);
								printf("CurrentGroup is %d and the reply looks like :%s \n", currentGroup, reply);
								
								if(allGroups.list[currentGroup].partnersNum > 0){
									for(int i=0; allGroups.list[currentGroup].partnersNum;i++){
										write(allGroups.list[currentGroup].partnersSockets[i], reply, sizeof(reply));
									}
								}
								write(allGroups.list[currentGroup].leaderSocket, reply, sizeof(reply));
								
							break;
						case 4 : //All players connected except the ones that are in a game
							sprintf(reply,"5/4/%s/%s",mySignature,message);
							int excludedGroups[allGames.gamesNum * 6];
							int currentNum;
							int totalExcludedGroups = 0;
							for(int i = 0; allGames.gamesNum; i++){
								int groupsInGame = allGames.list[i].groupsNum;
								for(int k = 0; k < groupsInGame;k++){
									excludedGroups[currentNum] = allGames.list[i].groupsIndexes[k];
									totalExcludedGroups++;
									currentNum++;
								}
							}
							for(int j=0;j<connectedPlayers.num;j++){
								int playerCurrentGroup = connectedPlayers.list[j].currentGroupIndex;
								int found = 0; //0 for no, 1 for yes
								int counter=0;
								
								for(int counter = 0;counter < totalExcludedGroups;counter++){
									 if(excludedGroups[counter] == playerCurrentGroup){
										found = 1;
									 }
								}
								if(found == 0){
									write(connectedPlayers.list[j].socket, reply, sizeof(reply));
								}
								
							}
							
							
							
							
							break;
						}
							
						
						
					}
	}
}
}
	////////////////////////////////////////////////Return Functions////////////////////////////////////////////////

int GetSocketOf (char* username){
		for(int i = 0; i < connectedPlayers.num; i++){
			if (strcmp(connectedPlayers.list[i].username, username ) == 0)
			{
				return connectedPlayers.list[i].socket;
			}
		}
		return -1;
	}
int GetIndexOf (int socket){
			for(int i = 0; i < connectedPlayers.num; i++){
				if(connectedPlayers.list[i].socket == socket)
					return i;
			}
			return -1;
		}
void GetOnlineUsersConcatenated (char* reply){
				for (int i=0; i < connectedPlayers.num; i++)
				{
					strcat(reply, connectedPlayers.list[i].username);	
					strcat(reply,"/");
				}
			}
int GetCurrentGroupOf(int playerIndex){
		return connectedPlayers.list[playerIndex].currentGroupIndex;
}
int WaitForPlayer(){
						int socket =  accept(serverSocket,NULL,NULL);
						return socket;
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
									strcpy(reply, "Nothing found, but no errors foung\n");
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
int AddUserToGroup(int userSocket, int groupIndex){
	int currentPartners = allGroups.list[groupIndex].partnersNum;
	if(currentPartners < 3){
		allGroups.list[groupIndex].partnersSockets[currentPartners] = userSocket;
		allGroups.list[groupIndex].partnersNum++;
		printf("El grupo se ha aumentado a %d\n", allGroups.list[groupIndex].partnersNum);
		
		return 0;
	}
	return -1;
}						


void GetCurrentPlayersInGroupConcatenated(int groupIndex, char* outputChar){
	int partnersNum = allGroups.list[groupIndex].partnersNum;
	if(partnersNum == 0){
		strcat(outputChar, "EMPTY");
		strcat(outputChar, "/");
	}
	for(int i = 0; i < partnersNum; i++){
		strcat(outputChar, connectedPlayers.list[GetIndexOf(allGroups.list[groupIndex].partnersSockets[i])].username);
		printf("Adding user :%s\n",connectedPlayers.list[GetIndexOf(allGroups.list[groupIndex].partnersSockets[i])].username);
		strcat(outputChar, "/");
	}
	strcat(outputChar, connectedPlayers.list[GetIndexOf(allGroups.list[groupIndex].leaderSocket)].username);
	strcat(outputChar, "/");
	
}
////////////////////////////////////////////////COMMUNICATION WITH PLAYERS FUNCTIONS///////////////////////////	

void NotifyUpdateGroup(int groupIndex, char* newGroupNames){
	char notification[100];
	strcpy(notification, "4/");
	strcat(notification, newGroupNames);
	for(int i = 0; i < allGroups.list[groupIndex].partnersNum; i++){//notifica a los demás partners
		write(allGroups.list[groupIndex].partnersSockets[i], notification, sizeof(notification));
	}
	write(allGroups.list[groupIndex].leaderSocket, notification, sizeof(notification));
}
void NotifyUpdateOnlineUsers(){
	char reply[300];
	strcpy(reply,"2/");
	GetOnlineUsersConcatenated(reply);
	NotifyEveryone(reply);
}
void NotifyEveryone(char* notification){
	for(int i = 0; i < connectedPlayers.num; i++){
		write(connectedPlayers.list[i].socket, notification, strlen(notification));
}

													}
void Invite(int originSocket, int destinationSocket){
	char invitationRequest[100];
	sprintf(invitationRequest, "3/%s",connectedPlayers.list[GetIndexOf(originSocket)].username);
	write(destinationSocket, invitationRequest, sizeof(invitationRequest));
}
void NotifyGroup(int groupindex, char*notification){
	
}
///////////////////////////////////////////////STRUCTS MANIPULATION FUNCTIONS////////////////////////////
void AddNewConnectedPlayer(int socket, char* usernameToAdd){
	printf("New user to add :%s\n", usernameToAdd);
	int currentConnectedPlayersNum = connectedPlayers.num;
	connectedPlayers.list[currentConnectedPlayersNum].socket = socket;
	strcpy(connectedPlayers.list[currentConnectedPlayersNum].username, usernameToAdd);
	connectedPlayers.num++;
}
void AddNewGroup(int mySocket, char* usernameToAdd){
	int currentGroupNum = allGroups.num;
	allGroups.list[currentGroupNum].leaderSocket = mySocket;
	allGroups.list[currentGroupNum].partnersNum = 0;
	allGroups.num++;
}
void RestorePlayerGroup(int playerSocket){
	//Player goes back to its original Group
	int playerIndex = GetIndexOf(playerSocket);
	connectedPlayers.list[playerIndex].currentGroupIndex = playerIndex;
}
int RemovePlayerFromGroup(int playerSocket){
	int playerCurrentGroup = connectedPlayers.list[GetIndexOf(playerSocket)].currentGroupIndex;
	int currenPartnersInGroup = allGroups.list[playerCurrentGroup].partnersNum;
	for(int i = 0; i < currenPartnersInGroup; i++){
		if(allGroups.list[playerCurrentGroup].partnersSockets[i] == playerSocket){
			//le hemos encontrado en este grupo
			if(i != currenPartnersInGroup - 1){
				while(i < currenPartnersInGroup -1){
					allGroups.list[playerCurrentGroup].partnersSockets[i] = allGroups.list[playerCurrentGroup].partnersSockets[i+1];
					i++;
				}
			}
			allGroups.list[currenPartnersInGroup].partnersNum--;
			return 0;
		}
	}
	return -1;
}
void DisconnectUser (int index){
	for (int i = index; i < connectedPlayers.num - 1; i++)
	{
		connectedPlayers.list[i] = connectedPlayers.list[i+1];
	}
	connectedPlayers.num--;
}																		
///////////////////////////////////////////////SERVER RELATED FUNCTIONS////////////////////////////
void StartServer( struct sockaddr_in* serverAddress ){
	serverSocket = socket(AF_INET,SOCK_STREAM,0);
	(*serverAddress).sin_family = AF_INET;
	(*serverAddress).sin_port = htons(9091);
	(*serverAddress).sin_addr.s_addr = INADDR_ANY;
	if(bind(serverSocket, (struct sockaddr*) &(*serverAddress), sizeof(*serverAddress)) < 0)
		printf("Binding Error\n");
	memset(&serverAddress, 0, sizeof(*serverAddress));
	listen(serverSocket, 100);
	connectedPlayers.num = 0;
	allGroups.num = 0;
}
void CreateNewThreadForPlayer(int * socket){
	pthread_t thread;
	LockThread();
	printf("Creating thread for the user with socket: %d\n", *socket);
	pthread_create(&thread,NULL, PlayerLoop, socket);
	UnlockThread();
}
void LockThread(){
	pthread_mutex_lock(&mutex);
}
void UnlockThread(){
	pthread_mutex_unlock(&mutex);
}	
	void* PlayerLoop(void* socket){
																							
	int PlayerSocketID = *((int*) socket);
	printf("User must first login\n");
	for(;;){
		printf("Waiting for user action\n");
		char rawRequest[900];
		int lenghtInBytes = read(PlayerSocketID, rawRequest, sizeof(rawRequest));	
		rawRequest[lenghtInBytes] = '\0';
		CookRequest(rawRequest, PlayerSocketID);
	}
}																						
