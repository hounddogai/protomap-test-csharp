namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient audit plans by code.</summary>
public interface ILenientAuditPlanStore
{
    LenientAuditPlan? Find(string code);
    void Save(LenientAuditPlan item);
    bool Remove(string code);
    List<LenientAuditPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAuditPlanStore(Dictionary<string, LenientAuditPlan> items, List<string> order) : ILenientAuditPlanStore
{
    public Dictionary<string, LenientAuditPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAuditPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAuditPlan item)
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

    public List<LenientAuditPlan> List()
    {
        var outValue = new List<LenientAuditPlan>();
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
public sealed class CachedLenientAuditPlanStore(MemoryLenientAuditPlanStore inner, LenientAuditPlanCache cache, int hits, int misses) : ILenientAuditPlanStore
{
    public MemoryLenientAuditPlanStore Inner { get; set; } = inner;
    public LenientAuditPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAuditPlan? Find(string code)
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

    public void Save(LenientAuditPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAuditPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient audit plans. Empty fields match all.</summary>
public sealed record LenientAuditPlanQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAuditPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAuditPlan> Apply(List<LenientAuditPlan> items)
    {
        var matched = Collections.Filter(items, (LenientAuditPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient audit plan store.</summary>
public static class LenientAuditPlanStore
{
    public static ILenientAuditPlanStore NewMemoryLenientAuditPlanStore()
    {
        return new MemoryLenientAuditPlanStore(new Dictionary<string, LenientAuditPlan>(), new List<string>());
    }

    public static ILenientAuditPlanStore NewCachedLenientAuditPlanStore(MemoryLenientAuditPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAuditPlanStore(inner, LenientAuditPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientAuditPlan> FindLenientAuditPlans(ILenientAuditPlanStore store, LenientAuditPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
