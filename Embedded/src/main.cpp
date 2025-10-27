#define DEBUGMODE

#include <DHT.h>
#include <queue>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <HTTPClient.h>

#include <secrets.h>
#include "Models/measurement.h"
#include "Services/math.h"
#include "Tasks/measurementTask.h"
#include "Tasks/sendingTask.h"


// Inkludiere FreeRTOS Header
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "freertos/queue.h"


QueueHandle_t sendingQueue;

const char* ntpServer = "pool.ntp.org";
const long gmtOffset_sec = 3600; // GMT+1 Stunde
const int daylightOffset_sec = 3600;

String macAddressString;
const char* macAddress;

void setup() 
{
    Serial.begin(9600);

    WiFi.begin(SSID, PASSWORD);
    
    macAddressString = WiFi.macAddress();
    macAddress = macAddressString.c_str();
    
    while (WiFi.status() != WL_CONNECTED) 
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println("\nVerbindung erfolgreich!");
    
    configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);

    sendingQueue = xQueueCreate(10, sizeof(Measurement));
    
    if (sendingQueue != NULL) 
    {
        Serial.println("Queue erfolgreich erstellt.");

        xTaskCreatePinnedToCore(
            measurementTask,
            "Sensor Task",
            4096,
            NULL,
            1,
            NULL,
            1
        );

        xTaskCreatePinnedToCore(
            sendingTask, 
            "Sending Task", 
            8192,
            NULL, 
            2,
            NULL,
            0
        );
    } 
    else 
    {
        Serial.println("Fehler beim Erstellen der Queue.");
    }
}

void loop() 
{
}