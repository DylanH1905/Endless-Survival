using UnityEngine;

public class SystemMonoBehaviour : MonoBehaviour
{
    protected string className => GetType().Name;
    public bool isLoggingEnabled = false;
    protected void Info(string message)
    {
        if (isLoggingEnabled)
        {
            Debug.Log($"[{className}] {message}", this);
        }
    }

    protected void Error(string message)
    {
        if (isLoggingEnabled)
        {
            Debug.LogError($"[{className}] {message}", this);
        }
    }

    protected void Warning(string message)
    {
        if (isLoggingEnabled)
        {
            Debug.LogWarning($"[{className}] {message}", this);
        }
    }
}