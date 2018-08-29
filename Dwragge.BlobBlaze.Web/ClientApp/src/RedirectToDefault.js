import React, { Component } from 'react';
import { Redirect } from 'react-router';

export class RedirectToDefault extends Component {
    constructor(props) {
        super(props)

        this.state = {
            loaded: false,
            defaultRemote: {}
        }
    }

    componentDidMount() {
        fetch('api/remotes').then(res => res.json()).then(json => {
            if (json.length === 0) {
                this.setState({loaded: true});
            }

            let defaults = json.filter(r => r.default === true)
            if (defaults.length === 0) {
                this.setState({
                    loaded: true,
                    defaultRemote: json[0]
                })
            }
            else {
                this.setState({
                    loaded: true,
                    defaultRemote: defaults[0]
                })
            }
        })
    }

    render() {
        if (this.state.loaded === false) {
            return (
                <div className="loader" />
            )
        }
        else {
            if (this.state.defaultRemoteName === undefined) {
                return <Redirect to="/createnewremote" />
            }
            else {
                return <Redirect to={{
                    pathname: `/${this.state.defaultRemote.urlName}/`, 
                    state: {
                        redirected: true, 
                        currentRemote: this.state.defaultRemote
                    }
                }} />
            }
        }
    }
}