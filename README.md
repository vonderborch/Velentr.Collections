# Velentr.Collections
A variety of helpful collections.

# Installation
A nuget package is available: [Velentr.Collections](https://www.nuget.org/packages/Velentr.Collections/)

# Available Collections
Namespace | Collection | Description | Min Supported Library Version | Example Usage
--------- | ---------- | ----------- | ----------------------------- | -------------
Collections.Concurrent | ConcurrentLimitedPriorityQueue | A priority queue that utilizes a list of available priorities based on a `PriorityConverter` | 1.0.0 | `var c = new ConcurrentLimitedPriorityQueue<int, string>(new StringPriorityConverter());`
Collections.Concurrent | ConcurrentPriorityQueue | A priority queue that has priorities available based on the `QueuePriority` enum | 1.0.0 | `var c = new ConcurrentPriorityQueue<string>();`
Collections.Concurrent | ConcurrentPool | A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection | 1.0.0 | `var c = new ConcurrentPool<object>();`
Collections.LockFree | LockFreeLimitedPriorityQueue | A priority queue that utilizes a list of available priorities based on a `PriorityConverter` | 1.0.0 | `var c = new LockFreeLimitedPriorityQueue<int, string>(new StringPriorityConverter());`
Collections.LockFree | LockFreePriorityQueue | A priority queue that has priorities available based on the `QueuePriority` enum | 1.0.0 | `var c = new LockFreePriorityQueue<string>();`
Collections.LockFree | LockFreePool | A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection | 1.0.0 | `var c = new LockFreePool<object>();`
Collections.LockFree | LockFreeQueue | A lock-free Queue implementation | 1.0.0 | `var c = new LockFreeQueue<string>();`
Collections.LockFree | LockFreeStack | A lock-free Stack implementation | 1.0.0 | `var c = new LockFreeStack<string>();`
Collections | OrderedDictionary | A Collection that combines functionality of a dictionary and a list. | 1.1.0 | `var c = new OrderedDictionary<string, List<string>>();`
Collections | SizeLimitedOrderedDictionary | A Collection that combines functionality of a dictionary and a list and that is also limited in max capacity. | 1.1.3 | `var c = new OrderedDictionary<string, List<string>>();`
Collections | DictionaryCache | A Thread-Safe and Lock-Free dictionary optimized for reads | 1.1.0 | `var c = new DictionaryCache<string, int>();`
Collections | SizeLimitedList | A list that is limited in max capacity | 1.1.0 | `var c = new SizeLimitedList<string>();`

**_NOTES:_**
- **Collections.Concurrent collections**: _Collections under this namespace utilize .NET Concurrent collections internally_
- **Lock-Free**: _Collections under this namespace utilize custom lock-free base collections_

# Deprecated Collections
Collection | Description | Max Supported Library Version
---------- | ----------- | -----------------------------
Bank | A Collection that combines functionality of a dictionary and a list. Renamed to `OrderedDictionary` in 1.1.0. | 1.0.5
Cache | A Thread-Safe and Lock-Free dictionary optimized for reads. Renamed to `DictionaryCache` in 1.1.0. | 1.0.5



# Future Plans
See list of issues under the Milestones: https://github.com/vonderborch/Velentr.Collections/milestones
