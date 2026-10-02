using NUnit.Framework;
using UnityEngine;

public class DestructiblePropRegistryTests
{
    private GameObject registryObject;
    private DestructiblePropRegistry registry;
    private GameObject propObject;
    private SmashableProp prop;

    [SetUp]
    public void SetUp()
    {
        registryObject = new GameObject("DestructiblePropRegistry Test");
        registry = registryObject.AddComponent<DestructiblePropRegistry>();

        propObject = new GameObject("SmashableProp Test");
        prop = propObject.AddComponent<SmashableProp>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(propObject);
        Object.DestroyImmediate(registryObject);
    }

    [Test]
    public void RegisteringSamePropTwiceKeepsOneEntry()
    {
        registry.RegisterProp(prop);
        registry.RegisterProp(prop);

        Assert.That(registry.GetAllProps().Count, Is.EqualTo(1));
    }

    [Test]
    public void BreakIsOnlyTriggeredOnceForSingleHit()
    {
        int destroyedCount = 0;
        registry.PropDestroyed += (brokenProp, hitInfo) => destroyedCount++;

        registry.RegisterProp(prop);
        prop.ApplyDamage(100f, DestructibleDamageType.Kick, Vector3.zero, Vector3.up, propObject);
        prop.ApplyDamage(100f, DestructibleDamageType.Kick, Vector3.zero, Vector3.up, propObject);

        Assert.That(destroyedCount, Is.EqualTo(1));
    }
}
