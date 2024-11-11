using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LerpAndEasings
{ 

    /// <summary>
    /// 
    /// Exponential decay is constant.
    /// A good range is 1 - 25, slow to fast
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="decay">"speed" of decay</param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public static float ExponentialDecay(float a, float b,float decay, float deltaTime)
    {
        return b + (a-b) * Mathf.Exp(-decay * deltaTime);
    }
    

}
