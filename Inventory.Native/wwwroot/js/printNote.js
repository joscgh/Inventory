window.printNote = {
    print: function (elementId, watermarkText) {
        try {
            var elem = document.getElementById(elementId);
            if (!elem) {
                console.error('printNote: element not found', elementId);
                return;
            }

            var printContent = elem.cloneNode(true);
            printContent.id = 'printable-note-clone';

            var printContainer = document.createElement('div');
            printContainer.id = 'printNoteContainer';
            printContainer.style.position = 'absolute';
            printContainer.style.left = '-9999px';
            printContainer.style.top = '0';
            printContainer.style.width = '210mm';
            printContainer.style.visibility = 'hidden';
            printContainer.setAttribute('aria-hidden', 'true');

            var watermark = document.createElement('div');
            watermark.className = 'print-watermark';
            for (var i = 0; i < 9; i++) {
                var span = document.createElement('span');
                span.textContent = watermarkText || '';
                watermark.appendChild(span);
            }

            printContainer.appendChild(watermark);
            printContainer.appendChild(printContent);
            document.body.appendChild(printContainer);

            var style = document.createElement('style');
            style.id = 'printNoteStyle';
            style.textContent = `
                @page { size: A4 portrait; margin: 0 !important; }
                @media screen { #printNoteContainer { display: none; } }
                @media print {
                    body > *:not(#printNoteContainer) { display: none !important; }
                    html, body { width: 100% !important; height: auto !important; margin: 0 !important; padding: 0 !important; background: white !important; }
                    #printNoteContainer { display: block !important; position: relative !important; inset: auto !important; width: 100% !important; max-width: 100% !important; margin: 0 !important; padding: 12mm 14mm 14mm 14mm !important; box-sizing: border-box !important; visibility: visible !important; opacity: 1 !important; background: white !important; overflow: visible !important; }
                    #printNoteContainer > * { max-width: 100% !important; box-sizing: border-box !important; }
                    #printNoteContainer img { max-width: 180px !important; max-height: 90px !important; width: auto !important; height: auto !important; object-fit: contain !important; display: block !important; visibility: visible !important; opacity: 1 !important; }
                    #printNoteContainer img[src] { visibility: visible !important; opacity: 1 !important; }
                    #printNoteContainer svg, #printNoteContainer picture, #printNoteContainer img { visibility: visible !important; }
                    #printNoteContainer .print-watermark { position: fixed; left: 0; right: 0; top: 20mm; bottom: 20mm; pointer-events: none; z-index: 9999; }
                    #printNoteContainer .print-watermark span { position: absolute; color: rgba(0,0,0,0.08); font-size: 48px; white-space: nowrap; transform: rotate(-30deg); }
                    #printNoteContainer .print-watermark span:nth-child(1) { top: 10%; left: 10%; }
                    #printNoteContainer .print-watermark span:nth-child(2) { top: 10%; left: 50%; transform: translateX(-50%) rotate(-30deg); }
                    #printNoteContainer .print-watermark span:nth-child(3) { top: 10%; right: 10%; }
                    #printNoteContainer .print-watermark span:nth-child(4) { top: 40%; left: 15%; }
                    #printNoteContainer .print-watermark span:nth-child(5) { top: 40%; left: 50%; transform: translateX(-50%) rotate(-30deg); }
                    #printNoteContainer .print-watermark span:nth-child(6) { top: 40%; right: 15%; }
                    #printNoteContainer .print-watermark span:nth-child(7) { bottom: 20%; left: 10%; }
                    #printNoteContainer .print-watermark span:nth-child(8) { bottom: 20%; left: 50%; transform: translateX(-50%) rotate(-30deg); }
                    #printNoteContainer .print-watermark span:nth-child(9) { bottom: 20%; right: 10%; }
                }
            `;
            document.head.appendChild(style);

            function cleanup() {
                try {
                    if (style && style.parentNode) {
                        style.parentNode.removeChild(style);
                    }
                    if (printContainer && printContainer.parentNode) {
                        printContainer.parentNode.removeChild(printContainer);
                    }
                } catch (e) { }
            }

            function doPrint() {
                try {
                    window.print();
                } catch (e) {
                    console.error('printNote: print failed', e);
                    cleanup();
                }
            }

            function onAfterPrint() {
                cleanup();
                window.removeEventListener('afterprint', onAfterPrint);
            }

            window.addEventListener('afterprint', onAfterPrint);

            function onAllImagesLoaded() {
                var images = printContainer.querySelectorAll('img');
                var total = images.length;
                if (total === 0) {
                    doPrint();
                    return;
                }

                var loadedCount = 0;
                function check() {
                    loadedCount++;
                    if (loadedCount >= total) {
                        doPrint();
                    }
                }

                for (var i = 0; i < total; i++) {
                    var img = images[i];
                    if (img.complete) {
                        check();
                    } else {
                        img.onload = img.onerror = check;
                    }
                }
            }

            function onReady() {
                onAllImagesLoaded();
            }

            setTimeout(onReady, 100);

        } catch (e) {
            console.error('printNote error', e);
        }
    }
};
