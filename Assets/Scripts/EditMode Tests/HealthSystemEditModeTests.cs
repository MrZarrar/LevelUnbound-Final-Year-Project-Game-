using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    public class HealthSystemEditModeTests
    {
        private GameObject _gameObject;
        private HealthSystem _healthSystem;

        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("HealthSystem_EditMode");
            _gameObject.AddComponent<Animator>();
            _healthSystem = _gameObject.AddComponent<HealthSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void InitializeVitals_WithRefill_SetsMaxAndCurrentValues()
        {
            _healthSystem.InitializeVitals(120f, 80f, 60f, refillVitals: true);

            Assert.AreEqual(120f, GetField<float>("maxHealth"));
            Assert.AreEqual(80f, GetField<float>("maxMana"));
            Assert.AreEqual(60f, GetField<float>("maxStamina"));

            Assert.AreEqual(120f, GetField<float>("currentHealth"));
            Assert.AreEqual(80f, GetField<float>("currentMana"));
            Assert.AreEqual(60f, GetField<float>("currentStamina"));
        }

        [Test]
        public void InitializeVitals_WithoutRefill_PreservesCurrentValues()
        {
            _healthSystem.InitializeVitals(100f, 40f, 30f, refillVitals: true);
            SetField("currentHealth", 25f);
            SetField("currentMana", 5f);
            SetField("currentStamina", 10f);

            _healthSystem.InitializeVitals(120f, 50f, 60f, refillVitals: false);

            Assert.AreEqual(25f, GetField<float>("currentHealth"));
            Assert.AreEqual(5f, GetField<float>("currentMana"));
            Assert.AreEqual(10f, GetField<float>("currentStamina"));
        }

        [Test]
        public void RegenerateMana_ClampsToMaximum()
        {
            _healthSystem.InitializeVitals(80f, 20f, 40f, refillVitals: true);
            _healthSystem.TryUseMana(10f);

            _healthSystem.RegenerateMana(50f);

            Assert.AreEqual(20f, GetField<float>("currentMana"));
        }

        [Test]
        public void TryUseStamina_ReturnsFalse_WhenNotEnough()
        {
            _healthSystem.InitializeVitals(60f, 30f, 5f, refillVitals: true);

            bool result = _healthSystem.TryUseStamina(10f);

            Assert.IsFalse(result);
            Assert.AreEqual(5f, GetField<float>("currentStamina"));
        }

        private T GetField<T>(string fieldName)
        {
            return (T)typeof(HealthSystem).GetField(fieldName, Flags)!.GetValue(_healthSystem);
        }

        private void SetField(string fieldName, float value)
        {
            typeof(HealthSystem).GetField(fieldName, Flags)!.SetValue(_healthSystem, value);
        }
    }
}

