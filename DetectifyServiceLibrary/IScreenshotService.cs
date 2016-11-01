using System.ServiceModel;

namespace DetectifyServiceLibrary
{
    [ServiceContract]
    public interface IScreenshotService
    {
        [OperationContract]
        void SaveScreenshots(string urls);

        [OperationContract]
        FileDownloadReturnMessage DownloadFile(FileDownloadMessage request);

        [OperationContract]
        FileDownloadReturnMessage DownloadAllFiles(FileDownloadMessage request);
    }
}
