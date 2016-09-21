using UnityEngine;

namespace MMADLab
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject managerCanvas;

        // Use this for initialization
        void Start()
        {
            managerCanvas.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                managerCanvas.SetActive(!managerCanvas.activeInHierarchy);
            }
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
