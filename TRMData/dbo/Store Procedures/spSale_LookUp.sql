CREATE PROCEDURE [dbo].[spSale_LookUp]
	@CashierId nvarchar(128),
	@SaleDate datetime2
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Id]
	FROM dbo.Sale
	WHERE Sale.CashierId = @CashierId AND Sale.SaleDate = @SaleDate;
END
