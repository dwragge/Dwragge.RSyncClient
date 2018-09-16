import React, { Component } from 'react';
import { LinkContainer } from 'react-router-bootstrap';

import './NavMenu.css';
import { RemoteSwitch } from './RemoteSwitch';

export class NavMenu extends Component {
  displayName = NavMenu.name

  render() {
    return (
      <div id="headerMenuCollapse" className="header collapse d-lg-flex p-0">
        <div className="container">
          <div className="row align-items-center">
            <div className="col-lg order-lg-first">
              <ul className="nav nav-tabs border-0 flex-column flex-lg-row">
                <li className="nav-item"> <LinkContainer to={`/${this.props.currentRemote.urlName}/`} exact><a className="nav-link">Home</a></LinkContainer> </li>
                <li className="nav-item"> <LinkContainer to={`/${this.props.currentRemote.urlName}/folders`}><a className="nav-link">Folders</a></LinkContainer> </li>
                <li className="nav-item"> <LinkContainer to={`/${this.props.currentRemote.urlName}/counter`}><a className="nav-link">Counter</a></LinkContainer> </li>
                <li className="nav-item"> <LinkContainer to={`/${this.props.currentRemote.urlName}/fetchData`}><a className="nav-link">Weather</a></LinkContainer> </li>  
                <RemoteSwitch currentRemote={this.props.currentRemote}/>          
              </ul>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
