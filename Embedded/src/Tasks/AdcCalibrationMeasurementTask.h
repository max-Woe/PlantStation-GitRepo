#ifndef ADCCALIBRATIONMEASUREMENTTASK_H
#define ADCCALIBRATIONMEASUREMENTTASK_H

#include "freertos/FreeRTOS.h"
#include "freertos/queue.h"

extern QueueHandle_t sendingQueue;
extern const char* macAddress;

void AdcCalibrationMeasurementTask(void *parameter);
#endif