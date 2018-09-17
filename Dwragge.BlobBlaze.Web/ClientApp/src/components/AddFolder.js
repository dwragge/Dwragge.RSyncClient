import React, {Component} from 'react';
import { Redirect } from 'react-router-dom';
import CenteredForm from './CenteredForm';
import TextInput from './TextInput';
import moment from 'moment';
import {TimePicker} from 'antd';
import 'antd/lib/time-picker/style/index.css';
import { postData } from '../Helpers';
import FormInput from './FormInput';

class AddFolder extends Component {
    constructor(props) {
        super(props)
        console.log(props)
        this.state = {
            errors: [],
            time: '02:00:00',
            path: '',
            remoteFolder: '',
            syncTimeSpan: '1:0:0',
            currentRemote: this.props.currentRemote
        }

        this.timeChange = this.timeChange.bind(this)
        this.handleChange = this.handleChange.bind(this)
        this.submitForm = this.submitForm.bind(this)
    }

    timeChange(moment, value) {
        this.setState({time: value})
    }

    handleChange(event) {
        const target = event.target;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.id;
        this.setState({
            [name]: value
        });
    }

    submitForm() {
        const data = {
            'syncTime': this.state.time,
            'path': this.state.path,
            'remoteFolder': this.state.remoteFolder,
            'syncTimeSpan': this.state.syncTimeSpan
        }
        
        postData(`/api/remotes/${this.state.currentRemote.backupRemoteId}/backupFolders`, data)
            .then(response => { 
                if (response.status === 400) {
                    response.json().then(errs => this.setState({ errors: errs }))
                }
                else if (response.status.ok) {
                    response.json().then(r => this.setState({ success: true }))
                }
                else {
                    this.setState({globalErr: response.statusText})
                }
            })
            .catch(err => this.setState({globalErr: err}))
    }

    render() {
        if (this.state.success) {
            return <Redirect to={`/${this.state.currentRemote.urlName}/folders`} />
        }
        let err = this.state.globalErr || '';
        return (
        <CenteredForm title="Add Folder">
            <TextInput id='path' placeholder='C:\Photos\Important' text='Local Folder' onChange={this.handleChange} errors={this.state.errors} />
            <TextInput id='remoteFolder' placeholder='photos/important' text='Remote Base Folder' onChange={this.handleChange} errors={this.state.errors} />
            <TextInput id='syncTimeSpan' placeholder='days:hours:minutes' default="1:0:0" text='Sync Time Span' onChange={this.handleChange} errors={this.state.errors} />
            <FormInput label="Sync Time" id='syncTime' errors={this.state.errors}>
                <TimePicker defaultValue={moment('02:00:00', 'HH:mm:ss')} onChange={this.timeChange} />                
            </FormInput>

            <div className="form-footer">
                <button type='button' className='btn btn-primary btn-block' onClick={this.submitForm}>Add Folder</button>
            </div>
            {err}
        </CenteredForm>
        )
    }
}

export default AddFolder;