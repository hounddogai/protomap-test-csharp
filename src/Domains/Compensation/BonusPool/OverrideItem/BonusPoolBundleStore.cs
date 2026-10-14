namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool bundles by code.</summary>
public interface IBonusPoolBundleStore
{
    BonusPoolBundle? Find(string code);
    void Save(BonusPoolBundle item);
    bool Remove(string code);
    List<BonusPoolBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolBundleStore(Dictionary<string, BonusPoolBundle> items, List<string> order) : IBonusPoolBundleStore
{
    public Dictionary<string, BonusPoolBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolBundle item)
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

    public List<BonusPoolBundle> List()
    {
        var outValue = new List<BonusPoolBundle>();
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
public sealed class CachedBonusPoolBundleStore(MemoryBonusPoolBundleStore inner, BonusPoolBundleCache cache, int hits, int misses) : IBonusPoolBundleStore
{
    public MemoryBonusPoolBundleStore Inner { get; set; } = inner;
    public BonusPoolBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolBundle? Find(string code)
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

    public void Save(BonusPoolBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool bundles. Empty fields match all.</summary>
public sealed record BonusPoolBundleQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolBundle> Apply(List<BonusPoolBundle> items)
    {
        var matched = Collections.Filter(items, (BonusPoolBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool bundle store.</summary>
public static class BonusPoolBundleStore
{
    public static IBonusPoolBundleStore NewMemoryBonusPoolBundleStore()
    {
        return new MemoryBonusPoolBundleStore(new Dictionary<string, BonusPoolBundle>(), new List<string>());
    }

    public static IBonusPoolBundleStore NewCachedBonusPoolBundleStore(MemoryBonusPoolBundleStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolBundleStore(inner, BonusPoolBundleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolBundle> FindBonusPoolBundles(IBonusPoolBundleStore store, BonusPoolBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
