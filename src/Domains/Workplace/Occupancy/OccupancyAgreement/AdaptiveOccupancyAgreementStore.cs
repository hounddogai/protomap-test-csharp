namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive occupancy agreements by code.</summary>
public interface IAdaptiveOccupancyAgreementStore
{
    AdaptiveOccupancyAgreement? Find(string code);
    void Save(AdaptiveOccupancyAgreement item);
    bool Remove(string code);
    List<AdaptiveOccupancyAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveOccupancyAgreementStore(Dictionary<string, AdaptiveOccupancyAgreement> items, List<string> order) : IAdaptiveOccupancyAgreementStore
{
    public Dictionary<string, AdaptiveOccupancyAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveOccupancyAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveOccupancyAgreement item)
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

    public List<AdaptiveOccupancyAgreement> List()
    {
        var outValue = new List<AdaptiveOccupancyAgreement>();
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
public sealed class CachedAdaptiveOccupancyAgreementStore(MemoryAdaptiveOccupancyAgreementStore inner, AdaptiveOccupancyAgreementCache cache, int hits, int misses) : IAdaptiveOccupancyAgreementStore
{
    public MemoryAdaptiveOccupancyAgreementStore Inner { get; set; } = inner;
    public AdaptiveOccupancyAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveOccupancyAgreement? Find(string code)
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

    public void Save(AdaptiveOccupancyAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveOccupancyAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive occupancy agreements. Empty fields match all.</summary>
public sealed record AdaptiveOccupancyAgreementQuery(string Department, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveOccupancyAgreement item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveOccupancyAgreement> Apply(List<AdaptiveOccupancyAgreement> items)
    {
        var matched = Collections.Filter(items, (AdaptiveOccupancyAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive occupancy agreement store.</summary>
public static class AdaptiveOccupancyAgreementStore
{
    public static IAdaptiveOccupancyAgreementStore NewMemoryAdaptiveOccupancyAgreementStore()
    {
        return new MemoryAdaptiveOccupancyAgreementStore(
            new Dictionary<string, AdaptiveOccupancyAgreement>(),
            new List<string>());
    }

    public static IAdaptiveOccupancyAgreementStore NewCachedAdaptiveOccupancyAgreementStore(MemoryAdaptiveOccupancyAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveOccupancyAgreementStore(
            inner,
            AdaptiveOccupancyAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveOccupancyAgreement> FindAdaptiveOccupancyAgreements(IAdaptiveOccupancyAgreementStore store, AdaptiveOccupancyAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
