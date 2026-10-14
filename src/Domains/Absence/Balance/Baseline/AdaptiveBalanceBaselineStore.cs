namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive balance baselines by code.</summary>
public interface IAdaptiveBalanceBaselineStore
{
    AdaptiveBalanceBaseline? Find(string code);
    void Save(AdaptiveBalanceBaseline item);
    bool Remove(string code);
    List<AdaptiveBalanceBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBalanceBaselineStore(Dictionary<string, AdaptiveBalanceBaseline> items, List<string> order) : IAdaptiveBalanceBaselineStore
{
    public Dictionary<string, AdaptiveBalanceBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBalanceBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBalanceBaseline item)
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

    public List<AdaptiveBalanceBaseline> List()
    {
        var outValue = new List<AdaptiveBalanceBaseline>();
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
public sealed class CachedAdaptiveBalanceBaselineStore(MemoryAdaptiveBalanceBaselineStore inner, AdaptiveBalanceBaselineCache cache, int hits, int misses) : IAdaptiveBalanceBaselineStore
{
    public MemoryAdaptiveBalanceBaselineStore Inner { get; set; } = inner;
    public AdaptiveBalanceBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBalanceBaseline? Find(string code)
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

    public void Save(AdaptiveBalanceBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBalanceBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive balance baselines. Empty fields match all.</summary>
public sealed record AdaptiveBalanceBaselineQuery(string Source, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBalanceBaseline item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBalanceBaseline> Apply(List<AdaptiveBalanceBaseline> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBalanceBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive balance baseline store.</summary>
public static class AdaptiveBalanceBaselineStore
{
    public static IAdaptiveBalanceBaselineStore NewMemoryAdaptiveBalanceBaselineStore()
    {
        return new MemoryAdaptiveBalanceBaselineStore(
            new Dictionary<string, AdaptiveBalanceBaseline>(),
            new List<string>());
    }

    public static IAdaptiveBalanceBaselineStore NewCachedAdaptiveBalanceBaselineStore(MemoryAdaptiveBalanceBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBalanceBaselineStore(
            inner,
            AdaptiveBalanceBaselineCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBalanceBaseline> FindAdaptiveBalanceBaselines(IAdaptiveBalanceBaselineStore store, AdaptiveBalanceBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
