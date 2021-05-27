using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2021  Aymir

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

public class TileParticleSystem : MonoBehaviour
{
    public static TileParticleSystem instance;
    public ParticleSystem particleEmitters;
    public Color[] tileColors;
    public Color[,] grid;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        grid = new Color[BoardController.instance.gridSizeX, BoardController.instance.gridSizeY];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SummonParticles(Vector2Int coords, int tileTexture)
    {
        // Any parameters we assign in emitParams will override the current system's when we call Emit.
        // Here we will override the start color and size.
        Debug.Log("X: " + coords.x + ", Y: " + coords.y + ", Tile Texture: " + tileTexture);
        if(tileTexture < 14)
        {
            // particleEmitters.Stop();
            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = new Vector3((float)coords.x, (float)coords.y, 0f);
            emitParams.startColor = tileColors[tileTexture];
            particleEmitters.Emit(emitParams, 64);
            particleEmitters.Play(); // Continue normal emissions
        }
    }
}
