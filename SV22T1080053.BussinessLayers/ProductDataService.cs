using SV22T1080053.DataLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.BussinessLayers
{
    public static class ProductDataService
    {
        private static readonly ProductDAL productDB;
        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }
        public static ProductDAL ProductDB => productDB;

    }
}
