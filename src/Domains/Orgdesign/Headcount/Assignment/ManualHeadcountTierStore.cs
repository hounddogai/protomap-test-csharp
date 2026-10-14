namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual headcount tiers by code.</summary>
public interface IManualHeadcountTierStore
{
    ManualHeadcountTier? Find(string code);
    void Save(ManualHeadcountTier item);
    bool Remove(string code);
    List<ManualHeadcountTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualHeadcountTierStore(Dictionary<string, ManualHeadcountTier> items, List<string> order) : IManualHeadcountTierStore
{
    public Dictionary<string, ManualHeadcountTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualHeadcountTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualHeadcountTier item)
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

    public List<ManualHeadcountTier> List()
    {
        var outValue = new List<ManualHeadcountTier>();
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
public sealed class CachedManualHeadcountTierStore(MemoryManualHeadcountTierStore inner, ManualHeadcountTierCache cache, int hits, int misses) : IManualHeadcountTierStore
{
    public MemoryManualHeadcountTierStore Inner { get; set; } = inner;
    public ManualHeadcountTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualHeadcountTier? Find(string code)
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

    public void Save(ManualHeadcountTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualHeadcountTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual headcount tiers. Empty fields match all.</summary>
public sealed record ManualHeadcountTierQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualHeadcountTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<ManualHeadcountTier> Apply(List<ManualHeadcountTier> items)
    {
        var matched = Collections.Filter(items, (ManualHeadcountTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual headcount tier store.</summary>
public static class ManualHeadcountTierStore
{
    public static IManualHeadcountTierStore NewMemoryManualHeadcountTierStore()
    {
        return new MemoryManualHeadcountTierStore(new Dictionary<string, ManualHeadcountTier>(), new List<string>());
    }

    public static IManualHeadcountTierStore NewCachedManualHeadcountTierStore(MemoryManualHeadcountTierStore inner, int ttl, IClock clock)
    {
        return new CachedManualHeadcountTierStore(inner, ManualHeadcountTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualHeadcountTier> FindManualHeadcountTiers(IManualHeadcountTierStore store, ManualHeadcountTierQuery query)
    {
        return query.Apply(store.List());
    }
}
