using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class GameModeManagerTests
{
    private Type managerType;
    private GameObject managerObject;
    private Component manager;

    [SetUp]
    public void SetUp()
    {
        managerType = Type.GetType("GameModeManager, Assembly-CSharp");
        Assert.That(managerType, Is.Not.Null, "GameModeManager must be present in Assembly-CSharp.");
        managerType.GetField("Instance").SetValue(null, null);

        managerObject = new GameObject("GameModeManager test");
        manager = managerObject.AddComponent(managerType);
    }

    [TearDown]
    public void TearDown()
    {
        managerType.GetField("Instance").SetValue(null, null);
        UnityEngine.Object.DestroyImmediate(managerObject);
    }

    [Test]
    public void ResetMatchRestoresScoreTimerAndObjectives()
    {
        managerType.GetField("matchTimer").SetValue(manager, 0f);
        managerType.GetField("totalCatDestructionPoints").SetValue(manager, 450);
        managerType.GetField("activeMode").SetValue(manager, Enum.Parse(managerType.Assembly.GetType("GameModeType"), "CoOpVsAI"));
        managerType.GetMethod("OnItemDestroyed").Invoke(manager, new object[] { "Vase", 150 });

        managerType.GetMethod("ResetMatch").Invoke(manager, null);

        Assert.That(managerType.GetField("matchTimer").GetValue(manager), Is.EqualTo(180f));
        Assert.That(managerType.GetField("totalCatDestructionPoints").GetValue(manager), Is.EqualTo(0));
        Assert.That(((IList)managerType.GetField("destroyedItems", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager)).Count, Is.EqualTo(0));
        Assert.That(managerType.GetField("isMatchActive", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager), Is.EqualTo(true));
    }

    [Test]
    public void DestroyedItemAwardsItsPoints()
    {
        managerType.GetMethod("OnItemDestroyed").Invoke(manager, new object[] { "Vase", 150 });

        Assert.That(managerType.GetField("totalCatDestructionPoints").GetValue(manager), Is.EqualTo(150));
    }
}