import React, { Component } from 'react'
import { withRouter, Link } from 'react-router-dom'

import './RemoteSwitcherModal.css'

class RemoteSwitcherModal extends Component {
    constructor(props) {
        super(props)
        this.state = {
            loading: true,
            remotes: [],
            selectedRemoteId: this.props.currentRemote.backupRemoteId,
            startingRemoteId: this.props.currentRemote.backupRemoteId
        }

        this.setSelectedRemote = this.setSelectedRemote.bind(this)
        this.changeRemote = this.changeRemote.bind(this)
        this.createNewRemote = this.createNewRemote.bind(this)
    }

    componentDidMount() {
        fetch('api/remotes').then(res => res.json()).then(json => {
            this.setState({
                remotes: json,
                loading: false,
            })
        })
    }

    setSelectedRemote(id, e) {
        this.setState({
            selectedRemoteId: id
        })
    }

    changeRemote() {
        let url = this.state.remotes.filter(r => r.backupRemoteId === this.state.selectedRemoteId)[0].urlName;
        this.props.history.push(`/${url}/`)
    }

    createNewRemote() {
        this.props.history.push('/createnewremote')
    }

    render() {
        let modalBody = "";
        if (this.state.loading) {
            modalBody = <div className="loader" />
        }
        else {
            modalBody = (
                <div className="table-responsive" id="remotes-table">
                    <table className="table table-hover table-outline table-vcenter text-nowrap card-table">
                        <thead>
                            <tr>
                                <th className="text-center w-1" />
                                <th>Remote</th>
                            </tr>
                        </thead>
                        <tbody>
                            {this.state.remotes.map(r => {
                                return (
                                    <tr key={r.backupRemoteId} onClick={(e) => this.setSelectedRemote(r.backupRemoteId, e)}>
                                        <td> {r.backupRemoteId === this.state.selectedRemoteId ? <i className="fe fe-check" /> : ''} </td>
                                        <td> <div> {r.name} </div> </td>
                                    </tr>
                                )
                            })}
                        </tbody>
                    </table>
                </div>
            )
        }
        return (
            <div className="modal fade" id="exampleModal" tabIndex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title" id="exampleModalLabel">Change Remote</h5>
                            <button id="closeButton" type="button" className="close" data-dismiss="modal" aria-label="Close">
                            </button>
                        </div>
                        <div className="modal-body">
                            <div className="card">
                                {modalBody}
                            </div>
                             <button type="button" className="btn btn-primary" onClick={this.createNewRemote} data-dismiss="modal" >Create New Remote </button>
                        </div>
                        <div className="modal-footer">
                            <button type="button" className="btn btn-secondary" data-dismiss="modal">Cancel</button>
                            <button type="button" className="btn btn-primary" 
                                onClick={this.changeRemote} disabled={this.state.selectedRemoteId === this.state.startingRemoteId}>
                                Change Remote
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}

export default withRouter(RemoteSwitcherModal);