import React from 'react'
import { withRouter, Link } from 'react-router-dom'

const GoBackLink = withRouter(props => {
    return <Link to='#' onClick={() => props.history.goBack()}>Back </Link>
});

export default GoBackLink;