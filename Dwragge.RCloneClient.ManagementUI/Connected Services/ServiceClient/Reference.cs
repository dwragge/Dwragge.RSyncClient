﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dwragge.RCloneClient.ManagementUI.ServiceClient {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BackupFolderDto", Namespace="http://schemas.datacontract.org/2004/07/Dwragge.RCloneClient.Persistence")]
    [System.SerializableAttribute()]
    public partial class BackupFolderDto : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int BackupFolderIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<System.DateTime> LastSyncField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PathField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool RealTimeUpdatesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string RemoteBaseFolderField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string RemoteNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int SyncTimeHourField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int SyncTimeMinuteField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.TimeSpan SyncTimeSpanField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int BackupFolderId {
            get {
                return this.BackupFolderIdField;
            }
            set {
                if ((this.BackupFolderIdField.Equals(value) != true)) {
                    this.BackupFolderIdField = value;
                    this.RaisePropertyChanged("BackupFolderId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<System.DateTime> LastSync {
            get {
                return this.LastSyncField;
            }
            set {
                if ((this.LastSyncField.Equals(value) != true)) {
                    this.LastSyncField = value;
                    this.RaisePropertyChanged("LastSync");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Path {
            get {
                return this.PathField;
            }
            set {
                if ((object.ReferenceEquals(this.PathField, value) != true)) {
                    this.PathField = value;
                    this.RaisePropertyChanged("Path");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool RealTimeUpdates {
            get {
                return this.RealTimeUpdatesField;
            }
            set {
                if ((this.RealTimeUpdatesField.Equals(value) != true)) {
                    this.RealTimeUpdatesField = value;
                    this.RaisePropertyChanged("RealTimeUpdates");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RemoteBaseFolder {
            get {
                return this.RemoteBaseFolderField;
            }
            set {
                if ((object.ReferenceEquals(this.RemoteBaseFolderField, value) != true)) {
                    this.RemoteBaseFolderField = value;
                    this.RaisePropertyChanged("RemoteBaseFolder");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string RemoteName {
            get {
                return this.RemoteNameField;
            }
            set {
                if ((object.ReferenceEquals(this.RemoteNameField, value) != true)) {
                    this.RemoteNameField = value;
                    this.RaisePropertyChanged("RemoteName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int SyncTimeHour {
            get {
                return this.SyncTimeHourField;
            }
            set {
                if ((this.SyncTimeHourField.Equals(value) != true)) {
                    this.SyncTimeHourField = value;
                    this.RaisePropertyChanged("SyncTimeHour");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int SyncTimeMinute {
            get {
                return this.SyncTimeMinuteField;
            }
            set {
                if ((this.SyncTimeMinuteField.Equals(value) != true)) {
                    this.SyncTimeMinuteField = value;
                    this.RaisePropertyChanged("SyncTimeMinute");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.TimeSpan SyncTimeSpan {
            get {
                return this.SyncTimeSpanField;
            }
            set {
                if ((this.SyncTimeSpanField.Equals(value) != true)) {
                    this.SyncTimeSpanField = value;
                    this.RaisePropertyChanged("SyncTimeSpan");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceClient.IRCloneManagementService")]
    public interface IRCloneManagementService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRCloneManagementService/Heartbeat", ReplyAction="http://tempuri.org/IRCloneManagementService/HeartbeatResponse")]
        bool Heartbeat();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRCloneManagementService/Heartbeat", ReplyAction="http://tempuri.org/IRCloneManagementService/HeartbeatResponse")]
        System.Threading.Tasks.Task<bool> HeartbeatAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRCloneManagementService/CreateTask", ReplyAction="http://tempuri.org/IRCloneManagementService/CreateTaskResponse")]
        void CreateTask(Dwragge.RCloneClient.ManagementUI.ServiceClient.BackupFolderDto info);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRCloneManagementService/CreateTask", ReplyAction="http://tempuri.org/IRCloneManagementService/CreateTaskResponse")]
        System.Threading.Tasks.Task CreateTaskAsync(Dwragge.RCloneClient.ManagementUI.ServiceClient.BackupFolderDto info);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IRCloneManagementServiceChannel : Dwragge.RCloneClient.ManagementUI.ServiceClient.IRCloneManagementService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RCloneManagementServiceClient : System.ServiceModel.ClientBase<Dwragge.RCloneClient.ManagementUI.ServiceClient.IRCloneManagementService>, Dwragge.RCloneClient.ManagementUI.ServiceClient.IRCloneManagementService {
        
        public RCloneManagementServiceClient() {
        }
        
        public RCloneManagementServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public RCloneManagementServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RCloneManagementServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RCloneManagementServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool Heartbeat() {
            return base.Channel.Heartbeat();
        }
        
        public System.Threading.Tasks.Task<bool> HeartbeatAsync() {
            return base.Channel.HeartbeatAsync();
        }
        
        public void CreateTask(Dwragge.RCloneClient.ManagementUI.ServiceClient.BackupFolderDto info) {
            base.Channel.CreateTask(info);
        }
        
        public System.Threading.Tasks.Task CreateTaskAsync(Dwragge.RCloneClient.ManagementUI.ServiceClient.BackupFolderDto info) {
            return base.Channel.CreateTaskAsync(info);
        }
    }
}
