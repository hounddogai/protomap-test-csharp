namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive headcount audits by code.</summary>
public interface IAdaptiveHeadcountAuditStore
{
    AdaptiveHeadcountAudit? Find(string code);
    void Save(AdaptiveHeadcountAudit item);
    bool Remove(string code);
    List<AdaptiveHeadcountAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveHeadcountAuditStore(Dictionary<string, AdaptiveHeadcountAudit> items, List<string> order) : IAdaptiveHeadcountAuditStore
{
    public Dictionary<string, AdaptiveHeadcountAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveHeadcountAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveHeadcountAudit item)
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

    public List<AdaptiveHeadcountAudit> List()
    {
        var outValue = new List<AdaptiveHeadcountAudit>();
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
public sealed class CachedAdaptiveHeadcountAuditStore(MemoryAdaptiveHeadcountAuditStore inner, AdaptiveHeadcountAuditCache cache, int hits, int misses) : IAdaptiveHeadcountAuditStore
{
    public MemoryAdaptiveHeadcountAuditStore Inner { get; set; } = inner;
    public AdaptiveHeadcountAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveHeadcountAudit? Find(string code)
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

    public void Save(AdaptiveHeadcountAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveHeadcountAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive headcount audits. Empty fields match all.</summary>
public sealed record AdaptiveHeadcountAuditQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveHeadcountAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveHeadcountAudit> Apply(List<AdaptiveHeadcountAudit> items)
    {
        var matched = Collections.Filter(items, (AdaptiveHeadcountAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive headcount audit store.</summary>
public static class AdaptiveHeadcountAuditStore
{
    public static IAdaptiveHeadcountAuditStore NewMemoryAdaptiveHeadcountAuditStore()
    {
        return new MemoryAdaptiveHeadcountAuditStore(
            new Dictionary<string, AdaptiveHeadcountAudit>(),
            new List<string>());
    }

    public static IAdaptiveHeadcountAuditStore NewCachedAdaptiveHeadcountAuditStore(MemoryAdaptiveHeadcountAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveHeadcountAuditStore(
            inner,
            AdaptiveHeadcountAuditCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveHeadcountAudit> FindAdaptiveHeadcountAudits(IAdaptiveHeadcountAuditStore store, AdaptiveHeadcountAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
