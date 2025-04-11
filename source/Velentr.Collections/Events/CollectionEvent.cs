namespace Velentr.Collections.Events;

/// <summary>
/// Provides a generic event management class that handles event subscriptions
/// and maintains a list of registered event handlers.
/// </summary>
/// <typeparam name="T">The type of EventArgs used by the event.</typeparam>
public class CollectionEvent<T> where T : EventArgs
{
    /// <summary>
    /// List of registered event handlers to enable tracking and bulk operations.
    /// </summary>
    internal List<EventHandler<T>> Delegates = new List<EventHandler<T>>();

    /// <summary>
    /// Gets or sets the event that clients can subscribe to or unsubscribe from.
    /// </summary>
    public event EventHandler<T> Event
    {
        add
        {
            InternalEvent += value;
            Delegates.Add(value);
        }

        remove
        {
            InternalEvent -= value;
            Delegates.Remove(value);
        }
    }

    /// <summary>
    /// The internal event that will be triggered when the public event is invoked.
    /// </summary>
    internal event EventHandler<T>? InternalEvent;

    /// <summary>
    /// Removes an event handler from the collection event.
    /// </summary>
    /// <param name="left">The collection event to remove from.</param>
    /// <param name="right">The event handler to remove.</param>
    /// <returns>The modified collection event.</returns>
    public static CollectionEvent<T> operator -(CollectionEvent<T> left, EventHandler<T> right)
    {
        left.InternalEvent -= right;
        left.Delegates.Remove(right);

        return left;
    }

    /// <summary>
    /// Adds an event handler to the collection event.
    /// </summary>
    /// <param name="left">The collection event to add to.</param>
    /// <param name="right">The event handler to add.</param>
    /// <returns>The modified collection event.</returns>
    public static CollectionEvent<T> operator +(CollectionEvent<T> left, EventHandler<T> right)
    {
        left.InternalEvent += right;
        left.Delegates.Add(right);

        return left;
    }

    /// <summary>
    /// Removes all event handlers from this collection event.
    /// </summary>
    public void Clear()
    {
        var list = Delegates;
        for (var i = 0; i < list.Count; i++)
        {
            InternalEvent -= list[i];
        }

        Delegates.Clear();
    }

    /// <summary>
    /// Triggers the event with the specified sender and event arguments.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    public void EventTriggered(object sender, T e)
    {
        InternalEvent?.Invoke(sender, e);
    }
}
