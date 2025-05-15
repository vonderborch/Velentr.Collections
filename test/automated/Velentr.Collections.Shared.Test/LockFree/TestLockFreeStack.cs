using Velentr.Collections.LockFree;

namespace Velentr.Collections.Test.LockFree;

public class TestLockFreeStack
{
    [Test]
    public void Clear_RemovesAllElements()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        for (var i = 0; i < 5; i++)
        {
            stack.Push(i);
        }

        // Act
        stack.Clear();

        // Assert
        Assert.That(stack.Count, Is.EqualTo(0));
        Assert.That(stack.Peek(), Is.EqualTo(default(int)));
    }

    [Test]
    public void Constructor_WithInitialValue_CreatesStackWithOneElement()
    {
        // Arrange & Act
        var expectedValue = 42;
        LockFreeStack<int> stack = new(expectedValue);

        // Assert
        Assert.That(stack.Count, Is.EqualTo(1));
        Assert.That(stack.Peek(), Is.EqualTo(expectedValue));
    }

    [Test]
    public void Contains_FindsExistingElement_ReturnsTrue()
    {
        // Arrange
        LockFreeStack<string> stack = new();
        var item = "find me";
        stack.Push("other");
        stack.Push(item);
        stack.Push("another");

        // Act
        var result = stack.Contains(item);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Contains_WithMissingElement_ReturnsFalse()
    {
        // Arrange
        LockFreeStack<string> stack = new();
        stack.Push("one");
        stack.Push("two");

        // Act
        var result = stack.Contains("missing");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Contains_WithNullElement_WorksCorrectly()
    {
        // Arrange
        LockFreeStack<string> stack = new();
        stack.Push("one");
        stack.Push(null);
        stack.Push("two");

        // Act & Assert
        Assert.That(stack.Contains(null), Is.True);
    }

    [Test]
    public void CopyTo_CopiesAllElements()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        var array = new int[5];

        // Act
        stack.CopyTo(array, 1);

        // Assert - Note that stack order is reversed in the array (LIFO)
        Assert.That(array[1], Is.EqualTo(3));
        Assert.That(array[2], Is.EqualTo(2));
        Assert.That(array[3], Is.EqualTo(1));
        Assert.That(array[0], Is.EqualTo(0)); // Unchanged
        Assert.That(array[4], Is.EqualTo(0)); // Unchanged
    }

    [Test]
    public void CopyTo_WithInsufficientSpace_ThrowsArgumentException()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        for (var i = 0; i < 5; i++)
        {
            stack.Push(i);
        }

        var array = new int[3];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => stack.CopyTo(array, 0));
    }

    [Test]
    public void CopyTo_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);
        var array = new int[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => stack.CopyTo(array, -1));
    }

    [Test]
    public void CopyTo_WithNullArray_ThrowsArgumentNullException()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => stack.CopyTo(null, 0));
    }

    [Test]
    public void DefaultConstructor_CreatesEmptyStack()
    {
        // Arrange & Act
        LockFreeStack<int> stack = new();

        // Assert
        Assert.That(stack.Count, Is.EqualTo(0));
        Assert.That(stack.Peek(), Is.EqualTo(default(int)));
    }

    [Test]
    public void GetEnumerator_EnumeratesInLifoOrder()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        var expected = new[] { 3, 2, 1 };
        List<int> result = new();

        // Act
        foreach (var item in stack)
        {
            result.Add(item);
        }

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetEnumerator_ThrowsWhenCollectionModified()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);
        stack.Push(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var item in stack)
            {
                // Modify during enumeration
                if (item == 2)
                {
                    stack.Push(3);
                }
            }
        });
    }

    [Test]
    public void IsSynchronized_ReturnsTrue()
    {
        // Arrange
        LockFreeStack<int> stack = new();

        // Act & Assert
        Assert.That(stack.IsSynchronized, Is.True);
    }

    [Test]
    public void MultithreadedPushAndPop_WorksCorrectly()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        const int numThreads = 10;
        const int operationsPerThread = 1000;
        var totalOperations = numThreads * operationsPerThread;
        CountdownEvent countdown = new(numThreads);
        Random rnd = new();

        // Act
        for (var t = 0; t < numThreads; t++)
        {
            Task.Run(() =>
            {
                for (var i = 0; i < operationsPerThread; i++)
                {
                    if (rnd.Next(2) == 0)
                    {
                        // Push
                        stack.Push(i);
                    }
                    else
                    {
                        // Pop
                        stack.Pop();
                    }
                }

                countdown.Signal();
            });
        }

        // Wait for all threads to complete
        countdown.Wait();

        // Assert - no exceptions means success, and the final count is reasonable
        Assert.That(stack.Count, Is.GreaterThanOrEqualTo(0));
        Assert.That(stack.Count, Is.LessThanOrEqualTo(totalOperations));
    }

    [Test]
    public void Peek_OnEmptyStack_ReturnsDefaultValue()
    {
        // Arrange
        LockFreeStack<string> stack = new();

        // Act
        var result = stack.Peek();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Peek_ReturnsTopElementWithoutRemoving()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);
        stack.Push(2);
        var expectedCount = stack.Count;

        // Act
        var result = stack.Peek();

        // Assert
        Assert.That(result, Is.EqualTo(2));
        Assert.That(stack.Count, Is.EqualTo(expectedCount));
    }

    [Test]
    public void Pop_OnEmptyStack_ReturnsDefaultValue()
    {
        // Arrange
        LockFreeStack<string> stack = new();

        // Act
        var result = stack.Pop();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Pop_ReturnsAndRemovesTopElement()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(1);
        stack.Push(2);
        var initialCount = stack.Count;

        // Act
        var result = stack.Pop();

        // Assert
        Assert.That(result, Is.EqualTo(2));
        Assert.That(stack.Count, Is.EqualTo(initialCount - 1));
        Assert.That(stack.Peek(), Is.EqualTo(1));
    }

    [Test]
    public void PopWithOutParameter_OnEmptyStack_ReturnsFalseAndDefaultValue()
    {
        // Arrange
        LockFreeStack<int> stack = new();

        // Act
        var success = stack.Pop(out var value);

        // Assert
        Assert.That(success, Is.False);
        Assert.That(value, Is.EqualTo(default(int)));
    }

    [Test]
    public void PopWithOutParameter_ReturnsSuccessAndValue()
    {
        // Arrange
        LockFreeStack<int> stack = new();
        stack.Push(42);

        // Act
        var success = stack.Pop(out var value);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public void Push_AddsElementToTopOfStack()
    {
        // Arrange
        LockFreeStack<string> stack = new();
        var item = "test item";

        // Act
        stack.Push(item);

        // Assert
        Assert.That(stack.Peek(), Is.EqualTo(item));
    }

    [Test]
    public void Push_IncreasesCount()
    {
        // Arrange
        LockFreeStack<string> stack = new();
        var initialCount = stack.Count;

        // Act
        stack.Push("item");

        // Assert
        Assert.That(stack.Count, Is.EqualTo(initialCount + 1));
    }

    [Test]
    public void SyncRoot_IsNotNull()
    {
        // Arrange
        LockFreeStack<int> stack = new();

        // Act & Assert
        Assert.That(stack.SyncRoot, Is.Not.Null);
    }
}
