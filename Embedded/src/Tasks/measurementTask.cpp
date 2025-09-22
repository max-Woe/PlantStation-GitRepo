#ifndef DEBUGMODE
#define DEBUGMODE
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

DHT dht(dhtPin, DHT22);

void measurementTask(void* parameter)
{
    int counter = 1;
    int counter_runs = 1;

    const char* type_temperature = "temperature";
    const char* type_humidity = "humidity";
    const char* type_soil = "soil_moisture";
    const char* type_water_level = "water_level";

    const char* unit_temperature = "°C";
    const char* unit_humidity = "%rel";
    const char* unit_soil = "%";
    const char* unit_water_level = "%";

    float tempReadings[60];
    float humReadings[60];
    float soilReadings[60];
    float waterReadings[60];

    dht.begin();

    struct tm timeinfo;
    
    while (true) 
    {
        float temp = dht.readTemperature();
        float hum = dht.readHumidity();
        int soil_analog = analogRead(soilMoisturePin);
        // int water_analog = analogRead(WATER_LEVEL_PIN);

        float soil =  map(soil_analog, soil_analog_dry, soil_analog_wet, 0, 100);
        // float water =  map(water_analog, 300, 2100, 0, 100);

        float temp_mean;
        float hum_mean;
        float soil_mean;
        // float water_mean;

        tempReadings[counter-1] = temp;
        humReadings[counter-1] = hum;
        soilReadings[counter-1] = soil;
        // waterReadings[counter] = water;

        if(counter >= 60) 
        { 
            Serial.println();

            if (!getLocalTime(&timeinfo)) 
            {
                Serial.println("Fehler beim Abrufen der Zeit");
                return;
            }

            Serial.println("Start Measurement processing...");

            time_t current_timestamp = mktime(&timeinfo);
            temp_mean = Math::mean(tempReadings, counter);

            Serial.println(counter);
            for (int i = 0; i < 60; i++) 
            {
                Serial.println(tempReadings[i]);
            }

            Serial.println("-----------------Temp mean calculated.----------------" + String(temp_mean));

            hum_mean = Math::mean(humReadings, counter);
            soil_mean = Math::mean(soilReadings, counter);
            // water_mean = Math::mean(waterReadings);

            #ifdef DEBUGMODE
            {
                Serial.println("--------------------------------------------------------------");

                Serial.print("Durchlauf: ");
                Serial.println(counter_runs); 
                
                Serial.print("Aktueller Zeitstempel: ");
                Serial.println(current_timestamp);
                
                /*/
                Serial.print("Wasserstand (Analog): ");
                Serial.println(water_analog);
                Serial.print("Wasserstand (%): ");
                Serial.println(water);
                Serial.println();

                Serial.print("Bodenfeuchtigkeit (Analog): ");
                Serial.println(soil_analog);
                */
                
                Serial.print(String(type_soil) +" (Pin:"+ String(soilMoisturePin) + "): ");
                Serial.print(soil_mean);
                Serial.println(" " + String(unit_soil));
                Serial.println();
                
                Serial.print(String(type_temperature) +" (Pin:"+ String(dhtPin) + "):");
                Serial.print(temp_mean);
                Serial.println(" " + String(unit_temperature));
                Serial.println();

                Serial.print(String(type_humidity) +" (Pin:"+ String(dhtPin) + "): ");
                Serial.print(hum_mean);
                Serial.println(" " + String(unit_humidity));
                Serial.println();
            
                Serial.print("Mac Adress: ");
                Serial.println(macAddress);
                Serial.println();
            
                Serial.println("--------------------------------------------------------------");
            }
            #endif


            Measurement temperature_measurement(current_timestamp, temp_mean, unit_temperature, type_temperature, dhtPin, macAddress);
            xQueueSend(sendingQueue, &temperature_measurement, portMAX_DELAY);
            Serial.println("Temperature measurement queued.");

            Measurement humidity_measurement(current_timestamp, hum_mean, unit_humidity, type_humidity, dhtPin, macAddress);
            xQueueSend(sendingQueue, &humidity_measurement, portMAX_DELAY);
            Serial.println("Humidity measurement queued.");
            
            Measurement soil_measurement(current_timestamp, soil_mean, unit_soil, type_soil, soilMoisturePin, macAddress);
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

        vTaskDelay(1000 / portTICK_PERIOD_MS);
    }
};