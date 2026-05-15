using Microsoft.AspNetCore.Http;

namespace GLMS_Monolith.Services;

public interface IFileStorageService
{
    bool IsPdf(IFormFile file, out string validationError);
    Task<StoredFileResult> SaveContractAgreementAsync(IFormFile file, CancellationToken cancellationToken = default);
    void DeleteIfExists(string? relativePath);
    string GetFullPath(string relativePath);
}

public record StoredFileResult(
    string OriginalFileName,
    string StoredRelativePath,
    string ContentType,
    DateTime UploadedAtUtc
);