// Includes the Servo library
#include <Servo.h>
// Defines Tirg and Echo pins of the Ultrasonic Sensor and Servo pin
const int trigPin = 11, echoPin = 10, servoPin = 2;

long distance , duration= 0;
int Angle = 10, turnLeft = 1;

Servo myServo; // Creates a servo object to controll the servo motor

void setup() {
  pinMode(trigPin, OUTPUT); // Sets the trigPin as an Output
  pinMode(echoPin, INPUT); // Sets the echoPin as an Input
  Serial.begin(9600); //Begin a sirial communication with Arduino Ide
  myServo.attach(servoPin); // Defines on which pin is the servo motor attached
}

void loop() {

//  myServo.write(90);
  // rotates the servo motor from 15 to 165 degrees
  if (Angle < 170 && turnLeft) Angle ++;
  else if (Angle >= 170 || !turnLeft)
  {
      turnLeft = (Angle <= 10);
      Angle--;
  }
  myServo.write(Angle);
  serialPrint();
  delay(25);
  distance = calculateDistance();
}

// Function for calculating the distance measured by the Ultrasonic sensor
long calculateDistance(){ 
  //cleat the trigPin to begin the measurment
  digitalWrite(trigPin, LOW); 
  delayMicroseconds(2);
  // Sets the trigPin on HIGH state for 10 micro seconds
  digitalWrite(trigPin, HIGH); 
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  duration = pulseIn(echoPin, HIGH); // Reads the echoPin, returns the sound wave travel time in microseconds
  //U(m/s)=dX(m)/dT(s) 
  //in this case Duration(time)= 2*Distance/SpeedOfSound=> 
  //Distance=SpeedOfSound*Duration/2
  // In dry air at 20 °C, the speed of sound is 343.2 m/s or 0.003432 m/Microsecond or 0,03434 cm/Microseconds
  return duration*0.034/2;
}

void serialPrint(){
  Serial.print(Angle); // Sends the current degree into the Serial Port
  Serial.print(","); // Sends addition character right next to the previous value needed later in the Processing IDE for indexing
  Serial.print(distance); // Sends the distance value into the Serial Port
  Serial.print(".\n"); // Sends addition character right next to the previous value needed later in the Processing IDE for indexing
  }

