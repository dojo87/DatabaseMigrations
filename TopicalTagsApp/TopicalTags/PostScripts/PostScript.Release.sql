﻿
IF NOT EXISTS (SELECT TOP 1 Id FROM Topic WHERE Id = 100)
BEGIN 
	INSERT INTO Topic (Id, Title, Url) VALUES (100, 'Release','localhost');
END;