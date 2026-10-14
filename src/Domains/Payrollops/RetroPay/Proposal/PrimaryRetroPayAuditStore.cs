namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary retro pay audits by code.</summary>
public interface IPrimaryRetroPayAuditStore
{
    PrimaryRetroPayAudit? Find(string code);
    void Save(PrimaryRetroPayAudit item);
    bool Remove(string code);
    List<PrimaryRetroPayAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRetroPayAuditStore(Dictionary<string, PrimaryRetroPayAudit> items, List<string> order) : IPrimaryRetroPayAuditStore
{
    public Dictionary<string, PrimaryRetroPayAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRetroPayAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRetroPayAudit item)
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

    public List<PrimaryRetroPayAudit> List()
    {
        var outValue = new List<PrimaryRetroPayAudit>();
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
public sealed class CachedPrimaryRetroPayAuditStore(MemoryPrimaryRetroPayAuditStore inner, PrimaryRetroPayAuditCache cache, int hits, int misses) : IPrimaryRetroPayAuditStore
{
    public MemoryPrimaryRetroPayAuditStore Inner { get; set; } = inner;
    public PrimaryRetroPayAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRetroPayAudit? Find(string code)
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

    public void Save(PrimaryRetroPayAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRetroPayAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary retro pay audits. Empty fields match all.</summary>
public sealed record PrimaryRetroPayAuditQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRetroPayAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRetroPayAudit> Apply(List<PrimaryRetroPayAudit> items)
    {
        var matched = Collections.Filter(items, (PrimaryRetroPayAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary retro pay audit store.</summary>
public static class PrimaryRetroPayAuditStore
{
    public static IPrimaryRetroPayAuditStore NewMemoryPrimaryRetroPayAuditStore()
    {
        return new MemoryPrimaryRetroPayAuditStore(
            new Dictionary<string, PrimaryRetroPayAudit>(),
            new List<string>());
    }

    public static IPrimaryRetroPayAuditStore NewCachedPrimaryRetroPayAuditStore(MemoryPrimaryRetroPayAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRetroPayAuditStore(inner, PrimaryRetroPayAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryRetroPayAudit> FindPrimaryRetroPayAudits(IPrimaryRetroPayAuditStore store, PrimaryRetroPayAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
