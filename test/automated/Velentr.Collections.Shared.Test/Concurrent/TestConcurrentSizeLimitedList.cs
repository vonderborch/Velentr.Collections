using System.Collections.Concurrent;
using System.Collections.Immutable;
using Velentr.Collections.CollectionFullActions;
using Velentr.Collections.Concurrent;
using Assert = NUnit.Framework.Assert;

namespace Velentr.Collections.Test.Concurrent;

public class TestConcurrentSizeLimitedList
{
    [Test]
    public void Test_Add_Item()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(3));
    }

    [Test]
    public void Test_Add_Item_Exceeding_MaxSize()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
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
    public void Test_AddAndReturn_Item()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        var poppedItem = list.AddAndReturn(4);

        Assert.That(poppedItem, Is.EqualTo(1));
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(2));
        Assert.That(list[1], Is.EqualTo(3));
        Assert.That(list[2], Is.EqualTo(4));
    }

    [Test]
    public void Test_AsImmutable()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        ImmutableList<int> immutableList = list.AsImmutable();

        Assert.That(immutableList.Count, Is.EqualTo(3));
        Assert.That(immutableList[0], Is.EqualTo(1));
        Assert.That(immutableList[1], Is.EqualTo(2));
        Assert.That(immutableList[2], Is.EqualTo(3));
    }

    [Test]
    public void Test_ChangeMaxSize()
    {
        ConcurrentSizeLimitedList<int> list = new(3, SizeLimitedCollectionFullAction.PopNewestItem);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        List<int> removedItems = list.ChangeMaxSize(2);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(removedItems.Count, Is.EqualTo(1));
        Assert.That(removedItems[0], Is.EqualTo(3));
    }

    [Test]
    public void Test_Clear()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.Clear();

        Assert.That(list, Is.Empty);
    }

    [Test]
    public void Test_Contains()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.That(list.Contains(2), Is.True);
        Assert.That(list.Contains(4), Is.False);
    }

    [Test]
    public void Test_IndexOf()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        Assert.That(list.IndexOf(2), Is.EqualTo(1));
        Assert.That(list.IndexOf(4), Is.EqualTo(-1));
    }

    [Test]
    public void Test_Insert()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(3);

        list.Insert(1, 2);

        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(2));
        Assert.That(list[2], Is.EqualTo(3));
    }

    [Test]
    public void Test_Remove()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        var removed = list.Remove(2);

        Assert.That(removed, Is.True);
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(3));
    }

    [Test]
    public void Test_RemoveAt()
    {
        ConcurrentSizeLimitedList<int> list = new(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);

        list.RemoveAt(1);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list[0], Is.EqualTo(1));
        Assert.That(list[1], Is.EqualTo(3));
    }

    [Test]
    public void Test_ThreadSafety()
    {
        // Create a concurrent size-limited list with larger capacity to observe thread interactions
        ConcurrentSizeLimitedList<int> list = new(1000);

        // Number of threads to use for testing
        var threadCount = 10;
        var itemsPerThread = 200;

        // Use a CountdownEvent to ensure all threads start at approximately the same time
        using CountdownEvent startSignal = new(threadCount);
        using CountdownEvent completedSignal = new(threadCount);

        // Track exceptions across threads
        ConcurrentQueue<Exception> exceptions = new();

        // Create and start threads
        for (var threadId = 0; threadId < threadCount; threadId++)
        {
            var id = threadId; // Capture for lambda
            new Thread(() =>
            {
                try
                {
                    // Signal thread is ready and wait for all threads to be ready
                    startSignal.Signal();
                    startSignal.Wait();

                    // Add items to the list - each thread adds a range of unique numbers
                    for (var i = 0; i < itemsPerThread; i++)
                    {
                        list.Add(id * itemsPerThread + i);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
                finally
                {
                    completedSignal.Signal();
                }
            }).Start();
        }

        // Wait for all threads to complete
        completedSignal.Wait();

        // Check if any exceptions were thrown
        Assert.That(exceptions, Is.Empty, "Exceptions were thrown during concurrent operations");

        // Verify list count is correct - should contain all added items up to the max size
        var expectedCount = Math.Min(threadCount * itemsPerThread, list.MaxSize);
        Assert.That(list.Count, Is.EqualTo(expectedCount));

        // Get an immutable copy to inspect the list contents
        ImmutableList<int> immutableList = list.AsImmutable();

        // Ensure no duplicates exist in the list
        var distinctCount = immutableList.Distinct().Count();
        Assert.That(distinctCount, Is.EqualTo(immutableList.Count), "List contains duplicate items");

        // Additional verification that all items are in the valid range
        foreach (var item in immutableList)
        {
            Assert.That(item, Is.GreaterThanOrEqualTo(0));
            Assert.That(item, Is.LessThan(threadCount * itemsPerThread));
        }
    }
}
