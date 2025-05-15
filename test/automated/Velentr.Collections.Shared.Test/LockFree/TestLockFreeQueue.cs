using System.Collections;
using Velentr.Collections.LockFree;

namespace Velentr.Collections.Test.LockFree;

[TestFixture]
public class TestLockFreeQueue
{
    [Test]
    public void Constructor_Default_CreatesEmptyQueue()
    {
        // Arrange & Act
        LockFreeQueue<int> queue = new();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
        Assert.That(queue.Peek(), Is.EqualTo(default(int)));
    }

    [Test]
    public void Constructor_SingleValue_CreatesQueueWithOneElement()
    {
        // Arrange & Act
        LockFreeQueue<int> queue = new(42);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(1));
        Assert.That(queue.Peek(), Is.EqualTo(42));
    }

    [Test]
    public void Constructor_Collection_CreatesQueueWithAllElements()
    {
        // Arrange
        List<int> collection = new() { 1, 2, 3, 4, 5 };

        // Act
        LockFreeQueue<int> queue = new(collection);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(5));
        Assert.That(queue.Peek(), Is.EqualTo(1));
    }

    [Test]
    public void Enqueue_AddingElements_IncrementsCount()
    {
        // Arrange
        LockFreeQueue<int> queue = new();

        // Act
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(3));
    }

    [Test]
    public void Dequeue_RemovingElements_DecrementsCount()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);

        // Act
        var first = queue.Dequeue();
        var second = queue.Dequeue();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(1));
        Assert.That(first, Is.EqualTo(10));
        Assert.That(second, Is.EqualTo(20));
    }

    [Test]
    public void Dequeue_EmptyQueue_ReturnsDefaultValue()
    {
        // Arrange
        LockFreeQueue<int> queue = new();

        // Act
        var result = queue.Dequeue();

        // Assert
        Assert.That(result, Is.EqualTo(default(int)));
    }

    [Test]
    public void Dequeue_WithOutParameter_ReturnsFalseForEmptyQueue()
    {
        // Arrange
        LockFreeQueue<int> queue = new();

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
        LockFreeQueue<int> queue = new();
        queue.Enqueue(42);

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
        LockFreeQueue<string> queue = new();

        // Act
        var result = queue.Peek();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Peek_NonEmptyQueue_ReturnsFirstValueWithoutRemoving()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);

        // Act
        var result = queue.Peek();

        // Assert
        Assert.That(result, Is.EqualTo(10));
        Assert.That(queue.Count, Is.EqualTo(2));
    }

    [Test]
    public void Clear_RemovesAllElements()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);

        // Act
        queue.Clear();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
        Assert.That(queue.Peek(), Is.EqualTo(default(int)));
    }

    [Test]
    public void IsSynchronized_AlwaysReturnsTrue()
    {
        // Arrange
        LockFreeQueue<int> queue = new();

        // Act & Assert
        Assert.That(((ICollection)queue).IsSynchronized, Is.True);
    }

    [Test]
    public void SyncRoot_ReturnsNonNullObject()
    {
        // Arrange
        LockFreeQueue<int> queue = new();

        // Act
        var syncRoot = ((ICollection)queue).SyncRoot;

        // Assert
        Assert.That(syncRoot, Is.Not.Null);
    }

    [Test]
    public void ToList_ReturnsListWithAllElements()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);

        // Act
        List<int> list = queue.ToList;

        // Assert
        Assert.That(list, Is.Not.Null);
        Assert.That(list, Has.Count.EqualTo(3));
        Assert.That(list[0], Is.EqualTo(10));
        Assert.That(list[1], Is.EqualTo(20));
        Assert.That(list[2], Is.EqualTo(30));
    }

    [Test]
    public void GetEnumerator_EnumeratesElementsInCorrectOrder()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);

        // Act
        List<int> result = new();
        foreach (var item in queue)
        {
            result.Add(item);
        }

        // Assert
        Assert.That(result, Is.EqualTo(new List<int> { 10, 20, 30 }));
    }

    [Test]
    public void GetEnumerator_ModifyingDuringEnumeration_ThrowsException()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var item in queue)
            {
                queue.Enqueue(40);
            }
        });
    }

    [Test]
    public void CopyTo_CopiesElementsToArray()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);
        var array = new int[5];

        // Act
        ((ICollection)queue).CopyTo(array, 1);

        // Assert
        Assert.That(array, Is.EqualTo(new[] { 0, 10, 20, 30, 0 }));
    }

    [Test]
    public void CopyTo_NullArray_ThrowsArgumentNullException()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ICollection)queue).CopyTo(null, 0));
    }

    [Test]
    public void CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        var array = new int[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ICollection)queue).CopyTo(array, -1));
    }

    [Test]
    public void CopyTo_IndexPlusCountExceedsArrayBounds_ThrowsArgumentException()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
        queue.Enqueue(20);
        queue.Enqueue(30);
        var array = new int[2];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ((ICollection)queue).CopyTo(array, 0));
    }

    [Test]
    public void CopyTo_MultidimensionalArray_ThrowsArgumentException()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        queue.Enqueue(10);
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
        LockFreeQueue<int> queue = new();
        CountdownEvent countdown = new(threadCount);

        // Act
        // Create producer threads that add items
        for (var t = 0; t < threadCount / 2; t++)
        {
            var threadNum = t;
            Task.Run(() =>
            {
                for (var i = 0; i < operationsPerThread; i++)
                {
                    queue.Enqueue(threadNum * operationsPerThread + i);
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
    public void EnqueueDequeue_ManyOperations_MaintainsCorrectOrder()
    {
        // Arrange
        LockFreeQueue<int> queue = new();
        const int itemCount = 10000;

        // Act - Enqueue many items
        for (var i = 0; i < itemCount; i++)
        {
            queue.Enqueue(i);
        }

        // Assert - Dequeue should get them in the same order
        for (var i = 0; i < itemCount; i++)
        {
            Assert.That(queue.Dequeue(), Is.EqualTo(i));
        }

        Assert.That(queue.Count, Is.EqualTo(0));
    }
}
