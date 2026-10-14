namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment audits by code.</summary>
public interface IGarnishmentAuditStore
{
    GarnishmentAudit? Find(string code);
    void Save(GarnishmentAudit item);
    bool Remove(string code);
    List<GarnishmentAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentAuditStore(Dictionary<string, GarnishmentAudit> items, List<string> order) : IGarnishmentAuditStore
{
    public Dictionary<string, GarnishmentAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentAudit item)
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

    public List<GarnishmentAudit> List()
    {
        var outValue = new List<GarnishmentAudit>();
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
public sealed class CachedGarnishmentAuditStore(MemoryGarnishmentAuditStore inner, GarnishmentAuditCache cache, int hits, int misses) : IGarnishmentAuditStore
{
    public MemoryGarnishmentAuditStore Inner { get; set; } = inner;
    public GarnishmentAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentAudit? Find(string code)
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

    public void Save(GarnishmentAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment audits. Empty fields match all.</summary>
public sealed record GarnishmentAuditQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<GarnishmentAudit> Apply(List<GarnishmentAudit> items)
    {
        var matched = Collections.Filter(items, (GarnishmentAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment audit store.</summary>
public static class GarnishmentAuditStore
{
    public static IGarnishmentAuditStore NewMemoryGarnishmentAuditStore()
    {
        return new MemoryGarnishmentAuditStore(new Dictionary<string, GarnishmentAudit>(), new List<string>());
    }

    public static IGarnishmentAuditStore NewCachedGarnishmentAuditStore(MemoryGarnishmentAuditStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentAuditStore(inner, GarnishmentAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentAudit> FindGarnishmentAudits(IGarnishmentAuditStore store, GarnishmentAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
