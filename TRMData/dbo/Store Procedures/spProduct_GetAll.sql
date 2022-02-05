CREATE PROCEDURE [dbo].[spProduct_GetAll]

AS
Begin
	SET NOCOUNT ON;

	SELECT Id, ProductName, [Description], RetailPrice, QuantityInStock, IsTaxable
	From dbo.Product AS P
	ORDER BY P.ProductName;
End

