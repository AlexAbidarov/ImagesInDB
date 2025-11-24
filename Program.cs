[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

if(args.Length > 0 && args[0].Equals("--update-xml", StringComparison.OrdinalIgnoreCase)) {
    if(args.Length < 3) {
        Console.WriteLine("Usage: program --update-xml <xmlFilePath> <imagesFolderPath>");
        Console.WriteLine("Example: program --update-xml data.xml ./DB_data.xml_Images");
        return;
    }
    
    string xmlFilePath = args[1];
    string imagesFolderPath = args[2];
    
    ImagesInDB.DBImagesUpdater.UpdateXMLImagesFromDir(xmlFilePath, imagesFolderPath);
}
else {
    ImagesInDB.DBImages.CreateDirImages(Environment.GetCommandLineArgs());
}
