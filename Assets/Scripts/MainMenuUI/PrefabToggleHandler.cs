using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(Toggle))]
public class PrefabToggleHandler : MonoBehaviour {
    [SerializeField]
    private GameObject m_prefab;
    public GameObject Prefab
    {
        get { return m_prefab; }
    }

    private Toggle toggle;
    private PrefabToggleGroup toggleGroup;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(HandleToggleChanged);
        toggleGroup = toggle.group.GetComponent<PrefabToggleGroup>();
    }

    public void HandleToggleChanged(bool value)
    {
        if (value)
        {
            toggleGroup.SetPlayerPrefab(m_prefab);
        }
    }
}
