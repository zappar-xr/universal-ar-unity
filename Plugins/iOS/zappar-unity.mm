#if defined(TARGET_OS_IPHONE)
#if ZAPPAR_METAL_SUPPORT
#import <Metal/Metal.h>
#endif
#include <OpenGLES/ES2/gl.h>

#import "UnityAppController.h"
#include "zappar-unity.h"

#include <map>
#include <utility>

static id<MTLDevice> s_device = 0;
static IUnityInterfaces* s_interface = 0;
static zappar_pipeline_t s_pipeline = 0;

static std::map<zappar_face_mesh_t,std::pair<void*,int>> s_faceMeshUnityVertexBuffers; //vertex buffer handle and count for each face mesh pipeline
#if !ZAPPAR_METAL_SUPPORT && (TARGET_OS_IOS || TARGET_OS_IPHONE)
static std::map<zappar_face_mesh_t,void*> s_mVertexBuffers; //cpu bound buffer only required for GLES
#endif
// Used here for updating Unity face mesh vertex buffer natively. Do not change the layout!
struct MeshVertex
{
	float pos[3];
	float normal[3];
	//float color[4];
	float uv[2];
};

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

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_set_unity_face_mesh_buffer(zappar_face_mesh_t faceMesh, void* vertexBufferHandle, int vertexCount)
{
    if(vertexBufferHandle==nullptr || vertexCount<=0){
        NSLog(@"Invalid vertex buffer handle or vertexCount, in zappar_set_mesh_buffers_from_unity in face mesh pipeline!");
        return;
    }
    s_faceMeshUnityVertexBuffers[faceMesh] = std::make_pair(vertexBufferHandle,vertexCount);
#if !ZAPPAR_METAL_SUPPORT && (TARGET_OS_IOS || TARGET_OS_IPHONE)
    if(s_mVertexBuffers.find(faceMesh)!=s_mVertexBuffers.end()) free(s_mVertexBuffers.find(faceMesh)->second);
        s_mVertexBuffers[faceMesh] = malloc(sizeof(MeshVertex)*vertexCount);
#endif
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_clear_unity_face_mesh_buffer(zappar_face_mesh_t faceMesh)
{
    auto it = s_faceMeshUnityVertexBuffers.find(faceMesh);
    if(it!=s_faceMeshUnityVertexBuffers.end())
        s_faceMeshUnityVertexBuffers.erase(it);
#if !ZAPPAR_METAL_SUPPORT
    auto it2 = s_mVertexBuffers.find(faceMesh);
    if(it2!=s_mVertexBuffers.end()){
        free(it2->second);
        s_mVertexBuffers.erase(it2);
    }
#endif
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_get_unity_face_mesh_buffers_count(){
    return s_faceMeshUnityVertexBuffers.size();
}

#ifdef ZAPPAR_METAL_SUPPORT
static void UNITY_INTERFACE_API OnNativeMetalFaceMeshEvent(int eventID)
{
    if (s_device != 0 && eventID==1011) {
        for(auto& fmb : s_faceMeshUnityVertexBuffers) {
            int vertexCount = fmb.second.second;
            void* unityBufferHandle = fmb.second.first;
            if(!unityBufferHandle) continue;
            
            id<MTLBuffer> buf = (__bridge id<MTLBuffer>)unityBufferHandle;
            size_t bufferSize = [buf length];
            void* bufferData = [buf contents];
            if(!bufferData) continue;

            int vertexStride = int(bufferSize / vertexCount);
            char* bufferPtr = (char*)bufferData;
            const float* zFaceVerts = zappar_face_mesh_vertices(fmb.first);
            const float* zFaceNorms = zappar_face_mesh_normals(fmb.first);
            const float* zFaceUVs = zappar_face_mesh_uvs(fmb.first);
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
#if !(TARGET_OS_IOS || TARGET_OS_IPHONE)
	        [buf didModifyRange:NSMakeRange(0, buf.length)];
#endif
        }
    }
}
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_update_face_mesh_buffer_callback_native_metal()
{
    return OnNativeMetalFaceMeshEvent;
}
#endif

static void UNITY_INTERFACE_API OnNativeGLFaceMeshEvent(int eventID){
    if(s_interface==nullptr || eventID!=1011) return;
    if(s_device!=nullptr){ NSLog(@"Incorrect GL api being called when metal was selected"); return; }
#if ZAPPAR_METAL_SUPPORT
    return;
#else
    for(auto& fmb : s_faceMeshUnityVertexBuffers) { 
        int vertexCount = fmb.second.second;
        void* unityBufferHandle = fmb.second.first;
        if(!unityBufferHandle) continue;

        glBindBuffer(GL_ARRAY_BUFFER,(GLuint)(size_t)unityBufferHandle);
        GLint bufferSize = 0;
        glGetBufferParameteriv(GL_ARRAY_BUFFER, GL_BUFFER_SIZE, &bufferSize);
#if TARGET_OS_IOS || TARGET_OS_IPHONE
        void* mapped = s_mVertexBuffers[fmb.first]; //malloc(bufferSize);
#else
        void* mapped = glMapBuffer(GL_ARRAY_BUFFER,GL_WRITE_ONLY);
#endif
        if(!mapped) continue;

        int vertexStride = int(bufferSize / vertexCount);
        char* bufferPtr = (char*)mapped;
        const float* zFaceVerts = zappar_face_mesh_vertices(fmb.first);
        const float* zFaceNorms = zappar_face_mesh_normals(fmb.first);
        const float* zFaceUVs = zappar_face_mesh_uvs(fmb.first);
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
#if TARGET_OS_IOS || TARGET_OS_IPHONE
        glBufferSubData(GL_ARRAY_BUFFER,0,bufferSize,mapped);
       //free(mapped);
#else
        glBindBuffer(GL_ARRAY_BUFFER,(GLuint)(size_t)unityBufferHandle);
        glUnmapBuffer(GL_ARRAY_BUFFER);
#endif
    }
#endif
}
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_update_face_mesh_buffer_callback_native_gl()
{
    return OnNativeGLFaceMeshEvent;
}


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
