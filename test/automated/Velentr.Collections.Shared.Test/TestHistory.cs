namespace Velentr.Collections.Test;

[TestFixture]
public class TestHistory
{
    [Test]
    public void Test_AddAndRetrieveItems()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);

        Assert.That(history.Count, Is.EqualTo(3));
        Assert.That(history.CurrentItem, Is.EqualTo(3));
        Assert.That(history.OldestItem, Is.EqualTo(1));
        Assert.That(history.NewestItem, Is.EqualTo(3));
    }

    [Test]
    public void Test_UndoRedo()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);

        Assert.That(history.CurrentItem, Is.EqualTo(3));

        var undoneItem = history.Undo();
        Assert.That(undoneItem, Is.EqualTo(2));
        Assert.That(history.CurrentItem, Is.EqualTo(2));

        var redoneItem = history.Redo();
        Assert.That(redoneItem, Is.EqualTo(3));
        Assert.That(history.CurrentItem, Is.EqualTo(3));
    }

    [Test]
    public void Test_UndoMultipleSteps()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);

        List<int> undoneItems = history.Undo(2);
        Assert.That(undoneItems, Is.EqualTo(new[] { 3, 2 }).AsCollection);
        Assert.That(history.CurrentItem, Is.EqualTo(1));
    }

    [Test]
    public void Test_RedoMultipleSteps()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);
        history.Undo(2);

        List<int> redoneItems = history.Redo(2);
        Assert.That(redoneItems, Is.EqualTo(new[] { 2, 3 }).AsCollection);
        Assert.That(history.CurrentItem, Is.EqualTo(3));
    }

    [Test]
    public void Test_ClearHistory()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);

        history.Clear();
        Assert.That(history.Count, Is.EqualTo(0));
        Assert.That(() =>
        {
            var item = history.CurrentItem;
        }, Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Test_MaxHistoryLimit()
    {
        History<int> history = new(3);
        history.Add(1);
        history.Add(2);
        history.Add(3);
        history.Add(4);

        Assert.That(history.Count, Is.EqualTo(3));
        Assert.That(history.OldestItem, Is.EqualTo(2));
        Assert.That(history.NewestItem, Is.EqualTo(4));
    }

    [Test]
    public void Test_AddAndReturn()
    {
        History<int?> history = new(3);
        history.Add(1);
        history.Add(2);
        var removedItem = history.AddAndReturn(3);
        Assert.That(removedItem, Is.Null);

        removedItem = history.AddAndReturn(4);
        Assert.That(removedItem, Is.EqualTo(1));
        Assert.That(history.Count, Is.EqualTo(3));
    }

    [Test]
    public void Test_Contains()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);

        Assert.That(history.Contains(2), Is.True);
        Assert.That(history.Contains(4), Is.False);
    }

    [Test]
    public void Test_CopyTo()
    {
        History<int> history = new(5);
        history.Add(1);
        history.Add(2);
        history.Add(3);

        var array = new int[3];
        history.CopyTo(array, 0);

        Assert.That(array, Is.EqualTo(new[] { 1, 2, 3 }).AsCollection);
    }
}
