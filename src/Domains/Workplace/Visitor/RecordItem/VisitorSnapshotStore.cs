namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor snapshots by code.</summary>
public interface IVisitorSnapshotStore
{
    VisitorSnapshot? Find(string code);
    void Save(VisitorSnapshot item);
    bool Remove(string code);
    List<VisitorSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorSnapshotStore(Dictionary<string, VisitorSnapshot> items, List<string> order) : IVisitorSnapshotStore
{
    public Dictionary<string, VisitorSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorSnapshot item)
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

    public List<VisitorSnapshot> List()
    {
        var outValue = new List<VisitorSnapshot>();
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
public sealed class CachedVisitorSnapshotStore(MemoryVisitorSnapshotStore inner, VisitorSnapshotCache cache, int hits, int misses) : IVisitorSnapshotStore
{
    public MemoryVisitorSnapshotStore Inner { get; set; } = inner;
    public VisitorSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorSnapshot? Find(string code)
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

    public void Save(VisitorSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor snapshots. Empty fields match all.</summary>
public sealed record VisitorSnapshotQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorSnapshot> Apply(List<VisitorSnapshot> items)
    {
        var matched = Collections.Filter(items, (VisitorSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor snapshot store.</summary>
public static class VisitorSnapshotStore
{
    public static IVisitorSnapshotStore NewMemoryVisitorSnapshotStore()
    {
        return new MemoryVisitorSnapshotStore(new Dictionary<string, VisitorSnapshot>(), new List<string>());
    }

    public static IVisitorSnapshotStore NewCachedVisitorSnapshotStore(MemoryVisitorSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorSnapshotStore(inner, VisitorSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VisitorSnapshot> FindVisitorSnapshots(IVisitorSnapshotStore store, VisitorSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
