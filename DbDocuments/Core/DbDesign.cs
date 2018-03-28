using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DbDocuments.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DbDocuments.Core
{
    public class DbDesign
    {
        private readonly IMemoryCache _memoryCache;

        public DbDesign(IMemoryCache memoryCache, IOptions<DatabaseConfig> options)
        {
            _memoryCache = memoryCache;
            
            this.dbs.Add("CXAgentDB", $"server={options.Value.Server};database=CXAgentDB;uid={options.Value.Uid};pwd={options.Value.Password};Application Name={options.Value.ApplicationName};");
            this.dbs.Add("CXGameUserDB", $"server={options.Value.Server};database=CXGameUserDB;uid={options.Value.Uid};pwd={options.Value.Password};Application Name={options.Value.ApplicationName};");
            this.dbs.Add("CXManageDB", $"server={options.Value.Server};database=CXManageDB;uid={options.Value.Uid};pwd={options.Value.Password};Application Name={options.Value.ApplicationName};");
            this.dbs.Add("CXServerInfoDB", $"server={options.Value.Server};database=CXServerInfoDB;uid={options.Value.Uid};pwd={options.Value.Password};Application Name={options.Value.ApplicationName};");
            this.dbs.Add("CXTreasureDB", $"server={options.Value.Server};database=CXTreasureDB;uid={options.Value.Uid};pwd={options.Value.Password};Application Name={options.Value.ApplicationName};");
            this.dbs.Add("CXReportDB", $"server={options.Value.Server};database=CXReportDB;uid={options.Value.Uid};pwd={options.Value.Password};Application Name={options.Value.ApplicationName};");
        }

        private Dictionary<string, string> dbs = new Dictionary<string, string>();
        
        

        public Dictionary<string, IList<TableFieldInfo>> GetTableInfo()
        {
            string key = "TableInfoCache";
            return _memoryCache.GetOrCreate(key, (entry =>
             {
                 string text = " SELECT  obj.name AS TableName,epTwo.value as TableDescription, CAST( col.colorder  as nvarchar) AS FieldIndex,col.name AS FieldName,ISNULL(ep.[value], '') AS Description, cast(t.name AS nvarchar)  AS DataType, CASE WHEN col.length=-1 THEN 'MAX' ELSE Convert(varchar(10),col.length) END AS DataLength, cast(ISNULL(COLUMNPROPERTY(col.id, col.name, 'Scale'), 0) AS nvarchar) AS Digit, CASE WHEN COLUMNPROPERTY(col.id, col.name, 'IsIdentity') = 1 THEN '√' ELSE ''  END AS IsIdentity, CASE WHEN EXISTS ( SELECT   1 FROM     dbo.sysindexes si INNER JOIN dbo.sysindexkeys sik ON si.id = sik.id AND si.indid = sik.indid INNER JOIN dbo.syscolumns sc ON sc.id = sik.id AND sc.colid = sik.colid INNER JOIN dbo.sysobjects so ON so.name = si.name AND so.xtype = 'PK' WHERE    sc.id = col.id AND sc.colid = col.colid ) THEN '√' ELSE '' END AS IsKey, CASE WHEN col.isnullable = 1 THEN '√'ELSE '' END AS AllowNull, ISNULL(comm.text, '') AS DefaultValue  FROM    dbo.syscolumns col LEFT OUTER JOIN dbo.systypes t ON col.xtype = t.xusertype INNER JOIN dbo.sysobjects obj ON col.id = obj.id AND obj.xtype = 'U' AND obj.status >= 0 LEFT OUTER JOIN dbo.syscomments comm ON col.cdefault = comm.id LEFT OUTER JOIN sys.extended_properties ep ON col.id = ep.major_id AND col.colid = ep.minor_id AND ep.name = 'MS_Description' LEFT OUTER JOIN sys.extended_properties epTwo ON obj.id = epTwo.major_id AND epTwo.minor_id = 0 AND epTwo.name = 'MS_Description' ORDER BY  obj.name,col.colorder ";
                 var dictionary = new Dictionary<string, IList<TableFieldInfo>>();
                 foreach (KeyValuePair<string, string> db in this.dbs)
                 {
                     using (SqlConnection cnn = new SqlConnection(db.Value))
                     {
                         List<TableFieldInfo> value = ((IDbConnection)cnn).Query<TableFieldInfo>(text).ToList();
                         dictionary.Add(db.Key, value);
                     }
                 }
                 return dictionary;
             }));
        }

        public Dictionary<string, IList<TableFieldInfo>> GetViewInfo()
        {
            string key = "ViewInfoCache";

            return _memoryCache.GetOrCreate(key, (entry =>
            {
                string text = " SELECT  obj.name AS TableName,(CASE WHEN (LEFT(definition, 2) = '/*') AND (charindex('*/', definition) > 0) THEN substring(definition, 3, patindex('%*/%', definition) - 3) ELSE '' END) as TableDescription, CAST( col.colorder  as nvarchar) AS FieldIndex,col.name AS FieldName,ISNULL(ep.[value], '') AS Description, cast(t.name AS nvarchar)  AS DataType, CASE WHEN col.length=-1 THEN 'MAX' ELSE Convert(varchar(10),col.length) END AS DataLength, cast(ISNULL(COLUMNPROPERTY(col.id, col.name, 'Scale'), 0) AS nvarchar) AS Digit, CASE WHEN COLUMNPROPERTY(col.id, col.name, 'IsIdentity') = 1 THEN '√' ELSE ''  END AS IsIdentity, CASE WHEN EXISTS ( SELECT   1 FROM     dbo.sysindexes si INNER JOIN dbo.sysindexkeys sik ON si.id = sik.id AND si.indid = sik.indid INNER JOIN dbo.syscolumns sc ON sc.id = sik.id AND sc.colid = sik.colid INNER JOIN dbo.sysobjects so ON so.name = si.name AND so.xtype = 'PK' WHERE    sc.id = col.id AND sc.colid = col.colid ) THEN '√' ELSE '' END AS IsKey, CASE WHEN col.isnullable = 1 THEN '√'ELSE '' END AS AllowNull, ISNULL(comm.text, '') AS DefaultValue  FROM    dbo.syscolumns col LEFT OUTER JOIN dbo.systypes t ON col.xtype = t.xusertype INNER JOIN dbo.sysobjects obj ON col.id = obj.id AND obj.xtype = 'V' AND obj.status >= 0 LEFT OUTER JOIN dbo.syscomments comm ON col.cdefault = comm.id LEFT OUTER JOIN sys.extended_properties ep ON col.id = ep.major_id AND col.colid = ep.minor_id AND ep.name = 'MS_Description' LEFT OUTER JOIN sys.sql_modules epTwo ON obj.id = epTwo.object_id ORDER BY  obj.name,col.colorder ";
                var dictionary = new Dictionary<string, IList<TableFieldInfo>>();
                foreach (KeyValuePair<string, string> db in this.dbs)
                {
                    using (SqlConnection cnn = new SqlConnection(db.Value))
                    {
                        List<TableFieldInfo> value = ((IDbConnection)cnn).Query<TableFieldInfo>(text).ToList();
                        dictionary.Add(db.Key, value);
                    }
                }
                return dictionary;
            }));
        }

        public Dictionary<string, IList<TableFieldInfo>> GetSPInfo()
        {
            string key = "SPInfoCache";
            return _memoryCache.GetOrCreate(key, (entry =>
            {
                string text = " SELECT  obj.name AS TableName,(CASE WHEN (LEFT(definition, 2) = '/*') AND (charindex('*/', definition) > 0) THEN substring(definition, 3, patindex('%*/%', definition) - 3) ELSE '' END) as TableDescription, CAST( col.colorder  as nvarchar) AS FieldIndex,col.name AS FieldName,ISNULL(ep.[value], '') AS Description, cast(t.name AS nvarchar)  AS DataType, CASE WHEN col.length=-1 THEN 'MAX' ELSE Convert(varchar(10),col.length) END AS DataLength, cast(ISNULL(COLUMNPROPERTY(col.id, col.name, 'Scale'), 0) AS nvarchar) AS Digit, CASE WHEN COLUMNPROPERTY(col.id, col.name, 'IsIdentity') = 1 THEN '√' ELSE ''  END AS IsIdentity, CASE WHEN EXISTS ( SELECT   1 FROM     dbo.sysindexes si INNER JOIN dbo.sysindexkeys sik ON si.id = sik.id AND si.indid = sik.indid INNER JOIN dbo.syscolumns sc ON sc.id = sik.id AND sc.colid = sik.colid INNER JOIN dbo.sysobjects so ON so.name = si.name AND so.xtype = 'PK' WHERE    sc.id = col.id AND sc.colid = col.colid ) THEN '√' ELSE '' END AS IsKey, CASE WHEN col.isnullable = 1 THEN '√'ELSE '' END AS AllowNull, ISNULL(comm.text, '') AS DefaultValue  FROM    dbo.syscolumns col LEFT OUTER JOIN dbo.systypes t ON col.xtype = t.xusertype INNER JOIN dbo.sysobjects obj ON col.id = obj.id AND obj.xtype = 'P' AND obj.status >= 0 LEFT OUTER JOIN dbo.syscomments comm ON col.cdefault = comm.id LEFT OUTER JOIN sys.extended_properties ep ON col.id = ep.major_id AND col.colid = ep.minor_id AND ep.name = 'MS_Description' LEFT OUTER JOIN sys.sql_modules epTwo ON obj.id = epTwo.object_id  ORDER BY  obj.name,col.colorder ";
                var dictionary = new Dictionary<string, IList<TableFieldInfo>>();
                foreach (KeyValuePair<string, string> db in this.dbs)
                {
                    using (SqlConnection cnn = new SqlConnection(db.Value))
                    {
                        List<TableFieldInfo> value = ((IDbConnection)cnn).Query<TableFieldInfo>(text).ToList();
                        dictionary.Add(db.Key, value);
                    }
                }
                return dictionary;
            }));
        }
    }
}
