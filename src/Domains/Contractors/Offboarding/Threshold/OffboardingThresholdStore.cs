namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding thresholds by code.</summary>
public interface IOffboardingThresholdStore
{
    OffboardingThreshold? Find(string code);
    void Save(OffboardingThreshold item);
    bool Remove(string code);
    List<OffboardingThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingThresholdStore(Dictionary<string, OffboardingThreshold> items, List<string> order) : IOffboardingThresholdStore
{
    public Dictionary<string, OffboardingThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingThreshold item)
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

    public List<OffboardingThreshold> List()
    {
        var outValue = new List<OffboardingThreshold>();
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
public sealed class CachedOffboardingThresholdStore(MemoryOffboardingThresholdStore inner, OffboardingThresholdCache cache, int hits, int misses) : IOffboardingThresholdStore
{
    public MemoryOffboardingThresholdStore Inner { get; set; } = inner;
    public OffboardingThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingThreshold? Find(string code)
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

    public void Save(OffboardingThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding thresholds. Empty fields match all.</summary>
public sealed record OffboardingThresholdQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingThreshold item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingThreshold> Apply(List<OffboardingThreshold> items)
    {
        var matched = Collections.Filter(items, (OffboardingThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding threshold store.</summary>
public static class OffboardingThresholdStore
{
    public static IOffboardingThresholdStore NewMemoryOffboardingThresholdStore()
    {
        return new MemoryOffboardingThresholdStore(
            new Dictionary<string, OffboardingThreshold>(),
            new List<string>());
    }

    public static IOffboardingThresholdStore NewCachedOffboardingThresholdStore(MemoryOffboardingThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingThresholdStore(inner, OffboardingThresholdCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OffboardingThreshold> FindOffboardingThresholds(IOffboardingThresholdStore store, OffboardingThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
