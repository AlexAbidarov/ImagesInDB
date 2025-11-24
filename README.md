# ImagesInDB

A bidirectional tool for working with images in databases. Extract images from various database formats or update images in XML databases.

## Supported Database Formats

- XML (.xml)
- Microsoft Access (.mdb)
- SQL Server (.mdf)
- SQLite (.sqlite3, .db)
- dBase (.dbf)

## Features

### 1. Extract Images from Databases (Original Feature)

Extract images from databases into organized folders.

#### Usage:
```bash
ImagesInDB.exe <directory1> [directory2] [...]
```

#### Example:
```bash
ImagesInDB.exe C:\Data
```

The program will:
- Scan for database files in the specified directories
- Extract images from tables with byte[] or Base64 string columns
- Create folders named `DB_{filename}_Images`
- Save images with the naming pattern: `{tableName}_{columnName}_{rowIndex}.{ext}`

**Example output:**
- `DB_products.xml_Images\Products_Image_01.jpg`
- `DB_products.xml_Images\Products_Thumbnail_02.png`

### 2. Update Images in XML Databases (New Feature)

Update images in XML databases from a folder of edited images.

#### Usage:
```bash
ImagesInDB.exe --update-xml <xmlFilePath> <imagesFolderPath>
```

#### Parameters:
- `xmlFilePath`: Path to the XML database file to update
- `imagesFolderPath`: Path to the folder containing updated images

#### Image File Naming Convention:
Images must follow this pattern:
```
{tableName}_{columnName}_{rowIndex}.{extension}
```

Where:
- `tableName`: Name of the table in the XML database
- `columnName`: Name of the column containing the image data
- `rowIndex`: Row number (1-based, starting from 01, 02, 03...)
- `extension`: Image file extension (jpg, png, gif, bmp, tiff)

#### Example:
```bash
ImagesInDB.exe --update-xml C:\Data\products.xml C:\Data\DB_products.xml_Images
```

### Typical Workflow

1. **Extract images from XML:**
   ```bash
   ImagesInDB.exe C:\DatabaseFolder
   ```
   Creates: `DB_products.xml_Images\Products_Image_01.jpg`, etc.

2. **Edit the images** using your preferred image editor

3. **Update the XML database:**
   ```bash
   ImagesInDB.exe --update-xml C:\DatabaseFolder\products.xml C:\DatabaseFolder\DB_products.xml_Images
   ```

## Supported Image Formats

- JPEG (.jpg, .jpeg)
- PNG (.png)
- GIF (.gif)
- BMP (.bmp)
- TIFF (.tiff)

## Column Data Types

The tool supports:
- `byte[]` columns: Images stored as binary data
- `string` columns: Images stored as Base64-encoded strings

## .NET Framework Compatibility

?? **Important:** The updater automatically preserves .NET Framework compatibility!

When running on .NET 8, the tool:
1. Creates a backup of your XML file (`.backup` extension)
2. Updates the image data
3. **Automatically converts** .NET 8 assembly references back to .NET Framework 4.0 format

This ensures that XML files updated with this tool can still be read by:
- .NET Framework 2.0+ applications
- .NET Framework 4.x applications  
- .NET Core 3.1+ applications
- .NET 5, 6, 7, 8+ applications

### What Gets Fixed:
The tool automatically converts assembly references like:
```xml
<!-- .NET 8 format (incompatible with .NET Framework) -->
msdata:DataType="System.Guid, System.Private.CoreLib, Version=8.0.0.0, ..."

<!-- Automatically converted to .NET Framework format -->
msdata:DataType="System.Guid, mscorlib, Version=4.0.0.0, ..."
```

### Backup Files
Every update operation creates a `.backup` file. If something goes wrong:
1. The original file is automatically restored from backup
2. You can manually restore by renaming `yourfile.xml.backup` to `yourfile.xml`

## Notes

- Row indexing in filenames is 1-based (01, 02, 03...) to match the extraction format
- The program searches recursively in the images folder
- Images with invalid filenames or mismatched table/column names will be skipped with a warning
- A backup is automatically created before any updates
- Schema compatibility with .NET Framework is automatically maintained
