# SizeLimitedDictionary

A dictionary that is limited in maximum capacity and will automatically remove items when it reaches capacity.

## Example Usage

```csharp
var dict = new SizeLimitedDictionary<string, DateTime>(3);
dict.Add(1);
dict.Add(2);
dict.Add(3);

Console.WriteLine(dict.Count); // 3
Console.WriteLine(dict[0].ToString()); // 1
Console.WriteLine(dict[1].ToString()); // 2
Console.WriteLine(dict[2].ToString()); // 3

dict.Add(4); // 1 is removed, 2, 3, 4 are left
Console.WriteLine(dict.Count.ToString()); // 3
Console.WriteLine(dict[0].ToString()); // 2
Console.WriteLine(dict[1].ToString()); // 3
Console.WriteLine(dict[2].ToString()); // 4

var poppedItem = dict.AddAndReturn(5); // 2 is removed and returned, 3, 4, 5 are left
Console.WriteLine(poppedItem.ToString()); // 2
Console.WriteLine(dict.Count.ToString()); // 3
Console.WriteLine(dict[0].ToString()); // 3
Console.WriteLine(dict[1].ToString()); // 4
Console.WriteLine(dict[2].ToString()); // 5

```

## Possible Dictionary Full Actions

Different actions that can be taken when the dictionary is full, detailed below. These are set in the constructor.

| Action                                        | Description                                                                                           |
|-----------------------------------------------|-------------------------------------------------------------------------------------------------------|
| SizeLimitedCollectionFullAction.PopNewestItem | Removes the newest item from the dictionary, effectively replacing it with the new value being added. |
| SizeLimitedCollectionFullAction.PopOldestItem | Removes the oldest item from the dictionary.                                                          |
