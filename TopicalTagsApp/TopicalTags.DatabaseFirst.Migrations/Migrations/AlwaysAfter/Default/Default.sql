PRINT N'Default configuration specific';
IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'Config')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('Config','Default');
END;
GO
