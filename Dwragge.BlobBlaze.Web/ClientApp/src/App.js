import React, { Component } from 'react';
import { Route } from 'react-router';
import jQuery from 'jquery/dist/jquery.slim';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import NotFound from './components/404';

import 'bootstrap/dist/js/bootstrap';
import CreateNewRemote from './components/CreateNewRemote';


class App extends Component {
  constructor(props) {
    super(props);
    this.state = {
      loading: true,
      valid: false,
      currentRemote: null,
    };
  }

  componentDidMount() {
    jQuery.noConflict(true);
    if (this.props.location.state !== undefined && this.props.location.state.redirected === true) {
      this.setState({
        loading: false,
        valid: true,
        currentRemote: this.props.location.state.currentRemote,
      });
    } else {
      this.setCurrentRemote();
    }
  }

  setCurrentRemote() {
    fetch('api/remotes').then(res => res.json()).then((json) => {
      const url = this.props.match.params.remoteId.toLowerCase();
      const index = json.findIndex(r => r.urlName === url);
      const currentRemote = index === -1 ? {} : json[index];

      this.setState({
        valid: index !== -1,
        currentRemote,
        loading: false,
      });
    });
    jQuery('#closeButton').click();
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevState.currentRemote === null || prevState.loading === true) {
      return;
    }
    if (prevState.currentRemote.urlName !== this.props.match.params.remoteId) {
      this.setCurrentRemote();
    }
  }

  render() {
    const match = this.props.match;
    if (this.state.loading) {
      return <div className="loader" />;
    }
    if (!this.state.valid) {
      return <NotFound />;
    }
    return (
      // show loader, load valid remotes and then check and return 404 if not found
      <Layout currentRemote={this.state.currentRemote}>
        <Route exact path={`${match.path}/`} component={Home} />
        <Route path={`${match.path}/counter`} component={Counter} />
        <Route path={`${match.path}/edit`} render={props => <CreateNewRemote currentRemote={this.state.currentRemote} /> } />
        <Route path={`${match.path}/fetchdata`} component={FetchData} />
      </Layout>
    );
  }
}


export default App;
