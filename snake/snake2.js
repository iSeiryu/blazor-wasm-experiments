const sun = new Image();
const moon = new Image();
const earth = new Image();
function init() {
  sun.src =
    "https://live.mdnplay.dev/en-US/docs/Web/API/Canvas_API/Tutorial/Basic_animations/canvas_sun.png";
  moon.src =
    "https://live.mdnplay.dev/en-US/docs/Web/API/Canvas_API/Tutorial/Basic_animations/canvas_moon.png";
  earth.src =
    "https://live.mdnplay.dev/en-US/docs/Web/API/Canvas_API/Tutorial/Basic_animations/canvas_earth.png";
  window.requestAnimationFrame(draw);
}

// grid dimension
var rows = 10;
var cols = 10;

// button-like rectangle dimensions and styles
var rectWidth = 30;
var rectHeight = 30;
var padding = 5; // space between rectangles
var lineWidth = 1; // border width

const ctx = document.getElementById("canvas").getContext("2d");
ctx.strokeStyle = "grey"; // border color
ctx.fillStyle = "green"; // fill color
ctx.shadowColor = "black"; // shadow color
ctx.shadowOffsetX = 2; // horizontal shadow offset
ctx.shadowOffsetY = 2; // vertical shadow offset
ctx.shadowBlur = 5; // shadow blur level

function drawGrid() {
  // Create the grid
  for (var i = 0; i < rows; i++) {
    for (var j = 0; j < cols; j++) {
      var x = i * (rectWidth + padding);
      var y = j * (rectHeight + padding);
      rectangles.push({
        // add each button rectangle to the array
        x: x,
        y: y,
        isPointInside: function (px, py) {
          // method to check if a point lies inside the rectangle
          return (
            px >= this.x &&
            px <= this.x + rectWidth &&
            py >= this.y &&
            py <= this.y + rectHeight
          );
        },
      });
      ctx.fillStyle = "green";
      createRect(ctx, x, y, rectWidth, rectHeight);
    }
  }
}

// Create an array to hold button rectangles
var rectangles = [];

function createRect(ctx, x, y, rectWidth, rectHeight) {
  ctx.fillRect(x, y, rectWidth, rectHeight);
  ctx.strokeRect(x, y, rectWidth, rectHeight);
}

// Track the mouse position on move
canvas.addEventListener("mousemove", function (e) {
  ctx.clearRect(0, 0, canvas.width, canvas.height); // clear the canvas
  var rect = canvas.getBoundingClientRect();
  var x = e.clientX - rect.left; // calculate x coordinate relative to canvas
  var y = e.clientY - rect.top; // calculate y coordinate relative to canvas
  rectangles.forEach(function (rect) {
    // loop through all rectangles
    ctx.fillStyle = rect.isPointInside(x, y) ? "red" : "green"; // change color if mouse is inside rectangle
    createRect(ctx, rect.x, rect.y, rectWidth, rectHeight);
  });
});

drawGrid();
