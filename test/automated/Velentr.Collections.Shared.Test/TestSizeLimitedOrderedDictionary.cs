using System.Collections.Immutable;
using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections.Test;

public class TestSizeLimitedOrderedDictionary
{
    [Test]
    public void Test_Add_Items_Exceeding_Limit_PopNewest()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(3, SizeLimitedCollectionFullAction.PopNewestItem);
        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");
        dictionary.Add(3, "Three");
        dictionary.Add(4, "Four");

        Assert.That(dictionary.Count, Is.EqualTo(3));
        Assert.That(dictionary.ContainsKey(3), Is.False);
        Assert.That(dictionary[0], Is.EqualTo("One"));
        Assert.That(dictionary[1], Is.EqualTo("Two"));
        Assert.That(dictionary[2], Is.EqualTo("Four"));
    }

    [Test]
    public void Test_Add_Items_Exceeding_Limit_PopOldest()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(3);
        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");
        dictionary.Add(3, "Three");
        dictionary.Add(4, "Four");

        Assert.That(dictionary.Count, Is.EqualTo(3));
        Assert.That(dictionary.ContainsKey(1), Is.False);
        Assert.That(dictionary[0], Is.EqualTo("Two"));
        Assert.That(dictionary[1], Is.EqualTo("Three"));
        Assert.That(dictionary[2], Is.EqualTo("Four"));
    }

    [Test]
    public void Test_Add_Items_Exceeding_Limit_PopOldestAndReturn()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(3);
        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");
        dictionary.Add(3, "Three");
        var poppedItem = dictionary.AddAndReturn(4, "Four");

        Assert.That(dictionary.Count, Is.EqualTo(3));
        Assert.That(dictionary.ContainsKey(1), Is.False);
        Assert.That(dictionary[0], Is.EqualTo("Two"));
        Assert.That(dictionary[1], Is.EqualTo("Three"));
        Assert.That(dictionary[2], Is.EqualTo("Four"));
        Assert.That(poppedItem, Is.EqualTo("One"));
    }

    [Test]
    public void Test_Add_Items_Under_Limit()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(3);
        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");

        Assert.That(dictionary.Count, Is.EqualTo(2));
        Assert.That(dictionary[0], Is.EqualTo("One"));
        Assert.That(dictionary[1], Is.EqualTo("Two"));
    }

    [Test]
    public void Test_ImmutableDictionary_Constructor()
    {
        ImmutableSortedDictionary<int, string> source = ImmutableSortedDictionary<int, string>.Empty.Add(1, "One")
            .Add(2, "Two");
        SizeLimitedOrderedDictionary<int, string> dictionary = new(source, 3);

        Assert.That(dictionary.Count, Is.EqualTo(2));
        Assert.That(dictionary[0], Is.EqualTo("One"));
        Assert.That(dictionary[1], Is.EqualTo("Two"));
    }

    [Test]
    public void Test_Overflow_Exception()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(2, (SizeLimitedCollectionFullAction)999);

        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");

        Assert.That(() => dictionary.Add(3, "Three"), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Test_Remove_Item()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(3);
        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");

        var removed = dictionary.Remove(1);

        Assert.That(removed, Is.True);
        Assert.That(dictionary.ContainsKey(1), Is.False);
        Assert.That(dictionary.Count, Is.EqualTo(1));
    }

    [Test]
    public void Test_TryGetValue()
    {
        SizeLimitedOrderedDictionary<int, string> dictionary = new(3);
        dictionary.Add(1, "One");

        var result = dictionary.TryGetValue(1, out var value);

        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo("One"));
    }
}
