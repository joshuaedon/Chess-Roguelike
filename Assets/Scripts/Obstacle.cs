using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Object/*, NotWalkable*/ {
	public void setSprite(int[,] pattern, int x, int y) {
		int index = 0;
		if(x - 1 >= 0 && pattern[x - 1, y] == -1)
			index += 1;
		if(y - 1 >= 0 && pattern[x, y - 1] == -1)
			index += 2;
		if(x + 1 < pattern.GetLength(0) && pattern[x + 1, y] == -1)
			index += 4;
		if(y + 1 < pattern.GetLength(1) && pattern[x, y + 1] == -1)
			index += 8;
		GetComponent<SpriteRenderer>().sprite = sprites[index];
    }

    public void setSprite() {
		GetComponent<SpriteRenderer>().sprite = sprites[16 + Random.Range(0, 2)];
    }
}
