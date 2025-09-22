
#ifndef DEBUGMODE
#define DEBUGMODE
#endif

#include <Tasks/sendingTask.h>

#include <HTTPClient.h>

#include <Models/measurement.h>
#include <secrets.h>

const int SHIFT_REGISTER_SIZE = 20;
Measurement shiftRegister[SHIFT_REGISTER_SIZE];
int shiftRegisterCount = 0;

std::string tempUrl= "https://" + std::string(SERVERIP) + "/api/MeasurementCollection/ReceiveMeasurement";
const char* serverUrl  = tempUrl.c_str();

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