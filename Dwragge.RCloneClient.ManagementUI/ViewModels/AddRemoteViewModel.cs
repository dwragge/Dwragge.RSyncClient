using System.Text.RegularExpressions;
using Caliburn.Micro;
using Dwragge.RCloneClient.ManagementUI.ServiceClient;

namespace Dwragge.RCloneClient.ManagementUI.ViewModels
{
    public class AddRemoteViewModel : Screen
    {
        private string _accessKey;
        private string _accountName;
        private string _connectionString;
        private string _remoteName;
        private string _actionString;
        private string _baseFolder;

        public bool OkClicked { get; set; }

        public AddRemoteViewModel(RemoteDto dto)
        {
            RemoteName = dto.Name;
            ConnectionString = dto.ConnectionString;
            RemoteId = dto.RemoteId;

            if (ConnectionString != "dev")
            {
                const string regex = @"AccountName=([^;]+);AccountKey=([^;]+);";
                var match = Regex.Match(ConnectionString, regex);
                AccountName = match.Groups[1].Value;
                AccessKey = match.Groups[2].Value;
            }

            ActionString = "Save";
        }

        public AddRemoteViewModel()
        {
            ActionString = "Add";
        }

        public int RemoteId { get; set; } = 0;

        public string AccessKey
        {
            get => _accessKey;
            set
            {
                if (value == _accessKey) return;
                _accessKey = value;
                NotifyOfPropertyChange(() => AccessKey);
                RecalculateConnectionString();
            }
        }

        public string BaseFolder
        {
            get => string.IsNullOrEmpty(_baseFolder) ? "" : _baseFolder;
            set
            {
                if (value == _baseFolder) return;
                _baseFolder = value;
                NotifyOfPropertyChange(() => BaseFolder);
            }
        }

        public string AccountName
        {
            get => _accountName;
            set
            {
                if (value == _accountName) return;
                _accountName = value;
                NotifyOfPropertyChange(() => AccountName);
                RecalculateConnectionString();
            }
        }


        public string RemoteName
        {
            get => _remoteName;
            set
            {
                if (value == _remoteName) return;
                _remoteName = value;
                NotifyOfPropertyChange(() => RemoteName);
            }
        }

        public string ActionString
        {
            get => _actionString;
            set
            {
                if (value == _actionString) return;
                _actionString = value;
                NotifyOfPropertyChange(() => ActionString);
            }
        }

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (value == _connectionString) return;
                _connectionString = value;
                NotifyOfPropertyChange(() => ConnectionString);
            }
        }

        public void RecalculateConnectionString()
        {
            if (string.IsNullOrEmpty(AccountName) || string.IsNullOrEmpty(AccessKey))
            {
                ConnectionString = "";
                return;
            }

            var baseString =
                "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net";
            var formatted = string.Format(baseString, AccountName, AccessKey);
            ConnectionString = formatted;
        }

        public void AddButton()
        {
            OkClicked = true;
            TryClose(true);
        }

        public void CancelButton()
        {
            OkClicked = false;
            TryClose(false);
        }
    }
}
