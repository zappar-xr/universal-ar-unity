#pragma once

#include "Unity/IUnityGraphics.h"
#include "zappar.h"

static void UNITY_INTERFACE_API OnNativeGLRenderEvent(int eventID);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_upload_callback_native_gl();

extern "C" void zappar_pipeline_set(zappar_pipeline_t pipeline);

// Face mesh vertex buffer
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_set_face_mesh_buffers_from_unity(zappar_face_mesh_t faceMesh, void* vertexBufferHandle, int vertexCount);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_clear_face_mesh_buffers_from_unity(zappar_face_mesh_t faceMesh);
extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_get_unity_face_mesh_buffers_count();
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API zappar_update_face_mesh_buffer_callback_native_gl();