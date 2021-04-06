# Collections.Net
A variety of helpful collections.

# Installation
A nuget package is available: [Collections.Net](https://www.nuget.org/packages/Collections.Net/)

# Available Collections
- Concurrent: _Collections under this namespace utilize .NET Concurrent collections internally_
  - ConcurrentLimitedPriorityQueue: A priority queue that utilizes a list of available priorities based on a `PriorityConverter`
  - ConcurrentPool: A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection
  - ConcurrentPriorityQueue: A priority queue that has priorities available based on the `QueuePriority` enum
- Lock-Free: _Collections under this namespace utilize custom lock-free base collections_
  - LockFreeLimitedPriorityQueue: A priority queue that utilizes a list of available priorities based on a `PriorityConverter`
  - LockFreePool: A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection
  - LockFreePriorityQueue: A priority queue that has priorities available based on the `QueuePriority` enum
  - LockFreeQueue: A lock-free Queue implementation
  - LockFreeStack: A lock-free Stack implementation
- Bank: A Collection that combines functionality of a dictionary and a list.
- Cache: A Thread-Safe and Lock-Free dictionary optimized for reads

# Future Plans
See list of issues under the Milestones: https://github.com/vonderborch/Velentr.Input/milestones
