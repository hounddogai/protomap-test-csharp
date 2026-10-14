namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment snapshots by code.</summary>
public interface IAdjustmentSnapshotStore
{
    AdjustmentSnapshot? Find(string code);
    void Save(AdjustmentSnapshot item);
    bool Remove(string code);
    List<AdjustmentSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentSnapshotStore(Dictionary<string, AdjustmentSnapshot> items, List<string> order) : IAdjustmentSnapshotStore
{
    public Dictionary<string, AdjustmentSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentSnapshot item)
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

    public List<AdjustmentSnapshot> List()
    {
        var outValue = new List<AdjustmentSnapshot>();
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
public sealed class CachedAdjustmentSnapshotStore(MemoryAdjustmentSnapshotStore inner, AdjustmentSnapshotCache cache, int hits, int misses) : IAdjustmentSnapshotStore
{
    public MemoryAdjustmentSnapshotStore Inner { get; set; } = inner;
    public AdjustmentSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentSnapshot? Find(string code)
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

    public void Save(AdjustmentSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment snapshots. Empty fields match all.</summary>
public sealed record AdjustmentSnapshotQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentSnapshot> Apply(List<AdjustmentSnapshot> items)
    {
        var matched = Collections.Filter(items, (AdjustmentSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment snapshot store.</summary>
public static class AdjustmentSnapshotStore
{
    public static IAdjustmentSnapshotStore NewMemoryAdjustmentSnapshotStore()
    {
        return new MemoryAdjustmentSnapshotStore(new Dictionary<string, AdjustmentSnapshot>(), new List<string>());
    }

    public static IAdjustmentSnapshotStore NewCachedAdjustmentSnapshotStore(MemoryAdjustmentSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentSnapshotStore(inner, AdjustmentSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentSnapshot> FindAdjustmentSnapshots(IAdjustmentSnapshotStore store, AdjustmentSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
