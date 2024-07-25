#include "Freenove_4WD_Car_For_ESP32.h"
#include "BluetoothSerial.h"
#include <ArduinoJson.h>

BluetoothSerial SerialBT;
bool isConnected = false;

enum CarStates {InitialWaiting, CommandWaiting, WaitForRun, Running};
CarStates carState = InitialWaiting;

String mode;
int carSpeed;
int duration;
int timeInterval;
bool ready;

void setup() {
  PCA9685_Setup();
  Serial.begin(115200);
  SerialBT.begin("CarT");
  delay(1000);
}

void loop() {
  switch (carState) {
    case InitialWaiting:
      HandleInitialWaiting();
    break;

    case CommandWaiting:
      HandleCommandWaiting();
    break;

    case WaitForRun:
      HandleWaitForRun();
    break;

    case Running:
      HandleRunning();
    break;
  }
}
void HandleInitialWaiting() {

  if (SerialBT.available()) {
    String receivedMsg = SerialBT.readStringUntil('.');

    if (receivedMsg == "Hello") {
      SerialBT.println("Bluetooth Connected!");
      isConnected = true;
      carState = CommandWaiting;
    }
  }
}

void HandleCommandWaiting(){
  if (SerialBT.available()) {
      String receivedMsg = SerialBT.readStringUntil('.');
      StaticJsonDocument<200> doc; // Create a StaticJsonDocument with a capacity of 200 bytes.
      DeserializationError error = deserializeJson(doc, receivedMsg); // Parse the JSON string

   
      mode = doc["mode"].as<String>();
      carSpeed = doc["carSpeed"]; 
      duration = doc["duration"]; 
      timeInterval = doc["timeInterval"];
      ready = doc["ready"];

      if (ready){
        // send confirmation message to the GUI
        SerialBT.println("pratemers set! Car speed is " + String(carSpeed));
        carState = WaitForRun;
      }
    }
  }
void HandleWaitForRun(){
  if (SerialBT.available()) {
    String run = SerialBT.readStringUntil('.');
    
    if (run == "run"){
    SerialBT.println(carSpeed);
    carState = Running;
    }
  }
}

void HandleRunning(){

  if (mode == "pattern"){
    // SerialBT.println(String(carSpeed) + "car is running");
    Motor_Move(carSpeed, carSpeed, carSpeed, carSpeed);

    unsigned long startTime = millis();   // record the current time
    while(millis() - startTime < duration * 1000){
    SerialBT.println("car is running with speed of " + String(carSpeed));
    delay(1000*timeInterval);
    }

    // delay(duration*1000);
    carSpeed = 0 ;
    Motor_Move(carSpeed,carSpeed,carSpeed,carSpeed);
    }
  else if (mode == "tape"){
    // implement tape tracking here
    }
carState = CommandWaiting;
}