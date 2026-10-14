namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default career path summaries by code.</summary>
public interface IDefaultCareerPathSummaryStore
{
    DefaultCareerPathSummary? Find(string code);
    void Save(DefaultCareerPathSummary item);
    bool Remove(string code);
    List<DefaultCareerPathSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultCareerPathSummaryStore(Dictionary<string, DefaultCareerPathSummary> items, List<string> order) : IDefaultCareerPathSummaryStore
{
    public Dictionary<string, DefaultCareerPathSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultCareerPathSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultCareerPathSummary item)
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

    public List<DefaultCareerPathSummary> List()
    {
        var outValue = new List<DefaultCareerPathSummary>();
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
public sealed class CachedDefaultCareerPathSummaryStore(MemoryDefaultCareerPathSummaryStore inner, DefaultCareerPathSummaryCache cache, int hits, int misses) : IDefaultCareerPathSummaryStore
{
    public MemoryDefaultCareerPathSummaryStore Inner { get; set; } = inner;
    public DefaultCareerPathSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultCareerPathSummary? Find(string code)
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

    public void Save(DefaultCareerPathSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultCareerPathSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default career path summaries. Empty fields match all.</summary>
public sealed record DefaultCareerPathSummaryQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultCareerPathSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<DefaultCareerPathSummary> Apply(List<DefaultCareerPathSummary> items)
    {
        var matched = Collections.Filter(items, (DefaultCareerPathSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default career path summary store.</summary>
public static class DefaultCareerPathSummaryStore
{
    public static IDefaultCareerPathSummaryStore NewMemoryDefaultCareerPathSummaryStore()
    {
        return new MemoryDefaultCareerPathSummaryStore(
            new Dictionary<string, DefaultCareerPathSummary>(),
            new List<string>());
    }

    public static IDefaultCareerPathSummaryStore NewCachedDefaultCareerPathSummaryStore(MemoryDefaultCareerPathSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultCareerPathSummaryStore(
            inner,
            DefaultCareerPathSummaryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultCareerPathSummary> FindDefaultCareerPathSummaries(IDefaultCareerPathSummaryStore store, DefaultCareerPathSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
