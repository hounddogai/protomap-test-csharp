namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool digests by code.</summary>
public interface IBonusPoolDigestStore
{
    BonusPoolDigest? Find(string code);
    void Save(BonusPoolDigest item);
    bool Remove(string code);
    List<BonusPoolDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolDigestStore(Dictionary<string, BonusPoolDigest> items, List<string> order) : IBonusPoolDigestStore
{
    public Dictionary<string, BonusPoolDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolDigest item)
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

    public List<BonusPoolDigest> List()
    {
        var outValue = new List<BonusPoolDigest>();
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
public sealed class CachedBonusPoolDigestStore(MemoryBonusPoolDigestStore inner, BonusPoolDigestCache cache, int hits, int misses) : IBonusPoolDigestStore
{
    public MemoryBonusPoolDigestStore Inner { get; set; } = inner;
    public BonusPoolDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolDigest? Find(string code)
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

    public void Save(BonusPoolDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool digests. Empty fields match all.</summary>
public sealed record BonusPoolDigestQuery(string Name, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolDigest> Apply(List<BonusPoolDigest> items)
    {
        var matched = Collections.Filter(items, (BonusPoolDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool digest store.</summary>
public static class BonusPoolDigestStore
{
    public static IBonusPoolDigestStore NewMemoryBonusPoolDigestStore()
    {
        return new MemoryBonusPoolDigestStore(new Dictionary<string, BonusPoolDigest>(), new List<string>());
    }

    public static IBonusPoolDigestStore NewCachedBonusPoolDigestStore(MemoryBonusPoolDigestStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolDigestStore(inner, BonusPoolDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BonusPoolDigest> FindBonusPoolDigests(IBonusPoolDigestStore store, BonusPoolDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
