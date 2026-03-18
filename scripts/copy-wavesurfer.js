const fs = require('fs');
const path = require('path');

// Source and destination paths
const sourceDir = path.join(__dirname, '../node_modules/wavesurfer.js/dist');
const destDir = path.join(__dirname, '../TellTeddie.Web/wwwroot/lib/wavesurfer');

// Ensure destination directory exists
if (!fs.existsSync(destDir)) {
    fs.mkdirSync(destDir, { recursive: true });
    console.log(`Created directory: ${destDir}`);
}

// Files to copy
const files = [
    'wavesurfer.js',
    'wavesurfer.min.js',
    'wavesurfer.min.css',
    'wavesurfer.css',
    'plugins/record.js',
    'plugins/record.min.js'
];

files.forEach(file => {
    const src = path.join(sourceDir, file);
    const destFile = path.join(destDir, file);
    
    // Create subdirectory if needed
    const destFileDir = path.dirname(destFile);
    if (!fs.existsSync(destFileDir)) {
        fs.mkdirSync(destFileDir, { recursive: true });
    }
    
    if (fs.existsSync(src)) {
        fs.copyFileSync(src, destFile);
        console.log(`Copied: ${file}`);
    } else {
        console.warn(`File not found: ${src}`);
    }
});

console.log('Wavesurfer files copied successfully!');
