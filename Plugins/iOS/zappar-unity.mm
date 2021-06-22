#include "zappar-unity.h"

static IUnityInterfaces* s_interface = 0;
static id<MTLDevice> s_device = 0;
static zappar_pipeline_t s_pipeline = 0;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {

    Debug::Finalize();
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_interface = unityInterfaces;

    if (s_interface != 0 ) {
        IUnityGraphicsMetal* metal = s_interface->Get<IUnityGraphicsMetal>();
        s_device = metal->MetalDevice();
    }

    Debug::Initialize();
}

static void UNITY_INTERFACE_API OnNativeMetalRenderEvent(int eventID)
{
    if (s_device != 0) {
        zappar_pipeline_camera_frame_upload_metal( s_pipeline, s_device );
    } else {
        NSLog(@"Do not have valid metal device for rendering.");
    }
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_metal()
{
    return OnNativeMetalRenderEvent;
}

static void UNITY_INTERFACE_API OnNativeGLRenderEvent(int eventID)
{
   zappar_pipeline_camera_frame_upload_gl( s_pipeline );
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl()
{
    return OnNativeGLRenderEvent;
}

static void UNITY_INTERFACE_API OnNativeNullRenderEvent(int eventID)
{
    
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_dx11()
{
    return OnNativeNullRenderEvent;
}

extern "C" void zappar_pipeline_set(zappar_pipeline_t pipeline)
{
    s_pipeline = pipeline;
}

extern "C" void zappar_analytics_project_id_set(const char* id) {}
extern "C" void* zappar_pipeline_camera_frame_texture_dx11(zappar_pipeline_t o) { return 0; }


// Unity Log methods

static void LogCallback(zappar_log_level_t lvl, const char* msg)
{
    if (lvl >= LOG_LEVEL_WARNING)
    {
        std::string log(msg);
        Debug::Log(log);
    }
    else if (lvl >= LOG_LEVEL_ERROR)
    {
        std::string log(msg);
        Debug::Error(log);
    }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ZapparSetDebugMode(Debug::Mode mode, zappar_log_level_t lvl)
{
    Debug::SetMode(mode);

    //register callback function
    zappar_redirect_log_set(lvl, &LogCallback);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ZapparSetLogFunc(Debug::DebugLogFuncPtr func)
{
    Debug::SetLogFunc(func);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ZapparSetErrorFunc(Debug::DebugLogFuncPtr func)
{
    Debug::SetErrorFunc(func);
}

