import React from 'react';
import PropTypes from 'prop-types';

const Header = ({title}) => (
    <div className="page-header">
        <h1 className="page-title">{title}</h1>
    </div>
);

Header.propTypes = {
    title: PropTypes.string.isRequired
}

export default Header;