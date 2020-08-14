IF NOT EXISTS (SELECT TOP 1 [Key] FROM [Configuration] WHERE [Key] = 'VariableConfig')
BEGIN 
	INSERT INTO [Configuration] ([Key], [Value]) VALUES ('VariableConfig','$(configuration)');
END;