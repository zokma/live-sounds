
// const API_BASE = 'https://api.zokma.net';
const API_BASE = 'http://localhost';

export default Object.freeze({
    API_GET_SOUND:            (API_BASE + '/live/v1/sounds'),
    API_RENDER_AUDIO:         (API_BASE + '/live/v1/audio/renderings'),
    APP_LOADING_MIN_MS:       1400,
    AXIOS_MAX_CONTENT_LENGTH: 10 * 1024 * 1024,
    AXIOS_MAX_BODY_LENGTH:    4 * 1024,
    AXIOS_TIMEOUT_MS:         60 * 1000,
});

