/*
Post-Deployment Script
Specific Default scripts
*/

IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'DEFAULT')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('Config','DEFAULT');
END;