CREATE PROCEDURE [dbo].[spProduct_GetById]
	@Id int = 0
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Id], [ProductName], [Description], [RetailPrice], [QuantityInStock], [IsTaxable] , ProductImage
	FROM dbo.Product AS P
	WHERE P.Id = @Id
END
