using System.Collections;
using Velentr.Collections.LockFree;

namespace Velentr.Collections.Test.LockFree;

[TestFixture]
public class TestLockFreeList
{
    [Test]
    public void Constructor_Default_CreatesEmptyList()
    {
        // Arrange & Act
        LockFreeList<int> list = new();

        // Assert
        Assert.That(list.Count, Is.EqualTo(0));
        Assert.That(list.First(), Is.EqualTo(default(int)));
    }

    [Test]
    public void Constructor_SingleValue_CreatesListWithOneElement()
    {
        // Arrange & Act
        LockFreeList<int> list = new(42);

        // Assert
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list.First(), Is.EqualTo(42));
    }

    [Test]
    public void Constructor_Collection_CreatesListWithAllElements()
    {
        // Arrange
        List<int> collection = new() { 1, 2, 3, 4, 5 };

        // Act
        LockFreeList<int> list = new(collection);

        // Assert
        Assert.That(list.Count, Is.EqualTo(5));
        Assert.That(list.First(), Is.EqualTo(1));
        Assert.That(list.Last(), Is.EqualTo(5));
    }

    [Test]
    public void Add_AddsItemsToEndOfList()
    {
        // Arrange
        LockFreeList<int> list = new();

        // Act
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Assert
        Assert.That(list.Count, Is.EqualTo(3));
        Assert.That(list.First(), Is.EqualTo(10));
        Assert.That(list.Last(), Is.EqualTo(30));
    }

    [Test]
    public void Contains_FindsExistingItems()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act & Assert
        Assert.That(list.Contains(10), Is.True);
        Assert.That(list.Contains(20), Is.True);
        Assert.That(list.Contains(30), Is.True);
        Assert.That(list.Contains(40), Is.False);
    }

    [Test]
    public void Contains_HandlesNullValues()
    {
        // Arrange
        LockFreeList<string> list = new();
        list.Add("test");
        list.Add(null);

        // Act & Assert
        Assert.That(list.Contains(null), Is.True);
        Assert.That(list.Contains("test"), Is.True);
        Assert.That(list.Contains("other"), Is.False);
    }

    [Test]
    public void Remove_RemovesExistingItems()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act
        var result1 = list.Remove(20);
        var result2 = list.Remove(40);

        // Assert
        Assert.That(result1, Is.True);
        Assert.That(result2, Is.False);
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Contains(20), Is.False);
    }

    [Test]
    public void Remove_HandlesNullValues()
    {
        // Arrange
        LockFreeList<string> list = new();
        list.Add("test");
        list.Add(null);

        // Act
        var result = list.Remove(null);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list.Contains(null), Is.False);
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act
        list.Clear();

        // Assert
        Assert.That(list.Count, Is.EqualTo(0));
        Assert.That(list.First(), Is.EqualTo(default(int)));
    }

    [Test]
    public void First_ReturnsFirstElement()
    {
        // Arrange
        LockFreeList<int> list = new();

        // Act & Assert - Empty list
        Assert.That(list.First(), Is.EqualTo(default(int)));

        // Act - Add items
        list.Add(10);
        list.Add(20);

        // Assert - Non-empty list
        Assert.That(list.First(), Is.EqualTo(10));

        // Act - Remove first item
        list.Remove(10);

        // Assert - After removal
        Assert.That(list.First(), Is.EqualTo(20));
    }

    [Test]
    public void Last_ReturnsLastElement()
    {
        // Arrange
        LockFreeList<int> list = new();

        // Act & Assert - Empty list
        Assert.That(list.Last(), Is.EqualTo(default(int)));

        // Act - Add items
        list.Add(10);
        list.Add(20);

        // Assert - Non-empty list
        Assert.That(list.Last(), Is.EqualTo(20));

        // Act - Remove last item
        list.Remove(20);

        // Assert - After removal
        Assert.That(list.Last(), Is.EqualTo(10));
    }

    [Test]
    public void TryGetAt_GetsElementAtIndex()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act & Assert
        Assert.That(list.TryGetAt(0, out var value0), Is.True);
        Assert.That(value0, Is.EqualTo(10));

        Assert.That(list.TryGetAt(1, out var value1), Is.True);
        Assert.That(value1, Is.EqualTo(20));

        Assert.That(list.TryGetAt(2, out var value2), Is.True);
        Assert.That(value2, Is.EqualTo(30));

        Assert.That(list.TryGetAt(3, out _), Is.False);
        Assert.That(list.TryGetAt(-1, out _), Is.False);
    }

    [Test]
    public void TryGetAt_HandlesRemoval()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act
        list.Remove(20);

        // Assert
        Assert.That(list.TryGetAt(0, out var value0), Is.True);
        Assert.That(value0, Is.EqualTo(10));

        Assert.That(list.TryGetAt(1, out var value1), Is.True);
        Assert.That(value1, Is.EqualTo(30));

        Assert.That(list.TryGetAt(2, out _), Is.False);
    }

    [Test]
    public void IsSynchronized_AlwaysReturnsTrue()
    {
        // Arrange
        LockFreeList<int> list = new();

        // Act & Assert
        Assert.That(((ICollection)list).IsSynchronized, Is.True);
    }

    [Test]
    public void SyncRoot_ReturnsNonNullObject()
    {
        // Arrange
        LockFreeList<int> list = new();

        // Act
        var syncRoot = ((ICollection)list).SyncRoot;

        // Assert
        Assert.That(syncRoot, Is.Not.Null);
    }

    [Test]
    public void ToList_ReturnsListWithAllElements()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act
        List<int> result = list.ToList;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(10));
        Assert.That(result[1], Is.EqualTo(20));
        Assert.That(result[2], Is.EqualTo(30));
    }

    [Test]
    public void GetEnumerator_EnumeratesElementsInOrder()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act
        List<int> result = new();
        foreach (var item in list)
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
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var item in list)
            {
                list.Add(40);
            }
        });
    }

    [Test]
    public void CopyTo_CopiesElementsToArray()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        var array = new int[5];

        // Act
        list.CopyTo(array, 1);

        // Assert
        Assert.That(array, Is.EqualTo(new[] { 0, 10, 20, 30, 0 }));
    }

    [Test]
    public void CopyTo_InterfaceMethod_CopiesElementsToArray()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        var array = new object[5];

        // Act
        ((ICollection)list).CopyTo(array, 1);

        // Assert
        Assert.That(array[1], Is.EqualTo(10));
        Assert.That(array[2], Is.EqualTo(20));
        Assert.That(array[3], Is.EqualTo(30));
    }

    [Test]
    public void CopyTo_NullArray_ThrowsArgumentNullException()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null, 0));
    }

    [Test]
    public void CopyTo_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        var array = new int[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(array, -1));
    }

    [Test]
    public void CopyTo_IndexPlusCountExceedsArrayBounds_ThrowsArgumentException()
    {
        // Arrange
        LockFreeList<int> list = new();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        var array = new int[2];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => list.CopyTo(array, 0));
    }

    [Test]
    [Timeout(10000)]
    public void MultithreadedOperations_EnsuresThreadSafety()
    {
        // Arrange
        const int operationsPerThread = 1000;
        const int threadCount = 4;
        LockFreeList<int> list = new();
        CountdownEvent countdown = new(threadCount);

        // Act
        // Create threads that add items
        for (var t = 0; t < threadCount / 2; t++)
        {
            var threadNum = t;
            Task.Run(() =>
            {
                for (var i = 0; i < operationsPerThread; i++)
                {
                    list.Add(threadNum * operationsPerThread + i);
                }

                countdown.Signal();
            });
        }

        // Create threads that remove random items
        for (var t = 0; t < threadCount / 2; t++)
        {
            Task.Run(() =>
            {
                Random random = new();
                var removed = 0;
                while (removed < operationsPerThread / 2)
                {
                    var valueToRemove = random.Next(operationsPerThread * 2);
                    if (list.Remove(valueToRemove))
                    {
                        removed++;
                    }
                }

                countdown.Signal();
            });
        }

        // Wait for all threads to finish
        countdown.Wait();

        // Assert - all operations should complete without exceptions
        Assert.That(list.Count, Is.LessThan(operationsPerThread * threadCount / 2));
    }
}
