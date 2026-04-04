window.neonSnakeCache = {
    renderObstacle: function (size, padding, dpr) {
        var canvas = document.getElementById('obstacle-cache');
        var logical = size + padding * 2;
        canvas.width = Math.ceil(logical * dpr);
        canvas.height = Math.ceil(logical * dpr);
        var ctx = canvas.getContext('2d');
        ctx.scale(dpr, dpr);

        function rockPath() {
            ctx.beginPath();
            ctx.moveTo(padding + size * 0.2, padding + size);
            ctx.lineTo(padding, padding + size * 0.5);
            ctx.lineTo(padding + size * 0.15, padding + size * 0.15);
            ctx.lineTo(padding + size * 0.5, padding);
            ctx.lineTo(padding + size * 0.85, padding + size * 0.1);
            ctx.lineTo(padding + size, padding + size * 0.45);
            ctx.lineTo(padding + size * 0.8, padding + size);
            ctx.closePath();
        }

        ctx.save();
        ctx.shadowColor = 'rgba(99,123,255,0.6)';
        ctx.shadowBlur = 12;
        var grad = ctx.createLinearGradient(padding, padding, padding + size, padding + size);
        grad.addColorStop(0, '#2a3b74');
        grad.addColorStop(1, '#141d3d');
        ctx.fillStyle = grad;
        rockPath();
        ctx.fill();
        ctx.restore();

        ctx.strokeStyle = '#6f8cff';
        ctx.lineWidth = 1.5;
        rockPath();
        ctx.stroke();

        ctx.strokeStyle = 'rgba(170,200,255,0.35)';
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.moveTo(padding + size * 0.22, padding + size * 0.35);
        ctx.lineTo(padding + size * 0.58, padding + size * 0.5);
        ctx.lineTo(padding + size * 0.74, padding + size * 0.72);
        ctx.stroke();

        window.obstacleCache = canvas;
    }
};
