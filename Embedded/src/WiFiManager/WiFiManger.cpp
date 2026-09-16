#include "WiFiManager.h"

WiFiManager::WiFiManager()
{
    _currentState = WiFiState::DISCONNECTED;
}

bool WiFiManager::transitionTo(WiFiState newState)
{
 switch (_currentState) {
        case WiFiState::DISCONNECTED:
            // Von DISCONNECTED ist nur ein Wechsel nach CONNECTING möglich
            if (newState == WiFiState::CONNECTING) {
                _currentState = newState;
                return true;
            }
            break;

        case WiFiState::CONNECTING:
            // Von CONNECTING dist nur ein Wechsel nach CONNECTED oder FAILEDED möglich
            if (newState == WiFiState::CONNECTED || newState == WiFiState::FAILED) {
                _currentState = newState;
                return true;
            }
            break;

        case WiFiState::CONNECTED:
            // Von CONNECTED ist nur ein Wechsel nach RECONNECTING oder DISCONNECTED möglich
            if (newState == WiFiState::RECONNECTING || newState == WiFiState::DISCONNECTED) {
                _currentState = newState;
                return true;
            }
            break;

        case WiFiState::RECONNECTING:
            // Von RECONNECTING ist nur ein Wechsel nach CONNECTED oder FAILED möglich
            if (newState == WiFiState::CONNECTED || newState == WiFiState::FAILED) {
                _currentState = newState;
                return true;
            }
            break;

        case WiFiState::FAILED:
            // Von FAILED ist nur ein Wechsel nach DISCONNECTED oder FAILED möglich
            if (newState == WiFiState::DISCONNECTED) {
                _currentState = newState;
                return true;
            }
            break;
    }

    // Ungültiger Übergang wird blockiert/ignoriert
    return false; 
}