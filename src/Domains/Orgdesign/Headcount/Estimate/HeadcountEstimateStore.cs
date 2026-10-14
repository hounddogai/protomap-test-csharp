namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount estimates by code.</summary>
public interface IHeadcountEstimateStore
{
    HeadcountEstimate? Find(string code);
    void Save(HeadcountEstimate item);
    bool Remove(string code);
    List<HeadcountEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountEstimateStore(Dictionary<string, HeadcountEstimate> items, List<string> order) : IHeadcountEstimateStore
{
    public Dictionary<string, HeadcountEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountEstimate item)
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

    public List<HeadcountEstimate> List()
    {
        var outValue = new List<HeadcountEstimate>();
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
public sealed class CachedHeadcountEstimateStore(MemoryHeadcountEstimateStore inner, HeadcountEstimateCache cache, int hits, int misses) : IHeadcountEstimateStore
{
    public MemoryHeadcountEstimateStore Inner { get; set; } = inner;
    public HeadcountEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountEstimate? Find(string code)
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

    public void Save(HeadcountEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount estimates. Empty fields match all.</summary>
public sealed record HeadcountEstimateQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<HeadcountEstimate> Apply(List<HeadcountEstimate> items)
    {
        var matched = Collections.Filter(items, (HeadcountEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount estimate store.</summary>
public static class HeadcountEstimateStore
{
    public static IHeadcountEstimateStore NewMemoryHeadcountEstimateStore()
    {
        return new MemoryHeadcountEstimateStore(new Dictionary<string, HeadcountEstimate>(), new List<string>());
    }

    public static IHeadcountEstimateStore NewCachedHeadcountEstimateStore(MemoryHeadcountEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountEstimateStore(inner, HeadcountEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountEstimate> FindHeadcountEstimates(IHeadcountEstimateStore store, HeadcountEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
