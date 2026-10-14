namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped audit cycles by code.</summary>
public interface ICappedAuditCycleStore
{
    CappedAuditCycle? Find(string code);
    void Save(CappedAuditCycle item);
    bool Remove(string code);
    List<CappedAuditCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAuditCycleStore(Dictionary<string, CappedAuditCycle> items, List<string> order) : ICappedAuditCycleStore
{
    public Dictionary<string, CappedAuditCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAuditCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAuditCycle item)
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

    public List<CappedAuditCycle> List()
    {
        var outValue = new List<CappedAuditCycle>();
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
public sealed class CachedCappedAuditCycleStore(MemoryCappedAuditCycleStore inner, CappedAuditCycleCache cache, int hits, int misses) : ICappedAuditCycleStore
{
    public MemoryCappedAuditCycleStore Inner { get; set; } = inner;
    public CappedAuditCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAuditCycle? Find(string code)
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

    public void Save(CappedAuditCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAuditCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped audit cycles. Empty fields match all.</summary>
public sealed record CappedAuditCycleQuery(string Category, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAuditCycle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAuditCycle> Apply(List<CappedAuditCycle> items)
    {
        var matched = Collections.Filter(items, (CappedAuditCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped audit cycle store.</summary>
public static class CappedAuditCycleStore
{
    public static ICappedAuditCycleStore NewMemoryCappedAuditCycleStore()
    {
        return new MemoryCappedAuditCycleStore(new Dictionary<string, CappedAuditCycle>(), new List<string>());
    }

    public static ICappedAuditCycleStore NewCachedCappedAuditCycleStore(MemoryCappedAuditCycleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAuditCycleStore(inner, CappedAuditCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedAuditCycle> FindCappedAuditCycles(ICappedAuditCycleStore store, CappedAuditCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
