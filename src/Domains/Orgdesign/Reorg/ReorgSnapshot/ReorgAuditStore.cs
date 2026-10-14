namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg audits by code.</summary>
public interface IReorgAuditStore
{
    ReorgAudit? Find(string code);
    void Save(ReorgAudit item);
    bool Remove(string code);
    List<ReorgAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgAuditStore(Dictionary<string, ReorgAudit> items, List<string> order) : IReorgAuditStore
{
    public Dictionary<string, ReorgAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgAudit item)
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

    public List<ReorgAudit> List()
    {
        var outValue = new List<ReorgAudit>();
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
public sealed class CachedReorgAuditStore(MemoryReorgAuditStore inner, ReorgAuditCache cache, int hits, int misses) : IReorgAuditStore
{
    public MemoryReorgAuditStore Inner { get; set; } = inner;
    public ReorgAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgAudit? Find(string code)
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

    public void Save(ReorgAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg audits. Empty fields match all.</summary>
public sealed record ReorgAuditQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgAudit item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgAudit> Apply(List<ReorgAudit> items)
    {
        var matched = Collections.Filter(items, (ReorgAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg audit store.</summary>
public static class ReorgAuditStore
{
    public static IReorgAuditStore NewMemoryReorgAuditStore()
    {
        return new MemoryReorgAuditStore(new Dictionary<string, ReorgAudit>(), new List<string>());
    }

    public static IReorgAuditStore NewCachedReorgAuditStore(MemoryReorgAuditStore inner, int ttl, IClock clock)
    {
        return new CachedReorgAuditStore(inner, ReorgAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgAudit> FindReorgAudits(IReorgAuditStore store, ReorgAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
