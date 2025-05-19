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

| Collection                | Description                                                                                                                                                             | Min Supported Library Version | Documentation                                   |
|---------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------|-------------------------------------------------|
| BiDirectionalDictionary   | A dictionary where you can access the value for a given key, and the key for a given value                                                                              | 1.2.0                         | [Documentation](docs/BiDirectionalDictionary)   |
| DefaultDictionary         | A dictionary that will return a default value if the key is not found.                                                                                                  | 3.1.0                         | [Documentation](docs/DefaultDictionary)         |
| SizeLimitedList           | A list that is limited in maximum capacity and will automatically remove items when it reaches capacity                                                                 | 1.1.0                         | [Documentation](docs/SizeLimitedList)           |
| SizeLimitedDictionary     | A dictionary that is limited in maximum capacity and will automatically remove items when it reaches capacity                                                           | 1.1.0                         | [Documentation](docs/SizeLimitedDictionary)     |
| Pool                      | A pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection             | 1.0.0                         | [Documentation](docs/Pool)                      |
| History                   | A collection that keeps track of the history of items added and removed from it. It can be used to track changes over time.                                             | 1.0.0                         | [Documentation](docs/History)                   |
| ConcurrentPriorityQueue   | A thread-safe priority queue where the item with the lowest priority is returned on dequeue                                                                             | 1.0.0                         | [Documentation](docs/ConcurrentPriorityQueue)   |
| ConcurrentPool            | A thread-safe pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for games as it can help reduce garbage collection | 1.0.0                         | [Documentation](docs/ConcurrentPool)            |
| ConcurrentSizeLimitedList | A thread-safe list that is limited in maximum capacity and automatically removes excess items when it is full                                                           | 1.0.0                         | [Documentation](docs/ConcurrentSizeLimitedList) |
| LockFreePriorityQueue     | A lock-free priority queue where the item with the lowest priority is returned on dequeue                                                                               | 1.0.0                         | [Documentation](docs/LockFreePriorityQueue)     |
| LockFreeList              | A lock-free linked list implementation                                                                                                                                  | 1.0.0                         | [Documentation](docs/LockFreeList)              |
| LockFreeQueue             | A lock-free queue implementation                                                                                                                                        | 1.0.0                         | [Documentation](docs/LockFreeQueue)             |
| LockFreeStack             | A lock-free stack implementation                                                                                                                                        | 1.0.0                         | [Documentation](docs/LockFreeStack)             |

**_NOTES:_**

- **Collections.Concurrent collections**: Collections under this namespace utilize .NET Concurrent collections
- **Lock-Free**: Collections under this namespace utilize custom lock-free base collections

## Future Collections

- **LockFreeArrayList**: A lock-free array list implementation
- **LockFreePool**: A lock-free pool implementation. Previously was implemented in this library, but removed due to
  testing results and need for heavy refactoring.

## Deprecated Collections

| Collection                     | Deprecation Reason                                                  | Deprecation Library Version | Max Available Library Version |
|--------------------------------|---------------------------------------------------------------------|-----------------------------|-------------------------------|
| DictionaryCache                | Use .NET ImmutableDictionary instead.                               | 2.0.2                       | 2.0.2                         |
| OrderedDictionary              | Use .NET OrderedDictionary instead.                                 | 2.0.2                       | 2.0.2                         |
| SizeLimitedPool                | Use Velentr.Collections.Pool instead.                               | 2.0.2                       | 2.0.2                         |
| HistoryCollection              | Use Velentr.Collections.History instead.                            | 2.0.2                       | 2.0.2                         |
| LockFreePool                   | In need of heavy refactoring and testing.                           | 2.0.2                       | 2.0.2                         |
| LockFreeLimitedPriorityQueue   | Use Velentr.Collections.LockFree.LockFreePriorityQueue instead.     | 2.0.2                       | 2.0.2                         |
| ConcurrentLimitedPriorityQueue | Use Velentr.Collections.Concurrent.ConcurrentPriorityQueue instead. | 2.0.2                       | 2.0.2                         |

## Development

1. Clone or fork the repo
2. Create a new branch
3. Code!
4. Push your changes and open a PR
5. Once approved, they'll be merged in
6. Profit!

## Future Plans

See list of issues under the Milestones: https://github.com/vonderborch/Velentr.Collections/milestones
