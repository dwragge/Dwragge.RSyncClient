import React, { Component } from 'react'
import {Link} from 'react-router-dom'
import { normalizeSlashes } from '../Helpers';

class BackupFolder extends Component {
  render() {
    const location = normalizeSlashes(this.props.location.pathname)
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
                  <div className="table-responsive">
                    <table className="table table-hover table-outline table-vcenter text-nowrap card-table">
                      <thead>
                        <tr>
                          <th>Path</th>
                          <th>Size</th>
                          <th>Last Sync</th>
                          <th>Next Sync</th>
                          <th>Realtime</th>
                        </tr>
                      </thead>
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