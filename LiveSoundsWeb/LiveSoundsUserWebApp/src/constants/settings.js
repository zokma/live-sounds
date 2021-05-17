
// const API_BASE = 'https://api.zokma.net';
const API_BASE = 'http://localhost';

export default Object.freeze({
    API_GET_SOUND:                  (API_BASE + '/live/v1/sounds'),
    API_RENDER_AUDIO:               (API_BASE + '/live/v1/audio/renderings'),
    APP_LOADING_MIN_MS:             1400,
    AXIOS_MAX_CONTENT_LENGTH:       10 * 1024 * 1024,
    AXIOS_MAX_BODY_LENGTH:          4 * 1024,
    AXIOS_TIMEOUT_MS:               60 * 1000,
    RETRY_AFTER_DEFALT_MS:          60 * 1000,
    RETRY_AFTER_NOTIFY_INTERVAL_MS: 1000,
    RETRY_AFTER_DONE_NOTIFY_MS:     4000,
    NOW_PLAYING_DURATION_MS:        10000,
    ERROR_NOTIFY_DURATION_MS:       10000,
    EMBEDDED_MARGIN:                80,
    EMBEDDED_PROPS: 
    [
        {
            threshold: 2560,
            stream: {
                width:  1920,
                height: 1080,
            },
            comment: {
                width:  640,
                height: 1080,
            },
        },
        {
            threshold: 2218,
            stream: {
                width:  1664,
                height: 936,
            },
            comment: {
                width:  554,
                height: 936,
            },
        },
        {
            threshold: 1878,
            stream: {
                width:  1408,
                height: 792,
            },
            comment: {
                width:  470,
                height: 792,
            },
        },
        {
            threshold: 1707,
            stream: {
                width:  1280,
                height: 720,
            },
            comment: {
                width:  427,
                height: 720,
            },
        },
        {
            threshold: 1536,
            stream: {
                width:  1152,
                height: 648,
            },
            comment: {
                width:  384,
                height: 648,
            },
        },
        {
            threshold: 1024,
            stream: {
                width:  768,
                height: 432,
            },
            comment: {
                width:  256,
                height: 432,
            },
        },
        {
            threshold: 640,
            stream: {
                width:  640,
                height: 360,
            },
            comment: {
                width:  256,
                height: 360,
            },
        },
        {
            threshold: 512,
            stream: {
                width:  512,
                height: 288,
            },
            comment: {
                width:  256,
                height: 360,
            },
        },
        {
            threshold: 384,
            stream: {
                width:  384,
                height: 216,
            },
            comment: {
                width:  256,
                height: 360,
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
                height: 360,
            },
        },
    ],
});
