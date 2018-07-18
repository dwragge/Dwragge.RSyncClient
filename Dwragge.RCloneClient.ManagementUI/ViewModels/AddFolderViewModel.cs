using System;
using System.IO;
using Caliburn.Micro;
using Dwragge.RCloneClient.ManagementUI.ServiceClient;
using Ookii.Dialogs.Wpf;

namespace Dwragge.RCloneClient.ManagementUI.ViewModels
{
    public class AddFolderViewModel : Screen
    {
        private string _selectedFolder;
        private string _remoteBaseFolder;
        private int _syncInterval = 1;
        private TimeSpan _selectedSyncTime = new TimeSpan(2, 00, 0);
        private string _remoteName;

        public AddFolderViewModel(RemoteDto dto)
        {
            RemoteName = dto.Name;
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

        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (value == _selectedFolder) return;
                _selectedFolder = value;
                NotifyOfPropertyChange(() => SelectedFolder);
            }
        }

        public string RemoteBaseFolder
        {
            get => _remoteBaseFolder;
            set
            {
                if (value == _remoteBaseFolder) return;
                _remoteBaseFolder = value;
                NotifyOfPropertyChange(() => RemoteBaseFolder);
            }
        }

        public int SyncInterval
        {
            get => _syncInterval;
            set
            {
                if (value == _syncInterval) return;
                _syncInterval = value;
                NotifyOfPropertyChange(() => SyncInterval);
            }
        }

        public TimeSpan SelectedSyncTime
        {
            get => _selectedSyncTime;
            set
            {
                if (value.Equals(_selectedSyncTime)) return;
                _selectedSyncTime = value;
                NotifyOfPropertyChange(() => SelectedSyncTime);
            }
        }

        public void BrowseFolder()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Choose Folder for Backup",
                UseDescriptionForTitle = true
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                SelectedFolder = dialog.SelectedPath;
                RemoteBaseFolder = new DirectoryInfo(SelectedFolder).Name;
            }
        }

        public void AddButton()
        {
            TryClose(true);
        }

        public void CancelButton()
        {
            TryClose(false);
        }
    }
}
