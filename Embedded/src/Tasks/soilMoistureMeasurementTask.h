#ifndef SOILMOISTUREMEASUREMENTTASK_H
#define SOILMOISTUREMEASUREMENTTASK_H

#include "freertos/FreeRTOS.h"
#include "freertos/queue.h"

extern QueueHandle_t sendingQueue;
extern const char* macAddress;

void soilMoistureMeasurementTask(void *parameter);
#endif