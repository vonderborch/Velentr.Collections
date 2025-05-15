# ConcurrentPriorityQueue

A thread-safe priority queue where the item with the lowest priority is returned on dequeue.

## Example Usage

```csharp
var queue = new ConcurrentPriorityQueue<string, int>();
queue.Enqueue("Medium", 5);
queue.Enqueue("Low", 1);
queue.Enqueue("High", 10);

Console.WriteLine(hist.Count); // 3
Console.WriteLine(hist.Dequeue().ToString()); // "Low"
Console.WriteLine(hist.Dequeue().ToString()); // "Medium"
Console.WriteLine(hist.Dequeue().ToString()); // "High"

```
