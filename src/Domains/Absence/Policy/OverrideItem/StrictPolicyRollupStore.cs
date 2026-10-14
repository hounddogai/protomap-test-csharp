namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict policy rollups by code.</summary>
public interface IStrictPolicyRollupStore
{
    StrictPolicyRollup? Find(string code);
    void Save(StrictPolicyRollup item);
    bool Remove(string code);
    List<StrictPolicyRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPolicyRollupStore(Dictionary<string, StrictPolicyRollup> items, List<string> order) : IStrictPolicyRollupStore
{
    public Dictionary<string, StrictPolicyRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPolicyRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPolicyRollup item)
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

    public List<StrictPolicyRollup> List()
    {
        var outValue = new List<StrictPolicyRollup>();
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
public sealed class CachedStrictPolicyRollupStore(MemoryStrictPolicyRollupStore inner, StrictPolicyRollupCache cache, int hits, int misses) : IStrictPolicyRollupStore
{
    public MemoryStrictPolicyRollupStore Inner { get; set; } = inner;
    public StrictPolicyRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPolicyRollup? Find(string code)
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

    public void Save(StrictPolicyRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPolicyRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict policy rollups. Empty fields match all.</summary>
public sealed record StrictPolicyRollupQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPolicyRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPolicyRollup> Apply(List<StrictPolicyRollup> items)
    {
        var matched = Collections.Filter(items, (StrictPolicyRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict policy rollup store.</summary>
public static class StrictPolicyRollupStore
{
    public static IStrictPolicyRollupStore NewMemoryStrictPolicyRollupStore()
    {
        return new MemoryStrictPolicyRollupStore(new Dictionary<string, StrictPolicyRollup>(), new List<string>());
    }

    public static IStrictPolicyRollupStore NewCachedStrictPolicyRollupStore(MemoryStrictPolicyRollupStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPolicyRollupStore(inner, StrictPolicyRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictPolicyRollup> FindStrictPolicyRollups(IStrictPolicyRollupStore store, StrictPolicyRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
