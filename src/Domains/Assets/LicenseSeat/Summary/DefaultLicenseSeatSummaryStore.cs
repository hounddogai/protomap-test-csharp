namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default license seat summaries by code.</summary>
public interface IDefaultLicenseSeatSummaryStore
{
    DefaultLicenseSeatSummary? Find(string code);
    void Save(DefaultLicenseSeatSummary item);
    bool Remove(string code);
    List<DefaultLicenseSeatSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultLicenseSeatSummaryStore(Dictionary<string, DefaultLicenseSeatSummary> items, List<string> order) : IDefaultLicenseSeatSummaryStore
{
    public Dictionary<string, DefaultLicenseSeatSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultLicenseSeatSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultLicenseSeatSummary item)
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

    public List<DefaultLicenseSeatSummary> List()
    {
        var outValue = new List<DefaultLicenseSeatSummary>();
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
public sealed class CachedDefaultLicenseSeatSummaryStore(MemoryDefaultLicenseSeatSummaryStore inner, DefaultLicenseSeatSummaryCache cache, int hits, int misses) : IDefaultLicenseSeatSummaryStore
{
    public MemoryDefaultLicenseSeatSummaryStore Inner { get; set; } = inner;
    public DefaultLicenseSeatSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultLicenseSeatSummary? Find(string code)
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

    public void Save(DefaultLicenseSeatSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultLicenseSeatSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default license seat summaries. Empty fields match all.</summary>
public sealed record DefaultLicenseSeatSummaryQuery(string Segment, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultLicenseSeatSummary item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultLicenseSeatSummary> Apply(List<DefaultLicenseSeatSummary> items)
    {
        var matched = Collections.Filter(items, (DefaultLicenseSeatSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default license seat summary store.</summary>
public static class DefaultLicenseSeatSummaryStore
{
    public static IDefaultLicenseSeatSummaryStore NewMemoryDefaultLicenseSeatSummaryStore()
    {
        return new MemoryDefaultLicenseSeatSummaryStore(
            new Dictionary<string, DefaultLicenseSeatSummary>(),
            new List<string>());
    }

    public static IDefaultLicenseSeatSummaryStore NewCachedDefaultLicenseSeatSummaryStore(MemoryDefaultLicenseSeatSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultLicenseSeatSummaryStore(
            inner,
            DefaultLicenseSeatSummaryCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultLicenseSeatSummary> FindDefaultLicenseSeatSummaries(IDefaultLicenseSeatSummaryStore store, DefaultLicenseSeatSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
