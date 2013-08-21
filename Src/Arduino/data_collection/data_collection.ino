//////////////////////////////////////////////////////////////////
//©2011 bildr
//Released under the MIT License - Please reuse change and share
//Simple code for the ADXL335, prints calculated orientation via serial
//////////////////////////////////////////////////////////////////

//Analog read pins
const int xPin = A0;
const int yPin = A1;
const int zPin = A2;

//Touch sensor multiplexers
const int touchMultiplexersPin = A3;

void setup(){
  Serial.begin(9600);
  
  pinMode(touchMultiplexersPin, OUTPUT);
  pinMode(buttonPin, INPUT);
}

void loop(){
  
  if(Serial.available() > 0)
  {
    char c = Serial.read();
    Serial.println("");
    Serial.println(";;;");
        Serial.println("");
    Keyboard.write( (c-96) %26 + 97);
  }
  
    //read the analog values from the accelerometer
  int xRead = analogRead(xPin);
  int yRead = analogRead(yPin);
  int zRead = analogRead(zPin);
  
  Serial.print(xRead);
  Serial.print(",");
    Serial.print(yRead);
  Serial.print(",");
    Serial.print(zRead);
  Serial.println();
 
  Serial.flush();
}
