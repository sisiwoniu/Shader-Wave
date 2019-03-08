using UnityEngine;

[ExecuteInEditMode]
public class ApplyRefCameraTransform : MonoBehaviour
{
    [SerializeField]
    private GameObject MainCam;

    void Update()
    {
        if(MainCam != null) {
            var mainCamP = MainCam.transform.position;

            mainCamP.y = -mainCamP.y;

            transform.position = mainCamP;

            var angles = MainCam.transform.eulerAngles;

            angles.x *= -1f;

            angles.z *= -1f;

            transform.rotation = Quaternion.Euler(angles);
        }
    }
}
