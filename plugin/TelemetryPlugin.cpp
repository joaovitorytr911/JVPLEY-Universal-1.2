#include <windows.h>

#include <chrono>
#include <cmath>
#include <condition_variable>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <thread>

#include "scssdk_telemetry.h"
#include "common/scssdk_telemetry_truck_common_channels.h"

namespace {
constexpr wchar_t kPipePath[] = L"\\\\.\\pipe\\JVRSMPLAY.Telemetry.v1";

std::mutex g_stateMutex;
std::condition_variable g_stateChanged;
bool g_running = false;
bool g_hasConsumption = false;
double g_consumptionLPer100Km = 0.0;
unsigned long long g_sequence = 0;
std::thread g_pipeThread;

void SCSAPIFUNC onFuelAverageConsumption(
    const scs_string_t,
    const scs_u32_t,
    const scs_value_t* const value,
    const scs_context_t)
{
    std::lock_guard<std::mutex> lock(g_stateMutex);

    if (value == nullptr || value->type != SCS_VALUE_TYPE_float) {
        g_hasConsumption = false;
        ++g_sequence;
        g_stateChanged.notify_all();
        return;
    }

    const double litersPerKm = static_cast<double>(value->value_float.value);
    if (!std::isfinite(litersPerKm) || litersPerKm < 0.0) {
        g_hasConsumption = false;
        ++g_sequence;
        g_stateChanged.notify_all();
        return;
    }

    // SCS reports liters/km. The JVRSMPLAY screen displays liters/100 km.
    g_consumptionLPer100Km = litersPerKm * 100.0;
    g_hasConsumption = true;
    ++g_sequence;
    g_stateChanged.notify_all();
}

void pipeServerLoop()
{
    while (true) {
        {
            std::lock_guard<std::mutex> lock(g_stateMutex);
            if (!g_running) break;
        }

        HANDLE pipe = CreateNamedPipeW(
            kPipePath,
            PIPE_ACCESS_OUTBOUND,
            PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
            1,
            512,
            512,
            0,
            nullptr);

        if (pipe == INVALID_HANDLE_VALUE) {
            Sleep(500);
            continue;
        }

        const BOOL connected = ConnectNamedPipe(pipe, nullptr)
            ? TRUE
            : (GetLastError() == ERROR_PIPE_CONNECTED);

        if (!connected) {
            CloseHandle(pipe);
            continue;
        }

        unsigned long long lastSentSequence = 0;
        bool clientConnected = true;

        while (clientConnected) {
            double consumption = 0.0;
            bool available = false;
            unsigned long long sequence = 0;

            {
                std::unique_lock<std::mutex> lock(g_stateMutex);
                g_stateChanged.wait_for(lock, std::chrono::milliseconds(1000), [&] {
                    return !g_running || g_sequence != lastSentSequence;
                });
                if (!g_running) break;

                sequence = g_sequence;
                if (sequence == lastSentSequence) continue;

                available = g_hasConsumption;
                consumption = g_consumptionLPer100Km;
                lastSentSequence = sequence;
            }

            char message[160]{};
            const int length = available
                ? std::snprintf(
                    message, sizeof(message),
                    "{\"type\":\"consumption\",\"available\":true,\"value\":%.3f,\"unit\":\"L/100km\"}\n",
                    consumption)
                : std::snprintf(
                    message, sizeof(message),
                    "{\"type\":\"consumption\",\"available\":false}\n");

            if (length <= 0 || length >= static_cast<int>(sizeof(message))) {
                continue;
            }

            DWORD written = 0;
            if (!WriteFile(pipe, message, static_cast<DWORD>(length), &written, nullptr) ||
                written != static_cast<DWORD>(length)) {
                clientConnected = false;
            }
        }

        DisconnectNamedPipe(pipe);
        CloseHandle(pipe);
    }
}

void stopPipeServer()
{
    {
        std::lock_guard<std::mutex> lock(g_stateMutex);
        g_running = false;
    }
    g_stateChanged.notify_all();

    if (g_pipeThread.joinable()) {
        // ConnectNamedPipe is synchronous. Cancel it so shutdown cannot wait
        // forever when the desktop app is not connected.
        CancelSynchronousIo(g_pipeThread.native_handle());
        g_pipeThread.join();
    }
}
} // namespace

extern "C" __declspec(dllexport) SCSAPI_RESULT scs_telemetry_init(
    const scs_u32_t version,
    const scs_telemetry_init_params_t* const params)
{
    if (version != SCS_TELEMETRY_VERSION_1_01) {
        return SCS_RESULT_unsupported;
    }
    if (params == nullptr) {
        return SCS_RESULT_invalid_parameter;
    }

    const auto* versionParams =
        static_cast<const scs_telemetry_init_params_v101_t*>(params);

    if (versionParams->common.game_id == nullptr ||
        std::strcmp(versionParams->common.game_id, "eut2") != 0) {
        return SCS_RESULT_unsupported;
    }

    const scs_result_t registration = versionParams->register_for_channel(
        SCS_TELEMETRY_TRUCK_CHANNEL_fuel_average_consumption,
        SCS_U32_NIL,
        SCS_VALUE_TYPE_float,
        SCS_TELEMETRY_CHANNEL_FLAG_no_value,
        onFuelAverageConsumption,
        nullptr);

    if (registration != SCS_RESULT_ok) {
        return registration;
    }

    {
        std::lock_guard<std::mutex> lock(g_stateMutex);
        g_running = true;
        g_hasConsumption = false;
        g_sequence = 0;
    }

    try {
        g_pipeThread = std::thread(pipeServerLoop);
    } catch (...) {
        {
            std::lock_guard<std::mutex> lock(g_stateMutex);
            g_running = false;
        }
        versionParams->unregister_from_channel(
            SCS_TELEMETRY_TRUCK_CHANNEL_fuel_average_consumption,
            SCS_U32_NIL,
            SCS_VALUE_TYPE_float);
        return SCS_RESULT_generic_error;
    }

    if (versionParams->common.log != nullptr) {
        versionParams->common.log(SCS_LOG_TYPE_message,
            "JVRSMPLAY telemetry plugin initialized; waiting for desktop client.");
    }

    return SCS_RESULT_ok;
}

extern "C" __declspec(dllexport) SCSAPI_VOID scs_telemetry_shutdown(void)
{
    stopPipeServer();
}
