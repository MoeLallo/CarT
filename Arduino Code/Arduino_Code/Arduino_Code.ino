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
String direction;
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

   
      String mode = doc["mode"].as<String>();
      carSpeed = doc["carSpeed"]; 
      duration = doc["duration"]; 
      direction = doc["direction"].as<String>();
      timeInterval = doc["timeInterval"];
      ready = doc["ready"];
      // send parameters back to the GUI
      // send confirmation message to the GUI

      if (ready){
        carState = WaitForRun;
      }
    }
  }
void HandleWaitForRun(){
  if (SerialBT.available()) {
    String run = SerialBT.readStringUntil('.');

    if (run == "run"){
    carState = Running;
    }
  }
}

void HandleRunning(){

  if (mode == "pattern"){
      Motor_Move(carSpeed, carSpeed, carSpeed, carSpeed);
      SerialBT.println(carSpeed);
      // Battery readings, 
      delay(duration*1000);
      Motor_Move(0,0,0,0);
    }

  else if (mode == "tape"){
      // implement tape tracking here
    }
}