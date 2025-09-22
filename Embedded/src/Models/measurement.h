#ifndef MEASUREMENT_H
#define MEASUREMENT_H

#include <Arduino.h>
#include <string>
#include <ctime>

class Measurement {
public:
    Measurement();
    Measurement(time_t time, float value, const char* unit, 
             const char* type, int pin, const char* macAddress);

    time_t getTime() const;
    float getValue() const;
    const char* getUnit() const;
    const char* getType() const;
    int getPin() const;
    const char* getMacAddress() const;
        
    void setTime(time_t time);
    void setValue(float value);
    void setUnit(const char* unit);
    void setType(const char* type);
    void setPin(int pin);
    void setMacAddress(const char* macAddress);
    
    void print() const;
    String toJson() const;

private:
    time_t _time;
    float _value;
    const char* _unit;
    const char* _type;
    int _pin;
    const char* _macAddress;
};

#endif // MEASUREMENT_H