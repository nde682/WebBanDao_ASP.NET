using Dapper;
using SV22T1080053.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DataLayers
{
    /// <summary>
    /// định nghĩa các phép xử lý dữ liệu liên quan đến tỉnh thành
    /// </summary>
    public class ProvinceDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public ProvinceDAL(string connectionString) : base(connectionString)
        {
        }
        public async Task<IEnumerable<Province>> ListAsync()
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = "select * from Provinces";
                return await connection.QueryAsync<Province>(sql);
            }
        }

    }
}
