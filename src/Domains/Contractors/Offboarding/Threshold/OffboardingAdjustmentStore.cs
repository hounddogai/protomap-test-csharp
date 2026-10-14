namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding adjustments by code.</summary>
public interface IOffboardingAdjustmentStore
{
    OffboardingAdjustment? Find(string code);
    void Save(OffboardingAdjustment item);
    bool Remove(string code);
    List<OffboardingAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingAdjustmentStore(Dictionary<string, OffboardingAdjustment> items, List<string> order) : IOffboardingAdjustmentStore
{
    public Dictionary<string, OffboardingAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingAdjustment item)
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

    public List<OffboardingAdjustment> List()
    {
        var outValue = new List<OffboardingAdjustment>();
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
public sealed class CachedOffboardingAdjustmentStore(MemoryOffboardingAdjustmentStore inner, OffboardingAdjustmentCache cache, int hits, int misses) : IOffboardingAdjustmentStore
{
    public MemoryOffboardingAdjustmentStore Inner { get; set; } = inner;
    public OffboardingAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingAdjustment? Find(string code)
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

    public void Save(OffboardingAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding adjustments. Empty fields match all.</summary>
public sealed record OffboardingAdjustmentQuery(string Department, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingAdjustment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingAdjustment> Apply(List<OffboardingAdjustment> items)
    {
        var matched = Collections.Filter(items, (OffboardingAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding adjustment store.</summary>
public static class OffboardingAdjustmentStore
{
    public static IOffboardingAdjustmentStore NewMemoryOffboardingAdjustmentStore()
    {
        return new MemoryOffboardingAdjustmentStore(
            new Dictionary<string, OffboardingAdjustment>(),
            new List<string>());
    }

    public static IOffboardingAdjustmentStore NewCachedOffboardingAdjustmentStore(MemoryOffboardingAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingAdjustmentStore(inner, OffboardingAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OffboardingAdjustment> FindOffboardingAdjustments(IOffboardingAdjustmentStore store, OffboardingAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
