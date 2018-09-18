import React from 'react'

const TextInput = (props) => {
    let inputClass = "form-control"

    let idPascal = props.id.charAt(0).toUpperCase() + props.id.substr(1)
    let errorItems = ''
    if (props.errors) {
        const errors = props.errors[idPascal]
        if (errors) {
            inputClass += " is-invalid"
        errorItems = errors.map((str, index) => <div key={index} className="invalid-feedback">{str}</div>)
        }
    }

    return (
        <div className="form-group">
            <label className="form-label">{props.text}</label>
            <input type="text" className={inputClass} {...props} />
            {errorItems}
        </div>
    )
};

export default TextInput;