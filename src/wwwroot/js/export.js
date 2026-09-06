window.MakeReadyExport = {

    downloadCsv: function (filename, content) {
        const bom = '﻿'; // UTF-8 BOM so Excel opens it correctly
        const blob = new Blob([bom + content], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    },

    exportDashboardAsPng: async function (svgIds) {
        const CANVAS_W = 1080;
        const PAD = 40;
        const TITLE_H = 90;
        const GAP = 20;

        const svgs = svgIds.map(id => document.getElementById(id)).filter(Boolean);
        if (svgs.length === 0) return null;

        // Compute per-chart draw heights (maintain aspect ratio to canvas width)
        const drawWidths  = svgs.map(() => CANVAS_W - PAD * 2);
        const drawHeights = svgs.map((svg, i) => {
            const vb = svg.viewBox && svg.viewBox.baseVal;
            if (vb && vb.width > 0 && vb.height > 0)
                return Math.round((vb.height / vb.width) * drawWidths[i]);
            const rect = svg.getBoundingClientRect();
            return rect.height > 0 ? Math.round(rect.height) : 200;
        });

        const totalH = TITLE_H + svgs.reduce((acc, _, i) => acc + drawHeights[i] + GAP, 0) + PAD;

        const canvas = document.createElement('canvas');
        canvas.width  = CANVAS_W;
        canvas.height = totalH;
        const ctx = canvas.getContext('2d');

        // Background
        ctx.fillStyle = '#0d1117';
        ctx.fillRect(0, 0, CANVAS_W, totalH);

        // Title bar
        ctx.fillStyle = '#161b22';
        ctx.fillRect(0, 0, CANVAS_W, TITLE_H);

        ctx.fillStyle = '#d4a017';
        ctx.font = 'bold 26px -apple-system, Segoe UI, sans-serif';
        ctx.fillText('MakeReady — Performance Report', PAD, 40);

        ctx.fillStyle = '#8b949e';
        ctx.font = '15px -apple-system, Segoe UI, sans-serif';
        ctx.fillText('Generated ' + new Date().toLocaleString(), PAD, 68);

        // Separator line
        ctx.strokeStyle = '#30363d';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(0, TITLE_H);
        ctx.lineTo(CANVAS_W, TITLE_H);
        ctx.stroke();

        let y = TITLE_H + GAP;

        for (let i = 0; i < svgs.length; i++) {
            const svg = svgs[i];
            try {
                const serialized = new XMLSerializer().serializeToString(svg);
                const base64 = btoa(unescape(encodeURIComponent(serialized)));
                const dataUrl = 'data:image/svg+xml;base64,' + base64;

                await new Promise((resolve) => {
                    const img = new Image();
                    img.onload = () => {
                        ctx.drawImage(img, PAD, y, drawWidths[i], drawHeights[i]);
                        y += drawHeights[i] + GAP;
                        resolve();
                    };
                    img.onerror = () => { y += GAP; resolve(); };
                    img.src = dataUrl;
                });
            } catch (e) {
                console.warn('Chart render failed', svg.id, e);
                y += GAP;
            }
        }

        return canvas.toDataURL('image/png');
    }
};
