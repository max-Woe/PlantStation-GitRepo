#include "math.h"
#include <Arduino.h>

int Math::mean(int* values, int count) {
    //Serial.println("INT MEAN ");
    if (count <= 0) return 0;

    int sum = 0;

    for (int i = 0; i < count; i++) {
        sum += values[i];
    }

    return sum / count;
}

float Math::mean(float* values, float count) {
    //Serial.println("FLOAT MEAN ");
    if (count <= 0) return 0;

    float sum = 0;

    for (int i = 0; i < count; i++) {
        sum += values[i];
    }

    return sum / count;
}

float Math::median(float* values, int count) {
    Serial.println("FLOAT MEDIAN ");
    std::sort(values, values + count);
    float median;

    if(count%2==1)
    {    
        median = values[count/2];
    }
    else
    {
        int upperMiddle = count/2;
        int lowerMiddle = count/2 - 1;

        median = (values[upperMiddle] + values[lowerMiddle]) / 2.0f;
    }
    return median;
}