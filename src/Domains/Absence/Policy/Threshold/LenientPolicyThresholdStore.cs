namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient policy thresholds by code.</summary>
public interface ILenientPolicyThresholdStore
{
    LenientPolicyThreshold? Find(string code);
    void Save(LenientPolicyThreshold item);
    bool Remove(string code);
    List<LenientPolicyThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPolicyThresholdStore(Dictionary<string, LenientPolicyThreshold> items, List<string> order) : ILenientPolicyThresholdStore
{
    public Dictionary<string, LenientPolicyThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPolicyThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPolicyThreshold item)
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

    public List<LenientPolicyThreshold> List()
    {
        var outValue = new List<LenientPolicyThreshold>();
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
public sealed class CachedLenientPolicyThresholdStore(MemoryLenientPolicyThresholdStore inner, LenientPolicyThresholdCache cache, int hits, int misses) : ILenientPolicyThresholdStore
{
    public MemoryLenientPolicyThresholdStore Inner { get; set; } = inner;
    public LenientPolicyThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPolicyThreshold? Find(string code)
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

    public void Save(LenientPolicyThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPolicyThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient policy thresholds. Empty fields match all.</summary>
public sealed record LenientPolicyThresholdQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPolicyThreshold item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPolicyThreshold> Apply(List<LenientPolicyThreshold> items)
    {
        var matched = Collections.Filter(items, (LenientPolicyThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient policy threshold store.</summary>
public static class LenientPolicyThresholdStore
{
    public static ILenientPolicyThresholdStore NewMemoryLenientPolicyThresholdStore()
    {
        return new MemoryLenientPolicyThresholdStore(
            new Dictionary<string, LenientPolicyThreshold>(),
            new List<string>());
    }

    public static ILenientPolicyThresholdStore NewCachedLenientPolicyThresholdStore(MemoryLenientPolicyThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPolicyThresholdStore(
            inner,
            LenientPolicyThresholdCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientPolicyThreshold> FindLenientPolicyThresholds(ILenientPolicyThresholdStore store, LenientPolicyThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
