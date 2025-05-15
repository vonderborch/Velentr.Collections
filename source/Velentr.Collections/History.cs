using System.Collections;
using System.Diagnostics;
using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections;

[DebuggerDisplay("Count = {Count}, Current Position = {CurrentPosition}, Can Undo = {CanUndo}, Can Redo = {CanRedo}")]
public class History<T>(int maxHistoryItems = 32) : ICollection<T>
{
    private readonly SizeLimitedList<T> internalList = new(maxHistoryItems, actionWhenFull: SizeLimitedCollectionFullAction.PopOldestItem);

    /// <summary>
    ///     Gets a value indicating whether a redo operation can be performed.
    /// </summary>
    public bool CanRedo => this.CurrentPosition != this.MaxHistoryIndex;

    /// <summary>
    ///     Gets a value indicating whether an undo operation can be performed.
    /// </summary>
    public bool CanUndo => this.CurrentPosition != 0;

    /// <summary>
    ///     Gets the current item in the history.
    /// </summary>
    public T CurrentItem => this.internalList[this.CurrentPosition];

    /// <summary>
    ///     Gets all items in the history as a list.
    /// </summary>
    public List<T> GetAllItems => new(this.internalList);

    /// <summary>
    ///     Gets the maximum index in the history.
    /// </summary>
    public int MaxHistoryIndex => this.internalList.Count > 0 ? this.internalList.Count - 1 : 0;

    /// <summary>
    ///     Gets the newest item in the history.
    /// </summary>
    public T NewestItem => this.internalList[this.MaxHistoryIndex];

    /// <summary>
    ///     Gets the oldest item in the history.
    /// </summary>
    public T OldestItem => this.internalList[0];

    /// <summary>
    ///     Gets the previous item in the history, or the default value if at the beginning.
    /// </summary>
    public T? PreviousItem => this.CurrentPosition == 0 ? default : this.internalList[this.CurrentPosition - 1];

    /// <summary>
    ///     Gets the current position in the history.
    /// </summary>
    public int CurrentPosition { get; private set; }

    /// <summary>
    ///     Gets or sets the item at the specified index in the history.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item at the specified index.</returns>
    public T this[int index]
    {
        get => this.internalList[index];
        set => this.internalList[index] = value;
    }

    /// <summary>
    ///     Gets the number of items in the history.
    /// </summary>
    public int Count => this.internalList.Count;

    /// <summary>
    ///     Gets a value indicating whether the history is read-only.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    ///     Adds a new item to the history and updates the current position.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        this.internalList.Add(item);
        this.CurrentPosition = this.MaxHistoryIndex;
    }

    /// <summary>
    ///     Clears all items from the history.
    /// </summary>
    public void Clear()
    {
        this.internalList.Clear();
        this.CurrentPosition = 0;
    }

    /// <summary>
    ///     Determines whether the history contains the specified item.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>True if the item exists; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return this.internalList.Contains(item);
    }

    /// <summary>
    ///     Copies the elements of the history to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this.internalList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the history.
    /// </summary>
    /// <returns>An enumerator for the history.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < this.internalList.Count; i++)
        {
            yield return this.internalList[i];
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the history.
    /// </summary>
    /// <returns>An enumerator for the history.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Throws an exception as removing discrete items is not supported in a history collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>Always throws an exception.</returns>
    public bool Remove(T item)
    {
        throw new NotImplementedException(
            "Unable to Remove discrete items from a history collection, use Clear() instead.");
    }

    /// <summary>
    ///     Adds a new item to the history and returns the item that was removed if the history exceeded its size limit.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The removed item if the history exceeded its size limit; otherwise, null.</returns>
    public T? AddAndReturn(T item)
    {
        T? output = this.internalList.AddAndReturn(item);
        this.CurrentPosition = this.MaxHistoryIndex;
        return output;
    }

    /// <summary>
    ///     Clears a specified number of items from the history starting from the beginning.
    /// </summary>
    /// <param name="steps">The number of items to clear.</param>
    public void Clear(int steps)
    {
        Clear(0, steps);
    }

    /// <summary>
    ///     Clears a specified number of items from the history starting at a given index.
    /// </summary>
    /// <param name="index">The starting index.</param>
    /// <param name="steps">The number of items to clear.</param>
    public void Clear(int index, int steps)
    {
        var itemsToClear = Math.Min(steps, this.internalList.Count - index);
        while (itemsToClear > 0)
        {
            this.internalList.RemoveAt(index);
            itemsToClear--;
        }
    }

    /// <summary>
    ///     Moves one step forward in the history and returns the next item.
    /// </summary>
    /// <returns>The next item in the history.</returns>
    /// <exception cref="InvalidOperationException">Thrown if redo is not possible.</exception>
    public T Redo()
    {
        if (!this.CanRedo)
        {
            throw new InvalidOperationException("Cannot redo, already at newest item.");
        }

        this.CurrentPosition += 1;
        return this.internalList[this.CurrentPosition];
    }

    /// <summary>
    ///     Moves multiple steps forward in the history and returns the list of redone items.
    /// </summary>
    /// <param name="steps">The number of steps to redo.</param>
    /// <returns>A list of redone items.</returns>
    /// <exception cref="InvalidOperationException">Thrown if redo is not possible.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if steps are less than 1 or exceed the available redo steps.</exception>
    public List<T> Redo(int steps)
    {
        if (!this.CanRedo)
        {
            throw new InvalidOperationException("Cannot redo, already at newest item.");
        }

        if (steps < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be greater than 0.");
        }

        var actualSteps = Math.Min(steps, this.MaxHistoryIndex - this.CurrentPosition);
        List<T> output = new();
        for (var i = 0; i < actualSteps; i++)
        {
            output.Add(this.internalList[this.CurrentPosition + 1]);
            this.CurrentPosition += 1;
        }

        return output;
    }

    /// <summary>
    ///     Changes the maximum size of the history and adjusts the internal list accordingly.
    /// </summary>
    /// <param name="newMaxSize">The new maximum size of the history.</param>
    public void SetMaxSize(int newMaxSize)
    {
        this.internalList.ChangeMaxSize(newMaxSize);
    }

    /// <summary>
    ///     Moves one step back in the history and returns the previous item.
    /// </summary>
    /// <returns>The previous item in the history.</returns>
    /// <exception cref="InvalidOperationException">Thrown if undo is not possible.</exception>
    public T Undo()
    {
        if (!this.CanUndo)
        {
            throw new InvalidOperationException("Cannot undo, already at oldest item.");
        }

        this.CurrentPosition -= 1;
        return this.internalList[this.CurrentPosition];
    }

    /// <summary>
    ///     Moves multiple steps back in the history and returns the list of undone items.
    /// </summary>
    /// <param name="steps">The number of steps to undo.</param>
    /// <returns>A list of undone items.</returns>
    /// <exception cref="InvalidOperationException">Thrown if undo is not possible.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if steps are less than 1 or exceed the current position.</exception>
    public List<T> Undo(int steps)
    {
        if (!this.CanUndo)
        {
            throw new InvalidOperationException("Cannot undo, already at oldest item.");
        }

        if (steps < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be greater than 0.");
        }

        var actualSteps = Math.Min(steps, this.CurrentPosition);
        if (this.CurrentPosition - steps < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(steps),
                "Steps must be less than or equal to the current position.");
        }

        List<T> output = new();

        for (var i = 0; i < actualSteps; i++)
        {
            output.Add(this.internalList[this.CurrentPosition]);
            this.CurrentPosition -= 1;
        }

        return output;
    }
}
