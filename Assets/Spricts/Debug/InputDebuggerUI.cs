using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputDebuggerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    private System.IDisposable eventListener;

    private void Start()
    {
        // スクリプト起動時に文字が変わるかテスト
        if (debugText != null)
        {
            debugText.text = "スクリプト接続成功：キーを押してください";
        }
        else
        {
            Debug.LogError("【エラー】debugText が Inspector で割り当てられていません！");
        }
    }

    private void OnEnable()
    {
        eventListener = InputSystem.onAnyButtonPress.Call(control =>
        {       
                string inputText = $"{control.device.displayName} : {control.name}";
                UpdateText(inputText);
        });
    }

    private void OnDisable()
    {
        eventListener?.Dispose();
    }

    private void UpdateText(string text)
    {
        if (debugText != null)
        {
            
            debugText.text = $"Last Input: {text}";
        }
    }
}