using FluentAssertions;
using GLMS.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Glms_Monolith_Test;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"glms-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);

        var env = new TestWebHostEnvironment
        {
            ContentRootPath = _tempRoot,
            WebRootPath = Path.Combine(_tempRoot, "wwwroot")
        };

        _service = new LocalFileStorageService(env);
    }

    [Fact]
    public void IsPdf_WithValidPdfHeaderAndExtension_ReturnsTrue()
    {
        var file = BuildFormFile("%PDF-1.7\nsample content"u8.ToArray(), "contract.pdf", "application/pdf");

        var valid = _service.IsPdf(file, out var validationError);

        valid.Should().BeTrue();
        validationError.Should().BeEmpty();
    }

    [Fact]
    public void IsPdf_WithNonPdfExtension_ReturnsFalse()
    {
        var file = BuildFormFile("%PDF-1.7\nsample content"u8.ToArray(), "contract.txt", "text/plain");

        var valid = _service.IsPdf(file, out var validationError);

        valid.Should().BeFalse();
        validationError.Should().Be("Only .pdf files are allowed.");
    }

    [Fact]
    public void IsPdf_WithExeExtension_ReturnsFalse()
    {
        var file = BuildFormFile("%PDF-1.7\n"u8.ToArray(), "malware.exe", "application/octet-stream");

        var valid = _service.IsPdf(file, out var validationError);

        valid.Should().BeFalse();
        validationError.Should().Be("Only .pdf files are allowed.");
    }

    [Fact]
    public void IsPdf_WithNullFile_ReturnsFalse()
    {
        var valid = _service.IsPdf(null!, out var validationError);

        valid.Should().BeFalse();
        validationError.Should().NotBeEmpty();
    }

    [Fact]
    public void IsPdf_WithWrongSignature_ReturnsFalse()
    {
        var file = BuildFormFile("NOT-A-PDF"u8.ToArray(), "contract.pdf", "application/pdf");

        var valid = _service.IsPdf(file, out var validationError);

        valid.Should().BeFalse();
        validationError.Should().Contain("Invalid PDF signature");
    }

    [Fact]
    public async Task SaveContractAgreementAsync_WithInvalidFile_Throws()
    {
        var file = BuildFormFile("NOT-A-PDF"u8.ToArray(), "contract.pdf", "application/pdf");

        var act = () => _service.SaveContractAgreementAsync(file);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid PDF signature*");
    }

    [Fact]
    public async Task SaveContractAgreementAsync_WithValidPdf_SavesFileAndReturnsMetadata()
    {
        var file = BuildFormFile("%PDF-1.7\nvalid payload"u8.ToArray(), "signed-contract.pdf", "application/pdf");

        var result = await _service.SaveContractAgreementAsync(file);

        result.OriginalFileName.Should().Be("signed-contract.pdf");
        result.ContentType.Should().Be("application/pdf");
        result.StoredRelativePath.Should().StartWith("uploads/contracts/");
        File.Exists(_service.GetFullPath(result.StoredRelativePath)).Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private static IFormFile BuildFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "GLMS.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
