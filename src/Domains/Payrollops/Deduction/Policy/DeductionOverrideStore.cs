namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction overrides by code.</summary>
public interface IDeductionOverrideStore
{
    DeductionOverride? Find(string code);
    void Save(DeductionOverride item);
    bool Remove(string code);
    List<DeductionOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionOverrideStore(Dictionary<string, DeductionOverride> items, List<string> order) : IDeductionOverrideStore
{
    public Dictionary<string, DeductionOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionOverride item)
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

    public List<DeductionOverride> List()
    {
        var outValue = new List<DeductionOverride>();
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
public sealed class CachedDeductionOverrideStore(MemoryDeductionOverrideStore inner, DeductionOverrideCache cache, int hits, int misses) : IDeductionOverrideStore
{
    public MemoryDeductionOverrideStore Inner { get; set; } = inner;
    public DeductionOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionOverride? Find(string code)
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

    public void Save(DeductionOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction overrides. Empty fields match all.</summary>
public sealed record DeductionOverrideQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionOverride> Apply(List<DeductionOverride> items)
    {
        var matched = Collections.Filter(items, (DeductionOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction override store.</summary>
public static class DeductionOverrideStore
{
    public static IDeductionOverrideStore NewMemoryDeductionOverrideStore()
    {
        return new MemoryDeductionOverrideStore(new Dictionary<string, DeductionOverride>(), new List<string>());
    }

    public static IDeductionOverrideStore NewCachedDeductionOverrideStore(MemoryDeductionOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionOverrideStore(inner, DeductionOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionOverride> FindDeductionOverrides(IDeductionOverrideStore store, DeductionOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
