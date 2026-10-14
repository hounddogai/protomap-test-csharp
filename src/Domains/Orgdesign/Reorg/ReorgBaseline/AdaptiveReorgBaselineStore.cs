namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive reorg baselines by code.</summary>
public interface IAdaptiveReorgBaselineStore
{
    AdaptiveReorgBaseline? Find(string code);
    void Save(AdaptiveReorgBaseline item);
    bool Remove(string code);
    List<AdaptiveReorgBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveReorgBaselineStore(Dictionary<string, AdaptiveReorgBaseline> items, List<string> order) : IAdaptiveReorgBaselineStore
{
    public Dictionary<string, AdaptiveReorgBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveReorgBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveReorgBaseline item)
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

    public List<AdaptiveReorgBaseline> List()
    {
        var outValue = new List<AdaptiveReorgBaseline>();
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
public sealed class CachedAdaptiveReorgBaselineStore(MemoryAdaptiveReorgBaselineStore inner, AdaptiveReorgBaselineCache cache, int hits, int misses) : IAdaptiveReorgBaselineStore
{
    public MemoryAdaptiveReorgBaselineStore Inner { get; set; } = inner;
    public AdaptiveReorgBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveReorgBaseline? Find(string code)
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

    public void Save(AdaptiveReorgBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveReorgBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive reorg baselines. Empty fields match all.</summary>
public sealed record AdaptiveReorgBaselineQuery(string Category, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveReorgBaseline item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveReorgBaseline> Apply(List<AdaptiveReorgBaseline> items)
    {
        var matched = Collections.Filter(items, (AdaptiveReorgBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive reorg baseline store.</summary>
public static class AdaptiveReorgBaselineStore
{
    public static IAdaptiveReorgBaselineStore NewMemoryAdaptiveReorgBaselineStore()
    {
        return new MemoryAdaptiveReorgBaselineStore(
            new Dictionary<string, AdaptiveReorgBaseline>(),
            new List<string>());
    }

    public static IAdaptiveReorgBaselineStore NewCachedAdaptiveReorgBaselineStore(MemoryAdaptiveReorgBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveReorgBaselineStore(
            inner,
            AdaptiveReorgBaselineCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveReorgBaseline> FindAdaptiveReorgBaselines(IAdaptiveReorgBaselineStore store, AdaptiveReorgBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
