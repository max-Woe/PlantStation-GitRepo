#include <Tasks/soilMoistureMeasurementTask.h>
#include <Models/measurement.h>
#include <Services/math.h>
#include "soilMoistureMeasurementTask.h"
#include <HelperServices/measurementHelper.h>


const int ADC_PIN = 32;

//Grenzwerte als Rohwerte aus der Kalibrierung
const int ADC_WET = 400;
const int ADC_DRY = 2500;

const int ADC_DELTA = ADC_DRY-ADC_WET;
const int ADC_PUFFER = ADC_DELTA*0.1;

//Maximalwerte mit einem 10% Sicherheitspuffer.
const int ADC_WET_PUFFERED = ADC_WET-ADC_PUFFER; // Nass
const int ADC_DRY_PUFFERED = ADC_DRY+ADC_PUFFER; // Trocken

//Informationen
static constexpr const char* TYPE = "soil_moisture";
static constexpr const char* UNIT = "%";
extern char deviceMacAddress[18];


void soilMoistureMeasurementTask(void* parameter)
{
    const int COUNT_SAMPLES_1s = 100;
    const int COUNT_SAMPLES_60s = 60;
 
    int counter_seconds = 0;

    //Bewusst als static deklariert um Stack-Überlauf zu verhindern.
    static float adc_samples_1s[COUNT_SAMPLES_1s];
    static float adc_samples_60s[COUNT_SAMPLES_60s];
    
    struct tm timeinfo;
    TickType_t xLastWakeTime;
    const TickType_t xFrequency = pdMS_TO_TICKS(1000);

    while(true)
    {
        //Täglicher Restart (um ungewollte Überläufe abzufangen)
        CheckForRestart();
        xLastWakeTime = xTaskGetTickCount();

        //Über 1s 100 Samples sammeln
        CollectSamples(ADC_PIN, adc_samples_1s, COUNT_SAMPLES_1s);

        //Den Medianwert für 1s ermitteln und als Sekundenwert ablegen
        adc_samples_60s[counter_seconds] = Math::median(adc_samples_1s, COUNT_SAMPLES_1s);

        counter_seconds++;
        
        //Nach 60 Sekunden...
        if(counter_seconds==60)
        {   
            counter_seconds = 0;

            if (!getLocalTime(&timeinfo)) 
            {
                Serial.println("Fehler beim Abrufen der Zeit - überspringe Zyklus");
                vTaskDelay(pdMS_TO_TICKS(1000));
                continue;
            }
            time_t current_timestamp = mktime(&timeinfo);

            //...den Medianwert der Sekundenmessungen ermitteln,...
            float adc_samples_60s_median = Math::median(adc_samples_60s, COUNT_SAMPLES_60s);
            
            //...die Minutenmessung mappen...
            float measurement_value = map(adc_samples_60s_median, ADC_DRY_PUFFERED, ADC_WET_PUFFERED, 0, 100);
            
            //...den Messwert validieren,...
            int validation_bounds[2] = {0, 100};
            float measurement_value_validated = ValidateMeasurement(measurement_value, validation_bounds);
        
            //...Erstellung des Measurement-Objektes,...
            Measurement temperature_measurement(
                current_timestamp, 
                measurement_value_validated, 
                UNIT, 
                TYPE, 
                ADC_PIN, 
                deviceMacAddress
            );
                
            //...Übergabe an die Sender-Task
            xQueueSend(sendingQueue, &temperature_measurement, portMAX_DELAY);
        }
        
        vTaskDelayUntil(&xLastWakeTime, xFrequency);
    }
}