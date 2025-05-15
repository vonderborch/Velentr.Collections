# BiDirectionalDictionary

A dictionary where you can access the value for a given key, and the key for a given value.

## Example Usage

```csharp
var _dictionary = new BiDirectionalDictionary<int, string>();
_dictionary.Add(1, "One");
_dictionary.Add(2, "Two");
Console.WriteLine(_dictionary[1].ToString()); // One
Console.WriteLine(_dictionary["Two"]); // 2

```
