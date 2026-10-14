namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling certification summaries by code.</summary>
public interface IRollingCertificationSummaryStore
{
    RollingCertificationSummary? Find(string code);
    void Save(RollingCertificationSummary item);
    bool Remove(string code);
    List<RollingCertificationSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingCertificationSummaryStore(Dictionary<string, RollingCertificationSummary> items, List<string> order) : IRollingCertificationSummaryStore
{
    public Dictionary<string, RollingCertificationSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingCertificationSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingCertificationSummary item)
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

    public List<RollingCertificationSummary> List()
    {
        var outValue = new List<RollingCertificationSummary>();
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
public sealed class CachedRollingCertificationSummaryStore(MemoryRollingCertificationSummaryStore inner, RollingCertificationSummaryCache cache, int hits, int misses) : IRollingCertificationSummaryStore
{
    public MemoryRollingCertificationSummaryStore Inner { get; set; } = inner;
    public RollingCertificationSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingCertificationSummary? Find(string code)
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

    public void Save(RollingCertificationSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingCertificationSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling certification summaries. Empty fields match all.</summary>
public sealed record RollingCertificationSummaryQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RollingCertificationSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingCertificationSummary> Apply(List<RollingCertificationSummary> items)
    {
        var matched = Collections.Filter(items, (RollingCertificationSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling certification summary store.</summary>
public static class RollingCertificationSummaryStore
{
    public static IRollingCertificationSummaryStore NewMemoryRollingCertificationSummaryStore()
    {
        return new MemoryRollingCertificationSummaryStore(
            new Dictionary<string, RollingCertificationSummary>(),
            new List<string>());
    }

    public static IRollingCertificationSummaryStore NewCachedRollingCertificationSummaryStore(MemoryRollingCertificationSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRollingCertificationSummaryStore(
            inner,
            RollingCertificationSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingCertificationSummary> FindRollingCertificationSummaries(IRollingCertificationSummaryStore store, RollingCertificationSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
