namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg baselines by code.</summary>
public interface IReorgBaselineStore
{
    ReorgBaseline? Find(string code);
    void Save(ReorgBaseline item);
    bool Remove(string code);
    List<ReorgBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgBaselineStore(Dictionary<string, ReorgBaseline> items, List<string> order) : IReorgBaselineStore
{
    public Dictionary<string, ReorgBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgBaseline item)
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

    public List<ReorgBaseline> List()
    {
        var outValue = new List<ReorgBaseline>();
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
public sealed class CachedReorgBaselineStore(MemoryReorgBaselineStore inner, ReorgBaselineCache cache, int hits, int misses) : IReorgBaselineStore
{
    public MemoryReorgBaselineStore Inner { get; set; } = inner;
    public ReorgBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgBaseline? Find(string code)
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

    public void Save(ReorgBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg baselines. Empty fields match all.</summary>
public sealed record ReorgBaselineQuery(string Country, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgBaseline item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgBaseline> Apply(List<ReorgBaseline> items)
    {
        var matched = Collections.Filter(items, (ReorgBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg baseline store.</summary>
public static class ReorgBaselineStore
{
    public static IReorgBaselineStore NewMemoryReorgBaselineStore()
    {
        return new MemoryReorgBaselineStore(new Dictionary<string, ReorgBaseline>(), new List<string>());
    }

    public static IReorgBaselineStore NewCachedReorgBaselineStore(MemoryReorgBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedReorgBaselineStore(inner, ReorgBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgBaseline> FindReorgBaselines(IReorgBaselineStore store, ReorgBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
