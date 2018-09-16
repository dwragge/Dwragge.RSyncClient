import React, {Component} from 'react'
import CenteredForm from './CenteredForm';
import TextInput from './TextInput';
import moment from 'moment'
import {TimePicker} from 'antd'
import 'antd/lib/time-picker/style/index.css'

class AddFolder extends Component {
    constructor(props) {
        super(props)
        this.state = {
            errors: [],
            time: '02:00:00',
            localFolder: '',
            remoteFolder: '',
            syncTimeSpan: '1,0,0',
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
        console.log(this.state)
    }

    render() {
        return (
        <CenteredForm title="Add Folder">
            <TextInput id='localFolder' placeholder='C:\Photos\Important' text='Local Folder' onChange={this.handleChange} />
            <TextInput id='remoteFolder' placeholder='photos/important' text='Remote Base Folder' onChange={this.handleChange} />
            <TextInput id='syncTimeSpan' placeholder='days,hours,minutes' text='Sync Time Span' onChange={this.handleChange} />
            <div className="form-group">
                <label className="form-label">Sync Time</label>
                <TimePicker defaultValue={moment('02:00:00', 'HH:mm:ss')} onChange={this.timeChange} />
            </div>
            <div className="form-footer">
                <button type='button' className='btn btn-primary btn-block' onClick={this.submitForm}>Add Folder</button>
            </div>
        </CenteredForm>
        )
    }
}

export default AddFolder