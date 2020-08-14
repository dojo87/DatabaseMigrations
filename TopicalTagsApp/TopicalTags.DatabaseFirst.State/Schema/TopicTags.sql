CREATE TABLE [dbo].[TopicTags]
(
	[TopicId] INT NOT NULL,
	[TagId] INT NOT NULL, 
	CONSTRAINT PK_TopicTags PRIMARY KEY (TopicId, TagId),
    CONSTRAINT [FK_TopicTag_ToTopic] FOREIGN KEY ([TopicId]) REFERENCES [Topic]([Id]), 
    CONSTRAINT [FK_TopicTag_ToTag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id])
)
