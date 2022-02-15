using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData : ISaleData
    {
        private readonly IConfiguration _config;
        private readonly IProductData _productData;
        private readonly ISqlDataAccess _sql;

        public SaleData(IConfiguration config, IProductData productData, ISqlDataAccess sql)
        {
            _config = config;
            _productData = productData;
            _sql = sql;
        }
        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            //TODO: Make this SOLID/DRY/Better
            //Start filling in the sale details models we will save to the database

            // SaleDetail thong tin chi tiet 1 san pham
            List<SaleDetailDBModel> details = new List<SaleDetailDBModel>();

            string rateText = _config["Tax:taxRate"];
            var taxRate = ConfigHelper.GetTaxRate(rateText) / 100;

            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDBModel
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                //Get the info about this product
                var productInfo = _productData.GetProductById(item.ProductId);

                if (productInfo == null)
                {
                    throw new Exception($"The product Id of {item.ProductId} could not be found in the database");
                }

                // co 2 gia tri PurchasePrice (SaleDetails vs Inventory)
                //Inventory PurchasePrice (tong so tien mua x mon) "store mua vao"
                //SaleDetails PurchasePrice (tong so tien mua x mon) "store ban ra or nguoi mua"
                detail.PurchasePrice = (productInfo.RetailPrice * detail.Quantity);

                if (productInfo.IsTaxable)
                {
                    // tax tren tong so tien
                    detail.Tax = (detail.PurchasePrice * taxRate);
                }

                details.Add(detail);
            }

            //Create the Sale model

            //Sale tong thong tin tren 1 bill List<san pham tren 1 bill>
            SaleDBModel sale = new SaleDBModel
            {
                // tong tien 1 bill chua tinh thue
                SubTotal = details.Sum(x => x.PurchasePrice),
                Tax = details.Sum(x => x.Tax),
                CashierId = cashierId
                //SaleDate default UTC time (in case of not processing right away, put sale in a queue process overnight)
                //thi manually chinh SaleDate
            };

            //total ca tien ca thue 1 bill
            sale.Total = sale.SubTotal + sale.Tax;

            try
            {
                _sql.StartTransaction("TRMData");

                //Save the sale model
                _sql.SaveDataInTransaction("dbo.spSale_Insert", sale);

                //Get the ID from the sale model
                sale.Id = _sql.LoadDataInTransaction<int, dynamic>("spSale_LookUp", new { sale.CashierId, sale.SaleDate }).FirstOrDefault();

                //Finish filling in the sale detail models
                foreach (var item in details)
                {
                    item.SaleId = sale.Id;
                    //Save the sale detail models
                    //call Database each time of foreach (if khong muon thi create table<T> bulk insert)
                    _sql.SaveDataInTransaction<SaleDetailDBModel>("dbo.spSaleDetail_Insert", item);
                }

                //commit transaction && close connection
                _sql.CommitTransaction();
            }
            catch
            {

                _sql.RollBackTransaction();
                throw;
            }
        }

        public List<SaleReportModel> GetSaleReport()
        {
            var output = _sql.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", new { }, "TRMData");

            return output;
        }
    }
}
