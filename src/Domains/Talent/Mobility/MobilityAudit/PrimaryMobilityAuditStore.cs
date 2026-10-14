namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary mobility audits by code.</summary>
public interface IPrimaryMobilityAuditStore
{
    PrimaryMobilityAudit? Find(string code);
    void Save(PrimaryMobilityAudit item);
    bool Remove(string code);
    List<PrimaryMobilityAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryMobilityAuditStore(Dictionary<string, PrimaryMobilityAudit> items, List<string> order) : IPrimaryMobilityAuditStore
{
    public Dictionary<string, PrimaryMobilityAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryMobilityAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryMobilityAudit item)
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

    public List<PrimaryMobilityAudit> List()
    {
        var outValue = new List<PrimaryMobilityAudit>();
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
public sealed class CachedPrimaryMobilityAuditStore(MemoryPrimaryMobilityAuditStore inner, PrimaryMobilityAuditCache cache, int hits, int misses) : IPrimaryMobilityAuditStore
{
    public MemoryPrimaryMobilityAuditStore Inner { get; set; } = inner;
    public PrimaryMobilityAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryMobilityAudit? Find(string code)
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

    public void Save(PrimaryMobilityAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryMobilityAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary mobility audits. Empty fields match all.</summary>
public sealed record PrimaryMobilityAuditQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryMobilityAudit item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<PrimaryMobilityAudit> Apply(List<PrimaryMobilityAudit> items)
    {
        var matched = Collections.Filter(items, (PrimaryMobilityAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary mobility audit store.</summary>
public static class PrimaryMobilityAuditStore
{
    public static IPrimaryMobilityAuditStore NewMemoryPrimaryMobilityAuditStore()
    {
        return new MemoryPrimaryMobilityAuditStore(
            new Dictionary<string, PrimaryMobilityAudit>(),
            new List<string>());
    }

    public static IPrimaryMobilityAuditStore NewCachedPrimaryMobilityAuditStore(MemoryPrimaryMobilityAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryMobilityAuditStore(inner, PrimaryMobilityAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryMobilityAudit> FindPrimaryMobilityAudits(IPrimaryMobilityAuditStore store, PrimaryMobilityAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
