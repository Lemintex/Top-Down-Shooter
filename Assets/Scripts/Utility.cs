using System.Collections;
using System.Collections.Generic;

public static class Utility
{
    // generates and returns a shuffled version of the argument array
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random rnd = new System.Random(seed);
        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = rnd.Next(i, array.Length);
            T temp = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = temp;
        }
        return array;
    }
}