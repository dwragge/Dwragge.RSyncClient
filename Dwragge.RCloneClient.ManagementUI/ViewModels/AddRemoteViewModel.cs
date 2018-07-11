using Caliburn.Micro;

namespace Dwragge.RCloneClient.ManagementUI.ViewModels
{
    public class AddRemoteViewModel : Screen
    {
        private string _accessKey;
        private string _accountName;
        private string _connectionString;
        private string _remoteName;

        public bool OkClicked { get; set; } = false;

        public string AccessKey
        {
            get => _accessKey;
            set
            {
                if (value == _accessKey) return;
                _accessKey = value;
                NotifyOfPropertyChange(() => AccessKey);
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
