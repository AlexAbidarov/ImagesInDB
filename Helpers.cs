using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImagesInDB {
    public static class ImageHelper {
        public static ImageFormat GetImageFormat(Image img) {
            return $"{img.RawFormat}" switch {
                "Jpeg" => ImageFormat.Jpeg,
                "Png" => ImageFormat.Png,
                "Gif" => ImageFormat.Gif,
                "Bmp" => ImageFormat.Bmp,
                "Emf" => ImageFormat.Emf,
                "Wmf" => ImageFormat.Wmf,
                "Tiff" => ImageFormat.Tiff,
                "Exif" => ImageFormat.Exif,
                "Icon" => ImageFormat.Icon,
                _ => ImageFormat.Bmp
            };
        }
        public static string GetImageExtension(Image img) {
            return $"{img.RawFormat}" switch {
                "Jpeg" => "jpg",
                _ => $"{img.RawFormat}".ToLower()
            };
        }
    }
    public static class IOHelper {
        public static void CheckDir(string dir) {
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
    public static class DBHelper {
        public static DataSet GetMDBDataSet(string path) {
            DataSet dataSet = new();
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};";
            using(OleDbConnection connection = new(connectionString)) {
                try {
                    connection.Open();
                    DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    foreach(DataRow row in schemaTable.Rows) {
                        string tableName = row["TABLE_NAME"].ToString();
                        if(!tableName.StartsWith("MSys")) {
                            OleDbDataAdapter dataAdapter = new($"SELECT * FROM [{tableName}]", connection);
                            DataTable dataTable = new(tableName);
                            dataAdapter.Fill(dataTable);
                            dataSet.Tables.Add(dataTable);
                        }
                    }
                } catch(Exception ex) {
                    Console.WriteLine($"MDB Error: {ex.Message}");
                } finally {
                    if(connection.State == ConnectionState.Open) {
                        connection.Close();
                    }
                }
            }
            return dataSet;
        }
        public static DataSet GetMDFDataSet(string path) {
            DataSet dataSet = new();
            string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={path};Integrated Security=True;";
            using(SqlConnection connection = new(connectionString)) {
                try {
                    connection.Open();
                    DataTable tablesSchema = connection.GetSchema("Tables");
                    foreach(DataRow row in tablesSchema.Rows) {
                        string tableName = row["TABLE_NAME"].ToString();
                        SqlDataAdapter dataAdapter = new($"SELECT * FROM {tableName}", connection);
                        DataTable dataTable = new(tableName);
                        dataAdapter.Fill(dataTable);
                        dataSet.Tables.Add(dataTable);
                    }
                } catch(Exception ex) {
                    Console.WriteLine($"MDF Error: {ex.Message}");
                } finally {
                    if(connection.State == ConnectionState.Open) {
                        connection.Close();
                    }
                }
                return dataSet;
            }
        }
        public static void GetSQLTables(FileInfo fi, string prefix, Action<string, string, string, List<object>> saveImages) {
            string connectionString = $"Data Source={fi.FullName};";
            using(SqliteConnection connection = new(connectionString)) {
                try {
                    connection.Open();
                    var tables = connection.Query<string>("SELECT name FROM sqlite_master WHERE type = 'table';");
                    foreach(var table in tables)
                        saveImages(prefix, fi.Name, $"{table}", connection.Query($"SELECT * FROM {table}").ToList());
                } catch(Exception ex) {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                }
            }
        }
    }
}
