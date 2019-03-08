using UnityEngine;

//ワールドポジションからUVに変換する
public class WorldPosToUV : MonoBehaviour {

    [SerializeField]
    private Wave wave;

    private void Update() {
        if(Input.GetMouseButton(0)) {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)) {
                var meshRenderer = hit.transform.GetComponent<MeshFilter>();

                var mesh = meshRenderer.sharedMesh;

                //trianglesは三角形（精密に言うと「頂点インデックス並びの配列」）の配列
                for(int i = 0; i < mesh.triangles.Length; i++) {
                    #region 座標が平面上に存在しているかどうか

                    var i_0 = i + 0;

                    var i_1 = i + 1;

                    var i_2 = i + 2;

                    Vector3 p0 = mesh.vertices[mesh.triangles[i_0]];

                    Vector3 p1 = mesh.vertices[mesh.triangles[i_1]];

                    Vector3 p2 = mesh.vertices[mesh.triangles[i_2]];

                    Vector3 p = hit.transform.InverseTransformPoint(hit.point);

                    var v0 = p1 - p0;

                    var v1 = p2 - p1;

                    var vp = p - p0;

                    var nv = Vector3.Cross(v0, vp);

                    var dv = Vector3.Dot(nv, vp);

                    //内積が0の場合垂直
                    var pInMesh = -0.000001f < dv && dv < 0.000001f;

                    #endregion

                    #region 平面上にあった点Pが三角形に収まっているかどうか

                    if(!pInMesh)
                        continue;
                    else {

                        var a = Vector3.Cross(p0 - p2, p - p0).normalized;

                        var b = Vector3.Cross(p1 - p0, p - p1).normalized;

                        var c = Vector3.Cross(p2 - p1, p - p2).normalized;

                        var aDotb = Vector3.Dot(a, b);

                        var bDotc = Vector3.Dot(b, c);

                        //内積結果が1の場合平行
                        pInMesh = 0.999f < aDotb && 0.999f < bDotc;
                    }
                    #endregion

                    #region PをUVに変換

                    if(!pInMesh)
                        continue;
                    else {
                        var uv0 = mesh.uv[mesh.triangles[i_0]];

                        var uv1 = mesh.uv[mesh.triangles[i_1]];

                        var uv2 = mesh.uv[mesh.triangles[i_2]];

                        //変換行列（透視投影を考慮したUV補間）
                        var mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * hit.transform.localToWorldMatrix;

                        //各点を変換する(プロジェクション空間に)
                        var p0_P = mvp * new Vector4(p0.x, p0.y, p0.z, 1);

                        var p1_P = mvp * new Vector4(p1.x, p1.y, p1.z, 1);

                        var p2_P = mvp * new Vector4(p2.x, p2.y, p2.z, 1);

                        var p_P = mvp * new Vector4(p.x, p.y, p.z, 1);

                        //同次座標から通常座標に変換
                        Vector2 p0_v = new Vector2(p0_P.x, p0_P.y) / p0_P.w;

                        Vector2 p1_v = new Vector2(p1_P.x, p1_P.y) / p1_P.w;

                        Vector2 p2_v = new Vector2(p2_P.x, p2_P.y) / p2_P.w;

                        Vector2 p_v = new Vector2(p_P.x, p_P.y) / p_P.w;

                        //Pにより三角形を分割し、必要の面積を計算
                        var s = ((p1_v.x - p0_v.x) * (p2_v.y - p0_v.y) - (p1_v.y - p0_v.y) * (p2_v.x - p0_v.x)) * 0.5f;

                        var s1 = ((p2_v.x - p_v.x) * (p0_v.y - p_v.y) - (p2_v.y - p_v.y) * (p0_v.x - p_v.x)) * 0.5f;

                        var s2 = ((p0_v.x - p_v.x) * (p1_v.y - p_v.y) - (p0_v.y - p_v.y) * (p1_v.x - p_v.x)) * 0.5f;

                        //面積比でUV補間
                        float u = s1 / s;

                        float v = s2 / s;

                        float w = 1 / ((1 - u - v) * 1 / p0_P.w + u * 1 / p1_P.w + v * 1 / p2_P.w);

                        var uv = w * ((1 - u - v) * uv0 / p0_P.w + u * uv1 / p1_P.w + v * uv2 / p2_P.w);

                        if(wave != null)
                            wave.UpdateFootPoint(uv);

                        Debug.Log(uv + ":" + hit.textureCoord);

                        return;
                    }

                    #endregion
                }
            }
        }       
    }
}