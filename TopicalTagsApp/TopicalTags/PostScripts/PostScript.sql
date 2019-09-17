/*
Post-Deployment Script Template
This file will be taken as default if no environment specific script will be found. 
You should not edit PostScript.sql as it is generated. If you want to edit default values, edit PostScript.Default.sql
*/

IF NOT EXISTS (SELECT TOP 1 Id FROM Topic WHERE Id = 100)
BEGIN 
	INSERT INTO Topic (Id, Title, Url) VALUES (100, 'DEFAULT','localhost');
END;