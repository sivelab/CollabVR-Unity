using UnityEngine;
using System.Collections;

public class WarpManager : MonoBehaviour
{

    public Material[] warpMaterials;
    public float effectScale;

    // Use this for initialization
    void Start()
    {
    
    }
    
    // Update is called once per frame
    void Update()
    {
        // check for input
        if (Input.GetAxis("IncreaseWarp") > 0.1)
        {
            foreach (var warpMaterial in warpMaterials)
            {
                float scale = warpMaterial.GetFloat("_EffectScale");
                scale += effectScale;
                warpMaterial.SetFloat("_EffectScale", scale);
            }
        } else if (Input.GetAxis("DecreaseWarp") > 0.1)
        {
            foreach (var warpMaterial in warpMaterials)
            {
                float scale = warpMaterial.GetFloat("_EffectScale");
                scale -= effectScale;
                warpMaterial.SetFloat("_EffectScale", scale);
            }
        }
    }
}
