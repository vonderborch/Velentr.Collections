# LockFreeList

A lock-free linked list implementation.

## Example Usage

```csharp
var lst = new LockFreeList<int>();
lst.Add(1);
lst.Add(2);
lst.Add(3);

Console.WriteLine(lst.Count); // 3
Console.WriteLine(lst[0].ToString()); // 1
Console.WriteLine(lst[1].ToString()); // 2
Console.WriteLine(lst[2].ToString()); // 3
Console.WriteLine(queue.Count); // 0

```
