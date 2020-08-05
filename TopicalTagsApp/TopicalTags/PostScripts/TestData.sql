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

	INSERT INTO Topic (Id, Title, Url) VALUES (4, 'Why did dinosaurs disapear?','https://www.youtube.com/watch?v=YEN8Jzsi2NI');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (4,1), (4,9);	
	
END;