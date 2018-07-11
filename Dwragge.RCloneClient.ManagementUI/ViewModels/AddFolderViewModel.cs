using System;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;

namespace Dwragge.RCloneClient.ManagementUI.ViewModels
{
    public class AddFolderViewModel : Screen
    {
        private string _selectedFolder;
        private string _remoteBaseFolder;

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
