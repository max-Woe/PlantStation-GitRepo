#include "measurementHelper.h"
#include <Arduino.h>

const uint32_t REBOOT_INTERVAL = 24 * 60 * 60 * 1000;

void CheckForRestart()
{
    if (millis() > REBOOT_INTERVAL)
    {
        delay(1000);
        ESP.restart();
    }
}

void CollectSamples(int pin, float* samples, int size)
{
    int delay_in_seconds = (1000 - 20) * 1000 / 100;
    for (int i = 0; i < size; i++)
    {
        int soil_analog = analogRead(pin); // s.u.
        samples[i] = soil_analog;
        delayMicroseconds(delay_in_seconds);
    }
}

float ValidateMeasurement(float measurement, int bounds[])
{
    if (measurement > bounds[1]) return 999;
    if (measurement < bounds[0]) return -999;
    return measurement;
}