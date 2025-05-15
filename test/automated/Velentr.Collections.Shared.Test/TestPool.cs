using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections.Test;

[TestFixture]
public class TestPool
{
    [Test]
    public void TestInitialization()
    {
        Pool<int> pool = new(10);
        Assert.That(pool.MaxSize, Is.EqualTo(10));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(10));
        Assert.That(pool.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestAddAndRetrieve()
    {
        Pool<int> pool = new(5);
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
        Pool<int> pool = new(2);
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
        Pool<int> pool = new(2, PoolFullAction.PopNewestItem);
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
        Pool<int?> pool = new(2, PoolFullAction.Ignore);
        pool.Add(1);
        pool.Add(2);
        var poppedItem = pool.AddAndReturn(3);

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(0));
        Assert.That(poppedItem, Is.Null);
        Assert.That(pool[0], Is.EqualTo(1));
        Assert.That(pool[1], Is.EqualTo(2));
    }

    [Test]
    public void TestAddBeyondCapacity_Throw()
    {
        Pool<int?> pool = new(2, PoolFullAction.ThrowException);
        pool.Add(1);
        pool.Add(2);
        Assert.Throws<PoolFullException>(() => pool.AddAndReturn(3));
    }

    [Test]
    public void TestAddBeyondCapacity_Grow()
    {
        Pool<int?> pool = new(2, PoolFullAction.Grow);
        pool.Add(1);
        pool.Add(2);
        Assert.That(pool.MaxSize, Is.EqualTo(2));
        var poppedItem = pool.AddAndReturn(3);

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
        Pool<int> pool = new(3);
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
        Pool<int> pool = new(3);
        pool.Add(1);
        pool.Add(2);

        Assert.That(pool.Count, Is.EqualTo(2));
        pool.Clear();
        Assert.That(pool.Count, Is.EqualTo(0));
        Assert.That(pool.RemainingCapacity, Is.EqualTo(3));
    }

    [Test]
    public void TestContains()
    {
        Pool<int> pool = new(3);
        pool.Add(1);
        pool.Add(2);

        Assert.That(pool.Contains(1), Is.True);
        Assert.That(pool.Contains(3), Is.False);
    }

    [Test]
    public void TestCopyTo()
    {
        Pool<int> pool = new(3);
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
        Pool<DisposableItem> pool = new(3);
        DisposableItem item1 = new();
        DisposableItem item2 = new();

        pool.Add(item1);
        pool.Add(item2);

        pool.Dispose();

        Assert.That(item1.IsDisposed, Is.True);
        Assert.That(item2.IsDisposed, Is.True);
    }

    private class DisposableItem : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.IsDisposed = true;
        }
    }
}
