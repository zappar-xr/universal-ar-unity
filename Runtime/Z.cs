using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Networking;

// Ignore unreachable code warnings in Play mode. 
// We exit early from computer vision functions to allow Play mode to run.
#pragma warning disable 0162

namespace Zappar {

public static class Config
{
#if UNITY_EDITOR_OSX || UNITY_OSX
    public const string PluginName = "zcv-macos";
#elif UNITY_ANDROID || UNITY_EDITOR_WIN
    public const string PluginName = "zcv";
#else                       
    public const string PluginName = "__Internal";
#endif

#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
    public const string ImageTrainPlugin = "ImageTraining";
#endif
}

public class Z
{
    public enum BarcodeFormat : uint {
        Unknown = 0,
        Aztec = 1 << 0,
        Codabar = 1 << 1,
        Code39 = 1 << 2,
        Code93 = 1 << 3,
        Code128 = 1 << 4,
        DataMatrix = 1 << 5,
        EAN8 = 1 << 6,
        EAN13 = 1 << 7,
        ITF = 1 << 8,
        MaxiCode = 1 << 9,
        PDF417 = 1 << 10,
        QRCode = 1 << 11,
        RSS14 = 1 << 12,
        RSSExpanded = 1 << 13,
        UPCA = 1 << 14,
        UPCE = 1 << 15,
        UPCEANExtension = 1 << 16,
        All = (1 << 17) - 1
    }

    public enum InstantTrackerTransformOrientation : uint {
        WORLD = 3,
        MINUS_Z_AWAY_FROM_USER = 4,
        MINUS_Z_HEADING = 5,
        UNCHANGED = 6
    }

    public enum LogLevel : uint {
        NONE = 0,
        ERROR = 1,
        WARNING = 2,
        VERBOSE = 3
    }

    public enum DebugMode : uint {
        None = 0,
        File = 1,
        UnityLog = 2
    }

    public enum FramePixelFormat : uint {
        FRAME_PIXEL_FORMAT_I420 = 0,
        FRAME_PIXEL_FORMAT_I420A = 1,
        FRAME_PIXEL_FORMAT_I422 = 2,
        FRAME_PIXEL_FORMAT_I444 = 3,
        FRAME_PIXEL_FORMAT_NV12 = 4,
        FRAME_PIXEL_FORMAT_RGBA = 5,
        FRAME_PIXEL_FORMAT_BGRA = 6,
        FRAME_PIXEL_FORMAT_Y = 7
    }

#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugLogDelegate(string str);

    [DllImport(Config.PluginName, EntryPoint = "ZapparSetDebugMode")]
    public static extern void SetDebugMode(DebugMode mode, LogLevel level);
    [DllImport(Config.PluginName, EntryPoint = "ZapparSetLogFunc")]
    public static extern void SetLogFunc(DebugLogDelegate func);
    [DllImport(Config.PluginName, EntryPoint = "ZapparSetErrorFunc")]
    public static extern void SetErrorFunc(DebugLogDelegate func);

    [StructLayout(LayoutKind.Sequential)]
    public struct FileData
    {
        public IntPtr data;
        public int length;
    }

    [DllImport(Config.ImageTrainPlugin, CallingConvention = CallingConvention.Cdecl)]
    public static extern int TrainImageCompressed(ref FileData compressed, ref FileData zpt, ref FileData preview, int preview_is_jpeg);
    [DllImport(Config.ImageTrainPlugin, CallingConvention = CallingConvention.Cdecl)]
    public static extern int TrainImageCompressedWithMax(ref FileData compressed, ref FileData zpt, ref FileData preview, int preview_is_jpeg, int max_width, int max_height);
    [DllImport(Config.ImageTrainPlugin, CallingConvention = CallingConvention.Cdecl)]
    public static extern int TrainImageCompressedWithMaxCurved(ref FileData compressed, ref FileData zpt, ref FileData preview, int preview_is_jpeg, int max_width, int max_height, float top_radius, float bottom_radius, float side_length, float physical_scale_factor);
    [DllImport(Config.ImageTrainPlugin, CallingConvention = CallingConvention.Cdecl)]
    public static extern void TrainImageFreeFileData(ref FileData d);
    
#endif

    private const string ZCVResourcesPath = "Packages/com.zappar.uar/Contents/";

#pragma warning disable 0162
        public static string FaceTrackingModelPath()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN

            if (Directory.Exists(ZCVResourcesPath))
                return ZCVResourcesPath + "face_tracking_model.zbin";            
            return Application.dataPath + "/Zappar/Contents/face_tracking_model.zbin";
#endif
            return "";
        }

    public static string FaceMeshFaceModelPath() {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
            if (Directory.Exists(ZCVResourcesPath))
                return ZCVResourcesPath + "face_mesh_face_model.zbin";
            return Application.dataPath + "/Zappar/Contents/face_mesh_face_model.zbin";
#endif
        return "";
    }

    public static string FaceMeshFullHeadSimplifiedModelPath() {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
            if (Directory.Exists(ZCVResourcesPath))
                return ZCVResourcesPath + "face_mesh_full_head_simplified_model.zbin";
            return Application.dataPath + "/Zappar/Contents/face_mesh_full_head_simplified_model.zbin";
#endif
        return "";
    }
#pragma warning restore 0162

// BEGIN AUTOGEN

        [DllImport(Config.PluginName)]
    private static extern int zappar_loaded();
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_camera_default_device_id(int userFacing);
    [DllImport(Config.PluginName)]
    private static extern int zappar_camera_count();
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_camera_id(int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_camera_name(int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_camera_user_facing(int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_projection_matrix_from_camera_model(float[] model, int renderWidth, int renderHeight);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_projection_matrix_from_camera_model_ext(float[] model, int renderWidth, int renderHeight, float zNear, float zFar);
    [DllImport(Config.PluginName)]
    private static extern void zappar_initialize();


    [DllImport(Config.PluginName)]
    private static extern void zappar_log_level_set(uint level);



    [DllImport(Config.PluginName)]
    private static extern void zappar_permission_request_ui();
    [DllImport(Config.PluginName)]
    private static extern void zappar_permission_denied_ui();
    [DllImport(Config.PluginName)]
    private static extern void zappar_permission_request_all();
    [DllImport(Config.PluginName)]
    private static extern void zappar_permission_request_camera();
    [DllImport(Config.PluginName)]
    private static extern void zappar_permission_request_motion();
    [DllImport(Config.PluginName)]
    private static extern int zappar_permission_granted_all();
    [DllImport(Config.PluginName)]
    private static extern int zappar_permission_granted_camera();
    [DllImport(Config.PluginName)]
    private static extern int zappar_permission_granted_motion();
    [DllImport(Config.PluginName)]
    private static extern int zappar_permission_denied_any();
    [DllImport(Config.PluginName)]
    private static extern int zappar_permission_denied_camera();
    [DllImport(Config.PluginName)]
    private static extern int zappar_permission_denied_motion();
    [DllImport(Config.PluginName)]
    private static extern void zappar_analytics_project_id_set(string id);
    
        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_create();
        [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_destroy(IntPtr o);

    
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_frame_texture_gl(IntPtr o);

    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_frame_texture_metal(IntPtr o);

    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_frame_texture_dx11(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_frame_texture_matrix(IntPtr o, int renderWidth, int renderHeight, int mirror);

    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_frame_update(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_pipeline_frame_number(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_model(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_pose_default(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_pose_with_attitude(IntPtr o, int mirror);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_pose_with_origin(IntPtr o, float[] pose);
    [DllImport(Config.PluginName)]
    private static extern int zappar_pipeline_camera_frame_user_data(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_camera_frame_submit(IntPtr o, byte[] data, int size, int width, int height, int user_data, float[] camera_to_device_transform, float[] camera_model, int user_facing);

    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_frame_camera_attitude(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_camera_frame_device_attitude(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_pipeline_camera_frame_user_facing(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_motion_accelerometer_submit(IntPtr o, double time, float x, float y, float z);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_motion_rotation_rate_submit(IntPtr o, double time, float x, float y, float z);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_motion_attitude_submit(IntPtr o, double time, float x, float y, float z);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_motion_attitude_matrix_submit(IntPtr o, float[] mat);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_sequence_record_start(IntPtr o, int expected_frames);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_sequence_record_stop(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_sequence_record_device_attitude_matrices_set(IntPtr o, int val);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_pipeline_sequence_record_data(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_pipeline_sequence_record_data_size(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_sequence_record_clear(IntPtr o);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_camera_source_create(IntPtr pipeline, string device_id);
        [DllImport(Config.PluginName)]
    private static extern void zappar_camera_source_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_camera_source_start(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_camera_source_pause(IntPtr o);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_sequence_source_create(IntPtr pipeline);
        [DllImport(Config.PluginName)]
    private static extern void zappar_sequence_source_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_sequence_source_start(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_sequence_source_pause(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_sequence_source_load_from_memory(IntPtr o, byte[] data, int size);
    [DllImport(Config.PluginName)]
    private static extern void zappar_sequence_source_max_playback_fps_set(IntPtr o, float fps);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_create(IntPtr pipeline);
        [DllImport(Config.PluginName)]
    private static extern void zappar_image_tracker_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_image_tracker_target_load_from_memory(IntPtr o, byte[] data, int size);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_loaded_version(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_count(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern uint zappar_image_tracker_target_type(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern float zappar_image_tracker_target_radius_top(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern float zappar_image_tracker_target_radius_bottom(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern float zappar_image_tracker_target_side_length(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern float zappar_image_tracker_target_physical_scale_factor(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_compressed(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_compressed_size(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_compressed_mimetype(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_rgba(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_rgba_size(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_rgba_width(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_rgba_height(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_mesh_indices(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_mesh_indices_size(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_mesh_vertices(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_mesh_vertices_size(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_mesh_normals(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_mesh_normals_size(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_target_preview_mesh_uvs(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_target_preview_mesh_uvs_size(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_enabled(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_image_tracker_enabled_set(IntPtr o, int enabled);
    [DllImport(Config.PluginName)]
    private static extern int zappar_image_tracker_anchor_count(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_anchor_id(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_anchor_pose_raw(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_anchor_pose_camera_relative(IntPtr o, int indx, int mirror);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_image_tracker_anchor_pose(IntPtr o, int indx, float[] camera_pose, int mirror);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_create(IntPtr pipeline);
        [DllImport(Config.PluginName)]
    private static extern void zappar_face_tracker_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_face_tracker_model_load_from_memory(IntPtr o, byte[] data, int size);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_tracker_model_load_default(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_tracker_model_loaded_version(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_tracker_enabled_set(IntPtr o, int enabled);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_tracker_enabled(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_tracker_max_faces_set(IntPtr o, int num);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_tracker_max_faces(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_tracker_anchor_count(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_anchor_id(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_anchor_pose_raw(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_anchor_pose_camera_relative(IntPtr o, int indx, int mirror);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_anchor_pose(IntPtr o, int indx, float[] camera_pose, int mirror);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_anchor_identity_coefficients(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_tracker_anchor_expression_coefficients(IntPtr o, int indx);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_mesh_create();
        [DllImport(Config.PluginName)]
    private static extern void zappar_face_mesh_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_face_mesh_load_from_memory(IntPtr o, byte[] data, int size, int fill_mouth, int fill_eye_l, int fill_eye_r, int fill_neck);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_mesh_load_default(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_mesh_load_default_full_head_simplified(IntPtr o, int fill_mouth, int fill_eye_l, int fill_eye_r, int fill_neck);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_mesh_load_default_face(IntPtr o, int fill_eye_l, int fill_eye_r, int fill_mouth);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_mesh_loaded_version(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_face_mesh_update(IntPtr o, float[] identity, float[] expression, int mirrored);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_mesh_indices_size(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_mesh_indices(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_mesh_vertices_size(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_mesh_vertices(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_mesh_normals_size(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_mesh_normals(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_face_mesh_uvs_size(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_mesh_uvs(IntPtr o);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_landmark_create(uint landmark);
        [DllImport(Config.PluginName)]
    private static extern void zappar_face_landmark_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_face_landmark_update(IntPtr o, float[] identity, float[] expression, int mirrored);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_face_landmark_anchor_pose(IntPtr o);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_barcode_finder_create(IntPtr pipeline);
        [DllImport(Config.PluginName)]
    private static extern void zappar_barcode_finder_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_barcode_finder_enabled_set(IntPtr o, int enabled);
    [DllImport(Config.PluginName)]
    private static extern int zappar_barcode_finder_enabled(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern int zappar_barcode_finder_found_number(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_barcode_finder_found_text(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern uint zappar_barcode_finder_found_format(IntPtr o, int indx);
    [DllImport(Config.PluginName)]
    private static extern uint zappar_barcode_finder_formats(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern void zappar_barcode_finder_formats_set(IntPtr o, uint f);
    

        [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_instant_world_tracker_create(IntPtr pipeline);
        [DllImport(Config.PluginName)]
    private static extern void zappar_instant_world_tracker_destroy(IntPtr o);

        [DllImport(Config.PluginName)]
    private static extern void zappar_instant_world_tracker_enabled_set(IntPtr o, int enabled);
    [DllImport(Config.PluginName)]
    private static extern int zappar_instant_world_tracker_enabled(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_instant_world_tracker_anchor_pose_raw(IntPtr o);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_instant_world_tracker_anchor_pose_camera_relative(IntPtr o, int mirror);
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_instant_world_tracker_anchor_pose(IntPtr o, float[] camera_pose, int mirror);
    [DllImport(Config.PluginName)]
    private static extern void zappar_instant_world_tracker_anchor_pose_set_from_camera_offset_raw(IntPtr o, float x, float y, float z, uint orientation);
    [DllImport(Config.PluginName)]
    private static extern void zappar_instant_world_tracker_anchor_pose_set_from_camera_offset(IntPtr o, float x, float y, float z, uint orientation);
    

// END AUTOGEN
#pragma warning disable 0414
    private static bool haveSetApplicationContext = false;
#pragma warning restore 0414
    public static void AndroidApplicationContextSet() {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (haveSetApplicationContext) return;
        haveSetApplicationContext = true;
        
        AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
        AndroidJavaObject activity = player.GetStatic <AndroidJavaObject>("currentActivity");
        
        // retrieve the Unity Android context
        AndroidJavaObject[] args = new AndroidJavaObject[1];
        args[0] = activity.Call<AndroidJavaObject>("getApplicationContext");

        // Retrieve the zappar library
        IntPtr zcv_JNI = AndroidJNI.FindClass("com/zappar/zcv/ZCV");
        
        bool isStatic = true;
        string methodSignature = "(Ljava/lang/Object;)V";
        string methodName = "android_application_context_set";

        // Retrieve the function we want to call
        IntPtr method_AndroidApplicationContextSet = AndroidJNIHelper.GetMethodID(zcv_JNI, methodName, methodSignature, isStatic);

        // pass the Unity context to the zappar library
        AndroidJNI.CallStaticVoidMethod(zcv_JNI, method_AndroidApplicationContextSet, AndroidJNIHelper.CreateJNIArgArray(args));
#endif
    }

    [DllImport(Config.PluginName)]
    private static extern int zappar_has_initialized();

#if UNITY_WEBGL
    [DllImport ("__Internal")]
    private static extern IntPtr zappar_process_callback_gl();
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport ("__Internal")]
    private static extern bool zappar_is_visible_webgl();
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("ZCVAndroidRenderPlugin")]        
#else
    [DllImport(Config.PluginName)]
#endif
    private static extern IntPtr zappar_upload_callback_native_gl();

#if UNITY_IOS || UNITY_EDITOR_OSX
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_upload_callback_native_metal();
#endif

#if UNITY_EDITOR_WIN
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_upload_callback_native_dx11();
#endif

#if UNITY_WEBGL
    [DllImport(Config.PluginName)]
    private static extern void zappar_pipeline_gl_context_set(IntPtr pipeline);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("ZCVAndroidRenderPlugin")]        
#else
    [DllImport(Config.PluginName)]
#endif
    private static extern void zappar_pipeline_set(IntPtr o);

#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("ZCVAndroidRenderPlugin")]        
#else
    [DllImport(Config.PluginName)]
#endif
    private static extern void zappar_set_unity_face_mesh_buffer(IntPtr o, IntPtr vertexBuffer, int vertexCount);

#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("ZCVAndroidRenderPlugin")]        
#else
    [DllImport(Config.PluginName)]
#endif
    private static extern void zappar_clear_unity_face_mesh_buffer(IntPtr o);
    
#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("ZCVAndroidRenderPlugin")]        
#else
    [DllImport(Config.PluginName)]
#endif
    private static extern int zappar_get_unity_face_mesh_buffers_count();

    public static void FaceMeshSetVertexBuffer(IntPtr faceMesh, Mesh unityMesh)
    {
        IntPtr meshVertexBuffer = unityMesh.GetNativeVertexBufferPtr(0);
        zappar_set_unity_face_mesh_buffer(faceMesh, meshVertexBuffer, unityMesh.vertexCount);
    }

    public static void FaceMeshClearVertexBuffer(IntPtr faceMesh)
    {
        zappar_clear_unity_face_mesh_buffer(faceMesh);
    }

    private static Dictionary<IntPtr, Texture2D> m_texturePool = new Dictionary<IntPtr, Texture2D>();

    public static void PipelineGLContextSet(IntPtr pipeline) {
#if UNITY_WEBGL && !UNITY_EDITOR
        zappar_pipeline_gl_context_set(pipeline);
#endif
    }

    public static bool HasInitialized() {
        return zappar_has_initialized() == 1 ? true : false;
    }

    public static bool ZapparInFocus()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return zappar_is_visible_webgl();
#else
        return true;
#endif
    }

    public static Matrix4x4 PipelineProjectionMatrix(IntPtr o, int renderWidth, int renderHeight, float zNear, float zFar, ref float[] cameraModel) {
        IntPtr ret = zappar_pipeline_camera_model(o);
        Marshal.Copy(ret, cameraModel, 0, 6);
        if (cameraModel[0] == 0) return Matrix4x4.identity;
        return ProjectionMatrixFromCameraModelExt(cameraModel, renderWidth, renderHeight, zNear, zFar);
    }

    public static Matrix4x4 WorldToCameraMatrix() {

        Matrix4x4 transform = Matrix4x4.identity;
        transform.SetRow(2, new Vector4(0, 0, -1, 0)); 

        if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal)
            return transform;

        transform.SetRow(3, new Vector4(0, 0, 0, -1)); 
        return transform;
    }

    public static void Process(IntPtr pipeline)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        zappar_pipeline_set(pipeline);
        GL.IssuePluginEvent(zappar_process_callback_gl(), 1);
#endif
    }

    public static Texture2D PipelineCameraFrameTexture(IntPtr o)
    {

        IntPtr ptr = IntPtr.Zero;
        Texture2D texture = null;

        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
        {
            ptr = zappar_pipeline_camera_frame_texture_gl(o);
        }
#if UNITY_IOS || UNITY_EDITOR_OSX
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
            ptr = zappar_pipeline_camera_frame_texture_metal(o);
#elif UNITY_EDITOR_WIN
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11)
            ptr = zappar_pipeline_camera_frame_texture_dx11(o);
#endif
        if (ptr != IntPtr.Zero)
        {
            if (!m_texturePool.TryGetValue(ptr, out texture))
            {
                texture = Texture2D.CreateExternalTexture(4, 4, TextureFormat.ARGB32, false, false, ptr);
                m_texturePool.Add(ptr, texture);
            }
        }
        return texture;
    }

    public static void CameraFrameUpload(IntPtr pipeline)
    {

        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore) 
        {
            zappar_pipeline_set(pipeline);
            GL.IssuePluginEvent(zappar_upload_callback_native_gl(), 1);
        }
#if UNITY_EDITOR_WIN
        else if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11) 
        {
            zappar_pipeline_set(pipeline);
            GL.IssuePluginEvent(zappar_upload_callback_native_dx11(), 1);
        }
#elif UNITY_IOS || UNITY_EDITOR_OSX
        else if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal) 
        {
            zappar_pipeline_set(pipeline);
            GL.IssuePluginEvent(zappar_upload_callback_native_metal(), 1);
        }
#endif
        else { 
            Debug.LogError("Unsupported graphics API. Please switch to either - DirectX11, OpenGL, or Metal depending upon your target platform");
        }
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("ZCVAndroidRenderPlugin")]        
#else
    [DllImport(Config.PluginName)]
#endif
    private static extern IntPtr zappar_update_face_mesh_buffer_callback_native_gl();

#if UNITY_IOS || UNITY_EDITOR_OSX
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_update_face_mesh_buffer_callback_native_metal();
#endif

#if UNITY_EDITOR_WIN
    [DllImport(Config.PluginName)]
    private static extern IntPtr zappar_update_face_mesh_buffer_callback_native_dx11();
#endif
    public static void FaceMeshUpdateVertexBuffer()
    {
        if(zappar_get_unity_face_mesh_buffers_count()<=0) return;
        if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.OpenGLES2 || 
            SystemInfo.graphicsDeviceType==GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
        {
            GL.IssuePluginEvent(zappar_update_face_mesh_buffer_callback_native_gl(), 1011);
        }
#if UNITY_EDITOR_WIN
        else if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11)
        {
            GL.IssuePluginEvent(zappar_update_face_mesh_buffer_callback_native_dx11(), 1011);
        }
#elif UNITY_IOS || UNITY_EDITOR_OSX
        else if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal) 
        {
            GL.IssuePluginEvent(zappar_update_face_mesh_buffer_callback_native_metal(), 1011);
        }
#endif
        else
        {
            Debug.LogError("Unsupported graphics API. Please switch to either - DirectX11, OpenGL, orMetaldependingupo your target platform");
        }
    }

    private static string GetValidZPTFilename(string filename)
    {
        DirectoryInfo directory = new DirectoryInfo(Application.streamingAssetsPath);
        FileInfo[] files = directory.GetFiles("*.zpt");
        foreach (FileInfo fileHandle in files)
        {
            if (fileHandle.Name.StartsWith(filename))
            {
                return fileHandle.Name;
            }
        }
        return "";
    }

    public static IEnumerator LoadZPTTarget(string filename, Action<byte[]> callback)
    {
        string zptFilename = filename;
        if (zptFilename != "" && zptFilename.EndsWith("zpt"))
        {
            string url = Path.Combine(Application.streamingAssetsPath, zptFilename);
            byte[] data = null;

#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_ANDROID)
    UnityWebRequest request = UnityWebRequest.Get(url);
    yield return request.SendWebRequest();
    data = request.downloadHandler.data;
#else
            data = File.ReadAllBytes(url);
#endif
            callback(data);
        }
        else
        {
            Debug.LogError("[Zappar]: Please specify a valid ZPT file for your image target in the inspector. You must specify the full filename, and the zpt file must have 'zpt' as an extension.");
        }
        
        yield return null;
    }

    public static Matrix4x4 ConvertToUnityPose(Matrix4x4 pose) 
    {
        Matrix4x4 transform = Matrix4x4.identity;
        transform[2,2] = -1;
        return transform * pose * transform;
    }

    public static Vector3 GetPosition(Matrix4x4 pose)
    {
        return pose.GetColumn(3);
    }

    public static Quaternion GetRotation(Matrix4x4 pose)
    {
        return Quaternion.LookRotation(pose.GetColumn(2), pose.GetColumn(1));
    }

    public static Vector3 GetScale(Matrix4x4 pose)
    {
        return new Vector3(pose.GetColumn(0).magnitude,pose.GetColumn(1).magnitude,pose.GetColumn(2).magnitude);
    }

    public static int[] UpdateFaceMeshTrianglesForUnity(int[] meshTriangles) 
    {
        int N = meshTriangles.Length;
        int[] triangles = new int[N];
        for (int i = 0; i < triangles.Length; i+=3) 
        {
            triangles[i+0] = meshTriangles[i+2];
            triangles[i+1] = meshTriangles[i+1];
            triangles[i+2] = meshTriangles[i+0];
        }
        return triangles;     
    }

    public static Vector3[] UpdateFaceMeshVerticesForUnity(float[] meshVertices) 
    {
        int N = meshVertices.Length / 3;
        Vector3[] vertices = new Vector3[N];
        for (int i = 0; i < N; ++i) 
        {   
            vertices[i].Set(meshVertices[i*3+0], 
                            meshVertices[i*3+1], 
                            meshVertices[i*3+2]*-1);
        }
        return vertices;
    }

    public static Vector2[] UpdateFaceMeshUVsForUnity(float[] meshUVs) 
    {
        int N = meshUVs.Length/2;
        Vector2[] uv = new Vector2[N];
        for (int i = 0; i < N; ++i) 
            uv[i].Set(meshUVs[i*2+0], meshUVs[i*2+1]);
        return uv;
    }

    public static Vector3[] UpdateFaceMeshNormalsForUnity(float[] meshNormals) 
    {
        int N = meshNormals.Length / 3;
        Vector3[] normals = new Vector3[N];
        for (int i = 0; i < N; ++i) 
        {   
            normals[i].Set(meshNormals[i*3+0], 
                            meshNormals[i*3+1], 
                            meshNormals[i*3+2]*-1);
        }
        return normals;
    }

    public static byte[] LoadRawBytes(string filename) {
        return File.ReadAllBytes(filename);
    }

    // BEGIN AUTOGEN

    public static bool Loaded() {
        
        int ret = zappar_loaded();
        return (ret == 1) ? true : false;
    }
	public static string CameraDefaultDeviceId(bool userFacing) {
        
        IntPtr ret = zappar_camera_default_device_id(userFacing ? 1 : 0);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static int CameraCount() {
        
        int ret = zappar_camera_count();
        return ret;
    }
	public static string CameraId(int indx) {
        
        IntPtr ret = zappar_camera_id(indx);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static string CameraName(int indx) {
        
        IntPtr ret = zappar_camera_name(indx);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static bool CameraUserFacing(int indx) {
        
        int ret = zappar_camera_user_facing(indx);
        return (ret == 1) ? true : false;
    }
	public static Matrix4x4 ProjectionMatrixFromCameraModel(float[] model, int renderWidth, int renderHeight) {
        
	
	
        IntPtr ret = zappar_projection_matrix_from_camera_model(model, renderWidth, renderHeight);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 ProjectionMatrixFromCameraModelExt(float[] model, int renderWidth, int renderHeight, float zNear, float zFar) {
        
	
	
	
	
        IntPtr ret = zappar_projection_matrix_from_camera_model_ext(model, renderWidth, renderHeight, zNear, zFar);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static void Initialize() {
        
        zappar_initialize();
        
    }
	
	
	public static void LogLevelSet(LogLevel level) {
        
        zappar_log_level_set((uint)level);
        
    }
	
	
	
	public static void PermissionRequestUi() {
        
        zappar_permission_request_ui();
        
    }
	public static void PermissionDeniedUi() {
        
        zappar_permission_denied_ui();
        
    }
	public static void PermissionRequestAll() {
        
        zappar_permission_request_all();
        
    }
	public static void PermissionRequestCamera() {
        
        zappar_permission_request_camera();
        
    }
	public static void PermissionRequestMotion() {
        
        zappar_permission_request_motion();
        
    }
	public static bool PermissionGrantedAll() {
        
        int ret = zappar_permission_granted_all();
        return (ret == 1) ? true : false;
    }
	public static bool PermissionGrantedCamera() {
        
        int ret = zappar_permission_granted_camera();
        return (ret == 1) ? true : false;
    }
	public static bool PermissionGrantedMotion() {
        
        int ret = zappar_permission_granted_motion();
        return (ret == 1) ? true : false;
    }
	public static bool PermissionDeniedAny() {
        
        int ret = zappar_permission_denied_any();
        return (ret == 1) ? true : false;
    }
	public static bool PermissionDeniedCamera() {
        
        int ret = zappar_permission_denied_camera();
        return (ret == 1) ? true : false;
    }
	public static bool PermissionDeniedMotion() {
        
        int ret = zappar_permission_denied_motion();
        return (ret == 1) ? true : false;
    }
	public static void AnalyticsProjectIdSet(string id) {
        
        zappar_analytics_project_id_set(id);
        
    }
    
	public static IntPtr PipelineCameraFrameTextureGl(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_frame_texture_gl(o);
        return ret;
    }
	
	public static IntPtr PipelineCameraFrameTextureMetal(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_frame_texture_metal(o);
        return ret;
    }
	
	public static IntPtr PipelineCameraFrameTextureDx11(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_frame_texture_dx11(o);
        return ret;
    }
	public static Matrix4x4 PipelineCameraFrameTextureMatrix(IntPtr o, int renderWidth, int renderHeight, bool mirror) {
        
	
	
	
        IntPtr ret = zappar_pipeline_camera_frame_texture_matrix(o, renderWidth, renderHeight, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	
	public static void PipelineFrameUpdate(IntPtr o) {
        
        zappar_pipeline_frame_update(o);
        
    }
	public static int PipelineFrameNumber(IntPtr o) {
        
        int ret = zappar_pipeline_frame_number(o);
        return ret;
    }
	public static float[] PipelineCameraModel(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_model(o);
        float[] retArr = new float[6];
        Marshal.Copy(ret, retArr, 0, 6);
        return retArr;
    }
	public static Matrix4x4 PipelineCameraPoseDefault(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_pose_default(o);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 PipelineCameraPoseWithAttitude(IntPtr o, bool mirror) {
        
	
        IntPtr ret = zappar_pipeline_camera_pose_with_attitude(o, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 PipelineCameraPoseWithOrigin(IntPtr o, Matrix4x4 pose) {
        
	float[] arg_pose = new float[16];
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                arg_pose[i * 4 + k] = pose[k, i];
        IntPtr ret = zappar_pipeline_camera_pose_with_origin(o, arg_pose);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static int PipelineCameraFrameUserData(IntPtr o) {
        
        int ret = zappar_pipeline_camera_frame_user_data(o);
        return ret;
    }
	public static void PipelineCameraFrameSubmit(IntPtr o, byte[] data, int width, int height, int user_data, Matrix4x4 camera_to_device_transform, float[] camera_model, bool user_facing) {
        
	
	
	
	
	float[] arg_camera_to_device_transform = new float[16];
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                arg_camera_to_device_transform[i * 4 + k] = camera_to_device_transform[k, i];
	
	
        zappar_pipeline_camera_frame_submit(o, data, data.Length, width, height, user_data, arg_camera_to_device_transform, camera_model, user_facing ? 1 : 0);
        
    }
	
	public static Matrix4x4 PipelineCameraFrameCameraAttitude(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_frame_camera_attitude(o);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 PipelineCameraFrameDeviceAttitude(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_camera_frame_device_attitude(o);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static bool PipelineCameraFrameUserFacing(IntPtr o) {
        
        int ret = zappar_pipeline_camera_frame_user_facing(o);
        return (ret == 1) ? true : false;
    }
	public static void PipelineMotionAccelerometerSubmit(IntPtr o, double time, float x, float y, float z) {
        
	
	
	
	
        zappar_pipeline_motion_accelerometer_submit(o, time, x, y, z);
        
    }
	public static void PipelineMotionRotationRateSubmit(IntPtr o, double time, float x, float y, float z) {
        
	
	
	
	
        zappar_pipeline_motion_rotation_rate_submit(o, time, x, y, z);
        
    }
	public static void PipelineMotionAttitudeSubmit(IntPtr o, double time, float x, float y, float z) {
        
	
	
	
	
        zappar_pipeline_motion_attitude_submit(o, time, x, y, z);
        
    }
	public static void PipelineMotionAttitudeMatrixSubmit(IntPtr o, Matrix4x4 mat) {
        
	float[] arg_mat = new float[16];
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                arg_mat[i * 4 + k] = mat[k, i];
        zappar_pipeline_motion_attitude_matrix_submit(o, arg_mat);
        
    }
	public static void PipelineSequenceRecordStart(IntPtr o, int expected_frames) {
        
	
        zappar_pipeline_sequence_record_start(o, expected_frames);
        
    }
	public static void PipelineSequenceRecordStop(IntPtr o) {
        
        zappar_pipeline_sequence_record_stop(o);
        
    }
	public static void PipelineSequenceRecordDeviceAttitudeMatricesSet(IntPtr o, bool val) {
        
	
        zappar_pipeline_sequence_record_device_attitude_matrices_set(o, val ? 1 : 0);
        
    }
	public static byte[] PipelineSequenceRecordData(IntPtr o) {
        
        IntPtr ret = zappar_pipeline_sequence_record_data(o);
        int N = PipelineSequenceRecordDataSize(o);
        byte[] retBytes = new byte[N];
        Marshal.Copy(ret, retBytes, 0, N);
        return retBytes;
    }
	public static int PipelineSequenceRecordDataSize(IntPtr o) {
        
        int ret = zappar_pipeline_sequence_record_data_size(o);
        return ret;
    }
	public static void PipelineSequenceRecordClear(IntPtr o) {
        
        zappar_pipeline_sequence_record_clear(o);
        
    }
	public static void CameraSourceStart(IntPtr o) {
        
        zappar_camera_source_start(o);
        
    }
	public static void CameraSourcePause(IntPtr o) {
        
        zappar_camera_source_pause(o);
        
    }
	public static void SequenceSourceStart(IntPtr o) {
        
        zappar_sequence_source_start(o);
        
    }
	public static void SequenceSourcePause(IntPtr o) {
        
        zappar_sequence_source_pause(o);
        
    }
	public static void SequenceSourceLoadFromMemory(IntPtr o, byte[] data) {
        
	
        zappar_sequence_source_load_from_memory(o, data, data.Length);
        
    }
	public static void SequenceSourceMaxPlaybackFpsSet(IntPtr o, float fps) {
        
	
        zappar_sequence_source_max_playback_fps_set(o, fps);
        
    }
	public static void ImageTrackerTargetLoadFromMemory(IntPtr o, byte[] data) {
        
	
        zappar_image_tracker_target_load_from_memory(o, data, data.Length);
        
    }
	public static int ImageTrackerTargetLoadedVersion(IntPtr o) {
        
        int ret = zappar_image_tracker_target_loaded_version(o);
        return ret;
    }
	public static int ImageTrackerTargetCount(IntPtr o) {
        
        int ret = zappar_image_tracker_target_count(o);
        return ret;
    }
	public static uint ImageTrackerTargetType(IntPtr o, int indx) {
        
	
        uint ret = zappar_image_tracker_target_type(o, indx);
        return ret;
    }
	public static float ImageTrackerTargetRadiusTop(IntPtr o, int indx) {
        
	
        float ret = zappar_image_tracker_target_radius_top(o, indx);
        return ret;
    }
	public static float ImageTrackerTargetRadiusBottom(IntPtr o, int indx) {
        
	
        float ret = zappar_image_tracker_target_radius_bottom(o, indx);
        return ret;
    }
	public static float ImageTrackerTargetSideLength(IntPtr o, int indx) {
        
	
        float ret = zappar_image_tracker_target_side_length(o, indx);
        return ret;
    }
	public static float ImageTrackerTargetPhysicalScaleFactor(IntPtr o, int indx) {
        
	
        float ret = zappar_image_tracker_target_physical_scale_factor(o, indx);
        return ret;
    }
	public static byte[] ImageTrackerTargetPreviewCompressed(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_compressed(o, indx);
        int N = ImageTrackerTargetPreviewCompressedSize(o, indx);
        byte[] retBytes = new byte[N];
        Marshal.Copy(ret, retBytes, 0, N);
        return retBytes;
    }
	public static int ImageTrackerTargetPreviewCompressedSize(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_compressed_size(o, indx);
        return ret;
    }
	public static string ImageTrackerTargetPreviewCompressedMimetype(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_compressed_mimetype(o, indx);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static byte[] ImageTrackerTargetPreviewRgba(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_rgba(o, indx);
        int N = ImageTrackerTargetPreviewRgbaSize(o, indx);
        byte[] retBytes = new byte[N];
        Marshal.Copy(ret, retBytes, 0, N);
        return retBytes;
    }
	public static int ImageTrackerTargetPreviewRgbaSize(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_rgba_size(o, indx);
        return ret;
    }
	public static int ImageTrackerTargetPreviewRgbaWidth(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_rgba_width(o, indx);
        return ret;
    }
	public static int ImageTrackerTargetPreviewRgbaHeight(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_rgba_height(o, indx);
        return ret;
    }
	public static int[] ImageTrackerTargetPreviewMeshIndices(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_mesh_indices(o, indx);
        int numIndices = ImageTrackerTargetPreviewMeshIndicesSize(o, indx);  
        int numBytes = numIndices*2;              
        byte[] src = new byte[numBytes];
        int[] dst = new int[numIndices];
        Marshal.Copy(ret, src, 0, numBytes);
        for (int n = 0; n < numBytes; n+=2) 
        {          
            dst[n/2] = (int)BitConverter.ToUInt16(src, n);   
        }
        return dst;
    }
	public static int ImageTrackerTargetPreviewMeshIndicesSize(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_mesh_indices_size(o, indx);
        return ret;
    }
	public static float[] ImageTrackerTargetPreviewMeshVertices(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_mesh_vertices(o, indx);
        int N = ImageTrackerTargetPreviewMeshVerticesSize(o, indx);
        float[] retFloats = new float[N];
        Marshal.Copy(ret, retFloats, 0, N);
        return retFloats;
    }
	public static int ImageTrackerTargetPreviewMeshVerticesSize(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_mesh_vertices_size(o, indx);
        return ret;
    }
	public static float[] ImageTrackerTargetPreviewMeshNormals(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_mesh_normals(o, indx);
        int N = ImageTrackerTargetPreviewMeshNormalsSize(o, indx);
        float[] retFloats = new float[N];
        Marshal.Copy(ret, retFloats, 0, N);
        return retFloats;
    }
	public static int ImageTrackerTargetPreviewMeshNormalsSize(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_mesh_normals_size(o, indx);
        return ret;
    }
	public static float[] ImageTrackerTargetPreviewMeshUvs(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_target_preview_mesh_uvs(o, indx);
        int N = ImageTrackerTargetPreviewMeshUvsSize(o, indx);
        float[] retFloats = new float[N];
        Marshal.Copy(ret, retFloats, 0, N);
        return retFloats;
    }
	public static int ImageTrackerTargetPreviewMeshUvsSize(IntPtr o, int indx) {
        
	
        int ret = zappar_image_tracker_target_preview_mesh_uvs_size(o, indx);
        return ret;
    }
	public static bool ImageTrackerEnabled(IntPtr o) {
        
        int ret = zappar_image_tracker_enabled(o);
        return (ret == 1) ? true : false;
    }
	public static void ImageTrackerEnabledSet(IntPtr o, bool enabled) {
        
	
        zappar_image_tracker_enabled_set(o, enabled ? 1 : 0);
        
    }
	public static int ImageTrackerAnchorCount(IntPtr o) {
        
        int ret = zappar_image_tracker_anchor_count(o);
        return ret;
    }
	public static string ImageTrackerAnchorId(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_anchor_id(o, indx);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static Matrix4x4 ImageTrackerAnchorPoseRaw(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_image_tracker_anchor_pose_raw(o, indx);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 ImageTrackerAnchorPoseCameraRelative(IntPtr o, int indx, bool mirror) {
        
	
	
        IntPtr ret = zappar_image_tracker_anchor_pose_camera_relative(o, indx, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 ImageTrackerAnchorPose(IntPtr o, int indx, Matrix4x4 camera_pose, bool mirror) {
        
	
	float[] arg_camera_pose = new float[16];
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                arg_camera_pose[i * 4 + k] = camera_pose[k, i];
	
        IntPtr ret = zappar_image_tracker_anchor_pose(o, indx, arg_camera_pose, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static void FaceTrackerModelLoadFromMemory(IntPtr o, byte[] data) {
        
	
        zappar_face_tracker_model_load_from_memory(o, data, data.Length);
        
    }
	public static void FaceTrackerModelLoadDefault(IntPtr o) {
        
        zappar_face_tracker_model_load_default(o);
        
    }
	public static int FaceTrackerModelLoadedVersion(IntPtr o) {
        
        int ret = zappar_face_tracker_model_loaded_version(o);
        return ret;
    }
	public static void FaceTrackerEnabledSet(IntPtr o, bool enabled) {
        
	
        zappar_face_tracker_enabled_set(o, enabled ? 1 : 0);
        
    }
	public static bool FaceTrackerEnabled(IntPtr o) {
        
        int ret = zappar_face_tracker_enabled(o);
        return (ret == 1) ? true : false;
    }
	public static void FaceTrackerMaxFacesSet(IntPtr o, int num) {
        
	
        zappar_face_tracker_max_faces_set(o, num);
        
    }
	public static int FaceTrackerMaxFaces(IntPtr o) {
        
        int ret = zappar_face_tracker_max_faces(o);
        return ret;
    }
	public static int FaceTrackerAnchorCount(IntPtr o) {
        
        int ret = zappar_face_tracker_anchor_count(o);
        return ret;
    }
	public static string FaceTrackerAnchorId(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_face_tracker_anchor_id(o, indx);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static Matrix4x4 FaceTrackerAnchorPoseRaw(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_face_tracker_anchor_pose_raw(o, indx);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 FaceTrackerAnchorPoseCameraRelative(IntPtr o, int indx, bool mirror) {
        
	
	
        IntPtr ret = zappar_face_tracker_anchor_pose_camera_relative(o, indx, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 FaceTrackerAnchorPose(IntPtr o, int indx, Matrix4x4 camera_pose, bool mirror) {
        
	
	float[] arg_camera_pose = new float[16];
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                arg_camera_pose[i * 4 + k] = camera_pose[k, i];
	
        IntPtr ret = zappar_face_tracker_anchor_pose(o, indx, arg_camera_pose, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static float[] FaceTrackerAnchorIdentityCoefficients(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_face_tracker_anchor_identity_coefficients(o, indx);
        float[] coefficients = new float[50];
            Marshal.Copy(ret, coefficients, 0, 50);
            return coefficients;
    }
	public static float[] FaceTrackerAnchorExpressionCoefficients(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_face_tracker_anchor_expression_coefficients(o, indx);
        float[] coefficients = new float[29];
            Marshal.Copy(ret, coefficients, 0, 29);
            return coefficients;
    }
	public static void FaceMeshLoadFromMemory(IntPtr o, byte[] data, bool fill_mouth, bool fill_eye_l, bool fill_eye_r, bool fill_neck) {
        
	
	
	
	
	
        zappar_face_mesh_load_from_memory(o, data, data.Length, fill_mouth ? 1 : 0, fill_eye_l ? 1 : 0, fill_eye_r ? 1 : 0, fill_neck ? 1 : 0);
        
    }
	public static void FaceMeshLoadDefault(IntPtr o) {
        
        zappar_face_mesh_load_default(o);
        
    }
	public static void FaceMeshLoadDefaultFullHeadSimplified(IntPtr o, bool fill_mouth, bool fill_eye_l, bool fill_eye_r, bool fill_neck) {
        
	
	
	
	
        zappar_face_mesh_load_default_full_head_simplified(o, fill_mouth ? 1 : 0, fill_eye_l ? 1 : 0, fill_eye_r ? 1 : 0, fill_neck ? 1 : 0);
        
    }
	public static void FaceMeshLoadDefaultFace(IntPtr o, bool fill_eye_l, bool fill_eye_r, bool fill_mouth) {
        
	
	
	
        zappar_face_mesh_load_default_face(o, fill_eye_l ? 1 : 0, fill_eye_r ? 1 : 0, fill_mouth ? 1 : 0);
        
    }
	public static int FaceMeshLoadedVersion(IntPtr o) {
        
        int ret = zappar_face_mesh_loaded_version(o);
        return ret;
    }
	public static void FaceMeshUpdate(IntPtr o, float[] identity, float[] expression, bool mirrored) {
        
	
	
	
        zappar_face_mesh_update(o, identity, expression, mirrored ? 1 : 0);
        
    }
	public static int FaceMeshIndicesSize(IntPtr o) {
        
        int ret = zappar_face_mesh_indices_size(o);
        return ret;
    }
	public static int[] FaceMeshIndices(IntPtr o) {
        
        IntPtr ret = zappar_face_mesh_indices(o);
        int numIndices = FaceMeshIndicesSize(o);  
        int numBytes = numIndices*2;              
        byte[] src = new byte[numBytes];
        int[] dst = new int[numIndices];
        Marshal.Copy(ret, src, 0, numBytes);
        for (int n = 0; n < numBytes; n+=2) 
        {          
            dst[n/2] = (int)BitConverter.ToUInt16(src, n);   
        }
        return dst;
    }
	public static int FaceMeshVerticesSize(IntPtr o) {
        
        int ret = zappar_face_mesh_vertices_size(o);
        return ret;
    }
	public static float[] FaceMeshVertices(IntPtr o) {
        
        IntPtr ret = zappar_face_mesh_vertices(o);
        int N = FaceMeshVerticesSize(o);
        float[] retFloats = new float[N];
        Marshal.Copy(ret, retFloats, 0, N);
        return retFloats;
    }
	public static int FaceMeshNormalsSize(IntPtr o) {
        
        int ret = zappar_face_mesh_normals_size(o);
        return ret;
    }
	public static float[] FaceMeshNormals(IntPtr o) {
        
        IntPtr ret = zappar_face_mesh_normals(o);
        int N = FaceMeshNormalsSize(o);
        float[] retFloats = new float[N];
        Marshal.Copy(ret, retFloats, 0, N);
        return retFloats;
    }
	public static int FaceMeshUvsSize(IntPtr o) {
        
        int ret = zappar_face_mesh_uvs_size(o);
        return ret;
    }
	public static float[] FaceMeshUvs(IntPtr o) {
        
        IntPtr ret = zappar_face_mesh_uvs(o);
        int N = FaceMeshUvsSize(o);
        float[] retFloats = new float[N];
        Marshal.Copy(ret, retFloats, 0, N);
        return retFloats;
    }
	public static void FaceLandmarkUpdate(IntPtr o, float[] identity, float[] expression, bool mirrored) {
        
	
	
	
        zappar_face_landmark_update(o, identity, expression, mirrored ? 1 : 0);
        
    }
	public static Matrix4x4 FaceLandmarkAnchorPose(IntPtr o) {
        
        IntPtr ret = zappar_face_landmark_anchor_pose(o);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static void BarcodeFinderEnabledSet(IntPtr o, bool enabled) {
        
	
        zappar_barcode_finder_enabled_set(o, enabled ? 1 : 0);
        
    }
	public static bool BarcodeFinderEnabled(IntPtr o) {
        
        int ret = zappar_barcode_finder_enabled(o);
        return (ret == 1) ? true : false;
    }
	public static int BarcodeFinderFoundNumber(IntPtr o) {
        
        int ret = zappar_barcode_finder_found_number(o);
        return ret;
    }
	public static string BarcodeFinderFoundText(IntPtr o, int indx) {
        
	
        IntPtr ret = zappar_barcode_finder_found_text(o, indx);
        return Marshal.PtrToStringAnsi(ret);
    }
	public static BarcodeFormat BarcodeFinderFoundFormat(IntPtr o, int indx) {
        
	
        uint ret = zappar_barcode_finder_found_format(o, indx);
        return (BarcodeFormat)ret;
    }
	public static BarcodeFormat BarcodeFinderFormats(IntPtr o) {
        
        uint ret = zappar_barcode_finder_formats(o);
        return (BarcodeFormat)ret;
    }
	public static void BarcodeFinderFormatsSet(IntPtr o, BarcodeFormat f) {
        
	
        zappar_barcode_finder_formats_set(o, (uint)f);
        
    }
	public static void InstantWorldTrackerEnabledSet(IntPtr o, bool enabled) {
        
	
        zappar_instant_world_tracker_enabled_set(o, enabled ? 1 : 0);
        
    }
	public static bool InstantWorldTrackerEnabled(IntPtr o) {
        
        int ret = zappar_instant_world_tracker_enabled(o);
        return (ret == 1) ? true : false;
    }
	public static Matrix4x4 InstantWorldTrackerAnchorPoseRaw(IntPtr o) {
        
        IntPtr ret = zappar_instant_world_tracker_anchor_pose_raw(o);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 InstantWorldTrackerAnchorPoseCameraRelative(IntPtr o, bool mirror) {
        
	
        IntPtr ret = zappar_instant_world_tracker_anchor_pose_camera_relative(o, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static Matrix4x4 InstantWorldTrackerAnchorPose(IntPtr o, Matrix4x4 camera_pose, bool mirror) {
        
	float[] arg_camera_pose = new float[16];
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                arg_camera_pose[i * 4 + k] = camera_pose[k, i];
	
        IntPtr ret = zappar_instant_world_tracker_anchor_pose(o, arg_camera_pose, mirror ? 1 : 0);
        float[] retFloats = new float[16];
        Marshal.Copy(ret, retFloats, 0, 16);
        Matrix4x4 retMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            for (int k = 0; k < 4; k++)
                retMatrix[k, i] = retFloats[i * 4 + k];
        return retMatrix;
    }
	public static void InstantWorldTrackerAnchorPoseSetFromCameraOffsetRaw(IntPtr o, float x, float y, float z, InstantTrackerTransformOrientation orientation) {
        
	
	
	
	
        zappar_instant_world_tracker_anchor_pose_set_from_camera_offset_raw(o, x, y, z, (uint)orientation);
        
    }
	public static void InstantWorldTrackerAnchorPoseSetFromCameraOffset(IntPtr o, float x, float y, float z, InstantTrackerTransformOrientation orientation) {
        
	
	
	
	
        zappar_instant_world_tracker_anchor_pose_set_from_camera_offset(o, x, y, z, (uint)orientation);
        
    }
    public static IntPtr PipelineCreate() 
    {
        return zappar_pipeline_create();
    }
	public static IntPtr CameraSourceCreate(IntPtr pipeline,string device_id) 
    {
        return zappar_camera_source_create(pipeline,device_id);
    }
	public static IntPtr SequenceSourceCreate(IntPtr pipeline) 
    {
        return zappar_sequence_source_create(pipeline);
    }
	public static IntPtr ImageTrackerCreate(IntPtr pipeline) 
    {
        return zappar_image_tracker_create(pipeline);
    }
	public static IntPtr FaceTrackerCreate(IntPtr pipeline) 
    {
        return zappar_face_tracker_create(pipeline);
    }
	public static IntPtr FaceMeshCreate() 
    {
        return zappar_face_mesh_create();
    }
	public static IntPtr FaceLandmarkCreate(uint landmark) 
    {
        return zappar_face_landmark_create(landmark);
    }
	public static IntPtr BarcodeFinderCreate(IntPtr pipeline) 
    {
        return zappar_barcode_finder_create(pipeline);
    }
	public static IntPtr InstantWorldTrackerCreate(IntPtr pipeline) 
    {
        return zappar_instant_world_tracker_create(pipeline);
    }
    public static void PipelineDestroy(IntPtr o)
    {
        zappar_pipeline_destroy(o);
    }
	public static void CameraSourceDestroy(IntPtr o)
    {
        zappar_camera_source_destroy(o);
    }
	public static void SequenceSourceDestroy(IntPtr o)
    {
        zappar_sequence_source_destroy(o);
    }
	public static void ImageTrackerDestroy(IntPtr o)
    {
        zappar_image_tracker_destroy(o);
    }
	public static void FaceTrackerDestroy(IntPtr o)
    {
        zappar_face_tracker_destroy(o);
    }
	public static void FaceMeshDestroy(IntPtr o)
    {
        zappar_face_mesh_destroy(o);
    }
	public static void FaceLandmarkDestroy(IntPtr o)
    {
        zappar_face_landmark_destroy(o);
    }
	public static void BarcodeFinderDestroy(IntPtr o)
    {
        zappar_barcode_finder_destroy(o);
    }
	public static void InstantWorldTrackerDestroy(IntPtr o)
    {
        zappar_instant_world_tracker_destroy(o);
    }

    // END AUTOGEN

    // Extra methods
    public static void PipelineCameraFrameTextureMatrix(IntPtr o, ref float[] mat4x4, int renderWidth, int renderHeight, bool mirror) {
        IntPtr ret = zappar_pipeline_camera_frame_texture_matrix(o, renderWidth, renderHeight, mirror ? 1 : 0);
        Marshal.Copy(ret, mat4x4, 0, 16);
    }
    public static void FaceTrackerAnchorUpdateIdentityCoefficients(IntPtr o, int indx, ref float[] coefficients) {
        IntPtr ret = zappar_face_tracker_anchor_identity_coefficients(o, indx);
        Marshal.Copy(ret, coefficients, 0, 50);
    }
    public static void FaceTrackerAnchorUpdateExpressionCoefficients(IntPtr o, int indx, ref float[] coefficients)
    {
        IntPtr ret = zappar_face_tracker_anchor_expression_coefficients(o, indx);
        Marshal.Copy(ret, coefficients, 0, 29);
    }
    public static void UpdateFaceMeshVertices(IntPtr o, ref float[] verts)
    {
        IntPtr ret = zappar_face_mesh_vertices(o);
        Marshal.Copy(ret, verts, 0, verts.Length);
    }
    public static void UpdateFaceMeshNormals(IntPtr o, ref float[] normals)
    {
        IntPtr ret = zappar_face_mesh_normals(o);
        Marshal.Copy(ret, normals, 0, normals.Length);
    }
}
}

// "Unreachable code."
#pragma warning restore 0162
