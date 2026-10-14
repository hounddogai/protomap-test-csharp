namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool groups by code.</summary>
public interface IBonusPoolGroupStore
{
    BonusPoolGroup? Find(string code);
    void Save(BonusPoolGroup item);
    bool Remove(string code);
    List<BonusPoolGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolGroupStore(Dictionary<string, BonusPoolGroup> items, List<string> order) : IBonusPoolGroupStore
{
    public Dictionary<string, BonusPoolGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolGroup item)
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

    public List<BonusPoolGroup> List()
    {
        var outValue = new List<BonusPoolGroup>();
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
public sealed class CachedBonusPoolGroupStore(MemoryBonusPoolGroupStore inner, BonusPoolGroupCache cache, int hits, int misses) : IBonusPoolGroupStore
{
    public MemoryBonusPoolGroupStore Inner { get; set; } = inner;
    public BonusPoolGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolGroup? Find(string code)
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

    public void Save(BonusPoolGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool groups. Empty fields match all.</summary>
public sealed record BonusPoolGroupQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolGroup item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolGroup> Apply(List<BonusPoolGroup> items)
    {
        var matched = Collections.Filter(items, (BonusPoolGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool group store.</summary>
public static class BonusPoolGroupStore
{
    public static IBonusPoolGroupStore NewMemoryBonusPoolGroupStore()
    {
        return new MemoryBonusPoolGroupStore(new Dictionary<string, BonusPoolGroup>(), new List<string>());
    }

    public static IBonusPoolGroupStore NewCachedBonusPoolGroupStore(MemoryBonusPoolGroupStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolGroupStore(inner, BonusPoolGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolGroup> FindBonusPoolGroups(IBonusPoolGroupStore store, BonusPoolGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
