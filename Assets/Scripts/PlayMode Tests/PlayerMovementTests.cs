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

    [UnityTest]
    public IEnumerator Player_PositionChangesAfterManualTranslation()
    {
        GameObject player = new GameObject("MoverPlayer");
        var controller = player.AddComponent<CharacterController>();

        Vector3 initialPosition = player.transform.position;
        player.transform.Translate(Vector3.forward * 2f);

        yield return null;

        Assert.Greater(player.transform.position.z, initialPosition.z, "Player should move forward after translation.");

        Object.Destroy(player);
    }
}