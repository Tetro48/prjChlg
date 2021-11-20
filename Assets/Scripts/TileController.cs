using System.Collections;
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

public class TileController : MonoBehaviour {
    public PieceController pieceController;
    public int tileIndex;
    public int textureID;
    Material material;
    float transparency;
    void Awake()
    {
        material = gameObject.GetComponent<MeshRenderer>().material;
    }
    void Update()
    {
        int hideTilesPerUpdates = pieceController.board.tileInvisTime;
        if(transparency < 1) if (hideTilesPerUpdates > 0 && pieceController.isPieceLocked())
        {
            float percentage = 1f/hideTilesPerUpdates;
            float alphaUpd = percentage * Time.deltaTime / Time.fixedDeltaTime;
            if (transparency + alphaUpd > 1)
            {
                alphaUpd = 1 - transparency;
            }
            transparency += alphaUpd;
            material.color -= new Color(0f,0f,0f, alphaUpd);
        }
    }
}
