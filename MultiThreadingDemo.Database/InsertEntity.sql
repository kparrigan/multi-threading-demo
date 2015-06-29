CREATE PROCEDURE [dbo].[InsertEntity]
	@id uniqueidentifier,
	@processingStatus nvarchar(50)
AS
	INSERT INTO dbo.DataEntity VALUES (@id, @processingStatus);
RETURN 0;
