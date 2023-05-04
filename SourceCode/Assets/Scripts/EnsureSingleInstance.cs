using System.Threading;
using UnityEngine;

public class EnsureSingleInstance : MonoBehaviour
{
 private static Mutex _mutex;

    void Awake()
    {
        _mutex = new Mutex(true, "DashPongMutex");
        if (!_mutex.WaitOne(System.TimeSpan.Zero))
        {
            Application.Quit();
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        _mutex?.ReleaseMutex();
    }
}
