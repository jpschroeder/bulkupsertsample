using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Reflection;

namespace BulkUpsert
{
    public class Repository
    {
        public static List<int> StoreData(List<SomeTable> list)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["mydb"].ConnectionString;
            List<int> ids = new List<int>();
            using (var connection = new SqlConnection(connectionString))
            {
                ids = connection.Query<int>("sometable_upsert",
                new
                {
                    data = list.AsTableValuedParameter("dbo.sometable_type")
                },
                commandType: CommandType.StoredProcedure).ToList();
            }
            return ids;
        }

        public static List<SomeTable> GetData(List<int> ids)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["mydb"].ConnectionString;
            var ret = new List<SomeTable>();
            using (var connection = new SqlConnection(connectionString))
            {
                ret = connection.Query<SomeTable>("select * from sometable where sometable_id in @ids order by sometable_id",
                new
                {
                    ids = ids.ToArray()
                }).ToList();
            }
            return ret;
        }

        public static void RemoveData(List<int> ids)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["mydb"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Execute("delete from sometable where sometable_id in @ids",
                new
                {
                    ids = ids.ToArray()
                });
            }
        }
    }

    public class SomeTable
    {
        public int unique_field { get; set; }
        public int field1 { get; set; }
        public string field2 { get; set; }
        public bool field3 { get; set; }

        public override bool Equals(System.Object obj)
        {
            SomeTable p = obj as SomeTable;
            if (p == null)
            {
                return false;
            }
            return  (field1 == p.field1) && (field2 == p.field2) &&
                    (field3 == p.field3) && (unique_field == p.unique_field);
        }
    }

    public static class DapperExtension
    {
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(
            this IEnumerable<T> enumerable,
            string typeName,
            IEnumerable<string> orderedColumnNames = null)
        {
            return enumerable.AsDataTable(orderedColumnNames).AsTableValuedParameter(typeName);
        }
    }

    public static class EnumerableExtension
    {
        public static DataTable AsDataTable<T>(this IEnumerable<T> enumerable, IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();
            if (typeof(T).IsValueType)
            {
                dataTable.Columns.Add("NONAME", typeof(T));
                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(obj);
                }
            }
            else
            {
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] readableProperties = properties.Where(w => w.CanRead).ToArray();
                var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name)).ToArray();
                foreach (string name in columnNames)
                {
                    dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(
                        columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
                            .ToArray());
                }
            }
            return dataTable;
        }
    }
}
