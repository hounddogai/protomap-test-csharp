namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary pay run audits by code.</summary>
public interface IPrimaryPayRunAuditStore
{
    PrimaryPayRunAudit? Find(string code);
    void Save(PrimaryPayRunAudit item);
    bool Remove(string code);
    List<PrimaryPayRunAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPayRunAuditStore(Dictionary<string, PrimaryPayRunAudit> items, List<string> order) : IPrimaryPayRunAuditStore
{
    public Dictionary<string, PrimaryPayRunAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPayRunAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPayRunAudit item)
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

    public List<PrimaryPayRunAudit> List()
    {
        var outValue = new List<PrimaryPayRunAudit>();
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
public sealed class CachedPrimaryPayRunAuditStore(MemoryPrimaryPayRunAuditStore inner, PrimaryPayRunAuditCache cache, int hits, int misses) : IPrimaryPayRunAuditStore
{
    public MemoryPrimaryPayRunAuditStore Inner { get; set; } = inner;
    public PrimaryPayRunAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPayRunAudit? Find(string code)
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

    public void Save(PrimaryPayRunAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPayRunAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary pay run audits. Empty fields match all.</summary>
public sealed record PrimaryPayRunAuditQuery(string Country, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPayRunAudit item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPayRunAudit> Apply(List<PrimaryPayRunAudit> items)
    {
        var matched = Collections.Filter(items, (PrimaryPayRunAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary pay run audit store.</summary>
public static class PrimaryPayRunAuditStore
{
    public static IPrimaryPayRunAuditStore NewMemoryPrimaryPayRunAuditStore()
    {
        return new MemoryPrimaryPayRunAuditStore(new Dictionary<string, PrimaryPayRunAudit>(), new List<string>());
    }

    public static IPrimaryPayRunAuditStore NewCachedPrimaryPayRunAuditStore(MemoryPrimaryPayRunAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPayRunAuditStore(inner, PrimaryPayRunAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryPayRunAudit> FindPrimaryPayRunAudits(IPrimaryPayRunAuditStore store, PrimaryPayRunAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
