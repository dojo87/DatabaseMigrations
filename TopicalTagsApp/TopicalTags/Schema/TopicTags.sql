CREATE TABLE [dbo].[TopicTags]
(
	[TopicId] INT,
	[TagId] INT, 
    CONSTRAINT [FK_TopicTag_ToTopic] FOREIGN KEY ([TopicId]) REFERENCES [Topics]([Id]), 
    CONSTRAINT [FK_TopicTag_ToTag] FOREIGN KEY ([TagId]) REFERENCES [Tags]([Id])
)
