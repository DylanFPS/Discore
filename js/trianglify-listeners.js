(function() {
  var reGenElements = [];

  // Triangle generation functions
  var genTriangle = function (element) {

    if (element.offsetHeight === 0 && element.offsetWidth === 0) {
      console.error('element errored, no new triangle');
      console.error(element);
      return;
    }

    var pattern = Trianglify({
      width: element.offsetWidth,
      height: element.offsetHeight,
      color_function: TrianglifyColors.prototype.RainbowColorFunction,
      cell_size: 60,
      variance: "0.2"
    });

    element.setAttribute('style', 'background: url(' + pattern.png() + ')');
  }

  var genBackground = function () {
    Trianglify({
      width: window.innerWidth,
      height: window.innerHeight,
      x_colors: TrianglifyColors.PrincessToQueen,
      cell_size: 120,
      variance: "1"
    }).canvas(document.getElementById('background'));
  }
  
  // Event listeners
  var doit;

  window.addEventListener('resize', function () {
    clearTimeout(doit);
    doit = setTimeout(function () {
      genBackground();

      reGenElements.forEach(function (element, index) {
        genTriangle(element);
      });
    }, 100);
  });

  document.addEventListener("dartLoadHL", function (event) {
    $('code.language-csharp').each(function (i, block) {
      hljs.highlightBlock(block);
    });
  });

  document.addEventListener("dartTrianglify", function (event) {
    var element = document.getElementById(JSON.parse(event.detail).elementID);

    var pattern = Trianglify({
      width: element.offsetWidth,
      height: element.offsetHeight,
      color_function: TrianglifyColors.prototype.RainbowColorFunction,
      cell_size: 60,
      variance: "0.2"
    });
    element.setAttribute('style', 'background: url(' + pattern.png() + ')');
    reGenElements.push(element);
  });

  // Start by generating the background for the first time
  genBackground();
})();