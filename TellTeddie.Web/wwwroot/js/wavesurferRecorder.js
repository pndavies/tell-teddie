window.wavesurferRecorder = {
    wavesurfer: null,
    record: null,
    recordedBlob: null,
    recordingPromise: null,
    recordingResolve: null,
    blazorReference: null,

    init: async function(containerId) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container with id '${containerId}' not found`);
            return false;
        }

        try {
            // Clean up existing instance if it exists
            if (this.wavesurfer) {
                this.wavesurfer.destroy();
                this.wavesurfer = null;
            }

            // Create wavesurfer instance
            this.wavesurfer = WaveSurfer.create({
                container: container,
                waveColor: '#ddd',
                progressColor: '#4CAF50',
                barWidth: 2,
                barGap: 1,
                barRadius: 3,
                height: 60,
                cursorColor: '#333',
                cursorWidth: 2
            });

            // Listen for when playback finishes
            this.wavesurfer.on('finish', () => {
                console.log('Playback finished');
                if (this.blazorReference) {
                    this.blazorReference.invokeMethodAsync('OnPlayFinished');
                }
            });

            // Initialize record plugin
            this.record = this.wavesurfer.registerPlugin(
                WaveSurfer.Record.create({
                    renderRecordedAudio: true,
                    scrollingWaveform: false,
                    continuousWaveform: true,
                    continuousWaveformDuration: 30
                })
            );

            // Listen for record-end event
            this.record.on('record-end', (blob) => {
                this.recordedBlob = blob;
                console.log('Recording ended, blob received:', blob);
                if (this.recordingResolve) {
                    this.recordingResolve(true);
                    this.recordingResolve = null;
                }
            });

            console.log('Wavesurfer initialized successfully');
            return true;
        } catch (error) {
            console.error('Failed to initialize wavesurfer:', error);
            return false;
        }
    },

    reinit: async function(containerId) {
        console.log('Reinitializing wavesurfer...');
        // Clear the recorded blob and state
        this.recordedBlob = null;
        this.recordingResolve = null;
        
        // Destroy existing instance if it exists
        if (this.wavesurfer) {
            try {
                this.wavesurfer.destroy();
            } catch (e) {
                console.warn('Error destroying wavesurfer:', e);
            }
            this.wavesurfer = null;
        }
        
        if (this.record) {
            try {
                this.record.destroy();
            } catch (e) {
                console.warn('Error destroying record plugin:', e);
            }
            this.record = null;
        }
        
        // Reinitialize
        return this.init(containerId);
    },

    setOnPlayFinished: function(blazorRef) {
        this.blazorReference = blazorRef;
        console.log('Blazor reference set for play finished callback');
    },

    startRecording: async function() {
        try {
            if (!this.record) {
                console.error('Record plugin not initialized');
                return false;
            }

            // Clear previous recording
            this.recordedBlob = null;
            this.wavesurfer.empty();

            // Start recording
            await this.record.startRecording();
            console.log('Recording started');
            return true;
        } catch (error) {
            console.error('Failed to start recording:', error);
            return false;
        }
    },

    stopRecording: async function() {
        try {
            if (!this.record) {
                console.error('Record plugin not initialized');
                return false;
            }

            // Create a promise that will be resolved when record-end event fires
            return new Promise((resolve) => {
                this.recordingResolve = resolve;
                
                // Set a timeout in case the event doesn't fire
                setTimeout(() => {
                    if (this.recordingResolve) {
                        console.error('Recording stop timeout - no blob received');
                        this.recordingResolve = null;
                        resolve(false);
                    }
                }, 5000);

                // Stop recording (this will trigger the record-end event)
                this.record.stopRecording();
                console.log('Stop recording called');
            });
        } catch (error) {
            console.error('Failed to stop recording:', error);
            return false;
        }
    },

    playRecording: function() {
        try {
            if (!this.wavesurfer) {
                console.error('Wavesurfer not initialized');
                return false;
            }
            this.wavesurfer.play();
            return true;
        } catch (error) {
            console.error('Failed to play recording:', error);
            return false;
        }
    },

    pauseRecording: function() {
        try {
            if (!this.wavesurfer) {
                console.error('Wavesurfer not initialized');
                return false;
            }
            this.wavesurfer.pause();
            return true;
        } catch (error) {
            console.error('Failed to pause recording:', error);
            return false;
        }
    },

    discardRecording: function() {
        try {
            if (this.wavesurfer) {
                this.wavesurfer.empty();
            }
            this.recordedBlob = null;
            return true;
        } catch (error) {
            console.error('Failed to discard recording:', error);
            return false;
        }
    },

    uploadToAzure: async function(sasUrl) {
        try {
            if (!this.recordedBlob) {
                throw new Error("No recording to upload!");
            }
            
            console.log('Uploading blob to Azure:', sasUrl, 'Blob size:', this.recordedBlob.size);
            
            const response = await fetch(sasUrl, {
                method: "PUT",
                headers: { "x-ms-blob-type": "BlockBlob" },
                body: this.recordedBlob
            });

            if (!response.ok) {
                throw new Error(`Upload failed with status ${response.status}: ${response.statusText}`);
            }
            
            console.log('Upload to Azure successful');
            return true;
        } catch (error) {
            console.error('Upload to Azure failed:', error);
            throw error;
        }
    }
};
