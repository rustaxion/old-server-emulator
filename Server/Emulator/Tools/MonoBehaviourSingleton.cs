// https://web.archive.org/web/20170615020710/https://wiki.unity3d.com/index.php/CoroutineHelper

using UnityEngine;

namespace Server.Emulator.Tools;

public class MonoBehaviourSingleton< TSelfType > : MonoBehaviour where TSelfType : MonoBehaviour
{
    private static TSelfType m_Instance = null;
    public static TSelfType Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = (TSelfType)FindObjectOfType(typeof(TSelfType));
                if (m_Instance == null)
                {
                    m_Instance = (new GameObject(typeof(TSelfType).Name)).AddComponent<TSelfType>();
                }
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }
}