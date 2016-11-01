using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace DetectifyServiceLibrary
{
    [MessageContract]
    public class FileDownloadMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public FileMetaData FileMetaData;
    }

    [MessageContract]
    public class FileDownloadReturnMessage : IDisposable
    {
        public FileDownloadReturnMessage() { }
        public FileDownloadReturnMessage(FileMetaData metaData, Stream stream)
        {
            this.DownloadedFileMetadata = metaData;
            this.FileByteStream = stream;
        }

        [MessageHeader(MustUnderstand = true)]
        public FileMetaData DownloadedFileMetadata;
        [MessageBodyMember(Order = 1)]
        public Stream FileByteStream;

        public void Dispose()
        {
            FileByteStream?.Dispose();
        }
    }

    [DataContract]
    public class FileMetaData
    {
        public FileMetaData(string localFileName, string remoteFileName, string url)
        {
            this.LocalFileName = localFileName;
            this.RemoteFileName = remoteFileName;
            this.Url = url;
        }

        [DataMember(Name = "localFilename", Order = 0, IsRequired = false)]
        public string LocalFileName;
        [DataMember(Name = "remoteFilename", Order = 1, IsRequired = false)]
        public string RemoteFileName;
        [DataMember(Name = "url", Order = 2, IsRequired = false)]
        public string Url;
    }
}
