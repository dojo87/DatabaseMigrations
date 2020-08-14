/*
Post-Deployment Script
Specific Debug scripts
*/

IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'Config')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('Config','DEBUG');
END;