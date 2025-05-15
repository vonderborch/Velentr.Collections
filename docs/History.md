# History

A collection that keeps track of the history of items added and removed from it. It can be used to track changes over time.

## Example Usage

```csharp
var hist = new History<int>(3);
hist.Add(1);
hist.Add(2);
hist.Add(3);

Console.WriteLine(hist.Count); // 3
Console.WriteLine(hist.OldestItem.ToString()); // 1
Console.WriteLine(hist.NewestItem.ToString()); // 3

list.Add(4); // 1 is removed, 2, 3, 4 are left
Console.WriteLine(list.Count.ToString()); // 3
Console.WriteLine(list.OldestItem.ToString()); // 2
Console.WriteLine(list.NewestItem.ToString()); // 4

```
