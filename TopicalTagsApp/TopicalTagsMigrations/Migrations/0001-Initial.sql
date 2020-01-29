PRINT N'Creating [dbo].[Tag]...';


GO
CREATE TABLE [dbo].[Tag] (
    [Id]   INT           NOT NULL,
    [Name] VARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating [dbo].[Topic]...';


GO
CREATE TABLE [dbo].[Topic] (
    [Id]    INT            NOT NULL,
    [Title] VARCHAR (2000) NOT NULL,
    [Url]   VARCHAR (2000) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating [dbo].[TopicTags]...';


GO
CREATE TABLE [dbo].[TopicTags] (
    [TopicId] INT NOT NULL,
    [TagId]   INT NOT NULL,
    CONSTRAINT [PK_TopicTags] PRIMARY KEY CLUSTERED ([TopicId] ASC, [TagId] ASC)
);


GO
PRINT N'Creating [dbo].[FK_TopicTag_ToTopic]...';


GO
ALTER TABLE [dbo].[TopicTags] WITH NOCHECK
    ADD CONSTRAINT [FK_TopicTag_ToTopic] FOREIGN KEY ([TopicId]) REFERENCES [dbo].[Topic] ([Id]);


GO
PRINT N'Creating [dbo].[FK_TopicTag_ToTag]...';


GO
ALTER TABLE [dbo].[TopicTags] WITH NOCHECK
    ADD CONSTRAINT [FK_TopicTag_ToTag] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tag] ([Id]);


GO
ALTER TABLE [dbo].[TopicTags] WITH CHECK CHECK CONSTRAINT [FK_TopicTag_ToTopic];

ALTER TABLE [dbo].[TopicTags] WITH CHECK CHECK CONSTRAINT [FK_TopicTag_ToTag];

