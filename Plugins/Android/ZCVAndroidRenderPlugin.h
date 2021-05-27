#pragma once

#include "Unity/IUnityGraphics.h"
#include "zappar.h"

static void UNITY_INTERFACE_API OnNativeGLRenderEvent(int eventID);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl();

extern "C" void zappar_pipeline_set(zappar_pipeline_t pipeline);