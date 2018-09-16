import React, { Component } from 'react'
import GoBackLink from './GoBackLink';

const CenteredForm = (props) => {
    let formColStyle = props.isLogin ? 'col-login mx-auto' : 'col-lg-8 col-sm-12 col-xs-12 mx-auto'
    return (
        <div className="page">
            <div className="page-single">
                <div className="container">
                    <div className="row">
                        <div className={formColStyle}>
                            <form className="card">
                                <div className="card-body p-6">
                                    <div className="card-title">{props.title}</div>
                                    {props.children}
                                </div>
                            </form>
                            <div className="text-center text-muted">
                                <GoBackLink />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        )
}

export default CenteredForm