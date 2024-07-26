class Cell {
  constructor(x, y, interp = 0.0) {
    this.x = x;
    this.y = y;
    this.animX = x; // Animated position
    this.animY = y;
    this.interp = interp; // Position interpolation for animation
  }

  // New target position arrived, reset interpolation
  resetInterp() {
    this.interp = 0.0;
  }

  // Interpolate between current position towards target position
  interpolate(targetX, targetY, deltaTime) {
    this.interp += deltaTime * gameSpeed;
    this.interp = Math.min(1.0, this.interp); // clamp max value at 1.0

    let t = this.f(this.interp);
    this.animX = this.lerp(this.x, targetX, t);
    this.animY = this.lerp(this.y, targetY, t);
  }

  f(t) {
    return t;
  }

  lerp(a, b, t) {
    return a + t * (b - a);
  }
}

// Game setup
const canvas = document.getElementById("canvas");
const ctx = canvas.getContext("2d");
const cellSize = 20;
let snakeTrail = [
  new Cell(120, cellSize),
  new Cell(120 - cellSize, cellSize),
  new Cell(120 - cellSize * 2, cellSize),
];
let direction = { x: 1, y: 0 };
let gameSpeed = 5;
let score = 0;
let food = { x: cellSize * 3, y: cellSize * 3 };

// Calculate next position of the snake
let nextSnakePosition = getNextSnakePosition(snakeTrail);

function getNextSnakePosition(snakeTrail) {
  const nextSnakePosition = [];
  for (let i = 0; i < snakeTrail.length; i++) {
    nextSnakePosition.push(
      new Cell(
        snakeTrail[i].x + direction.x * cellSize,
        snakeTrail[i].y + direction.y * cellSize
      )
    );
  }
  return nextSnakePosition;
}

function gameLoop(timeStamp) {
  if (!lastRender) lastRender = timeStamp;
  const deltaTime = (timeStamp - lastRender) / 1000;
  //const deltaTime = Math.min(1.0 / 60, (Date.now() - lastRender) / 1000); // Limit deltaTime to avoid 'jumps' in snake's movement
  lastRender = timeStamp;

  if (snakeTrail[0].interp >= 1.0) {
    snakeStep();
    if (snakeTrail[0].x === food.x && snakeTrail[0].y === food.y) {
      eatFood();
    }
  }

  for (let i = 0; i < snakeTrail.length; i++) {
    snakeTrail[i].interpolate(
      nextSnakePosition[i].x,
      nextSnakePosition[i].y,
      deltaTime
    );
  }

  draw();

  requestAnimationFrame(gameLoop);
}

function snakeStep() {
  for (let i = snakeTrail.length - 1; i > 0; --i) {
    snakeTrail[i] = snakeTrail[i - 1];
    nextSnakePosition[i] = nextSnakePosition[i - 1];
    snakeTrail[i].resetInterp();
  }
  snakeTrail[0] = new Cell(nextSnakePosition[0].x, nextSnakePosition[0].y);
  nextSnakePosition[0] = getNextHead();
}

function getNextHead() {
  return new Cell(
    nextSnakePosition[0].x + direction.x * cellSize,
    nextSnakePosition[0].y + direction.y * cellSize
  );
}

function eatFood() {
  // add cell length
  const head = snakeTrail[0];
  const nextHead = nextSnakePosition[0];
  snakeTrail.unshift(new Cell(head.x, head.y));
  nextSnakePosition.unshift(new Cell(nextHead.x, nextHead.y));

  // reposition food
  food.x = Math.floor((Math.random() * canvas.width) / cellSize) * cellSize;
  food.y = Math.floor((Math.random() * canvas.height) / cellSize) * cellSize;

  score += 1;
}

function draw() {
  ctx.fillStyle = "black";
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.fillStyle = "red";
  ctx.fillRect(food.x, food.y, cellSize, cellSize);

  for (let i = 0; i < snakeTrail.length; i++) {
    ctx.fillStyle = i === 0 ? "green" : "white";
    ctx.fillRect(snakeTrail[i].animX, snakeTrail[i].animY, cellSize, cellSize);
  }
}

// Snake direction controls
window.addEventListener("keydown", function (e) {
  switch (e.key) {
    case "ArrowUp":
      direction = { x: 0, y: -1 };
      break;
    case "ArrowDown":
      direction = { x: 0, y: 1 };
      break;
    case "ArrowLeft":
      direction = { x: -1, y: 0 };
      break;
    case "ArrowRight":
      direction = { x: 1, y: 0 };
      break;
  }
  //nextSnakePosition[0] = getNextHead();
});

let lastRender = undefined;
requestAnimationFrame(gameLoop);
