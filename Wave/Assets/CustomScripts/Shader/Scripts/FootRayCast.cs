using UnityEngine;

public class FootRayCast : MonoBehaviour {

    [SerializeField]
    private Wave wave;

    [SerializeField, Range(0f, 10f)]
    private float RayDur = 0.1f;

    public bool ShowRay = false;

    public bool KeepPos = true;

    private Vector3 prevPos = new Vector3(-999f, -999f, -999f);

    private int instanceID = 0;

    private void Start() {
        instanceID = GetInstanceID();
    }

    private void Update() {
        RaycastHit hit;

        Physics.Raycast(transform.position, -transform.up, out hit, RayDur);

        if(ShowRay)
            Debug.DrawRay(transform.position, -transform.up, Color.red, RayDur, false);

        if(hit.collider != null && wave != null) {
            if(!KeepPos) {
                var d = (hit.point - prevPos).sqrMagnitude;

                if(d < 0.001f) {
                    return;
                }
            }

            prevPos = hit.point;

            wave.UpdateFootPoint(hit.textureCoord);
        } else {
            prevPos.x = -999f;

            prevPos.y = -999f;

            prevPos.z = -999f;
        }
    }
}
