namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy span summaries by code.</summary>
public interface ILegacySpanSummaryStore
{
    LegacySpanSummary? Find(string code);
    void Save(LegacySpanSummary item);
    bool Remove(string code);
    List<LegacySpanSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySpanSummaryStore(Dictionary<string, LegacySpanSummary> items, List<string> order) : ILegacySpanSummaryStore
{
    public Dictionary<string, LegacySpanSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySpanSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySpanSummary item)
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

    public List<LegacySpanSummary> List()
    {
        var outValue = new List<LegacySpanSummary>();
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
public sealed class CachedLegacySpanSummaryStore(MemoryLegacySpanSummaryStore inner, LegacySpanSummaryCache cache, int hits, int misses) : ILegacySpanSummaryStore
{
    public MemoryLegacySpanSummaryStore Inner { get; set; } = inner;
    public LegacySpanSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySpanSummary? Find(string code)
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

    public void Save(LegacySpanSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySpanSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy span summaries. Empty fields match all.</summary>
public sealed record LegacySpanSummaryQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySpanSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LegacySpanSummary> Apply(List<LegacySpanSummary> items)
    {
        var matched = Collections.Filter(items, (LegacySpanSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy span summary store.</summary>
public static class LegacySpanSummaryStore
{
    public static ILegacySpanSummaryStore NewMemoryLegacySpanSummaryStore()
    {
        return new MemoryLegacySpanSummaryStore(new Dictionary<string, LegacySpanSummary>(), new List<string>());
    }

    public static ILegacySpanSummaryStore NewCachedLegacySpanSummaryStore(MemoryLegacySpanSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySpanSummaryStore(inner, LegacySpanSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacySpanSummary> FindLegacySpanSummaries(ILegacySpanSummaryStore store, LegacySpanSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
