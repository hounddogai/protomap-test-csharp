namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional desk summaries by code.</summary>
public interface IRegionalDeskSummaryStore
{
    RegionalDeskSummary? Find(string code);
    void Save(RegionalDeskSummary item);
    bool Remove(string code);
    List<RegionalDeskSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalDeskSummaryStore(Dictionary<string, RegionalDeskSummary> items, List<string> order) : IRegionalDeskSummaryStore
{
    public Dictionary<string, RegionalDeskSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalDeskSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalDeskSummary item)
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

    public List<RegionalDeskSummary> List()
    {
        var outValue = new List<RegionalDeskSummary>();
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
public sealed class CachedRegionalDeskSummaryStore(MemoryRegionalDeskSummaryStore inner, RegionalDeskSummaryCache cache, int hits, int misses) : IRegionalDeskSummaryStore
{
    public MemoryRegionalDeskSummaryStore Inner { get; set; } = inner;
    public RegionalDeskSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalDeskSummary? Find(string code)
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

    public void Save(RegionalDeskSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalDeskSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional desk summaries. Empty fields match all.</summary>
public sealed record RegionalDeskSummaryQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalDeskSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RegionalDeskSummary> Apply(List<RegionalDeskSummary> items)
    {
        var matched = Collections.Filter(items, (RegionalDeskSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional desk summary store.</summary>
public static class RegionalDeskSummaryStore
{
    public static IRegionalDeskSummaryStore NewMemoryRegionalDeskSummaryStore()
    {
        return new MemoryRegionalDeskSummaryStore(new Dictionary<string, RegionalDeskSummary>(), new List<string>());
    }

    public static IRegionalDeskSummaryStore NewCachedRegionalDeskSummaryStore(MemoryRegionalDeskSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalDeskSummaryStore(inner, RegionalDeskSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalDeskSummary> FindRegionalDeskSummaries(IRegionalDeskSummaryStore store, RegionalDeskSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
