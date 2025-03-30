using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Helper
{
    public static class SqlConst
    {
   
        private static string connectionString = @"server=";

        public static string SqlGetir()
        {
            return connectionString;
        }
    }
}
