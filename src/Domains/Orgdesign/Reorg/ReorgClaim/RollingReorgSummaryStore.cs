namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling reorg summaries by code.</summary>
public interface IRollingReorgSummaryStore
{
    RollingReorgSummary? Find(string code);
    void Save(RollingReorgSummary item);
    bool Remove(string code);
    List<RollingReorgSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingReorgSummaryStore(Dictionary<string, RollingReorgSummary> items, List<string> order) : IRollingReorgSummaryStore
{
    public Dictionary<string, RollingReorgSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingReorgSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingReorgSummary item)
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

    public List<RollingReorgSummary> List()
    {
        var outValue = new List<RollingReorgSummary>();
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
public sealed class CachedRollingReorgSummaryStore(MemoryRollingReorgSummaryStore inner, RollingReorgSummaryCache cache, int hits, int misses) : IRollingReorgSummaryStore
{
    public MemoryRollingReorgSummaryStore Inner { get; set; } = inner;
    public RollingReorgSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingReorgSummary? Find(string code)
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

    public void Save(RollingReorgSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingReorgSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling reorg summaries. Empty fields match all.</summary>
public sealed record RollingReorgSummaryQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RollingReorgSummary item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingReorgSummary> Apply(List<RollingReorgSummary> items)
    {
        var matched = Collections.Filter(items, (RollingReorgSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling reorg summary store.</summary>
public static class RollingReorgSummaryStore
{
    public static IRollingReorgSummaryStore NewMemoryRollingReorgSummaryStore()
    {
        return new MemoryRollingReorgSummaryStore(new Dictionary<string, RollingReorgSummary>(), new List<string>());
    }

    public static IRollingReorgSummaryStore NewCachedRollingReorgSummaryStore(MemoryRollingReorgSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRollingReorgSummaryStore(inner, RollingReorgSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingReorgSummary> FindRollingReorgSummaries(IRollingReorgSummaryStore store, RollingReorgSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
