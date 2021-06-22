//
//  ZCVDemo.hpp
//  ZCVMacOSTestRenderPlugin
//
//  Created by Jordan Campbell on 20/07/2020.
//  Copyright © 2020 Zappar. All rights reserved.
//

#pragma once

#include "Unity/IUnityGraphics.h"
#include "Unity/IUnityGraphicsMetal.h"
#include "Debug.h"
#include "zappar.h"

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload();

extern "C" void zappar_pipeline_set(zappar_pipeline_t pipeline_id);
extern "C" void zappar_analytics_project_id_set(const char *id);

static void UNITY_INTERFACE_API OnNativeGLRenderEvent(int eventID);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl();

static void UNITY_INTERFACE_API OnNativeMetalRenderEvent(int eventID);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_metal();

// Unity Log methods
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ZapparSetDebugMode(Debug::Mode mode, zappar_log_level_t lvl);

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ZapparSetLogFunc(Debug::DebugLogFuncPtr func);

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ZapparSetErrorFunc(Debug::DebugLogFuncPtr func);
