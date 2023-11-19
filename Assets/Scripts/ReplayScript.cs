using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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

public static class ReplayScript
{
    public static void SaveData<T>(T data, in string save_name)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, save_name.ToString());
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    /// <summary>
    ///Note: You have to manually manage this through code.
    /// </summary>
    public static dynamic LoadData(string saveName)
    {
        string path = Path.Combine(Application.persistentDataPath, saveName.ToString());
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            dynamic data = formatter.Deserialize(stream);

            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("A file is not there! Path: " + path);
            return null;
        }
    }
    public static void SaveReplay(ReplayRecord replayRecord, string saveName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "replay-" + saveName + ".clg");
        FileStream stream = new FileStream(path, FileMode.Create);

        ReplayVars data = new ReplayVars(replayRecord);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static ReplayVars LoadReplay(string saveName)
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
            Debug.LogError("Replay file is not there! Path: " + path);
            return null;
        }
    }
    public static void SaveInputConfig(ReplayRecord replayRecord, string saveName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "replay-" + saveName + ".clg");
        FileStream stream = new FileStream(path, FileMode.Create);

        ReplayVars data = new ReplayVars(replayRecord);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static InputVars LoadInputConfig(string saveName)
    {
        string path = Path.Combine(Application.persistentDataPath, "replay-" + saveName + ".clg");
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            InputVars data = formatter.Deserialize(stream) as InputVars;

            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("File is not there! Path: " + path);
            return null;
        }
    }
}
