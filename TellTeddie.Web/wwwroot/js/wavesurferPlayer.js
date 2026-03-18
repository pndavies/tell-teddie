window.wavesurferPlayer = {
    instances: {},

    init: async function(containerId, audioUrl) {
        try {
            console.log(`Initializing WaveSurfer for ${containerId} with URL: ${audioUrl}`);
            
            // Clean up existing instance if it exists
            if (this.instances[containerId]) {
                this.instances[containerId].destroy();
                delete this.instances[containerId];
            }

            const container = document.getElementById(containerId);
            if (!container) {
                console.error(`Container with id '${containerId}' not found`);
                return false;
            }

            // Create wavesurfer instance
            const wavesurfer = WaveSurfer.create({
                container: container,
                waveColor: '#ddd',
                progressColor: '#4CAF50',
                barWidth: 2,
                barGap: 1,
                barRadius: 3,
                height: 80,
                cursorColor: '#333',
                cursorWidth: 2,
                normalize: true,
                backend: 'WebAudio',
                xhr: {
                    mode: 'cors',
                    cache: 'default'
                }
            });

            // Handle ready event
            wavesurfer.on('ready', () => {
                console.log(`WaveSurfer ready for ${containerId}`);
            });

            // Handle errors
            wavesurfer.on('error', (error) => {
                console.error(`WaveSurfer error for ${containerId}:`, error);
            });

            // Load the audio file
            console.log(`Loading audio from: ${audioUrl}`);
            await wavesurfer.load(audioUrl);

            // Store instance
            this.instances[containerId] = wavesurfer;

            console.log(`WaveSurfer player initialized successfully for ${containerId}`);
            return true;
        } catch (error) {
            console.error('Failed to initialize wavesurfer player:', error);
            return false;
        }
    },

    play: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            instance.play();
            return true;
        }
        console.warn(`No instance found for ${containerId}`);
        return false;
    },

    pause: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            instance.pause();
            return true;
        }
        return false;
    },

    playPause: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            instance.playPause();
            return true;
        }
        console.warn(`No instance found for ${containerId}`);
        return false;
    },

    stop: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            instance.stop();
            return true;
        }
        return false;
    },

    setVolume: function(containerId, volume) {
        const instance = this.instances[containerId];
        if (instance) {
            instance.setVolume(volume);
            return true;
        }
        return false;
    },

    getCurrentTime: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            return instance.getCurrentTime();
        }
        return 0;
    },

    getDuration: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            return instance.getDuration();
        }
        return 0;
    },

    isPlaying: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            return instance.isPlaying();
        }
        return false;
    },

    destroy: function(containerId) {
        const instance = this.instances[containerId];
        if (instance) {
            instance.destroy();
            delete this.instances[containerId];
            return true;
        }
        return false;
    },

    destroyAll: function() {
        Object.keys(this.instances).forEach(id => {
            this.instances[id].destroy();
        });
        this.instances = {};
    }
};
