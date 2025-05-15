# LockFreeStack

A lock-free stack (First-in, Last-out) implementation

## Example Usage

```csharp
var stack = new LockFreeStack<int>();
stack.Push(1);
stack.Push(2);
stack.Push(3);

Console.WriteLine(stack.Count); // 3
Console.WriteLine(stack.Pop().ToString()); // 3
Console.WriteLine(stack.Pop().ToString()); // 2
Console.WriteLine(stack.Pop().ToString()); // 1
Console.WriteLine(stack.Count); // 0

```
