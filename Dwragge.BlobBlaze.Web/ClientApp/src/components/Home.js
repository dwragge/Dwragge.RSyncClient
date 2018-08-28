import React, { Component } from 'react';
import Header from './Header';

export class Home extends Component {
  displayName = Home.name

  render() {
    let remoteName = this.props.match.params.remoteId;

    return (
      <div className="container">
        <Header title={`Dashboard - ${remoteName}`}/>
        <div className="row row-cards">
          <div className="col-6 col-sm-4 col-lg-2">
            <div className="card">
              <div className="card-body p-3 text-center">
                <div className="text-right text-green">
                  &nbsp;
                </div>
                <div className="h1 m-0">43</div>
                <div className="text-muted mb-4">New Tickets</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
