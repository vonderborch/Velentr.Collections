using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Velentr.Collections.Test;

[TestFixture]
public class TestBiDirectionalDictionary
{
    private BiDirectionalDictionary<int, string> _dictionary;

    [SetUp]
    public void SetUp()
    {
        _dictionary = new BiDirectionalDictionary<int, string>();
    }

    [Test]
    public void TestAddAndRetrieve()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary[1], Is.EqualTo("One"));
        Assert.That(_dictionary.GetKey("One"), Is.EqualTo(1));
    }

    [Test]
    public void TestDuplicateKeyThrowsException()
    {
        _dictionary.Add(1, "One");
        Assert.That(() => _dictionary.Add(1, "Duplicate"), Throws.ArgumentException);
    }

    [Test]
    public void TestDuplicateValueThrowsException()
    {
        _dictionary.Add(1, "One");
        Assert.That(() => _dictionary.Add(2, "One"), Throws.ArgumentException);
    }

    [Test]
    public void TestRemoveByKey()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary.Remove(1), Is.True);
        Assert.That(_dictionary.ContainsKey(1), Is.False);
        Assert.That(_dictionary.ContainsValue("One"), Is.False);
    }

    [Test]
    public void TestRemoveByValue()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary.RemoveValue("One"), Is.True);
        Assert.That(_dictionary.ContainsKey(1), Is.False);
        Assert.That(_dictionary.ContainsValue("One"), Is.False);
    }

    [Test]
    public void TestTryGetValue()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary.TryGetValue(1, out var value), Is.True);
        Assert.That(value, Is.EqualTo("One"));
    }

    [Test]
    public void TestTryGetKey()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary.TryGetKey("One", out var key), Is.True);
        Assert.That(key, Is.EqualTo(1));
    }

    [Test]
    public void TestClear()
    {
        _dictionary.Add(1, "One");
        _dictionary.Add(2, "Two");
        _dictionary.Clear();
        Assert.That(_dictionary.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestContainsKey()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary.ContainsKey(1), Is.True);
        Assert.That(_dictionary.ContainsKey(2), Is.False);
    }

    [Test]
    public void TestContainsValue()
    {
        _dictionary.Add(1, "One");
        Assert.That(_dictionary.ContainsValue("One"), Is.True);
        Assert.That(_dictionary.ContainsValue("Two"), Is.False);
    }

    [Test]
    public void TestIndexerSet()
    {
        _dictionary[1] = "One";
        Assert.That(_dictionary[1], Is.EqualTo("One"));
        Assert.That(_dictionary.GetKey("One"), Is.EqualTo(1));
    }

    [Test]
    public void TestIndexerUpdate()
    {
        _dictionary.Add(1, "One");
        _dictionary[1] = "Updated";
        Assert.That(_dictionary[1], Is.EqualTo("Updated"));
        Assert.That(_dictionary.GetKey("Updated"), Is.EqualTo(1));
        Assert.That(_dictionary.ContainsValue("One"), Is.False);
    }

    [Test]
    public void TestKeyNotFoundThrowsException()
    {
        Assert.That(() => _ = _dictionary[1], Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void TestValueNotFoundThrowsException()
    {
        Assert.That(() => _dictionary.GetKey("NonExistent"), Throws.TypeOf<KeyNotFoundException>());
    }
}
