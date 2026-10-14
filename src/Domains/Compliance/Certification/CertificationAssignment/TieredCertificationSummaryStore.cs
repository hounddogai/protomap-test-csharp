namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered certification summaries by code.</summary>
public interface ITieredCertificationSummaryStore
{
    TieredCertificationSummary? Find(string code);
    void Save(TieredCertificationSummary item);
    bool Remove(string code);
    List<TieredCertificationSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredCertificationSummaryStore(Dictionary<string, TieredCertificationSummary> items, List<string> order) : ITieredCertificationSummaryStore
{
    public Dictionary<string, TieredCertificationSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredCertificationSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredCertificationSummary item)
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

    public List<TieredCertificationSummary> List()
    {
        var outValue = new List<TieredCertificationSummary>();
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
public sealed class CachedTieredCertificationSummaryStore(MemoryTieredCertificationSummaryStore inner, TieredCertificationSummaryCache cache, int hits, int misses) : ITieredCertificationSummaryStore
{
    public MemoryTieredCertificationSummaryStore Inner { get; set; } = inner;
    public TieredCertificationSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredCertificationSummary? Find(string code)
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

    public void Save(TieredCertificationSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredCertificationSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered certification summaries. Empty fields match all.</summary>
public sealed record TieredCertificationSummaryQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(TieredCertificationSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredCertificationSummary> Apply(List<TieredCertificationSummary> items)
    {
        var matched = Collections.Filter(items, (TieredCertificationSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered certification summary store.</summary>
public static class TieredCertificationSummaryStore
{
    public static ITieredCertificationSummaryStore NewMemoryTieredCertificationSummaryStore()
    {
        return new MemoryTieredCertificationSummaryStore(
            new Dictionary<string, TieredCertificationSummary>(),
            new List<string>());
    }

    public static ITieredCertificationSummaryStore NewCachedTieredCertificationSummaryStore(MemoryTieredCertificationSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedTieredCertificationSummaryStore(
            inner,
            TieredCertificationSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredCertificationSummary> FindTieredCertificationSummaries(ITieredCertificationSummaryStore store, TieredCertificationSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
