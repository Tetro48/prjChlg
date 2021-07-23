using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2021, Aymir

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

public class NotificationEngine : MonoBehaviour
{
    public static NotificationEngine instance;
    public GameObject prefab;
    public AudioClip notifyAudio;
    public List<TextMeshProUGUI> textNotification;
    public List<GameObject> notificationInstance;
    public List<int> notifAnimFrames;
    

    public void InstantiateNotification(string text, Color color = default)
    {
        Debug.Log(text);
        MenuEngine.instance.audioSource.PlayOneShot(notifyAudio);
        GameObject notifInstantiate = GameObject.Instantiate(prefab, transform);
        if(notificationInstance.Count > 0) for (int i = 0; i < notificationInstance.Count; i++)
        {
            notificationInstance[i].transform.position += new Vector3(0.0f, 72f*(float)(Screen.height / 1080.0), 0.0f);
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
    void FixedUpdate()
    {
        if(notificationInstance.Count > 0) for (int i = 0; i < notificationInstance.Count; i++)
        {
            notifAnimFrames[i]++;
            if (notifAnimFrames[i] < 50)
            {
                notificationInstance[i].transform.position -= new Vector3((256/25)*MenuEngine.instance.reswidth, 0, 0);
            }
            if (notifAnimFrames[i] > 449)
            {
                notificationInstance[i].transform.position += new Vector3((256/25)*MenuEngine.instance.reswidth, 0, 0);
            }
            if (notifAnimFrames[i] > 500)
            {
                Destroy(notificationInstance[i]);
                notificationInstance.RemoveAt(i);
                textNotification.RemoveAt(i);
                notifAnimFrames.RemoveAt(i);
            }
        }
    }
}
