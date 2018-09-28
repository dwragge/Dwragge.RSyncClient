import React, { Component } from 'react'
import {Link} from 'react-router-dom'
import { normalizeSlashes } from '../Helpers';
import { Skeleton, Spin } from 'antd';
import filesize from 'filesize';
import SyncNowLink from './SyncNowLink';

class BackupFolder extends Component {
  constructor(props) {
    super(props)
    this.state = {
      currentRemote: this.props.currentRemote,
      folders: [],
      loading: true
    }
  }
  componentDidMount() {
    fetch(`api/remotes/${this.state.currentRemote.backupRemoteId}/backupfolders`).then(res => res.json()).then(json => {
      this.setState({folders: json, loading: false})
    })
  }

  render() {
    const location = normalizeSlashes(this.props.location.pathname)
    let tableItems = [...Array(5).keys()].map(i => (
      <tr key={i}>
        <td><Skeleton active paragraph={false}/></td>
        <td><Skeleton active paragraph={false}/></td>
        <td><Skeleton active paragraph={false}/></td>
        <td><Skeleton active paragraph={false}/></td>
      </tr>
    ))
    if (this.state.loading === false) {
      tableItems = this.state.folders.map(f => (
        <tr key={f.backupFolderId}>
          <td>{f.name}</td>
          <td>{f.size === -1 ? <Spin size="small" /> : filesize(f.size)}</td>
          <td>never</td>
          <td>12/12/39</td>
          <td className="text-center">
            <div className="item-action dropdown">
              <a href="javascript:void(0)" data-toggle="dropdown" className="icon"><i className="fe fe-more-vertical"></i></a>
              <div className="dropdown-menu dropdown-menu-right">
                <Link to={`${location}${f.backupFolderId}/edit`} className="dropdown-item"><i className="dropdown-icon fe fe-edit-2"></i> Edit/Delete </Link>
                <Link to={`/`} className="dropdown-item"><i className="dropdown-icon fe fe-search"></i>View Details </Link>
                <SyncNowLink folderId={f.backupFolderId} remoteId={f.backupRemoteId} />
              </div>
            </div>
          </td>
        </tr>
      )); 
    }
    return (
      <div className="container">
        <div className="row row-cards row-deck">
          <div className="card">
            <div className="card-header">
              <Link to={`${location}add`}> <button className="btn btn-primary">Add New Folder</button></Link>
            </div>
            <div className="card-body">
              <div className="col-12">
                <div className="card">
                  <div className="table-responsive" style={{minHeight: '300px'}}>
                    <table className="table table-hover table-outline table-vcenter text-nowrap card-table">
                      <thead>
                        <tr>
                          <th>Name</th>
                          <th>Size</th>
                          <th>Last Sync</th>
                          <th>Next Sync</th>
                          <th className="text-center"><i className="icon-settings"></i></th>
                        </tr>
                      </thead>
                      <tbody>
                        {tableItems}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    )
  }
}

export default BackupFolder;