using System.Data;
using System.Drawing;

namespace ImagesInDB {
    public static class DBImages {
        public static void CreateDirImages(string[] dirNames) {
            try {
                foreach(string dirName in dirNames) {
                    if(Directory.Exists(dirName)) {
                        CreateDBImages("xml", CreateXMLImages, dirName);
                        CreateDBImages("mdb", CreateMDBImages, dirName);
                        CreateDBImages("sqlite3", CreateSQLImages, dirName);
                        CreateDBImages("db", CreateSQLImages, dirName);
                        CreateDBImages("mdf", CreateMDFImages, dirName);
                    }
                }
            } catch(Exception ex) {
                Console.WriteLine($"Create Images Error: {ex.Message}");
            }
        }
        static void CreateDBImages(string ext, Action<FileInfo, string> createImages, string dirName) {
            Directory.GetFiles(dirName, $"*.{ext}").ToList().ForEach(f => createImages(new FileInfo(f), dirName));
        }
        static void CreateXMLImages(FileInfo fi, string prefix = ".") {
            DataSet ds = new();
            ds.ReadXml(fi.FullName);
            CreateDataSetImages(ds, fi.Name, prefix);
        }
        static void CreateMDBImages(FileInfo fi, string prefix = ".") {
            CreateDataSetImages(DBHelper.GetMDBDataSet(fi.FullName), fi.Name, prefix);
        }
        static void CreateMDFImages(FileInfo fi, string prefix = ".") {
            CreateDataSetImages(DBHelper.GetMDFDataSet(fi.FullName), fi.Name, prefix);
        }
        static void CreateSQLImages(FileInfo fi, string prefix = ".") {
            DBHelper.GetSQLTables(fi, prefix, SaveImagesByList);
        }
        static void CreateDataSetImages(DataSet ds, string name, string prefix) {
            if(ds != null)
                foreach(DataTable dt in ds.Tables)
                    SaveImagesByTable(dt, $"{name}_Images", prefix);
        }
        static void SaveImagesByTable(DataTable dt, string name, string prefix) {
            int i = 0;
            foreach(DataRow dr in dt.Rows) {
                foreach(DataColumn dc in dt.Columns) {
                    if(dc.DataType == typeof(byte[]) && dr[dc] is not System.DBNull) {
                        byte[] image = (byte[])dr[dc];
                        SaveImage(image, @$"{prefix}\DB_{name}\{dt.TableName}_{dc.ColumnName}_{++i:D2}");
                    }
                }
            }
        }
        static void SaveImagesByList(string prefix, string dbName, string tableName, List<object> list) {
            int i = 0;
            foreach(var row in list) {
                var rowDict = (IDictionary<string, object>)row;
                foreach(var column in rowDict) {
                    if(column.Value is not null && column.Value.GetType() == typeof(byte[])) {
                        byte[] image = (byte[])column.Value;
                        SaveImage(image, @$"{prefix}\DB_{dbName}_Images\{tableName}_{column.Key}_{++i:D3}");
                    }
                }
            }
        }
        static void SaveImage(byte[] image, string name) {
            try {
                IOHelper.CheckDir(Path.GetDirectoryName(name));
                using(MemoryStream ms = new(image)) {
                    using(Image img = Image.FromStream(ms))
                        img.Save($"{name}.{ImageHelper.GetImageExtension(img)}",
                            ImageHelper.GetImageFormat(img));
                }
            } catch(Exception ex) {
                Console.WriteLine($"Save Image Error: {ex.Message}");
            }
        }
    }
}
