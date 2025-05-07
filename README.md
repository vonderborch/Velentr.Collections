# Velentr.Collections

![Logo](https://raw.githubusercontent.com/vonderborch/Velentr.Collections/refs/heads/main/logo.png)

A variety of helpful collections, with a focus on thread-safety.

## Installation

### Nuget

[![NuGet version (Velentr.Collections)](https://img.shields.io/nuget/v/Velentr.Collections.svg?style=flat-square)](https://www.nuget.org/packages/Velentr.Collections/)

The recommended installation approach is to use the available nuget
package: [Velentr.Collections](https://www.nuget.org/packages/Velentr.Collections/)

### Clone

Alternatively, you can clone this repo and reference the Velentr.Collections project in your project.

## Available Collections

### Collections

#### Core Collections

#### 

| Collection                        | Description                                                                                                                                                             | Min Supported Library Version | Documentation                             |
|-----------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------|-------------------------------------------|
| BiDirectionalDictionary           | A dictionary where you can access the value for a given key, and the key for a given value                                                                              | 1.2.0                         | [Documentation](#BiDirectionalDictionary) |
| ConcurrentBiDirectionalDictionary | A thread-safe dictionary where you can access the value for a given key, and the key for a given value                                                                  | 1.2.0                         | [Documentation](#BiDirectionalDictionary) |
| LockFreeBiDirectionalDictionary   | A lock-free dictionary where you can access the value for a given key, and the key for a given value                                                                    | 1.2.0                         | [Documentation](#BiDirectionalDictionary) |
| SizeLimitedList                   | A list that is limited in maximum capacity and will automatically remove items when it reaches capacity                                                                 | 1.1.0                         | [Documentation](#SizeLimitedList)         |
| SizeLimitedDictionary             | A dictionary that is limited in maximum capacity and will automatically remove items when it reaches capacity                                                           | 1.1.0                         | [Documentation](#SizeLimitedDictionary)   |
| Pool                              | A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection             | 1.0.0                         | [Documentation](#Pool)                    |
| ConcurrentPriorityQueue           | A thread-safe priority queue where the item with the lowest priority is returned on dequeue                                                                             | 1.0.0                         | [Documentation](#ConcurrentPriorityQueue) |
| ConcurrentPool                    | A thread-safe pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection | 1.0.0                         | [Documentation](#ConcurrentPool)          |
| LockFreePriorityQueue             | A lock-free priority queue where the item with the lowest priority is returned on dequeue                                                                               | 1.0.0                         | [Documentation](#LockFreePriorityQueue)   |
| LockFreePool                      | A lock-free pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection   | 1.0.0                         | [Documentation](#LockFreePool)            |
| LockFreeQueue                     | A lock-free queue implementation                                                                                                                                        | 1.0.0                         | [Documentation](#LockFreeQueue)           |
| LockFreeStack                     | A lock-free stack implementation                                                                                                                                        | 1.0.0                         | [Documentation](#LockFreeStack)           |


## Collection Documentation

### BiDirectionalDictionary

A dictionary where you can access the value for a given key, and the key for a given value.

Example usage:

```csharp
var _dictionary = new BiDirectionalDictionary<int, string>();
_dictionary.Add(1, "One");
_dictionary.Add(2, "Two");
Console.WriteLine(_dictionary[1].ToString()); // One
Console.WriteLine(_dictionary["Two"]); // 2

```

### SizeLimitedList

A list that is limited in maximum capacity and will automatically remove items when it reaches capacity.

Example usage:

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

#### Possible List Full Actions

Different actions that can be taken when the list is full, detailed below. These are set in the constructor.

| Action                                        | Description                                                                                     |
|-----------------------------------------------|-------------------------------------------------------------------------------------------------|
| SizeLimitedCollectionFullAction.PopNewestItem | Removes the newest item from the list, effectively replacing it with the new value being added. |
| SizeLimitedCollectionFullAction.PopOldestItem | Removes the oldest item from the list.                                                          |

### SizeLimitedDictionary

A dictionary that is limited in maximum capacity and will automatically remove items when it reaches capacity.

Example usage:

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

#### Possible Dictionary Full Actions

Different actions that can be taken when the dictionary is full, detailed below. These are set in the constructor.

| Action                                        | Description                                                                                           |
|-----------------------------------------------|-------------------------------------------------------------------------------------------------------|
| SizeLimitedCollectionFullAction.PopNewestItem | Removes the newest item from the dictionary, effectively replacing it with the new value being added. |
| SizeLimitedCollectionFullAction.PopOldestItem | Removes the oldest item from the dictionary.                                                          |

### Pool

A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it
can help reduce garbage collection.

Note: The pool can emit events when items are added or removed. This is useful for tracking changes in the pool.

Example usage:

```csharp
var _dictionary = new Pool<int>(2);
pool.Add(1);
pool.Add(2);
Console.WriteLine(pool[0].ToString()); // 1
Console.WriteLine(pool[1].ToString()); // 2

var poppedItem = pool.AddAndReturn(3);
Console.WriteLine(poppedItem.ToString()); // 1
Console.WriteLine(pool[0].ToString()); // 3
Console.WriteLine(pool[1].ToString()); // 2

```

#### Possible Pool Full Actions

Different actions that can be taken when the pool is full, detailed below. These are set in the constructor.

| Action                        | Description                                                                                     |
|-------------------------------|-------------------------------------------------------------------------------------------------|
| PoolFullAction.PopNewestItem  | Removes the newest item from the pool, effectively replacing it with the new value being added. |
| PoolFullAction.PopOldestItem  | Removes the oldest item from the pool.                                                          |
| PoolFullAction.Ignore         | The new item is ignored and not added to the pool, and no items are removed from the pool.      |
| PoolFullAction.Grow           | The maximum size of the pool is increased to accommodate the new item.                          |
| PoolFullAction.ThrowException | An exception is thrown when the pool is full.                                                   |

#### Events

The Pool class can emit events when items are added or removed. This is useful for tracking changes in the pool.

| Event                 | Description                                                            |
|-----------------------|------------------------------------------------------------------------|
| ClaimedSlotEvent      | Emitted when an item is added to the pool and is able to claim a slot. |
| ReleasedSlotEvent     | Emitted when an item is removed from the pool and releases its slot.   |
| SlotClaimFailureEvent | Emitted when an item fails to claim a slot.                            |

## Deprecated Collections

| Collection        | Deprecation Reason                       | Deprecation Library Version | Max Available Library Version |
|-------------------|------------------------------------------|-----------------------------|-------------------------------|
| DictionaryCache   | Use .NET ImmutableDictionary instead.    | 2.0.2                       | 2.0.2                         |
| OrderedDictionary | Use .NET OrderedDictionary instead.      | 2.0.2                       | 2.0.2                         |
| SizeLimitedPool   | Use Velentr.Collections.Pool instead.    | 2.0.2                       | 2.0.2                         |
| HistoryCollection | Use Velentr.Collections.History instead. | 2.0.2                       | 2.0.2                         |

## OLD Available Collections

| Namespace              | Collection                     | Description                                                                                                                                                 | Min Supported Library Version | Example Usage                                                                             |
|------------------------|--------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------|-------------------------------------------------------------------------------------------|
| Collections.Concurrent | ConcurrentLimitedPriorityQueue | A priority queue that utilizes a list of available priorities based on a `PriorityConverter`                                                                | 1.0.0                         | `var c = new ConcurrentLimitedPriorityQueue<int, string>(new StringPriorityConverter());` |
| Collections.Concurrent | ConcurrentPriorityQueue        | A priority queue that has priorities available based on the `QueuePriority` enum                                                                            | 1.0.0                         | `var c = new ConcurrentPriorityQueue<string>();`                                          |
| Collections.Concurrent | ConcurrentPool                 | A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection | 1.0.0                         | `var c = new ConcurrentPool<object>();`                                                   |
| Collections.LockFree   | LockFreeLimitedPriorityQueue   | A priority queue that utilizes a list of available priorities based on a `PriorityConverter`                                                                | 1.0.0                         | `var c = new LockFreeLimitedPriorityQueue<int, string>(new StringPriorityConverter());`   |
| Collections.LockFree   | LockFreePriorityQueue          | A priority queue that has priorities available based on the `QueuePriority` enum                                                                            | 1.0.0                         | `var c = new LockFreePriorityQueue<string>();`                                            |
| Collections.LockFree   | LockFreePool                   | A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection | 1.0.0                         | `var c = new LockFreePool<object>();`                                                     
| Collections.LockFree   | LockFreeQueue                  | A lock-free Queue implementation                                                                                                                            | 1.0.0                         | `var c = new LockFreeQueue<string>();`                                                    |
| Collections.LockFree   | LockFreeStack                  | A lock-free Stack implementation                                                                                                                            | 1.0.0                         | `var c = new LockFreeStack<string>();`                                                    |
| Collections            | OrderedDictionary              | A Collection that combines functionality of a dictionary and a list.                                                                                        | 1.1.0                         | `var c = new OrderedDictionary<string, List<string>>();`                                  |
| Collections            | SizeLimitedOrderedDictionary   | A Collection that combines functionality of a dictionary and a list and that is also limited in max capacity.                                               | 1.1.3                         | `var c = new OrderedDictionary<string, List<string>>();`                                  |
| Collections            | DictionaryCache                | A Thread-Safe and Lock-Free dictionary optimized for reads                                                                                                  | 1.1.0                         | `var c = new DictionaryCache<string, int>();`                                             |
| Collections            | SizeLimitedList                | A list that is limited in max capacity                                                                                                                      | 1.1.0                         | `var c = new SizeLimitedList<string>();`                                                  |
| Collections            | HistoryCollection              | A collection implementing undo and redo functionality                                                                                                       | 1.2.0                         | `var c = new HistoryCollection<string>();`                                                |
| Collections            | BiDirectionalDictionary        | A bi-directional dictionary, where you can access the value for a key/value pair using the key or vice-versa                                                | 1.2.0                         | `var c = new HistoryCollection<string>();`                                                |

**_NOTES:_**

- **Collections.Concurrent collections**: _Collections under this namespace utilize .NET Concurrent collections
  internally_
- **Lock-Free**: _Collections under this namespace utilize custom lock-free base collections_

## Development

1. Clone or fork the repo
2. Create a new branch
3. Code!
4. Push your changes and open a PR
5. Once approved, they'll be merged in
6. Profit!

## Future Plans

See list of issues under the Milestones: https://github.com/vonderborch/Velentr.Collections/milestones
