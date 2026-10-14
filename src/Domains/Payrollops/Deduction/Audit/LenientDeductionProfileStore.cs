namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient deduction profiles by code.</summary>
public interface ILenientDeductionProfileStore
{
    LenientDeductionProfile? Find(string code);
    void Save(LenientDeductionProfile item);
    bool Remove(string code);
    List<LenientDeductionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeductionProfileStore(Dictionary<string, LenientDeductionProfile> items, List<string> order) : ILenientDeductionProfileStore
{
    public Dictionary<string, LenientDeductionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeductionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeductionProfile item)
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

    public List<LenientDeductionProfile> List()
    {
        var outValue = new List<LenientDeductionProfile>();
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
public sealed class CachedLenientDeductionProfileStore(MemoryLenientDeductionProfileStore inner, LenientDeductionProfileCache cache, int hits, int misses) : ILenientDeductionProfileStore
{
    public MemoryLenientDeductionProfileStore Inner { get; set; } = inner;
    public LenientDeductionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeductionProfile? Find(string code)
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

    public void Save(LenientDeductionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeductionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient deduction profiles. Empty fields match all.</summary>
public sealed record LenientDeductionProfileQuery(string Category, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeductionProfile item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeductionProfile> Apply(List<LenientDeductionProfile> items)
    {
        var matched = Collections.Filter(items, (LenientDeductionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient deduction profile store.</summary>
public static class LenientDeductionProfileStore
{
    public static ILenientDeductionProfileStore NewMemoryLenientDeductionProfileStore()
    {
        return new MemoryLenientDeductionProfileStore(
            new Dictionary<string, LenientDeductionProfile>(),
            new List<string>());
    }

    public static ILenientDeductionProfileStore NewCachedLenientDeductionProfileStore(MemoryLenientDeductionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeductionProfileStore(
            inner,
            LenientDeductionProfileCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientDeductionProfile> FindLenientDeductionProfiles(ILenientDeductionProfileStore store, LenientDeductionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
