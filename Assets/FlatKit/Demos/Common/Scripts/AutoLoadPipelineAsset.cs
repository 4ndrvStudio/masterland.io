using UnityEngine;
using UnityEngine.Rendering;

namespace FlatKit {
[ExecuteAlways]
public class AutoLoadPipelineAsset : MonoBehaviour {
    [SerializeField]
    private RenderPipelineAsset pipelineAsset;
    private RenderPipelineAsset _previousPipelineAsset;
    private bool _overrodeQualitySettings;

    private void OnEnable() {
        UpdatePipeline();
    }

    private void OnDisable() {
        ResetPipeline();
    }

    private void OnValidate() {
        UpdatePipeline();
    }

    private void UpdatePipeline() {
        if (pipelineAsset) {
            if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != pipelineAsset) {
                _previousPipelineAsset = QualitySettings.renderPipeline;
                QualitySettings.renderPipeline = pipelineAsset;
                _overrodeQualitySettings = true;
            } else if (GraphicsSettings.defaultRenderPipeline != pipelineAsset) {
                _previousPipelineAsset = GraphicsSettings.defaultRenderPipeline;
                GraphicsSettings.defaultRenderPipeline = pipelineAsset;
                _overrodeQualitySettings = false;
            }
        }
    }

    private void ResetPipeline() {
        if (_previousPipelineAsset) {
            if (_overrodeQualitySettings) {
                QualitySettings.renderPipeline = _previousPipelineAsset;
            } else {
                GraphicsSettings.defaultRenderPipeline = _previousPipelineAsset;
            }
        }
    }
}
}