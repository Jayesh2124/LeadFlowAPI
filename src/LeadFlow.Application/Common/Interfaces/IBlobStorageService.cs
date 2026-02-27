using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeadFlow.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default);
    Task<(Stream Content, string ContentType)> DownloadAsync(string blobName, CancellationToken cancellationToken = default);
    Task<List<string>> ListAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
}
