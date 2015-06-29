CREATE PROCEDURE [dbo].[GetSubmittedEntities]

AS
	SELECT 
		Id, 
		ProcessingStatus
	FROM
		dbo.DataEntity
	WHERE
		lower(ProcessingStatus) = 'submitted';
