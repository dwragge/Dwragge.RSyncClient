import React from 'react'
import { message } from 'antd'

const syncFetch = (remoteId, folderId) => {
  fetch(`api/remotes/${remoteId}/backupfolders/${folderId}/sync`)
  .then(res => {
    if (res.ok) {
      message.success("Successfully started sync")
    } else {
      message.error("Error occurred while starting sync")
    }
  })
}

const SyncNowLink = (props) => (
    <a className='dropdown-item' onClick={() => syncFetch(props.remoteId, props.folderId)}><i className="dropdown-icon fe fe-play"/>Sync Now</a>
)

export default SyncNowLink;