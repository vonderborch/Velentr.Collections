namespace Velentr.Collections.Internal;

/// <summary>
///     Represents a node in a linked structure that can contain a value of type <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of value stored in the node.</typeparam>
public class Node<T> : IDisposable
{
    /// <summary>
    ///     Gets or sets the reference to the next node in the linked structure.
    /// </summary>
    public volatile Node<T>? Next;

    /// <summary>
    ///     Gets or sets the value stored in this node.
    /// </summary>
    public T Value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Node{T}" /> class with the specified value.
    /// </summary>
    public Node()
    {
        this.Next = null;
        this.Value = default!;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Node{T}" /> class with the specified value.
    /// </summary>
    /// <param name="value">The value to store in the node.</param>
    public Node(T value)
    {
        this.Next = null;
        this.Value = value;
    }

    /// <summary>
    ///     Disposes the current node by disposing its value if it implements <see cref="IDisposable" />.
    /// </summary>
    public void Dispose()
    {
        if (this.Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
