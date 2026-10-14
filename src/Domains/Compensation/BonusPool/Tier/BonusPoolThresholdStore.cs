namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool thresholds by code.</summary>
public interface IBonusPoolThresholdStore
{
    BonusPoolThreshold? Find(string code);
    void Save(BonusPoolThreshold item);
    bool Remove(string code);
    List<BonusPoolThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolThresholdStore(Dictionary<string, BonusPoolThreshold> items, List<string> order) : IBonusPoolThresholdStore
{
    public Dictionary<string, BonusPoolThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolThreshold item)
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

    public List<BonusPoolThreshold> List()
    {
        var outValue = new List<BonusPoolThreshold>();
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
public sealed class CachedBonusPoolThresholdStore(MemoryBonusPoolThresholdStore inner, BonusPoolThresholdCache cache, int hits, int misses) : IBonusPoolThresholdStore
{
    public MemoryBonusPoolThresholdStore Inner { get; set; } = inner;
    public BonusPoolThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolThreshold? Find(string code)
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

    public void Save(BonusPoolThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool thresholds. Empty fields match all.</summary>
public sealed record BonusPoolThresholdQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<BonusPoolThreshold> Apply(List<BonusPoolThreshold> items)
    {
        var matched = Collections.Filter(items, (BonusPoolThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool threshold store.</summary>
public static class BonusPoolThresholdStore
{
    public static IBonusPoolThresholdStore NewMemoryBonusPoolThresholdStore()
    {
        return new MemoryBonusPoolThresholdStore(new Dictionary<string, BonusPoolThreshold>(), new List<string>());
    }

    public static IBonusPoolThresholdStore NewCachedBonusPoolThresholdStore(MemoryBonusPoolThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolThresholdStore(inner, BonusPoolThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BonusPoolThreshold> FindBonusPoolThresholds(IBonusPoolThresholdStore store, BonusPoolThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
