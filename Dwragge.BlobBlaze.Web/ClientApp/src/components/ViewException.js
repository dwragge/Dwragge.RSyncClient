import React from 'react';
import {Redirect} from 'react-router-dom';
import GoBackLink from './GoBackLink';

const ViewException = (props) => {
  if (!props.location.state.exceptionHtml) {
    return <Redirect to="/" />
  }
  return (
    <div>
      <div>
        <GoBackLink />
      </div>
      <iframe style={{width: '100%', height: '-webkit-fill-available'}} src={'data:text/html;charset=utf-8,' + encodeURI(props.location.state.exceptionHtml)} />
    </div>
  )
}

export default ViewException;