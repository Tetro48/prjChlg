public class History6Rolls35Bag : IRandomizer
{
    bool first = true;
	int[] history = {3, 2, 3, 2};
	int[] pool = {
		1, 1, 1, 1, 1,
		6, 6, 6, 6, 6,
		4, 4, 4, 4, 4,
		5, 5, 5, 5, 5,
		2, 2, 2, 2, 2,
		3, 3, 3, 3, 3,
		0, 0, 0, 0, 0
	};
	int[] droughts = {0, 0, 0, 0, 0, 0, 0};
    System.Random random;
    int historyCount = 0;
    public History6Rolls35Bag(int seed)
    {
        random = new System.Random(seed);
    }
    public int getNextPieceID()
    {
        int index = 0, x = 0;
        if (first)
        {
            index = random.Next(0, 20);
            x = pool[index];
            first = false;
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                index = random.Next(0, 35);
                x = pool[index];
                if (!inHistory(x, history) || i == 5)
                    break;
            }
        }
        pool[index] = updateHistory(x);
        return x;
    }
    
    int updateHistory(int shape)
    {
        history[historyCount] = shape;

        historyCount++;
        historyCount %= history.Length;

        int highdrought = 0;
        int highdroughtcount = 0;
        for (int i = 0; i < droughts.Length; i++)
        {
            if (i == shape)
            {
                droughts[i] = 0;
            }
            else
            {
                droughts[i]++;
                if (droughts[i] >= highdroughtcount)
                {
                    highdrought = i;
                    highdroughtcount = droughts[i];
                }
            }
        }
        return highdrought;
    }

    bool inHistory(int pieceID, int[] history)
    {
        foreach (var item in history)
        {
            if (pieceID == item)
            {
                return true;
            }
        }
        return false;
    }
}