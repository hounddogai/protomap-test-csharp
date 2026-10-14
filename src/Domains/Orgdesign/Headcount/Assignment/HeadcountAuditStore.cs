namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount audits by code.</summary>
public interface IHeadcountAuditStore
{
    HeadcountAudit? Find(string code);
    void Save(HeadcountAudit item);
    bool Remove(string code);
    List<HeadcountAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountAuditStore(Dictionary<string, HeadcountAudit> items, List<string> order) : IHeadcountAuditStore
{
    public Dictionary<string, HeadcountAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountAudit item)
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

    public List<HeadcountAudit> List()
    {
        var outValue = new List<HeadcountAudit>();
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
public sealed class CachedHeadcountAuditStore(MemoryHeadcountAuditStore inner, HeadcountAuditCache cache, int hits, int misses) : IHeadcountAuditStore
{
    public MemoryHeadcountAuditStore Inner { get; set; } = inner;
    public HeadcountAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountAudit? Find(string code)
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

    public void Save(HeadcountAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount audits. Empty fields match all.</summary>
public sealed record HeadcountAuditQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountAudit item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountAudit> Apply(List<HeadcountAudit> items)
    {
        var matched = Collections.Filter(items, (HeadcountAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount audit store.</summary>
public static class HeadcountAuditStore
{
    public static IHeadcountAuditStore NewMemoryHeadcountAuditStore()
    {
        return new MemoryHeadcountAuditStore(new Dictionary<string, HeadcountAudit>(), new List<string>());
    }

    public static IHeadcountAuditStore NewCachedHeadcountAuditStore(MemoryHeadcountAuditStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountAuditStore(inner, HeadcountAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountAudit> FindHeadcountAudits(IHeadcountAuditStore store, HeadcountAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
