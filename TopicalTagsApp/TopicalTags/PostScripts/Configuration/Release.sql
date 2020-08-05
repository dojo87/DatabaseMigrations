/*
Post-Deployment Script
Specific Release scripts
*/

IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'RELEASE')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('Config','RELEASE');
END;