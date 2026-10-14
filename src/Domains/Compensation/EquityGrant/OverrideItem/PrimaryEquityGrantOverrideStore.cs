namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary equity grant overrides by code.</summary>
public interface IPrimaryEquityGrantOverrideStore
{
    PrimaryEquityGrantOverride? Find(string code);
    void Save(PrimaryEquityGrantOverride item);
    bool Remove(string code);
    List<PrimaryEquityGrantOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryEquityGrantOverrideStore(Dictionary<string, PrimaryEquityGrantOverride> items, List<string> order) : IPrimaryEquityGrantOverrideStore
{
    public Dictionary<string, PrimaryEquityGrantOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryEquityGrantOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryEquityGrantOverride item)
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

    public List<PrimaryEquityGrantOverride> List()
    {
        var outValue = new List<PrimaryEquityGrantOverride>();
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
public sealed class CachedPrimaryEquityGrantOverrideStore(MemoryPrimaryEquityGrantOverrideStore inner, PrimaryEquityGrantOverrideCache cache, int hits, int misses) : IPrimaryEquityGrantOverrideStore
{
    public MemoryPrimaryEquityGrantOverrideStore Inner { get; set; } = inner;
    public PrimaryEquityGrantOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryEquityGrantOverride? Find(string code)
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

    public void Save(PrimaryEquityGrantOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryEquityGrantOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary equity grant overrides. Empty fields match all.</summary>
public sealed record PrimaryEquityGrantOverrideQuery(string Team, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryEquityGrantOverride item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryEquityGrantOverride> Apply(List<PrimaryEquityGrantOverride> items)
    {
        var matched = Collections.Filter(items, (PrimaryEquityGrantOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary equity grant override store.</summary>
public static class PrimaryEquityGrantOverrideStore
{
    public static IPrimaryEquityGrantOverrideStore NewMemoryPrimaryEquityGrantOverrideStore()
    {
        return new MemoryPrimaryEquityGrantOverrideStore(
            new Dictionary<string, PrimaryEquityGrantOverride>(),
            new List<string>());
    }

    public static IPrimaryEquityGrantOverrideStore NewCachedPrimaryEquityGrantOverrideStore(MemoryPrimaryEquityGrantOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryEquityGrantOverrideStore(
            inner,
            PrimaryEquityGrantOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryEquityGrantOverride> FindPrimaryEquityGrantOverrides(IPrimaryEquityGrantOverrideStore store, PrimaryEquityGrantOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
