using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
// 소음 발생 시스템

public static class NoiseSystem
{
    // pos: 소리 위치, radius: 반경, distractTime: 지속시간
    public static event Action<Vector3, float, float> OnNoise;

    public static void Emit(Vector3 pos, float radius, float distractTime)
    {
        OnNoise?.Invoke(pos, radius, distractTime);
    }
}
