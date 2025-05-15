# ConcurrentPool

A thread-safe pool of objects that can be used to hold objects and define a maximum amount. More efficient generally for
games as it can help reduce garbage collection.

Note: The pool can emit events when items are added or removed. This is useful for tracking changes in the pool.

## Example Usage

```csharp
var _dictionary = new ConcurrentPool<int>(2);
pool.Add(1);
pool.Add(2);
Console.WriteLine(pool[0].ToString()); // 1
Console.WriteLine(pool[1].ToString()); // 2

var poppedItem = pool.AddAndReturn(3);
Console.WriteLine(poppedItem.ToString()); // 1
Console.WriteLine(pool[0].ToString()); // 3
Console.WriteLine(pool[1].ToString()); // 2

```

## Possible Pool Full Actions

Different actions that can be taken when the pool is full, detailed below. These are set in the constructor.

| Action                        | Description                                                                                     |
|-------------------------------|-------------------------------------------------------------------------------------------------|
| PoolFullAction.PopNewestItem  | Removes the newest item from the pool, effectively replacing it with the new value being added. |
| PoolFullAction.PopOldestItem  | Removes the oldest item from the pool.                                                          |
| PoolFullAction.Ignore         | The new item is ignored and not added to the pool, and no items are removed from the pool.      |
| PoolFullAction.Grow           | The maximum size of the pool is increased to accommodate the new item.                          |
| PoolFullAction.ThrowException | An exception is thrown when the pool is full.                                                   |

## Events

The Pool class can emit events when items are added or removed. This is useful for tracking changes in the pool.

| Event                 | Description                                                            |
|-----------------------|------------------------------------------------------------------------|
| ClaimedSlotEvent      | Emitted when an item is added to the pool and is able to claim a slot. |
| ReleasedSlotEvent     | Emitted when an item is removed from the pool and releases its slot.   |
| SlotClaimFailureEvent | Emitted when an item fails to claim a slot.                            |
