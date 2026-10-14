namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path summaries by code.</summary>
public interface ICareerPathSummaryStore
{
    CareerPathSummary? Find(string code);
    void Save(CareerPathSummary item);
    bool Remove(string code);
    List<CareerPathSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathSummaryStore(Dictionary<string, CareerPathSummary> items, List<string> order) : ICareerPathSummaryStore
{
    public Dictionary<string, CareerPathSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathSummary item)
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

    public List<CareerPathSummary> List()
    {
        var outValue = new List<CareerPathSummary>();
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
public sealed class CachedCareerPathSummaryStore(MemoryCareerPathSummaryStore inner, CareerPathSummaryCache cache, int hits, int misses) : ICareerPathSummaryStore
{
    public MemoryCareerPathSummaryStore Inner { get; set; } = inner;
    public CareerPathSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathSummary? Find(string code)
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

    public void Save(CareerPathSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path summaries. Empty fields match all.</summary>
public sealed record CareerPathSummaryQuery(string Locale, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathSummary item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<CareerPathSummary> Apply(List<CareerPathSummary> items)
    {
        var matched = Collections.Filter(items, (CareerPathSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path summary store.</summary>
public static class CareerPathSummaryStore
{
    public static ICareerPathSummaryStore NewMemoryCareerPathSummaryStore()
    {
        return new MemoryCareerPathSummaryStore(new Dictionary<string, CareerPathSummary>(), new List<string>());
    }

    public static ICareerPathSummaryStore NewCachedCareerPathSummaryStore(MemoryCareerPathSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathSummaryStore(inner, CareerPathSummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CareerPathSummary> FindCareerPathSummaries(ICareerPathSummaryStore store, CareerPathSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
