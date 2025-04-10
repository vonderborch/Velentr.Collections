using System.Collections;
using System.Diagnostics;
using Velentr.Collections.CollectionActions;

namespace Velentr.Collections;

[DebuggerDisplay("Count = {Count}, Current Position = {CurrentPosition}, Can Undo = {CanUndo}, Can Redo = {CanRedo}")]
public class History<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
    private readonly SizeLimitedList<T> list;
    
    public History(int maxHistoryItems = 32)
    {
        this.list = new SizeLimitedList<T>(maxHistoryItems, SizeLimitedCollectionFullAction.PopOldestItem);
    }
    
    public int CurrentPosition { get; private set; }
    
    public int MaxHistoryIndex => (int) this.list.Count - 1;
    
    public bool CanUndo => this.CurrentPosition != 0;
    
    public bool CanRedo => this.CurrentPosition != this.MaxHistoryIndex;
}
