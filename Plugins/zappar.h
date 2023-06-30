
#ifndef ZAPPAR_H_
#define ZAPPAR_H_

#ifdef __APPLE__
    #include <TargetConditionals.h>
    #if TARGET_OS_IOS
        #include <OpenGLES/ES2/gl.h>
        #include <OpenGLES/ES2/glext.h>
    #else
        #include <OpenGL/gl.h>
        #include <OpenGL/glext.h>
    #endif
    #include <objc/objc.h>
    #ifdef __OBJC__
        #include <Metal/Metal.h>
    #endif
    #define IOS_OBJ_C
    #define ZAPPAR_EXPORT
#endif

#ifdef EMSCRIPTEN
    #include <emscripten.h>
    #include <GLES/gl.h>
    #define ZAPPAR_EXPORT EMSCRIPTEN_KEEPALIVE
#endif

#ifdef ANDROID
    #include <jni.h>
    #include <GLES/gl.h>
    #define ZAPPAR_EXPORT JNIEXPORT
#endif

#ifdef _WIN32
#include "windows.h"
#include <GL/gl.h>
#include <d3d11.h>
#define ZAPPAR_EXPORT __declspec(dllexport)
#endif

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

enum zappar_barcode_format_t {
    BARCODE_FORMAT_UNKNOWN = 0,
    BARCODE_FORMAT_AZTEC = 1 << 0,
    BARCODE_FORMAT_CODABAR = 1 << 1,
    BARCODE_FORMAT_CODE_39 = 1 << 2,
    BARCODE_FORMAT_CODE_93 = 1 << 3,
    BARCODE_FORMAT_CODE_128 = 1 << 4,
    BARCODE_FORMAT_DATA_MATRIX = 1 << 5,
    BARCODE_FORMAT_EAN_8 = 1 << 6,
    BARCODE_FORMAT_EAN_13 = 1 << 7,
    BARCODE_FORMAT_ITF = 1 << 8,
    BARCODE_FORMAT_MAXICODE = 1 << 9,
    BARCODE_FORMAT_PDF_417 = 1 << 10,
    BARCODE_FORMAT_QR_CODE = 1 << 11,
    BARCODE_FORMAT_RSS_14 = 1 << 12,
    BARCODE_FORMAT_RSS_EXPANDED = 1 << 13,
    BARCODE_FORMAT_UPC_A = 1 << 14,
    BARCODE_FORMAT_UPC_E = 1 << 15,
    BARCODE_FORMAT_UPC_EAN_EXTENSION = 1 << 16,
    BARCODE_FORMAT_ALL = (1 << 17) - 1
};

enum zappar_face_landmark_name_t {
    FACE_LANDMARK_EYE_LEFT = 0,
    FACE_LANDMARK_EYE_RIGHT,
    FACE_LANDMARK_EAR_LEFT,
    FACE_LANDMARK_EAR_RIGHT,
    FACE_LANDMARK_NOSE_BRIDGE,
    FACE_LANDMARK_NOSE_TIP,
    FACE_LANDMARK_NOSE_BASE,
    FACE_LANDMARK_LIP_TOP,
    FACE_LANDMARK_LIP_BOTTOM,
    FACE_LANDMARK_MOUTH_CENTER,
    FACE_LANDMARK_CHIN,
    FACE_LANDMARK_EYEBROW_LEFT,
    FACE_LANDMARK_EYEBROW_RIGHT
};

enum zappar_instant_world_tracker_transform_orientation_t {
    INSTANT_TRACKER_TRANSFORM_ORIENTATION_WORLD = 3,
    INSTANT_TRACKER_TRANSFORM_ORIENTATION_MINUS_Z_AWAY_FROM_USER = 4,
    INSTANT_TRACKER_TRANSFORM_ORIENTATION_MINUS_Z_HEADING = 5,
    INSTANT_TRACKER_TRANSFORM_ORIENTATION_UNCHANGED = 6
};

enum zappar_log_level_t {
    LOG_LEVEL_NONE = 0,
    LOG_LEVEL_ERROR = 1,
    LOG_LEVEL_WARNING = 2,
    LOG_LEVEL_VERBOSE = 3
};

typedef void (*zappar_log_redirect_t)(zappar_log_level_t, const char *);  // Optional method for zcv log redirection

enum zappar_frame_pixel_format_t {
    FRAME_PIXEL_FORMAT_I420 = 0,
    FRAME_PIXEL_FORMAT_I420A = 1,
    FRAME_PIXEL_FORMAT_I422 = 2,
    FRAME_PIXEL_FORMAT_I444 = 3,
    FRAME_PIXEL_FORMAT_NV12 = 4,
    FRAME_PIXEL_FORMAT_RGBA = 5,
    FRAME_PIXEL_FORMAT_BGRA = 6,
    FRAME_PIXEL_FORMAT_Y = 7
};

enum zappar_image_target_type_t {
    IMAGE_TRACKER_TYPE_PLANAR = 0,
    IMAGE_TRACKER_TYPE_CYLINDRICAL = 1,
    IMAGE_TRACKER_TYPE_CONICAL = 2
};

#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_loaded();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const char * zappar_camera_default_device_id(int userFacing);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_camera_count();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const char * zappar_camera_id(int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const char * zappar_camera_name(int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_camera_user_facing(int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_projection_matrix_from_camera_model(const float * model, int renderWidth, int renderHeight);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_projection_matrix_from_camera_model_ext(const float * model, int renderWidth, int renderHeight, float zNear, float zFar);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_initialize();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_invert(const float * inp);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC) || defined(__EMSCRIPTEN__)
ZAPPAR_EXPORT zappar_log_level_t zappar_log_level();
#endif
ZAPPAR_EXPORT void zappar_log_level_set(zappar_log_level_t);
#if defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT void zappar_log_redirect_set(zappar_log_redirect_t);
#endif
#if defined(__ANDROID__)
ZAPPAR_EXPORT void zappar_android_application_context_set(jobject ctx);
#endif
#if defined(IOS_OBJ_C)
ZAPPAR_EXPORT void zappar_ios_uiviewcontroller_set(id);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_permission_request_ui();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_permission_denied_ui();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_permission_request_all();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_permission_request_camera();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_permission_request_motion();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_permission_granted_all();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_permission_granted_camera();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_permission_granted_motion();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_permission_denied_any();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_permission_denied_camera();
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_permission_denied_motion();
#endif
ZAPPAR_EXPORT void zappar_analytics_project_id_set(const char * id, const char * uid);



// ### pipeline ###

typedef struct zappar_pipeline_ti* zappar_pipeline_t;
ZAPPAR_EXPORT zappar_pipeline_t zappar_pipeline_create();
ZAPPAR_EXPORT void zappar_pipeline_destroy(zappar_pipeline_t);

#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_camera_frame_upload_gl(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT GLuint zappar_pipeline_camera_frame_texture_gl(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C)
ZAPPAR_EXPORT void zappar_pipeline_camera_frame_upload_metal(zappar_pipeline_t o, id);
#endif
#if defined(IOS_OBJ_C)
ZAPPAR_EXPORT id zappar_pipeline_camera_frame_texture_metal(zappar_pipeline_t o);
#endif
#if defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_camera_frame_upload_dx11(zappar_pipeline_t o, ID3D11Device* device);
#endif
#if defined(WIN32)
ZAPPAR_EXPORT ID3D11Texture2D* zappar_pipeline_camera_frame_texture_dx11(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_pipeline_camera_frame_texture_matrix(zappar_pipeline_t o, int renderWidth, int renderHeight, int mirror);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_process_gl(zappar_pipeline_t o);
#endif
ZAPPAR_EXPORT void zappar_pipeline_frame_update(zappar_pipeline_t o);
ZAPPAR_EXPORT int zappar_pipeline_frame_number(zappar_pipeline_t o);
ZAPPAR_EXPORT const float * zappar_pipeline_camera_model(zappar_pipeline_t o);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_pipeline_camera_pose_default(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_pipeline_camera_pose_with_attitude(zappar_pipeline_t o, int mirror);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_pipeline_camera_pose_with_origin(zappar_pipeline_t o, const float * pose);
#endif
ZAPPAR_EXPORT int zappar_pipeline_camera_frame_user_data(zappar_pipeline_t o);
ZAPPAR_EXPORT void zappar_pipeline_camera_frame_submit(zappar_pipeline_t o, const char * data, int length, int width, int height, int user_data, const float * camera_to_device_transform, const float * camera_model, int user_facing);
#if defined(IOS_OBJ_C) || defined(TARGET_OS_MAC) || defined(__EMSCRIPTEN__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_camera_frame_submit_raw_pointer(zappar_pipeline_t o, void * data, int dataLength, zappar_frame_pixel_format_t, int width, int height, int user_data, const float * camera_to_device_transform, int rotation, const float * camera_model, int user_facing);
#endif
ZAPPAR_EXPORT const float * zappar_pipeline_camera_frame_camera_attitude(zappar_pipeline_t o);
ZAPPAR_EXPORT const float * zappar_pipeline_camera_frame_device_attitude(zappar_pipeline_t o);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_pipeline_camera_frame_user_facing(zappar_pipeline_t o);
#endif
ZAPPAR_EXPORT void zappar_pipeline_motion_accelerometer_submit(zappar_pipeline_t o, double time, float x, float y, float z);
ZAPPAR_EXPORT void zappar_pipeline_motion_rotation_rate_submit(zappar_pipeline_t o, double time, float x, float y, float z);
ZAPPAR_EXPORT void zappar_pipeline_motion_attitude_submit(zappar_pipeline_t o, double time, float x, float y, float z);
ZAPPAR_EXPORT void zappar_pipeline_motion_attitude_matrix_submit(zappar_pipeline_t o, const float * mat);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_sequence_record_start(zappar_pipeline_t o, int expected_frames);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_sequence_record_stop(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_sequence_record_device_attitude_matrices_set(zappar_pipeline_t o, int val);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const uint8_t * zappar_pipeline_sequence_record_data(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_pipeline_sequence_record_data_size(zappar_pipeline_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_pipeline_sequence_record_clear(zappar_pipeline_t o);
#endif



// ### camera_source ###

typedef struct zappar_camera_source_ti* zappar_camera_source_t;
ZAPPAR_EXPORT zappar_camera_source_t zappar_camera_source_create(zappar_pipeline_t pipeline, const char * device_id);
ZAPPAR_EXPORT void zappar_camera_source_destroy(zappar_camera_source_t);

#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_camera_source_start(zappar_camera_source_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_camera_source_pause(zappar_camera_source_t o);
#endif



// ### sequence_source ###

typedef struct zappar_sequence_source_ti* zappar_sequence_source_t;
ZAPPAR_EXPORT zappar_sequence_source_t zappar_sequence_source_create(zappar_pipeline_t pipeline);
ZAPPAR_EXPORT void zappar_sequence_source_destroy(zappar_sequence_source_t);

#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_sequence_source_start(zappar_sequence_source_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_sequence_source_pause(zappar_sequence_source_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_sequence_source_load_from_memory(zappar_sequence_source_t o, const char * data, int length);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_sequence_source_max_playback_fps_set(zappar_sequence_source_t o, float fps);
#endif



// ### image_tracker ###

typedef struct zappar_image_tracker_ti* zappar_image_tracker_t;
ZAPPAR_EXPORT zappar_image_tracker_t zappar_image_tracker_create(zappar_pipeline_t pipeline);
ZAPPAR_EXPORT void zappar_image_tracker_destroy(zappar_image_tracker_t);

ZAPPAR_EXPORT void zappar_image_tracker_target_load_from_memory(zappar_image_tracker_t o, const char * data, int length);
ZAPPAR_EXPORT int zappar_image_tracker_target_loaded_version(zappar_image_tracker_t o);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT int zappar_image_tracker_target_count(zappar_image_tracker_t o);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT zappar_image_target_type_t zappar_image_tracker_target_type(zappar_image_tracker_t o, int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT float zappar_image_tracker_target_radius_top(zappar_image_tracker_t o, int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT float zappar_image_tracker_target_radius_bottom(zappar_image_tracker_t o, int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT float zappar_image_tracker_target_side_length(zappar_image_tracker_t o, int indx);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT float zappar_image_tracker_target_physical_scale_factor(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const uint8_t * zappar_image_tracker_target_preview_compressed(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_compressed_size(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const char * zappar_image_tracker_target_preview_compressed_mimetype(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const uint8_t * zappar_image_tracker_target_preview_rgba(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_rgba_size(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_rgba_width(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_rgba_height(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT const unsigned short * zappar_image_tracker_target_preview_mesh_indices(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_mesh_indices_size(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT const float * zappar_image_tracker_target_preview_mesh_vertices(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_mesh_vertices_size(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT const float * zappar_image_tracker_target_preview_mesh_normals(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_mesh_normals_size(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT const float * zappar_image_tracker_target_preview_mesh_uvs(zappar_image_tracker_t o, int indx);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32) || defined(TARGET_OS_MAC)
ZAPPAR_EXPORT int zappar_image_tracker_target_preview_mesh_uvs_size(zappar_image_tracker_t o, int indx);
#endif
ZAPPAR_EXPORT int zappar_image_tracker_enabled(zappar_image_tracker_t o);
ZAPPAR_EXPORT void zappar_image_tracker_enabled_set(zappar_image_tracker_t o, int enabled);
ZAPPAR_EXPORT int zappar_image_tracker_anchor_count(zappar_image_tracker_t o);
ZAPPAR_EXPORT const char * zappar_image_tracker_anchor_id(zappar_image_tracker_t o, int indx);
ZAPPAR_EXPORT const float * zappar_image_tracker_anchor_pose_raw(zappar_image_tracker_t o, int indx);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_image_tracker_anchor_pose_camera_relative(zappar_image_tracker_t o, int indx, int mirror);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_image_tracker_anchor_pose(zappar_image_tracker_t o, int indx, const float * camera_pose, int mirror);
#endif



// ### face_tracker ###

typedef struct zappar_face_tracker_ti* zappar_face_tracker_t;
ZAPPAR_EXPORT zappar_face_tracker_t zappar_face_tracker_create(zappar_pipeline_t pipeline);
ZAPPAR_EXPORT void zappar_face_tracker_destroy(zappar_face_tracker_t);

ZAPPAR_EXPORT void zappar_face_tracker_model_load_from_memory(zappar_face_tracker_t o, const char * data, int length);
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_tracker_model_load_default(zappar_face_tracker_t o);
#endif
ZAPPAR_EXPORT int zappar_face_tracker_model_loaded_version(zappar_face_tracker_t o);
ZAPPAR_EXPORT void zappar_face_tracker_enabled_set(zappar_face_tracker_t o, int enabled);
ZAPPAR_EXPORT int zappar_face_tracker_enabled(zappar_face_tracker_t o);
ZAPPAR_EXPORT void zappar_face_tracker_max_faces_set(zappar_face_tracker_t o, int num);
ZAPPAR_EXPORT int zappar_face_tracker_max_faces(zappar_face_tracker_t o);
ZAPPAR_EXPORT int zappar_face_tracker_anchor_count(zappar_face_tracker_t o);
ZAPPAR_EXPORT const char * zappar_face_tracker_anchor_id(zappar_face_tracker_t o, int indx);
ZAPPAR_EXPORT const float * zappar_face_tracker_anchor_pose_raw(zappar_face_tracker_t o, int indx);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_face_tracker_anchor_pose_camera_relative(zappar_face_tracker_t o, int indx, int mirror);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_face_tracker_anchor_pose(zappar_face_tracker_t o, int indx, const float * camera_pose, int mirror);
#endif
ZAPPAR_EXPORT const float * zappar_face_tracker_anchor_identity_coefficients(zappar_face_tracker_t o, int indx);
ZAPPAR_EXPORT const float * zappar_face_tracker_anchor_expression_coefficients(zappar_face_tracker_t o, int indx);



// ### face_mesh ###

typedef struct zappar_face_mesh_ti* zappar_face_mesh_t;
ZAPPAR_EXPORT zappar_face_mesh_t zappar_face_mesh_create();
ZAPPAR_EXPORT void zappar_face_mesh_destroy(zappar_face_mesh_t);

#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_mesh_load_from_memory(zappar_face_mesh_t o, const char * data, int length, int fill_mouth, int fill_eye_l, int fill_eye_r, int fill_neck);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_mesh_load_default(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_mesh_load_default_full_head_simplified(zappar_face_mesh_t o, int fill_mouth, int fill_eye_l, int fill_eye_r, int fill_neck);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_mesh_load_default_face(zappar_face_mesh_t o, int fill_eye_l, int fill_eye_r, int fill_mouth);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_face_mesh_loaded_version(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_mesh_update(zappar_face_mesh_t o, const float * identity, const float * expression, int mirrored);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_face_mesh_indices_size(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const unsigned short * zappar_face_mesh_indices(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_face_mesh_vertices_size(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_face_mesh_vertices(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_face_mesh_normals_size(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_face_mesh_normals(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT int zappar_face_mesh_uvs_size(zappar_face_mesh_t o);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_face_mesh_uvs(zappar_face_mesh_t o);
#endif



// ### face_landmark ###

typedef struct zappar_face_landmark_ti* zappar_face_landmark_t;
ZAPPAR_EXPORT zappar_face_landmark_t zappar_face_landmark_create(zappar_face_landmark_name_t);
ZAPPAR_EXPORT void zappar_face_landmark_destroy(zappar_face_landmark_t);

#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT void zappar_face_landmark_update(zappar_face_landmark_t o, const float * identity, const float * expression, int mirrored);
#endif
#if defined(__ANDROID__) || defined(IOS_OBJ_C) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_face_landmark_anchor_pose(zappar_face_landmark_t o);
#endif



// ### barcode_finder ###

typedef struct zappar_barcode_finder_ti* zappar_barcode_finder_t;
ZAPPAR_EXPORT zappar_barcode_finder_t zappar_barcode_finder_create(zappar_pipeline_t pipeline);
ZAPPAR_EXPORT void zappar_barcode_finder_destroy(zappar_barcode_finder_t);

ZAPPAR_EXPORT void zappar_barcode_finder_enabled_set(zappar_barcode_finder_t o, int enabled);
ZAPPAR_EXPORT int zappar_barcode_finder_enabled(zappar_barcode_finder_t o);
ZAPPAR_EXPORT int zappar_barcode_finder_found_number(zappar_barcode_finder_t o);
ZAPPAR_EXPORT const char * zappar_barcode_finder_found_text(zappar_barcode_finder_t o, int indx);
ZAPPAR_EXPORT zappar_barcode_format_t zappar_barcode_finder_found_format(zappar_barcode_finder_t o, int indx);
ZAPPAR_EXPORT zappar_barcode_format_t zappar_barcode_finder_formats(zappar_barcode_finder_t o);
ZAPPAR_EXPORT void zappar_barcode_finder_formats_set(zappar_barcode_finder_t o, zappar_barcode_format_t);



// ### instant_world_tracker ###

typedef struct zappar_instant_world_tracker_ti* zappar_instant_world_tracker_t;
ZAPPAR_EXPORT zappar_instant_world_tracker_t zappar_instant_world_tracker_create(zappar_pipeline_t pipeline);
ZAPPAR_EXPORT void zappar_instant_world_tracker_destroy(zappar_instant_world_tracker_t);

ZAPPAR_EXPORT void zappar_instant_world_tracker_enabled_set(zappar_instant_world_tracker_t o, int enabled);
ZAPPAR_EXPORT int zappar_instant_world_tracker_enabled(zappar_instant_world_tracker_t o);
ZAPPAR_EXPORT const float * zappar_instant_world_tracker_anchor_pose_raw(zappar_instant_world_tracker_t o);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_instant_world_tracker_anchor_pose_camera_relative(zappar_instant_world_tracker_t o, int mirror);
#endif
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT const float * zappar_instant_world_tracker_anchor_pose(zappar_instant_world_tracker_t o, const float * camera_pose, int mirror);
#endif
ZAPPAR_EXPORT void zappar_instant_world_tracker_anchor_pose_set_from_camera_offset_raw(zappar_instant_world_tracker_t o, float x, float y, float z, zappar_instant_world_tracker_transform_orientation_t);
#if defined(IOS_OBJ_C) || defined(__ANDROID__) || defined(WIN32)
ZAPPAR_EXPORT void zappar_instant_world_tracker_anchor_pose_set_from_camera_offset(zappar_instant_world_tracker_t o, float x, float y, float z, zappar_instant_world_tracker_transform_orientation_t);
#endif



#ifdef __cplusplus
}
#endif

#endif // #ifndef ZAPPAR_H_
