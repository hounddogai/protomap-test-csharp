namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool agreements by code.</summary>
public interface IPoolAgreementStore
{
    PoolAgreement? Find(string code);
    void Save(PoolAgreement item);
    bool Remove(string code);
    List<PoolAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolAgreementStore(Dictionary<string, PoolAgreement> items, List<string> order) : IPoolAgreementStore
{
    public Dictionary<string, PoolAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolAgreement item)
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

    public List<PoolAgreement> List()
    {
        var outValue = new List<PoolAgreement>();
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
public sealed class CachedPoolAgreementStore(MemoryPoolAgreementStore inner, PoolAgreementCache cache, int hits, int misses) : IPoolAgreementStore
{
    public MemoryPoolAgreementStore Inner { get; set; } = inner;
    public PoolAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolAgreement? Find(string code)
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

    public void Save(PoolAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool agreements. Empty fields match all.</summary>
public sealed record PoolAgreementQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PoolAgreement item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolAgreement> Apply(List<PoolAgreement> items)
    {
        var matched = Collections.Filter(items, (PoolAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool agreement store.</summary>
public static class PoolAgreementStore
{
    public static IPoolAgreementStore NewMemoryPoolAgreementStore()
    {
        return new MemoryPoolAgreementStore(new Dictionary<string, PoolAgreement>(), new List<string>());
    }

    public static IPoolAgreementStore NewCachedPoolAgreementStore(MemoryPoolAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPoolAgreementStore(inner, PoolAgreementCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolAgreement> FindPoolAgreements(IPoolAgreementStore store, PoolAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
