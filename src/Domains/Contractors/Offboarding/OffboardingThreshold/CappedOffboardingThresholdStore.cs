namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped offboarding thresholds by code.</summary>
public interface ICappedOffboardingThresholdStore
{
    CappedOffboardingThreshold? Find(string code);
    void Save(CappedOffboardingThreshold item);
    bool Remove(string code);
    List<CappedOffboardingThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedOffboardingThresholdStore(Dictionary<string, CappedOffboardingThreshold> items, List<string> order) : ICappedOffboardingThresholdStore
{
    public Dictionary<string, CappedOffboardingThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedOffboardingThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedOffboardingThreshold item)
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

    public List<CappedOffboardingThreshold> List()
    {
        var outValue = new List<CappedOffboardingThreshold>();
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
public sealed class CachedCappedOffboardingThresholdStore(MemoryCappedOffboardingThresholdStore inner, CappedOffboardingThresholdCache cache, int hits, int misses) : ICappedOffboardingThresholdStore
{
    public MemoryCappedOffboardingThresholdStore Inner { get; set; } = inner;
    public CappedOffboardingThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedOffboardingThreshold? Find(string code)
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

    public void Save(CappedOffboardingThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedOffboardingThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped offboarding thresholds. Empty fields match all.</summary>
public sealed record CappedOffboardingThresholdQuery(string Name, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedOffboardingThreshold item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedOffboardingThreshold> Apply(List<CappedOffboardingThreshold> items)
    {
        var matched = Collections.Filter(items, (CappedOffboardingThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped offboarding threshold store.</summary>
public static class CappedOffboardingThresholdStore
{
    public static ICappedOffboardingThresholdStore NewMemoryCappedOffboardingThresholdStore()
    {
        return new MemoryCappedOffboardingThresholdStore(
            new Dictionary<string, CappedOffboardingThreshold>(),
            new List<string>());
    }

    public static ICappedOffboardingThresholdStore NewCachedCappedOffboardingThresholdStore(MemoryCappedOffboardingThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedCappedOffboardingThresholdStore(
            inner,
            CappedOffboardingThresholdCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedOffboardingThreshold> FindCappedOffboardingThresholds(ICappedOffboardingThresholdStore store, CappedOffboardingThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
