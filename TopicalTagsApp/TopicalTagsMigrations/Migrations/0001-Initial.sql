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
-- W przypadku Tagów zastosujemy podejście UPSERT. 
-- Chcemy zawsze zadbać o to, że w bazie będzie te 10 tagów jednocześnie pozwalając, że w aplikacji może zachodzić jakaś administracja i dodawanie nowych tagów.

-- Cloning Tags into temp table
SELECT TOP 0 * INTO #TempTags FROM dbo.[Tag]

-- Seeding data
PRINT 'Data seed - Merging [Tags]'
INSERT INTO #TempTags ([Id], [Name]) VALUES
		(1, 'Answers'),
		(2, 'Worldview'),
		(3, 'Christianity'),
		(4, 'Science'),
		(5, 'Biology'),
		(6, 'Plants'),
		(7, 'Astronomy'),
		(8, 'Age of the Universe'),
		(9, 'Evolution'),
		(10, 'Origin of Life');

-- UPSERT
merge into dbo.[Tag] as Target
using #TempTags as Source
on Target.Id=Source.Id
when matched then
update set Target.[Name] = Source.[Name]
when not matched then
insert ([Id], [Name]) values (Source.[Id], Source.[Name]);

DROP TABLE #TempTags;



	
/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

IF NOT EXISTS (SELECT TOP 1 Id FROM Topic)
BEGIN

	INSERT INTO Topic (Id, Title, Url) VALUES (1, 'Origin of Life Problems for Naturalists','https://answersingenesis.org/origin-of-life/origin-of-life-problems-for-naturalists/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (1,1), (1,9), (1,10);	

	INSERT INTO Topic (Id, Title, Url) VALUES (2, 'Power Plants','https://answersingenesis.org/biology/plants/power-plants/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (2,1), (2,4), (2,5), (2,6);

	INSERT INTO Topic (Id, Title, Url) VALUES (3, 'Evidence for a Young World','https://answersingenesis.org/astronomy/age-of-the-universe/evidence-for-a-young-world/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (3,1), (3,4), (3,7), (3,8);

	INSERT INTO Topic (Id, Title, Url) VALUES (4, 'Are Atheists Right? Is Faith the Absence of Reason/Evidence?','https://answersingenesis.org/christianity/are-atheists-right/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (4,1), (4,2), (4,3);	
	
END;

IF NOT EXISTS (SELECT TOP 1 Id FROM Topic WHERE Id = 100)
BEGIN 
	INSERT INTO Topic (Id, Title, Url) VALUES (100, 'DEBUG','localhost');
END;
GO

GO
PRINT N'Checking existing data against newly created constraints';


GO
ALTER TABLE [dbo].[TopicTags] WITH CHECK CHECK CONSTRAINT [FK_TopicTag_ToTopic];

ALTER TABLE [dbo].[TopicTags] WITH CHECK CHECK CONSTRAINT [FK_TopicTag_ToTag];


GO
PRINT N'Update complete.';


GO
