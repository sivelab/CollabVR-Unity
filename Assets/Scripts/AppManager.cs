using UnityEngine;

namespace MMADLab
{
    public class AppManager : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                Debug.Break();
#else
            Application.Quit();
#endif
            }
        }
    }
}
