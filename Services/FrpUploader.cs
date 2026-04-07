using Renci.SshNet;

namespace OrdersDownloader.Services
{
    public class FtpUploader
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _remotePath;

        public FtpUploader(string host, int port, string username, string password, string remotePath)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _remotePath = remotePath;
        }

        public void Upload(string filePath)
        {
            using var client = new SftpClient(_host, _port, _username, _password);

            client.Connect();

            using var fileStream = File.OpenRead(filePath);
            client.UploadFile(fileStream, _remotePath, true);

            client.Disconnect();

            Console.WriteLine("✅ SFTP upload successful");
        }
    }
}