import { BrowserMultiFormatReader, NotFoundException } from 'https://cdn.jsdelivr.net/npm/@zxing/library@0.19.1/esm/browser.js';

const scanners = new Map();

export async function startBarcodeScanner(videoElementId, dotNetRef) {
    const videoElem = document.getElementById(videoElementId);
    if (!videoElem) {
        console.error('Video element not found:', videoElementId);
        return;
    }

    const codeReader = new BrowserMultiFormatReader();
    scanners.set(videoElementId, { codeReader, dotNetRef });

    try {
        const hints = new Map();
        hints.set(0, true);
        await codeReader.decodeFromVideoDevice(null, videoElem, (result, err) => {
            if (result) {
                const text = result.getText();
                dotNetRef.invokeMethodAsync('OnBarcodeScanned', text);
            }
            if (err && !(err instanceof NotFoundException)) {
                console.warn(err);
            }
        });
    }
    catch (error) {
        console.error('Failed to start barcode scanner', error);
    }
}

export async function stopBarcodeScanner(videoElementId) {
    const entry = scanners.get(videoElementId);
    if (!entry) return;

    entry.codeReader.reset();
    scanners.delete(videoElementId);
}
