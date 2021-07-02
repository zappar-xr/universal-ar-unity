#pragma once

#include "IUnityGraphics.h"
#include "IUnityGraphicsMetal.h"
#include "zappar.h"

#define ZAPPAR_METAL_SUPPORT // Comment out this line if you _do not_ want to include support for Metal. 

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload();

static void UNITY_INTERFACE_API OnNativeGLRenderEvent(int eventID);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl();

static void UNITY_INTERFACE_API OnNativeMetalRenderEvent(int eventID);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_metal();

extern "C" void zappar_pipeline_set(zappar_pipeline_t pipeline_id);
extern "C" void zappar_analytics_project_id_set(const char* id);
