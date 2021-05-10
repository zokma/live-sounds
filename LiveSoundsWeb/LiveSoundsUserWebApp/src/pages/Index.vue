<template>
  <q-page class="flex flex-center">
    <div v-if="this.isLoadSucceeded">
      succeeded
    </div>
    <div v-else-if="this.isLoadFailed">
      failed
    </div>
  </q-page>
</template>

<script>
import axios from 'app/node_modules/axios';
import Settings from '../constants/settings';
import { QSpinnerAudio, QSpinnerBars, QSpinnerBall, QSpinnerBox, QSpinnerClock, QSpinnerCube, QSpinnerDots, QSpinnerFacebook, QSpinnerGrid, QSpinnerHearts, QSpinnerHourglass, QSpinnerIos} from 'quasar';

const SPINNERS_LOADING = [
  QSpinnerBall,
  QSpinnerBox,
  QSpinnerCube,
  QSpinnerDots,
  QSpinnerFacebook,
  QSpinnerGrid,
  QSpinnerHearts,
  QSpinnerHourglass,
  QSpinnerIos,
];

function getRandomInt(max) {
  return Math.floor(Math.random() * max);
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function sendApiRequest(url, method, headers, data, successStatus) {
  return axios.request({
    url:              url,
    method:           method,
    headers:          headers,
    data:             data,
    timeout:          Settings.AXIOS_TIMEOUT_MS,
    maxContentLength: Settings.AXIOS_MAX_CONTENT_LENGTH,
    maxBodyLength:    Settings.AXIOS_MAX_BODY_LENGTH,
    validateStatus:   (status) => (status == successStatus),
    maxRedirects:     0,
  });
}

function sendApiGet(url, headers, successStatus) {
  return sendApiRequest(url, 'get', headers, undefined, successStatus);
}

export default {
  name: 'PageIndex',
  created() {
    this.init();
  },
  data () {
    return {
      isLoadSucceeded:   false,
      isLoadFailed:      false,
      resouceId:         null,
      resouces:          null,
      isLiveEmbedded:    false,
      isStreamEmbedded:  false,
      isCommentEmbedded: false,
      embedStream:       null,
      embedComment:      null,
    }
  },
  methods: {
    async init() {

      this.$q.loading.show({
        spinner: SPINNERS_LOADING[getRandomInt(SPINNERS_LOADING.length)],
        spinnerColor: 'white',
        message: this.$t('appLoading')
      });

      const offset = Date.now();

      const params = new URLSearchParams(window.location.search);

      const id = params.get('id');

      let isSucceeded = false;

      if(id) {
        try {
          const res = await sendApiGet(
            Settings.API_GET_SOUND + '/' + encodeURIComponent(id),
            {'Accept': 'application/json'},
            200
          );
          
          this.resouceId = res.data.id;
          this.resouces  = res.data.items;

          // For now, embedded "YouTube" is tested.
          // Twitch to be added, shortly.
          if(res.data.channelType === 'YouTube') {
            if(res.data.channelId) {
              this.embedStream = 'https://www.youtube.com/embed/live_stream?channel=' + encodeURIComponent(res.data.channelId) + '&autoplay=1&mute=1';
              
              this.isStreamEmbedded = true;
            }

            if(res.data.streamId) {
              this.embedComment = 'https://www.youtube.com/live_chat?v=' + encodeURIComponent(res.data.streamId) + '&embed_domain=' + encodeURIComponent(window.location.hostname);

              this.isCommentEmbedded = true;
            }
          }

          this.isLiveEmbedded = (this.isStreamEmbedded || this.isCommentEmbedded);

          isSucceeded = true;

        } catch (error) {
          console.error(error);
        }
      }

      const diff = Settings.APP_LOADING_MIN_MS - (Date.now() - offset);

      if(diff > 0) {
        await sleep(diff);
      }

      this.isLoadSucceeded = isSucceeded;
      this.isLoadFailed    = !isSucceeded;

      this.$q.loading.hide();
    },
  }
}
</script>
