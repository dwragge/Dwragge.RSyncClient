import React, { Component } from 'react';
import { NavMenu } from './NavMenu';

import 'tabler-ui/dist/assets/css/tabler.css';
import RemoteSwitcherModal from './RemoteSwitcherModal';

export class Layout extends Component {
  displayName = Layout.name

  render() {
    return (
      <div className="page">
        <div className="page-main">
          <NavMenu currentRemote={this.props.currentRemote}/>
          <RemoteSwitcherModal currentRemote={this.props.currentRemote} key={this.props.currentRemote.id}/>
          <div className="my-3 my-md-5">
          <div className="container">
            {this.props.children}
          </div>
        </div>
        </div>
      </div>
    );
  }
}
