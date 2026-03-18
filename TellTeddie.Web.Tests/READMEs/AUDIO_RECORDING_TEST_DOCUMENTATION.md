# Audio Recording Feature - Test Documentation

## Overview
This document describes the test coverage for the new audio recording functionality with wavesurfer.js integration in TellTeddie.

## Test Organization

### 1. Blazor Component Tests
**File:** `TellTeddie.Web.Tests/Components/Forms/AudioFormComponentTests.cs`

#### Rendering Tests
- ✅ AudioForm renders with title
- ✅ AudioForm renders with name input field
- ✅ AudioForm renders with caption textarea field
- ✅ AudioForm renders waveform container (#waveform div)
- ✅ AudioForm renders start recording button
- ✅ AudioForm renders stop recording button
- ✅ AudioForm renders submit button

#### Button State Tests
- ✅ Start Recording button is enabled initially (when jsReady = true)
- ✅ Play/Pause/Discard buttons only show when recording exists
- ✅ Submit button is disabled without recording

#### Component Lifecycle Tests
- ✅ Initializes wavesurfer on first render
- ✅ Sets up Blazor callback for playback finish event
- ✅ Reinitializes wavesurfer when form is reopened

#### Recording Flow Tests
- ✅ Can start recording via button click
- ✅ Can stop recording via button click
- ✅ Records for maximum 30 seconds with countdown timer

#### Form Validation Tests
- ✅ Includes DataAnnotationsValidator component
- ✅ Name field is optional (defaults to "Anonymous")
- ✅ Caption field is optional (has default text)

#### State Management Tests
- ✅ Reinitializes wavesurfer after form submission
- ✅ Properly resets form state on successful submission

#### Error Handling Tests
- ✅ Displays error message when wavesurfer initialization fails
- ✅ Handles JS exceptions gracefully
- ✅ Shows appropriate status messages for each operation state

#### Integration Tests
- ✅ Full component renders without errors
- ✅ All major UI elements are present

### 2. JavaScript Functionality Tests
**File:** `TellTeddie.Web.Tests/JavaScript/WavesurferRecorderTests.cs`

These tests document the expected behavior of wavesurferRecorder.js functions. In production, these would be tested with browser automation tools like Playwright or Cypress.

#### Initialization Tests
- Initializes wavesurfer instance with correct configuration
- Creates and registers Record plugin
- Sets up event listeners for 'record-end' and 'finish' events
- Returns true on success, false on failure
- Properly cleans up previous instance on reinit

#### Recording Tests
- StartRecording begins audio capture
- StopRecording captures audio blob to recordedBlob property
- RecordEndEvent listener resolves the stopRecording promise
- Handles timeout if 'record-end' event doesn't fire (5-second limit)
- Loads recorded blob into wavesurfer for preview

#### Playback Tests
- PlayRecording starts audio playback
- PauseRecording pauses playback
- PlaybackFinishEvent triggers Blazor callback when audio finishes

#### Upload Tests
- UploadToAzure sends blob to SAS URL via PUT request
- Sets correct Content-Type header (x-ms-blob-type: BlockBlob)
- Throws error if no blob is recorded
- Handles network errors appropriately

#### State Management Tests
- RecordedBlob persists after stopRecording()
- Reinit() clears all state and creates fresh instance
- DiscardRecording() clears waveform and blob

#### Error Handling Tests
- Init handles container not found error
- StartRecording handles plugin not initialized error
- StopRecording handles timeout with fallback
- UploadToAzure handles network failures

#### Integration Tests
- Full recording cycle: init → record → preview → upload
- Record → discard → re-record cycle
- Record → play → finish event → callback cycle
- Console logging for all major operations

### 3. API Service Tests
**File:** `TellTeddie.Api.Tests/Services/AudioPostServiceTests.cs`

Existing tests for the AudioPostService cover:
- ✅ Retrieving all audio posts
- ✅ Retrieving audio posts by ID
- ✅ Creating new audio posts
- ✅ Filtering expired posts
- ✅ Proper DTO mapping

#### Audio Recording Specific Validations
When integrated with the recording flow:
- Audio posts are created with MediaType = "AUDIO"
- Audio posts have 24-hour expiration set
- Audio posts can be marked as cheer or regular
- AudioPostUrl is properly stored from SAS blob upload
- Anonymous posts are handled correctly
- Concurrent audio uploads are handled safely

## Testing Best Practices

### Unit Tests (Component Level)
```csharp
// Using bUnit for Blazor component testing
var cut = Render<Audio>();
var button = cut.Find("button.btn-primary");
await button.ClickAsync(new MouseEventArgs());
```

### Mocking JavaScript Calls
```csharp
_jsRuntimeMock
    .Setup(x => x.InvokeAsync<bool>("wavesurferRecorder.startRecording", It.IsAny<object[]>()))
    .ReturnsAsync(true);
```

### Testing JavaScript Functions
JavaScript functions should be tested with browser automation:
- Playwright (recommended for .NET integration)
- Cypress (JavaScript-based)
- Selenium (legacy support)

## Manual Testing Checklist

### Recording Workflow
- [ ] Open audio form
- [ ] Click "Start Recording"
- [ ] Verify waveform visualization appears
- [ ] Speak into microphone
- [ ] Verify waveform updates in real-time
- [ ] Click "Stop Recording"
- [ ] Verify recording appears for preview
- [ ] Click "Play"
- [ ] Verify audio plays with waveform progress
- [ ] Audio completes and play button re-enables

### Re-recording Workflow
- [ ] Have a successful recording
- [ ] Click "Discard recording"
- [ ] Verify waveform clears
- [ ] Verify recording controls reappear
- [ ] Click "Start Recording" again
- [ ] Verify new recording can be captured

### Upload Workflow
- [ ] Have a successful recording
- [ ] Fill in Name and Caption (optional)
- [ ] Click "Submit Audio Post"
- [ ] Verify uploading status message
- [ ] Verify success message appears
- [ ] Verify form resets for new recording

### Error Scenarios
- [ ] Browser doesn't support Web Audio API - graceful error
- [ ] Microphone permission denied - appropriate error message
- [ ] Network error during upload - error handling
- [ ] Close form without submitting - no data loss

## Test Coverage Goals
- **Component Logic:** 85%+ coverage
- **JavaScript Logic:** Documented behavior with browser tests
- **API Integration:** 90%+ coverage for audio post operations
- **E2E Scenarios:** All major user workflows tested manually

## CI/CD Integration

### Unit Tests
Run automatically on every commit:
```bash
dotnet test TellTeddie.Web.Tests
dotnet test TellTeddie.Api.Tests
```

### Browser Tests (Recommended Setup)
Add Playwright tests for JavaScript functionality:
```bash
playwright test tests/audio-recording.spec.ts
```

### Coverage Reports
Generate coverage reports:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Known Limitations

1. **JavaScript Tests:** Cannot mock actual Web Audio API in unit tests. Use browser automation for integration testing.
2. **Blob Handling:** Blobs cannot be passed between JS and C# boundaries, so upload happens client-side.
3. **Microphone Access:** Requires HTTPS in production (browser security requirement).
4. **Cross-Origin:** CORS headers must be properly configured for SAS URL uploads.

## Future Improvements

1. Add Playwright tests for JavaScript recording functions
2. Add performance tests for large recordings (30+ seconds)
3. Add tests for concurrent uploads from multiple users
4. Add tests for different audio formats
5. Add tests for audio compression
6. Add tests for recording in low-bandwidth scenarios
