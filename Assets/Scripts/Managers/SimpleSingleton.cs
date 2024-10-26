
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
public class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    /// <summary>
    ///  is two threads might check if the instance is null at the same time and both find it to be true.
    ///  As a result, they each create an instance, which goes against the rule of having only one instance in the singleton pattern.
    ///  source https://thecodeman.net/posts/how-to-use-singleton-in-multithreading
    /// </summary>
    private static readonly object padlock = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject c = new GameObject(typeof(T).ToString());
                        instance = c.AddComponent<T>();
                        Debug.Log("Instanted new " + typeof(T).ToString());
                    }
                }
            }
                return instance;
            }
    }
    protected virtual void Awake()

    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<T>();
        }
        else if(instance!=this)
        {
            Destroy(gameObject);
        }
    }
}
