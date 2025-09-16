#ifndef MEASUREMENT_H
#define MEASUREMENT_H

#include <Arduino.h>
#include <string>
#include <ctime>

class Measurement {
public:
    Measurement();
    Measurement(time_t time, float value, const String& unit, 
                const String& type, int pin, const String& macAddress);

    time_t getTime() const;
    float getValue() const;
    String getUnit() const;
    String getType() const;
    int getPin() const;
    String getMacAddress() const;
        
    void setTime(time_t time);
    void setValue(float value);
    void setUnit(String unit);
    void setType(String type);
    void setPin(int pin);
    void setMacAddress(String macAddress);
    
    void print() const;
    String toJson() const;

private:
    time_t _time;
    float _value;
    String _unit;
    String _type;
    int _pin;
    String _macAddress;
};

#endif // MEASUREMENT_H