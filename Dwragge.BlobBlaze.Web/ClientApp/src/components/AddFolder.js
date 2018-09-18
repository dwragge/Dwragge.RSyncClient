import React, {Component} from 'react';
import { Redirect, Link } from 'react-router-dom';
import CenteredForm from './CenteredForm';
import TextInput from './TextInput';
import moment from 'moment';
import {TimePicker, AutoComplete, Spin, Popconfirm} from 'antd';
import { postData, httpDelete } from '../Helpers';
import FormInput from './FormInput';

class AddFolder extends Component {
    constructor(props) {
        super(props)
        this.state = {
            errors: [],
            time: '02:00:00',
            path: '',
            name: '',
            remoteFolder: '',
            syncTimeSpan: '1:0:0',
            currentRemote: this.props.currentRemote,
            folderAutoCompleteData: [],
            loading: props.match.params.folderId !== undefined
        }

        this.timeChange = this.timeChange.bind(this)
        this.handleChange = this.handleChange.bind(this)
        this.submitForm = this.submitForm.bind(this)
        this.onAutocompleteChange = this.onAutocompleteChange.bind(this)
    }

    componentDidMount() {
        if (this.state.loading) {
            fetch(`api/remotes/${this.state.currentRemote.backupRemoteId}/backupFolders/${this.props.match.params.folderId}`)
                .then(response => {
                    if (response.ok) {
                        return response.json()
                    }
                    else {
                        response.text().then(t => this.setState({exceptionHtml: t, exception: true, loading: false}))
                    }
                })
                .then(json => this.setState({loading: false, ...json}))
                .catch(err => console.log(err))
        }
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
            'syncTimeSpan': this.state.syncTimeSpan,
            'name': this.state.name
        }
        const id = this.props.match.params.folderId || ''

        postData(`/api/remotes/${this.state.currentRemote.backupRemoteId}/backupFolders/${id}`, data)
            .then(response => { 
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

    deleteFolder = () => {
        httpDelete(`/api/remotes/${this.state.currentRemote.backupRemoteId}/backupFolders/${this.props.match.params.folderId}`)
            .then(response => {
                if (response.ok) {
                    this.setState({success: true})
                }
                else {
                    this.setState({globalErr: response.statusText})
                    response.text().then(t => this.setState({exceptionHtml: t, exception: true})).catch()
                }
            })
    }

    render() {
        if (this.state.success) {
            return <Redirect to={`/${this.state.currentRemote.urlName}/folders`} />
        }

        let err = this.state.globalErr || '';
        if (this.state.exception) {
            err = <Link to={{pathname: '/viewexception', state: {exceptionHtml: this.state.exceptionHtml}}}>View Exception</Link>
        }

        let {name, time, path, remoteFolder, syncTimeSpan} = this.state
        const isEdit = this.props.match.params.folderId !== undefined
        let deleteButton = '';
        if (isEdit) {
            deleteButton = (
                <Popconfirm title='Are you sure you want to delete this folder?' onConfirm={this.deleteFolder} okText='Yes' cancelText='No'>
                    <button type='button' className="btn btn-danger btn-block">Delete</button>
                </Popconfirm>
            )
        }

        const formHtml = (
            <CenteredForm title={this.props.match.params.folderId === undefined ? 'Add Remote' : 'Edit Remote'}>
                <FormInput label='Local Folder' id='path' errors={this.state.errors}>
                    <AutoComplete dataSource={this.state.folderAutoCompleteData} onChange={this.onAutocompleteChange} value={path} backfill={true} disabled={isEdit}/>
                </FormInput>
                <TextInput id='name' placeholder='important' text='Name' value={name} onChange={this.handleChange} errors={this.state.errors} />
                <TextInput id='remoteFolder' placeholder='photos/important' value={remoteFolder} text='Remote Base Folder' onChange={this.handleChange} errors={this.state.errors} />
                <TextInput id='syncTimeSpan' placeholder='days:hours:minutes' default="1:0:0" value={syncTimeSpan} text='Sync Time Span' onChange={this.handleChange} errors={this.state.errors} />
                <FormInput label="Sync Time" id='time' errors={this.state.errors}>
                    <TimePicker value={moment(time, 'HH:mm:ss')} onChange={this.timeChange} />                
                </FormInput>

                <div className="form-footer">
                    {deleteButton}
                    <button type='button' className='btn btn-primary btn-block' onClick={this.submitForm}>Add Folder</button>
                    {err}
                </div>
            </CenteredForm>
        )

        if (this.state.loading) {
            return <Spin tip='Loading...'>
                {formHtml}
            </Spin>
        }
        return formHtml
    }
}

export default AddFolder;