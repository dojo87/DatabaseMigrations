PRINT N'Configuration of tags';

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

