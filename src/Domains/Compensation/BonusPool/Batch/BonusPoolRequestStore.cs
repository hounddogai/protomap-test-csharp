namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool requests by code.</summary>
public interface IBonusPoolRequestStore
{
    BonusPoolRequest? Find(string code);
    void Save(BonusPoolRequest item);
    bool Remove(string code);
    List<BonusPoolRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolRequestStore(Dictionary<string, BonusPoolRequest> items, List<string> order) : IBonusPoolRequestStore
{
    public Dictionary<string, BonusPoolRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolRequest item)
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

    public List<BonusPoolRequest> List()
    {
        var outValue = new List<BonusPoolRequest>();
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
public sealed class CachedBonusPoolRequestStore(MemoryBonusPoolRequestStore inner, BonusPoolRequestCache cache, int hits, int misses) : IBonusPoolRequestStore
{
    public MemoryBonusPoolRequestStore Inner { get; set; } = inner;
    public BonusPoolRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolRequest? Find(string code)
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

    public void Save(BonusPoolRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool requests. Empty fields match all.</summary>
public sealed record BonusPoolRequestQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<BonusPoolRequest> Apply(List<BonusPoolRequest> items)
    {
        var matched = Collections.Filter(items, (BonusPoolRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool request store.</summary>
public static class BonusPoolRequestStore
{
    public static IBonusPoolRequestStore NewMemoryBonusPoolRequestStore()
    {
        return new MemoryBonusPoolRequestStore(new Dictionary<string, BonusPoolRequest>(), new List<string>());
    }

    public static IBonusPoolRequestStore NewCachedBonusPoolRequestStore(MemoryBonusPoolRequestStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolRequestStore(inner, BonusPoolRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BonusPoolRequest> FindBonusPoolRequests(IBonusPoolRequestStore store, BonusPoolRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
