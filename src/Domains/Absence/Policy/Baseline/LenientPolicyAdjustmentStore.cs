namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient policy adjustments by code.</summary>
public interface ILenientPolicyAdjustmentStore
{
    LenientPolicyAdjustment? Find(string code);
    void Save(LenientPolicyAdjustment item);
    bool Remove(string code);
    List<LenientPolicyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPolicyAdjustmentStore(Dictionary<string, LenientPolicyAdjustment> items, List<string> order) : ILenientPolicyAdjustmentStore
{
    public Dictionary<string, LenientPolicyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPolicyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPolicyAdjustment item)
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

    public List<LenientPolicyAdjustment> List()
    {
        var outValue = new List<LenientPolicyAdjustment>();
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
public sealed class CachedLenientPolicyAdjustmentStore(MemoryLenientPolicyAdjustmentStore inner, LenientPolicyAdjustmentCache cache, int hits, int misses) : ILenientPolicyAdjustmentStore
{
    public MemoryLenientPolicyAdjustmentStore Inner { get; set; } = inner;
    public LenientPolicyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPolicyAdjustment? Find(string code)
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

    public void Save(LenientPolicyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPolicyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient policy adjustments. Empty fields match all.</summary>
public sealed record LenientPolicyAdjustmentQuery(string Locale, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPolicyAdjustment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<LenientPolicyAdjustment> Apply(List<LenientPolicyAdjustment> items)
    {
        var matched = Collections.Filter(items, (LenientPolicyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient policy adjustment store.</summary>
public static class LenientPolicyAdjustmentStore
{
    public static ILenientPolicyAdjustmentStore NewMemoryLenientPolicyAdjustmentStore()
    {
        return new MemoryLenientPolicyAdjustmentStore(
            new Dictionary<string, LenientPolicyAdjustment>(),
            new List<string>());
    }

    public static ILenientPolicyAdjustmentStore NewCachedLenientPolicyAdjustmentStore(MemoryLenientPolicyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPolicyAdjustmentStore(
            inner,
            LenientPolicyAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientPolicyAdjustment> FindLenientPolicyAdjustments(ILenientPolicyAdjustmentStore store, LenientPolicyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
