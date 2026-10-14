namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional career path summaries by code.</summary>
public interface IRegionalCareerPathSummaryStore
{
    RegionalCareerPathSummary? Find(string code);
    void Save(RegionalCareerPathSummary item);
    bool Remove(string code);
    List<RegionalCareerPathSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalCareerPathSummaryStore(Dictionary<string, RegionalCareerPathSummary> items, List<string> order) : IRegionalCareerPathSummaryStore
{
    public Dictionary<string, RegionalCareerPathSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalCareerPathSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalCareerPathSummary item)
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

    public List<RegionalCareerPathSummary> List()
    {
        var outValue = new List<RegionalCareerPathSummary>();
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
public sealed class CachedRegionalCareerPathSummaryStore(MemoryRegionalCareerPathSummaryStore inner, RegionalCareerPathSummaryCache cache, int hits, int misses) : IRegionalCareerPathSummaryStore
{
    public MemoryRegionalCareerPathSummaryStore Inner { get; set; } = inner;
    public RegionalCareerPathSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalCareerPathSummary? Find(string code)
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

    public void Save(RegionalCareerPathSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalCareerPathSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional career path summaries. Empty fields match all.</summary>
public sealed record RegionalCareerPathSummaryQuery(string Category, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalCareerPathSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalCareerPathSummary> Apply(List<RegionalCareerPathSummary> items)
    {
        var matched = Collections.Filter(items, (RegionalCareerPathSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional career path summary store.</summary>
public static class RegionalCareerPathSummaryStore
{
    public static IRegionalCareerPathSummaryStore NewMemoryRegionalCareerPathSummaryStore()
    {
        return new MemoryRegionalCareerPathSummaryStore(
            new Dictionary<string, RegionalCareerPathSummary>(),
            new List<string>());
    }

    public static IRegionalCareerPathSummaryStore NewCachedRegionalCareerPathSummaryStore(MemoryRegionalCareerPathSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalCareerPathSummaryStore(
            inner,
            RegionalCareerPathSummaryCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalCareerPathSummary> FindRegionalCareerPathSummaries(IRegionalCareerPathSummaryStore store, RegionalCareerPathSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
