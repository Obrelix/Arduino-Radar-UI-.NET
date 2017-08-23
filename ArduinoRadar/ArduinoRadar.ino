#include <Servo.h>
int trigPin, echoPin, servoPin, Angle, turnLeft;// "turnLeft" variable will be used as a boolean in a if statement 
Servo radarServo; //Creating a servo object

void setup(){
  trigPin = 11; echoPin = 10; servoPin = 2; Angle = 10; turnLeft = 1; 
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  radarServo.attach(servoPin); // Defines on which pin is the servo motor attached
  Serial.begin(9600); // Starts a new serial communication with baud rate 9600 bps
}

void loop(){
  if (Angle < 170 && turnLeft) Angle ++;  
  else if (Angle >= 170 || !turnLeft) { turnLeft = (Angle <= 11); Angle--; }
  radarServo.write(Angle);
  Serial.print(String(Angle) + "," + String(calculateDistance()) + ".\n");
  delay(50);
}

long calculateDistance(){ 
  digitalWrite(trigPin, LOW); // Clean the trigPin
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH); // Creates a new sound wave
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  return pulseIn(echoPin, HIGH) * 0.034 / 2; 
}

  // U(m/s)=dX(m)/dT(s)
  // In dry air at 20 °C, the speed of sound is 343.2 m/s or 0,03434 cm/Microseconds.
  // The function pulseIn(echoPin, HIGH) reads the echoPin until it catches a reflection
  // of the sound wave and returns the time that sound wave traveled in microseconds
