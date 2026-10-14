namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive statement of work audits by code.</summary>
public interface IAdaptiveStatementOfWorkAuditStore
{
    AdaptiveStatementOfWorkAudit? Find(string code);
    void Save(AdaptiveStatementOfWorkAudit item);
    bool Remove(string code);
    List<AdaptiveStatementOfWorkAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveStatementOfWorkAuditStore(Dictionary<string, AdaptiveStatementOfWorkAudit> items, List<string> order) : IAdaptiveStatementOfWorkAuditStore
{
    public Dictionary<string, AdaptiveStatementOfWorkAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveStatementOfWorkAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveStatementOfWorkAudit item)
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

    public List<AdaptiveStatementOfWorkAudit> List()
    {
        var outValue = new List<AdaptiveStatementOfWorkAudit>();
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
public sealed class CachedAdaptiveStatementOfWorkAuditStore(MemoryAdaptiveStatementOfWorkAuditStore inner, AdaptiveStatementOfWorkAuditCache cache, int hits, int misses) : IAdaptiveStatementOfWorkAuditStore
{
    public MemoryAdaptiveStatementOfWorkAuditStore Inner { get; set; } = inner;
    public AdaptiveStatementOfWorkAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveStatementOfWorkAudit? Find(string code)
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

    public void Save(AdaptiveStatementOfWorkAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveStatementOfWorkAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive statement of work audits. Empty fields match all.</summary>
public sealed record AdaptiveStatementOfWorkAuditQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveStatementOfWorkAudit item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveStatementOfWorkAudit> Apply(List<AdaptiveStatementOfWorkAudit> items)
    {
        var matched = Collections.Filter(items, (AdaptiveStatementOfWorkAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive statement of work audit store.</summary>
public static class AdaptiveStatementOfWorkAuditStore
{
    public static IAdaptiveStatementOfWorkAuditStore NewMemoryAdaptiveStatementOfWorkAuditStore()
    {
        return new MemoryAdaptiveStatementOfWorkAuditStore(
            new Dictionary<string, AdaptiveStatementOfWorkAudit>(),
            new List<string>());
    }

    public static IAdaptiveStatementOfWorkAuditStore NewCachedAdaptiveStatementOfWorkAuditStore(MemoryAdaptiveStatementOfWorkAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveStatementOfWorkAuditStore(
            inner,
            AdaptiveStatementOfWorkAuditCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveStatementOfWorkAudit> FindAdaptiveStatementOfWorkAudits(IAdaptiveStatementOfWorkAuditStore store, AdaptiveStatementOfWorkAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
