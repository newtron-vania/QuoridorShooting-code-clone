using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListUtil
{
    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;

        //맨 뒤부터 자리 확정
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);

            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
}
