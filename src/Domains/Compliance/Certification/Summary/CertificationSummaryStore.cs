namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification summaries by code.</summary>
public interface ICertificationSummaryStore
{
    CertificationSummary? Find(string code);
    void Save(CertificationSummary item);
    bool Remove(string code);
    List<CertificationSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationSummaryStore(Dictionary<string, CertificationSummary> items, List<string> order) : ICertificationSummaryStore
{
    public Dictionary<string, CertificationSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationSummary item)
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

    public List<CertificationSummary> List()
    {
        var outValue = new List<CertificationSummary>();
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
public sealed class CachedCertificationSummaryStore(MemoryCertificationSummaryStore inner, CertificationSummaryCache cache, int hits, int misses) : ICertificationSummaryStore
{
    public MemoryCertificationSummaryStore Inner { get; set; } = inner;
    public CertificationSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationSummary? Find(string code)
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

    public void Save(CertificationSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification summaries. Empty fields match all.</summary>
public sealed record CertificationSummaryQuery(string Source, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationSummary item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationSummary> Apply(List<CertificationSummary> items)
    {
        var matched = Collections.Filter(items, (CertificationSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification summary store.</summary>
public static class CertificationSummaryStore
{
    public static ICertificationSummaryStore NewMemoryCertificationSummaryStore()
    {
        return new MemoryCertificationSummaryStore(
            new Dictionary<string, CertificationSummary>(),
            new List<string>());
    }

    public static ICertificationSummaryStore NewCachedCertificationSummaryStore(MemoryCertificationSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationSummaryStore(inner, CertificationSummaryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CertificationSummary> FindCertificationSummaries(ICertificationSummaryStore store, CertificationSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
