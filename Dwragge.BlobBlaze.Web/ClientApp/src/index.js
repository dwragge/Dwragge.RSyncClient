import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import AppBase from './appBase'
import registerServiceWorker from './registerServiceWorker';




const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');



ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <AppBase />
  </BrowserRouter>,
  rootElement);

registerServiceWorker();
