namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary deduction overrides by code.</summary>
public interface IPrimaryDeductionOverrideStore
{
    PrimaryDeductionOverride? Find(string code);
    void Save(PrimaryDeductionOverride item);
    bool Remove(string code);
    List<PrimaryDeductionOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryDeductionOverrideStore(Dictionary<string, PrimaryDeductionOverride> items, List<string> order) : IPrimaryDeductionOverrideStore
{
    public Dictionary<string, PrimaryDeductionOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryDeductionOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryDeductionOverride item)
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

    public List<PrimaryDeductionOverride> List()
    {
        var outValue = new List<PrimaryDeductionOverride>();
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
public sealed class CachedPrimaryDeductionOverrideStore(MemoryPrimaryDeductionOverrideStore inner, PrimaryDeductionOverrideCache cache, int hits, int misses) : IPrimaryDeductionOverrideStore
{
    public MemoryPrimaryDeductionOverrideStore Inner { get; set; } = inner;
    public PrimaryDeductionOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryDeductionOverride? Find(string code)
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

    public void Save(PrimaryDeductionOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryDeductionOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary deduction overrides. Empty fields match all.</summary>
public sealed record PrimaryDeductionOverrideQuery(string Locale, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryDeductionOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryDeductionOverride> Apply(List<PrimaryDeductionOverride> items)
    {
        var matched = Collections.Filter(items, (PrimaryDeductionOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary deduction override store.</summary>
public static class PrimaryDeductionOverrideStore
{
    public static IPrimaryDeductionOverrideStore NewMemoryPrimaryDeductionOverrideStore()
    {
        return new MemoryPrimaryDeductionOverrideStore(
            new Dictionary<string, PrimaryDeductionOverride>(),
            new List<string>());
    }

    public static IPrimaryDeductionOverrideStore NewCachedPrimaryDeductionOverrideStore(MemoryPrimaryDeductionOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryDeductionOverrideStore(
            inner,
            PrimaryDeductionOverrideCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryDeductionOverride> FindPrimaryDeductionOverrides(IPrimaryDeductionOverrideStore store, PrimaryDeductionOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
