using NUnit.Framework;
using Velentr.Collections.Concurrent;
using Velentr.Collections.CollectionFullActions;
using Velentr.Collections.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Velentr.Collections.Test.Concurrent;

[TestFixture]
public class TestConcurrentPool
{
    [Test]
    public void TestInitialization()
    {
        var pool = new ConcurrentPool<int>(10);
        Assert.That(pool.MaxSize, Is.EqualTo(10));
        Assert.That(pool.Count, Is.EqualTo(0));
        Assert.That(pool.ActionWhenFull, Is.EqualTo(PoolFullAction.PopOldestItem));
    }

    [Test]
    public void TestAddAndRetrieveItems()
    {
        var pool = new ConcurrentPool<int>(5);
        pool.Add(1);
        pool.Add(2);
        pool.Add(3);

        Assert.That(pool.Count, Is.EqualTo(3));
        Assert.That(pool.Contains(1), Is.True);
        Assert.That(pool.Contains(2), Is.True);
        Assert.That(pool.Contains(3), Is.True);
    }

    [Test]
    public void TestAddBeyondCapacity()
    {
        var pool = new ConcurrentPool<int>(3);
        pool.Add(1);
        pool.Add(2);
        pool.Add(3);
        var removedItem = pool.AddAndReturn(4);

        Assert.That(pool.Count, Is.EqualTo(3));
        Assert.That(pool.Contains(1), Is.False); // Oldest item removed
        Assert.That(pool.Contains(2), Is.True);
        Assert.That(pool.Contains(3), Is.True);
        Assert.That(pool.Contains(4), Is.True);
        Assert.That(removedItem, Is.EqualTo(1));
    }

    [Test]
    public void TestClear()
    {
        var pool = new ConcurrentPool<int>(5);
        pool.Add(1);
        pool.Add(2);
        pool.Add(3);

        pool.Clear();
        Assert.That(pool.Count, Is.EqualTo(0));
        Assert.That(pool.Contains(1), Is.False);
        Assert.That(pool.Contains(2), Is.False);
        Assert.That(pool.Contains(3), Is.False);
    }

    [Test]
    public void TestEvents()
    {
        var pool = new ConcurrentPool<int>(2);
        bool claimedEventTriggered = false;
        bool releasedEventTriggered = false;

        pool.ClaimedSlotEvent += (s, e) => { claimedEventTriggered = true; };
        pool.ReleasedSlotEvent += (s, e) => { releasedEventTriggered = true; };

        pool.Add(1);
        Assert.That(claimedEventTriggered, Is.True);

        claimedEventTriggered = false;
        pool.Add(2);
        pool.Add(3); // This should trigger a release event for the oldest item (1)

        Assert.That(releasedEventTriggered, Is.True);
        Assert.That(claimedEventTriggered, Is.True);
    }

    [Test]
    public void TestDispose()
    {
        var pool = new ConcurrentPool<IDisposable>(2);
        var disposable1 = new TestDisposable();
        var disposable2 = new TestDisposable();

        pool.Add(disposable1);
        pool.Add(disposable2);

        pool.Dispose();

        Assert.That(disposable1.IsDisposed, Is.True);
        Assert.That(disposable2.IsDisposed, Is.True);
    }

    [Test]
    public void TestThreadSafety_AddItemsConcurrently()
    {
        var pool = new ConcurrentPool<int>(100);
        int numberOfThreads = 10;
        int itemsPerThread = 10;

        Parallel.For(0, numberOfThreads, threadId =>
        {
            for (int i = 0; i < itemsPerThread; i++)
            {
                pool.Add(threadId * itemsPerThread + i);
            }
        });

        Assert.That(pool.Count, Is.EqualTo(100));
    }

    [Test]
    public void TestThreadSafety_AddAndRemoveConcurrently()
    {
        var pool = new ConcurrentPool<int>(50);
        int numberOfThreads = 10;

        Parallel.For(0, numberOfThreads, threadId =>
        {
            for (int i = 0; i < 10; i++)
            {
                pool.Add(threadId * 10 + i);
                pool.Remove(threadId * 10 + i);
            }
        });

        Assert.That(pool.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestThreadSafety_ClaimAndReleaseSlotsConcurrently()
    {
        var pool = new ConcurrentPool<int>(10);
        int numberOfThreads = 5;

        Parallel.For(0, numberOfThreads, threadId =>
        {
            for (int i = 0; i < 2; i++)
            {
                pool.Add(threadId * 2 + i);
            }
        });

        Parallel.For(0, numberOfThreads, threadId =>
        {
            for (int i = 0; i < 2; i++)
            {
                pool.Remove(threadId * 2 + i);
            }
        });

        Assert.That(pool.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestThreadSafety_AddBeyondCapacityConcurrently()
    {
        var pool = new ConcurrentPool<int>(10);
        int numberOfThreads = 5;

        Parallel.For(0, numberOfThreads, threadId =>
        {
            for (int i = 0; i < 5; i++)
            {
                pool.Add(threadId * 5 + i);
            }
        });

        Assert.That(pool.Count, Is.EqualTo(10));
    }

    private class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}