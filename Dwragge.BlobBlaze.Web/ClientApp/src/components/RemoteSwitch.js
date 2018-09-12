import React, { Component } from 'react';
import {Link} from 'react-router-dom'

export class RemoteSwitch extends Component {
    render() {
        return(
            <li className="nav-item ml-auto dropdown"> 
                <a className="nav-link account-switch" href="javascript:void(0)" data-toggle="dropdown" aria-expanded="false"> 
                    Current Remote: {this.props.currentRemote.name} 
                </a>
                <div className="dropdown-menu dropdown-menu-arrow" x-placement="bottom-start">
                    <a className="dropdown-item" href="javascript:void(0)" data-toggle="modal" data-target="#exampleModal"> Change Remote </a>
                    <Link className="dropdown-item" to={`/${this.props.currentRemote.urlName}/edit`}>Edit Remote </Link>
                </div>
            </li>
        )
    }
}