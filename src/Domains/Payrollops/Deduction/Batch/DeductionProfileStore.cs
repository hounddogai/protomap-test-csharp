namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction profiles by code.</summary>
public interface IDeductionProfileStore
{
    DeductionProfile? Find(string code);
    void Save(DeductionProfile item);
    bool Remove(string code);
    List<DeductionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionProfileStore(Dictionary<string, DeductionProfile> items, List<string> order) : IDeductionProfileStore
{
    public Dictionary<string, DeductionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionProfile item)
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

    public List<DeductionProfile> List()
    {
        var outValue = new List<DeductionProfile>();
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
public sealed class CachedDeductionProfileStore(MemoryDeductionProfileStore inner, DeductionProfileCache cache, int hits, int misses) : IDeductionProfileStore
{
    public MemoryDeductionProfileStore Inner { get; set; } = inner;
    public DeductionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionProfile? Find(string code)
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

    public void Save(DeductionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction profiles. Empty fields match all.</summary>
public sealed record DeductionProfileQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DeductionProfile> Apply(List<DeductionProfile> items)
    {
        var matched = Collections.Filter(items, (DeductionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction profile store.</summary>
public static class DeductionProfileStore
{
    public static IDeductionProfileStore NewMemoryDeductionProfileStore()
    {
        return new MemoryDeductionProfileStore(new Dictionary<string, DeductionProfile>(), new List<string>());
    }

    public static IDeductionProfileStore NewCachedDeductionProfileStore(MemoryDeductionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionProfileStore(inner, DeductionProfileCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeductionProfile> FindDeductionProfiles(IDeductionProfileStore store, DeductionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
