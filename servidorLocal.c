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
void* PlayerLoop(void* socket);
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
void StartFightingEvent(int fightIndex);
int CreateFight(int group1Index, int group2Index);
int JoinFight(int newGroup, int fightIndex);
int GetFightIndex(int groupIndex);
void UpdateFight(int fightIndex, char* notification);
//Structs Definitions
typedef struct {
	char username[20];
	int socket;
	int currentGroupIndex;
	int orderInGroup;
}Player;
typedef struct {
	Player list[100];
	int num;
}ConnectedPlayers;

typedef struct {
	int partnersSockets[4];
	int num;
	char currentCoordinate[4];
	int gameIndex;
	int fightIndex;
}Group;
typedef struct {
	Group list[100];
	int num;
}AllGroups;
typedef struct {
	int groupsIndexes[4]; // Lista de ￭ndices de grupos en partida
	int groupsNum; // N￺mero grupos en partida
}Game;
typedef struct {
	Game list[100]; // Lista partidas activas
	int gamesNum; // N￺mero partidas activas
}AllGames;
typedef struct {
	int groupsInvolvedIndexes[4];
	int groupsInvolvedNum;
	int currentTurnNumber;
	int currentPlayerIndex;

}Fight;
typedef struct {
	Fight list[200];
	int num;//current amount of fights
}AllFights;

//Global Variables
int serverSocket;
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
ConnectedPlayers connectedPlayers;
AllGroups allGroups;
AllGames allGames;
AllFights allFights;
int main(int argc, char* argv[]) {
	struct sockaddr_in serverAddress;
	printf("Starting Server\n");
	StartServer(&serverAddress);
	printf("Listening...\n");
	srand(time(NULL));
	for (;;) {
		int incomingSocket = WaitForPlayer();
		printf("We have a new user\n");
		CreateNewThreadForPlayer(&incomingSocket);
	}
	return 0;
}
void StartServer(struct sockaddr_in* serverAddress) {
	serverSocket = socket(AF_INET, SOCK_STREAM, 0);
	(*serverAddress).sin_family = AF_INET;
	(*serverAddress).sin_port = htons(7004);
	(*serverAddress).sin_addr.s_addr = INADDR_ANY;
	if (bind(serverSocket, (struct sockaddr*)&(*serverAddress), sizeof(*serverAddress)) < 0)
		printf("Binding Error\n");
	memset(&serverAddress, 0, sizeof(*serverAddress));
	listen(serverSocket, 100);
	connectedPlayers.num = 0;
	allGroups.num = 0;
	allGames.gamesNum = 0;
	srand(time(NULL));
}
void CreateNewThreadForPlayer(int* socket) {
	pthread_t thread;
	LockThread();
	printf("Creating thread for the user with socket: %d\n", *socket);
	pthread_create(&thread, NULL, PlayerLoop, socket);
	UnlockThread();
}
void* PlayerLoop(void* socket) {
	int PlayerSocketID = *((int*)socket);
	for (;;) {
		printf("Waiting for socket %d  action\n", PlayerSocketID);
		char rawRequest[500];
		int lenghtInBytes = read(PlayerSocketID, rawRequest, sizeof(rawRequest));
		rawRequest[lenghtInBytes] = '\0';
		if (strlen(rawRequest) >= 2) {
			CookRequest(rawRequest, PlayerSocketID);
		}
	}
}
void AskToDataBase(char* request, char* reply) {
	MYSQL* DBConn = mysql_init(NULL);
	MYSQL_RES* DBResponse;
	DBConn = mysql_real_connect(DBConn, "localhost", "root", "mysql", "T6_DCDB", 0, NULL, 0);
	mysql_query(DBConn, request);
	DBResponse = mysql_store_result(DBConn);
	MYSQL_ROW row;
	if (DBResponse == NULL) {
		// No hemos tenido respuesta, comprobamos si se trata de un error o no
		if (mysql_field_count(DBConn) == 0)
		{
			strcpy(reply, "Nothing found, but no errors found\n");
			// La query no devuelve informaci￳n (no era un SELECT)
		}
		else {
			// Deber￭a haber retornado informaci￳n,
			printf("Ha ocurrido un error: %s\n", mysql_error(DBConn));
		}
		return;
	}
	row = mysql_fetch_row(DBResponse);
	strcpy(reply, "\0");
	int totalColumns = mysql_field_count(DBConn);
	int totalRows = mysql_num_rows(DBResponse);
	for (int i = 0; i < totalRows; i++) {
		for (int j = 0; j < totalColumns; j++) {
			strcat(reply, row[j]);
			if (j != totalColumns)
				strcat(reply, "/"); // Indica salto de columna
		}
		row = mysql_fetch_row(DBResponse);
	}
	mysql_free_result(DBResponse);
	mysql_close(DBConn);
}
void LockThread() {
	pthread_mutex_lock(&mutex);
}
void UnlockThread() {
	pthread_mutex_unlock(&mutex);
}
int WaitForPlayer() {
	int socket = accept(serverSocket, NULL, NULL);
	return socket;
}
///////////////////////////////////////////////////////
void CookRequest(char* request, int PlayerSocketID) {
	printf("We got the request : %s\n", request);
	int mySocket = PlayerSocketID;
	char* tempString = strtok(request, "/");
	int requestID = atoi(tempString);
	if (requestID == 0) { // Error code from atoi();
		printf("Ha ocurrido un error con el formato del mensaje recibido\n");
		return;
	}
	switch (requestID) {
	case -1: {
		// Desconectar
		// Tell my everyone im out
		char notificationToGroup[350];
		strcpy(notificationToGroup, "5"); // A￱adir nombres
		int myGroup = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
		for (int i = 0; i < 4; i++) {
			int socket = allGroups.list[myGroup].partnersSockets[i];
			if (socket != -1 && socket != mySocket) {
				strcat(notificationToGroup, "/");
				strcat(notificationToGroup, connectedPlayers.list[GetIndex(socket)].username);
			}
		}
		for (int i = 0; i < 4; i++) {
			if (allGroups.list[myGroup].partnersSockets[i] != -1 && allGroups.list[myGroup].partnersSockets[i] != mySocket) {
				printf("Telling my guys im out : %s \n", notificationToGroup);
				write(allGroups.list[myGroup].partnersSockets[i], notificationToGroup, sizeof(notificationToGroup));
			}
		}
		LockThread();
		int currentPlayersOnline = connectedPlayers.num;
		int index = GetIndex(mySocket);
		if (index == currentPlayersOnline - 1) {//Caso en el que el jugador es el ￺ltimo de la lista
			connectedPlayers.list[index].currentGroupIndex = -1;
			connectedPlayers.list[index].socket = -1;
		}
		else {
			for (int i = index; i < connectedPlayers.num - 1; i++)
			{
				connectedPlayers.list[i] = connectedPlayers.list[i + 1];
			}
		}
		int groupsNum = allGroups.num;
		if (groupsNum - 1 == index) {
			allGroups.num--;
		}
		else {
			for (int i = index; i < groupsNum - 2; i++) {
				allGroups.list[i] = allGroups.list[i + 1];
			}
		}
		connectedPlayers.num--;
		if (connectedPlayers.num < 0)
			connectedPlayers.num = 0;
		UnlockThread();
		close(mySocket);
		pthread_exit(0);
		break;
	}
	case 1: { //Registro
		char username[50], password[50], accountCreation[500], DBRequest[500];
		tempString = strtok(NULL, "/");
		strcpy(username, tempString);
		tempString = strtok(NULL, "/");
		strcpy(password, tempString);
		sprintf(DBRequest, "SELECT COUNT (NICKNAME) FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);
		AskToDataBase(DBRequest, accountCreation);
		int ans = atoi(accountCreation);
		if (ans == 1)
		{
			strcpy(accountCreation, "0/-1");
			write(PlayerSocketID, accountCreation, sizeof(accountCreation));
		}
		else
		{
			sprintf(DBRequest, "INSERT INTO PLAYERS VALUES (NULL,'%s','%s');", username, password);
			AskToDataBase(DBRequest, accountCreation);
			strcpy(accountCreation, "0/1");
			write(PlayerSocketID, accountCreation, sizeof(accountCreation));
			// Lo que se envia es un string diciendo que todo ha ido bien o que ha fallado la operaci￳n
		}
		break;
	case 2: {//Inicio de sesión

		char username[50], password[50],DBRequest[500];
		char credentialsCheck[20], c[40];
		tempString = strtok(NULL, "/");
		strcpy(username, tempString);
		tempString = strtok(NULL, "/");
		strcpy(password, tempString);
		sprintf(DBRequest, "SELECT COUNT(NICKNAME) FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);
		AskToDataBase(DBRequest, credentialsCheck);
		int ans = atoi(credentialsCheck);
		if (ans == 1 && connectedPlayers.num < 100) {
			printf("Se ha iniciado sesión\n");
			// Dile al nuevo usuario todos los usuarios conectados (si los hay) y dile a estos usuarios que hay un nuevo usuario conectado
			sprintf(DBRequest, "1/%d/", connectedPlayers.num);
			if (connectedPlayers.num > 0) {
				sprintf(password, "4/1/%s", username);
				for (int i = 0; i < connectedPlayers.num; i++) {

					strcat(DBRequest, connectedPlayers.list[i].username);
					strcat(DBRequest, "/");
					// Dile a tu el mundo que este jugador está ahora conectado
					
					printf("Telling now %s with socket %d that %s is connected\n", connectedPlayers.list[i].username,connectedPlayers.list[i].socket ,username);
					write(connectedPlayers.list[i].socket, password, sizeof(password));
				}
				// Dile a estos usuarios los usuarios conectados

			}
			write(mySocket, DBRequest, sizeof(DBRequest));
			// Añade este usuario a la lista de conectados
			LockThread();
			connectedPlayers.list[connectedPlayers.num].socket = mySocket;
			strcpy(connectedPlayers.list[connectedPlayers.num].username, username);
			connectedPlayers.list[connectedPlayers.num].currentGroupIndex = connectedPlayers.num;
			printf("We stored the user called %s with socket %d and group %d\n", username, mySocket, connectedPlayers.num);
			// Creamos un grupo para el jugador (donde está él solo) con el mismo índice
			//El índice dice del socket debe ser siempre el mismo que el índice de grupo y este índice de grupo será el grupo por defecto de este jugador
			allGroups.list[allGroups.num].partnersSockets[0] = mySocket;
			allGroups.list[allGroups.num].fightIndex = -1;
			connectedPlayers.list[connectedPlayers.num].currentGroupIndex = connectedPlayers.num;
			
			connectedPlayers.list[connectedPlayers.num].orderInGroup=0;
			// Los puestos libres están marcados como -1
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
	case 3: { // Borrar usuario de base de datos
		char username[50], password[50], deleteAccount[500], DBRequest[500];
		tempString = strtok(NULL, "/");
		strcpy(username, tempString);
		tempString = strtok(NULL, "/");
		strcpy(password, tempString);
		sprintf(DBRequest, "SELECT COUNT (NICKNAME) FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);
		AskToDataBase(DBRequest, deleteAccount);
		int ans = atoi(deleteAccount);
		if (ans == 1)
		{
			sprintf(deleteAccount, "-2/-1");
			write(PlayerSocketID, deleteAccount, sizeof(deleteAccount));
		}
		else
		{
			sprintf(DBRequest, "DELETE FROM PLAYERS WHERE NICKNAME = '%s' AND PASSWORD = '%s' ;", username, password);
			AskToDataBase(DBRequest, deleteAccount);
			sprintf(deleteAccount, "-2/0");
			write(PlayerSocketID, deleteAccount, sizeof(deleteAccount));
		}

		break;
	}
	case 4: { // Invitaci￳n a grupo
		tempString = strtok(NULL, "/"); // Nombre de a quien hemos invitado
		char whoWeInvitedUsername[50];
		strcpy(whoWeInvitedUsername, tempString);
		char invitationString[100];
		int myIndex = GetIndex(mySocket);
		if (myIndex != -1) {
			sprintf(invitationString, "2/%s", connectedPlayers.list[myIndex].username);
			int targetSocket = GetSocket(whoWeInvitedUsername);
			if (targetSocket != -1) {
				printf("El usuario %s ha invitado a %s que tiene socket %d\n", connectedPlayers.list[myIndex].username, whoWeInvitedUsername, targetSocket);
				write(targetSocket, invitationString, sizeof(invitationString));
			}
		}
		break;
	}
	case 5: { // Respuesta invitaci￳n a grupo
		char ans[5];
		char whoInvitedMeUsername[50];
		tempString = strtok(NULL, "/");
		strcpy(ans, tempString);
		if (strcmp(ans, "Y") == 0) { // Aceptado
			// A￱adir al grupo
			tempString = strtok(NULL, "/");
			strcpy(whoInvitedMeUsername, tempString);
			int whoInvitedMeSocket = GetSocket(whoInvitedMeUsername);
			if (whoInvitedMeSocket != -1) {
				int whoInvitedMeGroupIndex = connectedPlayers.list[GetIndex(whoInvitedMeSocket)].currentGroupIndex;
				if (allGroups.list[whoInvitedMeGroupIndex].num < 4) {
					LockThread();
					int add = 0;
					while (add != -1) {
						if (allGroups.list[whoInvitedMeGroupIndex].partnersSockets[add] == -1) {
							allGroups.list[whoInvitedMeGroupIndex].partnersSockets[add] = mySocket;
							allGroups.list[whoInvitedMeGroupIndex].num++;
							add = -1;
						}
						else
							add++;
					}
					connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex = whoInvitedMeGroupIndex;
					UnlockThread();
				}
				char notificationToGroup[350];
				sprintf(notificationToGroup, "5/%d/", whoInvitedMeGroupIndex);
				for (int i = 0; i < 4; i++) {
					int socket = allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i];
					if (socket != -1) {
						strcat(notificationToGroup, connectedPlayers.list[GetIndex(socket)].username);
						strcat(notificationToGroup, "/");
					}
				}
				for (int i = 0; i < 4; i++) {
					if (allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i] != -1) {
						write(allGroups.list[whoInvitedMeGroupIndex].partnersSockets[i], notificationToGroup, sizeof(notificationToGroup));
					}
				}

			}
			//Communicate the group the new member and the member the new Group
		}
		else {
			//work for later boy
		}
		break;
	}
	case 6: { // Mensaje de chat
		tempString = strtok(NULL, "/");
		int type = atoi(tempString); // Tipo de mensaje 
		switch (type)
		{
		case 0: break;
		case 1: { // Mensaje global
			tempString = strtok(NULL, "/"); // Mensaje actual
			char newMessage[250];
			char actualMessage[250];
			strcpy(actualMessage, tempString);
			sprintf(newMessage, "3/%s/%s/%s", "1", connectedPlayers.list[GetIndex(mySocket)].username, actualMessage);
			printf("The message sent is : %s\n", newMessage);
			for (int i = 0; i < connectedPlayers.num; i++) {
				write(connectedPlayers.list[i].socket, newMessage, sizeof(newMessage));
			}
			break;
		}

		case 2: { // Mensaje al grupo
			tempString = strtok(NULL, "/"); // Mensaje actual
			char newMessage[250];
			char actualMessage[250];
			strcpy(actualMessage, tempString);
			sprintf(newMessage, "3/%s/%s/%s", "2", connectedPlayers.list[GetIndex(mySocket)].username, actualMessage);
			printf("The message sent is : %s\n", newMessage);
			int myGroup = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
				NotifyGroup(myGroup, newMessage);
			break;
		}

		case 3: { // Susurro
			tempString = strtok(NULL, "/");
			char username[25];
			strcpy(username, tempString);
			int whisperTo = GetSocket(username);
			tempString = strtok(NULL, "/"); // Mensaje actual
			char newMessage[250];
			char actualMessage[250];
			strcpy(actualMessage, tempString);
			sprintf(newMessage, "3/%s/%s/%s", "3", connectedPlayers.list[GetIndex(mySocket)].username, actualMessage);
			printf("The message sent is : %s\n", newMessage);
			write(whisperTo, newMessage, sizeof(newMessage));

			break;
		}

		case 4: break;
		case 5: break;
		}
		break;
	}
	case 7: { // Buscar partida
		int groupIndex = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
		Group myGroup = allGroups.list[groupIndex];
		LockThread();
		Game waitingGame = allGames.list[allGames.gamesNum];
		int added = 0;
		int alreadyGroupsNum = 0;
		char tempStrng[100];
		if (waitingGame.groupsNum == 0) {//No tenemos grupos en la partida aun
			printf("we created a game for the group : %d\n", groupIndex);
			waitingGame.groupsIndexes[0] = groupIndex;
			waitingGame.groupsIndexes[1] = -1;
			waitingGame.groupsIndexes[2] = -1;
			waitingGame.groupsIndexes[3] = -1;
			waitingGame.groupsNum++;
			allGames.list[allGames.gamesNum] = waitingGame;
			NotifyGroup(groupIndex, "9/0/");
		}
		else {
			for (int i = 0; i < 4; i++) {
				if (waitingGame.groupsIndexes[i] == -1 && added == 0) { // Busca un grupo vac￭o cuyo valor es -1 y lo remplaza por nuestro groupIndex
					printf("We are now storing the new group in the game\n");
					waitingGame.groupsIndexes[i] = groupIndex;
					waitingGame.groupsNum++;
					allGames.list[allGames.gamesNum] = waitingGame;
					added = 1;
				}
				if (waitingGame.groupsIndexes[i] != -1 && waitingGame.groupsIndexes[i] != groupIndex) {//Tell the groups that are already waiting that one more group just got added
					char ourTeamInfo[50];
					sprintf(ourTeamInfo, "%d", groupIndex);//Preparamos nuestra informaci￳n para decirle al resto que nos hemos unido
					for (int j = 0; j < 4; j++) {
						int userSocket = myGroup.partnersSockets[j];
						if (userSocket != -1) {
							strcat(ourTeamInfo, "/");
							strcat(ourTeamInfo, connectedPlayers.list[GetIndex(userSocket)].username);
						}
						else {
							strcat(ourTeamInfo, "/");
							strcat(ourTeamInfo, "EMPTY");
						}

						int enemyPlayerSocket = allGroups.list[waitingGame.groupsIndexes[i]].partnersSockets[j];
						char teamIndex[3];
						sprintf(teamIndex, "%d", waitingGame.groupsIndexes[i]);
						strcat(tempStrng, teamIndex);//Copia el indice del grupo
						if (enemyPlayerSocket != -1) {
							strcat(tempStrng, "/");
							strcat(tempStrng, connectedPlayers.list[GetIndex(enemyPlayerSocket)].username);
						}
						else {
							strcat(tempStrng, "/");
							strcat(ourTeamInfo, "EMPTY");

						}
					}

					char notificationToGroup[250];
					printf("We are notifying a group that already is in the game\n");
					int targetGroup = waitingGame.groupsIndexes[i];
					sprintf(notificationToGroup, "9/1/%s", ourTeamInfo);
					NotifyGroup(targetGroup, notificationToGroup);
					alreadyGroupsNum++;
				}
			}
			if (allGames.list[allGames.gamesNum].groupsNum == 4) {
				printf("Starting game\n");
				StartGame();
			}
			char notificationToOwnGroup[250];
			sprintf(notificationToOwnGroup, "9/%d/%s", alreadyGroupsNum, tempStrng);
			NotifyGroup(groupIndex, notificationToOwnGroup);
		}
		UnlockThread();
		break;
	}
	case 8: { // Inicio de la partida
		StartGame();
		break;
	}
	case 9: { // Movimiento
		tempString = strtok(NULL, "/"); // Direcci￳n
		int direction = atoi(tempString); // 1 = Derecha, 2 = Izquierda, 0 = Stop
		int myGroup = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
		if (direction == 1) {
			NotifyGroup(myGroup, "11/1/");
		}
		else if (direction == -1) {
			NotifyGroup(myGroup, "11/2/");
		}
		else if (direction == 0) {
			NotifyGroup(myGroup, "11/0/");
		}
		break;
	}
	case 10: { // Actualizar habitaci￳n
		tempString = strtok(NULL, "/");
		int myGroupIndex = connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex;
		LockThread();
		strcpy(allGroups.list[myGroupIndex].currentCoordinate, tempString); // Actualiza nuestra habitaci￳n
		UnlockThread();
		int myGame = allGroups.list[myGroupIndex].gameIndex;
		int groupsAlreadyInRoomIndexes[3];
		for (int i = 0; i < 3; i++) {
			groupsAlreadyInRoomIndexes[i] = -1;
		}
		int groupsAlreadyInRoomNum = 0;
		for (int i = 0; i < 4; i++) {//Recolecta los indices de grupos con los que has coincidido al entrar a un nuevo sitio
			int targetGroupIndex = allGames.list[myGame].groupsIndexes[i];
			if (targetGroupIndex != myGroupIndex) {
				if (strcmp(allGroups.list[targetGroupIndex].currentCoordinate, tempString) == 0) {
					printf("There was a match with group: %d : %s", targetGroupIndex, tempString);
					groupsAlreadyInRoomIndexes[groupsAlreadyInRoomNum] = targetGroupIndex;//Lista de indices de grupos en la misma coordinada!
					groupsAlreadyInRoomNum++;
				}
			}
		}
		if (groupsAlreadyInRoomNum > 0) {
			printf("We found someone inside\n");
			for (int i = 0; i < groupsAlreadyInRoomNum; i++) {
				int targetGroupIndex = groupsAlreadyInRoomIndexes[i];
				int fightIndex1 = GetFightIndex(targetGroupIndex);
				int fightIndex2 = GetFightIndex(myGroupIndex);
				if (fightIndex1 == -1) {
					//They both are not fighting yet, lets make them fight eachother to death!
					LockThread();
					int newFightIndex = CreateFight(targetGroupIndex, myGroupIndex);
					allGroups.list[targetGroupIndex].fightIndex = newFightIndex;
					allGroups.list[myGroupIndex].fightIndex = newFightIndex;
					UnlockThread();
					char notification[100];
					sprintf(notification, "17/%d/%d/-1/-1|0/%d:0_%d:0/", targetGroupIndex, myGroupIndex, myGroupIndex, targetGroupIndex);
					NotifyGroup(myGroupIndex, notification);
					NotifyGroup(targetGroupIndex, notification);
					break;
				}
				else {//We found one active fight!
					LockThread();
					int result = JoinFight(myGroupIndex, fightIndex1);
					if (result == -1) {
						printf("There was a problem joinning a fight\n");
					}
					UnlockThread();

				}
			}
		}
		else {
			printf("We found no fight\n");
			//There is no one yet
		}
		break;
	}
		   //Check if there is a fight between whoever
	case 11: {//Deal damage to Player  user/amount
		tempString = strtok(NULL, "/");
		int damage = atoi(tempString);
		tempString = strtok(NULL, "/");
		char damagedPlayer[50];
		strcpy(damagedPlayer, tempString);
		int damagedSocket = GetSocket(damagedPlayer);
		int myGroupIndex = connectedPlayers.list[GetIndex(damagedSocket)].currentGroupIndex;
		int currentFightIndex = GetFightIndex(myGroupIndex);
		Fight myFight = allFights.list[currentFightIndex];
		char sendDamaged[50];
		sprintf(sendDamaged, "14/%d/%s", damage, damagedPlayer);
		for (int i = 0; i < 4; i++) {
			int targetGroup = myFight.groupsInvolvedIndexes[i];
			if (targetGroup != -1) {
				NotifyGroup(targetGroup, sendDamaged);
			}
		}
		break;
	}
	case 12: { // Lista de jugadores con los que he jugado
		char playedWith[500], DBRequest[500], DBReply[500];
		sprintf(DBRequest, "SELECT NICKNAME FROM PLAYERS WHERE ID IN (SELECT PLAYED FROM RECORD");
		AskToDataBase(DBRequest, DBReply);
		sprintf(playedWith, "15/%s", DBReply);
		write(PlayerSocketID, playedWith, sizeof(playedWith));
		break;
	}
	case 13: {//End of round
		int fightIndex = GetFightIndex(connectedPlayers.list[GetIndex(mySocket)].currentGroupIndex);
		char messageToAll[500];

		Fight currentF = allFights.list[fightIndex];
		currentF.currentTurnNumber++;
		currentF.currentPlayerIndex++;
		if (currentF.currentPlayerIndex > 3) {
			currentF.currentPlayerIndex = 0;
		}
		LockThread();
		allFights.list[fightIndex] = currentF;
		UnlockThread();
		strcpy(messageToAll, "17");
		char temp[10];
		for (int i = 0; i < 4; i++) {
			sprintf(temp, "%d", currentF.groupsInvolvedIndexes[i]);
			strcat(messageToAll, "/");
			strcat(messageToAll, temp);
		}
		printf("Hemos a￱adido los grupos participantes : %s\n", messageToAll);
		strcat(messageToAll, "|");
		char temp2[10];
		sprintf(temp2, "%d", currentF.currentTurnNumber);
		strcat(messageToAll, temp2);
		strcat(messageToAll, "/");
		char temp3[100];
		for (int i = 0; i < 4; i++) {
			int teamIndex = currentF.groupsInvolvedIndexes[i];
			if (teamIndex != -1) {
				sprintf(temp3, "%d:%d_", teamIndex, currentF.currentPlayerIndex);
				strcat(messageToAll, temp3);
			}
			else {

			}
		}
		strcat(messageToAll, "/");
		printf("We are about to update a fight : %s", messageToAll);
		for (int i = 0; i < 4; i++) {
			int teamIndex = currentF.groupsInvolvedIndexes[i];
			if (teamIndex != -1) {
				NotifyGroup(teamIndex, messageToAll);
			}
		}
		break;
	}
	case 14: { // Lista de partidas en una periodo de tiempo dado (YYYY-MM-DD)
		char DBRequest[500], betweenGames[500], DBReply[500];
		tempString = strtok(NULL, "/");
		char startDate[50];
		strcpy(startDate, tempString);
		tempString = strtok(NULL, "/");
		char endDate[50];
		strcpy(endDate, tempString);
		sprintf(DBRequest, "SELECT RESULT, ENDDATE FROM RECORD WHERE ENDDATE BETWEEN '%s' AND '%s';", startDate, endDate);
		AskToDataBase(DBRequest, DBReply);
		sprintf(betweenGames, "19/%s", DBReply);
		write(PlayerSocketID, betweenGames, sizeof(betweenGames));

	}
	case 15: { // Resultados de las partidas contra un jugador
		char username[50], DBRequest[500], resultGame[500], DBReply[500];
		tempString = strtok(NULL, "/");
		strcpy(username, tempString);
		sprintf(DBRequest, "SELECT GAME_ID, RESULT FROM RECORD WHERE PLAYED IN (SELECT ID FROM PLAYERS WHERE NICKNAME = '%s')", username);
		AskToDataBase(DBRequest, DBReply);
		sprintf(resultGame, "16/%s", DBReply);
		write(PlayerSocketID, resultGame, sizeof(resultGame));
		break;
	}
	case 16: {//Death Event
		break;
	}
	}// Final del switch
}}

void UpdateFight(int fightIndex, char* notification) {

}
void StartGame()
{
	int seed = rand() % 999999 + 100000;
	// 0 0  0 8  18 0  18 8 : spawnPoints 
	int spawnPoints[4][2] = {
	{0, 0} ,   /*  initializers for row indexed by 0 */
	{0, 8} ,   /*  initializers for row indexed by 1 */
	{18, 0} ,  /*  initializers for row indexed by 2 */
	{18, 8}
	};
	char notificationToGroups[3000];
	char partnersInfo[100];
	char newCoords[5];
	for (int i = 0; i < 4; i++) { // Prepara un string con toda la informaci￳n de todos los grupos 4:
		char groupNotStart[4];
		int groupIndex = allGames.list[allGames.gamesNum].groupsIndexes[i];
		printf("The group we are printing now has index : %d", groupIndex);
		if (groupIndex != -1) {
			Group thisGroup = allGroups.list[groupIndex];
			sprintf(groupNotStart, "%d:", groupIndex); // Mete el indice del grupo
			strcat(partnersInfo, groupNotStart);
			for (int j = 0; j < 4; j++) {
				int partnerSocket = thisGroup.partnersSockets[j];
				if (partnerSocket != -1) {
					strcat(partnersInfo, connectedPlayers.list[GetIndex(partnerSocket)].username);
					//Aqu￭ podemos a￱adir cosas como tipo de personaje and shits.
					strcat(partnersInfo, "|");
				}
				else {
					strcat(partnersInfo, "-1");
					strcat(partnersInfo, "|");
				}
			}
			strcat(partnersInfo, "_"); // Indica cambio de grupo
		}
	}
	LockThread();
	for (int i = 0; i < 4; i++) { // Pone a cada grupo en una posicion inicial  y se los notifica
		int targetGroupIndex = allGames.list[allGames.gamesNum].groupsIndexes[i];
		if (targetGroupIndex != -1) { // Notifica las posiciones iniciales
			sprintf(notificationToGroups, "10/%d/%d%d/%s", seed, spawnPoints[i][0], spawnPoints[i][1], partnersInfo);
			//El formato deber￭a ser : 10/seed/spawnPointCoords/int:ana|pedro|juan|-1||int:ZAPATO|-1|-1|-1||
			sprintf(newCoords, "%d%d", spawnPoints[i][0], spawnPoints[i][1]);
			strcpy(allGroups.list[targetGroupIndex].currentCoordinate, newCoords);
			NotifyGroup(targetGroupIndex, notificationToGroups); // ￚtima cosa que hay que hacer
		}
	}
	allGames.gamesNum++;
	UnlockThread();
}

int GetIndex(int socket) {
	for (int i = 0; i < connectedPlayers.num; i++) {
		if (socket == connectedPlayers.list[i].socket)
			return i;
	}
	return -1;
}
int GetSocket(char* username) {
	for (int i = 0; i < connectedPlayers.num; i++) {
		if (strcmp(connectedPlayers.list[i].username, username) == 0)
			return connectedPlayers.list[i].socket;
	}
	return -1;
}
void NotifyGroup(int groupIndex, char* notification) {
	printf("We are sending : %s to group %d\n", notification, groupIndex);
	char mensaje[500];
	strcpy(mensaje, notification);
	Group targetGroup = allGroups.list[groupIndex];
	for (int i = 0; i < 4; i++) {
		int targetSocket = targetGroup.partnersSockets[i];
		if (targetSocket != -1) {
			printf("Sent to : %s\n", connectedPlayers.list[GetIndex(targetSocket)].username);
			write(targetSocket, mensaje, sizeof(mensaje));
		}
	}
}
int CreateFight(int group1Index, int group2Index) {
	//ReturnsCreatedFightIndex
	int currentFightsNum = allFights.num;
	allFights.list[currentFightsNum].groupsInvolvedIndexes[0] = group1Index;
	allFights.list[currentFightsNum].groupsInvolvedIndexes[1] = group2Index;
	allFights.list[currentFightsNum].groupsInvolvedIndexes[2] = -1;
	allFights.list[currentFightsNum].groupsInvolvedIndexes[3] = -1;
	allFights.list[currentFightsNum].currentPlayerIndex = 0;//Siempre empieza el jugador en cabeza 
	allFights.list[currentFightsNum].groupsInvolvedNum = 2;
	allFights.num++;
	return currentFightsNum;
}
int GetFightIndex(int groupIndex) {
	return allGroups.list[groupIndex].fightIndex;
}
int JoinFight(int newGroup, int fightIndex) {

	for (int i = 0; i < 4; i++) {
		int freeIndex = allFights.list[fightIndex].groupsInvolvedIndexes[i];
		if (freeIndex == -1) {
			allFights.list[fightIndex].groupsInvolvedIndexes[i] = newGroup;
			return 0;
		}
	}
	return -1;
}


