﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRMDataManager.Library.Internal.DataAccess;
using TRMDataManager.Library.Models;

namespace TRMDataManager.Library.DataAccess
{
    public class SaleData
    {
        private readonly IConfiguration _config;

        public SaleData(IConfiguration config)
        {
            _config = config;
        }
        public void SaveSale(SaleModel saleInfo, string cashierId)
        {
            //TODO: Make this SOLID/DRY/Better
            //Start filling in the sale details models we will save to the database

            // SaleDetail thong tin chi tiet 1 san pham
            List<SaleDetailDBModel> details = new List<SaleDetailDBModel>();
            ProductData product = new ProductData(_config);
            var taxRate = ConfigHelper.GetTaxRate() / 100;

            foreach (var item in saleInfo.SaleDetails)
            {
                var detail = new SaleDetailDBModel
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                //Get the info about this product
                var productInfo = product.GetProductById(item.ProductId);

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

            using (SqlDataAccess sql = new SqlDataAccess(_config))
            {
                try
                {
                    sql.StartTransaction("TRMData");

                    //Save the sale model
                    sql.SaveDataInTransaction("dbo.spSale_Insert", sale);

                    //Get the ID from the sale model
                    sale.Id = sql.LoadDataInTransaction<int, dynamic>("spSale_LookUp", new { sale.CashierId, sale.SaleDate }).FirstOrDefault();

                    //Finish filling in the sale detail models
                    foreach (var item in details)
                    {
                        item.SaleId = sale.Id;
                        //Save the sale detail models
                        //call Database each time of foreach (if khong muon thi create table<T> bulk insert)
                        sql.SaveDataInTransaction<SaleDetailDBModel>("dbo.spSaleDetail_Insert", item);
                    }

                    //commit transaction && close connection
                    sql.CommitTransaction();
                }
                catch
                {

                    sql.RollBackTransaction();
                    throw;
                }
            }
        }

        public List<SaleReportModel> GetSaleReport()
        {
            SqlDataAccess sql = new SqlDataAccess(_config);

            var output = sql.LoadData<SaleReportModel, dynamic>("dbo.spSale_SaleReport", new { }, "TRMData");

            return output;
        }
    }
}
