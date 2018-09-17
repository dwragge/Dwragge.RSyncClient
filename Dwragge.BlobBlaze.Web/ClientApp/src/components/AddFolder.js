import React, {Component} from 'react';
import { Redirect, Link } from 'react-router-dom';
import CenteredForm from './CenteredForm';
import TextInput from './TextInput';
import moment from 'moment';
import {TimePicker, AutoComplete} from 'antd';
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
            currentRemote: this.props.currentRemote,
            folderAutoCompleteData: []
        }

        this.timeChange = this.timeChange.bind(this)
        this.handleChange = this.handleChange.bind(this)
        this.submitForm = this.submitForm.bind(this)
        this.onAutocompleteChange = this.onAutocompleteChange.bind(this)
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
                console.log('response then')
                if (response.status === 400) {
                    response.json().then(errs => this.setState({ errors: errs }))
                }
                else if (response.ok) {
                    response.json().then(r => this.setState({ success: true }))
                }
                else {
                    this.setState({globalErr: response.statusText})
                    response.text().then(t => this.setState({exceptionHtml: t, exception: true})).catch()
                }
            })
            .catch(err => this.setState({globalErr: err}))
    }

    componentWillUnmount() {
        this.setState({
                exception: false
            }
        )
    }

    onAutocompleteChange(value) {
        this.setState({path: value});
        fetch(`/api/filesystem/query/${encodeURIComponent(value)}`).then(res => res.json()).then(data => this.setState({folderAutoCompleteData: data}));
    }

    render() {
        if (this.state.success) {
            return <Redirect to={`/${this.state.currentRemote.urlName}/folders`} />
        }
        let err = this.state.globalErr || '';
        if (this.state.exception) {
            err = <Link to={{pathname: '/viewexception', state: {exceptionHtml: this.state.exceptionHtml}}}>{this.state.globalErr}</Link>
        }
        return (
        <CenteredForm title="Add Folder">
            <FormInput label='Local Folder' id='path' errors={this.state.errors}>
                <AutoComplete dataSource={this.state.folderAutoCompleteData} onChange={this.onAutocompleteChange} placeholder="C:\Foo\Important"/>
            </FormInput>
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