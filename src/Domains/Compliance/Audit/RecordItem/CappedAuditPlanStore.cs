namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped audit plans by code.</summary>
public interface ICappedAuditPlanStore
{
    CappedAuditPlan? Find(string code);
    void Save(CappedAuditPlan item);
    bool Remove(string code);
    List<CappedAuditPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAuditPlanStore(Dictionary<string, CappedAuditPlan> items, List<string> order) : ICappedAuditPlanStore
{
    public Dictionary<string, CappedAuditPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAuditPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAuditPlan item)
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

    public List<CappedAuditPlan> List()
    {
        var outValue = new List<CappedAuditPlan>();
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
public sealed class CachedCappedAuditPlanStore(MemoryCappedAuditPlanStore inner, CappedAuditPlanCache cache, int hits, int misses) : ICappedAuditPlanStore
{
    public MemoryCappedAuditPlanStore Inner { get; set; } = inner;
    public CappedAuditPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAuditPlan? Find(string code)
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

    public void Save(CappedAuditPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAuditPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped audit plans. Empty fields match all.</summary>
public sealed record CappedAuditPlanQuery(string Unit, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAuditPlan item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAuditPlan> Apply(List<CappedAuditPlan> items)
    {
        var matched = Collections.Filter(items, (CappedAuditPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped audit plan store.</summary>
public static class CappedAuditPlanStore
{
    public static ICappedAuditPlanStore NewMemoryCappedAuditPlanStore()
    {
        return new MemoryCappedAuditPlanStore(new Dictionary<string, CappedAuditPlan>(), new List<string>());
    }

    public static ICappedAuditPlanStore NewCachedCappedAuditPlanStore(MemoryCappedAuditPlanStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAuditPlanStore(inner, CappedAuditPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedAuditPlan> FindCappedAuditPlans(ICappedAuditPlanStore store, CappedAuditPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
