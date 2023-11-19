using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
    Project Challenger, a challenging block stacking game.
    Copyright (C) 2022-2023, Aymir

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
    public List<float> notifAnimFrames;
    public List<bool> isOverboard;

    public static void Notify(string text, Color color = default)
    {
        instance.InstantiateNotification(text, color);
    }
    public void InstantiateNotification(string text, Color color = default)
    {
        if (GameEngine.debugMode)
        {
            Debug.Log(text);
        }

        int strheight = Mathf.FloorToInt(text.Length / 76) * 38 + 38;
        MenuEngine.instance.audioSource.PlayOneShot(notifyAudio);
        GameObject notifInstantiate = GameObject.Instantiate(prefab, transform);
        if (notificationInstance.Count > 0)
        {
            for (int i = 0; i < notificationInstance.Count; i++)
            {
                notificationInstance[i].transform.position += new Vector3(0.0f, (34f + strheight) * (float)(Screen.height / 1080.0), 0.0f);
            }
        }

        Vector3 notifPos = notifInstantiate.transform.localPosition;
        notifPos.y += strheight - 38;
        notifInstantiate.transform.localPosition = notifPos;
        notificationInstance.Add(notifInstantiate);
        notifInstantiate.GetComponent<Image>().color = color;
        RectTransform notifTransf = notifInstantiate.GetComponent<RectTransform>();
        Vector3 size = notifTransf.sizeDelta;
        size.y = strheight;
        notifTransf.sizeDelta = size;
        TextMeshProUGUI textNotif = notifInstantiate.GetComponent<NotificationObject>().text.GetComponent<TextMeshProUGUI>();
        textNotification.Add(textNotif);
        textNotif.text = text;
        notifAnimFrames.Add(0);
        isOverboard.Add(true);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }

    //Called when there is an exception
    private void LogCallback(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            Notify(condition, Color.red);
        }
        // Notify("StackTrace: " + stackTrace, Color.red);
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    // Start is called before the first frame update
    private void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
        if (notificationInstance.Count > 0)
        {
            for (int i = 0; i < notificationInstance.Count; i++)
            {
                notifAnimFrames[i] += Time.deltaTime / Time.fixedDeltaTime;
                if (notifAnimFrames[i] < 50)
                {
                    notificationInstance[i].transform.position -= new Vector3((384 / 25) * MenuEngine.instance.reswidth * (Time.deltaTime / Time.fixedDeltaTime), 0, 0);
                }
                else if (isOverboard[i])
                {
                    Vector3 getPos = notificationInstance[i].transform.localPosition;
                    getPos.x = 308f;
                    notificationInstance[i].transform.localPosition = getPos;
                    isOverboard[i] = false;
                }
                if (notifAnimFrames[i] > 449)
                {
                    notificationInstance[i].transform.position += new Vector3((384 / 25) * MenuEngine.instance.reswidth * (Time.deltaTime / Time.fixedDeltaTime), 0, 0);
                }
                if (notifAnimFrames[i] > 500)
                {
                    Destroy(notificationInstance[i]);
                    notificationInstance.RemoveAt(i);
                    textNotification.RemoveAt(i);
                    notifAnimFrames.RemoveAt(i);
                    isOverboard.RemoveAt(i);
                }
            }
        }
    }
}
