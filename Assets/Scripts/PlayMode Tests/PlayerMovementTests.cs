using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// The class MUST be public
public class PlayerMovementTests
{
    [UnityTest]
    public IEnumerator Player_CanInstantiate()
    {
        GameObject player = new GameObject("TestPlayer");
        yield return null;
        Assert.IsNotNull(player);
        Object.Destroy(player);
    }
}