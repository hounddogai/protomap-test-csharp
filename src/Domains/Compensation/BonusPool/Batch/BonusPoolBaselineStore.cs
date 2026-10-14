namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool baselines by code.</summary>
public interface IBonusPoolBaselineStore
{
    BonusPoolBaseline? Find(string code);
    void Save(BonusPoolBaseline item);
    bool Remove(string code);
    List<BonusPoolBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolBaselineStore(Dictionary<string, BonusPoolBaseline> items, List<string> order) : IBonusPoolBaselineStore
{
    public Dictionary<string, BonusPoolBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolBaseline item)
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

    public List<BonusPoolBaseline> List()
    {
        var outValue = new List<BonusPoolBaseline>();
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
public sealed class CachedBonusPoolBaselineStore(MemoryBonusPoolBaselineStore inner, BonusPoolBaselineCache cache, int hits, int misses) : IBonusPoolBaselineStore
{
    public MemoryBonusPoolBaselineStore Inner { get; set; } = inner;
    public BonusPoolBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolBaseline? Find(string code)
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

    public void Save(BonusPoolBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool baselines. Empty fields match all.</summary>
public sealed record BonusPoolBaselineQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolBaseline item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<BonusPoolBaseline> Apply(List<BonusPoolBaseline> items)
    {
        var matched = Collections.Filter(items, (BonusPoolBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool baseline store.</summary>
public static class BonusPoolBaselineStore
{
    public static IBonusPoolBaselineStore NewMemoryBonusPoolBaselineStore()
    {
        return new MemoryBonusPoolBaselineStore(new Dictionary<string, BonusPoolBaseline>(), new List<string>());
    }

    public static IBonusPoolBaselineStore NewCachedBonusPoolBaselineStore(MemoryBonusPoolBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolBaselineStore(inner, BonusPoolBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolBaseline> FindBonusPoolBaselines(IBonusPoolBaselineStore store, BonusPoolBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
