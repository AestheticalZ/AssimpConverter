using System;
using System.Drawing;
using Assimp;
using Matcha;
using Pastel;

public class MainProgram
{
    public static MatchaLogger Logger;
    
    static void Main(string[] args)
    {
        MatchaLoggerSettings loggerSettings = new MatchaLoggerSettings()
        {
            LogToFile = false,
            OutputDate = false
        };
        Logger = new MatchaLogger(loggerSettings);
        
        Console.WriteLine("AssimpConverter".Pastel(Color.Aquamarine) + 
                          " - ".Pastel(Color.Turquoise) + 
                          "Simple conversion using Assimp".Pastel(Color.CornflowerBlue));
        Console.WriteLine("By ".Pastel(Color.DarkCyan) + 
                          "AestheticalZ ".Pastel(Color.CadetBlue) +
                          "https://github.com/AestheticalZ".Pastel(Color.Cyan));
        
        Console.WriteLine();
        
        if (args.Length < 2) Panic("You must pass at least 2 arguments to this program.");

        string inputFile = args[0];
        string outputFile = args[1];

        if (!File.Exists(inputFile)) Panic("Input file does not exist.");

        FileStream bogusStream = null;
        try
        {
            bogusStream = File.Create(outputFile);
        }
        catch (Exception)
        {
            Panic("Output file path does not exist.");
        }
        finally
        {
            bogusStream?.Close();
        }

        string inputExtension = Path.GetExtension(inputFile);
        string outputExtension = Path.GetExtension(outputFile);

        AssimpContext assimpContext = new AssimpContext();

        if (!assimpContext.IsImportFormatSupported(inputExtension))
        {
            string[] supportedImportFormats = assimpContext.GetSupportedImportFormats();

            IEnumerable<string> supportedImportExtensions = supportedImportFormats.Select(x => x.Replace(".", ""));

            string joinedExtensions = string.Join(", ", supportedImportExtensions);
            
            Panic("Input file format is not supported! Supported extensions are: " + joinedExtensions);
        }
        
        ExportFormatDescription[] supportedExportFormats = assimpContext.GetSupportedExportFormats();

        if (!assimpContext.IsExportFormatSupported(outputExtension))
        {
            IEnumerable<string> supportedFileExtensions = supportedExportFormats.Select(x => x.FileExtension);

            string joinedExtensions = string.Join(", ", supportedFileExtensions);
            
            Panic("Output file format is not supported! Supported extensions are: " + joinedExtensions);
        }

        string outputFormatId = supportedExportFormats.First(x => x.FileExtension == outputExtension.Replace(".", "")).FormatId;
        
        Logger.Log("Loading input file...", LogSeverity.Information);
        
        Scene importedScene = null;
        try
        {
            importedScene = assimpContext.ImportFile(inputFile);
        }
        catch (Exception e)
        {
            Panic("Error loading input file:\n" + e);
        }
        
        Logger.Log("Loaded input file successfully.", LogSeverity.Success);
        Logger.Log($"Exporting to output path in {outputFormatId} format...", LogSeverity.Information);

        try
        {
            bool successful = assimpContext.ExportFile(importedScene, outputFile, outputFormatId);

            if (!successful)
                Panic("There was an unknown error exporting the file.");
        }
        catch (Exception e)
        {
            Panic("Error writing output file:\n" + e);
        }
        
        Logger.Log($"Success! The file has been written to \"{Path.GetFullPath(outputFile)}\".", LogSeverity.Success);
    }

    static void Panic(string Message)
    {
        Logger.Log(Message, LogSeverity.Error);
        
        Environment.Exit(-1);
    }
}