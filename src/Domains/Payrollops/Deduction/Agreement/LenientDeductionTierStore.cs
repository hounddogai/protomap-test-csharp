namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient deduction tiers by code.</summary>
public interface ILenientDeductionTierStore
{
    LenientDeductionTier? Find(string code);
    void Save(LenientDeductionTier item);
    bool Remove(string code);
    List<LenientDeductionTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeductionTierStore(Dictionary<string, LenientDeductionTier> items, List<string> order) : ILenientDeductionTierStore
{
    public Dictionary<string, LenientDeductionTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeductionTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeductionTier item)
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

    public List<LenientDeductionTier> List()
    {
        var outValue = new List<LenientDeductionTier>();
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
public sealed class CachedLenientDeductionTierStore(MemoryLenientDeductionTierStore inner, LenientDeductionTierCache cache, int hits, int misses) : ILenientDeductionTierStore
{
    public MemoryLenientDeductionTierStore Inner { get; set; } = inner;
    public LenientDeductionTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeductionTier? Find(string code)
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

    public void Save(LenientDeductionTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeductionTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient deduction tiers. Empty fields match all.</summary>
public sealed record LenientDeductionTierQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeductionTier item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeductionTier> Apply(List<LenientDeductionTier> items)
    {
        var matched = Collections.Filter(items, (LenientDeductionTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient deduction tier store.</summary>
public static class LenientDeductionTierStore
{
    public static ILenientDeductionTierStore NewMemoryLenientDeductionTierStore()
    {
        return new MemoryLenientDeductionTierStore(
            new Dictionary<string, LenientDeductionTier>(),
            new List<string>());
    }

    public static ILenientDeductionTierStore NewCachedLenientDeductionTierStore(MemoryLenientDeductionTierStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeductionTierStore(inner, LenientDeductionTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientDeductionTier> FindLenientDeductionTiers(ILenientDeductionTierStore store, LenientDeductionTierQuery query)
    {
        return query.Apply(store.List());
    }
}
