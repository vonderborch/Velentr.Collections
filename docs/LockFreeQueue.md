# LockFreeQueue

A lock-free queue (First-in, First-out) implementation

## Example Usage

```csharp
var queue = new LockFreeQueue<int>();
queue.Enqueue(1);
queue.Enqueue(2);
queue.Enqueue(3);

Console.WriteLine(queue.Count); // 3
Console.WriteLine(queue.Dequeue().ToString()); // 1
Console.WriteLine(queue.Dequeue().ToString()); // 2
Console.WriteLine(queue.Dequeue().ToString()); // 3
Console.WriteLine(queue.Count); // 0

```
