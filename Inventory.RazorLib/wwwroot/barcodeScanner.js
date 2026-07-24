// ZXing se carga como script global (window.ZXing) desde
// _content/Inventory.RazorLib/lib/zxing.min.js (referenciado en index.html).
// Así este módulo no hace ningún import externo y funciona sin internet.

const scanners = new Map();

export async function startBarcodeScanner(videoElementId, dotNetRef) {
    const videoElem = document.getElementById(videoElementId);
    if (!videoElem) {
        await dotNetRef.invokeMethodAsync('OnScannerError', 'No se encontró el elemento de video.');
        return;
    }

    const ZXing = window.ZXing;
    if (!ZXing || !ZXing.BrowserMultiFormatReader) {
        await dotNetRef.invokeMethodAsync('OnScannerError', 'No se pudo cargar la librería del escáner (ZXing).');
        return;
    }

    // getUserMedia solo está disponible en contexto seguro (HTTPS o localhost).
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        await dotNetRef.invokeMethodAsync(
            'OnScannerError',
            'La cámara requiere una conexión segura. Abre la app por HTTPS o desde localhost (en el teléfono no funciona por http://IP).');
        return;
    }

    const NotFoundException = ZXing.NotFoundException;
    const codeReader = new ZXing.BrowserMultiFormatReader();
    scanners.set(videoElementId, { codeReader, dotNetRef });

    // Preferir la cámara trasera en el móvil; en laptop cae a la disponible.
    const constraints = { video: { facingMode: { ideal: 'environment' } } };

    try {
        await codeReader.decodeFromConstraints(constraints, videoElem, (result, err) => {
            if (result) {
                dotNetRef.invokeMethodAsync('OnBarcodeScanned', result.getText());
            }
            // NotFoundException es normal entre fotogramas sin código; se ignora.
            if (err && NotFoundException && !(err instanceof NotFoundException)) {
                console.warn(err);
            }
        });
    }
    catch (error) {
        console.error('Failed to start barcode scanner', error);
        let msg = 'No se pudo iniciar la cámara.';
        if (error) {
            if (error.name === 'NotAllowedError') msg = 'Permiso de cámara denegado. Habilítalo en el navegador.';
            else if (error.name === 'NotFoundError') msg = 'No se encontró ninguna cámara en el dispositivo.';
            else if (error.name === 'NotReadableError') msg = 'La cámara está en uso por otra aplicación.';
            else if (error.message) msg = error.message;
        }
        await dotNetRef.invokeMethodAsync('OnScannerError', msg);
        scanners.delete(videoElementId);
    }
}

export function stopBarcodeScanner(videoElementId) {
    const entry = scanners.get(videoElementId);
    if (!entry) return;

    try {
        entry.codeReader.reset();
    } catch (e) {
        console.warn('Error stopping scanner', e);
    }
    scanners.delete(videoElementId);
}
