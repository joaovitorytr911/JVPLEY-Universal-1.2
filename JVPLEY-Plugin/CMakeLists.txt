cmake_minimum_required(VERSION 3.20)

project(JVPLEYPlugin VERSION 0.1.0 LANGUAGES CXX)

if(NOT WIN32)
    message(FATAL_ERROR "JVPLEY telemetry plugin is currently Windows-only.")
endif()

if(NOT DEFINED SCS_SDK_INCLUDE_DIR OR
   NOT EXISTS "${SCS_SDK_INCLUDE_DIR}/scssdk_telemetry.h")
    message(FATAL_ERROR
        "SCS_SDK_INCLUDE_DIR must point to the official SCS SDK include folder.")
endif()

add_library(JVPLEY SHARED
    src/JVPLEYPlugin.cpp
    src/JVPLEY.def
)

target_include_directories(JVPLEY PRIVATE "${SCS_SDK_INCLUDE_DIR}")
target_compile_features(JVPLEY PRIVATE cxx_std_17)
target_compile_definitions(JVPLEY PRIVATE WIN32_LEAN_AND_MEAN NOMINMAX)

set_target_properties(JVPLEY PROPERTIES
    PREFIX ""
    OUTPUT_NAME "JVPLEY"
)

if(MSVC)
    target_compile_options(JVPLEY PRIVATE /W4 /permissive- /EHsc)
endif()
