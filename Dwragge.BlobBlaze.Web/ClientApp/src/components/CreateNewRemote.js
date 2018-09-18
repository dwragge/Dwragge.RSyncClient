import React, { Component } from 'react';
import GoBackLink from './GoBackLink'
import TextInput from './TextInput';
import CheckBoxInput from './CheckBoxInput';
import { postData, httpDelete } from '../Helpers';
import { Redirect } from 'react-router';
import CenteredForm from './CenteredForm';

class CreateNewRemote extends Component {
    constructor(props) {
        super(props);
        const c = this.props.currentRemote || null
        if (c === null) {
            this.state = {
                name: '',
                connectionString: '',
                default: false,
                baseFolder: '',
                errors: [],
                redirectTo: '',
                submitSuccessful: false,
                isEdit: false
            };
        }
        else {
            this.state = {
                name: c.name,
                connectionString: c.connectionString,
                default: c.default,
                baseFolder: c.baseFolder,
                errors: [],
                redirectTo: '',
                submitSuccessful: false,
                isEdit: true,
                currentRemote: c
            };
        }


        this.handleChange = this.handleChange.bind(this);
    }

    handleChange(event) {
        const target = event.target;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.id;

        this.setState({
            [name]: value
        });
    }

    submitForm = () => {
        let data = {
            name: this.state.name,
            connectionString: this.state.connectionString,
            default: this.state.default,
            baseFolder: this.state.baseFolder
        }

        let id = '';
        if (this.state.currentRemote) {
            id = this.state.currentRemote.backupRemoteId
        }

        postData(`/api/remotes/${id}`, data)
            .then(response => {
                if (response.status === 400) {
                    response.json().then(errs => this.setState({ errors: errs }))
                }
                else {
                    response.json().then(r => this.setState({ redirectTo: r.urlName, submitSuccessful: true }))
                }
            })
            .catch(err => console.error(err))
    }

    checkDelete = () => {
        httpDelete(`/api/remotes/${this.state.currentRemote.backupRemoteId}`)
            .then(res => this.setState({submitSuccessful: true, redirectTo: ''}))
            .catch(err => console.err(err))            
    }


    render() {
        if (this.state.submitSuccessful === true) {
            return <Redirect to={`/${this.state.redirectTo}`} />
        }

        const DeleteButton = this.state.isEdit ? <button type='button' onClick={this.checkDelete} className="btn btn-danger btn-block">Delete</button> : ""
        return (
            <CenteredForm title={this.state.isEdit ? "Edit Remote" : "Create New Remote"}>
                <div className="card-title"></div>
                <TextInput id='name' default={this.state.name} placeholder='Azure #2' text='Remote Name' onChange={this.handleChange} errors={this.state.errors} />
                <TextInput id='baseFolder' default={this.state.baseFolder} placeholder='/backup/some_folder' text='Base Folder' onChange={this.handleChange} errors={this.state.errors} />
                <TextInput id='connectionString' default={this.state.connectionString} placeholder='AccountName=Example;Key=kjQhnJ==' text='Connection String' onChange={this.handleChange} errors={this.state.errors} />
                <CheckBoxInput id='default' defaultChecked={this.state.default} text="Default Remote" onChange={this.handleChange} />
                <div className="form-footer">
                    {DeleteButton}
                    <button type='button' onClick={this.submitForm} className="btn btn-primary btn-block">{this.state.isEdit ? "Save" : "Create"}</button>
                </div>
            </CenteredForm>
        );
    }
}


export default CreateNewRemote;