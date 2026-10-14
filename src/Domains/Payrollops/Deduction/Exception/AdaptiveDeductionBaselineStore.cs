namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive deduction baselines by code.</summary>
public interface IAdaptiveDeductionBaselineStore
{
    AdaptiveDeductionBaseline? Find(string code);
    void Save(AdaptiveDeductionBaseline item);
    bool Remove(string code);
    List<AdaptiveDeductionBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveDeductionBaselineStore(Dictionary<string, AdaptiveDeductionBaseline> items, List<string> order) : IAdaptiveDeductionBaselineStore
{
    public Dictionary<string, AdaptiveDeductionBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveDeductionBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveDeductionBaseline item)
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

    public List<AdaptiveDeductionBaseline> List()
    {
        var outValue = new List<AdaptiveDeductionBaseline>();
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
public sealed class CachedAdaptiveDeductionBaselineStore(MemoryAdaptiveDeductionBaselineStore inner, AdaptiveDeductionBaselineCache cache, int hits, int misses) : IAdaptiveDeductionBaselineStore
{
    public MemoryAdaptiveDeductionBaselineStore Inner { get; set; } = inner;
    public AdaptiveDeductionBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveDeductionBaseline? Find(string code)
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

    public void Save(AdaptiveDeductionBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveDeductionBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive deduction baselines. Empty fields match all.</summary>
public sealed record AdaptiveDeductionBaselineQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveDeductionBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveDeductionBaseline> Apply(List<AdaptiveDeductionBaseline> items)
    {
        var matched = Collections.Filter(items, (AdaptiveDeductionBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive deduction baseline store.</summary>
public static class AdaptiveDeductionBaselineStore
{
    public static IAdaptiveDeductionBaselineStore NewMemoryAdaptiveDeductionBaselineStore()
    {
        return new MemoryAdaptiveDeductionBaselineStore(
            new Dictionary<string, AdaptiveDeductionBaseline>(),
            new List<string>());
    }

    public static IAdaptiveDeductionBaselineStore NewCachedAdaptiveDeductionBaselineStore(MemoryAdaptiveDeductionBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveDeductionBaselineStore(
            inner,
            AdaptiveDeductionBaselineCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveDeductionBaseline> FindAdaptiveDeductionBaselines(IAdaptiveDeductionBaselineStore store, AdaptiveDeductionBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
