#include <stddef.h>
#include <cstdlib>
#include <map>
#include <string>


#ifdef EMSCRIPTEN
    #include <GLES2/gl2.h>
    //#include <GLES2/gl2ext.h>
    #include <emscripten.h>

    #include "zappar.h"

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

	//PFNGLMAPBUFFEROESPROC glMapBufferOES = nullptr;
	//PFNGLUNMAPBUFFEROESPROC glUnmapBufferOES = nullptr;

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
    
    // ------------ Face Mesh Buffer Native Update ------------ // 

    static std::map<zappar_face_mesh_t,std::pair<void*,int>> s_faceMeshUnityVertexBuffers; //vertex buffer handle and count for each face mesh pipeline
    static std::map<zappar_face_mesh_t,void*> s_mVertexBuffers;
    // Used here for updating Unity face mesh vertex buffer natively
    struct MeshVertex
    {
    	float pos[3];
    	float normal[3];
    	//float color[4];
    	float uv[2];
    };

    // EM_JS(void, log_string, (const char *msg), {
    //     console.log(UTF8ToString(msg));
    // });

    extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_set_unity_face_mesh_buffer(zappar_face_mesh_t faceMesh, void* vertexBufferHandle, int vertexCount)
    {
        if(vertexBufferHandle==nullptr || vertexCount<=0){
            //log_string("Invalid vertex buffer handle or vertexCount, in zappar_set_mesh_buffers_from_unity in face mesh pipeline!");
            return;
        }
        s_faceMeshUnityVertexBuffers[faceMesh] = std::make_pair(vertexBufferHandle,vertexCount);
        if(s_mVertexBuffers.find(faceMesh)!=s_mVertexBuffers.end()) 
            free(s_mVertexBuffers.find(faceMesh)->second);
        s_mVertexBuffers[faceMesh] = malloc(sizeof(MeshVertex)*vertexCount);
        //log_string(("Saved face mesh for update: " + std::to_string((int)faceMesh)).c_str());
    }

    extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_clear_unity_face_mesh_buffer(zappar_face_mesh_t faceMesh)
    {
        auto it = s_faceMeshUnityVertexBuffers.find(faceMesh);
        if(it!=s_faceMeshUnityVertexBuffers.end())
            s_faceMeshUnityVertexBuffers.erase(it);
        auto it2 = s_mVertexBuffers.find(faceMesh);
        if(it2!=s_mVertexBuffers.end()){
            free(it2->second);
            s_mVertexBuffers.erase(it2);
        }
        //log_string(("Cleared face mesh for update: " + std::to_string((int)faceMesh)).c_str());
    }

    extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_get_unity_face_mesh_buffers_count(){
        return s_faceMeshUnityVertexBuffers.size();
    }

    EM_JS(const float*, zappar_issue_js_plugin_face_mesh_vertices, (void* o), {
        return window.zappar_native_callbacks.face_mesh_vertices(o);
    });
    EM_JS(int, zappar_issue_js_plugin_face_mesh_vertices_size, (void* o), {
        return window.zappar_native_callbacks.face_mesh_vertices_size(o);
    });
    EM_JS(const float*, zappar_issue_js_plugin_face_mesh_normals, (void* o), {
        return window.zappar_native_callbacks.face_mesh_normals(o);
    });
    EM_JS(int, zappar_issue_js_plugin_face_mesh_normals_size, (void* o), {
        return window.zappar_native_callbacks.face_mesh_normals_size(o);
    });
    EM_JS(const float*, zappar_issue_js_plugin_face_mesh_uvs, (void* o), {
        return window.zappar_native_callbacks.face_mesh_uvs(o);
    });
    EM_JS(int, zappar_issue_js_plugin_face_mesh_uvs_size, (void* o), {
        return window.zappar_native_callbacks.face_mesh_uvs_size(o);
    });

    static void UNITY_INTERFACE_API OnNativeGLFaceMeshEvent(int eventID){
        if(eventID!=1011) { 
            //log_string("Invalid event id"); 
            return;
        }
        
        for(auto& fmb : s_faceMeshUnityVertexBuffers) { 
            int vertexCount = fmb.second.second;
            void* unityBufferHandle = fmb.second.first;
            if(!unityBufferHandle) continue;

            glBindBuffer(GL_ARRAY_BUFFER,(GLuint)(size_t)unityBufferHandle);
            GLint bufferSize = 0;
            glGetBufferParameteriv(GL_ARRAY_BUFFER, GL_BUFFER_SIZE, &bufferSize);
            void* mapped = s_mVertexBuffers[fmb.first]; //malloc(bufferSize);
            if(!mapped) { 
                //log_string("No cpu bound vertex buffer found!");
                continue;
            }

            int vertexStride = int(bufferSize / vertexCount);
            char* bufferPtr = (char*)mapped;
            //copy vertices and normals of face_mesh from zcv to unity_mesh
            const float* zFaceVerts = zappar_issue_js_plugin_face_mesh_vertices(fmb.first);
            const float* zFaceNorms = zappar_issue_js_plugin_face_mesh_normals(fmb.first);
            const float* zFaceUVs = zappar_issue_js_plugin_face_mesh_uvs(fmb.first);
            for(int i=0;i<vertexCount;++i){
                MeshVertex& unityVert = *(MeshVertex*)bufferPtr;
                unityVert.pos[0] = zFaceVerts[3*i+0];
                unityVert.pos[1] = zFaceVerts[3*i+1];
                unityVert.pos[2] = -zFaceVerts[3*i+2];
                unityVert.normal[0] = zFaceNorms[3*i+0];
                unityVert.normal[1] = zFaceNorms[3*i+1];
                unityVert.normal[2] = -zFaceNorms[3*i+2];
                unityVert.uv[0] = zFaceUVs[2*i+0];
                unityVert.uv[1] = zFaceUVs[2*i+1];
                bufferPtr += vertexStride;
            }
            //bufferPtr -= bufferSize; //vertexCount * vertexStride
            glBufferSubData(GL_ARRAY_BUFFER,0,bufferSize,mapped);
            //free(mapped);
        }
    }

    extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_update_face_mesh_buffer_callback_native_gl()
    {
        return OnNativeGLFaceMeshEvent;
    }

#endif