#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include<string.h>
#include <mysql.h>
void StartServer(int* server_socket, struct sockaddr_in* server_address){
	*server_socket = socket(AF_INET,SOCK_STREAM,0);
	(*server_address).sin_family = AF_INET;
	(*server_address).sin_port = htons(9084);
	(*server_address).sin_addr.s_addr = INADDR_ANY;
	if(bind(*server_socket, (struct sockaddr*) &(*server_address), sizeof(*server_address)) < 0)
		printf("Error en el bind " );
	memset(&server_address, 0, sizeof(*server_address));
}
	void GetPetition(int* server_socket, char* client_string, int client_string_size, int* client_socket){
		*client_socket = accept(*server_socket,NULL,NULL);
		int lenghtInBytes = read(*client_socket, client_string, client_string_size);
		client_string[lenghtInBytes] = '\0';
	}
		void GenerateDBString(int petition, char* string, char* databaseString){
			char username[22];
			char email[22];
			char password[16];
			char newEmail[22];
			switch(petition){
			case 1 ://sign up
				string = strtok(NULL,"/");
				strcpy(username,string);
				string = strtok(NULL,"/");
				strcpy(email,string);
				string = strtok(NULL,"/");
				strcpy(password,string);
				sprintf(databaseString, "INSERT INTO PLAYERS VALUES (NULL,'%s','%s','%s');", username,email, password);
				break;
			case 2 : //sign in
				//work in progress
				
				break;
			case 3 : //change password
				string = strtok(NULL, "/");
				strcpy(email,string);
				string = strtok(NULL, "/");
				strcpy(password, string);
				sprintf (databaseString, "UPDATE PLAYERS SET PASSWORD = REPLACE(PASSWORD, PASSWORD, '%s') WHERE PLAYERS.EMAIL = '%s'", password, email);
				break;
			case 4 : //change email
				string = strtok(NULL, "/");
				strcpy(email, string);
				string = strtok(NULL, "/");
				strcpy(newEmail, string);
				string = strtok(NULL, "/");
				strcpy(password, string);
				sprintf (databaseString, "UPDATE PLAYERS SET EMAIL = REPLACE(EMAIL, EMAIL, '%s') WHERE PLAYERS.PASSWORD = '%s' AND PLAYERS.EMAIL = '%s'", newEmail, password, email);
				break;
			case 5 : //Get Recent Players Faced By a Player Given As a Parameter
				string= strtok(NULL, "/");
				strcpy(username, string);
				sprintf(databaseString, "SELECT NICKNAME FROM PLAYERS WHERE ID IN(SELECT ID FROM PLAYERS WHERE PLAYERS.NICKNAME != '%s' "
						"AND ID IN(SELECT PLAYER1 FROM RECORD WHERE GAME_ID IN(SELECT GAME_ID FROM RECORD WHERE PLAYER1 IN (SELECT ID FROM PLAYERS WHERE NICKNAME = '%s'))));", username, username);
				break;
				default : ;
			}
		}
			void GenerateAnswer(MYSQL_RES* DBResponse, MYSQL* DBConn, int* client_socket, int err){
				DBResponse = mysql_store_result(DBConn);
				MYSQL_ROW row;
				if (DBResponse == NULL){
					//No hemos tenido respuesta, comprobamos si se trata de un error o no
					if(mysql_field_count(DBConn) == 0)
					{
						// query does not return data
						// (it was not a SELECT)
					}
					else{
						//It sould have returned data, error
						printf("Ha ocurrido un error: %s",mysql_error(DBConn));
					}
					return;
				}
				row = mysql_fetch_row (DBResponse);
				char response[200];
				strcpy(response,"\0");
				int totalColumns = mysql_field_count(DBConn);
				int totalRows =  mysql_num_rows(DBResponse);
				for(int i = 0;i < totalRows;i++){
					for(int j = 0;j < totalColumns;j++){
						strcat(response,row[j]);
						strcat(response,"/");//indica salto de columna
					}
					strcat(response, "||");//Indica salto de fila
					row = mysql_fetch_row (DBResponse);
				}
				write(*client_socket, response, strlen(response));
				mysql_free_result(DBResponse);
			}
				int main(){
					//server initialization
					int server_socket, client_socket;
					struct sockaddr_in server_address;
					StartServer(&server_socket, &server_address);
					//server loop
					listen(server_socket, 20);
					for(;;){
						MYSQL *DBConn = mysql_init(NULL);
						MYSQL_RES* DBResponse;
						if (DBConn==NULL) {
							fprintf(stderr, "%s\n", mysql_error(DBConn));
							exit(1);
						}
						DBConn = mysql_real_connect (DBConn, "localhost","root", "mysql", "DCDB", 0, NULL, 0); 
						if (DBConn==NULL) {
							
							printf ("Error al inicializar : %u %s\n", mysql_errno(DBConn), mysql_error(DBConn));
							exit (1);
						}
						char databaseString[100];
						printf("\nListening\n");
						char client_string[512];
						GetPetition(&server_socket, client_string, sizeof(client_string), &client_socket);
						char* p = strtok(client_string,"/");
						int number = atoi(p);
						GenerateDBString(number, p, databaseString);
						printf("%s", databaseString);
						int err = mysql_query(DBConn, databaseString);
						if(err == 0){
							printf("\nLa soliciutat ha tenido exito");
						}
						GenerateAnswer(DBResponse, DBConn, &client_socket, err);
						close(client_socket);
						mysql_close(DBConn);
					}
					return 0 ;
				}		
					
