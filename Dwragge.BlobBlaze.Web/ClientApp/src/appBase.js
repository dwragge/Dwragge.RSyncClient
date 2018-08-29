import React, { Component } from 'react';
import { Route, Switch } from 'react-router';
import {RedirectToDefault } from './RedirectToDefault'
import CreateNewRemote from './components/CreateNewRemote'
import App from './App'

export default class AppBase extends Component {
  displayName = App.name

  render() {
    return (
        <Switch>
            <Route path='/createnewremote' exact component={CreateNewRemote} />           
            <Route path='/:remoteId' component={App}/>
            <Route path='/' exact component={RedirectToDefault} />
        </Switch>
    );
  }
}
