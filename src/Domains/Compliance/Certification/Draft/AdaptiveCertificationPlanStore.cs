namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive certification plans by code.</summary>
public interface IAdaptiveCertificationPlanStore
{
    AdaptiveCertificationPlan? Find(string code);
    void Save(AdaptiveCertificationPlan item);
    bool Remove(string code);
    List<AdaptiveCertificationPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCertificationPlanStore(Dictionary<string, AdaptiveCertificationPlan> items, List<string> order) : IAdaptiveCertificationPlanStore
{
    public Dictionary<string, AdaptiveCertificationPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCertificationPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCertificationPlan item)
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

    public List<AdaptiveCertificationPlan> List()
    {
        var outValue = new List<AdaptiveCertificationPlan>();
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
public sealed class CachedAdaptiveCertificationPlanStore(MemoryAdaptiveCertificationPlanStore inner, AdaptiveCertificationPlanCache cache, int hits, int misses) : IAdaptiveCertificationPlanStore
{
    public MemoryAdaptiveCertificationPlanStore Inner { get; set; } = inner;
    public AdaptiveCertificationPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCertificationPlan? Find(string code)
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

    public void Save(AdaptiveCertificationPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCertificationPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive certification plans. Empty fields match all.</summary>
public sealed record AdaptiveCertificationPlanQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCertificationPlan item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveCertificationPlan> Apply(List<AdaptiveCertificationPlan> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCertificationPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive certification plan store.</summary>
public static class AdaptiveCertificationPlanStore
{
    public static IAdaptiveCertificationPlanStore NewMemoryAdaptiveCertificationPlanStore()
    {
        return new MemoryAdaptiveCertificationPlanStore(
            new Dictionary<string, AdaptiveCertificationPlan>(),
            new List<string>());
    }

    public static IAdaptiveCertificationPlanStore NewCachedAdaptiveCertificationPlanStore(MemoryAdaptiveCertificationPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCertificationPlanStore(
            inner,
            AdaptiveCertificationPlanCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCertificationPlan> FindAdaptiveCertificationPlans(IAdaptiveCertificationPlanStore store, AdaptiveCertificationPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
