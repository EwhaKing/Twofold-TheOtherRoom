using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class UpperAlphaInput : MonoBehaviour
{
    TMP_InputField field;

    void Awake()
    {
        field = GetComponent<TMP_InputField>();
        field.onValidateInput = (text, idx, c) =>
        {
            c = char.ToUpperInvariant(c);
            return c >= 'A' && c <= 'Z' ? c : '\0';
        };
    }

    void Update()
    {
        if (!field.isFocused) return;
        if (Input.compositionString.Length > 0) return;

        Input.imeCompositionMode = IMECompositionMode.Off;
    }

    void OnDisable()
    {
        Input.imeCompositionMode = IMECompositionMode.Auto;
    }
}
