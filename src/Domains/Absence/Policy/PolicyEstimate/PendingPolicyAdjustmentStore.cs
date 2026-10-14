namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending policy adjustments by code.</summary>
public interface IPendingPolicyAdjustmentStore
{
    PendingPolicyAdjustment? Find(string code);
    void Save(PendingPolicyAdjustment item);
    bool Remove(string code);
    List<PendingPolicyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPolicyAdjustmentStore(Dictionary<string, PendingPolicyAdjustment> items, List<string> order) : IPendingPolicyAdjustmentStore
{
    public Dictionary<string, PendingPolicyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPolicyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPolicyAdjustment item)
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

    public List<PendingPolicyAdjustment> List()
    {
        var outValue = new List<PendingPolicyAdjustment>();
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
public sealed class CachedPendingPolicyAdjustmentStore(MemoryPendingPolicyAdjustmentStore inner, PendingPolicyAdjustmentCache cache, int hits, int misses) : IPendingPolicyAdjustmentStore
{
    public MemoryPendingPolicyAdjustmentStore Inner { get; set; } = inner;
    public PendingPolicyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPolicyAdjustment? Find(string code)
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

    public void Save(PendingPolicyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPolicyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending policy adjustments. Empty fields match all.</summary>
public sealed record PendingPolicyAdjustmentQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPolicyAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPolicyAdjustment> Apply(List<PendingPolicyAdjustment> items)
    {
        var matched = Collections.Filter(items, (PendingPolicyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending policy adjustment store.</summary>
public static class PendingPolicyAdjustmentStore
{
    public static IPendingPolicyAdjustmentStore NewMemoryPendingPolicyAdjustmentStore()
    {
        return new MemoryPendingPolicyAdjustmentStore(
            new Dictionary<string, PendingPolicyAdjustment>(),
            new List<string>());
    }

    public static IPendingPolicyAdjustmentStore NewCachedPendingPolicyAdjustmentStore(MemoryPendingPolicyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPolicyAdjustmentStore(
            inner,
            PendingPolicyAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingPolicyAdjustment> FindPendingPolicyAdjustments(IPendingPolicyAdjustmentStore store, PendingPolicyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
