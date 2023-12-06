using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Avalonia;

namespace NexusMods.App.UI.WorkspaceSystem;

public readonly struct WorkspaceGridState :
    IImmutableSet<PanelGridState>,
    IReadOnlyList<PanelGridState>
{
    public readonly ImmutableSortedSet<PanelGridState> Inner;
    public readonly bool IsHorizontal;

    public WorkspaceGridState(ImmutableSortedSet<PanelGridState> inner, bool isHorizontal)
    {
        Inner = inner.WithComparer(PanelGridStateComparer.Instance);
        IsHorizontal = isHorizontal;
    }

    public static WorkspaceGridState From(IEnumerable<KeyValuePair<PanelId, Rect>> values, bool isHorizontal)
    {
        return new WorkspaceGridState(
            inner: values.Select(kv => new PanelGridState(kv.Key, kv.Value)).ToImmutableSortedSet(PanelGridStateComparer.Instance),
            isHorizontal
        );
    }

    public static WorkspaceGridState From(IEnumerable<IPanelViewModel> panels, bool isHorizontal)
    {
        return new WorkspaceGridState(
            inner: panels.Select(panel => new PanelGridState(panel.Id, panel.LogicalBounds)).ToImmutableSortedSet(PanelGridStateComparer.Instance),
            isHorizontal
        );
    }

    public static WorkspaceGridState From(IEnumerable<PanelGridState> panels, bool isHorizontal)
    {
        return new WorkspaceGridState(
            inner: panels.ToImmutableSortedSet(PanelGridStateComparer.Instance),
            isHorizontal
        );
    }

    public static WorkspaceGridState Empty(bool isHorizontal) => new(ImmutableSortedSet<PanelGridState>.Empty, isHorizontal);

    private WorkspaceGridState WithInner(ImmutableSortedSet<PanelGridState> inner)
    {
        return new WorkspaceGridState(inner, IsHorizontal);
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    public PanelGridState this[PanelId id]
    {
        get
        {
            foreach (var panel in Inner)
            {
                if (panel.Id == id) return panel;
            }

            throw new KeyNotFoundException();
        }
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    public bool TryGetValue(PanelId id, out PanelGridState panel)
    {
        foreach (var item in Inner)
        {
            if (item.Id != id) continue;
            panel = item;
            return true;
        }

        panel = default;
        return false;
    }

    [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global")]
    public WorkspaceGridState UnionById(PanelGridState[] other)
    {
        var builder = Inner.ToBuilder();
        foreach (var panelToAdd in other)
        {
            if (TryGetValue(panelToAdd.Id, out var existingPanel))
            {
                builder.Remove(existingPanel);
            }

            builder.Add(panelToAdd);
        }

        return WithInner(builder.ToImmutable());
    }

    public AdjacentPanelEnumerator EnumerateAdjacentPanels(PanelGridState anchor, bool includeAnchor) => new(this, anchor, includeAnchor);

    [Flags]
    public enum AdjacencyKind : byte
    {
        None = 0,
        SameRow = 1 << 0,
        SameColumn = 1 << 1
    }

    public record struct AdjacentPanel(PanelGridState Panel, AdjacencyKind Kind);

    public struct AdjacentPanelEnumerator : IEnumerator<AdjacentPanel>
    {
        private ImmutableSortedSet<PanelGridState>.Enumerator _enumerator;
        private readonly PanelGridState _anchor;
        private readonly bool _includeAnchor;

        internal AdjacentPanelEnumerator(WorkspaceGridState parent, PanelGridState anchor, bool includeAnchor)
        {
            _enumerator = parent.GetEnumerator();
            _anchor = anchor;
            _includeAnchor = includeAnchor;
        }

        public AdjacentPanel Current { get; private set; }
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            while (true)
            {
                if (!_enumerator.MoveNext()) return false;

                var other = _enumerator.Current;
                if (!_includeAnchor && other.Id == _anchor.Id) continue;

                var (anchorRect, otherRect) = (_anchor.Rect, other.Rect);
                var flags = AdjacencyKind.None;

                // same column
                // | a | x |  | b | x |
                // | b | x |  | a | x |
                if (otherRect.Left.IsGreaterThanOrCloseTo(anchorRect.Left) && otherRect.Right.IsLessThanOrCloseTo(anchorRect.Right))
                {
                    if (otherRect.Top.IsCloseTo(anchorRect.Bottom) || otherRect.Bottom.IsCloseTo(anchorRect.Top))
                    {
                        flags |= AdjacencyKind.SameColumn;
                    }
                }

                // same row
                // | a | b |  | b | a |  | a | b |
                // | x | x |  | x | x |  | a | c |
                if (otherRect.Top.IsGreaterThanOrCloseTo(anchorRect.Top) && otherRect.Bottom.IsLessThanOrCloseTo(anchorRect.Bottom))
                {
                    if (otherRect.Left.IsCloseTo(anchorRect.Right) || otherRect.Right.IsCloseTo(anchorRect.Left))
                    {
                        flags |= AdjacencyKind.SameRow;
                    }
                }

                if (flags == AdjacencyKind.None) continue;

                Current = new AdjacentPanel(other, flags);
                return true;
            }
        }

        public void Reset() => _enumerator.Reset();
        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    #region Interface Implementations

    public ImmutableSortedSet<PanelGridState>.Enumerator GetEnumerator() => Inner.GetEnumerator();
    IEnumerator<PanelGridState> IEnumerable<PanelGridState>.GetEnumerator() => Inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Inner.GetEnumerator();

    public int Count => Inner.Count;
    public bool Contains(PanelGridState value) => Inner.Contains(value);

    public bool IsProperSubsetOf(IEnumerable<PanelGridState> other) => Inner.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<PanelGridState> other) => Inner.IsProperSubsetOf(other);
    public bool IsSubsetOf(IEnumerable<PanelGridState> other) => Inner.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<PanelGridState> other) => Inner.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<PanelGridState> other) => Inner.Overlaps(other);
    public bool SetEquals(IEnumerable<PanelGridState> other) => Inner.SetEquals(other);

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.Add(PanelGridState value) => Inner.Add(value);
    public WorkspaceGridState Add(PanelGridState value) => WithInner(Inner.Add(value));

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.Clear() => Inner.Clear();
    public WorkspaceGridState Clear() => WithInner(Inner.Clear());

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.Except(IEnumerable<PanelGridState> other) => Inner.Except(other);
    public WorkspaceGridState Except(IEnumerable<PanelGridState> other) => WithInner(Inner.Except(other));

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.Intersect(IEnumerable<PanelGridState> other) => Inner.Intersect(other);
    public WorkspaceGridState Intersect(IEnumerable<PanelGridState> other) => WithInner(Inner.Intersect(other));

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.Remove(PanelGridState value) => Inner.Remove(value);
    public WorkspaceGridState Remove(PanelGridState value) => WithInner(Inner.Remove(value));

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.SymmetricExcept(IEnumerable<PanelGridState> other) => Inner.SymmetricExcept(other);
    public WorkspaceGridState SymmetricExcept(IEnumerable<PanelGridState> other) => WithInner(Inner.SymmetricExcept(other));

    bool IImmutableSet<PanelGridState>.TryGetValue(PanelGridState equalValue, out PanelGridState actualValue) => Inner.TryGetValue(equalValue, out actualValue);

    IImmutableSet<PanelGridState> IImmutableSet<PanelGridState>.Union(IEnumerable<PanelGridState> other) => Inner.Union(other);
    public WorkspaceGridState Union(IEnumerable<PanelGridState> other) => WithInner(Inner.Union(other));

    public int IndexOf(PanelGridState item) => Inner.IndexOf(item);

    public PanelGridState this[int index] => Inner[index];

    #endregion
}
