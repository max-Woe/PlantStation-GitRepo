#ifndef MEASUREMENTTASK_H
#define MEASUREMENTTASK_H

#include "freertos/FreeRTOS.h"
#include "freertos/queue.h"

extern QueueHandle_t sendingQueue;
extern const char* macAddress;

void measurementTask(void* parameter);


#endif