﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class BoardParticleSystem : MonoBehaviour
{
    public static BoardParticleSystem instance;
    public ParticleSystem particleEmitters;
    public Color[] tileColors;
    public Color[,] grid;
    public GameObject fireworkPrefab, tileParticlesPrefab;
    public AudioClip[] fireworkSoundEffects;
    public List<GameObject> fireworkInstances, tileParticleInstances;
    public List<int> fireworkTime, tileParticleTime;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        grid = new Color[BoardController.instance.gridSizeX, BoardController.instance.gridSizeY];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(fireworkInstances.Count > 0) for (int i = 0; i < fireworkInstances.Count; i++)
        {
            fireworkTime[i]++;
            if (fireworkTime[i] > 400)
            {
                Destroy(fireworkInstances[i]);
                fireworkInstances.RemoveAt(i);
                fireworkTime.RemoveAt(i);
            }
        }
        if(tileParticleInstances.Count > 0) for (int i = 0; i < tileParticleInstances.Count; i++)
        {
            tileParticleTime[i]++;
            if (tileParticleTime[i] > 200)
            {
                Destroy(tileParticleInstances[i]);
                tileParticleInstances.RemoveAt(i);
                tileParticleTime.RemoveAt(i);
            }
        }
    }
    
    public void SummonFirework(Vector2 coordinates, Vector2 borderSize)
    {
        MenuEngine.instance.audioSource.PlayOneShot(fireworkSoundEffects[Random.Range(0, fireworkSoundEffects.Length-1)]);
        GameObject newFirework = Instantiate(fireworkPrefab, transform);
        newFirework.transform.position = new Vector3(Random.Range(coordinates.x, coordinates.x + borderSize.x), Random.Range(coordinates.y, coordinates.y + borderSize.y), 0.0f);
        ParticleSystem particlesModify = newFirework.GetComponent<ParticleSystem>();
        particlesModify.startColor = new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f),1f);
        particlesModify.Play();
    }
    public void SummonParticles(Vector2Int coords, int tileTexture)
    {
        // Any parameters we assign in emitParams will override the current system's when we call Emit.
        // Here we will override the start color and size.
        Debug.Log("X: " + coords.x + ", Y: " + coords.y + ", Tile Texture: " + tileTexture);
        if(tileTexture < 14)
        {
            // particleEmitters.Stop();
            // var emitParams = new ParticleSystem.EmitParams();
            // emitParams.position = new Vector3((float)coords.x, (float)coords.y, 0f);
            // emitParams.startColor = tileColors[tileTexture];
            // particleEmitters.Emit(emitParams, 64);
            // particleEmitters.Play(); // Continue normal emissions
            GameObject newParticle = GameObject.Instantiate(tileParticlesPrefab, transform);
            newParticle.transform.position = new Vector3((float)coords.x, (float)coords.y, 0f);
            ParticleSystem particlesModify = newParticle.GetComponent<ParticleSystem>();
            particlesModify.startColor = tileColors[tileTexture];
            particlesModify.Play();
        }
    }
}
