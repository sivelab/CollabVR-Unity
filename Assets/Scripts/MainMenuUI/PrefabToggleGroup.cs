using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ToggleGroup))]
public class PrefabToggleGroup : MonoBehaviour
{
    [SerializeField]
    private AppNetworkManager m_networkManager;

    private ToggleGroup group;

    // Use this for initialization
    void Start()
    {
        group = GetComponent<ToggleGroup>();
        if (group.AnyTogglesOn())
        {
            var toggle = group.ActiveToggles().First();
            var prefab = toggle.GetComponent<PrefabToggleHandler>().Prefab;
            SetPlayerPrefab(prefab);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPlayerPrefab(GameObject prefab)
    {
        m_networkManager.SetPlayerPrefab(prefab);
    }
}
