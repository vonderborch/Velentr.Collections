using System.Collections;
using Velentr.Collections.LockFree;

namespace Velentr.Collections.Test.LockFree;

[TestFixture]
public class TestLockFreePriorityQueue
{
    [Test]
    public void Constructor_Default_CreatesEmptyQueue()
    {
        // Arrange & Act
        LockFreePriorityQueue<int> queue = new();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
        Assert.That(queue.Peek(), Is.EqualTo(default(int)));
    }

    [Test]
    public void Constructor_WithPriorityLevels_CreatesCorrectQueue()
    {
        // Arrange & Act
        LockFreePriorityQueue<int> queue = new(10);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));

        // Test we can use priorities 0-9
        queue.Enqueue(1, 0);
        queue.Enqueue(2, 9);

        // Test we can't use priority 10 (out of range)
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Enqueue(3, 10));
    }

    [Test]
    public void Constructor_InvalidPriorityLevels_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new LockFreePriorityQueue<int>(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LockFreePriorityQueue<int>(65));
    }

    [Test]
    public void Constructor_WithSingleItem_CreatesQueueWithOneElement()
    {
        // Arrange & Act
        LockFreePriorityQueue<int> queue = new(42, 5);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(1));
        Assert.That(queue.Peek(), Is.EqualTo(42));
    }

    [Test]
    public void Constructor_WithCollection_CreatesQueueWithAllElements()
    {
        // Arrange
        List<int> collection = new() { 1, 2, 3, 4, 5 };

        // Act
        LockFreePriorityQueue<int> queue = new(collection, 2);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(5));
        Assert.That(queue.Peek(), Is.EqualTo(1));
    }

    [Test]
    public void Enqueue_AddingElements_IncrementsCount()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();

        // Act
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(3));
    }

    [Test]
    public void Enqueue_InvalidPriority_ThrowsException()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new(5);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Enqueue(10, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Enqueue(10, 5));
    }

    [Test]
    public void Dequeue_RemovesLowestPriorityFirst()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);
        queue.Enqueue(40, 0);

        // Act & Assert
        Assert.That(queue.Dequeue(), Is.EqualTo(30));
        Assert.That(queue.Dequeue(), Is.EqualTo(40));
        Assert.That(queue.Dequeue(), Is.EqualTo(20));
        Assert.That(queue.Dequeue(), Is.EqualTo(10));
        Assert.That(queue.Count, Is.EqualTo(0));
    }

    [Test]
    public void Dequeue_EmptyQueue_ReturnsDefaultValue()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();

        // Act
        var result = queue.Dequeue();

        // Assert
        Assert.That(result, Is.EqualTo(default(int)));
    }

    [Test]
    public void Dequeue_WithOutParameter_ReturnsFalseForEmptyQueue()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();

        // Act
        var success = queue.Dequeue(out var value);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(value, Is.EqualTo(default(int)));
    }

    [Test]
    public void Dequeue_WithOutParameter_ReturnsTrueAndValueForNonEmptyQueue()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(42, 0);

        // Act
        var success = queue.Dequeue(out var value);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void Peek_EmptyQueue_ReturnsDefaultValue()
    {
        // Arrange
        LockFreePriorityQueue<string> queue = new();

        // Act
        var result = queue.Peek();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Peek_WithOutParameter_ReturnsFalseForEmptyQueue()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();

        // Act
        var success = queue.Peek(out var value);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(value, Is.EqualTo(default(int)));
    }

    [Test]
    public void Peek_WithOutParameter_ReturnsTrueAndValueForNonEmptyQueue()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(42, 0);

        // Act
        var success = queue.Peek(out var value);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void Peek_ShowsLowestPriorityWithoutRemoving()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Act & Assert
        Assert.That(queue.Peek(), Is.EqualTo(30)); // Lowest priority first
        Assert.That(queue.Count, Is.EqualTo(3)); // Count unchanged
        Assert.That(queue.Peek(), Is.EqualTo(30)); // Still the same value
    }

    [Test]
    public void Clear_RemovesAllElements()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Act
        queue.Clear();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
        Assert.That(queue.Peek(), Is.EqualTo(default(int)));

        // Verify we can add new items after clearing
        queue.Enqueue(40, 1);
        Assert.That(queue.Count, Is.EqualTo(1));
        Assert.That(queue.Peek(), Is.EqualTo(40));
    }

    [Test]
    public void Contains_FindsExistingItems()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Act & Assert
        Assert.That(queue.Contains(10), Is.True);
        Assert.That(queue.Contains(20), Is.True);
        Assert.That(queue.Contains(30), Is.True);
        Assert.That(queue.Contains(40), Is.False);
    }

    [Test]
    public void Contains_HandlesNullValues()
    {
        // Arrange
        LockFreePriorityQueue<string> queue = new();
        queue.Enqueue("test", 0);
        queue.Enqueue(null, 1);

        // Act & Assert
        Assert.That(queue.Contains(null), Is.True);
        Assert.That(queue.Contains("test"), Is.True);
        Assert.That(queue.Contains("other"), Is.False);
    }

    [Test]
    public void IsSynchronized_AlwaysReturnsTrue()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();

        // Act & Assert
        Assert.That(((ICollection)queue).IsSynchronized, Is.True);
    }

    [Test]
    public void SyncRoot_ReturnsNonNullObject()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();

        // Act
        var syncRoot = ((ICollection)queue).SyncRoot;

        // Assert
        Assert.That(syncRoot, Is.Not.Null);
    }

    [Test]
    public void ToList_ReturnsListWithAllElements()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Act
        List<int> list = queue.ToList;

        // Assert
        Assert.That(list, Is.Not.Null);
        Assert.That(list, Has.Count.EqualTo(3));

        // Items should be in priority order (lowest first)
        Assert.That(list[0], Is.EqualTo(30));
        Assert.That(list[1], Is.EqualTo(20));
        Assert.That(list[2], Is.EqualTo(10));
    }

    [Test]
    public void GetEnumerator_EnumeratesElementsInPriorityOrder()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Act
        List<int> result = new();
        foreach (var item in queue)
        {
            result.Add(item);
        }

        // Assert - lowest priority (0) first
        Assert.That(result, Is.EqualTo(new List<int> { 30, 20, 10 }));
    }

    [Test]
    public void GetEnumerator_ModifyingDuringEnumeration_ThrowsException()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var item in queue)
            {
                queue.Enqueue(40, 0);
            }
        });
    }

    [Test]
    public void CopyTo_CopiesElementsToArray()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 2);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 0);
        var array = new int[5];

        // Act
        ((ICollection)queue).CopyTo(array, 1);

        // Assert - items in priority order (lowest first)
        Assert.That(array, Is.EqualTo(new[] { 0, 30, 20, 10, 0 }));
    }

    [Test]
    public void CopyTo_NullArray_ThrowsArgumentNullException()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 0);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ICollection)queue).CopyTo(null, 0));
    }

    [Test]
    public void CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 0);
        var array = new int[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ICollection)queue).CopyTo(array, -1));
    }

    [Test]
    public void CopyTo_IndexPlusCountExceedsArrayBounds_ThrowsArgumentException()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 0);
        queue.Enqueue(20, 1);
        queue.Enqueue(30, 2);
        var array = new int[2];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ((ICollection)queue).CopyTo(array, 0));
    }

    [Test]
    public void CopyTo_MultidimensionalArray_ThrowsArgumentException()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new();
        queue.Enqueue(10, 0);
        var array = new int[2, 2];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ((ICollection)queue).CopyTo(array, 0));
    }

    [Test]
    [Timeout(10000)]
    public void MultithreadedOperations_EnsuresThreadSafety()
    {
        // Arrange
        const int operationsPerThread = 1000;
        const int threadCount = 4;
        const int priorityLevels = 5;
        LockFreePriorityQueue<int> queue = new(priorityLevels);
        CountdownEvent countdown = new(threadCount);
        Random random = new();

        // Act
        // Create producer threads that add items with different priorities
        for (var t = 0; t < threadCount / 2; t++)
        {
            Task.Run(() =>
            {
                for (var i = 0; i < operationsPerThread; i++)
                {
                    var priority = random.Next(0, priorityLevels);
                    queue.Enqueue(i, priority);
                }

                countdown.Signal();
            });
        }

        // Create consumer threads that remove items
        for (var t = 0; t < threadCount / 2; t++)
        {
            Task.Run(() =>
            {
                var dequeued = 0;
                while (dequeued < operationsPerThread)
                {
                    if (queue.Dequeue(out _))
                    {
                        dequeued++;
                    }
                    else
                    {
                        Thread.Yield();
                    }
                }

                countdown.Signal();
            });
        }

        // Wait for all threads to finish
        countdown.Wait();

        // Assert - all items should be processed
        Assert.That(queue.Count, Is.EqualTo(0));
    }

    [Test]
    public void MultiplePriorities_DequeuesInCorrectOrder()
    {
        // Arrange
        LockFreePriorityQueue<int> queue = new(5);

        // Add items with mixed priorities
        queue.Enqueue(1, 3);
        queue.Enqueue(2, 1);
        queue.Enqueue(3, 4);
        queue.Enqueue(4, 0);
        queue.Enqueue(5, 2);
        queue.Enqueue(6, 0); // Same priority as 4

        // Act & Assert - Should come out in priority order
        Assert.That(queue.Dequeue(), Is.EqualTo(4)); // Priority 0
        Assert.That(queue.Dequeue(), Is.EqualTo(6)); // Priority 0
        Assert.That(queue.Dequeue(), Is.EqualTo(2)); // Priority 1
        Assert.That(queue.Dequeue(), Is.EqualTo(5)); // Priority 2
        Assert.That(queue.Dequeue(), Is.EqualTo(1)); // Priority 3
        Assert.That(queue.Dequeue(), Is.EqualTo(3)); // Priority 4
        Assert.That(queue.Count, Is.EqualTo(0));
    }
}
