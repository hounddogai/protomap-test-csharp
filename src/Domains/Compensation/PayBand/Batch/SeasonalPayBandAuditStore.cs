namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal pay band audits by code.</summary>
public interface ISeasonalPayBandAuditStore
{
    SeasonalPayBandAudit? Find(string code);
    void Save(SeasonalPayBandAudit item);
    bool Remove(string code);
    List<SeasonalPayBandAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPayBandAuditStore(Dictionary<string, SeasonalPayBandAudit> items, List<string> order) : ISeasonalPayBandAuditStore
{
    public Dictionary<string, SeasonalPayBandAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPayBandAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPayBandAudit item)
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

    public List<SeasonalPayBandAudit> List()
    {
        var outValue = new List<SeasonalPayBandAudit>();
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
public sealed class CachedSeasonalPayBandAuditStore(MemorySeasonalPayBandAuditStore inner, SeasonalPayBandAuditCache cache, int hits, int misses) : ISeasonalPayBandAuditStore
{
    public MemorySeasonalPayBandAuditStore Inner { get; set; } = inner;
    public SeasonalPayBandAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPayBandAudit? Find(string code)
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

    public void Save(SeasonalPayBandAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPayBandAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal pay band audits. Empty fields match all.</summary>
public sealed record SeasonalPayBandAuditQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPayBandAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPayBandAudit> Apply(List<SeasonalPayBandAudit> items)
    {
        var matched = Collections.Filter(items, (SeasonalPayBandAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal pay band audit store.</summary>
public static class SeasonalPayBandAuditStore
{
    public static ISeasonalPayBandAuditStore NewMemorySeasonalPayBandAuditStore()
    {
        return new MemorySeasonalPayBandAuditStore(
            new Dictionary<string, SeasonalPayBandAudit>(),
            new List<string>());
    }

    public static ISeasonalPayBandAuditStore NewCachedSeasonalPayBandAuditStore(MemorySeasonalPayBandAuditStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPayBandAuditStore(inner, SeasonalPayBandAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SeasonalPayBandAudit> FindSeasonalPayBandAudits(ISeasonalPayBandAuditStore store, SeasonalPayBandAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
