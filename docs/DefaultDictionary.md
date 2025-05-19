# DefaultDictionary

A dictionary that will return a default value if the key is not found.

## Example Usage

DefaultDictionary has two options for default values:
- Simple: A static default value that is returned when the key is not found.
- Expression: A function that is called to generate a default value when the key is not found.

### Simple Default Value

```csharp
var _dictionary = new DefaultDictionary<int, string>("42");
_dictionary.Add(1, "One");
_dictionary.Add(2, "Two");
Console.WriteLine(_dictionary[1].ToString()); // One
Console.WriteLine(_dictionary[2]); // Two
Console.WriteLine(_dictionary[3].ToString()); // 42

_dictionary[3] = "Three";
Console.WriteLine(_dictionary[3].ToString()); // Three

```

### Expression Default Value

```csharp
var _dictionary = new DefaultDictionary<int, List<string>>(() => new List<string>() { "42" } );
_dictionary.Add(1, new List<string> { "One" });
_dictionary.Add(2, new List<string> { "Two" });

Console.WriteLine(_dictionary[1].ToString()); // ["One"]
Console.WriteLine(_dictionary[2].ToString()); // ["Two"]
Console.WriteLine(_dictionary[3].ToString()); // ["42"]

_dictionary[3].Add("Three");
Console.WriteLine(_dictionary[3].ToString()); // ["42", "Three"]

```
