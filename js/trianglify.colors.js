var TrianglifyColors = function () { };

TrianglifyColors.PrincessToQueen = ["#FAAE8A", "#DC8574", "#AF5D6B", "#6C3755", "#432142"]; //http://www.colourlovers.com/palette/3806262/Princess_to_Queen
TrianglifyColors.CrashingOceanWaves = ["#2A4347", "#4A7D6A", "#648D7D", "#ADB4AC", "#DEDFD9"]; //http://www.colourlovers.com/palette/2331840/Crashing_Ocean_Waves
TrianglifyColors.PinkElephant = ["#FFC2CE", "#80B3FF", "#FD6E8A", "#A2122F", "#693726"]; //http://www.colourlovers.com/palette/124973/Pink_Elephant

TrianglifyColors.prototype.RainbowColorFunction = function(x, y) {
	return 'hsl('+Math.floor(Math.abs(x*y)*360)+',80%,60%)';
};