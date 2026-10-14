namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement adjustments by code.</summary>
public interface IEngagementAdjustmentStore
{
    EngagementAdjustment? Find(string code);
    void Save(EngagementAdjustment item);
    bool Remove(string code);
    List<EngagementAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementAdjustmentStore(Dictionary<string, EngagementAdjustment> items, List<string> order) : IEngagementAdjustmentStore
{
    public Dictionary<string, EngagementAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementAdjustment item)
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

    public List<EngagementAdjustment> List()
    {
        var outValue = new List<EngagementAdjustment>();
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
public sealed class CachedEngagementAdjustmentStore(MemoryEngagementAdjustmentStore inner, EngagementAdjustmentCache cache, int hits, int misses) : IEngagementAdjustmentStore
{
    public MemoryEngagementAdjustmentStore Inner { get; set; } = inner;
    public EngagementAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementAdjustment? Find(string code)
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

    public void Save(EngagementAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement adjustments. Empty fields match all.</summary>
public sealed record EngagementAdjustmentQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementAdjustment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementAdjustment> Apply(List<EngagementAdjustment> items)
    {
        var matched = Collections.Filter(items, (EngagementAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement adjustment store.</summary>
public static class EngagementAdjustmentStore
{
    public static IEngagementAdjustmentStore NewMemoryEngagementAdjustmentStore()
    {
        return new MemoryEngagementAdjustmentStore(
            new Dictionary<string, EngagementAdjustment>(),
            new List<string>());
    }

    public static IEngagementAdjustmentStore NewCachedEngagementAdjustmentStore(MemoryEngagementAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementAdjustmentStore(inner, EngagementAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EngagementAdjustment> FindEngagementAdjustments(IEngagementAdjustmentStore store, EngagementAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
