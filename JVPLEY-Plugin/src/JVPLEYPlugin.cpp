#include <windows.h>

#include <cmath>
#include <cstdio>
#include <stdio.h>
#include <cstring>
#include <string>
#include <vector>

#include "scssdk_telemetry.h"
#include "eurotrucks2/scssdk_telemetry_eut2.h"

namespace {

struct TelemetryState {
    float speed_mps = 0.0f;
    float engine_rpm = 0.0f;
    float fuel_l = 0.0f;
    scs_s32_t gear = 0;
    bool speed_valid = false;
    bool rpm_valid = false;
    bool fuel_valid = false;
    bool gear_valid = false;
    bool paused = true;
};

TelemetryState g_state;
scs_log_t g_game_log = nullptr;
ULONGLONG g_last_write_ms = 0;
bool g_output_warning_logged = false;

void log_message(const scs_log_type_t type, const char* message)
{
    if (g_game_log != nullptr) {
        g_game_log(type, message);
    }
}

bool get_output_paths(std::wstring& output_path, std::wstring& temp_path)
{
    const DWORD required = GetEnvironmentVariableW(L"LOCALAPPDATA", nullptr, 0);
    if (required == 0) {
        return false;
    }

    std::vector<wchar_t> buffer(required);
    if (GetEnvironmentVariableW(L"LOCALAPPDATA", buffer.data(), required) == 0) {
        return false;
    }

    std::wstring folder(buffer.data());
    folder += L"\\JVPLEY";

    if (!CreateDirectoryW(folder.c_str(), nullptr)) {
        const DWORD error = GetLastError();
        if (error != ERROR_ALREADY_EXISTS) {
            return false;
        }
    }

    output_path = folder + L"\\telemetry.json";
    temp_path = folder + L"\\telemetry.json.tmp";
    return true;
}

void write_json_number(FILE* file, const char* name, const bool valid, const double value)
{
    std::fprintf(file, "\"%s\":", name);
    if (valid && std::isfinite(value)) {
        std::fprintf(file, "%.3f", value);
    } else {
        std::fputs("null", file);
    }
}

bool write_telemetry_file()
{
    std::wstring output_path;
    std::wstring temp_path;
    if (!get_output_paths(output_path, temp_path)) {
        return false;
    }

    FILE* file = nullptr;
    if (_wfopen_s(&file, temp_path.c_str(), L"wb") != 0 || file == nullptr) {
        return false;
    }

    const double speed_kmh_signed = static_cast<double>(g_state.speed_mps) * 3.6;
    const double speed_kmh = std::fabs(speed_kmh_signed);

    std::fprintf(file,
        "{\"schema\":1,\"source\":\"JVPLEY.dll\",\"game\":\"eut2\","
        "\"state\":\"%s\",\"paused\":%s,",
        g_state.paused ? "paused" : "driving",
        g_state.paused ? "true" : "false");

    write_json_number(file, "speed_kmh", g_state.speed_valid, speed_kmh);
    std::fputc(',', file);
    write_json_number(file, "speed_kmh_signed", g_state.speed_valid, speed_kmh_signed);
    std::fputc(',', file);
    write_json_number(file, "speed_mps", g_state.speed_valid, g_state.speed_mps);
    std::fputc(',', file);
    write_json_number(file, "rpm", g_state.rpm_valid, g_state.engine_rpm);
    std::fputc(',', file);
    write_json_number(file, "fuel_l", g_state.fuel_valid, g_state.fuel_l);
    std::fputs(",\"gear\":", file);

    if (g_state.gear_valid) {
        std::fprintf(file, "%d", static_cast<int>(g_state.gear));
    } else {
        std::fputs("null", file);
    }

    std::fprintf(file, ",\"updated_ms\":%llu}\n",
        static_cast<unsigned long long>(GetTickCount64()));

    const bool write_ok = (std::fflush(file) == 0);
    const bool close_ok = (std::fclose(file) == 0);
    if (!write_ok || !close_ok) {
        DeleteFileW(temp_path.c_str());
        return false;
    }

    if (!MoveFileExW(temp_path.c_str(), output_path.c_str(),
        MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH)) {
        DeleteFileW(temp_path.c_str());
        return false;
    }

    return true;
}

SCSAPI_VOID on_speed(const scs_string_t name, const scs_u32_t index,
    const scs_value_t* const value, const scs_context_t context)
{
    (void)name; (void)index; (void)context;
    if (value == nullptr) {
        g_state.speed_valid = false;
        return;
    }
    if (value->type == SCS_VALUE_TYPE_float) {
        g_state.speed_mps = value->value_float.value;
        g_state.speed_valid = true;
    }
}

SCSAPI_VOID on_rpm(const scs_string_t name, const scs_u32_t index,
    const scs_value_t* const value, const scs_context_t context)
{
    (void)name; (void)index; (void)context;
    if (value == nullptr) {
        g_state.rpm_valid = false;
        return;
    }
    if (value->type == SCS_VALUE_TYPE_float) {
        g_state.engine_rpm = value->value_float.value;
        g_state.rpm_valid = true;
    }
}

SCSAPI_VOID on_gear(const scs_string_t name, const scs_u32_t index,
    const scs_value_t* const value, const scs_context_t context)
{
    (void)name; (void)index; (void)context;
    if (value == nullptr) {
        g_state.gear_valid = false;
        return;
    }
    if (value->type == SCS_VALUE_TYPE_s32) {
        g_state.gear = value->value_s32.value;
        g_state.gear_valid = true;
    }
}

SCSAPI_VOID on_fuel(const scs_string_t name, const scs_u32_t index,
    const scs_value_t* const value, const scs_context_t context)
{
    (void)name; (void)index; (void)context;
    if (value == nullptr) {
        g_state.fuel_valid = false;
        return;
    }
    if (value->type == SCS_VALUE_TYPE_float) {
        g_state.fuel_l = value->value_float.value;
        g_state.fuel_valid = true;
    }
}

SCSAPI_VOID on_event(const scs_event_t event, const void* const event_info,
    const scs_context_t context)
{
    (void)event_info; (void)context;

    if (event == SCS_TELEMETRY_EVENT_paused) {
        g_state.paused = true;
        return;
    }

    if (event == SCS_TELEMETRY_EVENT_started) {
        g_state.paused = false;
        return;
    }

    if (event == SCS_TELEMETRY_EVENT_frame_end) {
        const ULONGLONG now = GetTickCount64();
        if (now - g_last_write_ms < 100) {
            return; // Limit disk writes to approximately 10 updates per second.
        }
        g_last_write_ms = now;

        if (!write_telemetry_file() && !g_output_warning_logged) {
            log_message(SCS_LOG_TYPE_warning,
                "JVPLEY: could not write %LOCALAPPDATA%\\JVPLEY\\telemetry.json");
            g_output_warning_logged = true;
        }
    }
}

bool register_channel(
    const scs_telemetry_init_params_v100_t* params,
    const scs_string_t channel,
    const scs_value_type_t type,
    const scs_telemetry_channel_callback_t callback)
{
    const scs_result_t result = params->register_for_channel(
        channel,
        SCS_U32_NIL,
        type,
        SCS_TELEMETRY_CHANNEL_FLAG_no_value,
        callback,
        nullptr);

    if (result != SCS_RESULT_ok) {
        log_message(SCS_LOG_TYPE_warning,
            "JVPLEY: an ETS2 telemetry channel could not be registered.");
        return false;
    }
    return true;
}

} // namespace

SCSAPI_RESULT scs_telemetry_init(
    const scs_u32_t version,
    const scs_telemetry_init_params_t* const params)
{
    if (version != SCS_TELEMETRY_VERSION_1_00 || params == nullptr) {
        return SCS_RESULT_unsupported;
    }

    const auto* version_params =
        static_cast<const scs_telemetry_init_params_v100_t*>(params);

    if (version_params->common.game_id == nullptr ||
        std::strcmp(version_params->common.game_id, "eut2") != 0) {
        return SCS_RESULT_unsupported;
    }

    g_game_log = version_params->common.log;
    g_state = TelemetryState{};
    g_last_write_ms = 0;
    g_output_warning_logged = false;

    if (version_params->register_for_event(
            SCS_TELEMETRY_EVENT_frame_end, on_event, nullptr) != SCS_RESULT_ok ||
        version_params->register_for_event(
            SCS_TELEMETRY_EVENT_paused, on_event, nullptr) != SCS_RESULT_ok ||
        version_params->register_for_event(
            SCS_TELEMETRY_EVENT_started, on_event, nullptr) != SCS_RESULT_ok) {
        log_message(SCS_LOG_TYPE_error,
            "JVPLEY: failed to register required telemetry events.");
        return SCS_RESULT_generic_error;
    }

    const bool speed_ok = register_channel(version_params,
        SCS_TELEMETRY_TRUCK_CHANNEL_speed, SCS_VALUE_TYPE_float, on_speed);
    const bool rpm_ok = register_channel(version_params,
        SCS_TELEMETRY_TRUCK_CHANNEL_engine_rpm, SCS_VALUE_TYPE_float, on_rpm);
    const bool gear_ok = register_channel(version_params,
        SCS_TELEMETRY_TRUCK_CHANNEL_engine_gear, SCS_VALUE_TYPE_s32, on_gear);
    const bool fuel_ok = register_channel(version_params,
        SCS_TELEMETRY_TRUCK_CHANNEL_fuel, SCS_VALUE_TYPE_float, on_fuel);

    if (!speed_ok && !rpm_ok && !gear_ok && !fuel_ok) {
        log_message(SCS_LOG_TYPE_error,
            "JVPLEY: none of the requested ETS2 telemetry channels were available.");
        return SCS_RESULT_generic_error;
    }

    log_message(SCS_LOG_TYPE_message,
        "JVPLEY: ETS2 telemetry plugin initialized (development build).");
    return SCS_RESULT_ok;
}

SCSAPI_VOID scs_telemetry_shutdown(void)
{
    g_state.paused = true;
    write_telemetry_file();
    g_game_log = nullptr;
}
