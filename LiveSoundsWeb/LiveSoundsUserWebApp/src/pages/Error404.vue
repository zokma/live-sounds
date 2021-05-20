<template>
  <div class="fullscreen bg-blue text-white text-center q-pa-md flex flex-center">
    <div>
      <div style="font-size: 30vh">
        404
      </div>

      <div class="text-h2" style="opacity:.4">
        {{ $t('notFoundMessage') }}
      </div>

      <q-btn
        class="q-mt-xl q-ma-md"
        color="white"
        text-color="blue"
        unelevated
        to="/"
        :label="$t('goHomeButton')"
        no-caps
      />

      <q-btn
        class="q-mt-xl q-ma-md"
        type="a" 
        :href="$t('notFoundAppLink')" 
        target="_blank"
        color="white"
        text-color="blue"
        icon-right="open_in_new"
        unelevated
        :label="$t('notFoundAppButton')"
        no-caps
      />

    </div>
  </div>
</template>

<script>
export default {
  name: 'Error404',
    created () {

      this.$i18n.locale = this.$q.lang.getLocale();

      document.title = this.$t('appTitle');
  },
}
</script>
