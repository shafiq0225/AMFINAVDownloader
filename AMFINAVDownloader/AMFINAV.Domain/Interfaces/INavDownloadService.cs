using AMFINAV.Domain.Enums;

namespace AMFINAV.Domain.Interfaces
{
    public interface INavDownloadService
    {
        Task<(DownloadStatus Status, string Content, string ErrorMessage, int RecordCount)> DownloadNavDataAsync();

    }
}