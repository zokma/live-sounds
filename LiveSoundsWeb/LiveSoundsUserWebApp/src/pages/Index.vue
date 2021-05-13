<template>
  <q-page class="flex">
    <div v-if="isLoadSucceeded">
      <div class="q-mt-md q-ml-md q-mr-md text-body2 text-weight-bolder" style="color:blue;opacity:.4">
        {{$t('appDescription')}}
      </div>
      <div v-if="canLiveEmbedded" class="q-ma-md">
        <div>
          <q-toggle
            v-if="canStreamEmbedded"
            v-model="isStreamEmbedded"
            icon="live_tv"
            color="pink"
            :label="$t('embeddedLiveStream')"
            class="q-ml-md"
          />
          <q-toggle
            v-if="canCommentEmbedded"
            v-model="isCommentEmbedded"
            icon="comment"
            color="green"
            :label="$t('embeddedLiveComment')"
            class="q-ml-md"
          />
        </div>
        <div class="q-ma-md">
          <iframe v-if="isStreamEmbedded"
            :src="embedStream" 
            :width="embedProp.stream.width" :height="embedProp.stream.height" 
            frameborder="0" 
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
            allowfullscreen></iframe>
          <iframe v-if="isCommentEmbedded" 
            :src="embedComment" 
            :width="embedProp.comment.width" :height="embedProp.comment.height" 
            frameborder="0"
            allowfullscreen></iframe>
        </div>
      </div>
      <div class="q-ma-md">
        <q-knob
          show-value
          :max="100"
          :min="1"
          class="text-white q-ma-md"
          v-model="audioRenderVolume"
          size="94px"
          :thickness="0.2"
          color="purple-4"
          center-color="grey-8"
          track-color="transparent">
          <q-icon name="volume_up" />
          {{audioRenderVolume}}
        </q-knob>
        <div class="q-pa-md q-gutter-md">
          <q-btn v-for="item in resouces" :key="item.id" 
            :label="item.name" 
            @click="audioRenderButtonClicked(item.id)"
            :disable="!audioRenderEnabled"
            icon-right="play_circle_outline"
            align="between"
            color="purple-6"
            class="glossy"
            style="width: 240px" />
        </div>
      </div>
    </div>
    <div v-else-if="isLoadFailed">
      <div class="q-mt-md q-ml-md q-mr-md text-h4 text-weight-bolder" style="opacity:.4">
        {{$t('liveNotFoundTitle')}}
      </div>
      <div class="q-ml-md q-mr-md text-h6 text-weight-bold" style="opacity:.4">
        {{$t('liveNotFoundDescription')}}
      </div>
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

const SPINNERS_AUDIO_RENDERING = [
  QSpinnerAudio,
  QSpinnerBars,
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

function sendApiPost(url, headers, data, successStatus) {
  return sendApiRequest(url, 'post', headers, data, successStatus);
}

export default {
  name: 'PageIndex',
  created() {
    this.init();
  },
  mounted() {
    this.changeEmbeddedProp();
    window.addEventListener('resize', this.changeEmbeddedProp);
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.changeEmbeddedProp);
  },
  data () {
    return {
      isLoadSucceeded:    false,
      isLoadFailed:       false,
      resouceId:          null,
      resouces:           null,
      audioRenderVolume:  100,
      audioRenderEnabled: true,
      canLiveEmbedded:    false,
      canStreamEmbedded:  false,
      canCommentEmbedded: false,
      isStreamEmbedded:   false,
      isCommentEmbedded:  false,
      embedStream:        null,
      embedComment:       null,
      embedProp:          null,
    }
  },
  methods: {
    async init() {

      this.$q.loading.show({
        spinner:      SPINNERS_LOADING[getRandomInt(SPINNERS_LOADING.length)],
        spinnerColor: 'white',
        message:      this.$t('appLoading')
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

            if(res.data.streamingId) {
              this.embedStream  = 'https://www.youtube.com/embed/'       + encodeURIComponent(res.data.streamingId) + '?autoplay=1&mute=1';
              this.embedComment = 'https://www.youtube.com/live_chat?v=' + encodeURIComponent(res.data.streamingId) + '&embed_domain=' + encodeURIComponent(window.location.hostname);

              this.canStreamEmbedded  = true;
              this.canCommentEmbedded = true;
            }
            else if(res.data.channelId) {
              this.embedStream = 'https://www.youtube.com/embed/live_stream?channel=' + encodeURIComponent(res.data.channelId) + '&autoplay=1&mute=1';
              
              this.canStreamEmbedded = true;
            }

          }

          this.canLiveEmbedded = (this.canStreamEmbedded || this.canCommentEmbedded);

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
    changeEmbeddedProp() {

      const width = window.innerWidth - Settings.EMBEDDED_MARGIN;
      const count = Settings.EMBEDDED_PROPS.length;

      let prop = Settings.EMBEDDED_PROPS[count - 1];

      for (let i = 0; i < count; i++) {
        const item = Settings.EMBEDDED_PROPS[i];

        if(width >= item.threshold) {
          prop = item;
          break;
        }
      }

      this.embedProp = prop;
    },
    async audioRenderButtonClicked(id) {

      this.audioRenderEnabled = false;

      const render = {
        audioId: id,
        soundId: this.resouceId,
        volume: this.audioRenderVolume,
      }


      let isAudioRenderBlocked = false;
      let res;

      try {
        res = await sendApiPost(
          Settings.API_RENDER_AUDIO,
          {
            'Content-Type': 'application/json; charset=utf-8',
            'Accept': 'application/json',
          },
          render,
          204
        );

        this.$q.notify({
          spinner: SPINNERS_AUDIO_RENDERING[getRandomInt(SPINNERS_AUDIO_RENDERING.length)],
          message: this.$t('nowAudioRendering'),
          color: 'indigo-9',
          timeout: Settings.NOW_PLAYING_DURATION_MS,
        });

      } catch (error) {
        console.error(error);

        res = error.response;

        if(res && res.status === 429) {
          isAudioRenderBlocked = true;

          let retryAfter = parseInt(res.headers['retry-after'], 10);

          if(retryAfter !== NaN && retryAfter > 0 && retryAfter <= 60) {
            retryAfter *= 1000;
          }
          else {
            retryAfter = Settings.RETRY_AFTER_DEFALT_MS;
          }

          const message = this.$t('audioRenderRetryAfter');
          const prefix  = this.$t('prefixWaitForSeconds');
          const postfix = this.$t('postfixWaitForSeconds');

          const retryNotif = this.$q.notify({
            group: false,
            timeout: 0,
            spinner: QSpinnerClock,
            message: message,
            caption: prefix + (retryAfter / 1000) + postfix,
            color:   'red-13'
          });

          const interval = setInterval(() => {
            retryAfter -= Settings.RETRY_AFTER_NOTIFY_INTERVAL_MS;

            if(retryAfter <= 0) {

              this.audioRenderEnabled = true;

              retryNotif({
                icon: 'done',
                spinner: false,
                message: this.$t('audioRenderRetryAfterDone'),
                caption: prefix + 0 + postfix,
                color: 'blue-14',
                timeout: Settings.RETRY_AFTER_DONE_NOTIFY_MS,
              });

              clearInterval(interval);
            }
            else {
              retryNotif({
                caption: prefix + (retryAfter / 1000) + postfix,
              });
            }
          }, Settings.RETRY_AFTER_NOTIFY_INTERVAL_MS);
        }
        else {
          this.$q.notify({
            icon: 'report_problem',
            message: this.$t('audioRenderFailed'),
            color: 'deep-orange-7',
            timeout: Settings.ERROR_NOTIFY_DURATION_MS,
          });
        }
      }
      finally {
        if(!isAudioRenderBlocked) {
          this.audioRenderEnabled = true;
        }
      }
    },
  }
}
</script>
