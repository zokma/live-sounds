
// const API_BASE = 'https://api.zokma.net';
const API_BASE = 'http://localhost';

export default Object.freeze({
    API_GET_SOUND:            (API_BASE + '/live/v1/sounds'),
    API_RENDER_AUDIO:         (API_BASE + '/live/v1/audio/renderings'),
    APP_LOADING_MIN_MS:       1400,
    AXIOS_MAX_CONTENT_LENGTH: 10 * 1024 * 1024,
    AXIOS_MAX_BODY_LENGTH:    4 * 1024,
    AXIOS_TIMEOUT_MS:         60 * 1000,
    EMBEDDED_MARGIN:          80,
    EMBEDDED_PROPS:           [
        {
            threshold: 1680,
            stream: {
                width:  1280,
                height: 720,
            },
            comment: {
                width:  400,
                height: 720,
            },
        },
        {
            threshold: 1424,
            stream: {
                width:  1024,
                height: 576,
            },
            comment: {
                width:  400,
                height: 576,
            },
        },
        {
            threshold: 1360,
            stream: {
                width:  960,
                height: 540,
            },
            comment: {
                width:  400,
                height: 540,
            },
        },
        {
            threshold: 1136,
            stream: {
                width:  736,
                height: 414,
            },
            comment: {
                width:  400,
                height: 414,
            },
        },
        {
            threshold: 1040,
            stream: {
                width:  640,
                height: 360,
            },
            comment: {
                width:  400,
                height: 360,
            },
        },
        {
            threshold: 480,
            stream: {
                width:  480,
                height: 270,
            },
            comment: {
                width:  300,
                height: 270,
            },
        },
        {
            threshold: 256,
            stream: {
                width:  256,
                height: 144,
            },
            comment: {
                width:  256,
                height: 270,
            },
        },
    ],
});
