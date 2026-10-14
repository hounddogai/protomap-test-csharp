namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span snapshots by code.</summary>
public interface ISpanSnapshotStore
{
    SpanSnapshot? Find(string code);
    void Save(SpanSnapshot item);
    bool Remove(string code);
    List<SpanSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanSnapshotStore(Dictionary<string, SpanSnapshot> items, List<string> order) : ISpanSnapshotStore
{
    public Dictionary<string, SpanSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanSnapshot item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<SpanSnapshot> List()
    {
        var outValue = new List<SpanSnapshot>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedSpanSnapshotStore(MemorySpanSnapshotStore inner, SpanSnapshotCache cache, int hits, int misses) : ISpanSnapshotStore
{
    public MemorySpanSnapshotStore Inner { get; set; } = inner;
    public SpanSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanSnapshot? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(SpanSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span snapshots. Empty fields match all.</summary>
public sealed record SpanSnapshotQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SpanSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanSnapshot> Apply(List<SpanSnapshot> items)
    {
        var matched = Collections.Filter(items, (SpanSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span snapshot store.</summary>
public static class SpanSnapshotStore
{
    public static ISpanSnapshotStore NewMemorySpanSnapshotStore()
    {
        return new MemorySpanSnapshotStore(new Dictionary<string, SpanSnapshot>(), new List<string>());
    }

    public static ISpanSnapshotStore NewCachedSpanSnapshotStore(MemorySpanSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedSpanSnapshotStore(inner, SpanSnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanSnapshot> FindSpanSnapshots(ISpanSnapshotStore store, SpanSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
