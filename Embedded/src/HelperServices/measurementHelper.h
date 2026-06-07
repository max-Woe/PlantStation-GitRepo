#ifndef MEASUREMENTHELPERS_H
#define MEASUREMENTHELPERS_H

void CheckForRestart();
void CollectSamples(int pin, float* out, int size);
float ValidateMeasurement(float measurement, int bounds[]);

#endif