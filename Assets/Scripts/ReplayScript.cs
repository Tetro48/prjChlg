using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ReplayScript
{
    public static void SaveReplay(ReplayRecord replayRecord, string saveName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "replay-" + saveName + ".clg");
        FileStream stream = new FileStream(path, FileMode.Create);

        ReplayVars data = new ReplayVars(replayRecord);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static ReplayVars LoadReplay (string saveName)
    {
        string path = Path.Combine(Application.persistentDataPath, "replay-" + saveName + ".clg");
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ReplayVars data = formatter.Deserialize(stream) as ReplayVars;

            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("File is not initialized!");
            return null;
        }
    }
}
