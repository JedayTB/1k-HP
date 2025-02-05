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
  public static float ExponentialDecay(float a, float b, float decay, float deltaTime)
  {
    return b + (a - b) * Mathf.Exp(-decay * deltaTime);
  }
  public static Vector3 VexExpoDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
  {
    float vx = ExponentialDecay(a.x, b.x, decay, deltaTime);
    float vy = ExponentialDecay(a.y, b.y, decay, deltaTime);
    float vz = ExponentialDecay(a.z, b.z, decay, deltaTime);

    return new Vector3(vx, vy, vz);
  }
  public static float easeInOutQuad(float x)
  {
    return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
  }


}
