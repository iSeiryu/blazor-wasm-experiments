window.neonSnakeEggCache = {
    renderStaticEgg: function (er, eh, padding, dpr) {
        var canvas = document.getElementById('static-egg-cache');
        var logical = Math.max(er, eh) * 2 + padding * 2;
        var px = Math.ceil(logical * dpr);
        canvas.width = px;
        canvas.height = px;
        var ctx = canvas.getContext('2d');
        ctx.scale(dpr, dpr);

        var cx = logical / 2;
        var cy = logical / 2;

        // Glow
        var grd = ctx.createRadialGradient(cx, cy, er * 0.1, cx, cy, er * 2.6);
        grd.addColorStop(0, 'rgba(90,255,215,0.45)');
        grd.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = grd;
        ctx.beginPath();
        ctx.ellipse(cx, cy, er * 2.4, eh * 2.4, 0, 0, Math.PI * 2);
        ctx.fill();

        // Body with shadow
        ctx.save();
        ctx.shadowColor = 'rgba(60,220,185,0.8)';
        ctx.shadowBlur = 18;
        var bodyGrd = ctx.createRadialGradient(cx - er * 0.3, cy - eh * 0.32, eh * 0.05, cx, cy, eh * 1.15);
        bodyGrd.addColorStop(0, '#eefffa');
        bodyGrd.addColorStop(0.55, '#88f0d8');
        bodyGrd.addColorStop(1, '#28b09a');
        ctx.fillStyle = bodyGrd;
        ctx.beginPath();
        ctx.ellipse(cx, cy, er, eh, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();

        // Outline
        ctx.strokeStyle = 'rgba(110,255,225,0.8)';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.ellipse(cx, cy, er, eh, 0, 0, Math.PI * 2);
        ctx.stroke();

        // Speckles
        var speckles = [
            { ox: -0.35, oy: -0.18, rx: 0.18, ry: 0.09, a: 0.4 },
            { ox: 0.38, oy: 0.2, rx: 0.13, ry: 0.07, a: -0.6 },
            { ox: -0.1, oy: 0.35, rx: 0.1, ry: 0.06, a: 0.9 }
        ];
        ctx.fillStyle = 'rgba(25,135,115,0.28)';
        for (var i = 0; i < speckles.length; i++) {
            var s = speckles[i];
            ctx.beginPath();
            ctx.ellipse(cx + s.ox * er, cy + s.oy * eh, s.rx * er, s.ry * eh, s.a, 0, Math.PI * 2);
            ctx.fill();
        }

        // Highlight
        ctx.fillStyle = 'rgba(255,255,255,0.58)';
        ctx.beginPath();
        ctx.ellipse(cx - er * 0.28, cy - eh * 0.32, er * 0.38, eh * 0.2, -0.3, 0, Math.PI * 2);
        ctx.fill();

        window.staticEggCache = canvas;
        return logical;
    },

    renderRunningEgg: function (er, eh, padding, dpr) {
        var canvas = document.getElementById('running-egg-cache');
        var logical = Math.max(er, eh) * 2 + padding * 2;
        var px = Math.ceil(logical * dpr);
        canvas.width = px;
        canvas.height = px;
        var ctx = canvas.getContext('2d');
        ctx.scale(dpr, dpr);

        var cx = logical / 2;
        var cy = logical / 2;

        // Glow
        var grd = ctx.createRadialGradient(cx, cy, er * 0.1, cx, cy, er * 2.6);
        grd.addColorStop(0, 'rgba(255,195,50,0.55)');
        grd.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = grd;
        ctx.beginPath();
        ctx.ellipse(cx, cy, er * 2.4, eh * 2.4, 0, 0, Math.PI * 2);
        ctx.fill();

        // Body with shadow
        ctx.save();
        ctx.shadowColor = 'rgba(255,160,30,0.85)';
        ctx.shadowBlur = 18;
        var bodyGrd = ctx.createRadialGradient(cx - er * 0.3, cy - eh * 0.32, eh * 0.05, cx, cy, eh * 1.15);
        bodyGrd.addColorStop(0, '#fff8e0');
        bodyGrd.addColorStop(0.55, '#ffe070');
        bodyGrd.addColorStop(1, '#ff9020');
        ctx.fillStyle = bodyGrd;
        ctx.beginPath();
        ctx.ellipse(cx, cy, er, eh, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();

        // Outline
        ctx.strokeStyle = 'rgba(255,200,80,0.9)';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.ellipse(cx, cy, er, eh, 0, 0, Math.PI * 2);
        ctx.stroke();

        // Speckles
        var speckles = [
            { ox: -0.35, oy: -0.18, rx: 0.18, ry: 0.09, a: 0.4 },
            { ox: 0.38, oy: 0.2, rx: 0.13, ry: 0.07, a: -0.6 },
            { ox: -0.1, oy: 0.35, rx: 0.1, ry: 0.06, a: 0.9 }
        ];
        ctx.fillStyle = 'rgba(175,85,0,0.28)';
        for (var i = 0; i < speckles.length; i++) {
            var s = speckles[i];
            ctx.beginPath();
            ctx.ellipse(cx + s.ox * er, cy + s.oy * eh, s.rx * er, s.ry * eh, s.a, 0, Math.PI * 2);
            ctx.fill();
        }

        // Highlight
        ctx.fillStyle = 'rgba(255,255,255,0.58)';
        ctx.beginPath();
        ctx.ellipse(cx - er * 0.28, cy - eh * 0.32, er * 0.38, eh * 0.2, -0.3, 0, Math.PI * 2);
        ctx.fill();

        window.runningEggCache = canvas;
        return logical;
    }
};
