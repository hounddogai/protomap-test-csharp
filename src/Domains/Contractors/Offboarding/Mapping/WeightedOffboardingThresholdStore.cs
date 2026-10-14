namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted offboarding thresholds by code.</summary>
public interface IWeightedOffboardingThresholdStore
{
    WeightedOffboardingThreshold? Find(string code);
    void Save(WeightedOffboardingThreshold item);
    bool Remove(string code);
    List<WeightedOffboardingThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedOffboardingThresholdStore(Dictionary<string, WeightedOffboardingThreshold> items, List<string> order) : IWeightedOffboardingThresholdStore
{
    public Dictionary<string, WeightedOffboardingThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedOffboardingThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedOffboardingThreshold item)
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

    public List<WeightedOffboardingThreshold> List()
    {
        var outValue = new List<WeightedOffboardingThreshold>();
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
public sealed class CachedWeightedOffboardingThresholdStore(MemoryWeightedOffboardingThresholdStore inner, WeightedOffboardingThresholdCache cache, int hits, int misses) : IWeightedOffboardingThresholdStore
{
    public MemoryWeightedOffboardingThresholdStore Inner { get; set; } = inner;
    public WeightedOffboardingThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedOffboardingThreshold? Find(string code)
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

    public void Save(WeightedOffboardingThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedOffboardingThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted offboarding thresholds. Empty fields match all.</summary>
public sealed record WeightedOffboardingThresholdQuery(string Unit, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedOffboardingThreshold item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedOffboardingThreshold> Apply(List<WeightedOffboardingThreshold> items)
    {
        var matched = Collections.Filter(items, (WeightedOffboardingThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted offboarding threshold store.</summary>
public static class WeightedOffboardingThresholdStore
{
    public static IWeightedOffboardingThresholdStore NewMemoryWeightedOffboardingThresholdStore()
    {
        return new MemoryWeightedOffboardingThresholdStore(
            new Dictionary<string, WeightedOffboardingThreshold>(),
            new List<string>());
    }

    public static IWeightedOffboardingThresholdStore NewCachedWeightedOffboardingThresholdStore(MemoryWeightedOffboardingThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedOffboardingThresholdStore(
            inner,
            WeightedOffboardingThresholdCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedOffboardingThreshold> FindWeightedOffboardingThresholds(IWeightedOffboardingThresholdStore store, WeightedOffboardingThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
