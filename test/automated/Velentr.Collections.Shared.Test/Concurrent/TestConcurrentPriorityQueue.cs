using Velentr.Collections.Concurrent;

namespace Velentr.Collections.Test.Concurrent;

public class TestConcurrentPriorityQueue
{
    [Test]
    public void Clear_WithItems_RemovesAllItems()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        queue.Enqueue("Item1", 5);
        queue.Enqueue("Item2", 10);

        // Act
        queue.Clear();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Test]
    public void ConcurrentAccess_MultipleThreads_MaintainsConsistency()
    {
        // Arrange
        ConcurrentPriorityQueue<int, int> queue = new();
        var itemCount = 1000;
        var threadCount = 10;
        CountdownEvent countdown = new(threadCount);

        // Act
        for (var t = 0; t < threadCount; t++)
        {
            var threadId = t;
            Task.Run(() =>
            {
                for (var i = 0; i < itemCount; i++)
                {
                    var value = i * threadCount + threadId;
                    queue.Enqueue(value, value);
                }

                countdown.Signal();
            });
        }

        countdown.Wait();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(itemCount * threadCount));

        // Verify items are dequeued in priority order
        int? previous = null;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (previous.HasValue)
            {
                Assert.That(current, Is.GreaterThanOrEqualTo(previous.Value));
            }

            previous = current;
        }
    }

    [Test]
    public void ConcurrentAccessWithEnqueueDequeue_MultipleThreads_MaintainsConsistency()
    {
        // Arrange
        ConcurrentPriorityQueue<int, int> queue = new();
        var operationCount = 1000;
        var producerCount = 5;
        var consumerCount = 3;
        var totalTaskCount = producerCount + consumerCount;
        CountdownEvent countdown = new(totalTaskCount);
        var successfulDequeues = 0;

        // Act - Producers
        for (var t = 0; t < producerCount; t++)
        {
            Task.Run(() =>
            {
                for (var i = 0; i < operationCount; i++)
                {
                    queue.Enqueue(i, i);
                }

                countdown.Signal();
            });
        }

        // Act - Consumers
        for (var t = 0; t < consumerCount; t++)
        {
            Task.Run(() =>
            {
                for (var i = 0; i < operationCount; i++)
                {
                    if (queue.TryDequeue(out _, out _))
                    {
                        Interlocked.Increment(ref successfulDequeues);
                    }
                }

                countdown.Signal();
            });
        }

        countdown.Wait();

        // Assert
        var expectedEnqueues = producerCount * operationCount;
        var finalCount = queue.Count;
        Assert.That(finalCount + successfulDequeues, Is.EqualTo(expectedEnqueues),
            "Total enqueued items should equal dequeued items plus remaining items");
    }

    [Test]
    public void Constructor_Default_CreatesEmptyQueue()
    {
        // Arrange & Act
        ConcurrentPriorityQueue<string, int> queue = new();

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithCustomComparer_UsesComparer()
    {
        // Arrange
        Comparer<int> comparer = Comparer<int>.Create((a, b) => b.CompareTo(a)); // Reverse order
        ConcurrentPriorityQueue<string, int> queue = new(comparer);

        // Act
        queue.Enqueue("Low", 10);
        queue.Enqueue("High", 1);

        // Assert - with reverse comparer, higher number has priority
        Assert.That(queue.Dequeue(), Is.EqualTo("Low"));
    }

    [Test]
    public void Constructor_WithInitialCapacity_CreatesEmptyQueue()
    {
        // Arrange & Act
        ConcurrentPriorityQueue<string, int> queue = new(100);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithItems_AddsAllItems()
    {
        // Arrange
        (string, int)[] items = new[]
        {
            ("Item1", 5),
            ("Item2", 2),
            ("Item3", 10)
        };

        // Act
        ConcurrentPriorityQueue<string, int> queue = new(items);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(3));
        Assert.That(queue.Dequeue(), Is.EqualTo("Item2"));
        Assert.That(queue.Dequeue(), Is.EqualTo("Item1"));
        Assert.That(queue.Dequeue(), Is.EqualTo("Item3"));
    }

    [Test]
    public void Dequeue_EmptyQueue_ThrowsInvalidOperationException()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Test]
    public void Enqueue_WithMultipleItems_MaintainsPriorityOrder()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();

        // Act
        queue.Enqueue("Medium", 5);
        queue.Enqueue("Low", 1);
        queue.Enqueue("High", 10);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(3));
        Assert.That(queue.Dequeue(), Is.EqualTo("Low"));
        Assert.That(queue.Dequeue(), Is.EqualTo("Medium"));
        Assert.That(queue.Dequeue(), Is.EqualTo("High"));
    }

    [Test]
    public void EnqueueRange_NullElements_ThrowsArgumentNullException()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        IEnumerable<string> elements = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueRange(elements, 1));
    }

    [Test]
    public void EnqueueRange_NullItems_ThrowsArgumentNullException()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        IEnumerable<(string, int)> items = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => queue.EnqueueRange(items));
    }

    [Test]
    public void EnqueueRange_WithElementsAndPriority_AddsAllItems()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        var elements = new[] { "Item1", "Item2", "Item3" };

        // Act
        queue.EnqueueRange(elements, 5);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(3));
    }

    [Test]
    public void EnqueueRange_WithItems_AddsAllItems()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        (string, int)[] items = new[]
        {
            ("Item1", 5),
            ("Item2", 2),
            ("Item3", 10)
        };

        // Act
        queue.EnqueueRange(items);

        // Assert
        Assert.That(queue.Count, Is.EqualTo(3));
        Assert.That(queue.Dequeue(), Is.EqualTo("Item2"));
    }

    [Test]
    public void Peek_EmptyQueue_ThrowsInvalidOperationException()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Test]
    public void Peek_WithItems_ReturnsLowestPriorityWithoutRemoving()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        queue.Enqueue("Medium", 5);
        queue.Enqueue("Low", 1);
        queue.Enqueue("High", 10);

        // Act
        var peeked = queue.Peek();

        // Assert
        Assert.That(peeked, Is.EqualTo("Low"));
        Assert.That(queue.Count, Is.EqualTo(3)); // Count unchanged
    }

    [Test]
    public void TryDequeue_EmptyQueue_ReturnsFalse()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();

        // Act
        var result = queue.TryDequeue(out var element, out var priority);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(element, Is.EqualTo(default(string)));
        Assert.That(priority, Is.EqualTo(default(int)));
    }

    [Test]
    public void TryDequeue_WithItems_RemovesAndReturnsLowestPriority()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();
        queue.Enqueue("Medium", 5);
        queue.Enqueue("High", 10);

        // Act
        var result = queue.TryDequeue(out var element, out var priority);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(element, Is.EqualTo("Medium"));
        Assert.That(priority, Is.EqualTo(5));
        Assert.That(queue.Count, Is.EqualTo(1));
    }

    [Test]
    public void TryPeek_EmptyQueue_ReturnsFalse()
    {
        // Arrange
        ConcurrentPriorityQueue<string, int> queue = new();

        // Act
        var result = queue.TryPeek(out var element, out var priority);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(element, Is.EqualTo(default(string)));
        Assert.That(priority, Is.EqualTo(default(int)));
    }
}
