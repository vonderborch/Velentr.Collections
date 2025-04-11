using NUnit.Framework;
using Velentr.Collections;
using System.Collections.Generic;
using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections.Test;

[TestFixture]
public class TestPool
{
    [Test]
    public void TestInitialization()
    {
        var pool = new Pool<int>(10);
        Assert.That(pool.MaxSize, Is.EqualTo(10));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(10));
        Assert.That(pool.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestAddAndRetrieve()
    {
        var pool = new Pool<int>(5);
        pool.Add(1);
        pool.Add(2);

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(3));
        Assert.That(pool[0], Is.EqualTo(1));
        Assert.That(pool[1], Is.EqualTo(2));
    }

    [Test]
    public void TestAddBeyondCapacity_PopOldestItem()
    {
        var pool = new Pool<int>(2, PoolFullAction.PopOldestItem);
        pool.Add(1);
        pool.Add(2);
        var poppedItem = pool.AddAndReturn(3);

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(0));
        Assert.That(poppedItem, Is.EqualTo(1));
        Assert.That(pool[0], Is.EqualTo(3));
        Assert.That(pool[1], Is.EqualTo(2));
    }

    [Test]
    public void TestAddBeyondCapacity_PopNewestItem()
    {
        var pool = new Pool<int>(2, PoolFullAction.PopNewestItem);
        pool.Add(1);
        pool.Add(2);
        var poppedItem = pool.AddAndReturn(3);

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(0));
        Assert.That(poppedItem, Is.EqualTo(2));
        Assert.That(pool[0], Is.EqualTo(1));
        Assert.That(pool[1], Is.EqualTo(3));
    }

    [Test]
    public void TestAddBeyondCapacity_Ignore()
    {
        var pool = new Pool<int?>(2, PoolFullAction.Ignore);
        pool.Add(1);
        pool.Add(2);
        int? poppedItem = pool.AddAndReturn(3);

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(0));
        Assert.That(poppedItem, Is.Null);
        Assert.That(pool[0], Is.EqualTo(1));
        Assert.That(pool[1], Is.EqualTo(2));
    }

    [Test]
    public void TestAddBeyondCapacity_Throw()
    {
        var pool = new Pool<int?>(2, PoolFullAction.ThrowException);
        pool.Add(1);
        pool.Add(2);
        Assert.Throws<PoolFullException>(() => pool.AddAndReturn(3));
    }

    [Test]
    public void TestAddBeyondCapacity_Grow()
    {
        var pool = new Pool<int?>(2, PoolFullAction.Grow);
        pool.Add(1);
        pool.Add(2);
        Assert.That(pool.MaxSize, Is.EqualTo(2));
        int? poppedItem = pool.AddAndReturn(3);

        Assert.That(pool.Count, Is.EqualTo(3));
        Assert.That(pool.MaxSize, Is.EqualTo(3));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(0));
        Assert.That(poppedItem, Is.Null);
        Assert.That(pool[0], Is.EqualTo(1));
        Assert.That(pool[1], Is.EqualTo(2));
        Assert.That(pool[2], Is.EqualTo(3));
    }

    [Test]
    public void TestRemove()
    {
        var pool = new Pool<int>(3);
        pool.Add(1);
        pool.Add(2);

        var removed = pool.Remove(1);
        Assert.That(removed, Is.True);
        Assert.That(pool.Count, Is.EqualTo(1));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(2));
    }

    [Test]
    public void TestClear()
    {
        var pool = new Pool<int>(3);
        pool.Add(1);
        pool.Add(2);

        pool.Clear();
        Assert.That(pool.Count, Is.EqualTo(0));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(3));
    }

    [Test]
    public void TestContains()
    {
        var pool = new Pool<int>(3);
        pool.Add(1);
        pool.Add(2);

        Assert.That(pool.Contains(1), Is.True);
        Assert.That(pool.Contains(3), Is.False);
    }

    [Test]
    public void TestCopyTo()
    {
        var pool = new Pool<int>(3);
        pool.Add(1);
        pool.Add(2);

        var array = new int[3];
        pool.CopyTo(array, 0);

        Assert.That(array[0], Is.EqualTo(1));
        Assert.That(array[1], Is.EqualTo(2));
        Assert.That(array[2], Is.EqualTo(0));
    }

    [Test]
    public void TestDispose()
    {
        var pool = new Pool<DisposableItem>(3);
        var item1 = new DisposableItem();
        var item2 = new DisposableItem();

        pool.Add(item1);
        pool.Add(item2);

        pool.Dispose();

        Assert.That(item1.IsDisposed, Is.True);
        Assert.That(item2.IsDisposed, Is.True);
    }

    private class DisposableItem : System.IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
