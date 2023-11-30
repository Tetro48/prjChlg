public class Bag7 : IRandomizer 
{
    private int[] bag = {0, 1, 2, 3, 4, 5, 6};
    
    private int count = 0;
    
    public int getNextPieceID()
    {
        if (count == 0)
        {
            bag.Shuffle();
        }
        count++;
        count %= 7;
        return bag[count];
    }
}