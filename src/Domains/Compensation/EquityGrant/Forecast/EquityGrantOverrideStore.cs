namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant overrides by code.</summary>
public interface IEquityGrantOverrideStore
{
    EquityGrantOverride? Find(string code);
    void Save(EquityGrantOverride item);
    bool Remove(string code);
    List<EquityGrantOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantOverrideStore(Dictionary<string, EquityGrantOverride> items, List<string> order) : IEquityGrantOverrideStore
{
    public Dictionary<string, EquityGrantOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantOverride item)
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

    public List<EquityGrantOverride> List()
    {
        var outValue = new List<EquityGrantOverride>();
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
public sealed class CachedEquityGrantOverrideStore(MemoryEquityGrantOverrideStore inner, EquityGrantOverrideCache cache, int hits, int misses) : IEquityGrantOverrideStore
{
    public MemoryEquityGrantOverrideStore Inner { get; set; } = inner;
    public EquityGrantOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantOverride? Find(string code)
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

    public void Save(EquityGrantOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant overrides. Empty fields match all.</summary>
public sealed record EquityGrantOverrideQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantOverride> Apply(List<EquityGrantOverride> items)
    {
        var matched = Collections.Filter(items, (EquityGrantOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant override store.</summary>
public static class EquityGrantOverrideStore
{
    public static IEquityGrantOverrideStore NewMemoryEquityGrantOverrideStore()
    {
        return new MemoryEquityGrantOverrideStore(new Dictionary<string, EquityGrantOverride>(), new List<string>());
    }

    public static IEquityGrantOverrideStore NewCachedEquityGrantOverrideStore(MemoryEquityGrantOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantOverrideStore(inner, EquityGrantOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EquityGrantOverride> FindEquityGrantOverrides(IEquityGrantOverrideStore store, EquityGrantOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
