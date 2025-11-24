using System.Data;
using System.Text.RegularExpressions;

namespace ImagesInDB {
    public static class DBImagesUpdater {
        /// <summary>
        /// Updates an XML database file with images from a folder.
        /// The folder should contain images with names following the pattern: {tableName}_{columnName}_{index}.{ext}
        /// For example: Products_Image_01.jpg, Products_Thumbnail_02.png
        /// </summary>
        /// <param name="xmlFilePath">Path to the XML database file</param>
        /// <param name="imagesFolderPath">Path to the folder containing updated images</param>
        public static void UpdateXMLImagesFromDir(string xmlFilePath, string imagesFolderPath) {
            if(!File.Exists(xmlFilePath)) {
                Console.WriteLine($"XML file not found: {xmlFilePath}");
                return;
            }
            
            if(!Directory.Exists(imagesFolderPath)) {
                Console.WriteLine($"Images folder not found: {imagesFolderPath}");
                return;
            }
            
            Console.WriteLine($"Loading XML file: {xmlFilePath}");
            
            // Create backup
            string backupPath = $"{xmlFilePath}.backup";
            File.Copy(xmlFilePath, backupPath, true);
            Console.WriteLine($"Created backup: {backupPath}");
            
            DataSet ds = DBHelper.GetXMLDataSet(xmlFilePath);
            
            if(ds == null || ds.Tables.Count == 0) {
                Console.WriteLine("No tables found in XML file.");
                return;
            }
            
            UpdateImagesInDataSet(ds, imagesFolderPath);
            
            // Save the updated DataSet back to XML preserving .NET Framework compatibility
            try {
                string tempFile = $"{xmlFilePath}.temp";
                ds.WriteXml(tempFile, XmlWriteMode.WriteSchema);
                
                // Fix the schema to use .NET Framework compatible assembly references
                string xmlContent = File.ReadAllText(tempFile);
                
                // Replace .NET 8 assembly references with .NET Framework 4.0 compatible ones
                // This ensures compatibility when the XML is read by .NET Framework applications
                xmlContent = Regex.Replace(
                    xmlContent,
                    @"System\.Private\.CoreLib, Version=\d+\.\d+\.\d+\.\d+, Culture=neutral, PublicKeyToken=7cec85d7bea7798e",
                    "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                );
                
                File.WriteAllText(xmlFilePath, xmlContent);
                File.Delete(tempFile);
                
                Console.WriteLine($"Successfully updated XML file: {xmlFilePath}");
                Console.WriteLine("? Schema assembly references converted for .NET Framework compatibility");
            } catch(Exception ex) {
                Console.WriteLine($"Error saving XML file: {ex.Message}");
                if(File.Exists(backupPath)) {
                    File.Copy(backupPath, xmlFilePath, true);
                    Console.WriteLine("Restored from backup due to error");
                }
            }
        }
        
        static void UpdateImagesInDataSet(DataSet ds, string imagesFolderPath) {
            // Get all image files in the folder and subfolders
            string[] imageFiles = Directory.GetFiles(imagesFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            
            Console.WriteLine($"Found {imageFiles.Length} image files to process");
            
            int updatedCount = 0;
            
            foreach(string imageFile in imageFiles) {
                string fileName = Path.GetFileNameWithoutExtension(imageFile);
                
                // Parse filename: {tableName}_{columnName}_{index}
                // Example: Products_Image_01 or MyTable_MyColumn_003
                string[] parts = fileName.Split('_');
                if(parts.Length < 3) {
                    Console.WriteLine($"Skipping file with invalid name format: {fileName} (expected: tableName_columnName_index)");
                    continue;
                }
                
                // The index is the last part
                string indexStr = parts[^1];
                if(!int.TryParse(indexStr, out int rowIndex)) {
                    Console.WriteLine($"Skipping file with non-numeric index: {fileName}");
                    continue;
                }
                
                // Try to find the table name in the dataset
                // Start with single word, then try combinations
                string tableName = null;
                string columnName = null;
                
                for(int i = 1; i <= parts.Length - 2; i++) {
                    string testTableName = string.Join("_", parts.Take(i));
                    if(ds.Tables.Contains(testTableName)) {
                        tableName = testTableName;
                        columnName = string.Join("_", parts.Skip(i).Take(parts.Length - i - 1));
                        break;
                    }
                }
                
                if(tableName == null || columnName == null) {
                    Console.WriteLine($"Could not find matching table for filename: {fileName}");
                    continue;
                }
                
                DataTable dt = ds.Tables[tableName];
                if(!dt.Columns.Contains(columnName)) {
                    Console.WriteLine($"Column '{columnName}' not found in table '{tableName}'");
                    continue;
                }
                
                // Convert 1-based index from filename to 0-based row index
                // The extraction uses {i:D2} format which starts from 1
                int actualRowIndex = rowIndex - 1;
                if(actualRowIndex < 0 || actualRowIndex >= dt.Rows.Count) {
                    Console.WriteLine($"Row index {rowIndex} out of range for table '{tableName}' (has {dt.Rows.Count} rows)");
                    continue;
                }
                
                DataColumn dc = dt.Columns[columnName];
                DataRow dr = dt.Rows[actualRowIndex];
                
                // Read the image file and update the database
                try {
                    byte[] imageBytes = File.ReadAllBytes(imageFile);
                    
                    if(dc.DataType == typeof(byte[])) {
                        dr[dc] = imageBytes;
                        Console.WriteLine($"? Updated {tableName}.{columnName}[row {rowIndex}] with {Path.GetFileName(imageFile)} ({imageBytes.Length} bytes)");
                        updatedCount++;
                    }
                    else if(dc.DataType == typeof(string)) {
                        dr[dc] = Convert.ToBase64String(imageBytes);
                        Console.WriteLine($"? Updated {tableName}.{columnName}[row {rowIndex}] with base64 encoded {Path.GetFileName(imageFile)} ({imageBytes.Length} bytes)");
                        updatedCount++;
                    }
                    else {
                        Console.WriteLine($"Unsupported column type for '{columnName}': {dc.DataType} (expected byte[] or string)");
                    }
                } catch(Exception ex) {
                    Console.WriteLine($"Error reading image file {imageFile}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"\nTotal images updated: {updatedCount} out of {imageFiles.Length}");
        }
    }
}
