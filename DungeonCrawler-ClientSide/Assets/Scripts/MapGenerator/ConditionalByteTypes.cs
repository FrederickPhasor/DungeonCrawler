using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalByteTypes : MonoBehaviour
{
    //2way straight
    public static HashSet<int> corridorStraight = new HashSet<int>
    {
        0b0101,//base
        0b1010
    };

    //2 way L shape
    public static HashSet<int> corridorLShape = new HashSet<int>
    {
        0b0011,//base
        0b1100,
        0b1001,
        0b0110,
    };

    //3way
    public static HashSet<int> corridor3Way = new HashSet<int>
    {
        0b1101,
        0b0111,//base
        0b1110,
        0b1011
    };

    //4way
    public static HashSet<int> corridor4Way = new HashSet<int>
    {
        0b1111 //base
    };
}
