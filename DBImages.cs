using System.Data;
using System.Drawing;

namespace ImagesInDB {
    public static class DBImages {
        public static void CreateDirImages(string[] dirNames) {
            foreach(string dirName in dirNames) {
                if(Directory.Exists(dirName)) {
                    CreateDBImages("xml", DBHelper.GetXMLDataSet, dirName);
                    CreateDBImages("mdb", DBHelper.GetMDBDataSet, dirName);
                    CreateDBImages("sqlite3", CreateSQLImages, dirName);
                    CreateDBImages("db", CreateSQLImages, dirName);
                    CreateDBImages("mdf", DBHelper.GetMDFDataSet, dirName);
                    CreateDataSetImages(DBHelper.GetDBFDataSet(dirName), "DBFTables", dirName);
                }
            }
        }
        static void CreateDBImages(string ext, Action<FileInfo, string> createImages, string dirName) {
            Directory.GetFiles(dirName, $"*.{ext}").ToList()
                .ForEach(f => createImages(new FileInfo(f), dirName));
        }
        static void CreateDBImages(string ext, Func<string, DataSet> getDataSet, string dirName) {
            Directory.GetFiles(dirName, $"*.{ext}").ToList()
                .ForEach(f => {
                    FileInfo fi = new(f);
                    CreateDataSetImages(getDataSet(fi.FullName), fi.Name, dirName);
                });
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
            string folderName = $"DB_{name}";
            Console.WriteLine($"Saving images to {folderName}: {dt.TableName} table");
            foreach(DataRow dr in dt.Rows) {
                foreach(DataColumn dc in dt.Columns) {
                    if(dc.DataType == typeof(byte[]) && dr[dc] is not System.DBNull) {
                        byte[] image = (byte[])dr[dc];
                        SaveImage(image, @$"{prefix}\{folderName}\{dt.TableName}_{dc.ColumnName}_{++i:D2}");
                    }
                }
            }
        }
        static void SaveImagesByList(string prefix, string dbName, string tableName, List<object> list) {
            int i = 0;
            string folderName = $"DB_{dbName}_Images";
            Console.WriteLine($"Saving images to {folderName}: {tableName} table");
            foreach(var row in list) {
                var rowDict = (IDictionary<string, object>)row;
                foreach(var column in rowDict) {
                    if(column.Value is not null && column.Value.GetType() == typeof(byte[])) {
                        byte[] image = (byte[])column.Value;
                        SaveImage(image, @$"{prefix}\{folderName}\{tableName}_{column.Key}_{++i:D3}");
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
            } catch {
            }
        }
    }
}
