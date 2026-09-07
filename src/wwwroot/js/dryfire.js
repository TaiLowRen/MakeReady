window.MakeReadyDryFire = {

    _ctx: null,

    _getContext: function () {
        if (!this._ctx) {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            this._ctx = new AudioCtx();
        }
        if (this._ctx.state === 'suspended') {
            this._ctx.resume();
        }
        return this._ctx;
    },

    // Unlocks the audio context from a user gesture (tap on "Start").
    // iOS Safari/WebView refuses to play audio until a gesture has done this.
    unlock: function () {
        this._getContext();
    },

    beep: function (frequencyHz, durationMs) {
        const ctx = this._getContext();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'square';
        osc.frequency.value = frequencyHz || 1500;
        gain.gain.value = 0.35;
        osc.connect(gain);
        gain.connect(ctx.destination);
        const now = ctx.currentTime;
        osc.start(now);
        osc.stop(now + (durationMs || 150) / 1000);
    },

    vibrate: function (ms) {
        if (navigator.vibrate) navigator.vibrate(ms || 100);
    }
};
