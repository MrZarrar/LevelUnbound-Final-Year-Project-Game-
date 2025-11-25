using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HealthSystemPlayModeTests
{
    private GameObject _gameObject;
    private HealthSystem _healthSystem;
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject("HealthSystem_PlayMode");
        _gameObject.AddComponent<Animator>();
        _healthSystem = _gameObject.AddComponent<HealthSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_gameObject);
    }

    [UnityTest]
    public IEnumerator FullHeal_RestoresAllVitals()
    {
        _healthSystem.InitializeVitals(150f, 90f, 60f, refillVitals: true);

        SetField("currentHealth", 25f);
        SetField("currentMana", 10f);
        SetField("currentStamina", 5f);

        _healthSystem.FullHeal();
        yield return null;

        Assert.AreEqual(150f, GetField<float>("currentHealth"));
        Assert.AreEqual(90f, GetField<float>("currentMana"));
        Assert.AreEqual(60f, GetField<float>("currentStamina"));
    }

    [UnityTest]
    public IEnumerator TryUseMana_DeductsAmount_WhenSufficient()
    {
        _healthSystem.InitializeVitals(100f, 30f, 40f, refillVitals: true);

        bool result = _healthSystem.TryUseMana(15f);
        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(15f, GetField<float>("currentMana"));
    }

    private T GetField<T>(string field)
    {
        return (T)typeof(HealthSystem).GetField(field, Flags)!.GetValue(_healthSystem);
    }

    private void SetField(string field, float value)
    {
        typeof(HealthSystem).GetField(field, Flags)!.SetValue(_healthSystem, value);
    }
}

