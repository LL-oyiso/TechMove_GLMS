namespace GLMS.Api.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private readonly string _contractsFolderAbsolute;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        var webRoot = env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(env.ContentRootPath, "wwwroot");
        }

        _contractsFolderAbsolute = Path.Combine(webRoot, "uploads", "contracts");
        Directory.CreateDirectory(_contractsFolderAbsolute);
    }

    public bool IsPdf(IFormFile file, out string validationError)
    {
        validationError = string.Empty;

        if (file == null || file.Length == 0)
        {
            validationError = "Please upload a non-empty PDF file.";
            return false;
        }

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            validationError = "Only .pdf files are allowed.";
            return false;
        }

        using var stream = file.OpenReadStream();
        if (stream.Length < PdfSignature.Length)
        {
            validationError = "Invalid PDF file.";
            return false;
        }

        var header = new byte[PdfSignature.Length];
        _ = stream.Read(header, 0, header.Length);

        if (!header.SequenceEqual(PdfSignature))
        {
            validationError = "Invalid PDF signature. Only real PDF files are accepted.";
            return false;
        }

        return true;
    }

    public async Task<StoredFileResult> SaveContractAgreementAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (!IsPdf(file, out var error))
        {
            throw new InvalidOperationException(error);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}.pdf";
        var absolutePath = Path.Combine(_contractsFolderAbsolute, uniqueFileName);

        await using (var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        var relativePath = Path.Combine("uploads", "contracts", uniqueFileName).Replace("\\", "/");

        return new StoredFileResult(
            OriginalFileName: Path.GetFileName(file.FileName),
            StoredRelativePath: relativePath,
            ContentType: "application/pdf",
            UploadedAtUtc: DateTime.UtcNow
        );
    }

    public void DeleteIfExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public string GetFullPath(string relativePath)
    {
        var normalized = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(Directory.GetParent(_contractsFolderAbsolute)!.Parent!.FullName, normalized);
    }
}
