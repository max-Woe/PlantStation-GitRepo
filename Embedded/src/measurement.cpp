#include "Measurement.h"
#include <ArduinoJson.h>

Measurement::Measurement() 
    : _time(0), _value(0.0), _unit(""), _type(""), _pin(-1), _macAddress("") {}

Measurement::Measurement(time_t time, float value, const String& unit, 
                         const String& type, int pin, const String& macAddress)
    : _time(time), 
      _value(value), 
      _unit(unit), 
      _type(type), 
      _pin(pin), 
      _macAddress(macAddress) {}

time_t Measurement::getTime() const { return _time; }
float Measurement::getValue() const { return _value; }
String Measurement::getUnit() const { return _unit; }
String Measurement::getType() const { return _type; }
int Measurement::getPin() const { return _pin; }
String Measurement::getMacAddress() const { return _macAddress; }

void Measurement::setTime(time_t time) {_time = time;}; 
void Measurement::setValue(float value) {_value = value;};
void Measurement::setUnit(String unit) {_unit = unit;};
void Measurement::setType(String type) {_type = type;};
void Measurement::setPin(int pin)  {_pin = pin;};
void Measurement::setMacAddress(String macAddress) {_macAddress = macAddress;};

void Measurement::print() const {
    Serial.print("Time: "); Serial.print(_time);
    Serial.print(", Value: "); Serial.print(_value);
    Serial.print(", Unit: "); Serial.print(_unit);
    Serial.print(", Type: "); Serial.print(_type);
    Serial.print(", Pin: "); Serial.print(_pin);
    Serial.print(", MAC: "); Serial.println(_macAddress);
}

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