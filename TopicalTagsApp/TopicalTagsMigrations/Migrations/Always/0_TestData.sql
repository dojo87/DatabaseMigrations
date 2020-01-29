
IF NOT EXISTS (SELECT TOP 1 Id FROM Topic)
BEGIN
	PRINT N'Topic table empty - treating as fresh database to populate with some test data';

	INSERT INTO Topic (Id, Title, Url) VALUES (1, 'Origin of Life Problems for Naturalists','https://answersingenesis.org/origin-of-life/origin-of-life-problems-for-naturalists/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (1,1), (1,9), (1,10);	

	INSERT INTO Topic (Id, Title, Url) VALUES (2, 'Power Plants','https://answersingenesis.org/biology/plants/power-plants/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (2,1), (2,4), (2,5), (2,6);

	INSERT INTO Topic (Id, Title, Url) VALUES (3, 'Evidence for a Young World','https://answersingenesis.org/astronomy/age-of-the-universe/evidence-for-a-young-world/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (3,1), (3,4), (3,7), (3,8);

	INSERT INTO Topic (Id, Title, Url) VALUES (4, 'Are Atheists Right? Is Faith the Absence of Reason/Evidence?','https://answersingenesis.org/christianity/are-atheists-right/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (4,1), (4,2), (4,3);	
	
END;