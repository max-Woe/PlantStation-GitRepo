#ifndef WIFIMANAGER_H
#define WIFIMANAGER_H

enum class WiFiState {
    DISCONNECTED,
    CONNECTING,
    CONNECTED,
    RECONNECTING,
    FAILED  // z.B. nach N erfolglosen Versuchen
};

class WiFiManager {
    public:
        WiFiManager();
        WiFiState _currentState;
        bool transitionTo(WiFiState newState);
};
#endif WIFIMANAGER_H