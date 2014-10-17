//////////////////////////////////////////////////////////////////
//©2011 bildr
//Released under the MIT License - Please reuse change and share
//Simple code for the ADXL335, prints calculated orientation via serial
//////////////////////////////////////////////////////////////////

const int ORIENTATION_CHANGE_THRESHOLD = 200;

//Analog read pins
const int xPin = A0;
const int yPin = A1;
const int zPin = A2;

//Touch sensor multiplexers
const int touchMultiplexersPin = A3;

//The minimum and maximum values that came from
//the accelerometer while standing still
//You very well may need to change these
int minVal = 400;
int maxVal = 630;

//to hold the caculated values
double x;
double y;
double z;
int prevOrientation = 0;

// Button pin
const int buttonPin = 2;

// Button press variables
int buttonPressCount = 0;
int buttonPressCyclesBetween = 0;
const int maxButtonPressCyclesBetween = 250;
int prevIsButtonPressed = 0;

void setup(){
  Serial.begin(9600);
  
  pinMode(touchMultiplexersPin, OUTPUT);
  pinMode(buttonPin, INPUT);
}

void loop(){

  int orientation = calculateOrientation();
  int buttonEvent = calculateButtonPress();

  boolean needsPrint = false;
  if(prevOrientation != orientation)
    needsPrint = true;
  if(buttonEvent != 0)
    needsPrint = true;
    
  if(!needsPrint) 
    return;

  //changes the orientation multiplexers    
  if(orientation == 1)
    digitalWrite(touchMultiplexersPin, LOW);
  else
    digitalWrite(touchMultiplexersPin, HIGH);
  prevOrientation = orientation;
  
  //Output the caculations
  Serial.print("{");
  
  //Prints out flipping side
  Serial.print("\"FlippingSide\": ");
  if(orientation == 1)
    Serial.print("\"FaceUp\"");
  else
    Serial.print("\"FaceDown\"");
  
  //Prints out button event
  Serial.print(", ");
  Serial.print("\"ButtonEvent\": ");
  if(buttonEvent == 0)
    Serial.print("\"None\"");
  else if(buttonEvent == 1)
    Serial.print("\"Single\"");
  else if(buttonEvent == 2)
    Serial.print("\"Double\"");

  //Prints out the stack code to be used for the ID-12 integration
  Serial.print(", ");
  Serial.print("\"StackCode\": \"0000\"");

  //Finished the output
  Serial.println("}");
  Serial.flush();
}

int countOrientationChange = 0;
int calculateOrientation()
{
    //read the analog values from the accelerometer
  int xRead = analogRead(xPin);
  int yRead = analogRead(yPin);
  int zRead = analogRead(zPin);

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

  int orientation = -1;
  if(zAng >= 0)
    orientation = 1;

//  Serial.print(prevOrientation);
//  Serial.print(",");
//  Serial.print(orientation);
//  Serial.print(",");
//  Serial.println(countOrientationChange);

  if(prevOrientation == 0)
    return orientation;  
    
  if(orientation != prevOrientation && countOrientationChange > ORIENTATION_CHANGE_THRESHOLD)
    countOrientationChange = 0;
  else
    countOrientationChange++;

  if(countOrientationChange < ORIENTATION_CHANGE_THRESHOLD)
    orientation = prevOrientation;
  
  return orientation;
}

int calculateButtonPress()
{
  int isButtonPressed = digitalRead(buttonPin);
  
  // no press and nothing has happened
  if(isButtonPressed == 0 && !prevIsButtonPressed)
  {
    // the max number of cycles hasn't passed and thus is keeps waiting for the second press
    if(buttonPressCount == 1 && buttonPressCyclesBetween < maxButtonPressCyclesBetween)
    {
      buttonPressCyclesBetween++;
      prevIsButtonPressed = false;
      return 0;
    }
    
    // the max number of cycles has passed and thus it fires one single click
    if(buttonPressCount == 1) 
    {
      buttonPressCount = 0;
      buttonPressCyclesBetween = 0;
      return 1;
    }
    
    return 0;
  }
  
  // button just released (no press and it was pressed before)
  if(isButtonPressed == 0 && prevIsButtonPressed)
  {
    // starts waiting for the second press
    if(buttonPressCount == 0) 
    {
      buttonPressCount++;
      buttonPressCyclesBetween++;
      prevIsButtonPressed = false;
      return 0;
    }
    
    // second press
    if(buttonPressCount == 1)
    {
      buttonPressCount = 0;
      buttonPressCyclesBetween = 0;
      prevIsButtonPressed = false;
      return 2;
    }
  }
  
  prevIsButtonPressed = true;
  return 0;
  
}
