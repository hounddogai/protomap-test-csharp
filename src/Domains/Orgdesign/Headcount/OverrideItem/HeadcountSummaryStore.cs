namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount summaries by code.</summary>
public interface IHeadcountSummaryStore
{
    HeadcountSummary? Find(string code);
    void Save(HeadcountSummary item);
    bool Remove(string code);
    List<HeadcountSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountSummaryStore(Dictionary<string, HeadcountSummary> items, List<string> order) : IHeadcountSummaryStore
{
    public Dictionary<string, HeadcountSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountSummary item)
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

    public List<HeadcountSummary> List()
    {
        var outValue = new List<HeadcountSummary>();
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
public sealed class CachedHeadcountSummaryStore(MemoryHeadcountSummaryStore inner, HeadcountSummaryCache cache, int hits, int misses) : IHeadcountSummaryStore
{
    public MemoryHeadcountSummaryStore Inner { get; set; } = inner;
    public HeadcountSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountSummary? Find(string code)
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

    public void Save(HeadcountSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount summaries. Empty fields match all.</summary>
public sealed record HeadcountSummaryQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountSummary> Apply(List<HeadcountSummary> items)
    {
        var matched = Collections.Filter(items, (HeadcountSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount summary store.</summary>
public static class HeadcountSummaryStore
{
    public static IHeadcountSummaryStore NewMemoryHeadcountSummaryStore()
    {
        return new MemoryHeadcountSummaryStore(new Dictionary<string, HeadcountSummary>(), new List<string>());
    }

    public static IHeadcountSummaryStore NewCachedHeadcountSummaryStore(MemoryHeadcountSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountSummaryStore(inner, HeadcountSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HeadcountSummary> FindHeadcountSummaries(IHeadcountSummaryStore store, HeadcountSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
