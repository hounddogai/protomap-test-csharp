namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pool audits by code.</summary>
public interface ICappedPoolAuditStore
{
    CappedPoolAudit? Find(string code);
    void Save(CappedPoolAudit item);
    bool Remove(string code);
    List<CappedPoolAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPoolAuditStore(Dictionary<string, CappedPoolAudit> items, List<string> order) : ICappedPoolAuditStore
{
    public Dictionary<string, CappedPoolAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPoolAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPoolAudit item)
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

    public List<CappedPoolAudit> List()
    {
        var outValue = new List<CappedPoolAudit>();
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
public sealed class CachedCappedPoolAuditStore(MemoryCappedPoolAuditStore inner, CappedPoolAuditCache cache, int hits, int misses) : ICappedPoolAuditStore
{
    public MemoryCappedPoolAuditStore Inner { get; set; } = inner;
    public CappedPoolAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPoolAudit? Find(string code)
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

    public void Save(CappedPoolAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPoolAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pool audits. Empty fields match all.</summary>
public sealed record CappedPoolAuditQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPoolAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPoolAudit> Apply(List<CappedPoolAudit> items)
    {
        var matched = Collections.Filter(items, (CappedPoolAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pool audit store.</summary>
public static class CappedPoolAuditStore
{
    public static ICappedPoolAuditStore NewMemoryCappedPoolAuditStore()
    {
        return new MemoryCappedPoolAuditStore(new Dictionary<string, CappedPoolAudit>(), new List<string>());
    }

    public static ICappedPoolAuditStore NewCachedCappedPoolAuditStore(MemoryCappedPoolAuditStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPoolAuditStore(inner, CappedPoolAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedPoolAudit> FindCappedPoolAudits(ICappedPoolAuditStore store, CappedPoolAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
