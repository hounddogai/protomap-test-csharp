namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal license seat summaries by code.</summary>
public interface ISeasonalLicenseSeatSummaryStore
{
    SeasonalLicenseSeatSummary? Find(string code);
    void Save(SeasonalLicenseSeatSummary item);
    bool Remove(string code);
    List<SeasonalLicenseSeatSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalLicenseSeatSummaryStore(Dictionary<string, SeasonalLicenseSeatSummary> items, List<string> order) : ISeasonalLicenseSeatSummaryStore
{
    public Dictionary<string, SeasonalLicenseSeatSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalLicenseSeatSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalLicenseSeatSummary item)
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

    public List<SeasonalLicenseSeatSummary> List()
    {
        var outValue = new List<SeasonalLicenseSeatSummary>();
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
public sealed class CachedSeasonalLicenseSeatSummaryStore(MemorySeasonalLicenseSeatSummaryStore inner, SeasonalLicenseSeatSummaryCache cache, int hits, int misses) : ISeasonalLicenseSeatSummaryStore
{
    public MemorySeasonalLicenseSeatSummaryStore Inner { get; set; } = inner;
    public SeasonalLicenseSeatSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalLicenseSeatSummary? Find(string code)
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

    public void Save(SeasonalLicenseSeatSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalLicenseSeatSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal license seat summaries. Empty fields match all.</summary>
public sealed record SeasonalLicenseSeatSummaryQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalLicenseSeatSummary item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalLicenseSeatSummary> Apply(List<SeasonalLicenseSeatSummary> items)
    {
        var matched = Collections.Filter(items, (SeasonalLicenseSeatSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal license seat summary store.</summary>
public static class SeasonalLicenseSeatSummaryStore
{
    public static ISeasonalLicenseSeatSummaryStore NewMemorySeasonalLicenseSeatSummaryStore()
    {
        return new MemorySeasonalLicenseSeatSummaryStore(
            new Dictionary<string, SeasonalLicenseSeatSummary>(),
            new List<string>());
    }

    public static ISeasonalLicenseSeatSummaryStore NewCachedSeasonalLicenseSeatSummaryStore(MemorySeasonalLicenseSeatSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalLicenseSeatSummaryStore(
            inner,
            SeasonalLicenseSeatSummaryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalLicenseSeatSummary> FindSeasonalLicenseSeatSummaries(ISeasonalLicenseSeatSummaryStore store, SeasonalLicenseSeatSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
