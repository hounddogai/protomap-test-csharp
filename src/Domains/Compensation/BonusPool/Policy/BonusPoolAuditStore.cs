namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool audits by code.</summary>
public interface IBonusPoolAuditStore
{
    BonusPoolAudit? Find(string code);
    void Save(BonusPoolAudit item);
    bool Remove(string code);
    List<BonusPoolAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolAuditStore(Dictionary<string, BonusPoolAudit> items, List<string> order) : IBonusPoolAuditStore
{
    public Dictionary<string, BonusPoolAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolAudit item)
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

    public List<BonusPoolAudit> List()
    {
        var outValue = new List<BonusPoolAudit>();
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
public sealed class CachedBonusPoolAuditStore(MemoryBonusPoolAuditStore inner, BonusPoolAuditCache cache, int hits, int misses) : IBonusPoolAuditStore
{
    public MemoryBonusPoolAuditStore Inner { get; set; } = inner;
    public BonusPoolAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolAudit? Find(string code)
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

    public void Save(BonusPoolAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool audits. Empty fields match all.</summary>
public sealed record BonusPoolAuditQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<BonusPoolAudit> Apply(List<BonusPoolAudit> items)
    {
        var matched = Collections.Filter(items, (BonusPoolAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool audit store.</summary>
public static class BonusPoolAuditStore
{
    public static IBonusPoolAuditStore NewMemoryBonusPoolAuditStore()
    {
        return new MemoryBonusPoolAuditStore(new Dictionary<string, BonusPoolAudit>(), new List<string>());
    }

    public static IBonusPoolAuditStore NewCachedBonusPoolAuditStore(MemoryBonusPoolAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolAuditStore(inner, BonusPoolAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BonusPoolAudit> FindBonusPoolAudits(IBonusPoolAuditStore store, BonusPoolAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
