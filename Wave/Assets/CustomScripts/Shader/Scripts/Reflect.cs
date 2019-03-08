using UnityEngine;

[ExecuteInEditMode]
public class Reflect : MonoBehaviour {

    [SerializeField]
    private Camera RefCam;

    [SerializeField]
    private int RelTexWidth = -1;

    [SerializeField]
    private int RelTexHeight = -1;

    private Material sharedMaterial;

    private Renderer s_renderer;

    private int relTexWidth = -1;

    private int relTexHeight = -1;

    private readonly int ShaderPropertyReflectTex = Shader.PropertyToID("_RefTex");

    private readonly int ShaderProperty_RefVP = Shader.PropertyToID("_RefVP");

    private readonly int ShaderProperty_RefW = Shader.PropertyToID("_RefW");

    private void Start() {
        //反射用カメラにスクリーンと同じサイズのバッファを設定し、反射用テクスチャとしてマテリアルにセット
        s_renderer = GetComponent<Renderer>();

        //マテリアル取得インスタンス起こさないやり方
        sharedMaterial = s_renderer.sharedMaterial;

        relTexWidth = RelTexWidth <= -1 ? Screen.width : RelTexWidth;

        relTexHeight = RelTexHeight <= -1 ? Screen.height : RelTexHeight;

        //反射用カメラにレンダラーテクスチャを作成
        RefCam.targetTexture = new RenderTexture(relTexWidth, relTexHeight, 16);

        //マテリアルに反射テクスチャをセット
        sharedMaterial.SetTexture(ShaderPropertyReflectTex, RefCam.targetTexture);
    }

    //レンダリング前にすべてのオブジェクトから呼ばれる、カメラごとに呼ぶ
    private void OnWillRenderObject() {
        var cam = Camera.current;

        //反射用カメラだけ適用
        if(cam == RefCam) {
            //ワールドからビュー変換用変換行列
            var refVMatrix = cam.worldToCameraMatrix;

            //ビューからスクリーンスペース変換用変換行列を用意
            //カメラの射影行列からCPUの射影行列に変換する
            var refPMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);

            //ビューからスクリーンスペースに変換用の変換行列を作成
            var refVP = refPMatrix * refVMatrix;

            var refW = s_renderer.localToWorldMatrix;

            sharedMaterial.SetMatrix(ShaderProperty_RefVP, refVP);

            sharedMaterial.SetMatrix(ShaderProperty_RefW, refW);

            if(!Application.isPlaying && sharedMaterial.GetTexture(ShaderPropertyReflectTex) == null) {
                sharedMaterial.SetTexture(ShaderPropertyReflectTex, RefCam.targetTexture);
            }
        }
    }
}
