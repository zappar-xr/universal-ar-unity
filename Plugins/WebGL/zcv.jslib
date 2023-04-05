mergeInto(LibraryManager.library, {

$zappar_support: {},
$zappar_support__postset: `
    (function() {
    var hasInitialized = false;
    var zappar;
    var textureMap = new Map();
    var pipeline_;

    _zappar_has_initialized = function() {
        return hasInitialized ? 1 : 0;
    }

    _zappar_initialize = function() {
        if (typeof ZCV === 'undefined') {
            var scr = document.createElement("script");
            scr.src="https://libs.zappar.com/zappar-cv/2.0.1/zappar-cv.js";
            scr.addEventListener('load', function() {
                zappar = ZCV.initialize();
                hasInitialized = true;
            });
            document.body.appendChild(scr);
        } else {
            if(typeof window.zappar === 'undefined' && typeof zappar === 'undefined'){
                zappar = ZCV.initialize();
            }else if(typeof zappar === 'undefined') {
                zappar = window.zappar;
            }
            hasInitialized = true;
        }

        var zappar_native_callbacks = {
            process_gl: _zappar_pipeline_process_gl,
            upload_gl: _zappar_pipeline_camera_frame_upload_gl,
            face_mesh_vertices: _zappar_face_mesh_vertices,
            face_mesh_vertices_size: _zappar_face_mesh_vertices_size,
            face_mesh_normals: _zappar_face_mesh_normals,
            face_mesh_normals_size: _zappar_face_mesh_normals_size,
            face_mesh_uvs: _zappar_face_mesh_uvs,
            face_mesh_uvs_size: _zappar_face_mesh_uvs_size
        };

        window.zappar_native_callbacks = zappar_native_callbacks;
    }

    _zappar_pipeline_gl_context_set = function( pipeline ) {
        zappar.pipeline_gl_context_set(pipeline, GLctx);
    };

    _zappar_pipeline_set = function(o) {
        pipeline_ = o;
    };

    _zappar_pipeline_process_gl = function(o) {
        var ret = zappar.pipeline_process_gl( pipeline_ );
        return ret;
    };

    _zappar_pipeline_camera_frame_upload_gl = function(o) {  
        var ret = zappar.pipeline_camera_frame_upload_gl(pipeline_);
        return ret;
    };
    
    _zappar_loaded = function() {
        
        var ret = zappar.loaded();
        return ret;
    };
	_zappar_camera_default_device_id = function(userFacing) {
        var userFacing_val = userFacing;
        var ret = zappar.camera_default_device_id(userFacing_val);
        var bufferSize = lengthBytesUTF8(ret) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ret, buffer, bufferSize);
        return buffer;
    };
	_zappar_camera_count = function() {
        
        var ret = zappar.camera_count();
        return ret;
    };
	_zappar_camera_id = function(indx) {
        var indx_val = indx;
        var ret = zappar.camera_id(indx_val);
        var bufferSize = lengthBytesUTF8(ret) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ret, buffer, bufferSize);
        return buffer;
    };
	_zappar_camera_name = function(indx) {
        var indx_val = indx;
        var ret = zappar.camera_name(indx_val);
        var bufferSize = lengthBytesUTF8(ret) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ret, buffer, bufferSize);
        return buffer;
    };
	_zappar_camera_user_facing = function(indx) {
        var indx_val = indx;
        var ret = zappar.camera_user_facing(indx_val);
        return ret;
    };
	_zappar_projection_matrix_from_camera_model = function(model, renderWidth, renderHeight) {
        var model_val = new Float32Array(6);
        model_val.set(HEAPF32.subarray(model/4, 6 + model / 4));
		var renderWidth_val = renderWidth;
		var renderHeight_val = renderHeight;
        var ret = zappar.projection_matrix_from_camera_model(model_val, renderWidth_val, renderHeight_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_projection_matrix_from_camera_model_ext = function(model, renderWidth, renderHeight, zNear, zFar) {
        var model_val = new Float32Array(6);
        model_val.set(HEAPF32.subarray(model/4, 6 + model / 4));
		var renderWidth_val = renderWidth;
		var renderHeight_val = renderHeight;
		var zNear_val = zNear;
		var zFar_val = zFar;
        var ret = zappar.projection_matrix_from_camera_model_ext(model_val, renderWidth_val, renderHeight_val, zNear_val, zFar_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	
	
	
	_zappar_log_level_set = function(level) {
        var level_val = level;
        var ret = zappar.log_level_set(level_val);
        return ret;
    };
	
	
	
	_zappar_permission_request_ui = function() {
        
        var ret = zappar.permission_request_ui();
        return ret;
    };
	_zappar_permission_denied_ui = function() {
        
        var ret = zappar.permission_denied_ui();
        return ret;
    };
	_zappar_permission_request_all = function() {
        
        var ret = zappar.permission_request_all();
        return ret;
    };
	_zappar_permission_request_camera = function() {
        
        var ret = zappar.permission_request_camera();
        return ret;
    };
	_zappar_permission_request_motion = function() {
        
        var ret = zappar.permission_request_motion();
        return ret;
    };
	_zappar_permission_granted_all = function() {
        
        var ret = zappar.permission_granted_all();
        return ret;
    };
	_zappar_permission_granted_camera = function() {
        
        var ret = zappar.permission_granted_camera();
        return ret;
    };
	_zappar_permission_granted_motion = function() {
        
        var ret = zappar.permission_granted_motion();
        return ret;
    };
	_zappar_permission_denied_any = function() {
        
        var ret = zappar.permission_denied_any();
        return ret;
    };
	_zappar_permission_denied_camera = function() {
        
        var ret = zappar.permission_denied_camera();
        return ret;
    };
	_zappar_permission_denied_motion = function() {
        
        var ret = zappar.permission_denied_motion();
        return ret;
    };
	_zappar_analytics_project_id_set = function(id) {
        var id_val = UTF8ToString(id);
        var ret = zappar.analytics_project_id_set(id_val);
        return ret;
    };
    
    _zappar_pipeline_create = function() {
        
        var ret = zappar.pipeline_create();
        return ret;
    };
    _zappar_pipeline_destroy = function(o) {
        var o_val = o;
        var ret = zappar.pipeline_destroy(o_val);
        return ret;
    };
    
	_zappar_pipeline_camera_frame_texture_gl = function(o) {
        
        var ret = zappar.pipeline_camera_frame_texture_gl(o);
        if (!ret) return 0;
        var id = textureMap.get(ret);
        if (id === undefined) {
            id = GL.getNewId(GL.textures);
            GL.textures[id] = ret;
            textureMap.set (ret, id);
        }
        return id !== undefined ? id : 0;
        
    };
	
	
	
	
	_zappar_pipeline_camera_frame_texture_matrix = function(o, renderWidth, renderHeight, mirror) {
        var renderWidth_val = renderWidth;
		var renderHeight_val = renderHeight;
		var mirror_val = mirror;
        var ret = zappar.pipeline_camera_frame_texture_matrix(o, renderWidth_val, renderHeight_val, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	
	_zappar_pipeline_frame_update = function(o) {
        
        var ret = zappar.pipeline_frame_update(o);
        return ret;
    };
	_zappar_pipeline_frame_number = function(o) {
        
        var ret = zappar.pipeline_frame_number(o);
        return ret;
    };
	_zappar_pipeline_camera_model = function(o) {
        
        var ret = zappar.pipeline_camera_model(o);
        var buffer = _malloc(6 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_pipeline_camera_pose_default = function(o) {
        
        var ret = zappar.pipeline_camera_pose_default(o);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_pipeline_camera_pose_with_attitude = function(o, mirror) {
        var mirror_val = mirror;
        var ret = zappar.pipeline_camera_pose_with_attitude(o, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_pipeline_camera_pose_with_origin = function(o, pose) {
        var pose_val = new Float32Array(16);
        pose_val.set(HEAPF32.subarray(pose/4, 16 + pose / 4));
        var ret = zappar.pipeline_camera_pose_with_origin(o, pose_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_pipeline_camera_frame_user_data = function(o) {
        
        var ret = zappar.pipeline_camera_frame_user_data(o);
        return ret;
    };
	_zappar_pipeline_camera_frame_submit = function(o, data, data_size, width, height, user_data, camera_to_device_transform, camera_model, user_facing) {
        var data_val = new Uint8Array(data_size);
        data_val.set(HEAPU8.subarray(data, data + data_size));
		var width_val = width;
		var height_val = height;
		var user_data_val = user_data;
		var camera_to_device_transform_val = new Float32Array(16);
        camera_to_device_transform_val.set(HEAPF32.subarray(camera_to_device_transform/4, 16 + camera_to_device_transform / 4));
		var camera_model_val = new Float32Array(6);
        camera_model_val.set(HEAPF32.subarray(camera_model/4, 6 + camera_model / 4));
		var user_facing_val = user_facing;
        var ret = zappar.pipeline_camera_frame_submit(o, data_val, width_val, height_val, user_data_val, camera_to_device_transform_val, camera_model_val, user_facing_val);
        return ret;
    };
	
	_zappar_pipeline_camera_frame_camera_attitude = function(o) {
        
        var ret = zappar.pipeline_camera_frame_camera_attitude(o);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_pipeline_camera_frame_device_attitude = function(o) {
        
        var ret = zappar.pipeline_camera_frame_device_attitude(o);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_pipeline_camera_frame_user_facing = function(o) {
        
        var ret = zappar.pipeline_camera_frame_user_facing(o);
        return ret;
    };
	_zappar_pipeline_motion_accelerometer_submit = function(o, time, x, y, z) {
        var time_val = time;
		var x_val = x;
		var y_val = y;
		var z_val = z;
        var ret = zappar.pipeline_motion_accelerometer_submit(o, time_val, x_val, y_val, z_val);
        return ret;
    };
	_zappar_pipeline_motion_rotation_rate_submit = function(o, time, x, y, z) {
        var time_val = time;
		var x_val = x;
		var y_val = y;
		var z_val = z;
        var ret = zappar.pipeline_motion_rotation_rate_submit(o, time_val, x_val, y_val, z_val);
        return ret;
    };
	_zappar_pipeline_motion_attitude_submit = function(o, time, x, y, z) {
        var time_val = time;
		var x_val = x;
		var y_val = y;
		var z_val = z;
        var ret = zappar.pipeline_motion_attitude_submit(o, time_val, x_val, y_val, z_val);
        return ret;
    };
	_zappar_pipeline_motion_attitude_matrix_submit = function(o, mat) {
        var mat_val = new Float32Array(16);
        mat_val.set(HEAPF32.subarray(mat/4, 16 + mat / 4));
        var ret = zappar.pipeline_motion_attitude_matrix_submit(o, mat_val);
        return ret;
    };
	_zappar_pipeline_sequence_record_start = function(o, expected_frames) {
        var expected_frames_val = expected_frames;
        var ret = zappar.pipeline_sequence_record_start(o, expected_frames_val);
        return ret;
    };
	_zappar_pipeline_sequence_record_stop = function(o) {
        
        var ret = zappar.pipeline_sequence_record_stop(o);
        return ret;
    };
	_zappar_pipeline_sequence_record_device_attitude_matrices_set = function(o, val) {
        var val_val = val;
        var ret = zappar.pipeline_sequence_record_device_attitude_matrices_set(o, val_val);
        return ret;
    };
	_zappar_pipeline_sequence_record_data = function(o) {
        
        var ret = zappar.pipeline_sequence_record_data(o);
        return ret;
    };
	_zappar_pipeline_sequence_record_data_size = function(o) {
        
        var ret = zappar.pipeline_sequence_record_data_size(o);
        return ret;
    };
	_zappar_pipeline_sequence_record_clear = function(o) {
        
        var ret = zappar.pipeline_sequence_record_clear(o);
        return ret;
    };
    
	
    _zappar_camera_source_create = function(pipeline, device_id) {
        var pipeline_val = pipeline;
		var device_id_val = UTF8ToString(device_id);
        var ret = zappar.camera_source_create(pipeline_val, device_id_val);
        return ret;
    };
    _zappar_camera_source_destroy = function(o) {
        var o_val = o;
        var ret = zappar.camera_source_destroy(o_val);
        return ret;
    };
    _zappar_camera_source_start = function(o) {
        
        var ret = zappar.camera_source_start(o);
        return ret;
    };
	_zappar_camera_source_pause = function(o) {
        
        var ret = zappar.camera_source_pause(o);
        return ret;
    };
    
	
    _zappar_sequence_source_create = function(pipeline) {
        var pipeline_val = pipeline;
        var ret = zappar.sequence_source_create(pipeline_val);
        return ret;
    };
    _zappar_sequence_source_destroy = function(o) {
        var o_val = o;
        var ret = zappar.sequence_source_destroy(o_val);
        return ret;
    };
    _zappar_sequence_source_start = function(o) {
        
        var ret = zappar.sequence_source_start(o);
        return ret;
    };
	_zappar_sequence_source_pause = function(o) {
        
        var ret = zappar.sequence_source_pause(o);
        return ret;
    };
	_zappar_sequence_source_load_from_memory = function(o, data, data_size) {
        var data_val = new Uint8Array(data_size);
        data_val.set(HEAPU8.subarray(data, data + data_size));
        var ret = zappar.sequence_source_load_from_memory(o, data_val);
        return ret;
    };
	_zappar_sequence_source_max_playback_fps_set = function(o, fps) {
        var fps_val = fps;
        var ret = zappar.sequence_source_max_playback_fps_set(o, fps_val);
        return ret;
    };
    
	
    _zappar_image_tracker_create = function(pipeline) {
        var pipeline_val = pipeline;
        var ret = zappar.image_tracker_create(pipeline_val);
        return ret;
    };
    _zappar_image_tracker_destroy = function(o) {
        var o_val = o;
        var ret = zappar.image_tracker_destroy(o_val);
        return ret;
    };
    _zappar_image_tracker_target_load_from_memory = function(o, data, data_size) {
        var data_val = new Uint8Array(data_size);
        data_val.set(HEAPU8.subarray(data, data + data_size));
        var ret = zappar.image_tracker_target_load_from_memory(o, data_val);
        return ret;
    };
	_zappar_image_tracker_target_loaded_version = function(o) {
        
        var ret = zappar.image_tracker_target_loaded_version(o);
        return ret;
    };
	_zappar_image_tracker_target_count = function(o) {
        
        var ret = zappar.image_tracker_target_count(o);
        return ret;
    };
	_zappar_image_tracker_target_type = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_type(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_radius_top = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_radius_top(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_radius_bottom = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_radius_bottom(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_side_length = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_side_length(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_physical_scale_factor = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_physical_scale_factor(o, indx_val);
        return ret;
    };
	
	_zappar_image_tracker_target_preview_compressed_size = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_compressed_size(o, indx_val);
        return ret;
    };
	
	
	
	
	
	_zappar_image_tracker_target_preview_mesh_indices = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_indices(o, indx_val);
        var n = zappar.image_tracker_target_preview_mesh_indices_size(o, indx);
        var buffer = _malloc(n * 2);
        HEAPU16.set(ret, buffer / 2);
        return buffer;
    };
	_zappar_image_tracker_target_preview_mesh_indices_size = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_indices_size(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_preview_mesh_vertices = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_vertices(o, indx_val);
        var n = zappar.image_tracker_target_preview_mesh_vertices_size(o, indx);
        var buffer = _malloc(n * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_image_tracker_target_preview_mesh_vertices_size = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_vertices_size(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_preview_mesh_normals = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_normals(o, indx_val);
        var n = zappar.image_tracker_target_preview_mesh_normals_size(o, indx);
        var buffer = _malloc(n * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_image_tracker_target_preview_mesh_normals_size = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_normals_size(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_target_preview_mesh_uvs = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_uvs(o, indx_val);
        var n = zappar.image_tracker_target_preview_mesh_uvs_size(o, indx);
        var buffer = _malloc(n * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_image_tracker_target_preview_mesh_uvs_size = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_target_preview_mesh_uvs_size(o, indx_val);
        return ret;
    };
	_zappar_image_tracker_enabled = function(o) {
        
        var ret = zappar.image_tracker_enabled(o);
        return ret;
    };
	_zappar_image_tracker_enabled_set = function(o, enabled) {
        var enabled_val = enabled;
        var ret = zappar.image_tracker_enabled_set(o, enabled_val);
        return ret;
    };
	_zappar_image_tracker_anchor_count = function(o) {
        
        var ret = zappar.image_tracker_anchor_count(o);
        return ret;
    };
	_zappar_image_tracker_anchor_id = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_anchor_id(o, indx_val);
        var bufferSize = lengthBytesUTF8(ret) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ret, buffer, bufferSize);
        return buffer;
    };
	_zappar_image_tracker_anchor_pose_raw = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.image_tracker_anchor_pose_raw(o, indx_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_image_tracker_anchor_pose_camera_relative = function(o, indx, mirror) {
        var indx_val = indx;
		var mirror_val = mirror;
        var ret = zappar.image_tracker_anchor_pose_camera_relative(o, indx_val, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_image_tracker_anchor_pose = function(o, indx, camera_pose, mirror) {
        var indx_val = indx;
		var camera_pose_val = new Float32Array(16);
        camera_pose_val.set(HEAPF32.subarray(camera_pose/4, 16 + camera_pose / 4));
		var mirror_val = mirror;
        var ret = zappar.image_tracker_anchor_pose(o, indx_val, camera_pose_val, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
    
	
    _zappar_face_tracker_create = function(pipeline) {
        var pipeline_val = pipeline;
        var ret = zappar.face_tracker_create(pipeline_val);
        return ret;
    };
    _zappar_face_tracker_destroy = function(o) {
        var o_val = o;
        var ret = zappar.face_tracker_destroy(o_val);
        return ret;
    };
    _zappar_face_tracker_model_load_from_memory = function(o, data, data_size) {
        var data_val = new Uint8Array(data_size);
        data_val.set(HEAPU8.subarray(data, data + data_size));
        var ret = zappar.face_tracker_model_load_from_memory(o, data_val);
        return ret;
    };
	_zappar_face_tracker_model_load_default = function(o) {
        
        var ret = zappar.face_tracker_model_load_default(o);
        return ret;
    };
	_zappar_face_tracker_model_loaded_version = function(o) {
        
        var ret = zappar.face_tracker_model_loaded_version(o);
        return ret;
    };
	_zappar_face_tracker_enabled_set = function(o, enabled) {
        var enabled_val = enabled;
        var ret = zappar.face_tracker_enabled_set(o, enabled_val);
        return ret;
    };
	_zappar_face_tracker_enabled = function(o) {
        
        var ret = zappar.face_tracker_enabled(o);
        return ret;
    };
	_zappar_face_tracker_max_faces_set = function(o, num) {
        var num_val = num;
        var ret = zappar.face_tracker_max_faces_set(o, num_val);
        return ret;
    };
	_zappar_face_tracker_max_faces = function(o) {
        
        var ret = zappar.face_tracker_max_faces(o);
        return ret;
    };
	_zappar_face_tracker_anchor_count = function(o) {
        
        var ret = zappar.face_tracker_anchor_count(o);
        return ret;
    };
	_zappar_face_tracker_anchor_id = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.face_tracker_anchor_id(o, indx_val);
        var bufferSize = lengthBytesUTF8(ret) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ret, buffer, bufferSize);
        return buffer;
    };
	_zappar_face_tracker_anchor_pose_raw = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.face_tracker_anchor_pose_raw(o, indx_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_face_tracker_anchor_pose_camera_relative = function(o, indx, mirror) {
        var indx_val = indx;
		var mirror_val = mirror;
        var ret = zappar.face_tracker_anchor_pose_camera_relative(o, indx_val, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_face_tracker_anchor_pose = function(o, indx, camera_pose, mirror) {
        var indx_val = indx;
		var camera_pose_val = new Float32Array(16);
        camera_pose_val.set(HEAPF32.subarray(camera_pose/4, 16 + camera_pose / 4));
		var mirror_val = mirror;
        var ret = zappar.face_tracker_anchor_pose(o, indx_val, camera_pose_val, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_face_tracker_anchor_identity_coefficients = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.face_tracker_anchor_identity_coefficients(o, indx_val);
        var buffer = _malloc(50 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_face_tracker_anchor_expression_coefficients = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.face_tracker_anchor_expression_coefficients(o, indx_val);
        var buffer = _malloc(29 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
    
	
    _zappar_face_mesh_create = function() {
        
        var ret = zappar.face_mesh_create();
        return ret;
    };
    _zappar_face_mesh_destroy = function(o) {
        var o_val = o;
        var ret = zappar.face_mesh_destroy(o_val);
        return ret;
    };
    _zappar_face_mesh_load_from_memory = function(o, data, data_size, fill_mouth, fill_eye_l, fill_eye_r, fill_neck) {
        var data_val = new Uint8Array(data_size);
        data_val.set(HEAPU8.subarray(data, data + data_size));
		var fill_mouth_val = fill_mouth;
		var fill_eye_l_val = fill_eye_l;
		var fill_eye_r_val = fill_eye_r;
		var fill_neck_val = fill_neck;
        var ret = zappar.face_mesh_load_from_memory(o, data_val, fill_mouth_val, fill_eye_l_val, fill_eye_r_val, fill_neck_val);
        return ret;
    };
	_zappar_face_mesh_load_default = function(o) {
        
        var ret = zappar.face_mesh_load_default(o);
        return ret;
    };
	_zappar_face_mesh_load_default_full_head_simplified = function(o, fill_mouth, fill_eye_l, fill_eye_r, fill_neck) {
        var fill_mouth_val = fill_mouth;
		var fill_eye_l_val = fill_eye_l;
		var fill_eye_r_val = fill_eye_r;
		var fill_neck_val = fill_neck;
        var ret = zappar.face_mesh_load_default_full_head_simplified(o, fill_mouth_val, fill_eye_l_val, fill_eye_r_val, fill_neck_val);
        return ret;
    };
	_zappar_face_mesh_load_default_face = function(o, fill_eye_l, fill_eye_r, fill_mouth) {
        var fill_eye_l_val = fill_eye_l;
		var fill_eye_r_val = fill_eye_r;
		var fill_mouth_val = fill_mouth;
        var ret = zappar.face_mesh_load_default_face(o, fill_eye_l_val, fill_eye_r_val, fill_mouth_val);
        return ret;
    };
	_zappar_face_mesh_loaded_version = function(o) {
        
        var ret = zappar.face_mesh_loaded_version(o);
        return ret;
    };
	_zappar_face_mesh_update = function(o, identity, expression, mirrored) {
        var identity_val = new Float32Array(50);
        identity_val.set(HEAPF32.subarray(identity/4, 50 + identity / 4));
		var expression_val = new Float32Array(29);
        expression_val.set(HEAPF32.subarray(expression/4, 29 + expression / 4));
		var mirrored_val = mirrored;
        var ret = zappar.face_mesh_update(o, identity_val, expression_val, mirrored_val);
        return ret;
    };
	_zappar_face_mesh_indices_size = function(o) {
        
        var ret = zappar.face_mesh_indices_size(o);
        return ret;
    };
	_zappar_face_mesh_indices = function(o) {
        
        var ret = zappar.face_mesh_indices(o);
        var n = zappar.face_mesh_indices_size(o);
        var buffer = _malloc(n * 2);
        HEAPU16.set(ret, buffer / 2);
        return buffer;
    };
	_zappar_face_mesh_vertices_size = function(o) {
        
        var ret = zappar.face_mesh_vertices_size(o);
        return ret;
    };
	_zappar_face_mesh_vertices = function(o) {
        
        var ret = zappar.face_mesh_vertices(o);
        var n = zappar.face_mesh_vertices_size(o);
        var buffer = _malloc(n * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_face_mesh_normals_size = function(o) {
        
        var ret = zappar.face_mesh_normals_size(o);
        return ret;
    };
	_zappar_face_mesh_normals = function(o) {
        
        var ret = zappar.face_mesh_normals(o);
        var n = zappar.face_mesh_normals_size(o);
        var buffer = _malloc(n * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_face_mesh_uvs_size = function(o) {
        
        var ret = zappar.face_mesh_uvs_size(o);
        return ret;
    };
	_zappar_face_mesh_uvs = function(o) {
        
        var ret = zappar.face_mesh_uvs(o);
        var n = zappar.face_mesh_uvs_size(o);
        var buffer = _malloc(n * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
    
	
    _zappar_face_landmark_create = function(landmark) {
        var landmark_val = landmark;
        var ret = zappar.face_landmark_create(landmark_val);
        return ret;
    };
    _zappar_face_landmark_destroy = function(o) {
        var o_val = o;
        var ret = zappar.face_landmark_destroy(o_val);
        return ret;
    };
    _zappar_face_landmark_update = function(o, identity, expression, mirrored) {
        var identity_val = new Float32Array(50);
        identity_val.set(HEAPF32.subarray(identity/4, 50 + identity / 4));
		var expression_val = new Float32Array(29);
        expression_val.set(HEAPF32.subarray(expression/4, 29 + expression / 4));
		var mirrored_val = mirrored;
        var ret = zappar.face_landmark_update(o, identity_val, expression_val, mirrored_val);
        return ret;
    };
	_zappar_face_landmark_anchor_pose = function(o) {
        
        var ret = zappar.face_landmark_anchor_pose(o);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
    
	
    _zappar_barcode_finder_create = function(pipeline) {
        var pipeline_val = pipeline;
        var ret = zappar.barcode_finder_create(pipeline_val);
        return ret;
    };
    _zappar_barcode_finder_destroy = function(o) {
        var o_val = o;
        var ret = zappar.barcode_finder_destroy(o_val);
        return ret;
    };
    _zappar_barcode_finder_enabled_set = function(o, enabled) {
        var enabled_val = enabled;
        var ret = zappar.barcode_finder_enabled_set(o, enabled_val);
        return ret;
    };
	_zappar_barcode_finder_enabled = function(o) {
        
        var ret = zappar.barcode_finder_enabled(o);
        return ret;
    };
	_zappar_barcode_finder_found_number = function(o) {
        
        var ret = zappar.barcode_finder_found_number(o);
        return ret;
    };
	_zappar_barcode_finder_found_text = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.barcode_finder_found_text(o, indx_val);
        var bufferSize = lengthBytesUTF8(ret) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(ret, buffer, bufferSize);
        return buffer;
    };
	_zappar_barcode_finder_found_format = function(o, indx) {
        var indx_val = indx;
        var ret = zappar.barcode_finder_found_format(o, indx_val);
        return ret;
    };
	_zappar_barcode_finder_formats = function(o) {
        
        var ret = zappar.barcode_finder_formats(o);
        return ret;
    };
	_zappar_barcode_finder_formats_set = function(o, f) {
        var f_val = f;
        var ret = zappar.barcode_finder_formats_set(o, f_val);
        return ret;
    };
    
	
    _zappar_instant_world_tracker_create = function(pipeline) {
        var pipeline_val = pipeline;
        var ret = zappar.instant_world_tracker_create(pipeline_val);
        return ret;
    };
    _zappar_instant_world_tracker_destroy = function(o) {
        var o_val = o;
        var ret = zappar.instant_world_tracker_destroy(o_val);
        return ret;
    };
    _zappar_instant_world_tracker_enabled_set = function(o, enabled) {
        var enabled_val = enabled;
        var ret = zappar.instant_world_tracker_enabled_set(o, enabled_val);
        return ret;
    };
	_zappar_instant_world_tracker_enabled = function(o) {
        
        var ret = zappar.instant_world_tracker_enabled(o);
        return ret;
    };
	_zappar_instant_world_tracker_anchor_pose_raw = function(o) {
        
        var ret = zappar.instant_world_tracker_anchor_pose_raw(o);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_instant_world_tracker_anchor_pose_camera_relative = function(o, mirror) {
        var mirror_val = mirror;
        var ret = zappar.instant_world_tracker_anchor_pose_camera_relative(o, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_instant_world_tracker_anchor_pose = function(o, camera_pose, mirror) {
        var camera_pose_val = new Float32Array(16);
        camera_pose_val.set(HEAPF32.subarray(camera_pose/4, 16 + camera_pose / 4));
		var mirror_val = mirror;
        var ret = zappar.instant_world_tracker_anchor_pose(o, camera_pose_val, mirror_val);
        var buffer = _malloc(16 * 4);
        HEAPF32.set(ret, buffer / 4);
        return buffer;
    };
	_zappar_instant_world_tracker_anchor_pose_set_from_camera_offset_raw = function(o, x, y, z, orientation) {
        var x_val = x;
		var y_val = y;
		var z_val = z;
		var orientation_val = orientation;
        var ret = zappar.instant_world_tracker_anchor_pose_set_from_camera_offset_raw(o, x_val, y_val, z_val, orientation_val);
        return ret;
    };
	_zappar_instant_world_tracker_anchor_pose_set_from_camera_offset = function(o, x, y, z, orientation) {
        var x_val = x;
		var y_val = y;
		var z_val = z;
		var orientation_val = orientation;
        var ret = zappar.instant_world_tracker_anchor_pose_set_from_camera_offset(o, x_val, y_val, z_val, orientation_val);
        return ret;
    };
    

    }());
`,

    zappar_has_initialized: function() {},
    zappar_has_initialized__deps: ['$zappar_support'],

    zappar_initialize: function() {},
    zappar_initialize__deps: ['$zappar_support'],

    zappar_is_visible_webgl: function () { return document.visibilityState === "visible"; },

    zappar_pipeline_gl_context_set: function() {},
    zappar_pipeline_gl_context_set__deps: ['$zappar_support'],

    zappar_pipeline_set: function() {},
    zappar_pipeline_set__deps: ['$zappar_support'],

    zappar_pipeline_process_gl: function() {},
    zappar_pipeline_process_gl__deps: ['$zappar_support'],

    zappar_pipeline_camera_frame_upload_gl: function() {},
    zappar_pipeline_camera_frame_upload_gl__deps: ['$zappar_support'],

    
    zappar_loaded: function() {},
    zappar_loaded__deps: ['$zappar_support'],
	
    zappar_camera_default_device_id: function() {},
    zappar_camera_default_device_id__deps: ['$zappar_support'],
	
    zappar_camera_count: function() {},
    zappar_camera_count__deps: ['$zappar_support'],
	
    zappar_camera_id: function() {},
    zappar_camera_id__deps: ['$zappar_support'],
	
    zappar_camera_name: function() {},
    zappar_camera_name__deps: ['$zappar_support'],
	
    zappar_camera_user_facing: function() {},
    zappar_camera_user_facing__deps: ['$zappar_support'],
	
    zappar_projection_matrix_from_camera_model: function() {},
    zappar_projection_matrix_from_camera_model__deps: ['$zappar_support'],
	
    zappar_projection_matrix_from_camera_model_ext: function() {},
    zappar_projection_matrix_from_camera_model_ext__deps: ['$zappar_support'],
	
	
	
	
    zappar_log_level_set: function() {},
    zappar_log_level_set__deps: ['$zappar_support'],
	
	
	
	
    zappar_permission_request_ui: function() {},
    zappar_permission_request_ui__deps: ['$zappar_support'],
	
    zappar_permission_denied_ui: function() {},
    zappar_permission_denied_ui__deps: ['$zappar_support'],
	
    zappar_permission_request_all: function() {},
    zappar_permission_request_all__deps: ['$zappar_support'],
	
    zappar_permission_request_camera: function() {},
    zappar_permission_request_camera__deps: ['$zappar_support'],
	
    zappar_permission_request_motion: function() {},
    zappar_permission_request_motion__deps: ['$zappar_support'],
	
    zappar_permission_granted_all: function() {},
    zappar_permission_granted_all__deps: ['$zappar_support'],
	
    zappar_permission_granted_camera: function() {},
    zappar_permission_granted_camera__deps: ['$zappar_support'],
	
    zappar_permission_granted_motion: function() {},
    zappar_permission_granted_motion__deps: ['$zappar_support'],
	
    zappar_permission_denied_any: function() {},
    zappar_permission_denied_any__deps: ['$zappar_support'],
	
    zappar_permission_denied_camera: function() {},
    zappar_permission_denied_camera__deps: ['$zappar_support'],
	
    zappar_permission_denied_motion: function() {},
    zappar_permission_denied_motion__deps: ['$zappar_support'],
	
    zappar_analytics_project_id_set: function() {},
    zappar_analytics_project_id_set__deps: ['$zappar_support'],
    zappar_pipeline_create: function() {},
    zappar_pipeline_create__deps: ['$zappar_support'],
    zappar_pipeline_destroy: function() {},
    zappar_pipeline_destroy__deps: ['$zappar_support'],
    
    zappar_pipeline_camera_frame_upload_gl: function() {},
    zappar_pipeline_camera_frame_upload_gl__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_texture_gl: function() {},
    zappar_pipeline_camera_frame_texture_gl__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_upload_metal: function() {},
    zappar_pipeline_camera_frame_upload_metal__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_texture_metal: function() {},
    zappar_pipeline_camera_frame_texture_metal__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_upload_dx11: function() {},
    zappar_pipeline_camera_frame_upload_dx11__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_texture_dx11: function() {},
    zappar_pipeline_camera_frame_texture_dx11__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_texture_matrix: function() {},
    zappar_pipeline_camera_frame_texture_matrix__deps: ['$zappar_support'],
	
    zappar_pipeline_process_gl: function() {},
    zappar_pipeline_process_gl__deps: ['$zappar_support'],
	
    zappar_pipeline_frame_update: function() {},
    zappar_pipeline_frame_update__deps: ['$zappar_support'],
	
    zappar_pipeline_frame_number: function() {},
    zappar_pipeline_frame_number__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_model: function() {},
    zappar_pipeline_camera_model__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_pose_default: function() {},
    zappar_pipeline_camera_pose_default__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_pose_with_attitude: function() {},
    zappar_pipeline_camera_pose_with_attitude__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_pose_with_origin: function() {},
    zappar_pipeline_camera_pose_with_origin__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_user_data: function() {},
    zappar_pipeline_camera_frame_user_data__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_submit: function() {},
    zappar_pipeline_camera_frame_submit__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_submit_raw_pointer: function() {},
    zappar_pipeline_camera_frame_submit_raw_pointer__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_camera_attitude: function() {},
    zappar_pipeline_camera_frame_camera_attitude__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_device_attitude: function() {},
    zappar_pipeline_camera_frame_device_attitude__deps: ['$zappar_support'],
	
    zappar_pipeline_camera_frame_user_facing: function() {},
    zappar_pipeline_camera_frame_user_facing__deps: ['$zappar_support'],
	
    zappar_pipeline_motion_accelerometer_submit: function() {},
    zappar_pipeline_motion_accelerometer_submit__deps: ['$zappar_support'],
	
    zappar_pipeline_motion_rotation_rate_submit: function() {},
    zappar_pipeline_motion_rotation_rate_submit__deps: ['$zappar_support'],
	
    zappar_pipeline_motion_attitude_submit: function() {},
    zappar_pipeline_motion_attitude_submit__deps: ['$zappar_support'],
	
    zappar_pipeline_motion_attitude_matrix_submit: function() {},
    zappar_pipeline_motion_attitude_matrix_submit__deps: ['$zappar_support'],
	
    zappar_pipeline_sequence_record_start: function() {},
    zappar_pipeline_sequence_record_start__deps: ['$zappar_support'],
	
    zappar_pipeline_sequence_record_stop: function() {},
    zappar_pipeline_sequence_record_stop__deps: ['$zappar_support'],
	
    zappar_pipeline_sequence_record_device_attitude_matrices_set: function() {},
    zappar_pipeline_sequence_record_device_attitude_matrices_set__deps: ['$zappar_support'],
	
    zappar_pipeline_sequence_record_data: function() {},
    zappar_pipeline_sequence_record_data__deps: ['$zappar_support'],
	
    zappar_pipeline_sequence_record_data_size: function() {},
    zappar_pipeline_sequence_record_data_size__deps: ['$zappar_support'],
	
    zappar_pipeline_sequence_record_clear: function() {},
    zappar_pipeline_sequence_record_clear__deps: ['$zappar_support'],
	zappar_camera_source_create: function() {},
    zappar_camera_source_create__deps: ['$zappar_support'],
    zappar_camera_source_destroy: function() {},
    zappar_camera_source_destroy__deps: ['$zappar_support'],
    
    zappar_camera_source_start: function() {},
    zappar_camera_source_start__deps: ['$zappar_support'],
	
    zappar_camera_source_pause: function() {},
    zappar_camera_source_pause__deps: ['$zappar_support'],
	zappar_sequence_source_create: function() {},
    zappar_sequence_source_create__deps: ['$zappar_support'],
    zappar_sequence_source_destroy: function() {},
    zappar_sequence_source_destroy__deps: ['$zappar_support'],
    
    zappar_sequence_source_start: function() {},
    zappar_sequence_source_start__deps: ['$zappar_support'],
	
    zappar_sequence_source_pause: function() {},
    zappar_sequence_source_pause__deps: ['$zappar_support'],
	
    zappar_sequence_source_load_from_memory: function() {},
    zappar_sequence_source_load_from_memory__deps: ['$zappar_support'],
	
    zappar_sequence_source_max_playback_fps_set: function() {},
    zappar_sequence_source_max_playback_fps_set__deps: ['$zappar_support'],
	zappar_image_tracker_create: function() {},
    zappar_image_tracker_create__deps: ['$zappar_support'],
    zappar_image_tracker_destroy: function() {},
    zappar_image_tracker_destroy__deps: ['$zappar_support'],
    
    zappar_image_tracker_target_load_from_memory: function() {},
    zappar_image_tracker_target_load_from_memory__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_loaded_version: function() {},
    zappar_image_tracker_target_loaded_version__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_count: function() {},
    zappar_image_tracker_target_count__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_type: function() {},
    zappar_image_tracker_target_type__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_radius_top: function() {},
    zappar_image_tracker_target_radius_top__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_radius_bottom: function() {},
    zappar_image_tracker_target_radius_bottom__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_side_length: function() {},
    zappar_image_tracker_target_side_length__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_physical_scale_factor: function() {},
    zappar_image_tracker_target_physical_scale_factor__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_compressed: function() {},
    zappar_image_tracker_target_preview_compressed__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_compressed_size: function() {},
    zappar_image_tracker_target_preview_compressed_size__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_compressed_mimetype: function() {},
    zappar_image_tracker_target_preview_compressed_mimetype__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_rgba: function() {},
    zappar_image_tracker_target_preview_rgba__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_rgba_size: function() {},
    zappar_image_tracker_target_preview_rgba_size__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_rgba_width: function() {},
    zappar_image_tracker_target_preview_rgba_width__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_rgba_height: function() {},
    zappar_image_tracker_target_preview_rgba_height__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_indices: function() {},
    zappar_image_tracker_target_preview_mesh_indices__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_indices_size: function() {},
    zappar_image_tracker_target_preview_mesh_indices_size__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_vertices: function() {},
    zappar_image_tracker_target_preview_mesh_vertices__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_vertices_size: function() {},
    zappar_image_tracker_target_preview_mesh_vertices_size__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_normals: function() {},
    zappar_image_tracker_target_preview_mesh_normals__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_normals_size: function() {},
    zappar_image_tracker_target_preview_mesh_normals_size__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_uvs: function() {},
    zappar_image_tracker_target_preview_mesh_uvs__deps: ['$zappar_support'],
	
    zappar_image_tracker_target_preview_mesh_uvs_size: function() {},
    zappar_image_tracker_target_preview_mesh_uvs_size__deps: ['$zappar_support'],
	
    zappar_image_tracker_enabled: function() {},
    zappar_image_tracker_enabled__deps: ['$zappar_support'],
	
    zappar_image_tracker_enabled_set: function() {},
    zappar_image_tracker_enabled_set__deps: ['$zappar_support'],
	
    zappar_image_tracker_anchor_count: function() {},
    zappar_image_tracker_anchor_count__deps: ['$zappar_support'],
	
    zappar_image_tracker_anchor_id: function() {},
    zappar_image_tracker_anchor_id__deps: ['$zappar_support'],
	
    zappar_image_tracker_anchor_pose_raw: function() {},
    zappar_image_tracker_anchor_pose_raw__deps: ['$zappar_support'],
	
    zappar_image_tracker_anchor_pose_camera_relative: function() {},
    zappar_image_tracker_anchor_pose_camera_relative__deps: ['$zappar_support'],
	
    zappar_image_tracker_anchor_pose: function() {},
    zappar_image_tracker_anchor_pose__deps: ['$zappar_support'],
	zappar_face_tracker_create: function() {},
    zappar_face_tracker_create__deps: ['$zappar_support'],
    zappar_face_tracker_destroy: function() {},
    zappar_face_tracker_destroy__deps: ['$zappar_support'],
    
    zappar_face_tracker_model_load_from_memory: function() {},
    zappar_face_tracker_model_load_from_memory__deps: ['$zappar_support'],
	
    zappar_face_tracker_model_load_default: function() {},
    zappar_face_tracker_model_load_default__deps: ['$zappar_support'],
	
    zappar_face_tracker_model_loaded_version: function() {},
    zappar_face_tracker_model_loaded_version__deps: ['$zappar_support'],
	
    zappar_face_tracker_enabled_set: function() {},
    zappar_face_tracker_enabled_set__deps: ['$zappar_support'],
	
    zappar_face_tracker_enabled: function() {},
    zappar_face_tracker_enabled__deps: ['$zappar_support'],
	
    zappar_face_tracker_max_faces_set: function() {},
    zappar_face_tracker_max_faces_set__deps: ['$zappar_support'],
	
    zappar_face_tracker_max_faces: function() {},
    zappar_face_tracker_max_faces__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_count: function() {},
    zappar_face_tracker_anchor_count__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_id: function() {},
    zappar_face_tracker_anchor_id__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_pose_raw: function() {},
    zappar_face_tracker_anchor_pose_raw__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_pose_camera_relative: function() {},
    zappar_face_tracker_anchor_pose_camera_relative__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_pose: function() {},
    zappar_face_tracker_anchor_pose__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_identity_coefficients: function() {},
    zappar_face_tracker_anchor_identity_coefficients__deps: ['$zappar_support'],
	
    zappar_face_tracker_anchor_expression_coefficients: function() {},
    zappar_face_tracker_anchor_expression_coefficients__deps: ['$zappar_support'],
	zappar_face_mesh_create: function() {},
    zappar_face_mesh_create__deps: ['$zappar_support'],
    zappar_face_mesh_destroy: function() {},
    zappar_face_mesh_destroy__deps: ['$zappar_support'],
    
    zappar_face_mesh_load_from_memory: function() {},
    zappar_face_mesh_load_from_memory__deps: ['$zappar_support'],
	
    zappar_face_mesh_load_default: function() {},
    zappar_face_mesh_load_default__deps: ['$zappar_support'],
	
    zappar_face_mesh_load_default_full_head_simplified: function() {},
    zappar_face_mesh_load_default_full_head_simplified__deps: ['$zappar_support'],
	
    zappar_face_mesh_load_default_face: function() {},
    zappar_face_mesh_load_default_face__deps: ['$zappar_support'],
	
    zappar_face_mesh_loaded_version: function() {},
    zappar_face_mesh_loaded_version__deps: ['$zappar_support'],
	
    zappar_face_mesh_update: function() {},
    zappar_face_mesh_update__deps: ['$zappar_support'],
	
    zappar_face_mesh_indices_size: function() {},
    zappar_face_mesh_indices_size__deps: ['$zappar_support'],
	
    zappar_face_mesh_indices: function() {},
    zappar_face_mesh_indices__deps: ['$zappar_support'],
	
    zappar_face_mesh_vertices_size: function() {},
    zappar_face_mesh_vertices_size__deps: ['$zappar_support'],
	
    zappar_face_mesh_vertices: function() {},
    zappar_face_mesh_vertices__deps: ['$zappar_support'],
	
    zappar_face_mesh_normals_size: function() {},
    zappar_face_mesh_normals_size__deps: ['$zappar_support'],
	
    zappar_face_mesh_normals: function() {},
    zappar_face_mesh_normals__deps: ['$zappar_support'],
	
    zappar_face_mesh_uvs_size: function() {},
    zappar_face_mesh_uvs_size__deps: ['$zappar_support'],
	
    zappar_face_mesh_uvs: function() {},
    zappar_face_mesh_uvs__deps: ['$zappar_support'],
	zappar_face_landmark_create: function() {},
    zappar_face_landmark_create__deps: ['$zappar_support'],
    zappar_face_landmark_destroy: function() {},
    zappar_face_landmark_destroy__deps: ['$zappar_support'],
    
    zappar_face_landmark_update: function() {},
    zappar_face_landmark_update__deps: ['$zappar_support'],
	
    zappar_face_landmark_anchor_pose: function() {},
    zappar_face_landmark_anchor_pose__deps: ['$zappar_support'],
	zappar_barcode_finder_create: function() {},
    zappar_barcode_finder_create__deps: ['$zappar_support'],
    zappar_barcode_finder_destroy: function() {},
    zappar_barcode_finder_destroy__deps: ['$zappar_support'],
    
    zappar_barcode_finder_enabled_set: function() {},
    zappar_barcode_finder_enabled_set__deps: ['$zappar_support'],
	
    zappar_barcode_finder_enabled: function() {},
    zappar_barcode_finder_enabled__deps: ['$zappar_support'],
	
    zappar_barcode_finder_found_number: function() {},
    zappar_barcode_finder_found_number__deps: ['$zappar_support'],
	
    zappar_barcode_finder_found_text: function() {},
    zappar_barcode_finder_found_text__deps: ['$zappar_support'],
	
    zappar_barcode_finder_found_format: function() {},
    zappar_barcode_finder_found_format__deps: ['$zappar_support'],
	
    zappar_barcode_finder_formats: function() {},
    zappar_barcode_finder_formats__deps: ['$zappar_support'],
	
    zappar_barcode_finder_formats_set: function() {},
    zappar_barcode_finder_formats_set__deps: ['$zappar_support'],
	zappar_instant_world_tracker_create: function() {},
    zappar_instant_world_tracker_create__deps: ['$zappar_support'],
    zappar_instant_world_tracker_destroy: function() {},
    zappar_instant_world_tracker_destroy__deps: ['$zappar_support'],
    
    zappar_instant_world_tracker_enabled_set: function() {},
    zappar_instant_world_tracker_enabled_set__deps: ['$zappar_support'],
	
    zappar_instant_world_tracker_enabled: function() {},
    zappar_instant_world_tracker_enabled__deps: ['$zappar_support'],
	
    zappar_instant_world_tracker_anchor_pose_raw: function() {},
    zappar_instant_world_tracker_anchor_pose_raw__deps: ['$zappar_support'],
	
    zappar_instant_world_tracker_anchor_pose_camera_relative: function() {},
    zappar_instant_world_tracker_anchor_pose_camera_relative__deps: ['$zappar_support'],
	
    zappar_instant_world_tracker_anchor_pose: function() {},
    zappar_instant_world_tracker_anchor_pose__deps: ['$zappar_support'],
	
    zappar_instant_world_tracker_anchor_pose_set_from_camera_offset_raw: function() {},
    zappar_instant_world_tracker_anchor_pose_set_from_camera_offset_raw__deps: ['$zappar_support'],
	
    zappar_instant_world_tracker_anchor_pose_set_from_camera_offset: function() {},
    zappar_instant_world_tracker_anchor_pose_set_from_camera_offset__deps: ['$zappar_support'],

});