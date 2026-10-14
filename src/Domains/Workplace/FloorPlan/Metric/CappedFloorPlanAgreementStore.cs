namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped floor plan agreements by code.</summary>
public interface ICappedFloorPlanAgreementStore
{
    CappedFloorPlanAgreement? Find(string code);
    void Save(CappedFloorPlanAgreement item);
    bool Remove(string code);
    List<CappedFloorPlanAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedFloorPlanAgreementStore(Dictionary<string, CappedFloorPlanAgreement> items, List<string> order) : ICappedFloorPlanAgreementStore
{
    public Dictionary<string, CappedFloorPlanAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedFloorPlanAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedFloorPlanAgreement item)
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

    public List<CappedFloorPlanAgreement> List()
    {
        var outValue = new List<CappedFloorPlanAgreement>();
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
public sealed class CachedCappedFloorPlanAgreementStore(MemoryCappedFloorPlanAgreementStore inner, CappedFloorPlanAgreementCache cache, int hits, int misses) : ICappedFloorPlanAgreementStore
{
    public MemoryCappedFloorPlanAgreementStore Inner { get; set; } = inner;
    public CappedFloorPlanAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedFloorPlanAgreement? Find(string code)
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

    public void Save(CappedFloorPlanAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedFloorPlanAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped floor plan agreements. Empty fields match all.</summary>
public sealed record CappedFloorPlanAgreementQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(CappedFloorPlanAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedFloorPlanAgreement> Apply(List<CappedFloorPlanAgreement> items)
    {
        var matched = Collections.Filter(items, (CappedFloorPlanAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped floor plan agreement store.</summary>
public static class CappedFloorPlanAgreementStore
{
    public static ICappedFloorPlanAgreementStore NewMemoryCappedFloorPlanAgreementStore()
    {
        return new MemoryCappedFloorPlanAgreementStore(
            new Dictionary<string, CappedFloorPlanAgreement>(),
            new List<string>());
    }

    public static ICappedFloorPlanAgreementStore NewCachedCappedFloorPlanAgreementStore(MemoryCappedFloorPlanAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedCappedFloorPlanAgreementStore(
            inner,
            CappedFloorPlanAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedFloorPlanAgreement> FindCappedFloorPlanAgreements(ICappedFloorPlanAgreementStore store, CappedFloorPlanAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
