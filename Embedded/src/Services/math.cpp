#include "math.h"
#include <Arduino.h>

int Math::mean(int* values, int count) {
    Serial.println("INT");
    if (count <= 0) return 0;

    int sum = 0;

    for (int i = 0; i < count; i++) {
        sum += values[i];
    }

    return sum / count;
}

float Math::mean(float* values, float count) {
    Serial.println("FLOAT");
    if (count <= 0) return 0;

    float sum = 0;

    for (int i = 0; i < count; i++) {
        sum += values[i];
    }

    return sum / count;
}