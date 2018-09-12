import React from 'react'

const TextInput = (props) => {
    let inputClass = "form-control"

    let idPascal = props.id.charAt(0).toUpperCase() + props.id.substr(1)
    let errors = props.errors[idPascal]
    let errorItems = ''
    if (errors) {
        inputClass += " is-invalid"
        errorItems = errors.map((str, index) => <div key={index} className="invalid-feedback">{str}</div>)
    }

    return (
        <div className="form-group">
            <label className="form-label">{props.text}</label>
            <input defaultValue={props.default} type="text" className={inputClass} id={props.id} onChange={props.onChange} placeholder={props.placeholder} />
            {errorItems}
        </div>
    )
};

export default TextInput;