public static class FileNameGenerator
{
    public static string GenerateOutputFileName(string extension)
    {
        // Validate and ensure the extension starts with a dot (e.g., ".mp4")
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        // Generate a GUID and append the extension
        return Guid.NewGuid().ToString() + extension;
    }
}
