import React from 'react'

const CheckBoxInput = (props) => (
    <div className="form-group">
        <label className="custom-control custom-checkbox">
            <input id={props.id} defaultChecked={props.defaultChecked} type="checkbox" className="custom-control-input" onChange={props.onChange} />
            <span className="custom-control-label">{props.text}</span>
        </label>
    </div>
);

export default CheckBoxInput;