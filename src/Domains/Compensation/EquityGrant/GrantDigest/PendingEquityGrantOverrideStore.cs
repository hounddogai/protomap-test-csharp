namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending equity grant overrides by code.</summary>
public interface IPendingEquityGrantOverrideStore
{
    PendingEquityGrantOverride? Find(string code);
    void Save(PendingEquityGrantOverride item);
    bool Remove(string code);
    List<PendingEquityGrantOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingEquityGrantOverrideStore(Dictionary<string, PendingEquityGrantOverride> items, List<string> order) : IPendingEquityGrantOverrideStore
{
    public Dictionary<string, PendingEquityGrantOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingEquityGrantOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingEquityGrantOverride item)
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

    public List<PendingEquityGrantOverride> List()
    {
        var outValue = new List<PendingEquityGrantOverride>();
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
public sealed class CachedPendingEquityGrantOverrideStore(MemoryPendingEquityGrantOverrideStore inner, PendingEquityGrantOverrideCache cache, int hits, int misses) : IPendingEquityGrantOverrideStore
{
    public MemoryPendingEquityGrantOverrideStore Inner { get; set; } = inner;
    public PendingEquityGrantOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingEquityGrantOverride? Find(string code)
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

    public void Save(PendingEquityGrantOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingEquityGrantOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending equity grant overrides. Empty fields match all.</summary>
public sealed record PendingEquityGrantOverrideQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingEquityGrantOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<PendingEquityGrantOverride> Apply(List<PendingEquityGrantOverride> items)
    {
        var matched = Collections.Filter(items, (PendingEquityGrantOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending equity grant override store.</summary>
public static class PendingEquityGrantOverrideStore
{
    public static IPendingEquityGrantOverrideStore NewMemoryPendingEquityGrantOverrideStore()
    {
        return new MemoryPendingEquityGrantOverrideStore(
            new Dictionary<string, PendingEquityGrantOverride>(),
            new List<string>());
    }

    public static IPendingEquityGrantOverrideStore NewCachedPendingEquityGrantOverrideStore(MemoryPendingEquityGrantOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPendingEquityGrantOverrideStore(
            inner,
            PendingEquityGrantOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingEquityGrantOverride> FindPendingEquityGrantOverrides(IPendingEquityGrantOverrideStore store, PendingEquityGrantOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
