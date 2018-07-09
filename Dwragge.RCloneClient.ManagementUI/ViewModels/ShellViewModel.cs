using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using Caliburn.Micro;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.ManagementUI.ServiceClient;

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
        

        public IEnumerable<string> FolderNames
        {
            get => _folderNames;
            set
            {
                _folderNames = value;
                NotifyOfPropertyChange(() => FolderNames);
            }
        }

        public ShellViewModel(IWindowManager windowManager, IMapper mapper)
        {
            _windowManager = windowManager;
            _mapper = mapper;
            Task.Run(() => RetryConnectToService());
        }

        public void FireTest()
        {
            var folder = new BackupFolderInfo(@"M:\cbr", "azure", "backup");
            var dto = _mapper.Map<BackupFolderDto>(folder);
            _client.CreateTask(dto);
        }

        public async void RetryConnectToService()
        {
            _client = new RCloneManagementServiceClient();
            bool connected = false;
            try
            {
                connected = await _client.HeartbeatAsync();
            }
            catch
            {
            }

            CantConnectGridVisible = !connected;
        }
    }
}
