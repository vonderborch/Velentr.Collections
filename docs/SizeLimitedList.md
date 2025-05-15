# SizeLimitedList

A list that is limited in maximum capacity and will automatically remove items when it reaches capacity.

## Example Usage

```csharp
var list = new SizeLimitedList<int>(3);
list.Add(1);
list.Add(2);
list.Add(3);

Console.WriteLine(list.Count); // 3
Console.WriteLine(list[0].ToString()); // 1
Console.WriteLine(list[1].ToString()); // 2
Console.WriteLine(list[2].ToString()); // 3

list.Add(4); // 1 is removed, 2, 3, 4 are left
Console.WriteLine(list.Count.ToString()); // 3
Console.WriteLine(list[0].ToString()); // 2
Console.WriteLine(list[1].ToString()); // 3
Console.WriteLine(list[2].ToString()); // 4

var poppedItem = list.AddAndReturn(5); // 2 is removed and returned, 3, 4, 5 are left
Console.WriteLine(poppedItem.ToString()); // 2
Console.WriteLine(list.Count.ToString()); // 3
Console.WriteLine(list[0].ToString()); // 3
Console.WriteLine(list[1].ToString()); // 4
Console.WriteLine(list[2].ToString()); // 5

```

## Possible List Full Actions

Different actions that can be taken when the list is full, detailed below. These are set in the constructor.

| Action                                        | Description                                                                                     |
|-----------------------------------------------|-------------------------------------------------------------------------------------------------|
| SizeLimitedCollectionFullAction.PopNewestItem | Removes the newest item from the list, effectively replacing it with the new value being added. |
| SizeLimitedCollectionFullAction.PopOldestItem | Removes the oldest item from the list.                                                          |

