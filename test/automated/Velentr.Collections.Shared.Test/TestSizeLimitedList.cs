using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections.Test;

[TestFixture]
public class TestSizeLimitedList
{
    [Test]
    public void Test_AddingElementsWithinLimit()
    {
        SizeLimitedList<int> list = new(5);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(3));
    }

    [Test]
    public void Test_AddingElementsExceedingLimit()
    {
        SizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(2));
        Assert.That(list[1], Is.EqualTo(3));
        Assert.That(list[2], Is.EqualTo(4));
    }

    [Test]
    public void Test_AddingElementsExceedingLimit_PopNewest()
    {
        SizeLimitedList<int> list = new(3, SizeLimitedCollectionFullAction.PopNewestItem);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(4));
    }

    [Test]
    public void Test_AddingElementsExceedingLimit_PopNewest_WithReturn()
    {
        SizeLimitedList<int> list = new(3, SizeLimitedCollectionFullAction.PopNewestItem);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        var poppedItem = list.AddAndReturn(4);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(4));
        Assert.That(poppedItem, Is.EqualTo(3));
    }

    [Test]
    public void Test_RemovingElements()
    {
        SizeLimitedList<int> list = new(5);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Remove(2);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Contains(2), Is.False);
    }

    [Test]
    public void Test_EmptyListBehavior()
    {
        SizeLimitedList<int> list = new(5);

        Assert.That(list.Count, Is.EqualTo(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = list[0]);
    }

    [Test]
    public void Test_NullValues()
    {
        SizeLimitedList<string> list = new(3);
        list.Add(null);
        list.Add("test");

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list[0], Is.Null);
        Assert.That(list[1], Is.EqualTo("test"));
    }

    [Test]
    public void Test_MaxSizeZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SizeLimitedList<int>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SizeLimitedList<int>(0));
    }

    [Test]
    public void Test_ClearList()
    {
        SizeLimitedList<int> list = new(5);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Clear();

        Assert.That(list.Count, Is.EqualTo(0));
    }

    [Test]
    public void Test_OverwritingOldestElement()
    {
        SizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list.Contains(1), Is.False);
        Assert.That(list[0], Is.EqualTo(2));
    }
}
