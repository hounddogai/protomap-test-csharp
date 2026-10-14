namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg rollups by code.</summary>
public interface IReorgRollupStore
{
    ReorgRollup? Find(string code);
    void Save(ReorgRollup item);
    bool Remove(string code);
    List<ReorgRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgRollupStore(Dictionary<string, ReorgRollup> items, List<string> order) : IReorgRollupStore
{
    public Dictionary<string, ReorgRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgRollup item)
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

    public List<ReorgRollup> List()
    {
        var outValue = new List<ReorgRollup>();
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
public sealed class CachedReorgRollupStore(MemoryReorgRollupStore inner, ReorgRollupCache cache, int hits, int misses) : IReorgRollupStore
{
    public MemoryReorgRollupStore Inner { get; set; } = inner;
    public ReorgRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgRollup? Find(string code)
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

    public void Save(ReorgRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg rollups. Empty fields match all.</summary>
public sealed record ReorgRollupQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgRollup> Apply(List<ReorgRollup> items)
    {
        var matched = Collections.Filter(items, (ReorgRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg rollup store.</summary>
public static class ReorgRollupStore
{
    public static IReorgRollupStore NewMemoryReorgRollupStore()
    {
        return new MemoryReorgRollupStore(new Dictionary<string, ReorgRollup>(), new List<string>());
    }

    public static IReorgRollupStore NewCachedReorgRollupStore(MemoryReorgRollupStore inner, int ttl, IClock clock)
    {
        return new CachedReorgRollupStore(inner, ReorgRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgRollup> FindReorgRollups(IReorgRollupStore store, ReorgRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
