namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position summaries by code.</summary>
public interface IPositionSummaryStore
{
    PositionSummary? Find(string code);
    void Save(PositionSummary item);
    bool Remove(string code);
    List<PositionSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionSummaryStore(Dictionary<string, PositionSummary> items, List<string> order) : IPositionSummaryStore
{
    public Dictionary<string, PositionSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionSummary item)
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

    public List<PositionSummary> List()
    {
        var outValue = new List<PositionSummary>();
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
public sealed class CachedPositionSummaryStore(MemoryPositionSummaryStore inner, PositionSummaryCache cache, int hits, int misses) : IPositionSummaryStore
{
    public MemoryPositionSummaryStore Inner { get; set; } = inner;
    public PositionSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionSummary? Find(string code)
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

    public void Save(PositionSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position summaries. Empty fields match all.</summary>
public sealed record PositionSummaryQuery(string Region, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PositionSummary item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PositionSummary> Apply(List<PositionSummary> items)
    {
        var matched = Collections.Filter(items, (PositionSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position summary store.</summary>
public static class PositionSummaryStore
{
    public static IPositionSummaryStore NewMemoryPositionSummaryStore()
    {
        return new MemoryPositionSummaryStore(new Dictionary<string, PositionSummary>(), new List<string>());
    }

    public static IPositionSummaryStore NewCachedPositionSummaryStore(MemoryPositionSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPositionSummaryStore(inner, PositionSummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionSummary> FindPositionSummaries(IPositionSummaryStore store, PositionSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
