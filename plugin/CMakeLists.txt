cmake_minimum_required(VERSION 3.21)
project(JVRSMPLAYTelemetry LANGUAGES CXX)

if(NOT WIN32)
    message(FATAL_ERROR "The JVRSMPLAY ETS2 plugin build currently targets Windows only.")
endif()

if(NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "The JVRSMPLAY ETS2 plugin must be built as x64.")
endif()

set(SCS_SDK_INCLUDE_DIR "" CACHE PATH "Path to the official SCS SDK include directory")
if(NOT EXISTS "${SCS_SDK_INCLUDE_DIR}/scssdk_telemetry.h")
    message(FATAL_ERROR "SCS_SDK_INCLUDE_DIR must point to the official SDK include directory.")
endif()
if(NOT EXISTS "${SCS_SDK_INCLUDE_DIR}/common/scssdk_telemetry_truck_common_channels.h")
    message(FATAL_ERROR "The SCS SDK common telemetry channel headers were not found.")
endif()

add_library(JVRSMPLAYTelemetry SHARED
    TelemetryPlugin.cpp
    plugin.def
)

target_compile_features(JVRSMPLAYTelemetry PRIVATE cxx_std_17)
target_include_directories(JVRSMPLAYTelemetry PRIVATE "${SCS_SDK_INCLUDE_DIR}")
target_compile_definitions(JVRSMPLAYTelemetry PRIVATE WIN32_LEAN_AND_MEAN NOMINMAX)
target_compile_options(JVRSMPLAYTelemetry PRIVATE /W4 /permissive- /EHsc)

set_target_properties(JVRSMPLAYTelemetry PROPERTIES
    PREFIX ""
    OUTPUT_NAME "JVRSMPLAY-ETS2-x64"
    SUFFIX ".dll"
)
