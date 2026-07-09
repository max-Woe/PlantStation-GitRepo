#include "calibrationMeasurement.h"
#include <ArduinoJson.h>

CalibrationMeasurement::CalibrationMeasurement() 
    : _time(0), _value(0.0), _unit(""), _type(""), _pin(-1), _macAddress("") {}

CalibrationMeasurement::CalibrationMeasurement(time_t time, float value, const char* unit, 
                         const char* type, int pin, const char* macAddress)
    : _time(time), 
      _value(value), 
      _unit(unit), 
      _type(type), 
      _pin(pin), 
      _macAddress(macAddress) {}

time_t CalibrationMeasurement::getTime() const { return _time; }
float CalibrationMeasurement::getValue() const { return _value; }
const char* CalibrationMeasurement::getUnit() const { return _unit; }
const char* CalibrationMeasurement::getType() const { return _type; }
int CalibrationMeasurement::getPin() const { return _pin; }
const char* CalibrationMeasurement::getMacAddress() const { return _macAddress; }

void CalibrationMeasurement::setTime(time_t time) {_time = time;}; 
void CalibrationMeasurement::setValue(float value) {_value = value;};
void CalibrationMeasurement::setUnit(const char* unit) {_unit = unit;};
void CalibrationMeasurement::setType(const char* type) {_type = type;};
void CalibrationMeasurement::setPin(int pin)  {_pin = pin;};
void CalibrationMeasurement::setMacAddress(const char* macAddress) {_macAddress = macAddress;};

void CalibrationMeasurement::print() const {
    Serial.print("Time: "); Serial.print(_time);
    Serial.print(", Value: "); Serial.print(_value);
    Serial.print(", Unit: "); Serial.print(_unit);
    Serial.print(", Type: "); Serial.print(_type);
    Serial.print(", Pin: "); Serial.print(_pin);
    Serial.print(", MAC: "); Serial.println(_macAddress);
}
/*
String Measurement::toJson() const {
    // Erstellt ein dynamisches JSON-Dokument.
    // Die Kapazität wird automatisch angepasst.
    const size_t capacity = JSON_OBJECT_SIZE(1) + JSON_OBJECT_SIZE(6) + 60;
    DynamicJsonDocument doc(capacity);
    // JsonObject receivedMeasurement = doc.createNestedObject("receivedMeasurement");
    // Befüllt das Dokument mit den Daten der Klasse.
    // Die Zeit wird als Unix-Timestamp (Sekunden seit 1970) übergeben,
    // was die einfachste Art ist, die Daten an C# zu übergeben.
    doc["UnixTime"] = _time; 
    doc["Value"] = _value;
    doc["Unit"] = _unit;
    doc["Type"] = _type;
    doc["Pin"] = _pin;
    doc["MacAddress"] = _macAddress;

    // Serialisiert das JSON-Dokument in einen String.
    String jsonString;
    serializeJson(doc, jsonString);
    
    return jsonString;
}
    */
void CalibrationMeasurement::toStaticJson(char* buffer, size_t capacity) const {
    // StaticJsonDocument reserviert den Speicher auf dem Stack, nicht auf dem Heap
    StaticJsonDocument<256> doc; 

    doc["UnixTime"] = _time; 
    doc["Value"] = _value;
    doc["Unit"] = _unit;
    doc["Type"] = _type;
    doc["Pin"] = _pin;
    doc["MacAddress"] = _macAddress;

    // Schreibt das JSON direkt in den übergebenen char-Puffer
    serializeJson(doc, buffer, capacity);
}