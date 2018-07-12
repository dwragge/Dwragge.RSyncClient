using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Caliburn.Micro;
using Dwragge.RCloneClient.ManagementUI.ServiceClient;
using RemoteDto = Dwragge.RCloneClient.ManagementUI.ServiceClient.RemoteDto;

namespace Dwragge.RCloneClient.ManagementUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        private RCloneManagementServiceClient _client = new RCloneManagementServiceClient();
        private readonly IWindowManager _windowManager;
        private readonly IMapper _mapper;
        private bool _loadingIsVisible = true;
        private IEnumerable<string> _folderNames;
        private bool _cantConnectGridVisible;
        private BindableCollection<string> _remotesComboBoxValues = new BindableCollection<string>();

        private Dictionary<string, RemoteDto> _remotes = new Dictionary<string, RemoteDto>();
        private string _selectedRemote;
        private bool _operationInProgress;

        public bool CantConnectGridVisible
        {
            get => _cantConnectGridVisible;
            set
            {
                if (value == _cantConnectGridVisible) return;
                _cantConnectGridVisible = value;
                NotifyOfPropertyChange(() => CantConnectGridVisible);
            }
        }

        public bool LoadingIsVisible
        {
            get => _loadingIsVisible;
            set
            {
                _loadingIsVisible = value;
                NotifyOfPropertyChange(() => LoadingIsVisible);
            }
        }

        public string SelectedRemote
        {
            get => _selectedRemote;
            set
            {
                if (value == _selectedRemote) return;
                _selectedRemote = value;
                NotifyOfPropertyChange(() => SelectedRemote);
                NotifyOfPropertyChange(() => CanEditRemote);
                NotifyOfPropertyChange(() => CanDeleteRemote);
                NotifyOfPropertyChange(() => CanAddFolder);
            }
        }

        public bool OperationInProgress
        {
            get => _operationInProgress;
            set
            {
                if (value == _operationInProgress) return;
                _operationInProgress = value;
                NotifyOfPropertyChange(() => OperationInProgress);
            }
        }

        public void AddFolder()
        {
            var vm = new AddFolderViewModel(_remotes[SelectedRemote]);
            _windowManager.ShowDialog(vm);
        }

        public async void AddRemote()
        {
            var vm = new AddRemoteViewModel();
            await RunRemoteDialog(vm);
        }

        public async void EditRemote()
        {
            var selected = _remotes[SelectedRemote];
            var vm = new AddRemoteViewModel(selected);
            await RunRemoteDialog(vm);
        }

        private async Task RunRemoteDialog(AddRemoteViewModel vm)
        {
            var result = _windowManager.ShowDialog(vm);
            if (result != true) return;

            var dto = new RemoteDto
            {
                RemoteId = vm.RemoteId,
                ConnectionString = vm.ConnectionString,
                Name = vm.RemoteName
            };

            OperationInProgress = true;
            _client.AddOrUpdateRemote(dto);
            await InitializeData();
            OperationInProgress = false;
        }

        public async void DeleteRemote()
        {
            OperationInProgress = true;
            var selected = _remotes[SelectedRemote];
            await _client.DeleteRemoteAsync(selected);
            await InitializeData();
            OperationInProgress = false;
        }

        public bool CanEditRemote => !string.IsNullOrEmpty(SelectedRemote);
        public bool CanDeleteRemote => !string.IsNullOrEmpty(SelectedRemote);
        public bool CanAddFolder => !string.IsNullOrEmpty(SelectedRemote);

        public IEnumerable<string> FolderNames
        {
            get => _folderNames;
            set
            {
                _folderNames = value;
                NotifyOfPropertyChange(() => FolderNames);
            }
        }

        public BindableCollection<string> Remotes
        {
            get => _remotesComboBoxValues;
            set
            {
                if (Equals(value, _remotesComboBoxValues)) return;
                _remotesComboBoxValues = value;
                NotifyOfPropertyChange(() => Remotes);
            }
        }

        public ShellViewModel(IWindowManager windowManager, IMapper mapper)
        {
            _windowManager = windowManager;
            _mapper = mapper;
            Task.Run(() => RetryConnectToService());
        }

        public async void RetryConnectToService()
        {
            _client = new RCloneManagementServiceClient();
            bool connected = false;
            try
            {
                OperationInProgress = true;
                connected = await _client.HeartbeatAsync();
                await InitializeData();
                OperationInProgress = false;
            }
            catch
            {
            }

            CantConnectGridVisible = !connected;
        }

        private async Task InitializeData()
        {
            var remotes = await _client.GetRemotesAsync();
            _remotes = remotes.ToDictionary(x => x.Name);
            Remotes = new BindableCollection<string>(remotes.Select(x => x.Name));
        }
    }
}
