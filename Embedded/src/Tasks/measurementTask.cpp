#ifndef DEBUGMODE
#define DEBUGMODE FALSE
#endif

#include <Tasks/measurementTask.h>

#include <DHT.h>
// #include "freertos/queue.h"

#include <Models/measurement.h>
#include <Services/math.h>


const int dhtPin = 4;
const int soilMoisturePin = 32;

const int soil_analog_wet = 930; // Nass
const int soil_analog_dry = 2600; // Trocken

const char* type_temperature = "temperature";
const char* type_humidity = "humidity";
const char* type_soil = "soil_moisture";
const char* type_water_level = "water_level";

const char* unit_temperature = "°C";
const char* unit_humidity = "%rel";
const char* unit_soil = "%";
const char* unit_water_level = "%";

extern char deviceMacAddress[18];

DHT dht(dhtPin, DHT22);

const uint32_t REBOOT_INTERVAL = 24 * 60 * 60 * 1000; // 24 Stunden in Millisekunden


void measurementTask(void* parameter)
{
    int counter = 1;
    int counter_max_1s = 100;
    int counter_runs = 1;


    static float tempReadings[60];
    static float humReadings[60];
    static float soilReadings_60s[60];
    static float soilReadings_1s[100];
    static float waterReadings[60];

    dht.begin();

    struct tm timeinfo;
    TickType_t xLastWakeTime;
    while (true) 
    {
        if (millis() > REBOOT_INTERVAL) {
            //Serial.println("Präventiver Neustart nach 24 Stunden Laufzeit...");
            delay(1000); 
            ESP.restart();
        }
        const TickType_t xFrequency = pdMS_TO_TICKS(1000);

        xLastWakeTime = xTaskGetTickCount();

        float temp = dht.readTemperature();
        float hum = dht.readHumidity();
        
        for(int i = 0; i<100; i++) 
        {
            int soil_analog = analogRead(soilMoisturePin);


            soilReadings_1s[i] = soil_analog;
        }

        int soil_mean_1s = Math::mean(soilReadings_1s, counter_max_1s);

        float soil =  map(soil_mean_1s, soil_analog_dry, soil_analog_wet, 0, 100);
        
        float temp_mean_60s;
        float hum_mean_60s;
        float soil_mean_60s;
        
        tempReadings[counter-1] = temp;
        humReadings[counter-1] = hum;
        soilReadings_60s[counter-1] = soil;
        
        #ifdef DEBUGMODE
        {
            //Serial.print("Soil (Pin " + String(soilMoisturePin) + "): ");
            Serial.print("Soil (Pin ");
            Serial.print(soilMoisturePin);
            Serial.print("): ");
            for(int i = 0; i < counter; i++) 
            {
                Serial.print(soilReadings_60s[i]);
                Serial.print(" ");
            }
        }
        #endif
        if(counter >= 60) 
        { 
            Serial.println();

            if (!getLocalTime(&timeinfo)) 
            {
                Serial.println("Fehler beim Abrufen der Zeit - überspringe Zyklus");
                vTaskDelay(pdMS_TO_TICKS(1000));
                continue;
            }

            Serial.println("Start Measurement processing...");

            time_t current_timestamp = mktime(&timeinfo);
            temp_mean_60s = Math::mean(tempReadings, counter);

            Serial.println(counter);
            for (int i = 0; i < 60; i++) 
            {
                Serial.println(tempReadings[i]);
            }

            Serial.print("-----------------Temp mean calculated.----------------");
            Serial.println(temp_mean_60s);

            hum_mean_60s = Math::mean(humReadings, counter);
            soil_mean_60s = Math::mean(soilReadings_60s, counter);
            
            #ifdef DEBUGMODE
            {
                Serial.println("--------------------------------------------------------------");

                Serial.print("Durchlauf: ");
                Serial.println(counter_runs); 
                
                Serial.print("Aktueller Zeitstempel: ");
                Serial.println(current_timestamp);
                
                Serial.print(type_soil);
                Serial.print(" (Pin:");
                Serial.print(soilMoisturePin);
                Serial.print("): ");
                Serial.print(soil_mean_60s);
                Serial.print(" ");
                Serial.println(unit_soil);
                Serial.println();
                
                Serial.print(type_temperature);
                Serial.print(" (Pin:");
                Serial.print(dhtPin);
                Serial.print("):");
                Serial.print(temp_mean_60s);
                Serial.print(" ");
                Serial.println(unit_temperature);
                Serial.println();

                Serial.print(type_humidity);
                Serial.print(" (Pin:");
                Serial.print(dhtPin);
                Serial.print("): ");
                Serial.print(hum_mean_60s);
                Serial.print(" ");
                Serial.println(unit_humidity);
                Serial.println();
            
                Serial.print("Mac Adress: ");
                Serial.println(deviceMacAddress);
                Serial.println();
            
                Serial.println("--------------------------------------------------------------");
            }
            #endif


            Measurement temperature_measurement(current_timestamp, temp_mean_60s, unit_temperature, type_temperature, dhtPin, deviceMacAddress);
            xQueueSend(sendingQueue, &temperature_measurement, portMAX_DELAY);
            Serial.println("Temperature measurement queued.");

            Measurement humidity_measurement(current_timestamp, hum_mean_60s, unit_humidity, type_humidity, dhtPin, deviceMacAddress);
            xQueueSend(sendingQueue, &humidity_measurement, portMAX_DELAY);
            Serial.println("Humidity measurement queued.");
            
            Measurement soil_measurement(current_timestamp, soil_mean_60s, unit_soil, type_soil, soilMoisturePin, deviceMacAddress);
            xQueueSend(sendingQueue, &soil_measurement, portMAX_DELAY);

            Serial.println("Measurement processing done.");
            counter = 1;
        } 
        else 
        {
            if (counter % 10 == 0 && counter != 0) 
            {
                Serial.print(counter);
            }
            else
            {
                Serial.print(".");
            }

            counter++;
        }
        #ifdef DEBUGMODE
        {
            counter_runs++;
        }
        #endif

        vTaskDelayUntil(&xLastWakeTime, xFrequency);
    }
};