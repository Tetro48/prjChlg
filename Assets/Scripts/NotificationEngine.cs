using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificationEngine : MonoBehaviour
{
    public static NotificationEngine instance;
    public GameObject prefab;
    public AudioClip notifyAudio;
    public List<TextMeshProUGUI> textNotification;
    public List<GameObject> notificationInstance;
    public List<int> notifAnimFrames;
    

    public void InstantiateNotification(string text, Color color)
    {
        Debug.Log(text);
        MenuEngine.instance.audioSource.PlayOneShot(notifyAudio);
        GameObject notifInstantiate = GameObject.Instantiate(prefab, transform);
        if(notificationInstance.Count > 0) for (int i = 0; i < notificationInstance.Count; i++)
        {
            notificationInstance[i].transform.position += new Vector3(0.0f, 80f*MenuEngine.instance.reswidth, 0.0f);
        }
        notificationInstance.Add(notifInstantiate);
        // notifInstantiate.GetComponent<SpriteRenderer>().color = color;
        TextMeshProUGUI textNotif = notifInstantiate.GetComponent<NotificationObject>().text.GetComponent<TextMeshProUGUI>();
        textNotification.Add(textNotif);
        textNotif.text = text;
        notifAnimFrames.Add(0);
    }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(notificationInstance.Count > 0) for (int i = 0; i < notificationInstance.Count; i++)
        {
            notifAnimFrames[i]++;
            if (notifAnimFrames[i] < 30)
            {
                notificationInstance[i].transform.position -= new Vector3((256/15)*MenuEngine.instance.reswidth, 0, 0);
            }
            if (notifAnimFrames[i] > 569)
            {
                notificationInstance[i].transform.position += new Vector3((256/15)*MenuEngine.instance.reswidth, 0, 0);
            }
            if (notifAnimFrames[i] > 600)
            {
                Destroy(notificationInstance[i]);
                notificationInstance.RemoveAt(i);
                textNotification.RemoveAt(i);
                notifAnimFrames.RemoveAt(i);
            }
        }
    }
}
