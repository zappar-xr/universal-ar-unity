#include <stddef.h>

#ifdef EMSCRIPTEN
    #include <emscripten.h>

    #if defined(__CYGWIN32__)
        #define UNITY_INTERFACE_API __stdcall
        #define UNITY_INTERFACE_EXPORT __declspec(dllexport)
    #elif defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
        #define UNITY_INTERFACE_API __stdcall
        #define UNITY_INTERFACE_EXPORT __declspec(dllexport)
    #elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__)
        #define UNITY_INTERFACE_API
        #define UNITY_INTERFACE_EXPORT
    #else
        #define UNITY_INTERFACE_API
        #define UNITY_INTERFACE_EXPORT
    #endif

    typedef void (UNITY_INTERFACE_API * UnityRenderingEvent)(int eventId);

    // ------------ Camera Frame Process ------------ // 

    extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_process_callback_gl();
    static void UNITY_INTERFACE_API OnWebGLRenderEvent(int eventID);

    EM_JS(void, zappar_issue_js_plugin_render_event, (), {
        window.zappar_native_callbacks.process_gl();
    });

    static void UNITY_INTERFACE_API OnWebGLRenderEvent(int eventID)
    {
        zappar_issue_js_plugin_render_event();
    }

    extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_process_callback_gl()
    {
        return OnWebGLRenderEvent;
    }

    // ------------ Camera Frame Upload ------------ // 

    extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl();
    static void UNITY_INTERFACE_API OnWebGLUploadEvent(int eventID);

    EM_JS(void, zappar_issue_js_plugin_upload_gl_event, (), {
        window.zappar_native_callbacks.upload_gl();
    });

    static void UNITY_INTERFACE_API OnWebGLUploadEvent(int eventID)
    {
        zappar_issue_js_plugin_upload_gl_event();
    }

    extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl()
    {
        return OnWebGLUploadEvent;
    }
#endif