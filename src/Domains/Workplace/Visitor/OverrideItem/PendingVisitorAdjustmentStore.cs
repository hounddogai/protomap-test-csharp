namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending visitor adjustments by code.</summary>
public interface IPendingVisitorAdjustmentStore
{
    PendingVisitorAdjustment? Find(string code);
    void Save(PendingVisitorAdjustment item);
    bool Remove(string code);
    List<PendingVisitorAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingVisitorAdjustmentStore(Dictionary<string, PendingVisitorAdjustment> items, List<string> order) : IPendingVisitorAdjustmentStore
{
    public Dictionary<string, PendingVisitorAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingVisitorAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingVisitorAdjustment item)
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

    public List<PendingVisitorAdjustment> List()
    {
        var outValue = new List<PendingVisitorAdjustment>();
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
public sealed class CachedPendingVisitorAdjustmentStore(MemoryPendingVisitorAdjustmentStore inner, PendingVisitorAdjustmentCache cache, int hits, int misses) : IPendingVisitorAdjustmentStore
{
    public MemoryPendingVisitorAdjustmentStore Inner { get; set; } = inner;
    public PendingVisitorAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingVisitorAdjustment? Find(string code)
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

    public void Save(PendingVisitorAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingVisitorAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending visitor adjustments. Empty fields match all.</summary>
public sealed record PendingVisitorAdjustmentQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingVisitorAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingVisitorAdjustment> Apply(List<PendingVisitorAdjustment> items)
    {
        var matched = Collections.Filter(items, (PendingVisitorAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending visitor adjustment store.</summary>
public static class PendingVisitorAdjustmentStore
{
    public static IPendingVisitorAdjustmentStore NewMemoryPendingVisitorAdjustmentStore()
    {
        return new MemoryPendingVisitorAdjustmentStore(
            new Dictionary<string, PendingVisitorAdjustment>(),
            new List<string>());
    }

    public static IPendingVisitorAdjustmentStore NewCachedPendingVisitorAdjustmentStore(MemoryPendingVisitorAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingVisitorAdjustmentStore(
            inner,
            PendingVisitorAdjustmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingVisitorAdjustment> FindPendingVisitorAdjustments(IPendingVisitorAdjustmentStore store, PendingVisitorAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
