#include <Servo.h>
int trigPin, echoPin, servoPin, Angle, turnLeft;
Servo radarServo; //Creating a servo object

void setup() 
{
  trigPin = 11; echoPin = 10; servoPin = 2; Angle = 10; turnLeft = 1;
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  radarServo.attach(servoPin); // Defines on which pin is the servo motor attached
  Serial.begin(9600); // Begin a sirial communication
}

void loop() 
{
  if (Angle < 170 && turnLeft) Angle ++;
  else if (Angle >= 170 || !turnLeft) { turnLeft = (Angle <= 11); Angle--; }
  radarServo.write(Angle);
  Serial.print(String(Angle) + "," + String(calculateDistance()) + ".\n");
  delay(50);
}

long calculateDistance()
{ 
  digitalWrite(trigPin, LOW); // Clean the trigPin to begin the measurment
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH); // Sets the trigPin on HIGH state for 10 Microseconds
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  //U(m/s)=dX(m)/dT(s) In dry air at 20 °C, the speed of sound is 343.2 m/s or 0.003432 m/Microsecond or 0,03434 cm/Microseconds
  return pulseIn(echoPin, HIGH) * 0.034 / 2; // Reads the echoPin, returns the sound wave travel time in microseconds
}

