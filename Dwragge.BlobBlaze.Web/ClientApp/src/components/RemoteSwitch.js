import React, { Component } from 'react';

export class RemoteSwitch extends Component {
    render() {
        return(
            <li className="nav-item ml-auto dropdown"> 
                <a className="nav-link account-switch" href="javascript:void(0)" data-toggle="dropdown" aria-expanded="false"> 
                    Current Remote: {this.props.currentRemoteName} 
                </a>
                <div className="dropdown-menu dropdown-menu-arrow" x-placement="bottom-start">
                    <a className="dropdown-item" href="javascript:void(0)" data-toggle="modal" data-target="#exampleModal"> Change Remote </a>
                </div>
            </li>
        )
    }
}