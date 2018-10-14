import React, { Component } from 'react'
import { Skeleton } from 'antd';
import filesize from 'filesize';

class FolderInfo extends Component {
  constructor(props) {
    super(props)
    this.state = {
      loading: true,
      currentRemote: this.props.currentRemote,
    }
  }
  
  componentDidMount() {
    fetch(`/api/remotes/${this.state.currentRemote.backupRemoteId}/backupfolders/${this.props.match.params.folderId}/info`)
      .then(res => res.json())
      .then(json => this.setState({files: json, loading: false}))
  }

  render() {
    let tableItems = [...Array(5).keys()].map(i => (
      <tr key={i}>
        <td><Skeleton active paragraph={false}/></td>
        <td><Skeleton active paragraph={false}/></td>
        <td><Skeleton active paragraph={false}/></td>
      </tr>
    ))
    if (!this.state.loading) {
      tableItems = this.state.files.map(f => (
        <tr key={f.trackedFileId}>
          <td>{f.fileName}</td>
          <td>{filesize(f.size)}</td>
          <td>{f.versionCount}</td>
        </tr>
      ))
    }

    return (
      <div className="container">
        <h1>Folder Info</h1>
        <div className="table-responsive" style={{ minHeight: '300px' }}>
          <table className="table table-hover table-outline table-vcenter text-nowrap card-table">
            <thead>
              <tr>
                <th>Path</th>
                <th>Size</th>
                <th>Number Versions</th>
                <th className="text-center"><i className="icon-settings"></i></th>
              </tr>
            </thead>
            <tbody>
              {tableItems}
            </tbody>
          </table>
        </div>
      </div>
    )
  }
}

export default FolderInfo;