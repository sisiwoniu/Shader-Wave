using UnityEngine;

//波を起こすクラス
public class Wave : MonoBehaviour {
    //それぞれ「波計算マテリアル」と「波発生ポイントを書き込むマテリアル」
    public Material waveMaterial, InputWPMaterial;

    [Range(1, 10)]
    public int UpdateFrameTiming = 3;

    public bool debug = true;

    public Camera RefCam;

    private Texture2D init;

    private RenderTexture input;

    private RenderTexture prev;

    private RenderTexture prev2;

    private RenderTexture result;

    private Renderer s_renderer;

    private readonly int PropertyInputTex = Shader.PropertyToID("_InputTex");

    private readonly int PropertyPrevTex = Shader.PropertyToID("_PrevTex");

    private readonly int PropertyPrev2Tex = Shader.PropertyToID("_Prev2Tex");

    private readonly int PropertyWaveTex = Shader.PropertyToID("_WaveTex");

    private readonly int InputWPProperty_UV = Shader.PropertyToID("_uv");

    //接触点のUVを更新
    public void UpdateFootPoint(Vector2 UV) { 

        InputWPMaterial.SetVector(InputWPProperty_UV, UV);

        //波バッファ更新
        Graphics.Blit(null, input, InputWPMaterial);

        //更新したバッファを適用し、波を発生させる
        UpdateWave();
    }

    private void Start() {
        //入力初期化用ためのテクスチャなので、1pixel分だけでよかった
        init = new Texture2D(1, 1);

        init.SetPixel(0, 0, new Color(0, 0, 0, 1));

        init.Apply();

        //入力用テクスチャを取得し、波動方程式を求めるのに必要なバッファを生成
        input = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.R8);

        prev = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.R8);

        prev2 = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.R8);

        result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.R8);

        //バッファ初期化
        var r8Init = new Texture2D(1, 1);

        r8Init.SetPixel(0, 0, new Color(0f, 0, 0, 1f));

        r8Init.Apply();

        Graphics.Blit(r8Init, prev);

        Graphics.Blit(r8Init, prev2);

        s_renderer = GetComponent<Renderer>();
    }

    //フレームの最後で波バッファをクリア
    private void LateUpdate() {
        UpdateWave();

        //入力テクスチャ初期化
        Graphics.Blit(init, input);
    }

    private void UpdateWave() {
        if(Time.frameCount % UpdateFrameTiming != 0 || input == null) {
            return;
        }

        waveMaterial.SetTexture(PropertyInputTex, input);

        waveMaterial.SetTexture(PropertyPrevTex, prev);

        waveMaterial.SetTexture(PropertyPrev2Tex, prev2);

        //波動方程式を解いてreusltに格納
        Graphics.Blit(null, result, waveMaterial);

        //バッファを残す
        var tmp = prev2;

        prev2 = prev;

        prev = result;

        result = tmp;

        //現在のレンダリング結果を波テクスチャとして利用
        s_renderer.sharedMaterial.SetTexture(PropertyWaveTex, prev);
    }

    private void OnGUI() {
        if(debug) {
            var h = Screen.height / 2;
            const int StrWidth = 20;
            GUI.Box(new Rect(0, 0, h, h * 2), "");
            GUI.DrawTexture(new Rect(0, 0 * h, h, h), Texture2D.whiteTexture);
            //GUI.DrawTexture(new Rect(0, 0 * h, h, h), input);
            GUI.DrawTexture(new Rect(0, 0 * h, h, h), prev);
            GUI.DrawTexture(new Rect(0, 1 * h, h, h), prev2);
            //GUI.Box(new Rect(0, 1 * h - StrWidth, h, StrWidth), "INPUT");
            GUI.Box(new Rect(0, 1 * h - StrWidth, h, StrWidth), "PREV");
            GUI.Box(new Rect(0, 2 * h - StrWidth, h, StrWidth), "PREV2");
        }
    }
}
