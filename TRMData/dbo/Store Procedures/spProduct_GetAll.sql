CREATE PROCEDURE [dbo].[spProduct_GetAll]

AS
Begin
	SET NOCOUNT ON;

	SELECT Id, ProductName, [Description], RetailPrice, QuantityInStock, IsTaxable , ProductImage
	From dbo.Product AS P
	ORDER BY P.ProductName;
End

