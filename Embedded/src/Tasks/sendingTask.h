#ifndef SENDINGTASK_H
#define SENDINGTASK_H

#include "freertos/FreeRTOS.h"
#include "freertos/queue.h"
#include "Models/measurement.h"

extern QueueHandle_t sendingQueue;
extern const char* serverUrl;
extern const char* macAddress;

void sendingTask(void* parameter);
void sendMeasurement(const Measurement& measurement);


#endif