#include <Servo.h>

const int trigPin = 11, echoPin = 10, servoPin = 2;
long distance , duration;
int Angle, turnLeft;
String outputString;
Servo myServo; //Creating a servo object

void setup() 
{
  distance = 0; duration = 0; Angle = 0; turnLeft = 1;
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  myServo.attach(servoPin); // Defines on which pin is the servo motor attached
  Serial.begin(9600); //Begin a sirial communication
  distance = 0; duration = 0;
  Angle = 0; turnLeft = 1;
}

void loop() 
{
  if (Angle < 170 && turnLeft) Angle ++;
  else if (Angle >= 170 || !turnLeft) { turnLeft = (Angle <= 10); Angle--; }
  myServo.write(Angle);
  distance = calculateDistance();
  outputString = String(Angle) + "," + String(distance) + ".\n";
  Serial.print(outputString);
  delay(25);
}

long calculateDistance()
{ 
  //clean the trigPin to begin the measurment
  digitalWrite(trigPin, LOW); 
  delayMicroseconds(2);
  // Sets the trigPin on HIGH state for 10 micro seconds
  digitalWrite(trigPin, HIGH); 
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  duration = pulseIn(echoPin, HIGH); // Reads the echoPin, returns the sound wave travel time in microseconds
  //U(m/s)=dX(m)/dT(s) 
  // In dry air at 20 °C, the speed of sound is 343.2 m/s or 0.003432 m/Microsecond or 0,03434 cm/Microseconds
  return duration*0.034/2;
}

