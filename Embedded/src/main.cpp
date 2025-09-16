#define DEBUGMODE
// #include <Arduino.h>
#include <DHT.h>
#include <queue>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <HTTPClient.h>
#include <secrets.h>
#include "Measurement.h"


// Inkludiere FreeRTOS Header
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "freertos/queue.h"

// Definiere die Pins und Sensortypen
#define DHT_PIN 4
#define DHT_TYPE DHT22

#define SOIL_MOISTURE_PIN 32
#define WATER_LEVEL_PIN 33

// Sensordefinitionen und Datenspeicherung
DHT dht(DHT_PIN, DHT_TYPE);

// Queue für die asynchrone Datenübertragung
QueueHandle_t sendingQueue;

// Schieberegister für fehlgeschlagene Messungen
#define SHIFT_REGISTER_SIZE 20
Measurement shiftRegister[SHIFT_REGISTER_SIZE];
int shiftRegisterCount = 0;

const char* ntpServer = "pool.ntp.org";
const long gmtOffset_sec = 3600; // GMT+1 Stunde
const int daylightOffset_sec = 3600;
String macAddress;
#include <string>

std::string tempUrl= "https://" + std::string(SERVERIP) + "/api/MeasurementCollection/ReceiveMeasurement";
const char* serverUrl  = tempUrl.c_str();

/*
  Dies ist die Task-Funktion, die den Programmcode für unsere Task enthält.
  Sie läuft in einer Endlosschleife.
*/
void sensorTask(void* parameter) 
{
    // float tempReadings[60];
    // float humReadings[60];
    // float soilReadings[60];
    // float waterReadings[60];

    // int counter = 0;
    int counter_runs = 0;

    dht.begin();

    String unit_temperature = "°C";
    String unit_humidity = "%rel";
    String unit_soil = "%";
    String unit_water_level = "%";

    String type_temperature = "temperature";
    String type_humidity = "humidity";
    String type_soil = "soil_moisture";
    String type_water_level = "water_level";

    struct tm timeinfo;
    
    // Die Task-Schleife, die niemals enden darf
    while (true) 
    {
        // Lese die Sensoren aus
        float temp = dht.readTemperature();
        float hum = dht.readHumidity();
        float soil =  map(analogRead(SOIL_MOISTURE_PIN), 2500, 800, 0, 100);
        float water =  map(analogRead(WATER_LEVEL_PIN), 0, 1900, 0, 100);

        // tempReadings[counter] = temp;
        // humReadings[counter] = hum;
        // soilReadings[counter] = map(soil, 2500, 800, 0, 100); // Annahme: 0-4095 wird auf 0-100% gemappt
        // waterReadings[counter] = map(water, 0, 1900, 0, 100); // Annahme: 0-4095 wird auf 0-100% gemappt
        #ifdef DEBUGMODE
        {
            Serial.println("--------------------------------------------------------------");
            Serial.print("Durchlauf: ");
            Serial.println(counter_runs); 

            Serial.print("Messwert: ");
            // Serial.println(counter);

            Serial.print("Wasserstand (Analog): ");
            Serial.println(water);
            Serial.print("Wasserstand (%): ");
            Serial.println(water);
            Serial.println();

            Serial.print("Bodenfeuchtigkeit (Analog): ");
            Serial.println(soil);
            Serial.print("Bodenfeuchtigkeit (%): ");
            Serial.println(soil);
            Serial.println();
            
            Serial.print("Temperatur: ");
            Serial.println(temp);
            Serial.println();

            Serial.print("Luftfeuchtigkeit: ");
            Serial.println(hum);
            Serial.println();
        
            Serial.println("--------------------------------------------------------------");

        }
        #endif
        // Wenn 60 Messungen gesammelt wurden, berechne den Mittelwert
        // if (counter == 59) 
        // {
        //     float avgTemp = 0, avgHum = 0, avgSoil = 0, avgWater = 0;
        //     for (int i = 0; i < 60; i++) 
        //     {
        //         avgTemp += tempReadings[i];
        //         avgHum += humReadings[i];
        //         avgSoil += soilReadings[i];
        //         avgWater += waterReadings[i];
        //     }

        //     avgTemp /= 60;
        //     avgHum /= 60;
        //     avgSoil /= 60;
        //     avgWater /= 60;

        //     if (!getLocalTime(&timeinfo)) 
        //     {
        //         Serial.println("Fehler beim Abrufen der Zeit");
        //         return;
        //     }
            Serial.println("Start Measurement processing...");
            time_t current_timestamp = mktime(&timeinfo);

            Measurement temperature_measurement(current_timestamp, temp, unit_temperature, type_temperature, DHT_PIN, macAddress);
            xQueueSend(sendingQueue, &temperature_measurement, portMAX_DELAY);
            
            Measurement humidity_measurement(current_timestamp, hum, unit_humidity, type_humidity, DHT_PIN, macAddress);
            xQueueSend(sendingQueue, &humidity_measurement, portMAX_DELAY);
            
            Measurement soil_measurement(current_timestamp, soil, unit_soil, type_soil, SOIL_MOISTURE_PIN, macAddress);
            xQueueSend(sendingQueue, &soil_measurement, portMAX_DELAY);
            
            Measurement water_measurement(current_timestamp, water, unit_water_level, type_water_level, WATER_LEVEL_PIN, macAddress);
            xQueueSend(sendingQueue, &water_measurement, portMAX_DELAY);
            Serial.println("Measurement processing done.");
            // counter = 0;
        // } 
        // else 
        // {
        //     counter++;
        // }
        #ifdef DEBUGMODE
        {
            counter_runs++;
        }
        #endif


        // Warte 10 Sekunde bis zur nächsten Messung
        for(int i= 0; i<10; i++ )
        {
            Serial.print(".");
            vTaskDelay(1000 / portTICK_PERIOD_MS);
        }
        Serial.println();
    }
}

// Dies ist die neue Task-Funktion, die die Daten aus der Queue holt und per HTTP-POST sendet. Sie beinhaltet auch die Logik für das Schieberegister.

// ```cpp
/*
 * Diese Task-Funktion verarbeitet die Daten aus der Queue und sendet sie an den Server.
 */
void sendingTask(void* parameter) 
{
    Serial.println("Sending Task gestartet.");
    Measurement currentMeasurement;
    HTTPClient http;

    while (true) 
    {
        // Zuerst versuchen, Daten aus dem Schieberegister zu senden
        if (shiftRegisterCount > 0) 
        {
            for (int i = 0; i < shiftRegisterCount; i++) 
            {
                http.begin(serverUrl);
                http.addHeader("Content-Type", "application/json");

                // Konvertiere das Measurement-Objekt in ein JSON-String
                String jsonPayload = shiftRegister[i].toJson();
                int httpResponseCode = http.POST(jsonPayload);

                #ifdef DEBUGMODE
                {
                    Serial.println("--------------------------------------------------------------");
                    Serial.println("Versuche, gespeicherte Daten zu senden...");
                    Serial.print("Payload: ");
                    Serial.println(jsonPayload);
                    Serial.print("Senden an ServerURL: ");
                    Serial.println(serverUrl);
                    Serial.print("HTTP Response Code: ");
                    Serial.println(httpResponseCode);
                    Serial.println("--------------------------------------------------------------");
                }
                #endif

                if (httpResponseCode > 0) 
                {
                    Serial.printf("Gespeicherten Wert erfolgreich gesendet, Server-Antwort: %s\n", http.getString().c_str());
                    // Verschiebe die restlichen Elemente im Array um eine Position nach vorne
                    for (int j = i; j < shiftRegisterCount - 1; j++) 
                    {
                        shiftRegister[j] = shiftRegister[j+1];
                    }
                    shiftRegisterCount--;
                    i--; // Prüfe das neue Element an dieser Position
                } 
                else 
                {
                    Serial.printf("Fehler beim Senden der gespeicherten Daten. HTTP Response Code: %d\n", httpResponseCode);
                    // Abbruch, wenn das Senden fehlschlägt, um die Verbindung nicht zu überlasten
                    break;
                }
                http.end();
            }
        }

        // Dann versuchen, neue Daten aus der Queue zu senden
        if (xQueueReceive(sendingQueue, &currentMeasurement, portMAX_DELAY) == pdTRUE) 
        {
            #ifdef DEBUGMODE
            {
                Serial.print("Neue Messung aus der Queue erhalten: ");
            }

            #endif
            http.begin(serverUrl);
            http.addHeader("Content-Type", "application/json");

            // Konvertiere das Measurement-Objekt in einen JSON-String
            String jsonPayload = currentMeasurement.toJson();
            int httpResponseCode = http.POST(jsonPayload);

            #ifdef DEBUGMODE
            {
                Serial.println("--------------------------------------------------------------");
                Serial.println("Versuche, neue Daten zu senden...");
                Serial.print("Payload: ");
                Serial.println(jsonPayload);
                Serial.print("Senden an ServerURL: ");
                Serial.println(serverUrl);
                Serial.print("HTTP Response Code: ");
                Serial.println(httpResponseCode);
                Serial.println("--------------------------------------------------------------");
            }
            #endif

            if (httpResponseCode > 0) 
            {
                Serial.printf("Daten erfolgreich gesendet, Server-Antwort: %s\n", http.getString().c_str());
            } 
            else 
            {
                Serial.printf("Fehler beim Senden. HTTP Response Code: %d\n", httpResponseCode);
                // Wenn das Senden fehlschlägt, den Wert im Schieberegister speichern
                if (shiftRegisterCount < SHIFT_REGISTER_SIZE) 
                {
                    shiftRegister[shiftRegisterCount] = currentMeasurement;
                    shiftRegisterCount++;
                    Serial.println("Wert im Schieberegister gespeichert.");
                } 
                else 
                {
                    // Schieberegister ist voll, ältesten Wert verwerfen und neuen hinzufügen
                    for (int i = 0; i < SHIFT_REGISTER_SIZE - 1; i++) 
                    {
                        shiftRegister[i] = shiftRegister[i+1];
                    }
                    shiftRegister[SHIFT_REGISTER_SIZE-1] = currentMeasurement;
                    Serial.println("Schieberegister voll. Ältester Wert verworfen und neuer Wert hinzugefügt.");
                }
            }
            http.end();
        }
        // Warten, bevor die nächste Iteration startet, um CPU-Zeit zu sparen
        vTaskDelay(1000 / portTICK_PERIOD_MS);
    }
}

void setup() 
{
    Serial.begin(9600);

    WiFi.begin(SSID, PASSWORD);
    macAddress = WiFi.macAddress();
    // Warten, bis die Verbindung steht
    while (WiFi.status() != WL_CONNECTED) 
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println("\nVerbindung erfolgreich!");
    
    configTime(gmtOffset_sec, daylightOffset_sec, ntpServer);

    // Initialisiere die Queue für die Datenspeicherung
    sendingQueue = xQueueCreate(10, sizeof(Measurement));
    
    if (sendingQueue != NULL) 
    {
        Serial.println("Queue erfolgreich erstellt.");
        /*
          Hier wird die eigentliche Task erstellt und gestartet.
          Der erste Parameter ist der Pointer auf die Task-Funktion (sensorTask).
        */
        xTaskCreate(
            sensorTask,         // Pointer auf die Funktion, die die Task ausführt
            "Sensor Task",      // Ein Name für die Task
            4096,               // Stack-Größe in Bytes
            NULL,               // Parameter, die an die Task übergeben werden
            1,                  // Priorität der Task
            NULL                // Handle, um später auf die Task zuzugreifen
        );
        // Sende-Task erstellen
        xTaskCreate(
            sendingTask, 
            "Sending Task", 
            8192, // Größerer Stack-Speicher, da HTTP-Verbindungen mehr benötigen
            NULL, 
            2,    // Höhere Priorität als die Sensor-Task
            NULL
        );
    } 
    else 
    {
        Serial.println("Fehler beim Erstellen der Queue.");
    }
}

// Die Haupt-loop() Funktion bleibt leer, da die gesamte Logik in der Task läuft.
void loop() 
{
 
}