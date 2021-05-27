#if defined(TARGET_OS_IPHONE)

#import "UnityAppController.h"
#include "zappar-unity.h"

static id<MTLDevice> s_device = 0;
static IUnityInterfaces* s_interface = 0;
static zappar_pipeline_t s_pipeline = 0;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {}
extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_interface = unityInterfaces;

    if (s_interface != 0 ) {
#ifdef ZAPPAR_METAL_SUPPORT
        IUnityGraphicsMetal* metal = s_interface->Get<IUnityGraphicsMetal>();
        s_device = metal->MetalDevice();
#endif
    }
}

static void UNITY_INTERFACE_API OnNativeMetalRenderEvent(int eventID)
{
    if (s_device != 0) {
#ifdef ZAPPAR_METAL_SUPPORT
        zappar_pipeline_camera_frame_upload_metal( s_pipeline, s_device );
#endif
    } else {
        NSLog(@"Do not have valid metal device for rendering.");
    }
}
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_metal()
{
    return OnNativeMetalRenderEvent;
}

static void UNITY_INTERFACE_API OnNativeNullRenderEvent(int eventID)
{
    
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_dx11()
{
    return OnNativeNullRenderEvent;
}

static void UNITY_INTERFACE_API OnNativeGLRenderEvent(int eventID)
{
    zappar_pipeline_camera_frame_upload_gl( s_pipeline );
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl()
{
    return OnNativeGLRenderEvent;
}

extern "C" void zappar_pipeline_set(zappar_pipeline_t pipeline) 
{
    s_pipeline = pipeline;
}

extern "C" void zappar_analytics_project_id_set(const char* id) {}
extern "C" void* zappar_pipeline_camera_frame_texture_dx11(zappar_pipeline_t o) { return 0; }

@interface Zappar_iOSPluginController : UnityAppController
{
}
- (void)shouldAttachRenderDelegate;
@end

@implementation Zappar_iOSPluginController
- (void)shouldAttachRenderDelegate
{
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
    id rootViewController = UnityGetGLViewController();
    zappar_ios_uiviewcontroller_set( rootViewController );
}

@end
IMPL_APP_CONTROLLER_SUBCLASS(Zappar_iOSPluginController);

#endif 