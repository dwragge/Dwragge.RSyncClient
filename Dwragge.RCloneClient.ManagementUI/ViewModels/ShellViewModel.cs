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
        private readonly RCloneManagementServiceClient _client = new RCloneManagementServiceClient();
        private readonly IWindowManager _windowManager;
        private readonly IMapper _mapper;
        private bool _loadingIsVisible = true;
        private IEnumerable<string> _remotes;
        private string _selectedRemote;
        private IEnumerable<string> _folderNames;

        public bool LoadingIsVisible
        {
            get => _loadingIsVisible;
            set
            {
                _loadingIsVisible = value;
                NotifyOfPropertyChange(() => LoadingIsVisible);
            }
        }

        public IEnumerable<string> RemoteName
        {
            get => _remotes;
            set
            {
                _remotes = value;
                NotifyOfPropertyChange(() => RemoteName);
            }
        }

        public string SelectedRemote
        {
            get => _selectedRemote;
            set
            {
                _selectedRemote = value;
                NotifyOfPropertyChange(() => SelectedRemote);
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
            //Task.Run(() => Init());
        }

        public void FireTest()
        {
            var folder = new BackupFolderInfo(@"M:\cbr", "azure", "backup");
            var dto = _mapper.Map<BackupFolderDto>(folder);
            _client.CreateTask(dto);
        }

        private void Init()
        {
            try
            {
                _client.HelloWorld();
                LoadingIsVisible = false;
            }
            catch (Exception)
            {
                LoadingIsVisible = true;
                return;
            }

            RemoteName = _client.GetRemotes();
        }
    }
}
