#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include<unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <mysql.h> 
int main(int argc, char * argv[]){
	
	// initialises a MYSQL object suitable for mysql_real_connect() function
	MYSQL *conn = mysql_init(NULL);
	MYSQL_RES *resultado;
	MYSQL_ROW row;
	if (conn==NULL) {
		fprintf(stderr, "%s\n", mysql_error(conn));
		exit(1);
	}
	conn = mysql_real_connect (conn, "localhost","root", "mysql", "DCDB", 0, NULL, 0); 
	if (conn==NULL) {
		printf ("Error al inicializar : %u %s\n", 
				mysql_errno(conn), mysql_error(conn));
		exit (1);
	}
	//Database is ready to operate
	
	
	//Start Server
	int sock_conn, sock_listen, ret;
	struct sockaddr_in serv_adr;
	//Abrimos el socket
	if((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0){
		printf("Error creando el socket");
	}
	memset(&serv_adr, 0, sizeof(serv_adr)); //inicializar serv_adr a 0
	serv_adr.sin_family = AF_INET;
	//Asociar el socket a cualquiera de las IP de la maquina
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
	serv_adr.sin_port = htons(8561);//port
	if(bind(sock_listen,(struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
		printf("Error en el bind");
	if(listen(sock_listen,3) <0){
		printf("Error en el listen");
	}
	
	
	char peticion[512];
	//Servidor escuchando para siempre
	for(; ;){
		char consulta[100];
		printf("Escuchando\n");
		sock_conn = accept(sock_listen,NULL,NULL); //We wait here until a client connects
		ret = read(sock_conn, peticion, sizeof(peticion));//ret stores the number of bytes 
		peticion[ret] = '\0'; //End of line
		
		char *p = strtok(peticion, "/");// /int order/data
		int codigo = atoi(p);
		if(codigo == 1) //sign up format : nickname/email/password
		{
			char nickname[22];
			char email[22];
			char password[16];
			printf("We got a sign up\n");
			p = strtok(NULL, "/");
			strcpy(nickname, p);
			p = strtok(NULL, "/");//email
			strcpy(email, p);
			p = strtok(NULL, "/");
			strcpy(password, p);
			printf("The mail is %s and the password is %s",nickname, email, password);
			sprintf(consulta, "INSERT INTO PLAYERS VALUES (NULL,'%s','%s','%s');", nickname,email, password);
		}
		else if(codigo == 2){//login
			char nickname[20];
			char password[20];
			p = strtok(NULL, "/");
			strcpy(nickname,p);
			sprintf(consulta,"SELECT NICKNAME FROM PLAYERS WHERE NICKNAME = '%s';", nickname); 
			// hacemos la consulta 
			int err=mysql_query (conn, consulta); 
			if (err!=0) {
				printf ("Error al consultar datos de la base %u %s\n",
						mysql_errno(conn), mysql_error(conn));
				exit (1);
			}
			//recogemos el resultado de la consulta 
			resultado = mysql_store_result (conn); 
			row = mysql_fetch_row (resultado);
			if (row == NULL)
				printf ("No se encuentra dicho usuario\n");
			else
				printf ("El usuario está registrado %s\n");
			printf ("Dame la contraseña\n"); 
			p = strtok(NULL, "/");
			strcpy(password, p);
			sprintf(consulta, "SELECT password FROM PLAYERS WHERE password = '%s';", password);
			// hacemos la consulta 
			err=mysql_query (conn, consulta); 
			if (err!=0) {
				printf ("Error al consultar datos de la base %u %s\n",
						mysql_errno(conn), mysql_error(conn));
				exit (1);
			}
			//recogemos el resultado de la consulta 
			resultado = mysql_store_result (conn); 
			row = mysql_fetch_row (resultado);
			if (row == NULL)
				printf ("La contraseña es incorrecta\n");
			else{
				printf ("Contraseña correcta %s\n");
				char responce[2];
				strcpy(responce, "1");
				write(sock_conn, responce, strlen(responce));
			}
		}
		else if(codigo == 3){//change password
			char email[22];
			char password[16];
			p = strtok(NULL, "/");//email
			strcpy(email, p);
			p = strtok(NULL, "/");
			strcpy(password, p);
			printf("The mail is %s and the password is %s", email, password);
			sprintf (consulta, "UPDATE PLAYERS SET PASSWORD = REPLACE(PASSWORD, PASSWORD, '%s') WHERE PLAYERS.EMAIL = '%s'", password, email);
			printf("We got a password change\n");
		}
		else if(codigo == 4){//change mail email/newemail/password
			
			char email[22];
			char newEmail[22];
			char password[22];
			p = strtok(NULL, "/");
			strcpy(email, p);
			p = strtok(NULL, "/");
			strcpy(newEmail, p);
			p = strtok(NULL, "/");
			strcpy(password, p);
			printf("The mail is %s and the new email is is %s", email, newEmail);
			sprintf (consulta, "UPDATE PLAYERS SET EMAIL = REPLACE(EMAIL, EMAIL, '%s') WHERE PLAYERS.PASSWORD = '%s' AND PLAYERS.EMAIL = '%s'", newEmail, password, email);
			printf("We got a email change\n");
		}
		else if(codigo == 5){//check recent players
			char user[22];
			char response[50];
			p = strtok(NULL, "/");
			strcpy(user, p);
			sprintf(consulta, "SELECT NICKNAME FROM PLAYERS WHERE ID IN(SELECT ID FROM PLAYERS WHERE PLAYERS.NICKNAME != '%s' "
					"AND ID IN(SELECT PLAYER1 FROM RECORD WHERE GAME_ID IN(SELECT GAME_ID FROM RECORD WHERE PLAYER1 IN (SELECT ID FROM PLAYERS WHERE NICKNAME = '%s'))));", user, user);
			
			printf("%s",consulta);
			int err = mysql_query(conn, consulta);
			if (err!=0) {
				printf ("Error al  %u %s\n", 
						mysql_errno(conn), mysql_error(conn));
				exit (1);
			}
			resultado = mysql_store_result (conn);
			row = mysql_fetch_row (resultado);
			if (row == NULL)
				printf ("No se han obtenido datos en la consulta\n");
			else
			{
				//printf("Lista de jugadores registrados:");
				strcpy(response,"\0");
				while (row !=NULL) { 
					strcat(response,row[0]);
					strcat(response,"/");
					row = mysql_fetch_row (resultado);
				}
				printf("%s", response);
				write(sock_conn, response, strlen(response));
			}
			
		}
		
		int err = mysql_query(conn, consulta);
		if (err!=0) {
			printf ("Error al introducir datos la base %u %s\n", 
					mysql_errno(conn), mysql_error(conn));
			exit (1);
		}
		close(sock_conn);
	}
	return 0;
	
}

void Start(){
	
	
}
void ChangeUserName(){
	
}
void ChangePassword(){
	
}
void CheckRecentPlayersFaced(){
	
}