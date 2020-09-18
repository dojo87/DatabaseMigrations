
PRINT N'Debug configuration specific';
IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'Config')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('Config','DEBUG');
END;

IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'RecreateDatabase')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('RecreateDatabase','$RecreateDatabase$');
END;
GO
