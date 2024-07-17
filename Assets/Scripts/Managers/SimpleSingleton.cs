
using UnityEngine;
public class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject c = new GameObject(typeof(T).ToString());
                    instance = c.AddComponent<T>();
                    Debug.Log("Instanted new " + typeof(T).ToString());
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
