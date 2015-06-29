CREATE PROCEDURE [dbo].[UpdateEntity]
	@id uniqueidentifier,
	@processingStatus nvarchar(50)
AS
	UPDATE [dbo].DataEntity
		SET ProcessingStatus = @processingStatus
	WHERE Id = @id;
RETURN 0;
