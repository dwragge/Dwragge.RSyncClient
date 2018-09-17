import React from 'react';

const FormInput = (props) => {
  let idPascal = props.id.charAt(0).toUpperCase() + props.id.substr(1)
  let errorItems = ''
  if (props.errors) {
      const errors = props.errors[idPascal]
      if (errors) {
        errorItems = errors.map((str, index) => <div key={index} className="invalid-feedback" style={{"display": "inherit"}}>{str}</div>)
      }
  }
  return (
    <div className="form-group">
      <label className="form-label">{props.label}</label>
      {props.children}
      {errorItems}
    </div>
    );
};

export default FormInput;