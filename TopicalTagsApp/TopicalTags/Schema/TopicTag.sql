CREATE TABLE [dbo].[TopicTag]
(
	[Topic_Id] INT,
	[Tag_Id] INT, 
    CONSTRAINT [FK_TopicTag_ToTopic] FOREIGN KEY ([Topic_Id]) REFERENCES [Topic]([Id]), 
    CONSTRAINT [FK_TopicTag_ToTag] FOREIGN KEY ([Tag_Id]) REFERENCES [Tag]([Id])
)
