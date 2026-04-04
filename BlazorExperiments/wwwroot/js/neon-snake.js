class Vector2 {
    constructor(x = 0, y = 0) {
        this.x = x;
        this.y = y;
    }

    copy() {
        return new Vector2(this.x, this.y);
    }

    add(o) {
        let r = this.copy();
        r.x += o.x;
        r.y += o.y;
        return r;
    }

    mul(v) {
        let r = this.copy();
        r.x *= v;
        r.y *= v;
        return r;
    }

    sub(o) {
        return new Vector2(this.x - o.x, this.y - o.y);
    }

    distSq(o) {
        let dx = o.x - this.x;
        let dy = o.y - this.y;
        return dx * dx + dy * dy;
    }

    setFrom(o) {
        this.x = o.x;
        this.y = o.y;
    }
}
class SnakeBodyPart {
    constructor() {
        this.gridPos = new Vector2();
        this.direction = new Vector2();
        this.targetScreenPos = new Vector2();
        this.currentScreenPos = new Vector2();
        this.startScreenPos = new Vector2();
    }
}

const hudH = 50;
const screenW = 28 * 40;
const screenH = 18 * 40 + hudH;
let cols = 28 * 6;
let rows = 18 * 6;
const cellSize = 40;
const W = cellSize * cols;
const H = cellSize * rows;
let centerCoords = [];
let keyQue = [];

const KEY_UP = 38;
const KEY_DOWN = 40;
const KEY_LEFT = 37;
const KEY_RIGHT = 39;
let handledKeys = [KEY_UP, KEY_DOWN, KEY_LEFT, KEY_RIGHT];

const NEON = {
    bgOuter: "#040814",
    bgInner: "#0a1130",
    gridBase: "#0b1330",
    gridLine: "rgba(88, 205, 255, 0.1)",
    border: "#35f4ff",
    borderGlow: "rgba(153, 82, 255, 0.75)",
    snakePrimary: "#21f6ff",
    snakeSecondary: "#5a8bff",
    snakeGlow: "rgba(33, 246, 255, 0.75)",
    snakeEye: "#ff5de5",
    foodPrimary: "#ff36db",
    foodSecondary: "#ff84ff",
    foodGlow: "rgba(255, 74, 235, 0.9)",
    foodStem: "#77ffd2",
    obstacleBase: "#141d3d",
    obstacleEdge: "#6f8cff",
    obstacleGlow: "rgba(99, 123, 255, 0.6)",
    hudBgTop: "#080f26",
    hudBgBottom: "#0e1b3f",
    hudLine: "#33f1ff",
    textPrimary: "#88f8ff",
    textAccent: "#ff63ec",
    textMuted: "#8ca0d8",
    heartOn: "#ff4de2",
    heartOff: "#2b335a",
    danger: "#ff3f72",
    dangerGlow: "rgba(255,63,114,0.75)"
};

window.addEventListener("load", init);

function getIndex(col, row) {
    if (col < 0 || col >= cols || row < 0 || row >= rows) return -1;
    return row * cols + col;
}

class Game {
    constructor() {
        this.snake = [];
        this.score = 0;
        this.food = [];
        this.stepInterval = 300;
        this.stepAccumulator = 0;
        this.eatParticles = [];
        this.eatBounce = 0;
        this.dead = false;
        this.health = 3;
        this.camX = 0;
        this.camY = 0;
        this.obstacles = [];
        this.bestScore = parseInt(localStorage.getItem('snakeBestScore')) || 0;
        this.hitFlash = 0;
        this.hitParticles = [];
        this.screenShake = 0;
        this.deathTimer = 0;
        this.deathAnimDuration = 1500;
        this.deathSegments = [];
        this.showDeathScreen = false;
        this.audioCtx = new (window.AudioContext || window.webkitAudioContext)();

        let startCol = Math.floor(cols / 2);
        let startRow = Math.floor(rows / 2);
        for (let i = 0; i < 3; i++) {
            let part = new SnakeBodyPart();
            part.gridPos = new Vector2(startCol - i, startRow);
            part.direction = new Vector2(1, 0);
            let idx = getIndex(part.gridPos.x, part.gridPos.y);
            part.targetScreenPos = centerCoords[idx].copy();
            part.currentScreenPos = centerCoords[idx].copy();
            part.startScreenPos = centerCoords[idx].copy();
            this.snake.push(part);
        }
        this.head = this.snake[0];

        // Spawn obstacles
        let numObstacles = Math.floor(cols * rows * 0.02);
        for (let i = 0; i < numObstacles; i++) {
            this.spawnObstacle();
        }

        for (let i = 0; i < 50; i++) {
            this.spawnFood();
        }
    }

    isOccupied(pos) {
        for (let part of this.snake) {
            if (part.gridPos.x === pos.x && part.gridPos.y === pos.y) return true;
        }
        for (let f of this.food) {
            if (f.x === pos.x && f.y === pos.y) return true;
        }
        for (let o of this.obstacles) {
            if (o.x === pos.x && o.y === pos.y) return true;
        }
        return false;
    }

    spawnObstacle() {
        let pos;
        do {
            pos = new Vector2(
                Math.floor(Math.random() * cols),
                Math.floor(Math.random() * rows)
            );
        } while (this.isOccupied(pos));
        this.obstacles.push(pos);
    }

    spawnFood() {
        let pos;
        do {
            pos = new Vector2(
                Math.floor(Math.random() * cols),
                Math.floor(Math.random() * rows)
            );
        } while (this.isOccupied(pos));
        this.food.push({ x: pos.x, y: pos.y, timer: 5000, running: false, runDir: { x: 1, y: 0 }, runAccum: 0,
            screenX: centerCoords[getIndex(pos.x, pos.y)].x, screenY: centerCoords[getIndex(pos.x, pos.y)].y,
            startScreenX: centerCoords[getIndex(pos.x, pos.y)].x, startScreenY: centerCoords[getIndex(pos.x, pos.y)].y,
            targetScreenX: centerCoords[getIndex(pos.x, pos.y)].x, targetScreenY: centerCoords[getIndex(pos.x, pos.y)].y });
    }

    _randomRunDir() {
        const dirs = [{ x: 1, y: 0 }, { x: -1, y: 0 }, { x: 0, y: 1 }, { x: 0, y: -1 }];
        return dirs[Math.floor(Math.random() * 4)];
    }

    _eggBlocked(x, y, skipEgg) {
        if (x < 0 || x >= cols || y < 0 || y >= rows) return true;
        for (let o of this.obstacles) {
            if (o.x === x && o.y === y) return true;
        }
        for (let f of this.food) {
            if (f !== skipEgg && f.x === x && f.y === y) return true;
        }
        return false;
    }

    _stepEgg(egg) {
        const allDirs = [{ x: 1, y: 0 }, { x: -1, y: 0 }, { x: 0, y: 1 }, { x: 0, y: -1 }];
        let dirs;
        if (Math.random() < 0.72) {
            let rest = allDirs.filter(d => !(d.x === egg.runDir.x && d.y === egg.runDir.y));
            for (let i = rest.length - 1; i > 0; i--) {
                let j = Math.floor(Math.random() * (i + 1));
                [rest[i], rest[j]] = [rest[j], rest[i]];
            }
            dirs = [egg.runDir, ...rest];
        } else {
            dirs = [...allDirs];
            for (let i = dirs.length - 1; i > 0; i--) {
                let j = Math.floor(Math.random() * (i + 1));
                [dirs[i], dirs[j]] = [dirs[j], dirs[i]];
            }
        }
        for (let d of dirs) {
            let nx = egg.x + d.x;
            let ny = egg.y + d.y;
            if (!this._eggBlocked(nx, ny, egg)) {
                egg.x = nx;
                egg.y = ny;
                egg.runDir = { x: d.x, y: d.y };
                return;
            }
        }
    }

    playEatSound() {
        try {
            const ac = this.audioCtx;
            const now = ac.currentTime;

            // Rising chirp — main tone
            const osc1 = ac.createOscillator();
            const gain1 = ac.createGain();
            osc1.connect(gain1);
            gain1.connect(ac.destination);
            osc1.type = 'sine';
            osc1.frequency.setValueAtTime(520, now);
            osc1.frequency.exponentialRampToValueAtTime(1040, now + 0.13);
            gain1.gain.setValueAtTime(0.28, now);
            gain1.gain.exponentialRampToValueAtTime(0.001, now + 0.28);
            osc1.start(now);
            osc1.stop(now + 0.28);

            // Soft harmonic shimmer
            const osc2 = ac.createOscillator();
            const gain2 = ac.createGain();
            osc2.connect(gain2);
            gain2.connect(ac.destination);
            osc2.type = 'sine';
            osc2.frequency.setValueAtTime(1040, now + 0.03);
            osc2.frequency.exponentialRampToValueAtTime(2080, now + 0.15);
            gain2.gain.setValueAtTime(0.12, now + 0.03);
            gain2.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
            osc2.start(now + 0.03);
            osc2.stop(now + 0.22);
        } catch (e) {}
    }

    playHitSound() {
        try {
            const ac = this.audioCtx;
            const now = ac.currentTime;

            // Low thud — pitched drop
            const osc1 = ac.createOscillator();
            const gain1 = ac.createGain();
            osc1.connect(gain1);
            gain1.connect(ac.destination);
            osc1.type = 'sawtooth';
            osc1.frequency.setValueAtTime(240, now);
            osc1.frequency.exponentialRampToValueAtTime(55, now + 0.18);
            gain1.gain.setValueAtTime(0.38, now);
            gain1.gain.exponentialRampToValueAtTime(0.001, now + 0.22);
            osc1.start(now);
            osc1.stop(now + 0.22);

            // Sharp crack
            const osc2 = ac.createOscillator();
            const gain2 = ac.createGain();
            osc2.connect(gain2);
            gain2.connect(ac.destination);
            osc2.type = 'square';
            osc2.frequency.setValueAtTime(160, now);
            osc2.frequency.exponentialRampToValueAtTime(38, now + 0.09);
            gain2.gain.setValueAtTime(0.28, now);
            gain2.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
            osc2.start(now);
            osc2.stop(now + 0.12);
        } catch (e) {}
    }

    playDeathSound() {
        try {
            const ac = this.audioCtx;
            const now = ac.currentTime;

            // Three descending notes — game-over feel
            const notes = [
                { freq: 440, endFreq: 330, start: 0,    dur: 0.22 },
                { freq: 330, endFreq: 220, start: 0.19, dur: 0.22 },
                { freq: 220, endFreq: 100, start: 0.36, dur: 0.55 },
            ];
            for (let n of notes) {
                const osc = ac.createOscillator();
                const gain = ac.createGain();
                osc.connect(gain);
                gain.connect(ac.destination);
                osc.type = 'sawtooth';
                osc.frequency.setValueAtTime(n.freq, now + n.start);
                osc.frequency.exponentialRampToValueAtTime(n.endFreq, now + n.start + n.dur);
                gain.gain.setValueAtTime(0.32, now + n.start);
                gain.gain.exponentialRampToValueAtTime(0.001, now + n.start + n.dur + 0.05);
                osc.start(now + n.start);
                osc.stop(now + n.start + n.dur + 0.08);
            }
        } catch (e) {}
    }

    die() {
        this.dead = true;
        this.deathTimer = 0;
        this.showDeathScreen = false;
        this.screenShake = 1.0;
        this.playDeathSound();
        if (this.score > this.bestScore) {
            this.bestScore = this.score;
            localStorage.setItem('snakeBestScore', this.bestScore);
        }
        // Create scattered death segments
        this.deathSegments = [];
        let baseR = cellSize * 0.44;
        let tailR = baseR * 0.35;
        let len = Math.max(1, this.snake.length - 1);
        for (let i = 0; i < this.snake.length; i++) {
            let part = this.snake[i];
            let angle = Math.random() * Math.PI * 2;
            let speed = 2 + Math.random() * 4;
            this.deathSegments.push({
                x: part.currentScreenPos.x,
                y: part.currentScreenPos.y,
                vx: Math.cos(angle) * speed,
                vy: Math.sin(angle) * speed,
                r: baseR - (baseR - tailR) * (i / len),
                rotation: 0,
                rotSpeed: (Math.random() - 0.5) * 0.15,
                alpha: 1.0,
                isHead: i === 0
            });
        }
        // Explosion particles at head
        let hx = this.head.currentScreenPos.x;
        let hy = this.head.currentScreenPos.y;
        for (let p = 0; p < 16; p++) {
            let angle = (p / 16) * Math.PI * 2 + Math.random() * 0.3;
            let speed = 2 + Math.random() * 3;
            this.eatParticles.push({
                x: hx, y: hy,
                vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed,
                life: 1.0, color: [NEON.danger, "#ff6a9d", NEON.textAccent, "#8b5dff", "#ffc2ff"][p % 5]
            });
        }
    }

    loseHeart() {
        this.health--;
        this.hitFlash = 1.0;
        this.screenShake = 0.6;
        this.playHitSound();
        // Rock debris particles
        let hx = this.head.currentScreenPos.x;
        let hy = this.head.currentScreenPos.y;
        for (let p = 0; p < 10; p++) {
            let angle = Math.random() * Math.PI * 2;
            let speed = 2 + Math.random() * 3;
            this.hitParticles.push({
                x: hx, y: hy,
                vx: Math.cos(angle) * speed,
                vy: Math.sin(angle) * speed,
                life: 1.0,
                size: 3 + Math.random() * 5,
                color: ["#3d4a7f", "#4f63a1", "#32406e", "#6b7fd1", "#5868a8"][Math.floor(Math.random() * 5)],
                rotation: Math.random() * Math.PI * 2,
                rotSpeed: (Math.random() - 0.5) * 0.3
            });
        }
        if (this.health <= 0) {
            this.die();
        }
    }

    update(deltaTime) {
        // Always update: screen shake, hit flash, particles
        if (this.screenShake > 0) this.screenShake = Math.max(0, this.screenShake - deltaTime / 500);
        if (this.hitFlash > 0) this.hitFlash = Math.max(0, this.hitFlash - deltaTime / 400);

        // Hit particles (rock debris)
        for (let i = this.hitParticles.length - 1; i >= 0; i--) {
            let p = this.hitParticles[i];
            p.x += p.vx;
            p.y += p.vy;
            p.vy += 0.12;
            p.rotation += p.rotSpeed;
            p.life -= deltaTime / 500;
            if (p.life <= 0) {
                this.hitParticles[i] = this.hitParticles[this.hitParticles.length - 1];
                this.hitParticles.pop();
            }
        }

        // Eat/explosion particles
        for (let i = this.eatParticles.length - 1; i >= 0; i--) {
            let p = this.eatParticles[i];
            p.x += p.vx;
            p.y += p.vy;
            p.vy += 0.05;
            p.life -= deltaTime / 600;
            if (p.life <= 0) {
                this.eatParticles[i] = this.eatParticles[this.eatParticles.length - 1];
                this.eatParticles.pop();
            }
        }

        if (this.dead) {
            this.deathTimer += deltaTime;
            let progress = Math.min(1, this.deathTimer / this.deathAnimDuration);
            for (let seg of this.deathSegments) {
                seg.x += seg.vx;
                seg.y += seg.vy;
                seg.vy += 0.06;
                seg.vx *= 0.995;
                seg.rotation += seg.rotSpeed;
                seg.alpha = Math.max(0, 1 - progress * 0.8);
            }
            if (this.deathTimer >= this.deathAnimDuration) {
                this.showDeathScreen = true;
            }
            return;
        }

        let progress = this.stepAccumulator / this.stepInterval;
        for (var i = 0; i < this.snake.length; ++i) {
            var part = this.snake[i];
            part.currentScreenPos.x = part.startScreenPos.x + (part.targetScreenPos.x - part.startScreenPos.x) * progress;
            part.currentScreenPos.y = part.startScreenPos.y + (part.targetScreenPos.y - part.startScreenPos.y) * progress;
        }

        this.stepAccumulator += deltaTime;
        if (this.stepAccumulator >= this.stepInterval) {
            this.stepAccumulator -= this.stepInterval;
            this.gridStep();
        }

        // Camera follows head
        this.camX = this.head.currentScreenPos.x - screenW / 2;
        this.camY = this.head.currentScreenPos.y - screenH / 2;

        if (this.eatBounce > 0) this.eatBounce = Math.max(0, this.eatBounce - deltaTime / 300);

        // Update egg timers and running movement (same interpolation pattern as snake)
        for (let egg of this.food) {
            if (!egg.running) {
                // Sync screen position to grid cell center
                let cc = centerCoords[getIndex(egg.x, egg.y)];
                egg.screenX = cc.x;
                egg.screenY = cc.y;
                // Only tick hatching timer when egg is visible on screen
                let onScreen = cc.x > this.camX - cellSize && cc.x < this.camX + screenW + cellSize &&
                               cc.y > this.camY - cellSize && cc.y < this.camY + screenH + cellSize;
                if (onScreen) {
                    egg.timer -= deltaTime;
                    if (egg.timer <= 0) {
                        egg.running = true;
                        egg.runDir = this._randomRunDir();
                        egg.runAccum = 0;
                        egg.startScreenX = cc.x; egg.startScreenY = cc.y;
                        egg.targetScreenX = cc.x; egg.targetScreenY = cc.y;
                    }
                }
            } else {
                // Interpolate screen position (mirroring snake's pattern)
                let t = egg.runAccum / 500;
                egg.screenX = egg.startScreenX + (egg.targetScreenX - egg.startScreenX) * t;
                egg.screenY = egg.startScreenY + (egg.targetScreenY - egg.startScreenY) * t;

                egg.runAccum += deltaTime;
                if (egg.runAccum >= 500) {
                    egg.runAccum -= 500;
                    egg.startScreenX = egg.targetScreenX;
                    egg.startScreenY = egg.targetScreenY;
                    this._stepEgg(egg);
                    let nc = centerCoords[getIndex(egg.x, egg.y)];
                    egg.targetScreenX = nc.x;
                    egg.targetScreenY = nc.y;
                }
            }
        }
    }

    checkFoodCollision() {
        let eatDistSq = cellSize * cellSize;
        for (let fi = this.food.length - 1; fi >= 0; fi--) {
            let egg = this.food[fi];
            let fx = egg.screenX, fy = egg.screenY;
            let dx = this.head.currentScreenPos.x - fx, dy = this.head.currentScreenPos.y - fy;
            if (dx * dx + dy * dy < eatDistSq) {
                this.score++;
                // Spawn eat particles
                for (let p = 0; p < 8; p++) {
                    let angle = (p / 8) * Math.PI * 2 + Math.random() * 0.5;
                    let speed = 1.5 + Math.random() * 2;
                    this.eatParticles.push({
                        x: fx, y: fy,
                        vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed,
                        life: 1.0, color: ["#ffe080", "#ffb840", "#fff4c0", "#ff9420", NEON.textAccent][p % 5]
                    });
                }
                this.eatBounce = 1.0;
                this.playEatSound();
                this.food.splice(fi, 1);
                let newPart = new SnakeBodyPart();
                let tail = this.snake[this.snake.length - 1];
                newPart.gridPos = tail.gridPos.add(tail.direction.mul(-1));
                let idx = getIndex(newPart.gridPos.x, newPart.gridPos.y);
                newPart.direction.setFrom(tail.direction);
                newPart.targetScreenPos.setFrom(centerCoords[idx]);
                newPart.currentScreenPos.setFrom(newPart.targetScreenPos);
                newPart.startScreenPos.setFrom(newPart.targetScreenPos);
                this.snake.push(newPart);
                this.spawnFood();
            }
        }
    }

    processInput() {
        let processed = false;
        let dir = this.head.direction;

        while (keyQue.length > 0 && !processed) {
            let key = keyQue.shift();
            switch (key) {
                case KEY_UP:
                    if (dir.y !== 1) { this.head.direction = new Vector2(0, -1); processed = true; }
                    break;
                case KEY_DOWN:
                    if (dir.y !== -1) { this.head.direction = new Vector2(0, 1); processed = true; }
                    break;
                case KEY_LEFT:
                    if (dir.x !== 1) { this.head.direction = new Vector2(-1, 0); processed = true; }
                    break;
                case KEY_RIGHT:
                    if (dir.x !== -1) { this.head.direction = new Vector2(1, 0); processed = true; }
                    break;
            }
        }
    }

    gridStep() {
        this.processInput();
        this.checkFoodCollision();

        for (var i = 0; i < this.snake.length; ++i) {
            let part = this.snake[i];
            part.startScreenPos.setFrom(part.targetScreenPos);
            part.gridPos = part.gridPos.add(part.direction);

            // Check wall death (head only)
            if (i === 0) {
                let gx = part.gridPos.x, gy = part.gridPos.y;
                if (gx < 0 || gx >= cols || gy < 0 || gy >= rows) {
                    this.die();
                    return;
                }
                // Check obstacle collision
                for (let oi = this.obstacles.length - 1; oi >= 0; oi--) {
                    if (this.obstacles[oi].x === gx && this.obstacles[oi].y === gy) {
                        this.obstacles.splice(oi, 1);
                        this.loseHeart();
                        if (this.dead) return;
                        break;
                    }
                }
            }

            let idx = getIndex(part.gridPos.x, part.gridPos.y);
            part.targetScreenPos.setFrom(centerCoords[idx]);
        }
        for (var i = this.snake.length - 1; i > 0; --i) {
            this.snake[i].direction.setFrom(this.snake[i - 1].direction);
        }
    }

    drawGrid(ctx) {
        // Only draw cells visible on screen
        let startCol = Math.max(0, Math.floor(this.camX / cellSize));
        let endCol = Math.min(cols, Math.ceil((this.camX + screenW) / cellSize) + 1);
        let startRow = Math.max(0, Math.floor(this.camY / cellSize));
        let endRow = Math.min(rows, Math.ceil((this.camY + screenH) / cellSize) + 1);

        let x0 = startCol * cellSize;
        let y0 = startRow * cellSize;
        let w = (endCol - startCol) * cellSize;
        let h = (endRow - startRow) * cellSize;

        let bg = ctx.createLinearGradient(0, y0, 0, y0 + h);
        bg.addColorStop(0, NEON.gridBase);
        bg.addColorStop(1, "#090f26");
        ctx.fillStyle = bg;
        ctx.fillRect(x0, y0, w, h);

        ctx.strokeStyle = NEON.gridLine;
        ctx.lineWidth = 1;
        for (let c = startCol; c <= endCol; c++) {
            let x = c * cellSize + 0.5;
            ctx.beginPath();
            ctx.moveTo(x, y0);
            ctx.lineTo(x, y0 + h);
            ctx.stroke();
        }
        for (let r = startRow; r <= endRow; r++) {
            let y = r * cellSize + 0.5;
            ctx.beginPath();
            ctx.moveTo(x0, y);
            ctx.lineTo(x0 + w, y);
            ctx.stroke();
        }

        // Draw world border
        ctx.save();
        ctx.strokeStyle = NEON.border;
        ctx.lineWidth = 4;
        ctx.shadowColor = NEON.borderGlow;
        ctx.shadowBlur = 22;
        ctx.strokeRect(0, 0, W, H);
        ctx.restore();
    }

    drawObstacles(ctx) {
        let pad = cellSize * 0.08;
        let s = cellSize - pad * 2;
        for (let o of this.obstacles) {
            let ox = o.x * cellSize + pad;
            let oy = o.y * cellSize + pad;
            // Skip if off screen
            if (ox + s < this.camX - cellSize || ox > this.camX + screenW + cellSize) continue;
            if (oy + s < this.camY - cellSize || oy > this.camY + screenH + cellSize) continue;

            let rockPath = () => {
                ctx.beginPath();
                ctx.moveTo(ox + s * 0.2, oy + s);
                ctx.lineTo(ox, oy + s * 0.5);
                ctx.lineTo(ox + s * 0.15, oy + s * 0.15);
                ctx.lineTo(ox + s * 0.5, oy);
                ctx.lineTo(ox + s * 0.85, oy + s * 0.1);
                ctx.lineTo(ox + s, oy + s * 0.45);
                ctx.lineTo(ox + s * 0.8, oy + s);
                ctx.closePath();
            };

            let rockGrad = ctx.createLinearGradient(ox, oy, ox + s, oy + s);
            rockGrad.addColorStop(0, "#2a3b74");
            rockGrad.addColorStop(1, NEON.obstacleBase);

            ctx.save();
            ctx.shadowColor = NEON.obstacleGlow;
            ctx.shadowBlur = 12;
            ctx.fillStyle = rockGrad;
            rockPath();
            ctx.fill();
            ctx.restore();

            ctx.strokeStyle = NEON.obstacleEdge;
            ctx.lineWidth = 1.5;
            rockPath();
            ctx.stroke();

            ctx.strokeStyle = "rgba(170,200,255,0.35)";
            ctx.lineWidth = 1;
            ctx.beginPath();
            ctx.moveTo(ox + s * 0.22, oy + s * 0.35);
            ctx.lineTo(ox + s * 0.58, oy + s * 0.5);
            ctx.lineTo(ox + s * 0.74, oy + s * 0.72);
            ctx.stroke();
        }
    }

    drawEggs(ctx) {
        const now = Date.now();
        for (let egg of this.food) {
            let cx = egg.screenX;
            let cy = egg.screenY;

            // Off-screen culling
            if (cx + cellSize * 1.5 < this.camX || cx - cellSize * 1.5 > this.camX + screenW) continue;
            if (cy + cellSize * 1.5 < this.camY || cy - cellSize * 1.5 > this.camY + screenH) continue;

            let bob = egg.running ? 0 : Math.sin(now * 0.003 + egg.x * 2.1 + egg.y * 3.7) * 2.5;
            let dy = cy + bob;
            let er = cellSize * 0.22;
            let eh = cellSize * 0.3;
            let walkPhase = (now * 0.007) % (Math.PI * 2);

            // Legs drawn behind egg
            if (egg.running) {
                for (let si = 0; si < 2; si++) {
                    let side = si === 0 ? -1 : 1;
                    let extend = 0.65 + 0.35 * Math.sin(walkPhase + (si === 0 ? 0 : Math.PI));
                    let lx = cx + side * er * 0.5;
                    let ly = dy + eh * 0.7;
                    let lw = er * 0.48;
                    let lh = eh * 0.72 * extend;

                    ctx.fillStyle = "#ffdc96";
                    ctx.fillRect(lx - lw / 2, ly, lw, lh);

                    // Foot
                    let footDirX = egg.runDir.x !== 0 ? egg.runDir.x * er * 0.2 : 0;
                    ctx.fillStyle = "#ffb855";
                    ctx.beginPath();
                    ctx.ellipse(lx + footDirX, ly + lh, lw * 0.95, lw * 0.52, 0, 0, Math.PI * 2);
                    ctx.fill();
                }
            }

            // Glow aura
            let aura = ctx.createRadialGradient(cx, dy, er * 0.1, cx, dy, er * 2.6);
            aura.addColorStop(0, egg.running ? "rgba(255,195,50,0.55)" : "rgba(90,255,215,0.45)");
            aura.addColorStop(1, "rgba(0,0,0,0)");
            ctx.fillStyle = aura;
            ctx.beginPath();
            ctx.ellipse(cx, dy, er * 2.4, eh * 2.4, 0, 0, Math.PI * 2);
            ctx.fill();

            // Egg body
            let eggGrad = ctx.createRadialGradient(cx - er * 0.3, dy - eh * 0.32, eh * 0.05, cx, dy, eh * 1.15);
            if (egg.running) {
                eggGrad.addColorStop(0, "#fff8e0");
                eggGrad.addColorStop(0.55, "#ffe070");
                eggGrad.addColorStop(1, "#ff9020");
            } else {
                eggGrad.addColorStop(0, "#eefffa");
                eggGrad.addColorStop(0.55, "#88f0d8");
                eggGrad.addColorStop(1, "#28b09a");
            }
            ctx.save();
            ctx.shadowColor = egg.running ? "rgba(255,160,30,0.85)" : "rgba(60,220,185,0.8)";
            ctx.shadowBlur = 18;
            ctx.fillStyle = eggGrad;
            ctx.beginPath();
            ctx.ellipse(cx, dy, er, eh, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.restore();

            // Edge outline
            ctx.strokeStyle = egg.running ? "rgba(255,200,80,0.9)" : "rgba(110,255,225,0.8)";
            ctx.lineWidth = 1.5;
            ctx.beginPath();
            ctx.ellipse(cx, dy, er, eh, 0, 0, Math.PI * 2);
            ctx.stroke();

            // Speckles
            const speckles = [[-0.35, -0.18, 0.18, 0.09, 0.4], [0.38, 0.2, 0.13, 0.07, -0.6], [-0.1, 0.35, 0.1, 0.06, 0.9]];
            ctx.fillStyle = egg.running ? "rgba(175,85,0,0.28)" : "rgba(25,135,115,0.28)";
            for (let [ox, oy, rx, ry, a] of speckles) {
                ctx.beginPath();
                ctx.ellipse(cx + ox * er, dy + oy * eh, rx * er, ry * eh, a, 0, Math.PI * 2);
                ctx.fill();
            }

            // Highlight
            ctx.fillStyle = "rgba(255,255,255,0.58)";
            ctx.beginPath();
            ctx.ellipse(cx - er * 0.28, dy - eh * 0.32, er * 0.38, eh * 0.2, -0.3, 0, Math.PI * 2);
            ctx.fill();

            // Eyes when running (face toward movement direction)
            if (egg.running) {
                let rd = egg.runDir;
                let eyeR = er * 0.165;
                let eyeCx = cx + rd.x * er * 0.38;
                let eyeCy = dy + rd.y * eh * 0.38;
                let perpX = -rd.y;
                let perpY = rd.x;
                for (let side = -1; side <= 1; side += 2) {
                    let ex = eyeCx + perpX * er * 0.28 * side;
                    let ey2 = eyeCy + perpY * eh * 0.28 * side;
                    ctx.fillStyle = "rgba(248,252,255,0.95)";
                    ctx.beginPath();
                    ctx.arc(ex, ey2, eyeR, 0, Math.PI * 2);
                    ctx.fill();
                    ctx.fillStyle = "#18103a";
                    ctx.beginPath();
                    ctx.arc(ex + rd.x * eyeR * 0.22, ey2 + rd.y * eyeR * 0.22, eyeR * 0.55, 0, Math.PI * 2);
                    ctx.fill();
                    ctx.fillStyle = "rgba(255,255,255,0.88)";
                    ctx.beginPath();
                    ctx.arc(ex - eyeR * 0.2, ey2 - eyeR * 0.2, eyeR * 0.24, 0, Math.PI * 2);
                    ctx.fill();
                }
            }

            // Countdown timer ring
            if (!egg.running) {
                let frac = Math.max(0, egg.timer / 5000);
                let ringR = eh * 1.48;
                ctx.strokeStyle = "rgba(60,200,175,0.2)";
                ctx.lineWidth = 2.2;
                ctx.beginPath();
                ctx.arc(cx, dy, ringR, 0, Math.PI * 2);
                ctx.stroke();
                let tColor = frac > 0.6 ? "#38ffda" : frac > 0.3 ? "#ffd050" : "#ff6830";
                ctx.save();
                ctx.strokeStyle = tColor;
                ctx.shadowColor = tColor;
                ctx.shadowBlur = 10;
                ctx.lineWidth = 2.2;
                ctx.lineCap = "round";
                ctx.beginPath();
                ctx.arc(cx, dy, ringR, -Math.PI / 2, -Math.PI / 2 + frac * Math.PI * 2);
                ctx.stroke();
                ctx.restore();
            }
        }
    }

    getNearestFoodPos() {
        let hx = this.head.currentScreenPos.x;
        let hy = this.head.currentScreenPos.y;
        let bestDist = Infinity;
        let bestPos = null;
        for (let f of this.food) {
            let d = (f.screenX - hx) * (f.screenX - hx) + (f.screenY - hy) * (f.screenY - hy);
            if (d < bestDist) { bestDist = d; bestPos = { x: f.screenX, y: f.screenY }; }
        }
        return bestPos;
    }

    drawSnakeBody(ctx) {
        let baseR = cellSize * 0.44;
        let tailR = baseR * 0.35;
        let bodyPoints = [];
        for (let i = this.snake.length - 1; i >= 0; i--) {
            bodyPoints.push(this.snake[i].currentScreenPos);
        }
        if (bodyPoints.length < 2) return;

        let traceBodyPath = () => {
            ctx.beginPath();
            ctx.moveTo(bodyPoints[0].x, bodyPoints[0].y);
            for (let i = 1; i < bodyPoints.length - 1; i++) {
                let p = bodyPoints[i];
                let n = bodyPoints[i + 1];
                let mx = (p.x + n.x) * 0.5;
                let my = (p.y + n.y) * 0.5;
                ctx.quadraticCurveTo(p.x, p.y, mx, my);
            }
            let headJoin = bodyPoints[bodyPoints.length - 1];
            ctx.lineTo(headJoin.x, headJoin.y);
        };

        let tail = bodyPoints[0];
        let nearTail = bodyPoints[1];
        let tailAngle = Math.atan2(nearTail.y - tail.y, nearTail.x - tail.x);
        let bodyGrad = ctx.createLinearGradient(tail.x, tail.y, this.head.currentScreenPos.x, this.head.currentScreenPos.y);
        bodyGrad.addColorStop(0, NEON.snakeSecondary);
        bodyGrad.addColorStop(0.55, "#4fb9ff");
        bodyGrad.addColorStop(1, NEON.snakePrimary);

        ctx.lineCap = "round";
        ctx.lineJoin = "round";

        ctx.save();
        ctx.strokeStyle = "rgba(0,0,0,0.25)";
        ctx.lineWidth = baseR * 2.2;
        ctx.translate(1.5, 2);
        traceBodyPath();
        ctx.stroke();
        ctx.restore();

        ctx.save();
        ctx.shadowColor = NEON.snakeGlow;
        ctx.shadowBlur = 20;
        ctx.strokeStyle = bodyGrad;
        ctx.lineWidth = baseR * 2.05;
        traceBodyPath();
        ctx.stroke();
        ctx.restore();

        ctx.strokeStyle = "rgba(170, 238, 255, 0.7)";
        ctx.lineWidth = baseR * 1.12;
        traceBodyPath();
        ctx.stroke();

        ctx.strokeStyle = "rgba(255,255,255,0.24)";
        ctx.lineWidth = baseR * 0.48;
        ctx.save();
        ctx.translate(0, -baseR * 0.14);
        traceBodyPath();
        ctx.stroke();
        ctx.restore();

        ctx.save();
        ctx.translate(tail.x, tail.y);
        ctx.rotate(tailAngle);
        let tailGrad = ctx.createRadialGradient(-tailR * 0.5, -tailR * 0.2, tailR * 0.2, 0, 0, tailR * 1.4);
        tailGrad.addColorStop(0, "#9bb6ff");
        tailGrad.addColorStop(1, NEON.snakeSecondary);
        ctx.fillStyle = tailGrad;
        ctx.beginPath();
        ctx.ellipse(0, 0, tailR * 1.2, tailR * 0.9, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.restore();
    }

    drawHead(ctx) {
        let pos = this.head.currentScreenPos;
        let r = cellSize * 0.48;
        let bounce = Math.sin(this.eatBounce * Math.PI) * 4;
        let hr = r + bounce;

        let dir = this.head.direction;
        let fx = dir.x, fy = dir.y;
        let angle = Math.atan2(fy, fx);

        // Pointy teardrop head path in local space (+x = forward)
        const headPath = () => {
            ctx.beginPath();
            ctx.moveTo(hr * 0.92, 0);
            ctx.bezierCurveTo(hr * 0.72, -hr * 0.62, hr * 0.02, -hr * 0.9, -hr * 0.38, -hr * 0.76);
            ctx.bezierCurveTo(-hr * 0.84, -hr * 0.46, -hr * 0.84, hr * 0.46, -hr * 0.38, hr * 0.76);
            ctx.bezierCurveTo(hr * 0.02, hr * 0.9, hr * 0.72, hr * 0.62, hr * 0.92, 0);
            ctx.closePath();
        };

        // Drop shadow
        ctx.save();
        ctx.translate(pos.x + 2, pos.y + 3);
        ctx.rotate(angle);
        ctx.fillStyle = "rgba(0,0,0,0.28)";
        headPath();
        ctx.fill();
        ctx.restore();

        ctx.save();
        ctx.translate(pos.x, pos.y);
        ctx.rotate(angle);

        // Body gradient fill
        let headGrad = ctx.createLinearGradient(-hr * 0.5, -hr * 0.55, hr * 0.6, hr * 0.4);
        headGrad.addColorStop(0, "#aafdff");
        headGrad.addColorStop(0.4, "#38baff");
        headGrad.addColorStop(1, "#1862ee");
        ctx.save();
        ctx.shadowColor = NEON.snakeGlow;
        ctx.shadowBlur = 26;
        ctx.fillStyle = headGrad;
        headPath();
        ctx.fill();
        ctx.restore();

        // Neon edge
        ctx.strokeStyle = "rgba(170, 238, 255, 0.7)";
        ctx.lineWidth = 1.4;
        headPath();
        ctx.stroke();

        // Glossy top highlight
        ctx.fillStyle = "rgba(255,255,255,0.3)";
        ctx.beginPath();
        ctx.ellipse(-hr * 0.14, -hr * 0.36, hr * 0.4, hr * 0.17, -0.35, 0, Math.PI * 2);
        ctx.fill();

        // Cheek blushes (cute)
        ctx.fillStyle = "rgba(255, 145, 215, 0.28)";
        ctx.beginPath();
        ctx.ellipse(hr * 0.14, -hr * 0.54, hr * 0.14, hr * 0.09, 0, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.ellipse(hr * 0.14, hr * 0.54, hr * 0.14, hr * 0.09, 0, 0, Math.PI * 2);
        ctx.fill();

        // Eyes — on the sides, big and round for cuteness
        let foodPos = this.getNearestFoodPos();
        let eyeR = hr * 0.21;
        let eyeX = hr * 0.06;
        let eyeY = hr * 0.5;

        for (let side = -1; side <= 1; side += 2) {
            let ex = eyeX;
            let ey = eyeY * side;

            // Sclera
            ctx.fillStyle = "rgba(238,255,255,0.98)";
            ctx.beginPath();
            ctx.arc(ex, ey, eyeR, 0, Math.PI * 2);
            ctx.fill();

            ctx.strokeStyle = "rgba(130, 195, 255, 0.8)";
            ctx.lineWidth = 1.2;
            ctx.beginPath();
            ctx.arc(ex, ey, eyeR, 0, Math.PI * 2);
            ctx.stroke();

            // Pupil tracking food
            let lookX = 1, lookY = 0;
            if (foodPos) {
                let dx = foodPos.x - pos.x;
                let dy = foodPos.y - pos.y;
                let lx = dx * fx + dy * fy;
                let ly = dx * (-fy) + dy * fx;
                let d = Math.sqrt(lx * lx + ly * ly);
                if (d > 0.01) { lookX = lx / d; lookY = ly / d; }
            }
            let pupilX = ex + lookX * eyeR * 0.28;
            let pupilY = ey + lookY * eyeR * 0.28;

            // Iris
            ctx.fillStyle = "#2a2060";
            ctx.beginPath();
            ctx.arc(pupilX, pupilY, eyeR * 0.48, 0, Math.PI * 2);
            ctx.fill();

            // Sparkle
            ctx.fillStyle = "rgba(255,255,255,0.92)";
            ctx.beginPath();
            ctx.arc(pupilX - eyeR * 0.16, pupilY - eyeR * 0.14, eyeR * 0.18, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = "rgba(255,255,255,0.5)";
            ctx.beginPath();
            ctx.arc(pupilX + eyeR * 0.1, pupilY + eyeR * 0.14, eyeR * 0.08, 0, Math.PI * 2);
            ctx.fill();
        }

        // Tiny nostrils near the tip
        ctx.fillStyle = "rgba(25, 45, 105, 0.5)";
        ctx.beginPath();
        ctx.arc(hr * 0.46, -hr * 0.1, hr * 0.042, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(hr * 0.46, hr * 0.1, hr * 0.042, 0, Math.PI * 2);
        ctx.fill();

        // Mouth — spans y-axis (across the snout, perpendicular to movement)
        if (this.eatBounce > 0.3) {
            ctx.fillStyle = "#ff60bc";
            ctx.beginPath();
            // taller than wide so it reads as a vertical open mouth — 4× enlarged
            ctx.ellipse(hr * 0.76, 0, hr * 0.32, hr * 0.60, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = "#ffd8f4";
            ctx.beginPath();
            ctx.arc(hr * 0.76, 0, hr * 0.152, 0, Math.PI * 2);
            ctx.fill();
        }

        // Forked tongue flick from the pointed tip
        if (this.eatBounce < 0.15) {
            let tonguePhase = (Date.now() % 1800) / 1800;
            if (tonguePhase < 0.1) {
                let tLen = hr * 0.24 * (1 + Math.sin((tonguePhase / 0.1) * Math.PI));
                let baseX = hr * 0.9;
                ctx.strokeStyle = "#ff7ed4";
                ctx.lineWidth = 1.7;
                ctx.lineCap = "round";
                ctx.beginPath();
                ctx.moveTo(baseX, 0);
                ctx.lineTo(baseX + tLen, 0);
                ctx.lineTo(baseX + tLen + hr * 0.07, -hr * 0.055);
                ctx.moveTo(baseX + tLen, 0);
                ctx.lineTo(baseX + tLen + hr * 0.07, hr * 0.055);
                ctx.stroke();
            }
        }

        ctx.restore();
    }

    drawParticles(ctx) {
        for (let p of this.eatParticles) {
            let size = 4 * p.life;
            ctx.globalAlpha = p.life;
            ctx.fillStyle = p.color;
            ctx.shadowColor = p.color;
            ctx.shadowBlur = 14;

            // Star shape
            ctx.beginPath();
            for (let s = 0; s < 5; s++) {
                let a = (s / 5) * Math.PI * 2 - Math.PI / 2;
                let ox = Math.cos(a) * size;
                let oy = Math.sin(a) * size;
                if (s === 0) ctx.moveTo(p.x + ox, p.y + oy);
                else ctx.lineTo(p.x + ox, p.y + oy);
                let ia = a + Math.PI / 5;
                ctx.lineTo(p.x + Math.cos(ia) * size * 0.4, p.y + Math.sin(ia) * size * 0.4);
            }
            ctx.closePath();
            ctx.fill();
        }
        ctx.globalAlpha = 1;
        ctx.shadowBlur = 0;
    }

    drawHitParticles(ctx) {
        for (let p of this.hitParticles) {
            ctx.globalAlpha = p.life;
            ctx.fillStyle = p.color;
            ctx.save();
            ctx.translate(p.x, p.y);
            ctx.rotate(p.rotation);
            ctx.shadowColor = p.color;
            ctx.shadowBlur = 10;
            let s = p.size;
            ctx.beginPath();
            ctx.moveTo(-s, -s * 0.6);
            ctx.lineTo(-s * 0.3, -s);
            ctx.lineTo(s * 0.6, -s * 0.8);
            ctx.lineTo(s, -s * 0.1);
            ctx.lineTo(s * 0.7, s * 0.8);
            ctx.lineTo(-s * 0.5, s);
            ctx.closePath();
            ctx.fill();
            ctx.restore();
        }
        ctx.globalAlpha = 1;
        ctx.shadowBlur = 0;
    }

    drawDeathSegments(ctx) {
        let progress = Math.min(1, this.deathTimer / this.deathAnimDuration);
        let flashRate = Math.max(3, 15 - progress * 12);
        let flashOn = Math.sin(this.deathTimer * 0.01 * flashRate) > 0;

        for (let i = this.deathSegments.length - 1; i >= 0; i--) {
            let seg = this.deathSegments[i];
            if (seg.alpha <= 0) continue;

            ctx.globalAlpha = seg.alpha;
            ctx.save();
            ctx.translate(seg.x, seg.y);
            ctx.rotate(seg.rotation);

            let r = seg.r * (1 - progress * 0.4);

            // Shadow
            ctx.fillStyle = "rgba(0,0,0,0.22)";
            ctx.beginPath();
            ctx.arc(1.5, 2, r, 0, Math.PI * 2);
            ctx.fill();

            // Flash danger/electric cyan
            let baseColor = i % 2 === 0 ? NEON.snakePrimary : NEON.snakeSecondary;
            ctx.fillStyle = flashOn ? NEON.danger : baseColor;
            ctx.shadowColor = flashOn ? NEON.dangerGlow : NEON.snakeGlow;
            ctx.shadowBlur = 14;
            ctx.beginPath();
            ctx.arc(0, 0, r, 0, Math.PI * 2);
            ctx.fill();
            ctx.shadowBlur = 0;

            // Highlight
            ctx.fillStyle = "rgba(255,255,255,0.35)";
            ctx.beginPath();
            ctx.arc(-r * 0.25, -r * 0.25, r * 0.35, 0, Math.PI * 2);
            ctx.fill();

            // X eyes on head segment
            if (seg.isHead && progress < 0.6) {
                let eyeOff = r * 0.25;
                ctx.strokeStyle = "rgba(255, 220, 245, 0.9)";
                ctx.lineWidth = 2.5;
                ctx.lineCap = "round";
                for (let side = -1; side <= 1; side += 2) {
                    let ex = eyeOff * side;
                    let ey = -r * 0.1;
                    let xs = r * 0.15;
                    ctx.beginPath();
                    ctx.moveTo(ex - xs, ey - xs);
                    ctx.lineTo(ex + xs, ey + xs);
                    ctx.moveTo(ex + xs, ey - xs);
                    ctx.lineTo(ex - xs, ey + xs);
                    ctx.stroke();
                }
            }

            ctx.restore();
        }
        ctx.globalAlpha = 1;
    }

    drawHUD(ctx) {
        // HUD background bar
        let hudGrad = ctx.createLinearGradient(0, 0, 0, hudH);
        hudGrad.addColorStop(0, NEON.hudBgTop);
        hudGrad.addColorStop(1, NEON.hudBgBottom);
        ctx.fillStyle = hudGrad;
        ctx.fillRect(0, 0, screenW, hudH);
        ctx.fillStyle = NEON.hudLine;
        ctx.fillRect(0, hudH - 2, screenW, 2);

        // Score (left)
        ctx.fillStyle = NEON.textPrimary;
        ctx.shadowColor = NEON.snakeGlow;
        ctx.shadowBlur = 10;
        ctx.font = "bold 22px monospace";
        ctx.textAlign = "left";
        ctx.fillText("Score: " + this.score, 15, 33);
        ctx.shadowBlur = 0;

        // Hearts (center)
        let heartSize = 14;
        let heartGap = 36;
        let heartsW = 3 * heartGap;
        let hx0 = (screenW - heartsW) / 2 + heartGap / 2;
        let hy = 26;
        for (let i = 0; i < 3; i++) {
            let cx = hx0 + i * heartGap;
            if (i < this.health) {
                this.drawHeart(ctx, cx, hy, heartSize, NEON.heartOn);
            } else {
                this.drawHeart(ctx, cx, hy, heartSize, NEON.heartOff);
            }
        }

        // Best score (right)
        ctx.fillStyle = NEON.textMuted;
        ctx.font = "18px monospace";
        ctx.textAlign = "right";
        ctx.fillText("Best: " + this.bestScore, screenW - 15, 33);
        ctx.textAlign = "left";
    }

    drawHeart(ctx, cx, cy, size, color) {
        if (color !== NEON.heartOff) {
            ctx.save();
            ctx.shadowColor = "rgba(255, 77, 226, 0.75)";
            ctx.shadowBlur = 12;
        }
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.moveTo(cx, cy + size * 0.35);
        ctx.bezierCurveTo(cx, cy - size * 0.2, cx - size, cy - size * 0.6, cx - size, cy + size * 0.05);
        ctx.bezierCurveTo(cx - size, cy + size * 0.55, cx, cy + size * 0.9, cx, cy + size * 1.1);
        ctx.bezierCurveTo(cx, cy + size * 0.9, cx + size, cy + size * 0.55, cx + size, cy + size * 0.05);
        ctx.bezierCurveTo(cx + size, cy - size * 0.6, cx, cy - size * 0.2, cx, cy + size * 0.35);
        ctx.closePath();
        ctx.fill();
        if (color !== NEON.heartOff) ctx.restore();

        // Highlight on filled hearts
        if (color !== NEON.heartOff) {
            ctx.fillStyle = "rgba(255,255,255,0.45)";
            ctx.beginPath();
            ctx.arc(cx - size * 0.35, cy + size * 0.05, size * 0.25, 0, Math.PI * 2);
            ctx.fill();
        }
    }

    drawDeathScreen(ctx) {
        let overlay = ctx.createLinearGradient(0, hudH, 0, screenH);
        overlay.addColorStop(0, "rgba(7,12,31,0.7)");
        overlay.addColorStop(1, "rgba(2,4,13,0.8)");
        ctx.fillStyle = overlay;
        ctx.fillRect(0, hudH, screenW, screenH - hudH);
        ctx.fillStyle = NEON.danger;
        ctx.shadowColor = NEON.dangerGlow;
        ctx.shadowBlur = 18;
        ctx.font = "bold 48px monospace";
        ctx.textAlign = "center";
        let cy = hudH + (screenH - hudH) / 2;
        ctx.fillText("Game Over", screenW / 2, cy - 30);
        ctx.shadowBlur = 0;
        ctx.fillStyle = "#f2f8ff";
        ctx.font = "24px monospace";
        ctx.fillText("Score: " + this.score, screenW / 2, cy + 15);
        if (this.score >= this.bestScore && this.score > 0) {
            ctx.fillStyle = NEON.textAccent;
            ctx.font = "20px monospace";
            ctx.fillText("New Best!", screenW / 2, cy + 45);
        }
        ctx.font = "18px monospace";
        ctx.fillStyle = NEON.textMuted;
        ctx.fillText("Press SPACE to restart", screenW / 2, cy + 80);
        ctx.textAlign = "start";
    }

    draw(ctx) {
        ctx.clearRect(0, 0, screenW, screenH);
        // Background outside world
        let bg = ctx.createLinearGradient(0, 0, 0, screenH);
        bg.addColorStop(0, NEON.bgOuter);
        bg.addColorStop(1, NEON.bgInner);
        ctx.fillStyle = bg;
        ctx.fillRect(0, 0, screenW, screenH);

        let vignette = ctx.createRadialGradient(screenW * 0.5, screenH * 0.55, screenW * 0.2, screenW * 0.5, screenH * 0.55, screenW * 0.85);
        vignette.addColorStop(0, "rgba(0,0,0,0)");
        vignette.addColorStop(1, "rgba(0,0,0,0.38)");
        ctx.fillStyle = vignette;
        ctx.fillRect(0, 0, screenW, screenH);

        // Screen shake offset
        let shakeX = 0, shakeY = 0;
        if (this.screenShake > 0) {
            let intensity = this.screenShake * 8;
            shakeX = (Math.random() - 0.5) * intensity * 2;
            shakeY = (Math.random() - 0.5) * intensity * 2;
        }

        ctx.save();
        ctx.translate(-this.camX + shakeX, -this.camY + hudH + shakeY);

        this.drawGrid(ctx);
        this.drawObstacles(ctx);
        this.drawEggs(ctx);

        if (this.dead && this.deathSegments.length > 0) {
            this.drawDeathSegments(ctx);
        } else {
            this.drawSnakeBody(ctx);
            this.drawHead(ctx);
        }

        this.drawParticles(ctx);
        this.drawHitParticles(ctx);

        ctx.restore();

        // Hit flash overlay
        if (this.hitFlash > 0) {
            ctx.fillStyle = `rgba(255, 63, 114, ${this.hitFlash * 0.3})`;
            ctx.fillRect(0, hudH, screenW, screenH - hudH);
        }

        // HUD (screen-space)
        this.drawHUD(ctx);
        if (this.showDeathScreen) this.drawDeathScreen(ctx);
    }
}

function init() {
    let canvas = document.createElement("canvas");
    canvas.width = screenW;
    canvas.height = screenH;
    document.body.appendChild(canvas);
    let ctx = canvas.getContext("2d");

    for (let y = 0; y < rows; ++y) {
        for (let x = 0; x < cols; ++x) {
            let idx = y * cols + x;
            centerCoords[idx] = new Vector2(x * cellSize + cellSize * 0.5, y * cellSize + cellSize * 0.5);
        }
    }

    let game = new Game();

    window.addEventListener("keydown", function (e) {
        if (handledKeys.indexOf(e.keyCode) >= 0) {
            e.preventDefault();
            keyQue.push(e.keyCode);
        }
        if (e.keyCode === 32 && game.showDeathScreen) {
            keyQue = [];
            game = new Game();
        }
    });

    let lastTime = null;
    var mainLoop = function (timestamp) {
        if (lastTime === null) lastTime = timestamp;
        let deltaTime = timestamp - lastTime;
        lastTime = timestamp;
        game.update(deltaTime);
        game.draw(ctx);
        requestAnimationFrame(mainLoop);
    }
    requestAnimationFrame(mainLoop);
}
