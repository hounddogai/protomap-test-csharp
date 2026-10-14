namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat audits by code.</summary>
public interface ILicenseSeatAuditStore
{
    LicenseSeatAudit? Find(string code);
    void Save(LicenseSeatAudit item);
    bool Remove(string code);
    List<LicenseSeatAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatAuditStore(Dictionary<string, LicenseSeatAudit> items, List<string> order) : ILicenseSeatAuditStore
{
    public Dictionary<string, LicenseSeatAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatAudit item)
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

    public List<LicenseSeatAudit> List()
    {
        var outValue = new List<LicenseSeatAudit>();
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
public sealed class CachedLicenseSeatAuditStore(MemoryLicenseSeatAuditStore inner, LicenseSeatAuditCache cache, int hits, int misses) : ILicenseSeatAuditStore
{
    public MemoryLicenseSeatAuditStore Inner { get; set; } = inner;
    public LicenseSeatAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatAudit? Find(string code)
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

    public void Save(LicenseSeatAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat audits. Empty fields match all.</summary>
public sealed record LicenseSeatAuditQuery(string Segment, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatAudit item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatAudit> Apply(List<LicenseSeatAudit> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat audit store.</summary>
public static class LicenseSeatAuditStore
{
    public static ILicenseSeatAuditStore NewMemoryLicenseSeatAuditStore()
    {
        return new MemoryLicenseSeatAuditStore(new Dictionary<string, LicenseSeatAudit>(), new List<string>());
    }

    public static ILicenseSeatAuditStore NewCachedLicenseSeatAuditStore(MemoryLicenseSeatAuditStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatAuditStore(inner, LicenseSeatAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatAudit> FindLicenseSeatAudits(ILicenseSeatAuditStore store, LicenseSeatAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
