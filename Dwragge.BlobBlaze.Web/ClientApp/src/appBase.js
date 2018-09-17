import React, { Component } from 'react';
import { Route, Switch } from 'react-router';
import {RedirectToDefault } from './RedirectToDefault'
import CreateNewRemote from './components/CreateNewRemote'
import App from './App'
import ViewException from './components/ViewException';

export default class AppBase extends Component {
  displayName = App.name

  render() {
    return (
        <Switch>
            <Route path='/viewexception' component={ViewException} />
            <Route path='/createnewremote' exact component={CreateNewRemote} />           
            <Route path='/:remoteId' component={App}/>
            <Route path='/' exact component={RedirectToDefault} />
        </Switch>
    );
  }
}
