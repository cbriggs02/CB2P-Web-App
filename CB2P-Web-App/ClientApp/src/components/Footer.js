import React, { Component } from 'react';

export class Footer extends Component {
    render() {
        const footerStyles = {
            backgroundColor: '#6A5ACD',
            color: 'white',
            position: 'fixed',
            bottom: '0',
            width: '100%',
            padding: '15px 0',
        };

        return (
            <footer style={footerStyles}>
                <div className="container text-center">
                    <span>@2023 - ASP.NET Core React.js Web Application</span>
                </div>
            </footer>
        );
    }
}