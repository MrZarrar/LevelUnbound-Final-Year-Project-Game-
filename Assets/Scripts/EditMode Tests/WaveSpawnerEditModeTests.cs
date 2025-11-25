using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditModeTests
{
    public class WaveSpawnerEditModeTests
    {
        private GameObject _spawnerGO;
        private WaveSpawner _spawner;
        private TextMeshProUGUI _enemiesLeft;
        private TextMeshProUGUI _waveCounter;
        private TextMeshProUGUI _waveAnnouncement;

        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            _spawnerGO = new GameObject("WaveSpawner_EditMode");
            _spawner = _spawnerGO.AddComponent<WaveSpawner>();

            _enemiesLeft = new GameObject("EnemiesLeft").AddComponent<TextMeshProUGUI>();
            _waveCounter = new GameObject("WaveCounter").AddComponent<TextMeshProUGUI>();
            _waveAnnouncement = new GameObject("WaveAnnouncement").AddComponent<TextMeshProUGUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_spawnerGO);
            Object.DestroyImmediate(_enemiesLeft.gameObject);
            Object.DestroyImmediate(_waveCounter.gameObject);
            Object.DestroyImmediate(_waveAnnouncement.gameObject);
        }

        [Test]
        public void UpdateUI_ShowsEnemiesAndWavesLeft()
        {
            var waves = new[]
            {
                ScriptableObject.CreateInstance<Wave>(),
                ScriptableObject.CreateInstance<Wave>(),
                ScriptableObject.CreateInstance<Wave>()
            };

            SetField("waves", waves);
            SetField("currentWaveIndex", 0);
            SetField("enemiesLeftInWave", 4);
            SetField("bossHasSpawned", false);

            SetField("enemiesLeftText", _enemiesLeft);
            SetField("waveCounterText", _waveCounter);

            InvokePrivate("UpdateUI");

            Assert.AreEqual("Enemies Left: 4", _enemiesLeft.text);
            Assert.AreEqual("Waves until Boss: 3", _waveCounter.text);

            foreach (var wave in waves)
            {
                Object.DestroyImmediate(wave);
            }
        }

        [Test]
        public void UpdateUIForBossWave_DisplaysBossLabel()
        {
            SetField("enemiesLeftText", _enemiesLeft);
            SetField("waveCounterText", _waveCounter);
            SetField("enemiesLeftInWave", 2);

            InvokePrivate("UpdateUIForBossWave");

            Assert.AreEqual("Enemies Left: 2", _enemiesLeft.text);
            Assert.AreEqual("!! BOSS WAVE !!", _waveCounter.text);
        }

        [Test]
        public void SpawnExitPortal_InstantiatesPrefabAtSpawnPoint()
        {
            var spawnPointGO = new GameObject("PortalSpawnPoint");
            spawnPointGO.transform.position = new Vector3(1f, 2f, 3f);

            var portalPrefab = new GameObject("ExitPortal");

            SetField("exitPortalPrefab", portalPrefab);
            SetField("exitPortalSpawnPoint", spawnPointGO.transform);
            SetField("portalSpawnHeightOffset", 1.5f);

            InvokePrivate("SpawnExitPortal");

            var spawnedPortal = GameObject.Find("ExitPortal(Clone)");
            Assert.IsNotNull(spawnedPortal, "SpawnExitPortal should instantiate the portal prefab.");
            Assert.AreEqual(new Vector3(1f, 3.5f, 3f), spawnedPortal.transform.position);

            Object.DestroyImmediate(spawnedPortal);
            Object.DestroyImmediate(spawnPointGO);
            Object.DestroyImmediate(portalPrefab);
        }

        private void SetField(string fieldName, object value)
        {
            typeof(WaveSpawner).GetField(fieldName, Flags)!.SetValue(_spawner, value);
        }

        private void InvokePrivate(string methodName)
        {
            typeof(WaveSpawner).GetMethod(methodName, Flags)!.Invoke(_spawner, null);
        }
    }
}

