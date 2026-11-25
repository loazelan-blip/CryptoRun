using UnityEngine;
using UnityEngine.InputSystem;

public class ControlesLoader : MonoBehaviour
{
    public InputActionAsset asset;
    public static bool Joystick;   // false = teclado
    const string KEY = "binds";

    void Awake()
    {
        var json = PlayerPrefs.GetString(KEY, "");
        if (!string.IsNullOrEmpty(json)) asset.LoadBindingOverridesFromJson(json);
        asset.Enable();
    }

    public static void Salvar(InputActionAsset a)
    {
        PlayerPrefs.SetString(KEY, a.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }
}