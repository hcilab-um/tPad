//////////////////////////////////////////////////////////////////
//©2011 bildr
//Released under the MIT License - Please reuse change and share
//Simple code for the ADXL335, prints calculated orientation via serial
//////////////////////////////////////////////////////////////////

//Analog read pins
const int xPin = 0;
const int yPin = 1;
const int zPin = 2;

//Digital read pins
const int stackPin0 = 10;
const int stackPin1 = 16;
const int stackPin2 = 14;
const int stackPin3 = 15;

//The minimum and maximum values that came from
//the accelerometer while standing still
//You very well may need to change these
int minVal = 400;
int maxVal = 630;

//to hold the caculated values
double x;
double y;
double z;

void setup(){
  Serial.begin(9600);
  
  pinMode(stackPin0, INPUT);
  pinMode(stackPin1, INPUT);
  pinMode(stackPin2, INPUT);
  pinMode(stackPin3, INPUT);
}

void loop(){

  //read the analog values from the accelerometer
  int xRead = analogRead(xPin);
  int yRead = analogRead(yPin);
  int zRead = analogRead(zPin);

//  Serial.print("xRead: ");
//  Serial.print(xRead);
//  Serial.print(" | yRead: ");
//  Serial.print(yRead);
//  Serial.print(" | zRead: ");
//  Serial.println(zRead);

  //convert read values to degrees -90 to 90 - Needed for atan2
  int xAng = map(xRead, minVal, maxVal, -90, 90);
  int yAng = map(yRead, minVal, maxVal, -90, 90);
  int zAng = map(zRead, minVal, maxVal, -90, 90);

  //Caculate 360deg values like so: atan2(-yAng, -zAng)
  //atan2 outputs the value of -π to π (radians)
  //We are then converting the radians to degrees
  x = RAD_TO_DEG * (atan2(-yAng, -zAng) + PI);
  y = RAD_TO_DEG * (atan2(-xAng, -zAng) + PI);
  z = RAD_TO_DEG * (atan2(-yAng, -xAng) + PI);

  //Calculates the side that's on top. 1: front up | -1: reverse up
  int orientation = -1;
  int count = 0;
  if(x >= 270 || x <= 90)
    count++;
  if(y >= 270 || y <= 90)
    count++;
  if(z >= 90 && z <= 360)
    count ++;
  if(count >= 2)
    orientation = 1;
  
  //read the digital values for the staking codes
  int stackCode0 = digitalRead(stackPin0);
  int stackCode1 = digitalRead(stackPin1);
  int stackCode2 = digitalRead(stackPin2);
  int stackCode3 = digitalRead(stackPin3);

  //Output the caculations
  Serial.print("{");
  Serial.print("\"Orientation\": { \"Side\": ");
  if(orientation == 1)
    Serial.print("\"Front\", ");
  else
    Serial.print("\"Reverse\", ");
  Serial.print("\"X\": ");
  Serial.print(x);
  Serial.print(", \"Y\": ");
  Serial.print(y);
  Serial.print(", \"Z\": ");
  Serial.print(z);
  Serial.print(" }");

  Serial.print(", ");
  Serial.print("\"StackCode\": \"");
  Serial.print(stackCode0);
  Serial.print(stackCode1);
  Serial.print(stackCode2);
  Serial.print(stackCode3);
  Serial.print("\"");
  Serial.println("}");
  Serial.flush();

  delay(100);//just here to slow down the serial output - Easier to read
}
