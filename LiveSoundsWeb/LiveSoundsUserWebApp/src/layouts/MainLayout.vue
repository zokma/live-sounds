<template>
  <q-layout view="hHh lpR fFf">

    <q-header reveal elevated class="bg-primary text-white">
      <q-btn
        flat
        dense
        round
        icon="speaker"
      />

      {{$t('appTitle')}}
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>

    <q-footer reveal elevated class="bg-grey-8 text-white">
      <div class="q-pa-sm q-gutter-sm text-right">
        <q-btn type="a" :href="$t('zokmaNetLink')"      target="_blank" :label="$t('zokmaNetInfo')"  icon-right="open_in_new" color="indigo" size="sm"/>
        <q-btn type="a" :href="$t('termsOfUseLink')"    target="_blank" :label="$t('termsOfUse')"    icon-right="open_in_new" color="purple" size="sm"/>
        <q-btn type="a" :href="$t('privacyPolicyLink')" target="_blank" :label="$t('privacyPolicy')" icon-right="open_in_new" color="pink"   size="sm"/>
      </div>
      <div class="q-pa-sm q-gutter-md text-right text-caption">
        {{this.developingYear}} {{$t('developedByZokmaLabs')}}
      </div>
    </q-footer>

  </q-layout>
</template>

<script>

const DEVELOPED_YEAR = 2021;

export default {
  created () {

    this.$i18n.locale = this.$q.lang.getLocale();

    document.title = this.$t('appTitle');

    const year = (new Date()).getFullYear();

    if(year > DEVELOPED_YEAR) {
      this.developingYear = DEVELOPED_YEAR.toString() + ' - ' + year;
    }
    else {
      this.developingYear = DEVELOPED_YEAR.toString();
    }

  },
  data () {
    return {
      developingYear: '',
    }
  }
}
</script>